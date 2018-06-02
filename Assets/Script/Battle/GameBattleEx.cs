using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using CoClass;

//负责战斗中相机的位置指定之类，主角色目标组作为摄像机 视锥体范围，参考Tank教程里的简单相机控制
//负责战斗场景内位置间的寻路缓存
public class GameBattleEx : MonoBehaviour {
    [HideInInspector]
    public CameraFollow m_CameraControl;
    int time = 1000;
    float timeTick;
    float timeClock = 0.0f;
    const float ViewLimit = 180;
    static GameBattleEx _Ins;
    public static GameBattleEx Instance { get { return _Ins; } }
    // Use this for initialization
    void Awake()
    {
        if (_Ins == null)
        {
            _Ins = this;
            m_CameraControl = GameObject.Find("CameraEx").GetComponent<CameraFollow>();
        }   
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (_Ins == this)
            _Ins = null;
    }

    //显示失败，或者胜利界面 >=1 = win <= 0 = lose 2 == none
    public void GameOver(int result)
    {
        //锁定键盘鼠标摇杆按键。显示一个战斗结束面板，看是平局还是负，然后开个定时器，跳转到主场景
        //if (result > 0)
        //{
        //    //战斗胜利
        //    OnBattleWinWnd();
        //}
        //else if (result < 0)
        //{
        //    OnBattleFailed();
        //}
        //关闭声音
        SoundManager.Instance.StopAll();
        //关闭界面的血条缓动和动画
        if (FightWnd.Exist)
            FightWnd.Instance.OnBattleEnd();
        if (DebugWnd.Exist)
            DebugWnd.Instance.Close();
        if (NewSystemWnd.Exist)
            NewSystemWnd.Instance.Close();
        //打开结算面板
        if (BattleResultWnd.Exist)
            BattleResultWnd.Instance.Close();
        BattleResultWnd.Instance.Open();
        StartCoroutine(BattleResultWnd.Instance.SetResult(result));

        if (NGUICameraJoystick.instance)
            NGUICameraJoystick.instance.Lock(true);
        if (NGUIJoystick.instance)
            NGUIJoystick.instance.Lock(true);
        //如果胜利，且不是最后一关，打开最新关标志.
        if (result == 1 && GameData.gameStatus.Level < 9 && Global.GLevelItem.ID + 1 > GameData.gameStatus.Level)
            GameData.gameStatus.Level = Global.GLevelItem.ID + 1;
        Global.PauseAll = true;
        Invoke("PlayEndMovie", 5.0f);
    }

    void PlayEndMovie()
    {
        if (!string.IsNullOrEmpty(Global.GLevelItem.goodList))
        {
            string num = Global.GLevelItem.goodList.Substring(2);
            int number = int.Parse(num);
            Debug.Log("v" + number);
            U3D.PlayMovie("v" + number);
        }
        
        GotoMenu();
    }

    public int GetMiscGameTime()
    {
        return Mathf.FloorToInt(1000 * timeClock);
    }

    public int GetGameTime()
    {
        return Mathf.FloorToInt(timeClock);
    }

    void Start () {
        timeTick = 0.0f;
        StartCoroutine(UpdateTime());
	}

    IEnumerator UpdateTime()
    {
        while (true)
        {
            if (IsTimeup())
            {
                StopCoroutine("UpdateTime");
                yield break;
            }
            yield return new WaitForSeconds(1);
            if (FightWnd.Exist)
                FightWnd.Instance.UpdateTime(GetTimeClock());
        }
    }

    GameObject SelectTarget;
    float timeDelay = 1;
	// Update is called once per frame
	void Update () {
        if (Global.PauseAll)
            return;

        timeClock += Time.deltaTime;
        if (timeClock > (float)time)//时间超过，平局
        {
            GameOver(2);
            return;
        }

        //更新BUFF
        foreach (var each in BuffMng.Instance.BufDict)
            each.Value.Update();

        //更新左侧角色对话文本
        for (int i = 0; i < UnitActKey.Count; i++)
        {
            if (UnitActionStack.ContainsKey(UnitActKey[i]))
                UnitActionStack[UnitActKey[i]].Update(Time.deltaTime);
        }

        //3S以后开始播放剧本
        if (lev_script != null)
        {
            lev_script.OnUpdate();
            //timeDelay = 0.0f;
        }

        timeDelay += Time.deltaTime;
        if (lev_script != null)
            lev_script.Scene_OnIdle();//每一帧调用一次场景物件更新.
        RefreshAutoTarget();
        CollisionCheck();
    }

