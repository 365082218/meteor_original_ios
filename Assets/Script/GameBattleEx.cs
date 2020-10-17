using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using System;
using protocol;
using Idevgame.Meteor.AI;
using Excel2Json;
using UnityEngine.Networking;

public enum SceneEvent
{
    EventEnter = 200,//进入传送门
    EventExit = 201,//离开传送门
    EventDeath = 202,//死亡事件
}

public class ResultItem
{
    public int camp;
    public int deadCount;
    public int killCount;
    public int id;
}

//负责战斗中相机的位置指定之类，主角色目标组作为摄像机 视锥体范围，参考Tank教程里的简单相机控制
//负责战斗场景内位置间的寻路缓存
public partial class GameBattleEx : NetBehaviour {
    int time = 1000;//秒
    float timeClock = 0.0f;
    float ViewLimit = 40000;//90000(远程)-40000(近战)码，自动解除锁定.
    float Tick = 0;
    float ScriptDelay = 0.5f;//每0.5S执行一次寻找，关卡脚本
    public new void Awake()
    {
        base.Awake();
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
    }

    public bool BattleWin()
    {
        return Result == 1 || Result == 2;
    }

    public bool BattleLose()
    {
        return Result <= 0 && Result != -10;
    }

    public bool BattleTimeup()
    {
        return Result == 3;
    }

    public bool BattleFinished()
    {
        return Result != -10;
    }

    public void NetGameStart() {
        Resume();
        FrameReplay.Ins.Resume();
        if (CombatData.Ins.GScript != null) {
            CombatData.Ins.GScript.Scene_OnLoad();
            CombatData.Ins.GScript.Scene_OnInit();
        }

        for (int i = 0; i < MeteorManager.Ins.SceneItems.Count; i++) {
            MeteorManager.Ins.SceneItems[i].OnStart(CombatData.Ins.GScript);
        }
        SoundManager.Ins.MuteMusic(false);
        MeteorManager.Ins.UnitInfos.Add(Main.Ins.LocalPlayer);
        FrameSyncLocal.Ins.SyncNetPlayer();
    }

