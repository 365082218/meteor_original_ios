using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SLua;
using System.Collections.Generic;
using System;
using System.IO;

using UnityEngine.Profiling;
using protocol;
using System.Linq;
using System.Net;
using System.ComponentModel;
using Excel2Json;
using Idevgame.Util;
using Idevgame.GameState.DialogState;

[CustomLuaClass]
public class U3D : MonoBehaviour {
    public static U3D Ins = null;
    void Awake()
    {
        if (Ins == null)
            Ins = this;
    }

    [DoNotToLua]
    public static List<string> NicksBoys;
    [DoNotToLua]
    public static List<string> NicksGirls;
    [DoNotToLua]
    public static void ClearNames() {
        if (NicksBoys != null)
            NicksBoys.Clear();
        if (NicksGirls != null)
            NicksGirls.Clear();
    }

    public static string GetRandomName(bool girls = true)
    {
        List<string> nl = girls ? NicksGirls : NicksBoys;
        if (nl != null && nl.Count != 0)
        {
            int index2 = Utility.Range(0, nl.Count);
            string m = nl[index2];
            nl.RemoveAt(index2);
            return m;
        }

        string data_path = girls ? "NameGirls" : "NameBoys";
        TextAsset name = Resources.Load<TextAsset>(data_path);
        string s = name.text.Replace("\r\n", ",");
        s = s.Replace(" ", ",");
        nl = s.Split(new char[] { ','}, StringSplitOptions.RemoveEmptyEntries).ToList();
        int index = Utility.Range(0, nl.Count);
        string n = nl[index];
        nl.RemoveAt(index);
        return n;
    }

    public static int GetPrevUnitId(int unit)
    {
        List<int> all = GetUnitList();
        int index = all.IndexOf(unit);
        index -= 1;
        index = Mathf.Clamp(index, 0, all.Count - 1);
        return all[index];
    }

    public static int GetNextUnitId(int unit)
    {
        List<int> all = GetUnitList();
        int index = all.IndexOf(unit);
        index += 1;
        index = Mathf.Clamp(index, 0, all.Count - 1);
        return all[index];
    }

    //取得某个种类的随机一把武器.
    public static int GetRandomWeaponType()
    {
        int i = Utility.Range((int)EquipWeaponType.Sword, 1 + (int)EquipWeaponType.NinjaSword);
        return i;
    }

    public static int GetNormalWeaponType()
    {
        List<int> s = new List<int>();
        for (int i = 0; i <= 11; i++)
        {
            s.Add(i);
        }
        //去掉远程武器
        s.Remove(2);
        s.Remove(3);
        s.Remove(6);
        int k = Utility.Range(0, s.Count);
        return s[k];
    }