    private void CollisionCheck()
    {
        //每一个攻击盒，与所有受击盒碰撞测试.
        Dictionary<MeteorUnit, List<MeteorUnit>> hitDic = new Dictionary<MeteorUnit, List<MeteorUnit>>();
        //每个攻击盒，与所有环境物受击盒碰撞测试
        Dictionary<MeteorUnit, List<SceneItemAgent>> hitDic2 = new Dictionary<MeteorUnit, List<SceneItemAgent>>();
        foreach (var attack in DamageList)
        {
            //攻击盒与角色受击盒碰撞
            foreach (var each in MeteorManager.Instance.UnitInfos)
            {
                if (attack.Key == each)
                    continue;
                //同阵营不计算碰撞
                if (attack.Key.SameCamp(each))
                    continue;
                //已经发生过碰撞不再次计算
                if (attack.Key.ExistDamage(each))
                    continue;
                //buff无法攻击的没考虑.
                //不允许碰撞多次.
                if (hitDic.ContainsKey(attack.Key) && hitDic[attack.Key].Contains(each))
                    continue;
                if (OnHitTest(attack.Value, each))
                {
                    //Debug.LogError("OnHitTest");
                    if (hitDic.ContainsKey(attack.Key))
                        hitDic[attack.Key].Add(each);
                    else
                        hitDic.Add(attack.Key, new List<MeteorUnit> { each });
                    //攻击盒打受击盒
                    //attack.Key.Attack(each);
                    //受击盒
                    //each.OnAttack(attack.Key);
                }
            }

            //攻击盒与环境物件碰撞
            foreach (var each in Collision)
            {
                if (attack.Key.ExistDamage(each.Key))
                    continue;
                if (OnHitTest(attack.Value, each.Value))
                {
                    if (hitDic2.ContainsKey(attack.Key))
                        hitDic2[attack.Key].Add(each.Key);
                    else
                        hitDic2.Add(attack.Key, new List<SceneItemAgent> { each.Key });
                }
            }
        }  
        foreach (var each in hitDic)
        {
            for (int i = 0; i < each.Value.Count; i++)
            {
                each.Key.Attack(each.Value[i]);
                each.Value[i].OnAttack(each.Key);
            }
        }
        foreach (var each in hitDic2)
        {
            for (int i = 0; i < each.Value.Count; i++)
            {
                each.Key.Attack(each.Value[i]);
                each.Value[i].OnDamage(each.Key);
            }
        }

        //处理推挤问题。
    }

    public void OnSFXDestroy(MeteorUnit unit, Collider co)
    {
        if (DamageList.ContainsKey(unit))
            DamageList[unit].Remove(co);
    }

    public bool OnHitTest(List<Collider> colist, MeteorUnit ondamaged)
    {
        for (int i = 0; i < colist.Count; i++)
        {
            if (colist[i] == null)//协程中删除
                continue;
            if (!colist[i].enabled)
                continue;
            for (int j = 0; j < ondamaged.hitList.Count; j++)
            {
                if (!ondamaged.hitList[j].enabled)
                    continue;
                if (colist[i].bounds.Intersects(ondamaged.hitList[j].bounds))
                    return true;
                //if (colist[i].bounds.min.x >= ondamaged.hitList[j].bounds.min.x &&
                //    colist[i].bounds.min.y >= ondamaged.hitList[j].bounds.min.y &&
                //    colist[i].bounds.min.z >= ondamaged.hitList[j].bounds.min.z &&
                //    colist[i].bounds.max.x <= ondamaged.hitList[j].bounds.max.x &&
                //    colist[i].bounds.max.y <= ondamaged.hitList[j].bounds.max.y &&
                //    colist[i].bounds.max.z <= ondamaged.hitList[j].bounds.max.z)
                //{
                //    return true;
                //}

                //if (colist[i].bounds.min.x <= ondamaged.hitList[j].bounds.min.x &&
                //    colist[i].bounds.min.y <= ondamaged.hitList[j].bounds.min.y &&
                //    colist[i].bounds.min.z <= ondamaged.hitList[j].bounds.min.z &&
                //    colist[i].bounds.max.x >= ondamaged.hitList[j].bounds.max.x &&
                //    colist[i].bounds.max.y >= ondamaged.hitList[j].bounds.max.y &&
                //    colist[i].bounds.max.z >= ondamaged.hitList[j].bounds.max.z)
                //{
                //    return true;
                //}
            }
        }
        return false;
    }