    //没有胜负，只有战绩，分混战/阵营2种
    public void NetGameOver() {
        Main.Ins.GameBattleEx.NetPause();
        FrameSyncServer.Ins.Pause();
        CombatData.Ins.GameFinished = true;
        PathHelper.Ins.StopCalc();
        CombatData.Ins.PauseAll = true;
        //停止音乐
        SoundManager.Ins.MuteMusic(true);
        //取得结算信息，显示结算页面.
        //打开结算面板
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.BattleResultDialogState);
        BattleResultDialogState.Instance.SetResult(3);
    }

    int Result = -10;
    bool showResult = false;
    float showResultTick = 0.0f;
    //显示失败，或者胜利界面 (2 >= x >= 1 = win)  (x <= 0 = lose) (x == 3) = none
    public void GameOver(int result)
    {
        if (Result != -10)
            return;
        CombatData.Ins.GameFinished = true;
        PathHelper.Ins.StopCalc();
        CombatData.Ins.PauseAll = true;
        U3D.OnPauseAI();
        ShowWayPoint(false);
        Main.Ins.LocalPlayer.meteorController.LockMove(true);
        Main.Ins.LocalPlayer.meteorController.LockInput(true);
        if (Main.Ins.playerListener != null) {
            GameObject.Destroy(Main.Ins.playerListener);
            Main.Ins.playerListener = null;
        }
        Main.Ins.listener.enabled = true;
        if (Main.Ins.CameraFree != null && Main.Ins.CameraFree.enabled)
        {
            EnableFreeCamera(false);
            EnableFollowCamera(true);
            Main.Ins.MainCamera = Main.Ins.CameraFollow.m_Camera;
        }
        SoundManager.Ins.StopAll();
        //关闭界面的血条缓动和动画
        //if (FightWnd.Exist)
        //    FightWnd.Instance.OnBattleEnd();
        //if (SfxWnd.Exist)
        //    SfxWnd.Instance.Close();
        //if (EscWnd.Exist)
        //    EscWnd.Instance.Close();
        
        Result = result;

        showResult = true;
        showResultTick = 0;

        if (NGUICameraJoystick.Ins)
            NGUICameraJoystick.Ins.Lock(true);
        if (UGUIJoystick.Ins)
            UGUIJoystick.Ins.Lock(true);
        //如果胜利，且不是最后一关，打开最新关标志.
        if (result == 1 || result == 2)
        {
            if (CombatData.Ins.Chapter == null)
            {
                if (CombatData.Ins.GLevelItem.Id == GameStateMgr.Ins.gameStatus.Level)
                    GameStateMgr.Ins.gameStatus.Level++;
                if (GameStateMgr.Ins.gameStatus.Level >= CombatData.LEVELMAX)
                    GameStateMgr.Ins.gameStatus.Level = CombatData.LEVELMAX;
                GameStateMgr.Ins.SaveState();
            }
            else
            {
                int nextLevel = 0;
                List<LevelData> all = CombatData.Ins.Chapter.LoadAll();
                for (int i = 0; i < all.Count; i++)
                {
                    if (CombatData.Ins.GLevelItem.Id == all[i].Id)
                    {
                        nextLevel = i + 1;
                        break;
                    }
                }

                if (nextLevel < all.Count && CombatData.Ins.Chapter.level < all[nextLevel].Id)
                    CombatData.Ins.Chapter.level = all[nextLevel].Id;

                GameStateMgr.Ins.SaveState();
            }
        }
    }

    public int GetMiscGameTime()
    {
        return Mathf.FloorToInt(1000 * timeClock);
    }

    public int GetGameTime()
    {
        return Mathf.FloorToInt(timeClock);
    }
    
    //多久触发一次.
    public void UpdateTime()
    {
        if (FightState.Exist())
            FightState.Instance.UpdateTime(GetTimeClock());
        if (ReplayState.Exist())
            ReplayState.Instance.UpdateTime(GetTimeClock());
    }

    GameObject SelectTarget;
    List<int> UnitActKeyDeleted = new List<int>();
    //战斗场景时，需要每一帧执行的.
    public override void NetUpdate()
    {
        if (showResult)
        {
            if (showResultTick >= 3.0f)
            {
                showResultTick = 0.0f;
                showResult = false;
                //打开结算面板
                Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.BattleResultDialogState);
                BattleResultDialogState.Instance.SetResult(this.Result);
            }
            showResultTick += FrameReplay.deltaTime;
            return;
        }

        if (CombatData.Ins.PauseAll || BattleFinished())
            return;

        if (CombatData.Ins.GLevelMode <= LevelMode.CreateWorld)
        {
            timeClock += FrameReplay.deltaTime;
            if (timeClock > time)//时间超过，平局
            {
                GameOver((int)GameResult.TimeOut);
                UpdateTime();
                return;
            }
        }

        //检查盟主模式下的死亡单位，令其复活
        if (CombatData.Ins.GGameMode == GameMode.MENGZHU || 
            U3D.IsMultiplyPlayer() && (CombatData.Ins.GGameMode == GameMode.ANSHA || CombatData.Ins.GGameMode == GameMode.SIDOU))
        {
            MeteorManager.Ins.DeadUnitsClone.Clear();
            for (int i = 0; i < MeteorManager.Ins.DeadUnits.Count; i++)
            {
                MeteorManager.Ins.DeadUnitsClone.Add(MeteorManager.Ins.DeadUnits[i]);
            }
            //循环里复活会导致列表变化导致问题
            for (int i = 0; i < MeteorManager.Ins.DeadUnitsClone.Count; i++)
            {
                MeteorManager.Ins.DeadUnitsClone[i].RebornUpdate();
            }
        }

        //更新BUFF
        foreach (var each in BuffMng.Ins.BufDict)
            each.Value.NetUpdate();

        //更新左侧角色对话文本
        for (int i = 0; i < UnitActKey.Count; i++)
        {
            if (UnitActionStack.ContainsKey(UnitActKey[i]))
            {
                //StackAction act = UnitActionStack[UnitActKey[i]].action[UnitActionStack[UnitActKey[i]].action.Count - 1].type;
                UnitActionStack[UnitActKey[i]].Update(FrameReplay.deltaTime);
                if (UnitActionStack[UnitActKey[i]].action.Count == 0)
                {
                    //MeteorUnit unit = U3D.GetUnit(UnitActKey[i]);
                    //Debug.Log(string.Format("{0} action call finished:{1}", unit.name, act));
                    UnitActKeyDeleted.Add(UnitActKey[i]);
                }
            }
        }

        if (UnitActKeyDeleted.Count != 0)
        {
            for (int i = 0; i < UnitActKeyDeleted.Count; i++)
            {
                UnitActKey.Remove(UnitActKeyDeleted[i]);
                UnitActionStack.Remove(UnitActKeyDeleted[i]);
            }
            UnitActKeyDeleted.Clear();
        }

        //AI目标选择
        Tick += FrameReplay.deltaTime;
        if (Tick >= ScriptDelay) {
            for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++) {
                MeteorUnit unit = MeteorManager.Ins.UnitInfos[i];
                if (unit.Dead || !unit.isActiveAndEnabled)
                    continue;
                if (unit.StateMachine != null) {
                    unit.StateMachine.SelectTarget();
                    unit.StateMachine.SelectItem();
                }
            }

            if (CombatData.Ins.GScript != null && CombatData.Ins.GLevelMode < LevelMode.CreateWorld) {
                CombatData.Ins.GScript.OnUpdate();//单机关卡才有剧情
            }
            Tick = 0;
        }

        if (CombatData.Ins.GScript != null)
            CombatData.Ins.GScript.Scene_OnIdle();
        RefreshAutoTarget();
        UpdateTime();
    }


    System.Reflection.MethodInfo Scene_OnCharacterEvent;
    System.Reflection.MethodInfo Scene_OnEvent;
    int EventDeath = 202;
    public void Init(LevelScriptBase script)
    {
        Scene_OnCharacterEvent = CombatData.Ins.GScriptType.GetMethod("Scene_OnCharacterEvent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (Scene_OnCharacterEvent == null)
        {
            System.Type typeParent = CombatData.Ins.GScriptType.BaseType;
            while (Scene_OnCharacterEvent == null && typeParent != null)
            {
                Scene_OnCharacterEvent = typeParent.GetMethod("Scene_OnCharacterEvent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                typeParent = typeParent.BaseType;
            }
        }
        Scene_OnEvent = CombatData.Ins.GScriptType.GetMethod("Scene_OnEvent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (Scene_OnEvent == null)
        {
            System.Type typeParent = CombatData.Ins.GScriptType.BaseType;
            while (Scene_OnEvent == null && typeParent != null)
            {
                Scene_OnEvent = typeParent.GetMethod("Scene_OnEvent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                typeParent = typeParent.BaseType;
            }
        }

        DisableCameraLock = !GameStateMgr.Ins.gameStatus.AutoLock;
        if (script != null)
        {
            if (CombatData.Ins.GLevelMode <= LevelMode.SinglePlayerTask)
                time = script.GetRoundTime() * 60;
            else if (CombatData.Ins.GLevelMode == LevelMode.CreateWorld)
                time = CombatData.Ins.RoundTime * 60;
            else
                time = 30 * 60;
        }
        timeClock = 0.0f;
        //上一局如果暂停了，新局开始都恢复
        Resume();

        if (CombatData.Ins.GScript != null)
        {
            CombatData.Ins.GScript.Scene_OnLoad();
            CombatData.Ins.GScript.Scene_OnInit();
        }

        for (int i = 0; i < MeteorManager.Ins.SceneItems.Count; i++)
        {
            MeteorManager.Ins.SceneItems[i].OnStart(CombatData.Ins.GScript);
        }

        if (CombatData.Ins.GLevelMode <= LevelMode.CreateWorld)
        {
            OnCreatePlayer();

            if (CombatData.Ins.GScript != null && CombatData.Ins.GLevelMode <= LevelMode.SinglePlayerTask)
                CombatData.Ins.GScript.OnStart();

            for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
            {
                if (MeteorManager.Ins.UnitInfos[i].Attr != null)
                    MeteorManager.Ins.UnitInfos[i].Attr.OnStart();//AI脚本必须在所有角色都加载完毕后再调用
            }

            ShowWayPoint(GameStateMgr.Ins.gameStatus.ShowWayPoint);
            CreateWayPointTrigger();

            //单机剧本时游戏开始前最后给脚本处理事件的机会,
            if (CombatData.Ins.GScript != null && CombatData.Ins.GLevelMode < LevelMode.CreateWorld) {
                CombatData.Ins.GScript.OnLateStart();
                CombatData.Ins.GScript.OnUpdate();
            }

            if (CombatData.Ins.GGameMode == GameMode.ANSHA)
            {
                MeteorUnit uEnemy = U3D.GetTeamLeader(EUnitCamp.EUC_ENEMY);
                SFXEffectPlay leaderEffect = SFXLoader.Ins.PlayEffect("vipblue.ef", Main.Ins.LocalPlayer.gameObject, false);
                Main.Ins.LocalPlayer.SetAsLeader(leaderEffect);
                if (uEnemy != null) {
                    leaderEffect = SFXLoader.Ins.PlayEffect("vipred.ef", uEnemy.gameObject, false);
                    uEnemy.SetAsLeader(leaderEffect);
                }
            }
        }

        if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer) {
            FrameSyncLocal.Ins.SyncMainPlayer();
            FrameSyncLocal.Ins.SyncNetPlayer();
        }

        CreateCamera();

        string bgm = CombatData.Ins.GLevelItem.BgmName;
        string soundFile = "";
        //资料片有自身的音乐
        if (CombatData.Ins.Chapter != null)
            soundFile = CombatData.Ins.Chapter.GetResPath(FileExt.MP3, bgm);
        //新剧本读取自己的音乐
        if (!string.IsNullOrEmpty(soundFile) && System.IO.File.Exists(soundFile)) {
            if (playBGM != null) {
                StopCoroutine(playBGM);
                playBGM = null;
            }
            playBGM = StartCoroutine(PlayBgm(soundFile));
        } else {
            if (!string.IsNullOrEmpty(bgm))
                SoundManager.Ins.PlayMusic(bgm);
        }
    }

    Coroutine playBGM;
    IEnumerator PlayBgm(string url) {
        using (WWW uwr = new WWW("file:///" + url)) {
            yield return uwr;
            if (!string.IsNullOrEmpty(uwr.error)) {
                Debug.LogError(uwr.error);
            } else {
                AudioClip clip = uwr.GetAudioClip(false);
                SoundManager.Ins.PlayMusic(clip);
            }
        }
    }

    public void CreateCamera()
    {
        //先创建一个相机
        GameObject camera = GameObject.Instantiate(Resources.Load("CameraEx")) as GameObject;
        camera.name = "CameraEx";

        //角色摄像机跟随者着角色.
        CameraFollow followCamera = GameObject.Find("CameraEx").GetComponent<CameraFollow>();
        followCamera.Init();
        followCamera.FollowTarget(Main.Ins.LocalPlayer.transform);
        Main.Ins.MainCamera = followCamera.GetComponent<Camera>();
        Main.Ins.CameraFollow = followCamera;

        //把音频侦听移到角色
        Main.Ins.listener.enabled = false;
        Main.Ins.playerListener = Main.Ins.MainCamera.gameObject.AddComponent<AudioListener>();
    }

    void SpawnAllRobot()
    {
        U3D.ClearNames();
        if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
        {
            for (int i = 1; i < CombatData.Ins.MaxPlayer; i++)
            {
                U3D.SpawnRobot(U3D.GetRandomUnitIdx(), EUnitCamp.EUC_KILLALL, GameStateMgr.Ins.gameStatus.Single.DisallowSpecialWeapon ? U3D.GetNormalWeaponType() : U3D.GetRandomWeaponType(), CombatData.Ins.PlayerLife);
            }
        }
        else if (CombatData.Ins.GGameMode == GameMode.ANSHA || CombatData.Ins.GGameMode == GameMode.SIDOU)
        {
            int FriendCount = CombatData.Ins.MaxPlayer / 2 - 1;
            for (int i = 0; i < FriendCount; i++)
            {
                U3D.SpawnRobot(U3D.GetRandomUnitIdx(), Main.Ins.LocalPlayer.Camp, GameStateMgr.Ins.gameStatus.Single.DisallowSpecialWeapon ? U3D.GetNormalWeaponType() : U3D.GetRandomWeaponType(), CombatData.Ins.PlayerLife);
            }

            for (int i = FriendCount + 1; i < CombatData.Ins.MaxPlayer; i++)
            {
                U3D.SpawnRobot(U3D.GetRandomUnitIdx(), U3D.GetAnotherCamp(Main.Ins.LocalPlayer.Camp), GameStateMgr.Ins.gameStatus.Single.DisallowSpecialWeapon ? U3D.GetNormalWeaponType() : U3D.GetRandomWeaponType(), CombatData.Ins.PlayerLife);
            }
        }
    }

    //单机角色创建
    public void OnCreatePlayer()
    {
        //设置主角属性
        U3D.InitPlayer(CombatData.Ins.GScript);
        if (CombatData.Ins.GLevelMode == LevelMode.CreateWorld)
            SpawnAllRobot();

        //除了主角的所有角色,开始输出,选择阵营, 进入战场
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i] == Main.Ins.LocalPlayer)
                continue;
            MeteorUnit unitLog = MeteorManager.Ins.UnitInfos[i];
            U3D.InsertSystemMsg(U3D.GetCampEnterLevelStr(unitLog.Camp, unitLog.name));
        }

        U3D.InsertSystemMsg("新回合开始计时");
    }

    RaycastHit [] SortRaycastHit(RaycastHit [] hit)
    {
        int index = 0;
        while (true)
        {
            //每次得到一个最小的插槽，与最前面一个调换位置.
            int slot = -1;
            float minDistance = float.MaxValue;
            for (int i = index; i < hit.Length; i++)
            {
                if (hit[i].distance < minDistance)
                {
                    minDistance = hit[i].distance;
                    slot = i;
                }
            }
            if (slot != -1 && slot != index)
            {
                RaycastHit ray = hit[index];
                hit[index] = hit[slot];
                hit[slot] = ray;
            }
            index++;
            if (index >= hit.Length)//火枪BUG修复
                break;
        }
        return hit;
    }

    List<MeteorUnit> hitTarget = new List<MeteorUnit>();
    List<SceneItemAgent> hitItem = new List<SceneItemAgent>();
    public void AddDamageCheck(MeteorUnit unit, AttackDes attackDef)
    {
        if (unit == null || unit.Dead)
            return;

        //联机远程武器
        //if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer) {
        //    if (unit.GetWeaponType() == (int)EquipWeaponType.Gun) {

        //    } else if (unit.GetWeaponType() == (int)EquipWeaponType.Dart) {

        //    } else if (unit.GetWeaponType() == (int)EquipWeaponType.Guillotines) {

        //    }
        //    return;
        //}

        //单机远程武器，主角和AI是不同的方式，如果用来做联机，会每个人的射击和
        hitTarget.Clear();
        hitItem.Clear();
        if (unit.GetWeaponType() == (int)EquipWeaponType.Gun)
        {
            //Debug.Log("fire");
            if (unit.Attr.IsPlayer)
            {
                Ray r = Main.Ins.CameraFollow.m_Camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, DartLoader.MaxDistance));
                RaycastHit[] allHit = Physics.RaycastAll(r, DartLoader.MaxDistance, LayerManager.ShootMask);
                RaycastHit[] allHitSort = SortRaycastHit(allHit);
                bool showEffect = false;
                //先排个序，从近到远
                for (int i = 0; i < allHitSort.Length; i++)
                {
                    if (allHitSort[i].transform.root.gameObject.layer == LayerManager.Scene)
                    {
                        //主角可以击中
                        GameObject other = allHitSort[i].transform.gameObject;
                        SceneItemAgent it = other.GetComponentInParent<SceneItemAgent>();
                        if (it != null)
                        {
                            //Debug.Log("dart attack sceneitemagent");
                            if (!hitItem.Contains(it))
                            {
                                it.OnDamage(unit, attackDef);
                                hitItem.Add(it);
                            }
                        }
                        if (!showEffect)
                        {
                            SFXLoader.Ins.PlayEffect("GunHit.ef", allHitSort[i].point, true, false);
                            //Debug.Log("play effect gunhit");
                            showEffect = true;
                        }
                        break;
                    }

                    MeteorUnit unitAttacked = allHitSort[i].transform.gameObject.GetComponentInParent<MeteorUnit>();
                    if (unitAttacked != null)
                    {
                        if (unit == unitAttacked)
                            continue;
                        if (unit.SameCamp(unitAttacked))
                            continue;
                        if (unitAttacked.Dead)
                            continue;
                        if (!hitTarget.Contains(unitAttacked))
                        {
                            //在指定位置播放一个特效.
                            if (!showEffect)
                            {
                                SFXLoader.Ins.PlayEffect("GunHit.ef", allHitSort[i].point, true, false);
                                //Debug.Log("play effect gunhit");
                                showEffect = true;
                            }
                            unitAttacked.OnAttack(unit, attackDef);
                            hitTarget.Add(unitAttacked);
                        }
                    }
                }
            }
            else
            {
                //火枪射线应该由，持枪点朝目标处，直接用概率算。
                GameObject gun = unit.weaponLoader.GetGunTrans();
                if (gun != null)
                {
                    Vector3 vec = Vector3.zero;
                    if (unit.LockTarget != null)
                    {
                        Vector3 vecDir = (unit.LockTarget.mSkeletonPivot - gun.transform.position);
                        vec = vecDir;
                        vec.y = 0;
                        Vector3 vecShootEndPoint = unit.LockTarget.mSkeletonPivot + Quaternion.AngleAxis(90, Vector3.up) * vec.normalized * unit.charController.radius;//角色某侧顶端
                        float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot((vecShootEndPoint - gun.transform.position).normalized, vecDir.normalized), -1.0f, 1.0f)) * Mathf.Rad2Deg;
                        float ratio = ((100.0f - unit.Attr.Aim) / 100.0f);//miss概率                                            //正负1倍角度一定可以射击命中.不在范围内一定不会命中.
                        float angleRatio = Utility.Range(-(angle * (1 + ratio)), angle * (1 + ratio));
                        vec = Quaternion.AngleAxis(angleRatio, Vector3.up) * (unit.LockTarget.mSkeletonPivot - gun.transform.position).normalized;
                    }
                    else
                        vec = -1 * gun.transform.forward;
                    Ray r = new Ray(gun.transform.position, vec);
                    RaycastHit[] allHit = Physics.RaycastAll(r, DartLoader.MaxDistance, LayerManager.ShootMask);
                    RaycastHit[] allHitSort = SortRaycastHit(allHit);
                    bool showEffect = false;
                    //先排个序，从近到远
                    for (int i = 0; i < allHitSort.Length; i++)
                    {
                        if (allHitSort[i].transform.root.gameObject.layer == LayerManager.Scene)
                        {
                            //在指定位置播放一个特效.
                            if (!showEffect)
                            {
                                SFXLoader.Ins.PlayEffect("GunHit.ef", allHitSort[i].point, true, false);
                                showEffect = true;
                            }
                            break;
                        }

                        MeteorUnit unitAttacked = allHitSort[i].transform.gameObject.GetComponentInParent<MeteorUnit>();
                        if (unitAttacked != null)
                        {
                            if (unit == unitAttacked)
                                continue;
                            if (unit.SameCamp(unitAttacked))
                                continue;
                            if (unitAttacked.Dead)
                                continue;
                            if (!hitTarget.Contains(unitAttacked))
                            {
                                //在指定位置播放一个特效.
                                if (!showEffect)
                                {
                                    SFXLoader.Ins.PlayEffect("GunHit.ef", allHitSort[i].point, true, false);
                                    showEffect = true;
                                }
                                unitAttacked.OnAttack(unit, attackDef);
                                hitTarget.Add(unitAttacked);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            //飞镖，那么从绑点生成一个物件，让他朝
            //"Sphere3"是挂点
            //即刻生成一个物件飞出去。
            if (unit.characterLoader.sfxEffect != null)
            {
                if (unit.GetWeaponType() == (int)EquipWeaponType.Dart)
                {
                    //飞镖.
                    Transform bulletBone = unit.characterLoader.sfxEffect.FindEffectByName("Sphere_3");//出生点，
                    Vector3 vecSpawn = bulletBone.position;
                    Vector3 forw = Vector3.zero;
                    if (unit.Attr.IsPlayer)
                    {
                        //远程锁定目标为空时
                        if (lockedTarget2 == null)
                        {
                            Vector3 shootPivot = new Vector3(Screen.width / 2, (Screen.height) * 0.65f, DartLoader.MaxDistance);
                            Vector3 vec = Main.Ins.CameraFollow.m_Camera.ScreenToWorldPoint(shootPivot);
                            forw = (vec - vecSpawn).normalized;
                        }
                        else
                        {
                            Vector3 vec = lockedTarget2.mShootPivot;
                            forw = (vec - vecSpawn).normalized;
                        }
                    }
                    else
                    {
                        //AI在未发现敌人时随便发飞镖?
                        if (unit.LockTarget == null)
                        {
                            //角色的面向 + 一定随机
                            forw = (Quaternion.AngleAxis(Utility.Range(-35, 35), Vector3.up)  * Quaternion.AngleAxis(Utility.Range(-5, 5), Vector3.right)  * (-1 * unit.transform.forward)).normalized;
                        }
                        else
                        {
                            //判断角色是否面向目标，不面向，则朝身前发射飞镖
                            MeteorUnit u = unit.LockTarget;
                            if (u != null)
                            {
                                if (!unit.IsFacetoTarget(u))
                                    u = null;
                            }

                            if (u == null)
                                forw = (Quaternion.AngleAxis(Utility.Range(-35, 35), Vector3.up) * Quaternion.AngleAxis(Utility.Range(-5, 5), Vector3.right) * (-1 * unit.transform.forward)).normalized;//角色的面向
                            else
                            {
                                float k = Utility.Range(Mathf.FloorToInt(-(100 - unit.Attr.Aim) / 3.0f), Mathf.FloorToInt((100 - unit.Attr.Aim) / 3.0f));
                                Vector3 vec = unit.LockTarget.transform.position + new Vector3(k, Utility.Range(10, 38), k);
                                forw = (vec - vecSpawn).normalized;
                            }
                            //要加一点随机，否则每次都打一个位置不正常
                        }
                    }
                    //主角则方向朝着摄像机正前方
                    //不是主角没有摄像机，那么就看有没有目标，有则随机一个方向，根据与目标的包围盒的各个顶点的，判定这个方向
                    //得到武器模型，在指定位置出生.
                    InventoryItem weapon = unit.weaponLoader.GetCurrentWeapon();
                    DartLoader.Init(vecSpawn, forw, weapon, attackDef, unit);
                }
                else if (unit.GetWeaponType() == (int)EquipWeaponType.Guillotines)
                {
                    //隐藏右手的飞轮
                    unit.weaponLoader.HideWeapon();
                    //飞轮
                    Vector3 vecSpawn = unit.WeaponR.transform.position;
                    InventoryItem weapon = unit.weaponLoader.GetCurrentWeapon();
                    if (unit.Attr.IsPlayer)
                        FlyWheel.Init(vecSpawn, autoTarget, weapon, attackDef, unit);
                    else
                    {
                        //判断角色是否面向目标，不面向，则朝身前发射飞轮
                        MeteorUnit u = unit.LockTarget;
                        if (u != null)
                        {
                            if (!unit.IsFacetoTarget(u))
                                u = null;
                        }
                        
                        FlyWheel.Init(vecSpawn, u, weapon, attackDef, unit);
                    }
                }
            }
        }
    }

    //所有角色的攻击盒.特效，武器
    //Dictionary<MeteorUnit, List<Collider>> DamageList = new Dictionary<MeteorUnit, List<Collider>>();
    //缓存角色的攻击盒.
    //public void AddDamageCollision(MeteorUnit unit, Collider co)
    //{
    //    if (unit == null || co == null)
    //        return;
    //    if (DamageList.ContainsKey(unit))
    //    {
    //        if (DamageList[unit].Contains(co))
    //            return;
    //        DamageList[unit].Add(co);
    //    }
    //    else
    //        DamageList.Add(unit, new List<Collider>() { co });
    //}

    //清理角色的攻击盒.
    //public void ClearDamageCollision(MeteorUnit unit)
    //{
    //    if (DamageList.ContainsKey(unit))
    //        DamageList.Remove(unit);
    //}

    public string GetTimeClock()
    {
        //600 = 10:00
        if (CombatData.Ins.GLevelMode <= LevelMode.CreateWorld)
        {
            int left = time - (int)timeClock;
            if (left < 0)
                left = 0;
            int minute = left / 60;
            int seconds = left % 60;
            string t = "";
            t = string.Format("{0:D2}:{1:D2}", minute, seconds);
            return t;
        }
        else if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer)
        {
            int left = Mathf.FloorToInt(NetWorkBattle.Ins.GameTime / 1000);
            if (left < 0)
                left = 0;
            int minute = left / 60;
            int seconds = left % 60;
            string t = "";
            t = string.Format("{0:D2}:{1:D2}", minute, seconds);
            return t;
        }
        return "--:--";
    }

    public bool IsTimeup()
    {
        int left = time - (int)timeClock;
        if (left < 0)
            return true;
        return false;
    }
    
    public void NetPause()
    {
        if (Main.Ins.LocalPlayer != null)
            Main.Ins.LocalPlayer.meteorController.LockInput(true);
        if (UGUIJoystick.Ins != null)
            UGUIJoystick.Ins.Lock(true);
        if (NGUICameraJoystick.Ins != null)
            NGUICameraJoystick.Ins.Lock(true);
        U3D.OnPauseAI();
        U3D.EnableUpdate(CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer);
        CombatData.Ins.PauseAll = true;
    }

    //全部AI暂停，游戏时间停止，任何依据时间做动画的物件，全部要停止.
    public void Pause()
    {
        NetPause();
    }

    public void Resume()
    {
        if (Main.Ins.LocalPlayer != null){
            Main.Ins.LocalPlayer.meteorController.LockInput(false);
        }
        if (UGUIJoystick.Ins != null)
            UGUIJoystick.Ins.Lock(false);
        if (NGUICameraJoystick.Ins != null)
            NGUICameraJoystick.Ins.Lock(false);
        U3D.OnResumeAI();
        U3D.EnableUpdate(true);
        CombatData.Ins.PauseAll = false;
    }

    public MeteorUnit lockedTarget;//近战武器-自动锁定目标
    public MeteorUnit lockedTarget2;//远程武器-自动锁定目标
    public MeteorUnit autoTarget;
    public SFXEffectPlay lockedEffect;
    public SFXEffectPlay autoEffect;
    //主角是否能锁定目标
    public bool CanLockTarget(MeteorUnit unit)
    {
        if (unit == Main.Ins.LocalPlayer)
            return false;
        //使用枪械/远程武器时
        int weapon = Main.Ins.LocalPlayer.GetWeaponType();
        if (weapon == (int) EquipWeaponType.Dart || 
            weapon == (int)EquipWeaponType.Gun || 
            weapon == (int)EquipWeaponType.Guillotines)
        {
            return false;
        }
        //只判断距离，因为匕首背刺不一定面对角色，但是在未有锁定目标时近距离打到敌人，则把敌人设置为目标.
        if (lockedTarget == null)
        {
            if (unit.Dead)
                return false;
            Vector3 vec = unit.transform.position - Main.Ins.LocalPlayer.transform.position;
            float v = vec.sqrMagnitude;
            if (v > ViewLimit)
                return false;
            return true;
        }
        return false;
    }

    //如果是空，标志着，锁定目标已经被消灭
    public void ChangeLockedTarget(MeteorUnit unit)
    {
        if (unit == null)
        {
            if (lockedEffect != null)
            {
                lockedEffect.OnPlayAbort();
                lockedEffect = null;
            }
            lockedTarget = null;
            bLocked = false;
            Main.Ins.CameraFollow.OnChangeLockTarget(null);
        }
        else
        {
            if (autoEffect != null)
            {
                autoEffect.OnPlayAbort();
                autoEffect = null;
            }
            autoTarget = null;
            lockedTarget = unit;
            lockedEffect = SFXLoader.Ins.PlayEffect("lock.ef", lockedTarget.gameObject);
            bLocked = true;
            Main.Ins.CameraFollow.OnChangeLockTarget(lockedTarget.transform);
        }
        if (FightState.Exist())
            FightState.Instance.OnChangeLock(bLocked);
    }
    //存在自动目标时，把自动目标删除，然后设置其为锁定目标.
    public bool bLocked = false;
    public void Lock()
    {
        if (autoTarget != null)
        {
            MeteorUnit lockTarget = autoTarget;
            Main.Ins.LocalPlayer.SetLockedTarget(lockTarget);
            ChangeLockedTarget(lockTarget);
            Main.Ins.CameraFollow.OnLockTarget();
        }
    }

    bool DisableCameraLock;
    public void DisableLock()
    {
        DisableCameraLock = true;
    }

    public void EnableLock()
    {
        DisableCameraLock = false;
    }

    //远程目标的解除锁定
    public void Unlock2()
    {
        if (lockedEffect != null)
        {
            lockedEffect.OnPlayAbort();
            lockedEffect = null;
        }
        lockedTarget2 = null;

        if (autoEffect != null)
        {
            autoEffect.OnPlayAbort();
            autoEffect = null;
        }
        autoTarget = null;
        //远程武器与摄像机锁定系统无关
    }

    public void Unlock()
    {
        if (lockedEffect != null)
        {
            lockedEffect.OnPlayAbort();
            lockedEffect = null;
        }
        lockedTarget = null;

        if (autoEffect != null)
        {
            autoEffect.OnPlayAbort();
            autoEffect = null;
        }
        autoTarget = null;
        Main.Ins.LocalPlayer.SetLockedTarget(null);
        Main.Ins.CameraFollow.OnUnlockTarget();
        bLocked = false;
        if (FightState.Exist())
            FightState.Instance.OnChangeLock(bLocked);
    }

    //远程武器自动锁定系统.血滴子算特殊武器，
    public void RefreshAutoTarget2()
    {
        ViewLimit = 1000000;
        float radius = 1000;
        //夹角超过限制
        MeteorUnit player = Main.Ins.LocalPlayer;
        float angleMax = 30;//cos值越大，角度越小
        float autoDis = ViewLimit;//自动目标与主角的距离，距离近，优先
        MeteorUnit wantRotation = null;//夹角最小的
        MeteorUnit wantDis = null;//距离最近的
        Vector3 vecPlayer = -1 * player.transform.forward;
        vecPlayer.y = 0;
        Collider[] other = Physics.OverlapSphere(player.transform.position, radius, LayerManager.PlayerMask);
        for (int i = 0; i < other.Length; i++) {
            MeteorUnit target = other[i].gameObject.GetComponent<MeteorUnit>();
            if (target == null || target == player)
                continue;
            if (player.SameCamp(target))
                continue;
            if (target.Dead)
                continue;
            Vector3 vec = target.transform.position - player.transform.position;
            float v = vec.sqrMagnitude;
            target.distance = v;
            vec.y = 0;
            //先判断夹角是否在限制范围内.
            vec = Vector3.Normalize(vec);
            float angle = Mathf.Acos(Vector3.Dot(vecPlayer.normalized, vec)) * Mathf.Rad2Deg;
            target.angle = angle;
            //角度小于75则可以成为自动对象.
            if (angle < angleMax) {
                angleMax = angle;
                wantRotation = target;

            }
            if (v < autoDis) {
                autoDis = v;
                wantDis = target;
            }
        }

        MeteorUnit finallyTarget = wantRotation;
        //如果一个角色较近，但是角度差更大，一个距离较远但是角度差较小，判定2者该挑选谁作为最终目标
        if (wantDis != null && wantDis != wantRotation && finallyTarget != null)
        {
            finallyTarget = wantRotation.TargetWeight() < wantDis.TargetWeight() ? wantRotation : wantDis;
            wantRotation = wantDis;
        }
        if (finallyTarget != null) {
            Vector3 vecDir = new Vector3();
            vecDir = finallyTarget.mSkeletonPivot - Main.Ins.LocalPlayer.mSkeletonPivot;
            //期望目标与主角之间有墙壁阻隔
            if (Physics.Raycast(Main.Ins.LocalPlayer.mSkeletonPivot, vecDir.normalized, vecDir.magnitude, LayerManager.AllSceneMask)) {
                Unlock();
                return;
            }
        }
        //与角色的夹角不能大于75度,否则主角脑袋骨骼可能注视不到自动目标.
        if (finallyTarget != autoTarget)
        {
            if (autoEffect != null)
            {
                autoEffect.OnPlayAbort();
                autoEffect = null;
            }
            autoTarget = finallyTarget;
            //Debug.Log("切换目标");
            if (autoTarget != null)
                autoEffect = SFXLoader.Ins.PlayEffect("Track.ef", autoTarget.gameObject);
            return;
        }

        //如果当前的自动目标存在，且夹角超过75度，即在主角背后，那么自动目标清空
        if (autoTarget != null && autoTarget.angle > 45)
        {
            autoEffect.OnPlayAbort();
            autoTarget = null;
            //Debug.LogError("夹角太大，自动目标重置");
        }
    }

    public void RefreshAutoTarget()
    {
        if (Main.Ins.LocalPlayer == null)
            return;
        MeteorUnit player = Main.Ins.LocalPlayer;
        //等待主角稳定住方向后再来计算
        if (player.ActionMgr.Rotateing)
            return;

        if (player.GetWeaponType() == (int)EquipWeaponType.Dart || player.GetWeaponType() == (int)EquipWeaponType.Gun)
        {
            //远程武器，自动锁定系统.
            RefreshAutoTarget2();
            lockedTarget2 = autoTarget;
            return;
        }

        float radius = 200;
        if (player.GetWeaponType() == (int)EquipWeaponType.Guillotines)
        {
            ViewLimit = 4000000;
            radius = 2000;
        }
        else
        {
            ViewLimit = 40000;
            radius = 200;
        }
        //超出距离，并没有考虑角色背后-方向导致的解除锁定
        if (lockedTarget != null)
        {
            Vector3 vec = lockedTarget.transform.position - player.transform.position;
            float v = vec.sqrMagnitude;
            if (v > ViewLimit)
                Unlock();
            return;
        }
        
        float angleMax = 75;//cos值越大，角度越小
        float autoDis = ViewLimit;//自动目标与主角的距离，距离近，优先
        MeteorUnit wantRotation = null;//夹角最小的
        MeteorUnit wantDis = null;//距离最近的
        Vector3 vecPlayer = -player.transform.forward;
        vecPlayer.y = 0;
        Collider[] other = Physics.OverlapSphere(player.transform.position, radius, LayerManager.PlayerMask);
        for (int i = 0; i < other.Length; i++)
        {
            MeteorUnit target = other[i].gameObject.GetComponent<MeteorUnit>();
            if (target == null) {
                Debug.LogError("???:" + other[i].name);
            }
            if (other[i].gameObject == player.gameObject)
                continue;
            if (player.SameCamp(target))
                continue;
            if (target.Dead)
                continue;
            Vector3 vec = target.transform.position - player.transform.position;
            float v = vec.sqrMagnitude;
            target.distance = v;
            //飞轮时，无限角度距离
            if (v > ViewLimit && player.GetWeaponType() != (int)EquipWeaponType.Guillotines)
                continue;
            //高度差2个角色，不要成为自动对象，否则摄像机位置有问题
            if (Mathf.Abs(vec.y) >= 75 && player.GetWeaponType() != (int)EquipWeaponType.Guillotines)
                continue;
            vec.y = 0;
            //先判断夹角是否在限制范围内.
            vec = Vector3.Normalize(vec);
            float angle = Mathf.Acos(Vector3.Dot(vecPlayer.normalized, vec)) * Mathf.Rad2Deg;
            target.angle = angle;
            //角度小于75则可以成为自动对象.
            if (angle < angleMax)
            {
                angleMax = angle;
                wantRotation = target;
            }
            if (v < autoDis)
            {
                autoDis = v;
                wantDis = target;
            }
        }

        MeteorUnit finallyTarget = wantRotation;
        //如果一个角色较近，但是角度差更大，一个距离较远但是角度差较小，判定2者该挑选谁作为最终目标
        if (wantDis != null && wantDis != wantRotation && finallyTarget != null)
        {
            finallyTarget = wantRotation.TargetWeight() < wantDis.TargetWeight() ? wantRotation : wantDis;
            //wantRotation = wantDis;
        }

        if (finallyTarget != null) {
            Vector3 vecDir = new Vector3();
            vecDir = finallyTarget.mSkeletonPivot - player.mSkeletonPivot;
            //期望目标与主角之间有墙壁阻隔
            if (Physics.Raycast(player.mSkeletonPivot, vecDir.normalized, vecDir.magnitude, LayerManager.AllSceneMask)) {
                Unlock();
                return;
            }
        }

        //与角色的夹角不能大于75度,否则主角脑袋骨骼可能注视不到自动目标.
        if (finallyTarget != autoTarget)
        {
            if (autoEffect != null)
            {
                autoEffect.OnPlayAbort();
                autoEffect = null;
            }
            autoTarget = finallyTarget;
            if (autoTarget != null)
                autoEffect = SFXLoader.Ins.PlayEffect("Track.ef", autoTarget.gameObject);
            return;
        }

        //如果当前的自动目标存在，且夹角超过75度，即在主角背后，那么自动目标清空
        if (autoTarget != null && autoTarget.angle > 90 && player.GetWeaponType() != (int)EquipWeaponType.Guillotines)
        {
            autoEffect.OnPlayAbort();
            autoTarget = null;
        }

        if (autoTarget != null && player.GetWeaponType() != (int)EquipWeaponType.Guillotines)
        {
            if (autoTarget.distance > ViewLimit)
                Unlock();
        }
    }


    SortedDictionary<int, ResultItem> battleResult = new SortedDictionary<int, ResultItem>();
    public SortedDictionary<int, ResultItem> BattleResult { get { return battleResult; } }
    public void OnUnitDead(MeteorUnit unit, MeteorUnit killer = null)
    {
        if (Scene_OnCharacterEvent != null)
            Scene_OnCharacterEvent.Invoke(CombatData.Ins.GScript, new object[] { unit.InstanceId, EventDeath });
        //无阵营的角色,杀死人，不统计信息
        if (killer != null)
        {
            //统计杀人计数
            if (!battleResult.ContainsKey(killer.InstanceId))
            {
                ResultItem it = new ResultItem();
                it.camp = (int)killer.Camp;
                it.id = killer.InstanceId;
                it.deadCount = 0;
                it.killCount = 1;
                battleResult.Add(killer.InstanceId, it);
            }
            else
                battleResult[killer.InstanceId].killCount += 1;
        }

        if (unit != null)
        {
            //统计被杀次数
            if (!battleResult.ContainsKey(unit.InstanceId))
            {
                ResultItem it = new ResultItem();
                it.camp = (int)unit.Camp;
                it.id = unit.InstanceId;
                it.deadCount = 1;
                it.killCount = 0;
                battleResult.Add(unit.InstanceId, it);
            }
            else
                battleResult[unit.InstanceId].deadCount += 1;
        }

        if (unit == Main.Ins.LocalPlayer)
        {
            //如果是
            if (CombatData.Ins.GLevelMode == LevelMode.CreateWorld)
            {
                if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
                {
                    //等一段时间后复活.
                }
                else if (CombatData.Ins.GGameMode == GameMode.ANSHA)
                {
                    //检查是否是队长
                    if (unit.IsLeader)
                        GameOver(0);
                }
                else if (CombatData.Ins.GGameMode == GameMode.SIDOU)
                {
                    //检查双方是否有一边全部战败
                    if (U3D.AllEnemyDead())
                        GameOver(1);
                    else if (U3D.AllFriendDead())
                        GameOver(0);
                    else
                    {
                        //用一个自由相机，拍摄场上PK的几个人
                        InitFreeCamera();
                        EnableFollowCamera(false);
                    }
                }
            }
            else if (CombatData.Ins.GLevelMode == LevelMode.SinglePlayerTask)
                GameOver(0);
            else if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer) {

            }
            DropMng.Ins.Drop(unit);
            Unlock();
        }
        else
        {
            //无论谁杀死，都要爆东西
            DropMng.Ins.Drop(unit);
            //如果是被任意流星角色的伤害，特效，技能，等那么主角得到经验，如果是被场景环境杀死，
            //爆钱和东西
            //检查剧情
            //检查任务
            //检查过场对白，是否包含角色对话
            if (unit == autoTarget)
            {
                if (autoEffect != null)
                {
                    autoEffect.OnPlayAbort();
                    autoEffect = null;
                }
                autoTarget = null;
            }
            if (unit == lockedTarget)
                Unlock();
            if (CombatData.Ins.GLevelMode == LevelMode.SinglePlayerTask)
            {
                GameResult result = CombatData.Ins.GScript.OnUnitDead(unit);
                if (result != GameResult.None)
                    GameOver((int)result);
            }
            //如果是
            else if (CombatData.Ins.GLevelMode == LevelMode.CreateWorld)
            {
                if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
                {
                    //等一段时间后复活.
                }
                else if (CombatData.Ins.GGameMode == GameMode.ANSHA)
                {
                    //检查是否是队长
                    if (unit.IsLeader)
                        GameOver(1);
                }
                else if (CombatData.Ins.GGameMode == GameMode.SIDOU)
                {
                    //检查双方是否有一边全部战败
                    if (U3D.AllEnemyDead())
                        GameOver(1);
                    else if (U3D.AllFriendDead())
                        GameOver(0);
                }
            }
        }
    }

    public void EnableFollowCamera(bool enable)
    {
        if (Main.Ins.CameraFollow != null)
        {
            Main.Ins.CameraFollow.m_Camera.enabled = enable;
            Main.Ins.CameraFollow.enabled = enable;
            AudioListener listener = Main.Ins.CameraFree.GetComponent<AudioListener>();
            if (listener != null) {
                listener.enabled = enable;
                if (enable) {
                    Main.Ins.playerListener = listener;
                }
            }
        }
    }

    public void EnableFreeCamera(bool enable)
    {
        if (Main.Ins.CameraFree != null)
        {
            Main.Ins.CameraFree.m_Camera.enabled = enable;
            Main.Ins.CameraFree.enabled = enable;
            AudioListener listener = Main.Ins.CameraFree.GetComponent<AudioListener>();
            if (listener != null) {
                listener.enabled = enable;
                if (enable) {
                    Main.Ins.playerListener = listener;
                }
            }
        }
    }

    public void InitFreeCamera(MeteorUnit target = null)
    {
        if (Main.Ins.CameraFree == null)
        {
            GameObject FreeCamera = GameObject.Instantiate(Resources.Load("CameraFreeEx")) as GameObject;
            FreeCamera.name = "FreeCamera";
            Main.Ins.CameraFree = FreeCamera.GetComponent<CameraFree>();
            FreeCamera.AddComponent<AudioListener>();
        }
        Main.Ins.CameraFree.GetComponent<Camera>().enabled = true;
        Main.Ins.CameraFree.enabled = true;
        Main.Ins.MainCamera = Main.Ins.CameraFree.m_Camera;
        //找一个正在打斗的目标，随便谁都行
        MeteorUnit watchTarget = target;
        if (watchTarget == null)
        {
            for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
            {
                if (MeteorManager.Ins.UnitInfos[i].Dead)
                    continue;
                watchTarget = MeteorManager.Ins.UnitInfos[i];
                break;
            }
        }
        Main.Ins.CameraFree.Init(watchTarget);
    }

    public void ChangeLockStatus()
    {
        if (lockedTarget != null)
            Unlock();
        //else if (autoTarget != null)
        //    Lock();
    }
    //每个角色拥有的动作堆栈
    SortedDictionary<int, ActionConfig> UnitActionStack = new SortedDictionary<int, ActionConfig>();
    List<int> UnitActKey = new List<int>();

    public bool IsPerforming(int unit)
    {
        return UnitActionStack.ContainsKey(unit);
    }

    public void PushActionSay(int id, string text)
    {
        PushAction(id, StackAction.SAY, 0, text);
    }

    public void PushActionPatrol(int id, List<int> path)
    {
        if (!UnitActKey.Contains(id))
            UnitActKey.Add(id);
        if (!UnitActionStack.ContainsKey(id))
        {
            UnitActionStack.Add(id, new ActionConfig());
            UnitActionStack[id].id = id;
        }
        ActionItem it = new ActionItem();
        it.pause_time = 0;
        it.type = StackAction.Patrol;
        it.Path = path;
        UnitActionStack[id].action.Add(it);
    }

    void PushActionUse(int id, StackAction type, int idx)
    {
        if (!UnitActKey.Contains(id))
            UnitActKey.Add(id);
        if (!UnitActionStack.ContainsKey(id))
        {
            UnitActionStack.Add(id, new ActionConfig());
            UnitActionStack[id].id = id;
        }
        ActionItem it = new ActionItem();
        it.text = "";
        it.pause_time = 0;
        it.type = type;//say = 1 pause = 2 skill = 3;crouch = 4, block=5
        it.param = idx;
        it.target = Vector3.zero;
        UnitActionStack[id].action.Add(it);
    }

    //为了增加，朝指定位置攻击功能加的
    void PushAction(int id, StackAction type, Vector3 position, int count)
    {
        if (!UnitActKey.Contains(id))
            UnitActKey.Add(id);
        if (!UnitActionStack.ContainsKey(id))
        {
            UnitActionStack.Add(id, new ActionConfig());
            UnitActionStack[id].id = id;
        }
        ActionItem it = new ActionItem();
        it.text = "";
        it.pause_time = 0;
        it.type = type;//say = 1 pause = 2 skill = 3;crouch = 4, block=5
        it.param = count;
        it.target = position;
        UnitActionStack[id].action.Add(it);
    }

    void PushAction(int id, StackAction type, float t = 0.0f, string text = "", int param = 0)
    {
        if (!UnitActKey.Contains(id))
            UnitActKey.Add(id);
        if (!UnitActionStack.ContainsKey(id))
        {
            UnitActionStack.Add(id, new ActionConfig());
            UnitActionStack[id].id = id;
        }
        ActionItem it = new ActionItem();
        it.text = text;
        it.pause_time = t;
        it.type = type;//say = 1 pause = 2 skill = 3;crouch = 4, block=5
        it.param = param;
        UnitActionStack[id].action.Add(it);
    }

    public void PushActionGuard(int id, int time)
    {
        PushAction(id, StackAction.GUARD, time, "", 0);
    }

    public void PushActionAggress(int id)
    {
        PushAction(id, StackAction.Aggress);
    }

    public void PushActionSkill(int id)
    {
        PushAction(id, StackAction.SKILL);
    }

    public void PushActionHelp(int id, int target) {
        PushAction(id, StackAction.Help, 0, "", target);
    }

    public void PushActionBlock(int id, int status)
    {
        PushAction(id, StackAction.BLOCK, 0, "", status);
    }

    public void PushActionUse(int id, int idx)
    {
        PushActionUse(id, StackAction.Use, idx);
    }
    public void PushActionCrouch(int id, int status)
    {
        PushAction(id, StackAction.CROUCH, 0, "", status);
    }

    public void PushActionPause(int id, float pause)
    {
        PushAction(id, StackAction.PAUSE, pause, "");
    }

    public void PushActionFollow(int id, int target)
    {
        PushAction(id, StackAction.Follow, 0, "", target);
    }

    public void PushActionIdle(int id) {
        MeteorUnit unit = U3D.GetUnit(id);
        if (unit != null && unit.StateMachine != null)
            unit.StateMachine.ChangeState(unit.StateMachine.IdleState);
    }

    public void PushActionWait(int id)
    {
        MeteorUnit unit = U3D.GetUnit(id);
        if (unit != null && unit.StateMachine != null)
            unit.StateMachine.ChangeState(unit.StateMachine.WaitState);
    }

    public void PushActionFaceTo(int id, int target)
    {
        PushAction(id, StackAction.FaceTo, 0, "", target);
    }

    public void PushActionKill(int id, int target)
    {
        PushAction(id, StackAction.Kill, 0, "", target);
    }

    public void PushActionDodge(int id, int target) {
        PushAction(id, StackAction.Dodge, 0, "", target);
    }

    public void PushActionAttackTarget(int id, int targetIndex, int count = 0)
    {
        PushAction(id, StackAction.AttackTarget, LevelScriptBase.GetTarget(targetIndex), count);
    }

    public void StopAction(int id)
    {
        //Debug.LogError("stop action");
        if (UnitActKey.Contains(id))
            UnitActKey.Remove(id);
        if (UnitActionStack.ContainsKey(id))
            UnitActionStack.Remove(id);
    }

    //创建路点触发器.寻路时使用
    void CreateWayPointTrigger() {
        GameObject wayPointRoot = new GameObject("WayPoints");
        wayPointRoot.transform.position = Vector3.zero;
        wayPointRoot.transform.rotation = Quaternion.identity;
        wayPointRoot.transform.localScale = Vector3.one;
        wayPointRoot.layer = LayerManager.WayPoint;
        for (int i = 0; i < CombatData.Ins.wayPoints.Count; i++) {
            WayPoint wp = CombatData.Ins.wayPoints[i];
            GameObject waypoint = new GameObject(string.Format("way_{0}", wp.index));
            waypoint.transform.SetParent(wayPointRoot.transform);
            waypoint.layer = LayerManager.WayPoint;
            waypoint.transform.position = wp.pos;
            waypoint.transform.rotation = Quaternion.identity;
            waypoint.transform.localScale = Vector3.one;
            WayPoints waypoints = waypoint.AddComponent<WayPoints>();
            BoxCollider b = waypoint.AddComponent<BoxCollider>();
            b.size = Vector3.one * wp.size;
            b.isTrigger = true;
            waypoints.WayIndex = wp.index;
        }
    }

    public List<GameObject> wayArrowList = new List<GameObject>();
    GameObject wayArrowRoot;
    public void ShowWayPoint(bool on)
    {
        if (CombatData.Ins.GLevelItem == null)
            return;
        if (!on)
        {
            for (int i = 0; i < wayArrowList.Count; i++)
                GameObject.Destroy(wayArrowList[i]);
            wayArrowList.Clear();
        }

        if (on)
        {
            if (wayArrowList.Count != 0)
                return;
            if (wayArrowRoot == null) {
                wayArrowRoot = new GameObject("wayArrowRoot");
                wayArrowRoot.transform.position = Vector3.zero;
                wayArrowRoot.transform.rotation = Quaternion.identity;
                wayArrowRoot.transform.localScale = Vector3.one;
                wayArrowRoot.layer = LayerManager.WayPoint;
            }
            for (int i = 0; i < CombatData.Ins.wayPoints.Count; i++)
            {
                //GameObject obj = WsGlobal.AddDebugLine(CombatData.Ins.wayPoints[i].pos - 2 * Vector3.up, CombatData.Ins.wayPoints[i].pos + 2 * Vector3.up, Color.red, "WayPoint" + i, float.MaxValue, true);
                //wayPointList.Add(obj);
                //BoxCollider capsule = obj.AddComponent<BoxCollider>();
                //capsule.isTrigger = true;
                //capsule.size = Vector3.one * (Global.GLevelItem.wayPoint[i].size) * 10;
                //capsule.center = Vector3.zero;
                //obj.name = string.Format("WayPoint{0}", CombatData.Ins.wayPoints[i].index);

                foreach (var each in CombatData.Ins.wayPoints[i].link)
                {
                    GameObject objArrow = GameObject.Instantiate(Resources.Load("PathArrow")) as GameObject;
                    objArrow.transform.SetParent(wayArrowRoot.transform);
                    objArrow.layer = LayerManager.WayPoint;
                    objArrow.transform.position = CombatData.Ins.wayPoints[i].pos;
                    Vector3 vec = CombatData.Ins.wayPoints[each.Key].pos - CombatData.Ins.wayPoints[i].pos;
                    objArrow.transform.forward = vec.normalized;
                    objArrow.transform.localScale = new Vector3(30, 30, vec.magnitude / 2.2f);
                    objArrow.name = string.Format("Way{0}->Way{1}:{2}", CombatData.Ins.wayPoints[each.Key].index, CombatData.Ins.wayPoints[i].index, each.Value.mode);
                    wayArrowList.Add(objArrow);
                }
            }
        }

        GameStateMgr.Ins.gameStatus.ShowWayPoint = on;
    }

    public void OnSceneEvent(SceneEvent evt, int unit)
    {
        if (Scene_OnEvent != null)
            Scene_OnEvent.Invoke(CombatData.Ins.GScript, new object[] { unit, evt});
    }
}