    public static List<int> GetUnitList()
    {
        List<int> all = new List<int>();
        for (int i = 0; i < CombatData.Ins.MaxModel; i++)
        {
            all.Add(i);
        }

        if (GameStateMgr.Ins.gameStatus != null)
        {
            for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginModel.Count; i++)
            {
                GameStateMgr.Ins.gameStatus.pluginModel[i].Check();
                if (!GameStateMgr.Ins.gameStatus.pluginModel[i].Installed)
                    continue;
                all.Add(GameStateMgr.Ins.gameStatus.pluginModel[i].ModelId);
            }
        }
        return all;
    }

    //取得随机英雄ID
    public static int GetRandomUnitIdx()
    {
        List<int> all = GetUnitList();
        return all[Utility.Range(0, all.Count)];
    }

    public static EUnitCamp GetAnotherCamp(EUnitCamp camp)
    {
        if (camp == EUnitCamp.EUC_ENEMY)
            return EUnitCamp.EUC_FRIEND;
        if (camp == EUnitCamp.EUC_FRIEND)
            return EUnitCamp.EUC_ENEMY;
        return EUnitCamp.EUC_KILLALL;
    }

    //通过EquipWeaponCode,得到指定类型的武器.用于在联机界面的武器选择时.
    public static int GetWeaponByCode(int i)
    {
        if (i < 0 || i >= weaponCode.Length)
            return 5;//默认是匕首
        return weaponCode[i];
    }

    public static int GetWeaponBySort(int weapon, bool next) {
        List<ItemData> we = DataMgr.Ins.GetItemDatas();
        bool compare_begin = false;
        int firstWeapon = -1;
        int step = next ? 1 : -1;
        int start = next ? 0 : we.Count - 1;
        int end = next ? we.Count : 0;
        for (int i = start; i != end && i >= 0 && i < we.Count;) {
            if (firstWeapon == -1) {
                if (we[i].MainType == (int)UnitType.Weapon) {
                    firstWeapon = we[i].Key;
                }
            }

            if (compare_begin) {
                if (we[i].MainType == (int)UnitType.Weapon) {
                    return we[i].Key;
                } else {
                    i += next ? 1 : -1;
                    continue;
                }
            }

            if (we[i].Key == weapon) {
                i += next ? 1 : -1;
                compare_begin = true;
                continue;
            } else {
                i += next ? 1 : -1;
            }
        }

        return firstWeapon;
    }

    //通过EquipWeaponType,得到指定类型的武器.用于在
    public static int GetWeaponByType(int weaponIndex)
    {
        List<int> weaponList = null;
        if (weaponDict.ContainsKey(weaponIndex))
            weaponList = weaponDict[weaponIndex];
        else
            weaponList = new List<int>();
        if (weaponList.Count == 0)
        {
            List<ItemData> we = DataMgr.Ins.GetItemDatas();
            for (int i = 0; i < we.Count; i++)
            {
                if (we[i].MainType == (int)UnitType.Weapon)
                {
                    if (we[i].SubType == (int)weaponIndex)
                    {
                        weaponList.Add(we[i].Key);
                    }
                }
            }
            if (!weaponDict.ContainsKey(weaponIndex))
                weaponDict.Add(weaponIndex, weaponList);
        }
        int k = Rand(weaponList.Count);
        return weaponList[k];
    }

    //七星，旋风，怒火，峨眉，蛇吻，碧血剑，战戟,斩铁,流星,乾坤刀,指虎,忍刀
    static int[] weaponCode = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 47, 51, 55 };
    //1,13,17,女性
    //其他男性
    static SortedDictionary<int, List<int>> weaponDict = new SortedDictionary<int, List<int>>();
    public static void SpawnRobot(int idx, EUnitCamp camp, int weaponIndex = 0, int hpMax = 1000)
    {
        if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer)
        {
            U3D.PopupTip("联机无法添加机器人");
            return;
        }

        List<int> weaponList = null;
        if (weaponDict.ContainsKey(weaponIndex))
            weaponList = weaponDict[weaponIndex];
        else
            weaponList = new List<int>();
        if (weaponList.Count == 0)
        {
            List<ItemData> we = DataMgr.Ins.GetItemDatas();
            for (int i = 0; i < we.Count; i++)
            {
                if (we[i].MainType == (int)UnitType.Weapon)
                {
                    if (we[i].SubType == (int)weaponIndex)
                    {
                        weaponList.Add(we[i].Key);
                    }
                }
            }
            if (!weaponDict.ContainsKey(weaponIndex))
                weaponDict.Add(weaponIndex, weaponList);
        }

        MonsterEx mon = new MonsterEx(hpMax * 10);
        int k = Rand(weaponList.Count);
        mon.Weapon = weaponList[k];
        mon.Weapon2 = weaponList[(k + 1) % weaponList.Count];
        mon.IsPlayer = false;

        //得到角色的性别信息，避免男角色用女角色名字很古怪的样子
        ModelItem model = DlcMng.GetPluginModel(idx);
        bool girls = false;
        if (model != null)
            girls = model.useFemalePos;
        else {
            if (idx == 1 || idx == 3 || idx == 13)
                girls = true;
            else
                girls = false;
        }

        mon.name = U3D.GetRandomName(girls);
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        unit.Camp = camp;
        mon.GetItem = 0;
        mon.View = 1000;//视野给大一点
        unit.Init(idx, mon);
        UnitTopState unitTopState = new UnitTopState(unit);
        PersistDialogMgr.Ins.EnterState(unitTopState);
        MeteorManager.Ins.OnGenerateUnit(unit);
        unit.SetGround(false);

        if (camp == EUnitCamp.EUC_FRIEND)
        {
            unit.SetPosition(CombatData.Ins.GCampASpawn[CombatData.Ins.CampASpawnIndex]);
            CombatData.Ins.CampASpawnIndex++;
            CombatData.Ins.CampASpawnIndex %= 8;
        }
        else if (camp == EUnitCamp.EUC_ENEMY)
        {
            unit.SetPosition(CombatData.Ins.GCampBSpawn[CombatData.Ins.CampBSpawnIndex]);
            CombatData.Ins.CampBSpawnIndex++;
            CombatData.Ins.CampBSpawnIndex %= 8;
        }
        else
        {
            //16个点
            unit.SetPosition(CombatData.Ins.GLevelSpawn[CombatData.Ins.SpawnIndex]);
            CombatData.Ins.SpawnIndex++;
            CombatData.Ins.SpawnIndex %= 16;
        }
        
        //战场开始后才进入战场，否则交给关卡初始化部分处理角色入场
        if (FrameReplay.Ins.Started)
            InsertSystemMsg(U3D.GetCampEnterLevelStr(unit.Camp, unit.name));
        //找寻敌人攻击.因为这个并没有脚本模板
        if (Main.Ins.LocalPlayer != null)
            unit.StateMachine.FollowTarget(Main.Ins.LocalPlayer.InstanceId);
        else
            unit.StateMachine.ChangeState(unit.StateMachine.WaitState);
        
        if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
        {
            
        }
        else if (CombatData.Ins.GGameMode == GameMode.ANSHA)
        {
            if (unit.IsLeader)
                U3D.ChangeBehaviorEx(unit.InstanceId, "follow", new object[] { "enemyvip" });
            else
                U3D.ChangeBehaviorEx(unit.InstanceId, "follow", new object[] { "vip" });
        }
        else if (CombatData.Ins.GGameMode == GameMode.SIDOU)
        {
            if (unit.IsLeader)
                U3D.ChangeBehaviorEx(unit.InstanceId, "follow", new object[] { "enemyvip" });
            else
                U3D.ChangeBehaviorEx(unit.InstanceId, "follow", new object[] { "vip" });
        }
        return;
    }

    public static List<int> GetUnitBuffs(MeteorUnit player) {
        List<int> buffs = new List<int>();
        foreach (var each in BuffMng.Ins.BufDict) {
            if (each.Value.Units.ContainsKey(player))
                buffs.Add(each.Key);
        }
        return buffs;
    }

    public static void ChangePlayerModel(MeteorUnit player, int model) {
        player.Init(model, player.Attr, true);
    }

    public static MeteorUnit InitNetPlayer(PlayerSync player) {
        MonsterEx mon = SceneMng.Ins.InitNetPlayer(player);
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        if (mon.IsPlayer)
            Main.Ins.LocalPlayer = unit;
        unit.Camp = (EUnitCamp)player.camp;
        unit.Init(mon.Model, mon);
        if (player.playerId != NetWorkBattle.Ins.PlayerId) {
            UnitTopState unitTopState = new UnitTopState(unit);
            PersistDialogMgr.Ins.EnterState(unitTopState);
        }
        MeteorManager.Ins.OnGenerateUnit(unit, (int)player.playerId);
        unit.SetGround(false);
        if (U3D.IsMultiplyPlayer()) {
            if (player.position != null)
                unit.transform.position = new Vector3(player.position.x / 1000.0f, player.position.y / 1000.0f, player.position.z / 1000.0f);//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
            if (player.rotation != null)
                unit.transform.rotation = new Quaternion(player.rotation.x / 1000.0f, player.rotation.y / 1000.0f, player.rotation.z / 1000.0f, player.rotation.w / 1000.0f);
            //设置角色当前的BUFF状态
            if (player.buff != null) {
                for (int i = 0; i < player.buff.Count; i++) {
                    AddBuff((int)player.playerId, (int)player.buff[i]);
                }
            }
            //设置角色当前动作
            unit.ActionMgr.SetAction(player.action);
        }
        return unit;
    }

    public static MeteorUnit InitNetPlayer(PlayerEvent player)
    {
        MonsterEx mon = SceneMng.Ins.InitNetPlayer(player);
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        if (mon.IsPlayer)
            Main.Ins.LocalPlayer = unit;
        unit.Camp = (EUnitCamp)player.camp;
        unit.Init(mon.Model, mon);
        if (player.playerId != NetWorkBattle.Ins.PlayerId) {
            UnitTopState unitTopState = new UnitTopState(unit);
            PersistDialogMgr.Ins.EnterState(unitTopState);
        }
        MeteorManager.Ins.OnGenerateUnit(unit, (int)player.playerId);
        unit.SetGround(false);
        unit.ResetPosition();
        return unit;
    }

    public static void OnDestroyNetPlayer(PlayerEvent player) {
        MeteorUnit unit = GetUnit((int)player.playerId);
        if (unit != null) {
            InsertSystemMsg(player.name + "离开了战场");
            MeteorManager.Ins.OnRemoveUnit(unit);
        }
    }

    public static void OnCreateNetPlayer(PlayerSync player) {
        MeteorUnit unit = InitNetPlayer(player);
        InsertSystemMsg(GetCampEnterLevelStr(unit.Camp, unit.name));
    }

    public static void OnCreateNetPlayer(PlayerEvent player) {
        MeteorUnit unit = InitNetPlayer(player);
        InsertSystemMsg(GetCampEnterLevelStr(unit.Camp, unit.name));
    }


    public static MeteorUnit InitPlayer(LevelScriptBase script)
    {
        MonsterEx mon = SceneMng.Ins.InitPlayer(script);
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        Main.Ins.LocalPlayer = unit;
        unit.Camp = EUnitCamp.EUC_FRIEND;//流星阵营
        //单机剧本.用设置的主角，刷新替换掉，关卡里设定的.
        if (GameStateMgr.Ins.gameStatus.UseModel != -1 && CombatData.Ins.GLevelMode <= LevelMode.CreateWorld) {
            //先检查这个ID是否正常
            ModelItem model = DlcMng.GetPluginModel(GameStateMgr.Ins.gameStatus.UseModel);
            if (model != null)
                mon.Model = GameStateMgr.Ins.gameStatus.UseModel;
            else
                GameStateMgr.Ins.gameStatus.UseModel = -1;
        }
        unit.Init(mon.Model, mon);
        MeteorManager.Ins.OnGenerateUnit(unit);
        unit.SetGround(false);
        if (CombatData.Ins.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            if (CombatData.Ins.wayPoints.Count == 0)
            {
                unit.transform.position = CombatData.Ins.GLevelSpawn[mon.SpawnPoint];
            }
            else
                unit.transform.position = CombatData.Ins.wayPoints.Count > mon.SpawnPoint ? CombatData.Ins.wayPoints[mon.SpawnPoint].pos : CombatData.Ins.wayPoints[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
        }
        else if (CombatData.Ins.GLevelMode > LevelMode.SinglePlayerTask && CombatData.Ins.GLevelMode <= LevelMode.MultiplyPlayer)
        {
            if (CombatData.Ins.GGameMode == GameMode.Normal)
            {
                unit.transform.position = CombatData.Ins.wayPoints.Count > mon.SpawnPoint ? CombatData.Ins.wayPoints[mon.SpawnPoint].pos : CombatData.Ins.wayPoints[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
            }
            else if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                unit.transform.position = CombatData.Ins.GLevelSpawn[CombatData.Ins.SpawnIndex];
                CombatData.Ins.SpawnIndex++;
                CombatData.Ins.SpawnIndex %= 16;
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (CombatData.Ins.GGameMode == GameMode.ANSHA || CombatData.Ins.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.
                if (unit.Camp == EUnitCamp.EUC_FRIEND)
                {
                    unit.transform.position = CombatData.Ins.GCampASpawn[CombatData.Ins.CampASpawnIndex];
                    CombatData.Ins.CampASpawnIndex++;
                    CombatData.Ins.CampASpawnIndex %= 8;
                }
                else if (unit.Camp == EUnitCamp.EUC_ENEMY)
                {
                    unit.transform.position = CombatData.Ins.GCampASpawn[CombatData.Ins.CampBSpawnIndex];
                    CombatData.Ins.CampBSpawnIndex++;
                    CombatData.Ins.CampBSpawnIndex %= 8;
                }
            }
        }
        unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
        U3D.InsertSystemMsg(U3D.GetCampEnterLevelStr(unit.Camp, unit.name));
        return unit;
    }

    public static string GetDefaultFile(string Path, FileExt type, bool local, bool inzipPath)
    {
        string suffix = "";
        switch (type)
        {
            case FileExt.Jpeg:
                suffix = ".jpg";
                break;
            case FileExt.Dll:
                suffix = ".dll";
                break;
            case FileExt.Txt:
                suffix = ".txt";
                break;
            case FileExt.Json:
                suffix = ".json";
                break;
        }

        string shortDir = "";
        int k = Path.LastIndexOf('/');
        shortDir = Path.Substring(0, k);
        string shortName = Path.Substring(k + 1);
        shortName = shortName.Substring(0, shortName.Length - 4);
        if (local)
            return Main.Ins.localPath + "/Plugins/" + shortDir + "/" + (inzipPath ? shortName + "/" + shortName: shortName) + suffix;//.zip => .png
        return Path.Substring(0, Path.Length - 4) + suffix;
    }

    public static int AddNPC(string script)
    {
        if (string.IsNullOrEmpty(script))
            return -1;
        MeteorUnit target = SceneMng.Ins.Spawn(script);
        return target.InstanceId;
    }

    public static string GetCampStr(EUnitCamp Camp)
    {
        if (Camp == EUnitCamp.EUC_ENEMY)
            return string.Format("{0}","蝴蝶");
        if (Camp == EUnitCamp.EUC_FRIEND)
            return string.Format("{0}", "流星");
        return string.Format("{0}", "无");
    }

    public static string GetCampEnterLevelStr(EUnitCamp Camp, string name)
    {
        if (Camp == EUnitCamp.EUC_ENEMY)
            return string.Format("{0} 选择蝴蝶,进入战场", name);
        if (Camp == EUnitCamp.EUC_FRIEND)
            return string.Format("{0} 选择流星,进入战场", name);
        return string.Format("{0} 进入战场", name);
    }

    public static void InsertSystemMsg(string msg)
    {
        if (GameOverlayDialogState.Exist())
            GameOverlayDialogState.Instance.InsertSystemMsg(msg);
    }

    public static void ShowTargetBlood() {
        GameStateMgr.Ins.gameStatus.ShowBlood = true;
        InsertSystemMsg("打开敌方血量,攻击任意敌方后展示");
    }
    //弹出一个简单提示
    public static void PopupTip(string str)
    {
        PopupTipController Tips = new PopupTipController();
        Tips.Popup(str);
    }

    static UnityEngine.AsyncOperation backOp;
    static UnityEngine.AsyncOperation loadMainOp;

    //回到关卡选择面板，继续剧本
    public static void GoToLevelMenu()
    {
        U3D.LoadScene("Startup", () => {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LevelDialogState, CombatData.Ins.Chapter == null);
            ClearLevelData();
        });
    }
    //返回到主目录
    public static void GoBack()
    {
        CombatData.Ins.GLevelItem = null;
        CombatData.Ins.wayPoints = null;
        CombatData.Ins.GLevelMode = LevelMode.SinglePlayerTask;
        U3D.LoadScene("Startup", () => {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainMenuState);
            ClearLevelData();
        });
    }

    //修改版本号后回到Startup重新加载资源
    public static void ReStart()
    {
        FrameReplay.Ins.OnDisconnected();
        CombatData.Ins.GLevelItem = null;
        CombatData.Ins.wayPoints = null;
        CombatData.Ins.GLevelMode = LevelMode.SinglePlayerTask;
        U3D.LoadScene("Startup", () => {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.StartupDialogState);
            ClearLevelData();
        });
    }

    void OnLoadMainFinished(Action t)
    {
        //MainWnd.Instance.Open();
        GameStateMgr.Ins.SaveState();
        if (t != null)
            t.Invoke();
    }

    public static void OpenRobotWnd()
    {
        if (RobotDialogState.Exist)
            RobotDialogState.Instance.OnBackPress();
        else
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RobotDialogState);
    }

    public static void OpenSfxWnd()
    {
        if (SfxDialogState.Exist)
            SfxDialogState.Instance.OnBackPress();
        else
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.SfxDialogState);
    }

    //打开武器界面，主角色调试切换主手武器.
    public static void OpenWeaponWnd()
    {
        if (WeaponDialogState.Exist)
            WeaponDialogState.Instance.OnBackPress();
        else
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.WeaponDialogState);
    }

    public static void OpenSystemWnd()
    {
        if (CombatData.Ins.GLevelItem != null)
        {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.EscDialogState);
            if (UGUIJoystick.Ins != null)
                UGUIJoystick.Ins.Lock(true);
            return;
        }
    }

    public static void PlayBtnAudio(string audio = "btn")
    {
        if (Main.Ins.SoundManager != null)
            SoundManager.Ins.PlaySound(audio);
    }

    public static void LoadScene(string scene, Action OnFinished)
    {
        LevelHelper helper = Ins.gameObject.AddComponent<LevelHelper>();
        helper.LoadScene(scene, OnFinished);
    }

    //加载当前设置的关卡.在打开模组剧本时/联机
    public static void LoadLevelEx()
    {
        ClearLevelData();
        SoundManager.Ins.StopAll();
        SoundManager.Ins.Enable(false);
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LoadingDialogState);
        LevelHelper helper = Ins.gameObject.AddComponent<LevelHelper>();
        helper.Load();
    }

    static bool CheckRecord(GameRecord rec) {
        if (rec.frames == null) {
            PopupTip("指令记录为空,无法播放");
            return false;
        }

        if (rec.AppVersion != AppInfo.Ins.AppVersion()) {
            PopupTip(string.Format("游戏版本不一致,无法播放 录像:{0} 当前:{1}", rec.AppVersion, AppInfo.Ins.AppVersion()));
            return false;
        }

        if (rec.MeteorVersion != AppInfo.Ins.MeteorV1()) {
            PopupTip("流星版本不一致,无法播放,录像:" + (rec.MeteorVersion == 1 ? "1.07" : "9.07") + " 当前:" + (AppInfo.Ins.MeteorV1() == 1 ? "1.07" : "9.07"));
            return false;
        }

        return true;
    }

    public static bool IsMultiplyPlayer() {
        if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer)
            return true;
        return false;
    }

    //播放录像-不够精确会导致演算误差越来越大
    public static void PlayRecord(GameRecord rec)
    {
        U3D.PopupTip("功能已取消");
        //if (CheckRecord(rec)) {
        //    //第二部检查，需要查看录像保存的设置，和当前的设置之间的差异，有差异的，要提醒，不然表现会不一样
        //    //录像的状态，覆盖当前的状态，但是不保存
        //    GameStateMgr.Ins.gameStatus.EnableGodMode = rec.EnableGodMode;
        //    GameStateMgr.Ins.gameStatus.HidePlayer = rec.HidePlayer;
        //    GameStateMgr.Ins.gameStatus.EnableInfiniteAngry = rec.EnableInfiniteAngry;
        //    GameStateMgr.Ins.gameStatus.Undead = rec.Undead;
        //    CombatData.Ins.GRecord = rec;
        //    CombatData.Ins.GLevelItem = rec.Level;
        //    CombatData.Ins.Chapter = DlcMng.GetPluginChapter(rec.Chapter);
        //    CombatData.Ins.GLevelMode = rec.LevelMode;
        //    CombatData.Ins.GGameMode = rec.GameMode;
        //    CombatData.Ins.wayPoints = CombatData.GetWayPoint(CombatData.Ins.GLevelItem);
        //    LoadLevelEx();
        //}
    }

    //走参数指定关卡.非剧本&非回放
    public static void LoadLevel(LevelData lev, LevelMode levelmode, GameMode gamemode)
    {
        CombatData.Ins.GRecord = null;
        CombatData.Ins.Chapter = null;
        ClearLevelData();
        Resources.UnloadUnusedAssets();
        GC.Collect();
        CombatData.Ins.GLevelItem = lev;
        CombatData.Ins.GLevelMode = levelmode;
        CombatData.Ins.GGameMode = gamemode;
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LoadingDialogState);
        //暂时不允许使用声音管理器，在切换场景时不允许播放
        SoundManager.Ins.StopAll();
        SoundManager.Ins.Enable(false);
        CombatData.Ins.wayPoints = CombatData.GetWayPoint(lev);
        LevelHelper helper = Ins.gameObject.AddComponent<LevelHelper>();
        helper.Load();
    }

    public static void ClearLevelData()
    {
        SFXLoader.Ins.Clear();
        DesLoader.Ins.Clear();
        GMCLoader.Ins.Clear();
        GMBLoader.Ins.Clear();
        FMCLoader.Ins.Clear();
        SkcLoader.Ins.Clear();
        BncLoader.Ins.Clear();
        FMCPoseLoader.Ins.Clear();
        //先清理BUF
        BuffMng.Ins.Clear();
        MeteorManager.Ins.Clear();
        LevelScriptBase.Clear();
        CombatData.Ins.GLevelSpawn = null;
        CombatData.Ins.GCampASpawn = null;
        CombatData.Ins.GCampBSpawn = null;
        CombatData.Ins.CampASpawnIndex = 0;
        CombatData.Ins.CampBSpawnIndex = 0;
        CombatData.Ins.SpawnIndex = 0;
        CombatData.Ins.GameFinished = false;
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    public static void SetSceneItem(string name, string features, int value1)
    {

        SceneItemAgent agent = MeteorManager.Ins.FindSceneItem(name);
        if (agent != null)
            agent.SetSceneItem(features, value1);
    }

    public static void SetSceneItem(string name, string features, string sub_features, int value)
    {
        SceneItemAgent agent = MeteorManager.Ins.FindSceneItem(name);
        if (agent != null)
            agent.SetSceneItem(features, sub_features, value);
    }

    //setSceneItem("xx", "pos", posid, loop)
    public static void SetSceneItem(string name, string features, int value1, int value2)
    {
        SceneItemAgent agent = MeteorManager.Ins.FindSceneItem(name);
        if (agent != null)
            agent.SetSceneItem(features, value1, value2);
    }

    public static void SetSceneItem(int id, string features, string sub_feature, int value)
    {
        SceneItemAgent objSelected = GetSceneItem(id);
        if (objSelected != null)
            objSelected.SetSceneItem(features, sub_feature, value);
    }

    public static void SetSceneItem(int id, string features, int value1, int value2)
    {
        SceneItemAgent objSelected = GetSceneItem(id);
        if (objSelected != null)
            objSelected.SetSceneItem(features, value1, value2);
    }

    public static void SetSceneItem(int id, string feature, string value)
    {
        SceneItemAgent objSelected = GetSceneItem(id);
        
        if (objSelected != null)
            objSelected.SetSceneItem(feature, value);
    }
    public static void SetSceneItem(string name, string feature, string value)
    {
        SceneItemAgent agent = MeteorManager.Ins.FindSceneItem(name);
        if (agent != null)
            agent.SetSceneItem(feature, value);
    }

    public static SceneItemAgent GetSceneItem(string name)
    {
        for (int i = 0; i < MeteorManager.Ins.SceneItems.Count; i++)
        {
            if (MeteorManager.Ins.SceneItems[i].name == name)
                return MeteorManager.Ins.SceneItems[i];
        }
        return null;
    }

    public static int GetSceneItem(string name, string feature)
    {
        for (int i = 0; i < MeteorManager.Ins.SceneItems.Count; i++)
        {
            if (MeteorManager.Ins.SceneItems[i].name == name)
                return MeteorManager.Ins.SceneItems[i].GetSceneItem(feature);
        }
        return -1;
    }

    public static SceneItemAgent GetSceneItem(int id)
    {
        for (int i = 0; i < MeteorManager.Ins.SceneItems.Count; i++)
        {
            if (MeteorManager.Ins.SceneItems[i].InstanceId == id)
                return MeteorManager.Ins.SceneItems[i];
        }
        return null;
    }

    public static PickupItemAgent GetPickupItem(int id) {
        for (int i = 0; i < MeteorManager.Ins.PickupItems.Count; i++) {
            if (MeteorManager.Ins.PickupItems[i].InstanceId == id)
                return MeteorManager.Ins.PickupItems[i];
        }
        return null;
    }

    public static int GetSceneItem(int id, string feature)
    {
        for (int i = 0; i < MeteorManager.Ins.SceneItems.Count; i++)
        {
            if (MeteorManager.Ins.SceneItems[i].InstanceId == id)
                return MeteorManager.Ins.SceneItems[i].GetSceneItem(feature);
        }
        return -1;
    }

    public static int GetTeam(int characterId)
    {
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i].InstanceId == characterId)
                return (int)MeteorManager.Ins.UnitInfos[i].Camp;
        }

        for (int i = 0; i < MeteorManager.Ins.DeadUnits.Count; i++) {
            if (MeteorManager.Ins.DeadUnits[i].InstanceId == characterId)
                return (int)MeteorManager.Ins.DeadUnits[i].Camp;
        }
        return -1;
    }

    public static void CreateEffect(string target, string effect, bool loop = false)
    {
        GameObject objEffect = null;
        for (int i = 0; i < Loader.Instance.transform.childCount; i++)
        {
            GameObject obj = Loader.Instance.transform.GetChild(i).gameObject;
            if (obj.name == target)
            {
                objEffect = obj;
                break;
            }
        }
        if (Main.Ins.SFXLoader != null && objEffect != null)
            SFXLoader.Ins.PlayEffect(effect, objEffect, !loop);
    }

    public static void CreateEffect(int id, string effect, bool loop = false)
    {
        SceneItemAgent[] agents = FindObjectsOfType<SceneItemAgent>();
        SceneItemAgent objEffect = null;
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i].InstanceId == id)
            {
                objEffect = agents[i];
                break;
            }
        }
        if (Main.Ins.SFXLoader != null && objEffect != null)
            SFXLoader.Ins.PlayEffect(effect, objEffect.gameObject, !loop);
    }

    public static MeteorUnit GetTeamLeader(EUnitCamp camp)
    {
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i].Camp == camp)
            {
                if (MeteorManager.Ins.UnitInfos[i].IsLeader)
                    return MeteorManager.Ins.UnitInfos[i];
            }
        }
        return null;
    }

    //取得敌对方首领.
    static MeteorUnit GetEnemyTeamLeader(EUnitCamp camp)
    {
        if (camp == EUnitCamp.EUC_ENEMY)
            return GetTeamLeader(EUnitCamp.EUC_FRIEND);
        else if (camp == EUnitCamp.EUC_FRIEND)
            return GetTeamLeader(EUnitCamp.EUC_ENEMY);
        return null;
    }

    //提供给外部脚本调的
    public static void ChangeBehavior(int id, params object[] value)
    {
        if (value != null && value.Length != 0)
        {
            string act = value[0] as string;
            if (act == "wait")
            {
                Main.Ins.GameBattleEx.PushActionWait(id);
            }
            else if (act == "follow")
            {
                //object target = null;
                if (value[1].GetType() == typeof(int))
                {
                    Main.Ins.GameBattleEx.PushActionFollow(id, (int)value[1]);
                }
                else
                {
                    //target = value[1] as string;
                    if (value[1] as string == "vip")
                    {
                        //跟随己方首领.
                        MeteorUnit u = GetUnit(id);
                        if (u != null)
                        {
                            MeteorUnit t = GetTeamLeader(u.Camp);
                            if (t != null)
                                Main.Ins.GameBattleEx.PushActionFollow(id, t.InstanceId);
                        }
                    }
                    else if (value[1] as string == "enemyvip")
                    {
                        //跟随敌方首领.
                        MeteorUnit u = GetUnit(id);
                        if (u != null)
                        {
                            MeteorUnit t = GetEnemyTeamLeader(u.Camp);
                            if (t != null)
                                Main.Ins.GameBattleEx.PushActionFollow(id, t.InstanceId);
                        }
                    }
                    else if (value[1] as string == "player")
                    {
                        Main.Ins.GameBattleEx.PushActionFollow(id, Main.Ins.LocalPlayer.InstanceId);
                    }
                    else
                    {
                        int flag = U3D.GetChar("flag");//跟随镖物跑，A，镖物被人取得，B，镖物在地图某处.
                        if (flag >= 0)
                        {
                            Main.Ins.GameBattleEx.PushActionFollow(id, flag);
                        }
                        else
                        {
                            Debug.LogError(string.Format("follow flag:{0} can not find", flag));
                        }
                    }
                }
            }
            else if (act == "patrol")
            {
                List<int> Path = new List<int>();
                for (int i = 1; i < value.Length; i++)
                {
                    if (value[i] == null)
                        continue;
                    if (value[i].GetType() == typeof(double))
                        Path.Add((int)(double)value[i]);
                    else if (value[i].GetType() == typeof(int))
                        Path.Add((int)value[i]);
                }
                Main.Ins.GameBattleEx.PushActionPatrol(id, Path);
            }
            else if (act == "faceto")
            {
                Main.Ins.GameBattleEx.PushActionFaceTo(id, (int)value[1]);
            }
            else if (act == "kill")
            {
                //Debug.LogError("gamebattleex kill");
                Main.Ins.GameBattleEx.PushActionKill(id, (int)value[1]);
            }
            else if (act == "idle")
            {
                //原地不动.
                Main.Ins.GameBattleEx.PushActionIdle(id);
            }
            else if (act == "attacktarget")
            {
                if (value.Length == 2)
                {
                    //UnityEngine.Debug.LogError(string.Format("attacktarget:{0}", (int)value[1]));
                    Main.Ins.GameBattleEx.PushActionAttackTarget(id, (int)value[1]);
                }
                else if (value.Length == 3)
                {
                    //UnityEngine.Debug.LogError(string.Format("attacktarget:{0}, {1}", (int)value[1], (int)value[2]));
                    Main.Ins.GameBattleEx.PushActionAttackTarget(id, (int)value[1], (int)value[2]);
                }
            }
            else if (act == "run")
            {
                Main.Ins.GameBattleEx.PushActionWait(id);
            }
            else if (act == "dodge")
            {
                MeteorUnit u = GetUnit(id);
                if (value.Length == 2) {
                    if (value[1].GetType() == typeof(int)) {
                        Main.Ins.GameBattleEx.PushActionDodge(id, (int)value[1]);
                    }
                }
            }
        }
    }
    // behavior="wait", "idle", "run", "follow", "patrol", "attacktarget", "kill"
    //只有string 和 int类型，后者
    public static void ChangeBehaviorEx(int id, string act, params object[] value)
    {
        object[] param = new object[value == null ? 1 : value.Length + 1];
        param[0] = act;
        if (value != null)
        {
            for (int i = 0; i < value.Length; i++)
                param[i + 1] = value[i];
        }
        ChangeBehavior(id, param);
    }

    public static MeteorUnit GetFlag()
    {
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i].GetFlag)
                return MeteorManager.Ins.UnitInfos[i];
        }
        return null;
    }

    public static int GetChar(string player)
    {
        if (player == "player")
            return Main.Ins.LocalPlayer.InstanceId;
        //取得谁获得了任务物品
        if (player == "flag")
        {
            for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
            {
                if (MeteorManager.Ins.UnitInfos[i].GetFlag)
                    return MeteorManager.Ins.UnitInfos[i].InstanceId;
            }
            return -1;
        }
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i].name == player)
                return MeteorManager.Ins.UnitInfos[i].InstanceId;
        }

        for (int i = 0; i < MeteorManager.Ins.DeadUnits.Count; i++)
        {
            if (MeteorManager.Ins.DeadUnits[i].name == player)
                return MeteorManager.Ins.DeadUnits[i].InstanceId;
        }

        foreach (var each in MeteorManager.Ins.LeavedUnits)
        {
            if (each.Value == player)
                return each.Key;
        }
        return -1;
    }

    // name="player", "vip", "enemyvip", "flag", "xxx"
    public static int GetAnyChar(string name)
    {
        if (name == "player")
            return 0;
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i].name == name)
                return MeteorManager.Ins.UnitInfos[i].InstanceId;
        }
        for (int i = 0; i < MeteorManager.Ins.DeadUnits.Count; i++)
        {
            if (MeteorManager.Ins.DeadUnits[i].name == name)
                return MeteorManager.Ins.DeadUnits[i].InstanceId;
        }
        return -1;
    }

    public static int GetAngry(int id)
    {
        MeteorUnit unit = GetUnit(id);
        if (unit != null)
            return unit.AngryValue;
        return 0;
    }

    public static int GetHP(int id)
    {
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i].InstanceId == id)
                return MeteorManager.Ins.UnitInfos[i].Attr.hpCur;
        }
        for (int i = 0; i < MeteorManager.Ins.DeadUnits.Count; i++)
        {
            if (MeteorManager.Ins.DeadUnits[i].InstanceId == id)
                return MeteorManager.Ins.DeadUnits[i].Attr.hpCur;
        }
        return 0;
    }
    public static int GetMaxHP(int id)
    {
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i].InstanceId == id)
                return MeteorManager.Ins.UnitInfos[i].Attr.HpMax;
        }
        for (int i = 0; i < MeteorManager.Ins.DeadUnits.Count; i++)
        {
            if (MeteorManager.Ins.DeadUnits[i].InstanceId == id)
                return MeteorManager.Ins.DeadUnits[i].Attr.HpMax;
        }
        return 0;
    }

    public static int GetEnemy(int id)
    {
        MeteorUnit unit = GetUnit(id);
        if (unit == null)
            return -1;
        MeteorUnit enemy = unit.LockTarget;
        if (enemy != null)
            return enemy.InstanceId;
        return -1;
    }

    //场景发生破坏后，对路点的影响.
    public static int EnableWaypoints(int a, int b, params int[] way)
    {
        return 0;
    }

    public static int DisableWaypoints(int a, int b, params int[] way)
    {
        return 0;
    }

    public static int Perform(int id, string pose, int last)
    {
        return Perform(id, pose, new int[] { last });
    }

    public static int Perform(int id, string pose, params int[] fun)
    {
        MeteorUnit unit = GetUnit(id);
        if (unit != null)
        {
            if (pose == "pause" && fun != null && fun.Length == 1) {
                Main.Ins.GameBattleEx.PushActionPause(id, fun[0]);
            }
            //unit.PauseAI(fun[0]);
            else if (pose == "faceto" && fun != null && fun.Length == 1) {
                MeteorUnit target = GetUnit(fun[0]);
                if (target != null)
                    unit.FaceToTarget(target);
            } else if (pose == "guard" && fun != null && fun.Length == 1)
                Main.Ins.GameBattleEx.PushActionGuard(id, fun[0]);
            else if (pose == "aggress")
                Main.Ins.GameBattleEx.PushActionAggress(id);
            else if (pose == "attack") {
                //攻击。？？
            } else if (pose == "use") {
                unit.GetItem(fun[0]);
            } else if (pose == "skill") {
                Main.Ins.GameBattleEx.PushActionSkill(id); //unit.PlaySkill();
            } else if (pose == "crouch")
                Main.Ins.GameBattleEx.PushActionCrouch(id, fun[0]);//1是指令状态，1代表应用状态， 0代表取消状态
            else if (pose == "block")
                Main.Ins.GameBattleEx.PushActionBlock(id, fun[0]);//阻止输入/取消阻止输入.与硬直应该差不多.
            else if (pose == "help")
                Main.Ins.GameBattleEx.PushActionHelp(id, fun[0]);
        }
        return 0;
    }

    public static int Perform(int id, string pose, string param)
    {
        if (pose == "say")
            Say(id, param);
        return 0;
    }
    // pose="faceto", "say", "pause", "aggress", "jump", "attack", "guard", "crouch", "use"
    public static int PlayerPerform(string act, int param)
    {
        if (act == "pause")
            Main.Ins.GameBattleEx.PushActionPause(0, param);//0代表主角色.
        else if (act == "use")
        {
            Main.Ins.GameBattleEx.PushActionUse(0, param);
        }
        else if (act == "crouch")
            Main.Ins.GameBattleEx.PushActionCrouch(0, param);//1是指令状态，1代表应用状态， 0代表取消状态
        else if (act == "block")
            Main.Ins.GameBattleEx.PushActionBlock(0, param);//阻止输入/取消阻止输入.与硬直应该差不多.
        else if (act == "guard")
            Main.Ins.GameBattleEx.PushActionGuard(0, param);
        else if (act == "faceto")
        {
            MeteorUnit target = GetUnit(param);
            if (target != null)
                Main.Ins.LocalPlayer.FaceToTarget(target);
        }
        //Debug.Log("PlayerPerform(string act, int param):" + "act:" + act + " param:" + param);
        return 0;
    }
    public static int PlayerPerform(string act, string content)
    {
        if (act == "say")
            Say(0, content);
        return 0;
    }

    // pose="say", "pause", "use", "block"
    public static int StopPerform(int id)
    {
        MeteorUnit u = GetUnit(id);
        //停止动作的时候，状态为等待，一般遇见敌人时会调用此接口
        if (u != null)
        {
            u.AIPause(false, 0);
            //先清除该角色的聊天动作，其他动画动作不处理。
            Main.Ins.GameBattleEx.StopAction(id);
        }
        return 0;
    }

    // type="char", "waypoint", "flag", "safe"
    //获得2个角色的距离.
    public static float Distance(int idx1, int idx2)
    {
        if (CombatData.Ins.GLevelItem != null && CombatData.Ins.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            MeteorUnit a = GetUnit(idx1);
            MeteorUnit b = GetUnit(idx2);
            if (a == null || b == null)
                return 0;
            return Vector3.Distance(a.transform.position, b.transform.position);
        }
        return 0;
    }

    public static int Rand(int n)
    {
        return Utility.Range(0, n);
    }

    //单机下
    public static void RotatePlayer(string name, float yRotate)
    {
        if (CombatData.Ins.GLevelItem != null && CombatData.Ins.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            int id = GetChar(name);
            if (id != -1)
            {
                MeteorUnit unit = GetUnit(id);
                if (unit != null)
                    unit.transform.rotation = Quaternion.Euler(0, yRotate, 0);
            }
        }
    }

    public static void MovePlayer(string name, Vector3 position)
    {
        if (CombatData.Ins.GLevelItem != null && CombatData.Ins.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            int id = GetChar(name);
            if (id != -1)
            {
                MeteorUnit unit = GetUnit(id);
                if (unit != null)
                    unit.SetPosition(position);
            }
        }
    }

    public static void MoveNpc(string name, int spawnPoint)
    {
        if (CombatData.Ins.GLevelItem != null && CombatData.Ins.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            int id = GetChar(name);
            if (id != -1)
            {
                MeteorUnit unit = GetUnit(id);
                if (unit != null)
                    unit.SetPosition(spawnPoint);
            }
        }
    }

    public static void RemoveNPC(int id)
    {
        MeteorUnit unit = GetUnit(id);
        bool dead = unit.Dead;
        string message = unit.name + " 离开战场";
        MeteorManager.Ins.OnRemoveUnit(unit);
        if (!dead)
            InsertSystemMsg(message);
    }

    public static void UpdateAIAttrib(int id)
    {
        MeteorUnit monster = GetUnit(id);
        monster.Attr.UpdateAttr();
    }

    public static string GetNetPlayerName(int id) {
        MeteorUnit unit = GetUnit(id);
        return unit != null ? unit.name : "不明身份者";
    }

    public static MeteorUnit GetUnit(int id)
    {
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i].InstanceId == id)
                return MeteorManager.Ins.UnitInfos[i];
        }

        for (int i = 0; i < MeteorManager.Ins.DeadUnits.Count; i++)
        {
            if (MeteorManager.Ins.DeadUnits[i].InstanceId == id)
                return MeteorManager.Ins.DeadUnits[i];
        }
        return null;
    }

    public static void Say(int id, string param)
    {
        if (Main.Ins.GameBattleEx != null)
            Main.Ins.GameBattleEx.PushActionSay(id, param);//1=say 2=pause
    }

    

    public static int GetEnemyCount()
    {
        int total = 0;
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i].Camp != EUnitCamp.EUC_Meteor)
                total++;
        }
        return total;
    }

    //脚本输入框支持
    //角色获得物品-可拾取类
    public static void GetItem(int player, int item)
    {
        MeteorUnit u = GetUnit(player);
        if (u != null)
            u.GetItem(item);
    }

    //角色获得BUFF
    public static void AddBuff(int player, int buff)
    {
        GetItem(player, buff);
    }

    //角色获得怒气
    public static void AddAngry(int player, int angry)
    {
        MeteorUnit u = GetUnit(player);
        if (u != null)
            u.AddAngry(angry);
    }

    public static Camera GetMainCamera()
    {
        GameObject objCamera = GameObject.Find("CameraEx");
        return objCamera == null ? null : objCamera.GetComponent<Camera>();
    }

    public static bool IsSpecialWeapon(EquipWeaponType t)
    {
        return t == EquipWeaponType.Gun || t == EquipWeaponType.Guillotines || t == EquipWeaponType.Dart;
    }

    public static bool IsSpecialWeapon(int itemIdx)
    {
        ItemData it0 = GameStateMgr.Ins.FindItemByIdx(itemIdx);
        if (it0 == null)
            return false;
        if (it0.SubType == (int)EquipWeaponType.Gun || 
            it0.SubType == (int)EquipWeaponType.Guillotines || 
            it0.SubType == (int)EquipWeaponType.Dart)
        {
            return true;
        }
        return false;
    }

    public static void UnlockLevel()
    {
        GameStateMgr.Ins.gameStatus.Level = CombatData.LEVELMAX;
        if (GameStateMgr.Ins.gameStatus.pluginChapter != null) {
            for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginChapter.Count; i++) {
                Chapter chapter = GameStateMgr.Ins.gameStatus.pluginChapter[i];
                List<LevelData> all = chapter.LoadAll();
                if (all != null && all.Count != 0)
                    chapter.level = all[all.Count - 1].Id;
            }
        }
        GameStateMgr.Ins.SaveState();
    }

    public static bool IsUnitDead(int instanceid)
    {
        MeteorUnit u = GetUnit(instanceid);
        if (u)
            return u.Dead;
        return true;
    }

    public static bool AllFriendDead()
    {
        bool alldead = true;
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i] == null)
                continue;
            if (MeteorManager.Ins.UnitInfos[i].SameCamp(Main.Ins.LocalPlayer))
            {
                if (!MeteorManager.Ins.UnitInfos[i].Dead)
                {
                    alldead = false;
                    break;
                }
            }
        }
        return alldead;
    }

    public static bool AllEnemyDead()
    {
        bool alldead = true;
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i] == null)
                continue;
            if (!MeteorManager.Ins.UnitInfos[i].SameCamp(Main.Ins.LocalPlayer))
            {
                if (!MeteorManager.Ins.UnitInfos[i].Dead)
                {
                    alldead = false;
                    break;
                }
            }
        }
        return alldead;
    }

    public static void OnPauseAI()
    {
        //Debug.LogError("pause ai");
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i] == null)
                continue;
            MeteorManager.Ins.UnitInfos[i].EnableAI(false);
        }
    }

    public static void EnableUpdate(bool enable) {
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++) {
            if (MeteorManager.Ins.UnitInfos[i] == null)
                continue;
            MeteorManager.Ins.UnitInfos[i].EnableUpdate(enable);
        }
    }

    public static void OnResumeAI()
    {
        //Debug.LogError("resume ai");
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (MeteorManager.Ins.UnitInfos[i] == null)
                continue;
            MeteorManager.Ins.UnitInfos[i].EnableAI(true);
        }
    }

    public static void Snow() {
        if (CombatData.Ins.GScript != null)
            CombatData.Ins.GScript.Snow();
    }

    public static void DoScript() {
        ScriptInputDialogState.State.Open();
    }

    //以下为脚本系统执行面板里能响应的操作
    public static void GodLike()
    {
        GameStateMgr.Ins.gameStatus.CheatEnable = !GameStateMgr.Ins.gameStatus.CheatEnable;
        U3D.InsertSystemMsg(GameStateMgr.Ins.gameStatus.CheatEnable ? "作弊[开]" : "作弊[关]");
    }

    public static WeaponData GetWeaponProperty(int weaponIdx)
    {
        WeaponData w = DataMgr.Ins.GetWeaponData(weaponIdx);
        //if (w == null)
        //    w = PluginWeaponMng.Instance.GetItem(weaponIdx);
        if (w == null)
            Debug.LogError("can not find weapon:" + weaponIdx);
        return w;
    }

    public static void DeletePlugins(int filter = 0)
    {
        if (filter == 0)
            GameStateMgr.Ins.gameStatus.pluginChapter.Clear();
        if (filter == 1)
            GameStateMgr.Ins.gameStatus.pluginModel.Clear();
        CombatData.Ins.PluginUpdated = false;
        GameStateMgr.Ins.SaveState();
    }

    /// <summary>
    /// 加载路径内的指令文件-供播放
    /// </summary>
    public static byte[] LoadReplayData()
    {
        return null;
    }

    public static void ShowLeaderSfx()
    {
        MeteorUnit uEnemy = U3D.GetTeamLeader(EUnitCamp.EUC_ENEMY);
        MeteorUnit uPlayer = U3D.GetUnit(0);
        SFXLoader.Ins.PlayEffect("vipblue.ef", uPlayer.gameObject, false);
        if (uEnemy != null)
            SFXLoader.Ins.PlayEffect("vipred.ef", uEnemy.gameObject, false);
    }

    public static void Kill(int instance) {
        MeteorUnit unit = GetUnit(instance);
        if (unit != null) {
            unit.OnDead();
        }
    }

    //踢出
    public static void Kick(int id) {

    }

    //永久剔除-这个房间记录对方的IP，不允许对方再加入.
    public static void Skick(int id) {

    }

    [DoNotToLua]
    public static bool showBox = false;
    [DoNotToLua]
    public static bool WatchAi = false;

    public static void Box() {
        showBox = !showBox;
        if (Main.Ins.GameBattleEx == null) {
            return;
        }
        if (showBox) {
            if (Main.Ins.LocalPlayer != null)
                Main.Ins.LocalPlayer.ShowAttackBox();
        } else {
            BoundsGizmos.Instance.Clear();
        }
    }

    [DoNotToLua]
    public static void CloseBox() {
        showBox = false;
        if (BoundsGizmos.Instance)
            BoundsGizmos.Instance.Clear();
    }

    [DoNotToLua]
    public static void CancelWatch() {
        WatchAi = false;
    }

    [DoNotToLua]
    public static void WatchPrevRobot() {
        if (!U3D.WatchAi) {
            U3D.InsertSystemMsg("需要先[观察电脑]");
            return;
        }
        MeteorUnit watchTarget = Main.Ins.CameraFree.Watched;
        List<MeteorUnit> allow = new List<MeteorUnit>();
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++) {
            if (MeteorManager.Ins.UnitInfos[i] == null)
                continue;
            if (MeteorManager.Ins.UnitInfos[i].Dead)
                continue;
            if (MeteorManager.Ins.UnitInfos[i] == Main.Ins.LocalPlayer)
                continue;
            allow.Add(MeteorManager.Ins.UnitInfos[i]);
        }

        if (allow.Count != 0) {
            int j = allow.IndexOf(watchTarget);
            if (j <= 0) {
                watchTarget = allow[allow.Count - 1];
            } else {
                watchTarget = allow[j - 1];
            }
            Main.Ins.CameraFree.Init(watchTarget);
        }
    }

    [DoNotToLua]
    public static void WatchNextRobot() {
        if (!U3D.WatchAi) {
            U3D.InsertSystemMsg("需要先[观察电脑]");
            return;
        }
        MeteorUnit watchTarget = Main.Ins.CameraFree.Watched;
        List<MeteorUnit> allow = new List<MeteorUnit>();
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++) {
            if (MeteorManager.Ins.UnitInfos[i] == null)
                continue;
            if (MeteorManager.Ins.UnitInfos[i].Dead)
                continue;
            if (MeteorManager.Ins.UnitInfos[i] == Main.Ins.LocalPlayer)
                continue;
            allow.Add(MeteorManager.Ins.UnitInfos[i]);
        }
        if (allow.Count != 0) {
            int j = allow.IndexOf(watchTarget);
            if (j == allow.Count - 1 || j == -1) {
                watchTarget = allow[0];
            } else {
                watchTarget = allow[j + 1];
            }
            Main.Ins.CameraFree.Init(watchTarget);
        }
    }
}