    public bool OnHitTest(List<Collider> colist, List<Collider> ondamaged)
    {
        for (int i = 0; i < colist.Count; i++)
        {
            if (colist[i] == null)
                continue;
            if (!colist[i].enabled)
                continue;
            for (int j = 0; j < ondamaged.Count; j++)
            {
                if (!ondamaged[j].enabled)
                    return false;
                if (colist[i].bounds.Intersects(ondamaged[j].bounds))
                    return true;
                //if (colist[i].bounds.min.x >= ondamaged[j].bounds.min.x &&
                //    colist[i].bounds.min.y >= ondamaged[j].bounds.min.y &&
                //    colist[i].bounds.min.z >= ondamaged[j].bounds.min.z &&
                //    colist[i].bounds.max.x <= ondamaged[j].bounds.max.x &&
                //    colist[i].bounds.max.y <= ondamaged[j].bounds.max.y &&
                //    colist[i].bounds.max.z <= ondamaged[j].bounds.max.z)
                //{
                //    return true;
                //}

                //if (colist[i].bounds.min.x <= ondamaged[j].bounds.min.x &&
                //    colist[i].bounds.min.y <= ondamaged[j].bounds.min.y &&
                //    colist[i].bounds.min.z <= ondamaged[j].bounds.min.z &&
                //    colist[i].bounds.max.x >= ondamaged[j].bounds.max.x &&
                //    colist[i].bounds.max.y >= ondamaged[j].bounds.max.y &&
                //    colist[i].bounds.max.z >= ondamaged[j].bounds.max.z)
                //{
                //    return true;
                //}
            }
        }
        return false;
    }

    //bool IsUIAction()
    //{
    //    LayerMask maskNgui = 1 << LayerMask.NameToLayer("NGUI") | 1 << LayerMask.NameToLayer("UI");
    //    //先看是否触发了UI控件.是则不选择对象
    //    Ray nguiRay = GameObject.Find("Camera").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
    //    RaycastHit[] UISelection = Physics.RaycastAll(nguiRay, 100, maskNgui.value);
    //    if (UISelection.Length != 0)
    //        return true;
    //    return false;
    //}