public enum StackAction
{
    SAY = 1,
    PAUSE = 2,
    SKILL = 3,
    CROUCH = 4,
    BLOCK = 5,
    GUARD = 6,
    //Wait = 7,
    Follow = 7,
    Patrol = 8,
    FaceTo = 9,
    Kill = 10,
    Aggress = 11,
    AttackTarget = 12,
    Use = 13,//使用道具
    Dodge = 14,//躲避
    Help = 15,//疗伤
}

//基本上只能存在单机模式下,联机不支持
public class ActionConfig
{
    public List<ActionItem> action = new List<ActionItem>();
    public int id;
    public void Update(float time)
    {
        if (action.Count != 0)
        {
            if (action[action.Count - 1].type == StackAction.PAUSE) {
                if (action[action.Count - 1].first) {
                    MeteorUnit unit = U3D.GetUnit(id);
                    if (unit != null)
                        unit.AIPause(true, action[action.Count - 1].pause_time);
                    action[action.Count - 1].first = false;
                }
                action[action.Count - 1].pause_time -= time;

                if (action[action.Count - 1].pause_time <= 0.0f) {
                    MeteorUnit unit = U3D.GetUnit(id);
                    if (unit != null)
                        unit.AIPause(false, 0);
                    action.RemoveAt(action.Count - 1);
                }
            } else if (action[action.Count - 1].type == StackAction.SAY) {
                MeteorUnit unit = U3D.GetUnit(id);
                if (FightState.Exist() && unit != null)
                    FightState.Instance.InsertFightMessage(unit.name + " : " + action[action.Count - 1].text);
                if (ReplayState.Exist() && unit != null)
                    ReplayState.Instance.InsertFightMessage(unit.name + " : " + action[action.Count - 1].text);
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.SKILL) {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit && !unit.Dead)
                    unit.PlaySkill();
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.Aggress) {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit && !unit.Dead) {
                    //在攻击或防御中,无法嘲讽其他
                    if (unit.ActionMgr.IsAttackPose() || unit.ActionMgr.IsHurtPose()) {

                    } else
                        unit.ActionMgr.ChangeAction(CommonAction.Taunt, 0.1f);
                }
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.CROUCH) {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null) {
                    if (action[action.Count - 1].param == 1) {
                        unit.meteorController.Input.OnKeyDownProxy(EKeyList.KL_Crouch, true);
                    } else
                        unit.meteorController.Input.OnKeyUpProxy(EKeyList.KL_Crouch, true);
                }
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.BLOCK) {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null) {
                    bool locked = action[action.Count - 1].param == 1;
                    unit.meteorController.LockInput(locked);
                }
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.GUARD) {
                if (action[action.Count - 1].pause_time > 0) {
                    MeteorUnit unit = U3D.GetUnit(id);
                    if (unit && !unit.Dead)
                        unit.Guard(true, action[action.Count - 1].pause_time);
                    action[action.Count - 1].pause_time -= time;
                } else {
                    MeteorUnit unit = U3D.GetUnit(id);
                    if (unit && !unit.Dead)
                        unit.Guard(false, 0);
                    action.RemoveAt(action.Count - 1);
                }
            } else if (action[action.Count - 1].type == StackAction.Help) {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null && unit.StateMachine != null)
                    unit.ActionMgr.ChangeAction(CommonAction.Reborn, 0.1f);
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.Follow) {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null && unit.StateMachine != null)
                    unit.StateMachine.FollowTarget(action[action.Count - 1].param);
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.Patrol) {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null && unit.StateMachine != null && !unit.Dead) {
                    unit.StateMachine.SetPatrolPath(action[action.Count - 1].Path);
                    //有些关卡会设置不存在的路点，特别是玩家剧本里,很多错误，如果所有路点都不存在，则不要修改角色的状态
                    if (unit.StateMachine.GetPatrolPath().Count != 0)
                        unit.StateMachine.ChangeState(unit.StateMachine.PatrolState);//多点巡逻.
                    else
                        unit.StateMachine.ChangeState(unit.StateMachine.WaitState);//待机-搜索
                }
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.FaceTo) {
                MeteorUnit unit = U3D.GetUnit(id);
                MeteorUnit target = U3D.GetUnit(action[action.Count - 1].param);
                if (unit != null && target != null) {
                    if (!unit.Dead)
                        unit.FaceToTarget(target);
                }
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.Kill) {
                MeteorUnit unit = U3D.GetUnit(id);
                MeteorUnit target = U3D.GetUnit(action[action.Count - 1].param);
                if (unit != null && target != null)
                    unit.Kill(target);
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.AttackTarget) {
                //攻击指定位置
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null && unit.StateMachine != null && !unit.Dead) {
                    Idevgame.Meteor.AI.AttackState attackstate = null;
                    if (unit.StateMachine.IsAttacking()) {
                        attackstate = unit.StateMachine.CurrentState as Idevgame.Meteor.AI.AttackState;
                        if (attackstate.AttackTargetComplete()) {
                            action.RemoveAt(action.Count - 1);
                            unit.StateMachine.ChangeState(unit.StateMachine.WaitState);
                        }
                        return;
                    }
                    unit.StateMachine.ChangeState(unit.StateMachine.AttackState);
                    attackstate = unit.StateMachine.CurrentState as Idevgame.Meteor.AI.AttackState;
                    attackstate.AttackCount = action[action.Count - 1].param;
                    attackstate.AttackTarget = action[action.Count - 1].target;
                    attackstate.StartAttack();
                }
            } else if (action[action.Count - 1].type == StackAction.Use) {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null)
                    unit.GetItem(action[action.Count - 1].param);
                action.RemoveAt(action.Count - 1);
            } else if (action[action.Count - 1].type == StackAction.Dodge) {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null && unit.StateMachine != null && !unit.Dead) {
                    unit.StateMachine.ChangeState(unit.StateMachine.DodgeState, U3D.GetUnit(action[action.Count - 1].param));
                }
                action.RemoveAt(action.Count - 1);
            }
        }
    }
}
public class ActionItem
{
    public StackAction type;
    public bool first;
    public float pause_time;
    public string text;
    public int param;
    public Vector3 target;
    public List<int> Path;//for patrol
    public ActionItem()
    {
        first = true;
    }
}