    bool IsSelectAction(ref GameObject touched)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = 1 << LayerMask.NameToLayer("LocalPlayer") | 1 << LayerMask.NameToLayer("Trigger");
        RaycastHit[] UISelection = Physics.RaycastAll(ray, 100, mask.value);
        if (UISelection.Length != 0)
        {
            float fdisMin = float.MaxValue;
            for (int i = 0; i < UISelection.Length; i++)
            {
                if (UISelection[i].distance <= fdisMin)
                {
                    fdisMin = UISelection[i].distance;
                    touched = UISelection[i].collider.gameObject;
                }
            }
            return true;
        }
        return false;
    }
    //SLua.LuaFunction updateFn;
    public LevelScriptBase Script { get { return lev_script; } }
    LevelScriptBase lev_script;
    public void Init(Level lev, LevelScriptBase script)
    {
        lev_script = script;
        //updateFn = ScriptMng.ins.GetFunc("OnUpdate");
        if (script != null)
            time = script.GetRoundTime() * 60;
        timeClock = 0.0f;
        //注册所有场景物件的受击框
        SceneItemAgent[] sceneObjs = FindObjectsOfType<SceneItemAgent>();
        //上一局如果暂停了，新局开始都恢复
        Resume();
        if (lev_script != null)
        {
            lev_script.Scene_OnLoad();
            lev_script.Scene_OnInit();
            lev_script.OnStart();
        }

        for (int i = 0; i < sceneObjs.Length; i++)
        {
            sceneObjs[i].OnStart(lev_script);
            RegisterCollision(sceneObjs[i]);
        }
        U3D.InsertSystemMsg("新回合开始计时");
    }

    //场景物件的受击框
    Dictionary<SceneItemAgent, List<Collider>> Collision = new Dictionary<SceneItemAgent, List<Collider>>();
    //Dictionary<SceneItemAgent, List<Collider>>
    public void RegisterCollision(SceneItemAgent agent)
    {
        if (!Collision.ContainsKey(agent))
        {
            if (agent.HasCollision())
                Collision.Add(agent, agent.GetCollsion());
        }
        else
        {
            if (agent.HasCollision())
                Collision[agent] = agent.GetCollsion();
            else
                Collision.Remove(agent);
        }
    }

    public void RemoveCollision(SceneItemAgent agent)
    {
        if (agent != null && Collision.ContainsKey(agent))
            Collision.Remove(agent);
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
            if (index == hit.Length)
                break;
        }
        return hit;
    }

    public void AddDamageCheck(MeteorUnit unit, AttackDes attackDef)
    {
        if (unit == null || unit.Dead)
            return;
        if (unit.GetWeaponType() == (int)EquipWeaponType.Gun)
        {
            Ray r = CameraFollow.Ins.m_Camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, DartLoader.MaxDistance));
            RaycastHit[] allHit = Physics.RaycastAll(r, 3000, 1 << LayerMask.NameToLayer("Bone") | 1 << LayerMask.NameToLayer("Scene") | 1 << LayerMask.NameToLayer("Monster") | 1 << LayerMask.NameToLayer("LocalPlayer"));
            RaycastHit[] allHitSort = SortRaycastHit(allHit);
            //先排个序，从近到远
            for (int i = 0; i < allHitSort.Length; i++)
            {
                if (allHitSort[i].transform.gameObject.layer == LayerMask.NameToLayer("Scene"))
                    break;
                MeteorUnit unitAttacked = allHitSort[i].transform.gameObject.GetComponentInParent<MeteorUnit>();
                if (unitAttacked != null)
                {
                    if (unit.SameCamp(unitAttacked))
                        continue;
                    if (unitAttacked.Dead)
                        continue;
                    unitAttacked.OnAttack(unit, attackDef);
                }
            }
        }
        else
        {
            //飞镖，那么从绑点生成一个物件，让他朝
            //"Sphere3"是挂点
            //即刻生成一个物件飞出去。
            if (unit.charLoader.sfxEffect != null)
            {
                if (unit.GetWeaponType() == (int)EquipWeaponType.Dart)
                {
                    //飞镖.
                    Transform bulletBone = unit.charLoader.sfxEffect.FindEffectByName("Sphere_3");//出生点，
                    Vector3 vecSpawn = bulletBone.position + 10 * Vector3.up;
                    Vector3 forw = Vector3.zero;
                    if (unit.Attr.IsPlayer)
                    {
                        Vector3 vec = CameraFollow.Ins.m_Camera.ScreenToWorldPoint(new Vector3(Screen.width / 2, (Screen.height) / 2 + 75, DartLoader.MaxDistance));
                        forw = (vec - vecSpawn).normalized;
                    }
                    else
                    {
                        //敌人在未发现敌人时随便发飞镖?
                        if (unit.GetLockedTarget() == null)
                        {
                            forw = (-unit.transform.forward + new Vector3(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10))).normalized;//角色的面向
                        }
                        else
                        {
                            //判断角色是否面向目标，不面向，则朝身前发射飞镖
                            MeteorUnit u = unit.GetLockedTarget();
                            if (u != null)
                            {
                                if (!unit.IsFacetoTarget(u))
                                    u = null;
                            }

                            if (u == null)
                                forw = (-unit.transform.forward + new Vector3(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10))).normalized;//角色的面向
                            else
                                forw = (unit.GetLockedTarget().mPos - new Vector3(unit.mPos.x + Random.Range(1, 10), unit.mPos.y + Random.Range(1, 20), unit.mPos.z + Random.Range(1, 10))).normalized;
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
                        MeteorUnit u = unit.GetLockedTarget();
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

    //所有角色的攻击盒.
    Dictionary<MeteorUnit, List<Collider>> DamageList = new Dictionary<MeteorUnit, List<Collider>>();
    //缓存角色的攻击盒.
    public void AddDamageCollision(MeteorUnit unit, Collider co)
    {
        if (unit == null || co == null)
            return;
        if (DamageList.ContainsKey(unit))
        {
            if (DamageList[unit].Contains(co))
                return;
            DamageList[unit].Add(co);
        }
        else
            DamageList.Add(unit, new List<Collider>() { co });
    }

    //清理角色的攻击盒.
    public void ClearDamageCollision(MeteorUnit unit)
    {
        if (DamageList.ContainsKey(unit))
            DamageList.Remove(unit);
    }

    public string GetTimeClock()
    {
        //600 = 10:00
        int left = time - Mathf.FloorToInt(timeClock);
        if (left < 0)
            left = 0;
        int minute = left / 60;
        int seconds = left % 60;
        string t = "";
        t = string.Format("{0:D2}:{1:D2}", minute, seconds);
        return t;
    }

    public bool IsTimeup()
    {
        int left = time - Mathf.FloorToInt(timeClock);
        if (left < 0)
            return true;
        return false;
    }

    //public void SetCameraTargets()
    //{
    //    MeteorUnit targetGroup = MeteorManager.Instance.LocalPlayer.GetLockedTarget();
    //    // Create a collection of transforms the same size as the number of tanks.
    //    Transform[] targets = new Transform[targetGroup == null ? 1 : 2];
    //    targets[0] = MeteorManager.Instance.LocalPlayer.transform;
    //    if (targets.Length == 2)
    //        targets[1] = targetGroup.CameraTarget.transform;
    //    // These are the targets the camera should follow.
    //    m_CameraControl.m_Targets = targets;
    //}

    #region 寻路缓存
    List<int> looked = new List<int>();
    public List<WayPoint> FindPath(Vector3 now, MeteorUnit user, MeteorUnit target, out int freeSlot, out Vector3 end)
    {
        int p0 = GetWayIndex(now);
        Vector3 vec = target.GetFreePos(out freeSlot, user);
        end = vec;
        int p1 = GetWayIndex(vec);
        looked.Clear();
        List<WayPoint> ret = FindPath2(p0, p1);
        return ret;
    }

    List<WayPoint> FindPath2(int start, int end)
    {
        if (looked.Contains(start))
            return null;
        looked.Add(start);
        List<WayPoint> path = new List<WayPoint>();
        if (start == -1 || end == -1)
            return path;
        //从开始点，跑到最终点，最短线路？
        if (Global.GLevelItem.wayPoint[start].link.ContainsKey(end))
        {
            path.Add(Global.GLevelItem.wayPoint[start]);
            path.Add(Global.GLevelItem.wayPoint[end]);
            return path;
        }
        else
        {
            //深度优先递归.
            Dictionary<int, WayLength> ways = Global.GLevelItem.wayPoint[start].link;
            foreach (var each in ways)
            {
                List<WayPoint> p = FindPath2(each.Key, end);
                if (p != null && p.Count != 0)
                {
                    for (int i = 0; i < p.Count; i++)
                        path.Add(p[i]);
                    return path;
                }
            }
        }
        return path;
    }

    int GetWayIndex(Vector3 now)
    {
        int ret = -1;
        float min = 10000.0f;
        for (int i = 0; i < Global.GLevelItem.wayPoint.Count; i++)
        {
            WayPoint way = Global.GLevelItem.wayPoint[i];
            float dis = Vector3.Distance(way.pos, now);
            if (dis <= way.size)
                return i;
            if (dis < min)
            {
                min = dis;
                ret = i;
            }
        }
        return ret;
    }
    #endregion

    //全部AI暂停，游戏时间停止，任何依据时间做动画的物件，全部要停止.
    public void Pause()
    {
        if (MeteorManager.Instance.LocalPlayer != null)
            MeteorManager.Instance.LocalPlayer.controller.LockInput(true);
        if (NGUIJoystick.instance != null)
            NGUIJoystick.instance.Lock(true);
        if (NGUICameraJoystick.instance != null)
            NGUICameraJoystick.instance.Lock(true);
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
            MeteorManager.Instance.UnitInfos[i].EnableAI(false);
        Global.PauseAll = true;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        if (MeteorManager.Instance.LocalPlayer != null)
            MeteorManager.Instance.LocalPlayer.controller.LockInput(false);
        if (NGUIJoystick.instance != null)
            NGUIJoystick.instance.Lock(false);
        if (NGUICameraJoystick.instance != null)
            NGUICameraJoystick.instance.Lock(false);
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
            MeteorManager.Instance.UnitInfos[i].EnableAI(true);
        Global.PauseAll = false;
        Time.timeScale = 1;
    }

    public MeteorUnit lockedTarget;
    public MeteorUnit autoTarget;
    public SFXEffectPlay lockedEffect;
    public SFXEffectPlay autoEffect;
    public bool CanLockTarget(MeteorUnit unit)
    {
        //使用枪械/远程武器时
        int weapon = MeteorManager.Instance.LocalPlayer.GetWeaponType();
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
            Vector3 vec = unit.transform.position - MeteorManager.Instance.LocalPlayer.transform.position;
            float v = vec.magnitude;
            if (v > ViewLimit)
                return false;
            //vec.y = 0;
            //vec = Vector3.Normalize(vec);
            //Vector3 vecPlayer = -MeteorManager.Instance.LocalPlayer.transform.forward;
            //float angle = Vector3.Dot(vecPlayer, vec);
            //float angleMax = Mathf.Cos(75 * Mathf.Deg2Rad);//cos值越大，角度越小
            ////大于75度，不能
            //if (angle < angleMax)
            //    return false;
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
            lockedEffect = SFXLoader.Instance.PlayEffect("lock.ef", lockedTarget.gameObject);
            bLocked = true;
        }
        if (FightWnd.Exist)
            FightWnd.Instance.OnChangeLock(bLocked);
    }
    //存在自动目标时，把自动目标删除，然后设置其为锁定目标.
    public bool bLocked = false;
    public void Lock()
    {
        if (autoTarget != null)
        {
            MeteorUnit lockTarget = autoTarget;
            MeteorManager.Instance.LocalPlayer.SetLockedTarget(lockTarget);
            ChangeLockedTarget(lockTarget);
        }
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
        MeteorManager.Instance.LocalPlayer.SetLockedTarget(null);
        CameraFollow.Ins.OnUnlockTarget();
        bLocked = false;
        if (FightWnd.Exist)
            FightWnd.Instance.OnChangeLock(bLocked);
    }

    //敌方角色移动时，不用整个刷新，只需要用当前的自动目标和这个对象对比下角度就OK
    public void RefreshAutoTarget()
    {
        if (MeteorManager.Instance.LocalPlayer.GetWeaponType() == (int)EquipWeaponType.Dart || MeteorManager.Instance.LocalPlayer.GetWeaponType() == (int)EquipWeaponType.Gun)
            return;
        if (lockedTarget != null)
        {
            Vector3 vec = lockedTarget.transform.position - MeteorManager.Instance.LocalPlayer.transform.position;
            float v = vec.magnitude;
            if (v > ViewLimit)
                Unlock();
            return;
        }
        MeteorUnit player = MeteorManager.Instance.LocalPlayer;
        float angleMax = Mathf.Cos(75 * Mathf.Deg2Rad);//cos值越大，角度越小
        float autoAngle = 0.0f;//自动目标与主角的夹角
        MeteorUnit want = null;
        Vector3 vecPlayer = -player.transform.forward;
        vecPlayer.y = 0;
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i] == player)
                continue;
            if (player.SameCamp(MeteorManager.Instance.UnitInfos[i]))
                continue;
            if (MeteorManager.Instance.UnitInfos[i].Dead)
                continue;
            Vector3 vec = MeteorManager.Instance.UnitInfos[i].transform.position - player.transform.position;
            float v = vec.magnitude;
            //飞轮时，无限角度距离
            if (v > ViewLimit && MeteorManager.Instance.LocalPlayer.GetWeaponType() != (int)EquipWeaponType.Guillotines)
                continue;
            //高度差2个角色，不要成为自动对象，否则摄像机位置有问题
            if (Mathf.Abs(vec.y) >= 75 && MeteorManager.Instance.LocalPlayer.GetWeaponType() != (int)EquipWeaponType.Guillotines)
                continue;
            vec.y = 0;
            vec = Vector3.Normalize(vec);
            float angle = Vector3.Dot(vecPlayer, vec);
            //角度小于75则可以成为自动对象.
            if (angle > angleMax)
            {
                angleMax = angle;
                want = MeteorManager.Instance.UnitInfos[i];
            }
            //保存自动对象 与主角的角度
            if (autoTarget == MeteorManager.Instance.UnitInfos[i])
                autoAngle = angle;
        }

        //与角色的夹角不能大于75度,否则主角脑袋骨骼可能注视不到自动目标.
        if (want != null && want != autoTarget)
        {
            if (autoEffect != null)
            {
                autoEffect.OnPlayAbort();
                autoEffect = null;
            }
            autoTarget = want;
            autoEffect = SFXLoader.Instance.PlayEffect("Track.ef", autoTarget.gameObject);
            return;
        }

        //如果当前的自动目标存在，且夹角超过75度，即在主角背后，那么自动目标清空
        if (autoTarget != null && autoAngle < Mathf.Cos(75 * Mathf.Deg2Rad) && MeteorManager.Instance.LocalPlayer.GetWeaponType() != (int)EquipWeaponType.Guillotines)
        {
            autoEffect.OnPlayAbort();
            autoTarget = null;
        }

        if (autoTarget != null && MeteorManager.Instance.LocalPlayer.GetWeaponType() != (int)EquipWeaponType.Guillotines)
        {
            Vector3 vec = autoTarget.transform.position - player.transform.position;
            float v = vec.magnitude;
            if (v > ViewLimit)
                Unlock();
        }
    }
    public class BattleResultItem
    {
        public int camp;
        public int deadCount;
        public int killCount;
        public int id;
    }
    Dictionary<string, BattleResultItem> battleResult = new Dictionary<string, BattleResultItem>();
    public Dictionary<string, BattleResultItem> BattleResult { get { return battleResult; } }
    public void OnUnitDead(MeteorUnit unit, MeteorUnit killer = null)
    {
        //无阵营的角色,杀死人，不统计信息
        if (killer != null && (killer.Camp == EUnitCamp.EUC_ENEMY || killer.Camp == EUnitCamp.EUC_FRIEND))
        {
            //统计杀人计数
            if (!battleResult.ContainsKey(killer.name))
            {
                BattleResultItem it = new BattleResultItem();
                it.camp = (int)killer.Camp;
                it.id = killer.InstanceId;
                it.deadCount = 0;
                it.killCount = 1;
                battleResult.Add(killer.name, it);
            }
            else
                battleResult[killer.name].killCount += 1;
        }

        if (unit != null && (unit.Camp == EUnitCamp.EUC_ENEMY || unit.Camp == EUnitCamp.EUC_FRIEND))
        {
            //统计被杀次数
            if (!battleResult.ContainsKey(unit.name))
            {
                BattleResultItem it = new BattleResultItem();
                it.camp = (int)unit.Camp;
                it.id = unit.InstanceId;
                it.deadCount = 1;
                it.killCount = 0;
                battleResult.Add(unit.name, it);
            }
            else
                battleResult[unit.name].deadCount += 1;
        }

        if (unit == MeteorManager.Instance.LocalPlayer)
        {
            GameBattleEx.Instance.GameOver(0);
            DropMng.Instance.Drop(unit);
            Unlock();
        }
        else
        {
            //无论谁杀死，都要爆东西
            DropMng.Instance.Drop(unit);
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
            //检测关卡通关或者失败条件。
            if (Global.GLevelItem.Pass == 1)//敌方阵营全死
            {
                int totalEnemy = U3D.GetEnemyCount();
                if (totalEnemy == 0)
                    GameOver(1);
            }
            else if (Global.GLevelItem.Pass == 2)//敌方指定脚本角色死亡
            {
                if (unit.Attr.NpcTemplate == Global.GLevelItem.Param)
                    GameOver(1);
            }
        }
    }

    void GotoMenu()
    {
        Pause();
        SoundManager.Instance.StopAll();
        MeteorManager.Instance.Clear();
        FightWnd.Instance.Close();
        U3D.GoBack(MainMenu.Instance.Open);
    }

    public void ChangeLockStatus()
    {
        if (lockedTarget != null)
            Unlock();
        else if (autoTarget != null)
            Lock();
    }
    //每个角色拥有的动作堆栈
    Dictionary<int, ActionConfig> UnitActionStack = new Dictionary<int, ActionConfig>();
    List<int> UnitActKey = new List<int>();
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
        PushAction(id, StackAction.GUARD, 0, "", time);
    }

    public void PushActionSkill(int id)
    {
        PushAction(id, StackAction.SKILL);
    }

    public void PushActionBlock(int id, int status)
    {
        PushAction(id, StackAction.BLOCK, 0, "", status);
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

    public void PushActionWait(int id, float wait)
    {
        PushAction(id, StackAction.Wait, wait);
    }

    public void PushActionFaceTo(int id, int target)
    {
        PushAction(id, StackAction.FaceTo, 0, "", target);
    }

    public void PushActionKill(int id, int target)
    {
        PushAction(id, StackAction.Kill, 0, "", target);
    }
    public void StopAction(int id)
    {
        if (UnitActKey.Contains(id))
            UnitActKey.Remove(id);
        if (UnitActionStack.ContainsKey(id))
            UnitActionStack.Remove(id);
    }

    public List<GameObject> wayPointList = new List<GameObject>();
    public void ShowWayPoint(bool on)
    {
        if (!on)
        {
            for (int i = 0; i < wayPointList.Count; i++)
            {
                 WsGlobal.RemoveLine(wayPointList[i]);
            }
            wayPointList.Clear();
        }

        if (on)
        {
            for (int i = 0; i < Global.GLevelItem.wayPoint.Count; i++)
            {
                GameObject obj = WsGlobal.AddDebugLine(Global.GLevelItem.wayPoint[i].pos - 2 * Vector3.up, Global.GLevelItem.wayPoint[i].pos + 2 * Vector3.up, Color.red, "WayPoint" + i);
                wayPointList.Add(obj);
                CapsuleCollider capsule = obj.AddComponent<CapsuleCollider>();
                capsule.isTrigger = true;
                capsule.radius = Global.GLevelItem.wayPoint[i].size;
                capsule.height = 200.0f;
            }
        }
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
    Wait = 7,
    Follow = 8,
    Patrol = 9,
    FaceTo = 10,
    Kill = 11,
}

public class ActionConfig
{
    public List<ActionItem> action = new List<ActionItem>();
    public int id;
    public void Update(float time)
    {
        if (action.Count != 0)
        {
            if (action[action.Count - 1].type == StackAction.PAUSE)
            {
                action[action.Count - 1].pause_time -= time;
                if (action[action.Count - 1].pause_time <= 0.0f)
                    action.RemoveAt(action.Count - 1);
            }
            else if (action[action.Count - 1].type == StackAction.SAY)
            {
                MeteorUnit unit = U3D.GetUnit(id);//, action[action.Count - 1])
                if (FightWnd.Exist)
                    FightWnd.Instance.InsertFightMessage(unit.name + " : " + action[action.Count - 1].text);
                action.RemoveAt(action.Count - 1);
            }
            else if (action[action.Count - 1].type == StackAction.SKILL)
            {
                MeteorUnit unit = U3D.GetUnit(id);
                unit.PlaySkill();
                action.RemoveAt(action.Count - 1);
            }
            else if (action[action.Count - 1].type == StackAction.CROUCH)
            {
                MeteorUnit unit = U3D.GetUnit(id);
                if (action[action.Count - 1].param == 1)
                {
                    unit.controller.Input.OnKeyDown(EKeyList.KL_Crouch, true);
                }
                else
                    unit.controller.Input.OnKeyUp(EKeyList.KL_Crouch);
                action.RemoveAt(action.Count - 1);
            }
            else if (action[action.Count - 1].type == StackAction.BLOCK)
            {
                MeteorUnit unit = U3D.GetUnit(id);
                unit.controller.LockInput(action[action.Count - 1].param == 1);
                unit.posMng.OnChangeAction(0);
                action.RemoveAt(action.Count - 1);
            }
            else if (action[action.Count - 1].type == StackAction.GUARD)
            {
                MeteorUnit unit = U3D.GetUnit(id);
                unit.Guard(action[action.Count - 1].param);
                action.RemoveAt(action.Count - 1);
            }
            else if (action[action.Count - 1].type == StackAction.Wait)
            {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null && unit.robot != null)
                    unit.robot.ChangeState(EAIStatus.Wait, action[action.Count - 1].param);
                action.RemoveAt(action.Count - 1);
            }
            else if (action[action.Count - 1].type == StackAction.Follow)
            {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null && unit.robot != null)
                    unit.robot.FollowTarget(action[action.Count - 1].param);
                action.RemoveAt(action.Count - 1);
            }
            else if (action[action.Count - 1].type == StackAction.Patrol)
            {
                MeteorUnit unit = U3D.GetUnit(id);
                if (unit != null && unit.robot != null)
                {
                    unit.robot.SetPatrolPath(action[action.Count - 1].Path);
                    unit.robot.ChangeState(EAIStatus.Patrol, float.MaxValue);
                }
                action.RemoveAt(action.Count - 1);
            }
            else if (action[action.Count - 1].type == StackAction.FaceTo)
            {
                MeteorUnit unit = U3D.GetUnit(id);
                MeteorUnit target = U3D.GetUnit(action[action.Count - 1].param);
                if (unit != null && target != null)
                    unit.FaceToTarget(target);
                action.RemoveAt(action.Count - 1);
            }
            else if (action[action.Count - 1].type == StackAction.Kill)
            {
                MeteorUnit unit = U3D.GetUnit(id);
                MeteorUnit target = U3D.GetUnit(action[action.Count - 1].param);
                if (unit != null && target != null)
                    unit.KillPlayer(target);
                action.RemoveAt(action.Count - 1);
            }
        }
    }
}
public class ActionItem
{
    public StackAction type;
    public float pause_time;
    public string text;
    public int param;
    public List<int> Path;//for patrol
}