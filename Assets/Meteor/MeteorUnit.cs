using UnityEngine;
using System.Collections;
using CoClass;
using System.Collections.Generic;
using System;

public enum EBUFF_ID
{ 
    Drug = 10,//钝筋
    DrugEx = 13,//错骨，
}

public enum EBUFF_Type
{
    MaxHP = 1,//最大气血+
    HP,//现有气血
    ATT,
    DEF,
    SPEED,
    ANG,
    HIDE = 10,
}

public class BuffMng:Singleton<BuffMng>
{
    public SortedDictionary<int, Buff> BufDict = new SortedDictionary<int, Buff>();
    public void Clear()
    {
        BufDict.Clear();
    }

    //取得某对象是否拥有某BUFF
    public bool HasBuff(int id, MeteorUnit unit)
    {
        foreach (var each in BufDict)
        {
            if (each.Value.Units.ContainsKey(unit) && each.Value.Id == id)
                return true;
        }
        return false;
    }

    public bool HasBuff(EBUFF_Type type, MeteorUnit unit)
    {
        foreach (var each in BufDict)
        {
            if (each.Value.Units.ContainsKey(unit) && each.Value.type == type)
                return true;
        }
        return false;
    }

    //某个角色删除全部BUFF
    public void RemoveUnit(MeteorUnit unit)
    {
        foreach (var each in BufDict)
        {
            if (each.Value.Units.ContainsKey(unit))
            {
                each.Value.ClearBuff(unit);
                each.Value.Units.Remove(unit);
            }
        }
    }
}

public class BuffContainer
{
    public float refresh_tick;
    public float refresh_round_tick;
    public SFXEffectPlay effect;
}

public class Buff
{
    public bool Finished { get { return false; } }
    public EBUFF_Type type;
    public int Id;//12是蛊毒，
    public string Iden;
    public int value;//单次增加数
    public int refresh_type;//刷新方式 <=0都刷一次, 1 按设置时间刷新, 99999 每0.1秒刷新1次
    public int refresh_delay;//刷新间隔,多久计算一次
    public int effectIdx;//角色一直带特效.
    public int last_time;//BUFF持续时间
    public Dictionary<MeteorUnit, BuffContainer> Units = new Dictionary<MeteorUnit, BuffContainer>();
    public void AddUnit(MeteorUnit unit)
    {
        if (!Units.ContainsKey(unit))
        {
            BuffContainer con = new BuffContainer();
            con.effect = effectIdx == 0 ? null : SFXLoader.Instance.PlayEffect(effectIdx, unit.gameObject, false);
            Units.Add(unit, con);
            DoBuff(unit);
        }
        else
        {
            //重复一次BUFF。要看这个BUFF是否可叠加,
            //生命上限，无法叠加
            //生命值，无法叠加
            //
        }

        Units[unit].refresh_tick = last_time / 10;
        Units[unit].refresh_round_tick = refresh_delay;
        if (refresh_type == 99999)//按固定时间0.1秒刷新.
            Units[unit].refresh_round_tick = (refresh_delay == 0 ? 0.1f : refresh_delay);//没有值就每0.1S刷新，否则就以值为刷新

        if (unit.Attr.IsPlayer)
        {
            //FightWnd.Instance.AddBuff(this);
            FightWnd.Instance.UpdatePlayerInfo();
        }
    }

    public void Clear()
    {
        foreach (var each in Units)
        {
            if (each.Value != null)
                ClearBuff(each.Key);
        }
        Units.Clear();
    }

    //清除一个对象的某个BUFF，除了状态值都不需要处理,但是要把特效清理掉
    public void ClearBuff(MeteorUnit unit)
    {
        if (!Units.ContainsKey(unit))
            return;
        if (Units[unit].effect != null)
        {
            Units[unit].effect.OnPlayAbort();
            Units[unit].effect = null;
        }
        //战斗UI把BUFF元素清除掉
        //if (unit.Attr.IsPlayer)
        //    FightWnd.Instance.RemoveBuff(this);
        //else
        //    FightWnd.Instance.RemoveBuff(this, unit);
        if (unit.Dead)
            return;
        switch (type)
        {
            case EBUFF_Type.MaxHP:
                unit.Attr.AddMaxHP(-value);
                break;
            case EBUFF_Type.HP:
                //unit.Attr.AddHP(value / 10);//无需处理
                break;
            case EBUFF_Type.ANG:
            //    unit.AddAngry(value);
                break;
            case EBUFF_Type.ATT:
                unit.Attr.AddDamage(-value);
                break;
            case EBUFF_Type.DEF:
                unit.Attr.AddDefence(-value);
                break;
            case EBUFF_Type.HIDE:
                unit.ChangeHide(value == 0);
                break;
            case EBUFF_Type.SPEED:
                unit.Attr.MultiplySpeed(100.0f / (float)value);
                break;
        }
    }

    void DoBuff(MeteorUnit unit)
    {
        //无论如何都要执行一次BUFF
        switch (type)
        {
            //要看同BUFF，可否叠加，比如最大气血值
            case EBUFF_Type.MaxHP:
                unit.Attr.AddMaxHP(value);
                break;
            case EBUFF_Type.HP:
                if (value < 0)
                    unit.OnBuffDamage(-value);
                else
                    unit.Attr.AddHP(value);
                break;
            case EBUFF_Type.ANG:
                unit.AddAngry(value);
                break;
            case EBUFF_Type.ATT:
                unit.Attr.AddDamage(value);
                break;
            case EBUFF_Type.DEF:
                unit.Attr.AddDefence(value);
                break;
            case EBUFF_Type.HIDE:
                unit.ChangeHide(value == 1);
                break;
            case EBUFF_Type.SPEED:
                unit.Attr.MultiplySpeed((float)value / 100.0f);
                break;
        }
    }

    List<MeteorUnit> unitRemoved = new List<MeteorUnit>();
    public void LockUpdate()
    {
        unitRemoved.Clear();
        switch (refresh_type)
        {
            case 1:
            case 99999://间隔多久刷一次，整体时间到了，删除对象
                foreach (var each in Units)
                {
                    each.Value.refresh_tick -= FrameReplay.deltaTime;
                    if (each.Value.refresh_tick <= 0.0f)
                    {
                        unitRemoved.Add(each.Key);//整体时间到了，就不要算单轮了
                        continue;
                    }
                    each.Value.refresh_round_tick -= FrameReplay.deltaTime;
                    if (each.Value.refresh_round_tick <= 0.0f)
                    {
                        each.Value.refresh_round_tick = refresh_delay;
                        DoBuff(each.Key);
                        if (each.Key.Attr.Dead)
                        {
                            unitRemoved.Add(each.Key);
                            continue;
                        }
                    }
                    if (each.Key.Attr.IsPlayer)
                        FightWnd.Instance.UpdatePlayerInfo();
                    else if (!each.Key.SameCamp(MeteorManager.Instance.LocalPlayer) && GameData.Instance.gameStatus.ShowBlood)
                        FightWnd.Instance.UpdateMonsterInfo(each.Key);
                }
                break;
            case -1://状态，持续时间到了取消状态，且删除对象
                foreach (var each in Units)
                {
                    each.Value.refresh_tick -= FrameReplay.deltaTime;
                    if (each.Value.refresh_tick <= 0.0f)
                        unitRemoved.Add(each.Key);
                }
                break;
            case 0://不可能走这里，0代表不是BUFF
                break;
        }
        //把超时的BUFF都删除掉
        for (int i = 0; i < unitRemoved.Count; i++)
        {
            ClearBuff(unitRemoved[i]);
            Units.Remove(unitRemoved[i]);
            if (unitRemoved[i].Attr.Dead)
                unitRemoved[i].OnDead();
            //if (unitRemoved[i].Attr.IsPlayer)
            //    FightWnd.Instance.UpdatePlayerInfo();
            //else if (unitRemoved[i] == MeteorManager.Instance.LocalPlayer.GetLockedTarget())
            //    FightWnd.Instance.UpdateMonsterInfo(unitRemoved[i]);
        }
    }
}

public partial class MeteorUnit : LockBehaviour
{
    public bool Finished { get { return Dead; } }
    public virtual bool IsDebugUnit() { return false; }
    public int UnitId;
    public int InstanceId;
    public Vector3 ShadowPosition;//影子坐标-服务器
    public Quaternion ShadowRotation;//影子旋转-服务器
    public float ShadowDelta = 0.0f;
    public bool ShadowSynced = false;//是否同步完影子位置
    public bool ShadowUpdate = false;//影子是否更新位置
    public Transform WeaponL;//右手骨骼
    public Transform WeaponR;
    public Transform ROOTNull;
    public Transform RootdBase;
    public UnitTopUI UnitTopUI;
    public Transform HeadBone;//头部骨骼.在自动目标存在时,头部骨骼朝向自动目标
    public EUnitCamp Camp = EUnitCamp.EUC_FRIEND;
    public PoseStatus posMng;
    public CharacterLoader charLoader;
    public MeteorController controller;
    public WeaponLoader weaponLoader;
    public int wayIndex;
    public uint OnGroundTick = 0;
    public float RebornTick = 0;//复活需要在死亡后多久间隔
    public bool WaitReborn = false;//盟主模式-等待系统复活
    public MeteorAI Robot;
    //按照角色的坐标围成一圈，每个30度 12个空位，距离50，其实应该按
    //指定的对象是否在自己视野内
    public bool Find(MeteorUnit unit)
    {
        float d = Vector3.Distance(unit.transform.position, transform.position);
        if (unit.HasBuff(EBUFF_Type.HIDE))
        {
            //隐身20码内可发现，2个角色相距较近
            if (d >= 60.0f)
                return false;
        }
        else
        {
            float v = U3D.IsSpecialWeapon(Attr.Weapon) ? Attr.View : Attr.View / 2;
            if (d >= (v + 50))//给一定距离以免不停的切换目标
                return false;
        }
        return true;
    }

    //单机关卡设置位置到路点.
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetPosition(int spawnPoint)
    {
        //禁止寻路，代表场景无路点.只能用来联机.
        if (Global.Instance.GLevelItem.DisableFindWay == 1)
        {
            //不许寻路，无寻路点的关卡，使用
            if (Global.Instance.GLevelSpawn != null && Global.Instance.GLevelSpawn.Length != 0)
                transform.position = Global.Instance.GLevelSpawn[spawnPoint >= Global.Instance.GLevelSpawn.Length ? 0 : spawnPoint];
            return;
        }
        if (spawnPoint >= Global.Instance.GLevelItem.wayPoint.Count)
            return;
        transform.position = Global.Instance.GLevelItem.wayPoint[spawnPoint].pos;
    }

    public void AIPause(bool pause, float t = 0.0f)
    {
        if (Robot != null)
            Robot.Pause(pause, t);
    }

    public System.Action<InventoryItem> OnEquipChanged;
    //public MeteorUnit NGUIJoystick_skill_TargetUnit;//技能目标
    //public List<SkillInput> SkillList = new List<SkillInput>();
    //MeteorUnit wantTarget = null;//绿色目标.只有主角拥有。
    MeteorUnit lockTarget = null;//攻击目标.主角主动攻击敌方后，没解锁前，都以这个目标作为锁定攻击目标，摄像机自动以主角和此目标，做一个自动视围盒，
    Vector3 pos2 = Vector3.zero;
    public Vector3 mPos2d { get { pos2 = transform.position; pos2.y = 0; return pos2; } }
    public Vector3 mPos { get { return transform.position; } }
    public Vector3 mSkeletonPivot { get { return transform.position + Global.Instance.BodyHeight; } }
    public bool Crouching { get { return posMng.mActiveAction.Idx == CommonAction.Crouch || (posMng.mActiveAction.Idx >= CommonAction.CrouchForw && posMng.mActiveAction.Idx <= CommonAction.CrouchBack); } }
    public bool Climbing { get { return posMng.mActiveAction.Idx == CommonAction.ClimbLeft || posMng.mActiveAction.Idx == CommonAction.ClimbRight || posMng.mActiveAction.Idx == CommonAction.ClimbUp; } }
    public bool ClimbJumping { get { return posMng.mActiveAction.Idx == CommonAction.WallRightJump || posMng.mActiveAction.Idx == CommonAction.WallLeftJump; } }
    //int mCacheLayerMask;
    public float ClimbingTime;//限制爬墙Y轴速度持续
    public bool Dead = false;
    public bool OnTopGround = false;//顶部顶着了,无法向上继续
    public bool OnGround = false;//控制器是否收到阻隔无法前进.
    public bool MoveOnGroundEx = false;//移动的瞬间，射线是否与地相聚不到4M。下坡的时候很容易离开地面
    public bool OnTouchWall = false;//贴着墙壁
    //public bool IsShow = true;
    float MoveSpeedScale = 1.0f;
    float ActionSpeedScale = 1.0f;

    public void SpeedFast()
    {
        MoveSpeedScale = Mathf.MoveTowards(MoveSpeedScale, SpeedMax, 0.1f);
        ActionSpeedScale = Mathf.MoveTowards(ActionSpeedScale, SpeedMax, 0.1f);
    }

    public void SpeedSlow()
    {
        MoveSpeedScale = Mathf.MoveTowards(MoveSpeedScale, SpeedMin, 0.1f);
        ActionSpeedScale = Mathf.MoveTowards(ActionSpeedScale, SpeedMin, 0.1f);
    }

    public static float SpeedMax = 16.0f;
    public static float SpeedMin = 0.01f;


    public float ActionSpeed { get { return ActionSpeedScale; } }
    //MoveSpeed
    public float GetMoveSpeedScale()
    {
        return (CalcSpeed() / 100.0f) * MoveSpeedScale;
    }

    public int MoveSpeed { get { return (int)(Attr.Speed * GetMoveSpeedScale()); } }
    public int AngryValue
    {
        get
        {
            return Attr.AngryValue;
        }
        set
        {
            Attr.AngryValue = Mathf.Clamp(value, 0, 100);
            if (Attr.IsPlayer)
                FightWnd.Instance.UpdateAngryBar();
        }
    }
    //当前武器
    public int GetWeaponSubType() { return weaponLoader == null ? 0 : weaponLoader.WeaponSubType(); }
    public int GetWeaponType() { return weaponLoader == null ? -1 : weaponLoader.WeaponType(); }
    public int GetGuardPose(int direction)
    {
        switch ((EquipWeaponType)GetWeaponType())
        {
            case EquipWeaponType.Knife:
                if (direction == 0)//左侧
                    return 56;
                else if (direction == 1)//背后
                    return 85;
                else if (direction == 2)//右侧
                    return 58;
                else if (direction == 3)
                    return 59;
                break;
            case EquipWeaponType.Sword:
                if (direction == 0)
                    return 60;
                else if (direction == 1)
                    return 61;
                else if (direction == 2)
                    return 62;
                else if (direction == 3)
                    return 63;
                break;
            case EquipWeaponType.Blade:
                if (direction == 0)
                    return 68;
                else if (direction == 1)
                    return 82;
                else if (direction == 2)
                    return 70;
                else if (direction == 3)
                    return 71;
                break;
            case EquipWeaponType.Lance:
                if (direction == 0)
                    return 64;
                else if (direction == 1)
                    return 88;
                else if (direction == 2)
                    return 66;
                else if (direction == 3)
                    return 67;
                break;
            case EquipWeaponType.Brahchthrust:
                if (direction == 0)
                    return 52;
                else if (direction == 1)
                    return 53;
                else if (direction == 2)
                    return 54;
                else if (direction == 3)
                    return 55;
                break;
            case EquipWeaponType.Gloves:
                if (direction == 0)
                    return 490;//491防地
                else if (direction == 1)
                    return 515;
                else if (direction == 2)
                    return 492;
                else if (direction == 3)
                    return 493;
                break;
            case EquipWeaponType.Hammer:
                if (direction == 0)
                    return 72;
                else if (direction == 1)
                    return 79;
                else if (direction == 2)
                    return 74;
                else if (direction == 3)
                    return 75;
                break;
            case EquipWeaponType.NinjaSword:
                if (direction == 0)
                    return 516;
                else if (direction == 1)
                    return 477;
                else if (direction == 2)
                    return 518;
                else if (direction == 3)
                    return 519;
                break;
            case EquipWeaponType.HeavenLance:
                if (direction == 0)
                    return 494;
                else if (direction == 1)
                    return 500;
                else if (direction == 2)
                    return 496;
                else if (direction == 3)
                    return 497;
                break;
            case EquipWeaponType.Gun: Debug.LogError("Gun can't guard"); break;
            case EquipWeaponType.Dart: Debug.LogError("Dart can't guard"); break;
            case EquipWeaponType.Guillotines: Debug.LogError("Guillotines can't guard"); break;
        }
        return 0;
    }

    //public Dictionary<MeteorUnit, int> hurtRecord = new Dictionary<MeteorUnit, int>();//所有存活着攻击我的伤害记录.死亡的去掉.
    public MeteorUnit GetLockedTarget()
    {
        return lockTarget;
    }

    public MeteorUnit GetKillTarget()
    {
        return killTarget;
    }

    public void SetLockedTarget(MeteorUnit target)
    {
        lockTarget = target;
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
        if (UnitTopUI != null)
            GameObject.DestroyImmediate(UnitTopUI.gameObject);
        if (weaponLoader != null)
            weaponLoader.RemoveTrail();
    }

    public bool ExistDamage(MeteorUnit t)
    {
        return Damaged.ContainsKey(t);
    }

    public bool ExistDamage(SceneItemAgent t)
    {
        return Damaged2.ContainsKey(t);
    }
    public bool SameCamp(MeteorUnit t)
    {
        if (t.Camp == EUnitCamp.EUC_KILLALL)
            return false;
        if (t.Camp == EUnitCamp.EUC_NONE)
            return true;
        if (Camp == EUnitCamp.EUC_KILLALL)
            return false;
        if (Camp == EUnitCamp.EUC_NONE)
            return true;
        return t.Camp == Camp;
    }

    public void Guard(bool guard)
    {
        if (Robot != null)
            Robot.ChangeState(guard ? EAIStatus.Guard: EAIStatus.Wait);
    }

    //初始化的时候，设置默认选择友军是谁，所有治疗技能，增益BUFF均默认释放给他
    void SelectFriend()
    {
        float dis = Attr.View;
        if (dis <= 10)
            dis = 100.0f;
        int index = -1;
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            MeteorUnit unit = MeteorManager.Instance.UnitInfos[i];
            if (unit == this)
                continue;
            if (!SameCamp(unit))
                continue;
            if (unit.Dead)
                continue;
            float d = Vector3.Distance(transform.position, MeteorManager.Instance.UnitInfos[i].transform.position);
            if (dis > d)
            {
                dis = d;
                index = i;
            }
        }
    }

    //选择一个敌方目标
    public void SelectEnemy()
    {
        lockTarget = null;
        float dis = Attr.View / 2;//视野，可能指的是直径，这里变为半径
        if (U3D.IsSpecialWeapon(Attr.Weapon))
            dis = Attr.View;//远程武器，视野距离翻倍.
        int index = -1;
        MeteorUnit tar = null;
        Collider[] other = Physics.OverlapSphere(transform.position, dis, 1 << LayerMask.NameToLayer("Monster") | 1 << LayerMask.NameToLayer("LocalPlayer"));

        //直接遍历算了
        for (int i = 0; i < other.Length; i++)
        {
            MeteorUnit unit = other[i].GetComponent<MeteorUnit>();
            if (unit == null)
                continue;
            if (unit == this)
                continue;
            if (SameCamp(unit))
                continue;
            if (unit.Dead)
                continue;
            float d = Vector3.Distance(transform.position, unit.transform.position);
            //隐身只能在10M内发现目标
            if (HasBuff(EBUFF_Type.HIDE))
            {
                if (d > 60.0f)
                    continue;
            }

            if (dis > d)
            {
                dis = d;
                index = i;
                tar = unit;
            }
        }
        if (index >= 0 && index < other.Length && tar != null)
            lockTarget = tar;
    }

    public bool HasBuff(int id)
    {
        return BuffMng.Instance.HasBuff(id, this);

    }
    //查看角色是否拥有某个类型的BUF
    public bool HasBuff(EBUFF_Type type)
    {
        return BuffMng.Instance.HasBuff(type, this);
    }

    bool canControlOnAir;//是跳跃到空中落下，还是被击飞到空中落下.击飞落下时是不能控制出招的.
    public bool CanActionOnAir()
    {
        return canControlOnAir;
    }

    //是否精确在地面，由控制器返回在地面标识
    public bool IsOnGroundEx()
    {
        bool ret = OnGround;
        return posMng.Jump ? false : ret;
    }

    public bool IsOnGround()
    {
        bool ret = OnGround || MoveOnGroundEx;
        return posMng.Jump ? false : ret;
    }

    public void SetGround(bool onground)
    {
        OnGround = onground;
        MoveOnGroundEx = onground;
    }


    public MonsterEx Attr;
    protected new void Awake()
    {
        base.Awake();
        //单场景启动.
#if !STRIP_DBG_SETTING
        if (!IsDebugUnit())
            WSDebug.Ins.AddDebuggableObject(this);
#endif
    }


    // Update is called once per frame
    List<MeteorUnit> keyM = new List<MeteorUnit>();
    List<MeteorUnit> removedM = new List<MeteorUnit>();
    List<SceneItemAgent> keyS = new List<SceneItemAgent>();
    List<SceneItemAgent> removedS = new List<SceneItemAgent>();
    List<SceneItemAgent> keyD = new List<SceneItemAgent>();
    List<SceneItemAgent> removedD = new List<SceneItemAgent>();
    List<SceneItemAgent> keyT = new List<SceneItemAgent>();
    List<SceneItemAgent> removedT = new List<SceneItemAgent>();
    public float xRotateDelta = 0;
    public float yRotateDelta = 0;
    protected override void LockUpdate()
    {
        if (!gameObject.activeInHierarchy)
            return;
        if (Climbing)
            ClimbingTime += FrameReplay.deltaTime;
        else
            ClimbingTime = 0;
        if (posMng.Jump)
            posMng.JumpTick += FrameReplay.deltaTime;
        keyM.Clear();
        keyM.AddRange(Damaged.Keys);
        removedM.Clear();
        foreach (var each in keyM)
        {
            Damaged[each] -= FrameReplay.deltaTime;
            //Debug.LogError("time:" + Time.deltaTime);
            if (Damaged[each] < 0.0f)
                removedM.Add(each);
        }
        for (int i = 0; i < removedM.Count; i++)
            Damaged.Remove(removedM[i]);
        keyS.Clear();
        keyS.AddRange(Damaged2.Keys);
        removedS.Clear();
        foreach (var each in keyS)
        {
            Damaged2[each] -= FrameReplay.deltaTime;
            if (Damaged2[each] < 0.0f)
                removedS.Add(each);
        }

        for (int i = 0; i < removedS.Count; i++)
            Damaged2.Remove(removedS[i]);

        //机关或者尖刺打到我.
        keyD.Clear();
        keyD.AddRange(attackDelay.Keys);
        removedD.Clear();
        foreach (var each in keyD)
        {
            attackDelay[each] -= FrameReplay.deltaTime;
            if (attackDelay[each] < 0.0f)
                removedD.Add(each);
        }

        for (int i = 0; i < removedD.Count; i++)
            attackDelay.Remove(removedD[i]);

        keyT.Clear();
        keyT.AddRange(touchDelay.Keys);
        removedT.Clear();
        foreach (var each in keyT)
        {
            touchDelay[each] -= FrameReplay.deltaTime;
            if (touchDelay[each] < 0.0f)
                removedT.Add(each);
        }

        for (int i = 0; i < removedT.Count; i++)
            touchDelay.Remove(removedT[i]);

        //动画帧和位置
        charLoader.LockUpdate();
        //控制者更新
        controller.LockUpdate();

        if (!Global.Instance.PauseAll)
        {
            if (Robot != null && gameObject.activeInHierarchy)
                Robot.Update();
        }

        if (Robot != null)
            RefreshTarget();

        ProcessGravity();

        //除了受击，防御，其他动作在有锁定目标下，都要转向锁定目标.
        if (GetLockedTarget() != null && posMng.mActiveAction.Idx == CommonAction.Run)
        {
            if (Robot == null && GameData.Instance.gameStatus.AutoLock)
            {
                if (GetWeaponType() != (int)EquipWeaponType.Guillotines &&
                    GetWeaponType() != (int)EquipWeaponType.Gun &&
                    GetWeaponType() != (int)EquipWeaponType.Dart)
                {
                    MeteorUnit target = GetLockedTarget();
                    float dis = Vector2.Distance(target.mPos2d, this.mPos2d);
                    //小于一定距离后，不再改变朝向.否则一直抖动
                    if (dis > 20)
                        FaceToTarget(GetLockedTarget());//抖动是因为招式忽略了角色间的碰撞,导致的离的太近
                }
            }
        }

        if (Attr.IsPlayer)
        {
            float yRotate = 0;
            if (posMng.CanRotateY)
            {
#if STRIP_KEYBOARD
                //Debug.LogError(string.Format("deltaLast.x:{0}", NGUICameraJoystick.instance.deltaLast.x));
                yRotate = NGUICameraJoystick.instance.deltaLast.x * GameData.Instance.gameStatus.AxisSensitivity.x;
#else
                yRotate = Input.GetAxis("Mouse X") * 5;
#endif
            }
            if (yRotate != 0)
                MeteorManager.Instance.LocalPlayer.SetOrientation(yRotate, false);

            float xRotate = 0;
#if STRIP_KEYBOARD
            xRotate = NGUICameraJoystick.instance.deltaLast.y * GameData.Instance.gameStatus.AxisSensitivity.y;
#else
            xRotate = Input.GetAxis("Mouse Y") * 2;
#endif
            //把Mouse的移动事件/触屏的拖拽事件发送到
            FSS.Instance.PushMouseDelta(InstanceId, xRotate, yRotate);
        }
        xRotateDelta = 0;
        yRotateDelta = 0;
        ProcessCommand();
    }

    //解析角色的真动作.
    protected void ProcessCommand()
    {
        var command = FSC.Instance.GetCommand(FrameReplay.Instance.LogicFrameIndex);
        for (int i = 0; i < command.Count; i++)
        {
            //取得该角色得操作指令，并且执行.
            switch (command[i].command)
            {
                //case protocol.MeteorMsg.Command.MouseDelta:
                //    OnPlayerMouseDelta(command.data5, command.data6);
                //    break;
            }
        }
    }

    protected void OnPlayerMouseDelta(float x, float y)
    {
        if (y != 0)
            MeteorManager.Instance.LocalPlayer.SetOrientation(y, false);
        //存储上一帧鼠标或者触屏的偏移.
        xRotateDelta = x;
        yRotateDelta = y;
    }

    //刷新敌人，视野内的可拾取道具
    public SceneItemAgent GetSceneItemTarget()
    {
        return TargetItem;
    }

    void SelectSceneItem()
    {
        float dis = Attr.View / 2;//视野，可能指的是直径，这里变为半径
        //Collider[] other = Physics.OverlapSphere(transform.position, dis, LayerMask.NameToLayer("Trigger"));
        int index = -1;
        SceneItemAgent tar = null;
        //直接遍历算了
        for (int i = 0; i < MeteorManager.Instance.SceneItems.Count; i++)
        {
            SceneItemAgent item = MeteorManager.Instance.SceneItems[i].gameObject.GetComponent<SceneItemAgent>();
            if (item == null)
                continue;
            if (!item.CanPickup())
                continue;
            float d = Vector3.Distance(transform.position, item.transform.position);
            if (dis > d)
            {
                dis = d;
                index = i;
                tar = item;
            }
        }
        if (index >= 0 && index < MeteorManager.Instance.SceneItems.Count && tar != null)
            TargetItem = tar;
    }

    SceneItemAgent TargetItem;
    void RefreshTarget()
    {
        MeteorUnit temp = lockTarget;
        if (lockTarget == null || lockTarget.Dead)
            SelectEnemy();
        else
        {
            //死亡，失去视野，超出视力范围，重新选择
            if (Robot.killTarget == lockTarget)
            {
                //强制杀死还存活的角色时，不会丢失目标.
            }
            else
            {
                float d = Vector3.Distance(lockTarget.transform.position, transform.position);
                if (lockTarget.HasBuff(EBUFF_Type.HIDE))
                {
                    //隐身20码内可发现，2个角色紧贴着
                    if (d >= 60.0f)
                    {
                        lockTarget = null;
                    }
                }
                else
                {
                    float v = U3D.IsSpecialWeapon(Attr.Weapon) ? Attr.View : Attr.View / 2;
                    if (d >= (v + 50))//给一定距离以免不停的切换目标
                        lockTarget = null;
                }
            }
        }

        if (TargetItem == null)
            SelectSceneItem();
        else
        {
            if (!TargetItem.CanPickup())
            {
                TargetItem = null;
            }
            else
            {
                float d = Vector3.Distance(TargetItem.transform.position, transform.position);
                if (d > Attr.View / 2 + 50)
                    TargetItem = null;
            }
        }
    }

    public const float Jump2Velocity = 160;//蹬腿反弹速度
    public const float JumpVelocityForward = 180.0f;//向前跳跃速度
    public const float JumpVelocityOther = 100.0f;//其他方向上的速度
    
    //public const float gGravity = 980.0f;//971.4f;//向上0.55秒，向下0.45秒
    public const float groundFriction = 3000.0f;//地面摩擦力，在地面不是瞬间停止下来的。
    public const float yLimitMin = -700f;//最大向下速度
    public const float yLimitMax = 700;//最大向上速度
    public const float yClimbLimitMax = 180.0f;
    public const float yClimbEndLimit = -30.0f;//爬墙时,Y速度到达此速度，开始计时，时间到就从墙壁落下
    public const float JumpLimit = 68f;
    //角色跳跃高度74，是以脚趾算最低点，倒过来算出dbase,则需要减去差值。
    public bool IgnoreGravity = false;
    //物体动量(质量*速度)的改变,等于物体所受外力冲量的总和.这就是动量定理
    public Vector3 ImpluseVec = Vector3.zero;//冲量，ft = mat = mv2 - mv1,冲量在时间内让物体动量由A变化为B

    //增加一点速度，让其爬墙，或者持续跳
    public void AddYVelocity(float y)
    {
        //return;
        ImpluseVec.y += y;
        if (ImpluseVec.y > yClimbLimitMax)
            ImpluseVec.y = yClimbLimitMax;
    }

    public void ResetYVelocity()
    {
        ImpluseVec.y = 0;
    }

    public void IgnoreGravitys(bool ignore)
    {
        IgnoreGravity = ignore;
    }

    //处理地面摩擦力, scale地面摩擦力倍数，空中时摩擦力为0.2倍
    void ProcessFriction(float scale = 1.0f)
    {
        //return;
        if (ImpluseVec.x != 0)
        {
            if (ImpluseVec.x > 0)
            {
                ImpluseVec.x -= scale * groundFriction * FrameReplay.deltaTime;
                if (ImpluseVec.x < 0)
                    ImpluseVec.x = 0;
            }
            else
            {
                ImpluseVec.x += scale * groundFriction * FrameReplay.deltaTime;
                if (ImpluseVec.x > 0)
                    ImpluseVec.x = 0;
            }
        }
        if (ImpluseVec.z != 0)
        {
            if (ImpluseVec.z > 0)
            {
                ImpluseVec.z -= scale * groundFriction * FrameReplay.deltaTime;
                if (ImpluseVec.z < 0)
                    ImpluseVec.z = 0;
            }
            else
            {
                ImpluseVec.z += scale * groundFriction * FrameReplay.deltaTime;
                if (ImpluseVec.z > 0)
                    ImpluseVec.z = 0;
            }
        }
    }


    public virtual void ProcessGravity()
    {
        //死亡姿势播放完毕后，会取消掉角色的碰撞盒，就不要再计算角色的落下之类的了.
        if (!charController.enabled)
            return;
        //计算运动方向
        //角色forward指向人物背面
        //根据角色状态计算重力大小，在墙壁，空中，以及空中水平轴的阻力
        float gScale = Global.Instance.gGravity;
        //跳跃起身与墙壁碰撞.重力模拟为墙壁摩擦
        //if (OnTouchWall)
        //{
        //    if (ImpluseVec.y > 0)
        //        gScale = gGravity * 0.5f;
        //    else
        //        gScale = gGravity * 0.5f;
        //}
        //if (OnTopGround)
        //    ImpluseVec.y = 0;

        Vector3 v;
        v.x = ImpluseVec.x * FrameReplay.deltaTime;
        v.y = IgnoreGravity ? 0 : ImpluseVec.y * FrameReplay.deltaTime;
        v.z = ImpluseVec.z * FrameReplay.deltaTime;
        v += charLoader.moveDelta;
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            //联机避免抖动
            if (v != Vector3.zero)
                Move(v);
        }
        else
        {
            if (v != Vector3.zero)
                Move(v);
        }

        if (OnTouchWall)
            ProcessFriction(0.3f);//爬墙或者在墙面滑动，摩擦力是地面的0.2倍

        if (!IsOnGroundEx())
        {
            if (IgnoreGravity)
            {

            }
            else
            {
                ImpluseVec.y = ImpluseVec.y - gScale * FrameReplay.deltaTime;
                if (ImpluseVec.y < yLimitMin)
                    ImpluseVec.y = yLimitMin;
            }
        }
        else
        {
            if (OnGround || OnTopGround)//如果在地面，或者顶到天花板，那么应用摩擦力.
                ProcessFriction();
            else if (MoveOnGroundEx)
                ProcessFriction(0.9f);//没贴着地面，还是要有摩擦力，否则房顶滑动太厉害
        }
    }


    IEnumerator HeadLookAt()
    {
        while (true)
        {
            if (GameBattleEx.Instance != null && GameBattleEx.Instance.autoTarget != null && GetWeaponType() != (int)EquipWeaponType.Guillotines)
                HeadLookAtTarget(GameBattleEx.Instance.autoTarget.mPos);
            yield return 0;
        }
    }

    //这个转脑袋很多问题，最好事先限制每个轴的转动，否则很容易扭曲脑袋。
    //没有角速度
    public void HeadLookAtTarget(Vector3 pos)
    {
        if (pos == Vector3.zero)
            return;
        pos.y = transform.position.y + 35;
        HeadBone.LookAt(pos);
        HeadBone.localEulerAngles = new Vector3(HeadBone.localEulerAngles.x, HeadBone.localEulerAngles.y, HeadBone.localEulerAngles.z + 90);
    }

    public bool CanBurst()
    {
        return (AngryValue >= Global.ANGRYBURST);
    }

    //专门用来播放左转，右转动画的，直接面对角色不要调用这个。
    public void SetOrientation(float orient, bool ai = true)
    {
        if (ai && posMng != null && posMng.onhurt)
            return;
        Quaternion quat = Quaternion.identity;
        try
        {
            quat = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + orient, transform.eulerAngles.z);
        }
        catch (Exception exp)
        {
            Debug.LogError(exp.Message);
            Debug.DebugBreak();
        }
        transform.rotation = quat;
        if (controller.Input.OnInputMoving())
        {
            //if (!Attr.IsPlayer)
            //    Debug.Log("SetOrientation returned");
            return;
        }
        OnCameraRotateStart();
        if (GetWeaponType() == (int)EquipWeaponType.Gun)
        {
            if (orient < 0.0f)
            {
                if (Crouching || posMng.mActiveAction.Idx == CommonAction.GunIdle)
                {
                    if (posMng.mActiveAction.Idx == CommonAction.Crouch || posMng.mActiveAction.Idx == CommonAction.GunIdle
                    || (posMng.mActiveAction.Idx == CommonAction.CrouchRight && controller.Input.mInputVector.x == 0))
                        posMng.ChangeAction(CommonAction.CrouchLeft, 0.1f);
                }
                else
                if (posMng.mActiveAction.Idx < CommonAction.Crouch
                    || (posMng.mActiveAction.Idx == CommonAction.WalkRight && controller.Input.mInputVector.x == 0))
                    posMng.ChangeAction(CommonAction.WalkLeft, 0.1f);
            }
            else
            if (orient > 0.0f)
            {
                if (Crouching || posMng.mActiveAction.Idx == CommonAction.GunIdle)
                {
                    if (posMng.mActiveAction.Idx == CommonAction.Crouch || posMng.mActiveAction.Idx == CommonAction.GunIdle
                    || (posMng.mActiveAction.Idx == CommonAction.CrouchLeft && controller.Input.mInputVector.x == 0))
                        posMng.ChangeAction(CommonAction.CrouchRight, 0.1f);
                }
                else
                if (posMng.mActiveAction.Idx < CommonAction.Crouch
                    || (posMng.mActiveAction.Idx == CommonAction.WalkLeft && controller.Input.mInputVector.x == 0))
                    posMng.ChangeAction(CommonAction.WalkRight, 0.1f);
            }
        }
        else
        {
            if (orient < 0.0f)
            {
                if (Crouching)
                {
                    if (posMng.mActiveAction.Idx == CommonAction.Crouch
                    || (posMng.mActiveAction.Idx == CommonAction.CrouchRight && controller.Input.mInputVector.x == 0))
                        posMng.ChangeAction(CommonAction.CrouchLeft, 0.1f);
                }
                else
                if (posMng.mActiveAction.Idx < CommonAction.Crouch
                    || (posMng.mActiveAction.Idx == CommonAction.WalkRight && controller.Input.mInputVector.x == 0))
                    posMng.ChangeAction(CommonAction.WalkLeft, 0.1f);
            }
            else
            if (orient > 0.0f)
            {
                if (Crouching)
                {
                    if (posMng.mActiveAction.Idx == CommonAction.Crouch
                    || (posMng.mActiveAction.Idx == CommonAction.CrouchLeft && controller.Input.mInputVector.x == 0))
                        posMng.ChangeAction(CommonAction.CrouchRight, 0.1f);
                }
                else
                if (posMng.mActiveAction.Idx < CommonAction.Crouch
                    || (posMng.mActiveAction.Idx == CommonAction.WalkLeft && controller.Input.mInputVector.x == 0))
                    posMng.ChangeAction(CommonAction.WalkRight, 0.1f);
            }
        }
    }

    //角色是否面向指定位置.
    public bool IsFacetoVector3(Vector3 target)
    {
        return Vector3.Dot(-transform.forward, (new Vector3(target.x, 0, target.z) - new Vector3(mPos.x, 0, mPos.z)).normalized) >= 0;
    }

    public bool IsFacetoTarget(MeteorUnit target)
    {
        return Vector3.Dot(-transform.forward, (new Vector3(target.mPos.x, 0, target.mPos.z) - new Vector3(mPos.x, 0, mPos.z)).normalized) >= 0;
    }

    //战斗场景,控制视角相机
    public void SetOrientation(Quaternion quatDiff, Quaternion orig)
    {
        transform.rotation = quatDiff * orig;
        float orient = quatDiff.eulerAngles.y * Mathf.Deg2Rad;
        if (orient <= 0.0f)
        {
            if (posMng.mActiveAction.Idx == CommonAction.Idle || 
                posMng.mActiveAction.Idx == CommonAction.WalkRight)
                posMng.ChangeAction(CommonAction.WalkLeft);
        }
        else
        if (orient > 0.0f)
        {
            if (posMng.mActiveAction.Idx == CommonAction.Idle ||
                posMng.mActiveAction.Idx == CommonAction.WalkLeft)
                posMng.ChangeAction(CommonAction.WalkRight);
        }
    }

    MeteorUnit killTarget = null;
    public void KillTarget(MeteorUnit unit)
    {
        if (Robot != null)
        {
            killTarget = unit;
            Robot.ChangeState(EAIStatus.Kill);
        }
    }

    public void FaceToTarget(Vector3 target)
    {
        Vector3 vdiff = transform.position - target;
        vdiff.y = 0;
        if (vdiff == Vector3.zero)
            return;
        transform.rotation = Quaternion.LookRotation(new Vector3(vdiff.x, 0, vdiff.z), Vector3.up);
        if (this == MeteorManager.Instance.LocalPlayer)
        {
            if (CameraFollow.Ins != null)
                CameraFollow.Ins.ForceUpdate();
        }
    }

    public void FaceToTarget(MeteorUnit unit)
    {
        if (unit == this)
            return;
        //UnityEngine.Debug.LogError(string.Format("{0} faceto :{1}", name, unit.name));
        FaceToTarget(unit.transform.position);
    }

    public CharacterController charController;
    public void Init(int modelIdx, MonsterEx mon = null, bool updateModel = false)
    {
        //wayIndex = mon == null ? 0 : mon.SpawnPoint;
        WeaponReturned(0);
        Vector3 vec = transform.position;
        Quaternion rotation = transform.rotation;

        tag = "meteorUnit";
        UnitId = modelIdx;
        Attr = mon;
        IgnoreGravity = true;
        IgnorePhysical = false;
        name = Attr.Name;
        if (Global.Instance.GLevelMode <= LevelMode.SinglePlayerTask)
            gameObject.layer = Attr.IsPlayer ? LayerMask.NameToLayer("LocalPlayer") : LayerMask.NameToLayer("Monster");
        else
            gameObject.layer = LayerMask.NameToLayer("LocalPlayer");

        //单机模式下有ai
        if (Global.Instance.GLevelMode <= LevelMode.CreateWorld)
            Robot = Attr.IsPlayer ? null : new MeteorAI(this);
        
        if (controller == null)
            controller = new MeteorController();

        if (updateModel)
        {
            //把伤害盒子去掉，把受击盒子去掉
            hitList.Clear();
            GameBattleEx.Instance.ClearDamageCollision(this);

            //切换模型把BUFF删掉
            BuffMng.Instance.RemoveUnit(this);

            if (flag)
                flagEffect.OnPlayAbort();

            if (charLoader != null)
            {
                GameObject.Destroy(charLoader.rootBone.parent.gameObject);
                GameObject.Destroy(charLoader.Skin.gameObject);
                charLoader = null;
            }
        }

        if (charLoader == null)
            charLoader = new CharacterLoader();
        if (posMng == null)
            posMng = new PoseStatus();

        if (updateModel)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
        charLoader.LoadCharactor(UnitId, transform);
        try
        {
            posMng.Init(this);
        }
        catch
        {
            Debug.LogError("unit id:" + UnitId);
        }
        WeaponR = Global.ldaControlX("d_wpnR", charLoader.rootBone.gameObject).transform;
        WeaponL = Global.ldaControlX("d_wpnL", charLoader.rootBone.gameObject).transform;
        ROOTNull = Global.ldaControlX("b", charLoader.rootBone.gameObject).transform;
        HeadBone = Global.ldaControlX("bau_Head", charLoader.rootBone.gameObject).transform;
        RootdBase = charLoader.rootBone;

        weaponLoader = gameObject.GetComponent<WeaponLoader>();
        if (updateModel)
        {
            Destroy(weaponLoader);
            weaponLoader = null;
        }
        if (weaponLoader == null)
            weaponLoader = gameObject.AddComponent<WeaponLoader>();
        weaponLoader.Init(this);

        if (Attr.IsPlayer)
        {
            //CameraFollow followCamera = GameObject.Find("CameraEx").GetComponent<CameraFollow>();
            //头部骨骼朝向自动目标对象-取消，部分情况下出现扭曲
            //StartCoroutine("HeadLookAt");
        }

        charController = gameObject.GetComponent<CharacterController>();
        if (charController == null)
            charController = gameObject.AddComponent<CharacterController>();
        charController.center = new Vector3(0, 16, 0);
        charController.height = 32;
        charController.radius = 9.0f;//不这么大碰不到寻路点.
        charController.stepOffset = 7.6f;

        posMng.ChangeAction();
        if (controller != null)
            controller.Init(this);

        UnitTopUI = (GameObject.Instantiate(Resources.Load("UnitTopUI")) as GameObject).GetComponentInChildren<UnitTopUI>();
        UnitTopUI.Init(Attr, transform, Camp);

        InventoryItem itWeapon = GameData.Instance.MakeEquip(Attr.Weapon);
        weaponLoader.EquipWeapon(itWeapon);

        //换主角模型用
        if (updateModel)
        {
            transform.position = vec;
            transform.rotation = rotation;
        }

        //换主角模型用
        if (flag)
            SetFlag(FlagItem, flagEffectIdx);

        GunReady = false;
        IsPlaySkill = false;

        //初始化角色周围的占位寻路点
        //for (int i = 0; i < 15; i++)
        //{
            //SlotInfo.Add(i, null);
            //GameObject obj = GameObject.Instantiate(ResMng.LoadPrefab("FreePos"), transform.position + Quaternion.AngleAxis(i * 24, Vector3.up) * (Vector3.forward * 40), Quaternion.identity, null) as GameObject;
            //obj.transform.localScale = new Vector3(1, 0.1f, 1);
            //SlotFreePos.Add(i, obj);
        //}
    }

    public bool IsPlaySkill { get; set; }
    public void EquipWeapon(InventoryItem it)
    {
        IndicatedWeapon = it;
        ChangeWeapon();
    }

    InventoryItem IndicatedWeapon;
    public void ChangeWeaponCode(int weaponCode)
    {
        if (weaponLoader != null)
            weaponLoader.UnEquipWeapon();

        Attr.Weapon = weaponCode;
        IndicatedWeapon = GameData.Instance.MakeEquip(Attr.Weapon);

        InventoryItem toEquip = IndicatedWeapon;
        if (toEquip != null && weaponLoader != null)
            weaponLoader.EquipWeapon(toEquip);
        if (OnEquipChanged != null)
            OnEquipChanged.Invoke(toEquip);
        IndicatedWeapon = null;
        //没有自动目标，攻击目标，不许计算自动/锁定目标，无转向
        if (Attr.IsPlayer && (GetWeaponType() == (int)EquipWeaponType.Gun || GetWeaponType() == (int)EquipWeaponType.Dart))
            GameBattleEx.Instance.Unlock();
    }

    public void ChangeWeaponPos(int pose)
    {
        if (weaponLoader != null)
            weaponLoader.ChangeWeaponPos(pose);
    }

    //移动速度/非动作速度
    public int CalcSpeed()
    {
        if (weaponLoader == null || weaponLoader.GetCurrentWeapon() == null || weaponLoader.GetCurrentWeapon().Info() == null)
            return 100;
        if (weaponLoader.GetCurrentWeapon().Info().Speed == 0)
            return 100;
        return weaponLoader.GetCurrentWeapon().Info().Speed;
    }

    public int CalcDamage()
    {
        return weaponLoader.GetCurrentWeapon().Info().Damage;
    }

    public int CalcDef()
    {
        return weaponLoader.GetCurrentWeapon().Info().Def;
    }

    public int GetNextWeaponType()
    {
        if (Attr.Weapon2 == 0)
            return -1;
        InventoryItem it = GameData.Instance.MakeEquip(Attr.Weapon2);
        if (it != null)
            return it.Info().SubType;
        return -1;
    }

    public void SyncWeapon(int weapon1, int weapon2)
    {
        if (weaponLoader == null && Attr.Weapon == weapon1)
        {
            Attr.Weapon2 = weapon2;
            return;
        }
        weaponLoader.UnEquipWeapon();
        Attr.Weapon = weapon1;
        Attr.Weapon2 = weapon2;
        IndicatedWeapon = GameData.Instance.MakeEquip(Attr.Weapon);
        if (IndicatedWeapon != null && weaponLoader != null)
            weaponLoader.EquipWeapon(IndicatedWeapon);
        IndicatedWeapon = null;
    }

    //丢掉主武器，切换到武器2
    public void DropAndChangeWeapon()
    {
        if (Attr.Weapon2 != 0)
        {
            if (weaponLoader != null)
                weaponLoader.UnEquipWeapon();
            int weapon = Attr.Weapon;
            Attr.Weapon = Attr.Weapon2;
            Attr.Weapon2 = 0;
            IndicatedWeapon = GameData.Instance.MakeEquip(Attr.Weapon);
        }
        if (IndicatedWeapon != null && weaponLoader != null)
            weaponLoader.EquipWeapon(IndicatedWeapon);
        IndicatedWeapon = null;
        //没有自动目标，攻击目标，不许计算自动/锁定目标，无转向
        if (Attr.IsPlayer &&
            (GetWeaponType() == (int)EquipWeaponType.Gun ||
             GetWeaponType() == (int)EquipWeaponType.Dart ||
             GetWeaponType() == (int)EquipWeaponType.Guillotines))
            GameBattleEx.Instance.Unlock();
        if (Robot != null)
            Robot.OnChangeWeapon();
    }

    public void ChangeNextWeapon()
    {
        //Debug.Log("ChangeNextWeapon");
        if (Attr.Weapon2 != 0)
        {
            if (weaponLoader != null)
                weaponLoader.UnEquipWeapon();
            int weapon = Attr.Weapon;
            Attr.Weapon = Attr.Weapon2;
            Attr.Weapon2 = weapon;
            IndicatedWeapon = GameData.Instance.MakeEquip(Attr.Weapon);
        }
        if (IndicatedWeapon != null && weaponLoader != null)
            weaponLoader.EquipWeapon(IndicatedWeapon);
        IndicatedWeapon = null;
        //没有自动目标，攻击目标，不许计算自动/锁定目标，无转向
        if (Attr.IsPlayer && 
            (GetWeaponType() == (int)EquipWeaponType.Gun || 
             GetWeaponType() == (int)EquipWeaponType.Dart || 
             GetWeaponType() == (int)EquipWeaponType.Guillotines))
            GameBattleEx.Instance.Unlock();
        if (Robot != null)
            Robot.OnChangeWeapon();
    }


    public void ChangeWeapon()
    {
        posMng.ChangeAction(IsOnGround() ? CommonAction.ChangeWeapon : CommonAction.AirChangeWeapon, 0.0f);
    }

    public void OnCrouch()
    {
        posMng.ChangeAction(CommonAction.Crouch, 0.1f);
    }

    public void Defence()
    {
        int weapon = GetWeaponType();
        if (weapon == (int)EquipWeaponType.Gun || weapon == (int)EquipWeaponType.Dart || weapon == (int)EquipWeaponType.Guillotines)
            return;
        posMng.ChangeAction(CommonAction.Defence, 0.1f);
    }

    //火枪是否进入预备状态，这个状态下，除了跳跃，爆气，受击之外都会继续回到预备姿势，而不进入默认的IDLE
    public bool GunReady { get; set; }
    public void SetGunReady(bool ready)
    {
        GunReady = ready;
    }


    public void DoBreakOut()
    {
        if (AngryValue >= 60 || GameData.Instance.gameStatus.EnableInfiniteAngry)
        {
            posMng.ChangeAction(CommonAction.BreakOut);
            AngryValue -= GameData.Instance.gameStatus.EnableInfiniteAngry ? 0 : 60;
            if (Attr.IsPlayer)
                FightWnd.Instance.UpdateAngryBar();
        }
    }

    //处理上升途中的跳跃键，查看周围有无可伸腿踩的点，如果有，则判断方向，切换姿势，并给一个速度
    //面向左45，面向，面向右45，查看是否
    public void ProcessJump2()
    {
        if (Climbing)
        {
            if (posMng.mActiveAction.Idx == CommonAction.ClimbUp)
            {
                //倒跳
                SetWorldVelocity(transform.forward * Jump2Velocity);
                Jump2(JumpLimit / 3, CommonAction.JumpBack);
            }
            else if (posMng.mActiveAction.Idx == CommonAction.ClimbRight)
            {
                //像右
                SetWorldVelocity(-transform.right * Jump2Velocity);
                Jump2(JumpLimit / 3, CommonAction.WallLeftJump);
            }
            else if (posMng.mActiveAction.Idx == CommonAction.ClimbLeft)
            {
                SetWorldVelocity(transform.right * Jump2Velocity);
                Jump2(JumpLimit / 3, CommonAction.WallRightJump);
            }
        }
        else
        {
            RaycastHit hit;
            Vector3 vec = Vector3.zero;
            Vector3 nearest = Vector3.zero;
            float length = 100.0f;
            int idx = -3;//0左侧，1中间，2右侧
            for (int i = -2; i < 3; i++)
            {
                if (Physics.Raycast(mPos, Quaternion.AngleAxis(i * 45, Vector3.up) * -transform.forward, out hit, charController.radius + 5, 1 << LayerMask.NameToLayer("Scene")))
                {
                    vec = mPos - hit.point;
                    vec.y = 0;
                    // = vec.magnitude;
                    if (length > vec.magnitude)
                    {
                        length = vec.magnitude;
                        nearest = vec;
                        idx = i;
                    }
                }
            }

            //Debug.LogError("idx:" + idx);
            switch (idx)
            {
                case -2:
                case -1:
                    SetWorldVelocity(Vector3.Normalize(vec) * Jump2Velocity);
                    Jump2(JumpLimit / 2, CommonAction.WallLeftJump);
                    break;
                case 0:
                    SetWorldVelocity(Vector3.Normalize(vec) * Jump2Velocity);
                    Jump2(JumpLimit / 2, CommonAction.JumpBack);
                    break;
                case 1:
                case 2:
                    SetWorldVelocity(Vector3.Normalize(vec) * Jump2Velocity);
                    Jump2(JumpLimit / 2, CommonAction.WallRightJump);
                    break;
            }
        }
    }
    //处理贴墙轻功时,给XY初速度，再给Y初速度
    public void ProcessTouchWallJump(bool minVelocity)
    {
        //Debug.LogError("hit Wall Jump");
        if (posMng.mActiveAction.Idx == CommonAction.ClimbUp)
        {
            //倒跳
            SetWorldVelocity(hitNormal * Jump2Velocity);
            Jump2(JumpLimit / 2, CommonAction.JumpBack);
        }
        else if (posMng.mActiveAction.Idx == CommonAction.ClimbRight)
        {
            //像右
            SetWorldVelocity(Quaternion.AngleAxis(-45, Vector3.up) * hitNormal * Jump2Velocity);
            Jump2(JumpLimit / 2, CommonAction.WallLeftJump);
        }
        else if (posMng.mActiveAction.Idx == CommonAction.ClimbLeft)
        {
            SetWorldVelocity(Quaternion.AngleAxis(45, Vector3.up) * hitNormal * Jump2Velocity);
            Jump2(JumpLimit / 2, CommonAction.WallRightJump);
        }
    }

    //被墙壁推开.
    public void ProcessFall(float scale = 1.0f)
    {
        //return;
        Vector3 vec = transform.position - hitPoint;
        vec.y = 0;
        //恰好碰撞点，就是自己所处的坐标,把角色向前推一定距离.
        if (vec == Vector3.zero)
            vec = -transform.forward;
        SetWorldVelocity(Vector3.Normalize(vec) * 30 * scale);
        //如果在墙壁处被推开
        //if (robot != null)
        //    robot.ChangeState(EAIStatus.PushByWall);
        //WsGlobal.AddDebugLine(hitPoint, hitPoint + Vector3.Normalize(vec) * 30, Color.red, "pushDir", 10);
        //Debug.LogError("被墙壁推开");
        //RaycastHit hit;
        //bool processed = false;
        //for (int i = 0; i < 12; i++)
        //{
        //    if (Physics.Raycast(mPos, Quaternion.AngleAxis(i * 30, Vector3.up) * -transform.forward, out hit, charController.radius + 5, 1 << LayerMask.NameToLayer("Scene")))
        //    {
        //        Vector3 vec = mPos - hit.point;
        //        if (posMng.mActiveAction.Idx == CommonAction.Run ||
        //            posMng.mActiveAction.Idx == CommonAction.RunOnDrug ||
        //            posMng.mActiveAction.Idx == CommonAction.WalkLeft ||
        //            posMng.mActiveAction.Idx == CommonAction.WalkRight ||
        //            posMng.mActiveAction.Idx == CommonAction.WalkBackward ||
        //            posMng.mActiveAction.Idx == CommonAction.ClimbRight ||
        //            posMng.mActiveAction.Idx == CommonAction.ClimbLeft ||
        //            posMng.mActiveAction.Idx == CommonAction.ClimbUp)

        //        SetWorldVelocity(Vector3.Normalize(vec) * 30);
        //        //Jump(0.01f, CommonAction.JumpFall);
        //        processed = true;
        //        break;
        //    }
        //}
        ////没有处理？？？
        //if (!processed)
        //{
        //    SetWorldVelocity(transform.forward * 30);
        //Jump(0.01f, CommonAction.JumpFall);
        //}
    }

    float floatTick = -1.0f;//浮空时刻
    float groundTick = -1.0f;//贴地面时刻
    void UpdateFlags(CollisionFlags flag)
    {
        if ((flag & CollisionFlags.Sides) != 0)
            OnTouchWall = true;
        else
            OnTouchWall = false;
        if ((flag & CollisionFlags.Above) != 0)
            OnTopGround = true;
        if (flag == CollisionFlags.None)
            OnTopGround = OnGround = OnTouchWall = false;
        if ((flag & CollisionFlags.Below) != 0)
            OnGround = true;

        if (OnGround || OnTopGround || OnTouchWall)
        {
            if (posMng.JumpTick != 0.0f)
                posMng.CanAdjust = false;
        }
        //减少射线发射次数.
        bool Floating = false;
        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out hit, 1000, 1 << LayerMask.NameToLayer("Scene") | 1 << LayerMask.NameToLayer("Trigger")))
        {
            MoveOnGroundEx = hit.distance <= 4f;
            //Debug.Log(string.Format("distance:{0}", hit.distance));
            Floating = hit.distance >= 12.0f;
        }
        else
        {
            MoveOnGroundEx = false;
            Floating = true;
        }
        if (OnGround)
        {
            //检测脚底是否踩住地面了
            //Y轴速度下降到速度超过能爬墙的速度.停止攀爬.被墙壁弹开.
            if (Climbing)
            {
                //爬墙
                if (!MoveOnGroundEx && ImpluseVec.y < yClimbEndLimit)
                {
                    posMng.ClimbFallTick += FrameReplay.deltaTime;
                    if (posMng.ClimbFallTick > PoseStatus.ClimbFallLimit)
                    {
                        //Debug.LogError("爬墙速度低于最低速度-爬墙落下");
                        posMng.ChangeAction(CommonAction.JumpFall, 0.1f);//短时间内落地姿势
                        ProcessFall();
                        posMng.ClimbFallTick = 0.0f;
                    }
                }
                else if (MoveOnGroundEx)
                {
                    //Debug.LogError("爬墙碰到地面-落到地面");
                    posMng.ChangeAction(CommonAction.JumpFall, 0.1f);//短时间内落地姿势
                }
                else
                {
                    //只要在爬行中接触到可以算做落地的位置，都弹开
                    //尝试被推开.
                    ProcessFall();
                }
            }
            else if (OnTouchWall && Floating && Time.timeSinceLevelLoad - floatTick >= 0.75f)//贴墙浮空，被墙壁推开
            {
                if (posMng.mActiveAction.Idx == CommonAction.Idle ||
                    posMng.mActiveAction.Idx == CommonAction.WalkLeft ||
                    posMng.mActiveAction.Idx == CommonAction.Run ||
                    posMng.mActiveAction.Idx == CommonAction.RunOnDrug ||
                        posMng.mActiveAction.Idx == CommonAction.WalkRight ||
                        posMng.mActiveAction.Idx == CommonAction.WalkBackward ||
                        posMng.mActiveAction.Idx == CommonAction.Jump ||
                        posMng.mActiveAction.Idx == CommonAction.JumpLeft ||
                        posMng.mActiveAction.Idx == CommonAction.JumpRight ||
                        posMng.mActiveAction.Idx == CommonAction.JumpBack ||
                        posMng.mActiveAction.Idx == CommonAction.JumpLeftFall ||
                        posMng.mActiveAction.Idx == CommonAction.JumpRightFall ||
                        posMng.mActiveAction.Idx == CommonAction.JumpBackFall ||
                        posMng.mActiveAction.Idx == CommonAction.JumpFallOnGround)
                {
                    //Debug.LogError("贴着墙壁-贴着地面-被墙壁推开");
                    ProcessFall();
                    floatTick = Time.timeSinceLevelLoad;
                }
            }
            else if (Floating)
            {
                //Debug.LogError(name + " 在地面-但是角色底部浮空推开");
                ProcessFall(0.75f);
                floatTick = Time.timeSinceLevelLoad;
            }
        }
        else
        {
            if (OnTouchWall)
            {
                //碰到墙壁
                if (!MoveOnGroundEx)
                {
                    //却没碰到地面.
                    if (ImpluseVec.y > 0)
                    {
                        //向上跳，轻功处理。??
                        if (Climbing)
                        {
                            //已经在爬墙过程中,按跳跃键时，弹开墙壁.
                            //if (controller.Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_Click, Time.deltaTime))
                            //    ProcessTouchWallJump(false);
                        }
                        else
                        {
                            //只有当前跳时,或者爬墙时.或者从墙壁弹开时，可以继续爬墙.|| ClimbJumping
                            if (posMng.JumpTick >= Global.JumpTimeLimit && 
                                (posMng.mActiveAction.Idx == CommonAction.Jump || Climbing || posMng.mActiveAction.Idx == CommonAction.JumpFallOnGround && Floating) &&
                                ImpluseVec.y > 100.0f &&
                                posMng.CheckClimb)//速度最少要达到多少才能轻功
                            {
                                //3条射线，-5°面向 5°左边近就调用右爬，中间则上爬，右边近则左爬.
                                //Debug.LogError("轻功开始");
                                posMng.CheckClimb = false;//单次爬墙不重复检测
                                float left = 100;
                                float middle = 100;
                                float right = 100;
                                if (Physics.Raycast(mPos, Quaternion.AngleAxis(-5, Vector3.up) * -transform.forward, out hit, charController.radius + 5, 1 << LayerMask.NameToLayer("Scene")))
                                    left = Vector3.Distance(hit.point, mPos);
                                if (Physics.Raycast(mPos, -transform.forward, out hit, charController.radius + 5, 1 << LayerMask.NameToLayer("Scene")))
                                    middle = Vector3.Distance(hit.point, mPos);
                                if (Physics.Raycast(mPos, Quaternion.AngleAxis(5, Vector3.up) * -transform.forward, out hit, charController.radius + 5, 1 << LayerMask.NameToLayer("Scene")))
                                    right = Vector3.Distance(hit.point, mPos);
                                float fMin = Mathf.Min(left, middle, right);
                                if (fMin != 100)
                                {
                                    if (fMin == left)
                                        posMng.ChangeAction(CommonAction.ClimbRight, 0.1f);
                                    else if (fMin == right)
                                        posMng.ChangeAction(CommonAction.ClimbLeft, 0.1f);
                                    else if (fMin == middle)
                                        posMng.ChangeAction(CommonAction.ClimbUp, 0.1f);
                                }
                            }
                        }

                    }
                    else//下落
                    if (Climbing)
                    {
                        //爬墙
                        if (!MoveOnGroundEx && ImpluseVec.y < yClimbEndLimit)
                        {
                            posMng.ClimbFallTick += FrameReplay.deltaTime;
                            if (posMng.ClimbFallTick > PoseStatus.ClimbFallLimit)
                            {
                                //Debug.LogError("爬墙速度低于最低速度-爬墙落下");
                                posMng.ChangeAction(CommonAction.JumpFall, 0.1f);//短时间内落地姿势
                                ProcessFall();
                                posMng.ClimbFallTick = 0.0f;
                            }
                        }
                        else if (MoveOnGroundEx)
                        {
                            //Debug.LogError("爬墙碰到地面-落到地面");
                            posMng.ChangeAction(CommonAction.JumpFall, 0.1f);//短时间内落地姿势
                        }
                    }
                    else //落地时候碰到墙壁.给一个反向的推力,
                    {
                        //!!!!贴墙落下，不能站在墙壁上，必须被弹开，否则城墙直接跳就可以上去
                        if (posMng.mActiveAction.Idx == CommonAction.Idle ||
                            posMng.mActiveAction.Idx == CommonAction.Run ||
                            posMng.mActiveAction.Idx == CommonAction.RunOnDrug ||
                            posMng.mActiveAction.Idx == CommonAction.WalkLeft ||
                            posMng.mActiveAction.Idx == CommonAction.WalkRight ||
                            posMng.mActiveAction.Idx == CommonAction.WalkBackward ||
                            posMng.mActiveAction.Idx == CommonAction.JumpLeft ||
                            posMng.mActiveAction.Idx == CommonAction.JumpRight ||
                            posMng.mActiveAction.Idx == CommonAction.JumpBack ||
                            posMng.mActiveAction.Idx == CommonAction.JumpLeftFall ||
                            posMng.mActiveAction.Idx == CommonAction.JumpRightFall ||
                            posMng.mActiveAction.Idx == CommonAction.JumpBackFall ||
                            posMng.mActiveAction.Idx == CommonAction.JumpFallOnGround)
                        {
                            //Debug.LogError("被墙壁轻微推开，避免悬挂在墙壁上");
                            ProcessFall();
                        }
                    }
                }
                else
                {
                    //碰到墙壁，也碰到地面（离地面不足4M）。贴着墙壁走，没什么需要处理的.
                }
            }
            else if (Climbing)
            {
                //爬墙过程中忽然没贴着墙壁了???直接落下
                //Debug.LogError("爬墙没有贴着墙壁-结束爬墙");
                posMng.ChangeAction(CommonAction.JumpFall, 0.1f);
                ProcessFall();
            }
            else if (Floating)
            {
                if (posMng.mActiveAction.Idx == CommonAction.Run ||
                    posMng.mActiveAction.Idx == CommonAction.Idle ||
                    posMng.mActiveAction.Idx == CommonAction.RunOnDrug ||
                    posMng.mActiveAction.Idx == CommonAction.WalkLeft ||
                    posMng.mActiveAction.Idx == CommonAction.WalkRight ||
                    posMng.mActiveAction.Idx == CommonAction.WalkBackward)
                {

                    //Debug.LogError(name + " 浮空-落地" + Time.frameCount);
                    //AddYVelocity(-100);//让他快速一点落地
                    //与落地的间隔超过0.3S再切换动作，否则就会抽搐
                    if (groundTick + 0.3f < Time.timeSinceLevelLoad)
                        posMng.ChangeAction(CommonAction.JumpFall, 0.1f);
                    //看是否被物件推开
                    ProcessFall();
                    floatTick = Time.timeSinceLevelLoad;
                }
            }
        }

        //如果Y轴速度向下，但是已经接触地面了
        if (ImpluseVec.y <= 0 && !IgnoreGravity)
        {
            if (MoveOnGroundEx || OnGround)
            {
                if ((posMng.mActiveAction.Idx >= CommonAction.Jump && posMng.mActiveAction.Idx <= CommonAction.JumpBackFall) || posMng.mActiveAction.Idx == CommonAction.JumpFallOnGround)
                {
                    posMng.ChangeAction(0, 0f);
                    //Debug.LogError(name + " 接触地面切换到IDle" + Time.frameCount);
                    groundTick = Time.timeSinceLevelLoad;
                }
                ResetYVelocity();
            }
        }

        //如果撞到天花板了.
        if (OnTopGround)
            if (ImpluseVec.y > 0)
                ImpluseVec.y = 0;


    }

    public void Move(Vector3 trans)
    {
        if (charController != null && charController.enabled)
        {
            CollisionFlags collisionFlags = charController.Move(trans);
            UpdateFlags(collisionFlags);
        }
        else
            transform.position += trans;
    }

    public void ResetWorldVelocity(bool reset)
    {
        if (reset)
            ImpluseVec.x = ImpluseVec.z = 0;
    }

    public void SetWorldVelocityExcludeY(Vector3 vec)
    {
        ImpluseVec.x = vec.x;
        ImpluseVec.z = vec.z;
    }

    public void SetWorldVelocity(Vector3 vec)
    {
        ImpluseVec.x = vec.x;
        ImpluseVec.y = vec.y;
        ImpluseVec.z = vec.z;
    }

    public void SetVelocityY(float y)
    {
        ImpluseVec.y = y;
    }

    //设置世界坐标系的速度,z向人物面前，x向人物右侧
    public void SetJumpVelocity(Vector2 velocityM)
    {
        float z = velocityM.y * JumpVelocityForward;
        float x = velocityM.x * JumpVelocityForward;
        Vector3 vec = z * -transform.forward + x * -transform.right;
        ImpluseVec.z = vec.z;
        ImpluseVec.x = vec.x;
    }

    public void SetVelocity(float z, float x)
    {
        Vector3 vec = z * -transform.forward + x * -transform.right;
        ImpluseVec.z = vec.z;
        ImpluseVec.x = vec.x;
        //Log.Print("z:" + ImpluseVec.z);
    }

    //计算水平轴的冲量 = 物体的末速度（1S内）
    public float CalcImpluseVec(float h, float f)
    {
        float ret = f * Mathf.Sqrt(2 * h / f);
        return ret;
    }

    public float CalcVelocity(float h)
    {
        float ret = Global.Instance.gGravity * Mathf.Sqrt(2 * h / Global.Instance.gGravity);
        if (ret > yLimitMax)
            ret = yLimitMax;
        return ret;
    }

    //踏墙壁跳跃,y为正常跳跃高度倍数.
    public void Jump2(float y, int act = CommonAction.Jump)
    {
        canControlOnAir = true;
        OnGround = false;
        ImpluseVec.y = CalcVelocity(y);
        posMng.JumpTick = 0.0f;
        posMng.CanAdjust = true;
        posMng.CheckClimb = true;
        posMng.ChangeAction(act, 0.1f);
        charLoader.SetActionScale(y / JumpLimit);
    }

    //给3个参数,Y轴完整跳跃的高度缩放(就是按下跳跃的压力缩放)，前方速度，右方速度
    public void Jump(bool Short, float ShortScale, int act = CommonAction.Jump)
    {
        canControlOnAir = true;
        OnGround = false;
        float jumpScale = Short ? (ShortScale * 0.32f) : 1.0f;
        float h = JumpLimit * jumpScale;
        ImpluseVec.y = CalcVelocity(h);
        posMng.JumpTick = 0.0f;
        posMng.CanAdjust = true;
        posMng.CheckClimb = true;
        posMng.ChangeAction(act, 0.1f);
        charLoader.SetActionScale(jumpScale);
    }

    public void ReleaseDefence()
    {
        posMng.ChangeAction(CommonAction.Idle, 0.1f);
    }

    MeteorUnit RebornTarget = null;
    SFXEffectPlay RebornEffect = null;
    public bool HasRebornTarget()
    {
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
            return false;
        //创建房间-盟主-死斗-无法复活队友
        if (Global.Instance.GLevelMode == LevelMode.CreateWorld)
        {
            if (Global.Instance.GGameMode == GameMode.MENGZHU || Global.Instance.GGameMode == GameMode.SIDOU)
                return false;
        }
        RebornTarget = null;
        float dis = Global.RebornRange;
        int index = -1;
        for (int i = 0; i < MeteorManager.Instance.DeadUnits.Count; i++)
        {
            MeteorUnit unit = MeteorManager.Instance.DeadUnits[i];
            if (unit == this)
                continue;
            if (!SameCamp(unit))
                continue;
            if (!unit.Dead)
                continue;
            float d = Vector3.Distance(transform.position, MeteorManager.Instance.DeadUnits[i].transform.position);
            if (dis > d)
            {
                dis = d;
                index = i;
            }
        }
        return index != -1;
    }

    public void SelectRebornTarget()
    {
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
            return;
        //创建房间-盟主-死斗-无法复活队友
        if (Global.Instance.GLevelMode == LevelMode.CreateWorld)
        {
            if (Global.Instance.GGameMode == GameMode.MENGZHU || Global.Instance.GGameMode == GameMode.SIDOU)
                return;
        }
        RebornTarget = null;
        float dis = Global.RebornRange;
        int index = -1;
        for (int i = 0; i < MeteorManager.Instance.DeadUnits.Count; i++)
        {
            MeteorUnit unit = MeteorManager.Instance.DeadUnits[i];
            if (unit == this)
                continue;
            if (!SameCamp(unit))
                continue;
            if (!unit.Dead)
                continue;
            float d = Vector3.Distance(transform.position, MeteorManager.Instance.DeadUnits[i].transform.position);
            if (dis > d)
            {
                dis = d;
                index = i;
            }
        }
        if (index >= 0 && index < MeteorManager.Instance.DeadUnits.Count)
        {
            RebornTarget = MeteorManager.Instance.DeadUnits[index];
            RebornEffect = SFXLoader.Instance.PlayEffect("ReBorn.ef", RebornTarget.transform.position, true);
        }
    }

    public void RebornFriend()
    {
        if (RebornTarget != null)
        {
            RebornTarget.OnReborn();
            RebornTarget = null;
        }
        if (RebornEffect != null)
        {
            GameObject.Destroy(RebornEffect.gameObject);
            RebornEffect = null;
        }
    }

    //被复活.
    public void OnReborn(float max = 0.3f)
    {
        if (Global.Instance.GLevelMode == LevelMode.CreateWorld)
        {
            if (Global.Instance.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                transform.position = Global.Instance.GLevelSpawn[Global.Instance.SpawnIndex];
                Global.Instance.SpawnIndex++;
                Global.Instance.SpawnIndex %= 16;
            }
            else if (Global.Instance.GGameMode == GameMode.ANSHA || Global.Instance.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.
                if (Camp == EUnitCamp.EUC_FRIEND)
                {
                    transform.position = Global.Instance.GCampASpawn[Global.Instance.CampASpawnIndex];
                    Global.Instance.CampASpawnIndex++;
                    Global.Instance.CampASpawnIndex %= 8;
                }
                else if (Camp == EUnitCamp.EUC_ENEMY)
                {
                    transform.position = Global.Instance.GCampASpawn[Global.Instance.CampBSpawnIndex];
                    Global.Instance.CampBSpawnIndex++;
                    Global.Instance.CampBSpawnIndex %= 8;
                }
            }
        }
        else if (Global.Instance.GLevelMode == LevelMode.SinglePlayerTask)
        {
            //闪现到出生点
            if (Global.Instance.GLevelItem.DisableFindWay != 1)
            {
                if (Attr.SpawnPoint < Global.Instance.GLevelItem.wayPoint.Count)
                    transform.position = Global.Instance.GLevelItem.wayPoint[Attr.SpawnPoint].pos;
                else
                    transform.position = Global.Instance.GLevelItem.wayPoint[0].pos;
            }
            else
            {
                if (Camp == EUnitCamp.EUC_FRIEND)
                {
                    transform.position = Global.Instance.GCampASpawn[Global.Instance.CampASpawnIndex];
                    Global.Instance.CampASpawnIndex++;
                    Global.Instance.CampASpawnIndex %= 8;
                }
                else if (Camp == EUnitCamp.EUC_ENEMY)
                {
                    transform.position = Global.Instance.GCampASpawn[Global.Instance.CampBSpawnIndex];
                    Global.Instance.CampBSpawnIndex++;
                    Global.Instance.CampBSpawnIndex %= 8;
                }
            }
        }
        Dead = false;
        posMng.WaitPause(false);
        posMng.OnReborn();
        EnableAI(true);
        MeteorManager.Instance.UnitInfos.Add(this);
        MeteorManager.Instance.DeadUnits.Remove(this);
        Attr.OnReborn(max);
        AngryValue = 0;
        charController.enabled = true;
        MeteorManager.Instance.PhysicalIgnore(this, false);
        MoveOnGroundEx = false;
        OnGround = false;
        IgnoreGravity = false;
        SetWorldVelocity(new Vector3(0, -100, 0));
    }

    //盟主模式下的自动复活.
    public void RebornUpdate()
    {
        RebornTick += FrameReplay.deltaTime;
        if (RebornTick >= 5.0f)
        {
            OnReborn(1.0f);
            if (Attr.IsPlayer)
            {
                if (FightWnd.Exist)
                {
                    FightWnd.Instance.UpdatePlayerInfo();
                    FightWnd.Instance.OnBattleStart();
                }
            }
            RebornTick = 0.0f;
        }
    }

    //暗杀模式，是否是队长，队长是一方阵营进入战场的第一个角色.
    public bool IsLeader { get; set; }

    public void OnDead(MeteorUnit killer = null)
    {
        if (!Dead)
        {
            if (Attr.IsPlayer && GameData.Instance.gameStatus.Undead)
                return;
            Dead = true;

            //如果有镖物，丢弃
            SetFlag(null, 0);
            //盟主模式，玩家在几秒后会复活.
            //暗杀模式，需要队长去复活.
            if (Global.Instance.GLevelMode == LevelMode.CreateWorld && (Global.Instance.GGameMode == GameMode.MENGZHU))
            {
                RebornTick = 0;
                WaitReborn = true;
            }

            if (killer == null)
            {
                GameOverlayWnd.Instance.InsertSystemMsg(string.Format("{0}死亡", name));
            }
            else
            {
                GameOverlayWnd.Instance.InsertSystemMsg(string.Format("{0}击败{1}", killer.name, name));
            }
            posMng.ChangeAction(CommonAction.Dead);
            posMng.WaitPause();//等待这个动作完毕后，暂停播放
            posMng.OnDead();
            Attr.ReduceHp(Attr.hpCur);
            EnableAI(false);
            BuffMng.Instance.RemoveUnit(this);
            MeteorManager.Instance.OnUnitDead(this);
            GameBattleEx.Instance.OnUnitDead(this, killer);
            if (FightWnd.Exist)
            {
                if (Attr.IsPlayer)
                    FightWnd.Instance.UpdatePlayerInfo();
            }
            if (Attr.IsPlayer && NGUICameraJoystick.instance)
                NGUICameraJoystick.instance.ResetJoystick();
        }
    }

    public void EnableAI(bool enable)
    {
        if (Robot != null)
            Robot.EnableAI(enable);
    }

    public void AddAngry(int angry)
    {
        int ang = AngryValue + angry;
        AngryValue = Mathf.Clamp(ang, 0, Global.ANGRYMAX);
    }

    //其他单位挂了.或者自己挂了。
    public void OnUnitDead(MeteorUnit deadunit)
    {
        if (deadunit == null)//自己挂了
        {
            SFXEffectPlay[] play = GetComponents<SFXEffectPlay>();
            for (int i = 0; i < play.Length; i++)
                play[i].OnPlayAbort();
            posMng.LinkEvent(()=> { charController.enabled = false; });
            return;
        }
        if (lockTarget == deadunit && lockTarget != null)
            lockTarget = null;

        if (Robot != null)
            Robot.OnUnitDead(deadunit);
    }

    //特殊招式时，关闭碰撞，让角色互相穿透,
    //设置角色间characterController不互相碰撞.
    public bool IgnorePhysical;
    public void PhysicalIgnore(MeteorUnit unit, bool ignore)
    {
        Physics.IgnoreCollision(charController, unit.charController, ignore);
        //IgnorePhysical = ignore;
    }

    Vector3 hitPoint;//最近一次碰撞的点.用这个点和法线来算一些轻功的东西
    Vector3 hitNormal;//碰撞面法线
    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.transform.root.tag.Equals("meteorUnit"))
        {
            MeteorUnit hitUnit = hit.gameObject.transform.root.GetComponent<MeteorUnit>();
            Vector3 vec = hitUnit.mPos - transform.position;
            vec.y = 0;
            //在防御中.不受推挤.
            if (hitUnit.posMng != null && hitUnit.posMng.onDefence)
                return;
            hitUnit.SetWorldVelocity(Vector3.Normalize(vec) * 30);
        }
        else if (hit.gameObject.transform.root.tag.Equals("SceneItemAgent"))
        {
            SceneItemAgent agent = hit.gameObject.GetComponentInParent<SceneItemAgent>();
            if (agent != null && agent.HasDamage() && (!attackDelay.ContainsKey(agent)))
            {
                if (!Attr.Dead)
                {
                    OnDamage(null, agent.DamageValue());
                    attackDelay[agent] = 2.0f;
                }
            }
        }
        else
        {
            hitPoint = hit.point;
            hitNormal = hit.normal;
            if (Robot != null)
            {
                Robot.CheckStatus();
            }
        }
    }

    //由目标的脊椎骨看向目标，若有场景阻碍则返回false，否则表示可穿过
    //该函数只能用来计算角色所处于的路点.
    public bool PassThrough(Vector3 target)
    {
        RaycastHit ray = new RaycastHit();
        if (Physics.SphereCast(mSkeletonPivot, 2.0f, (target - mSkeletonPivot).normalized, out ray, Vector3.Distance(target, mSkeletonPivot), 1 << LayerMask.NameToLayer("Scene")))
            return false;
        return true;
    }

    public void WeaponReturned(int poseIdx)
    {
        //219等待回收武器.
        if (charLoader != null)
        {
            if (AppInfo.Instance.MeteorVersion.Equals("1.07"))
            {
                if (posMng.mActiveAction.Idx == (poseIdx + 1))
                {
                    charLoader.SetLoop(false);
                }
                else
                {
                    charLoader.LinkEvent(poseIdx + 1, PoseEvt.WeaponIsReturned);
                }
            }
            else if (AppInfo.Instance.MeteorVersion.Equals("9.07"))
            {
                if (posMng.mActiveAction.Idx == 219)
                    charLoader.SetLoop(false);
                else
                    charLoader.LinkEvent(219, PoseEvt.WeaponIsReturned);
            }
        }
    }

    //public bool allowAttack { get; set; }
    public AttackDes CurrentDamage { get { return damage; } }
    AttackDes damage;
    //每8帧一次伤害判定.(5 * 1.0f / 30.0f)
    const float refreshTick = 10f / 30.0f;
    Dictionary<SceneItemAgent, float> Damaged2 = new Dictionary<SceneItemAgent, float>();
    Dictionary<MeteorUnit, float> Damaged = new Dictionary<MeteorUnit, float>();
    public void ChangeAttack(AttackDes attack)
    {
        //if (attack != null && attack.PoseIdx == 260)
        //    Debug.LogError("260 attack start");
        //if (attack == null && damage != null && damage.PoseIdx == 260)
        //    Debug.LogError("260 attack end");
        if (attack != null && attack.PoseIdx < 200)
            return;
        if (GameBattleEx.Instance == null)
            return;
        if (damage == attack && damage != null)
            return;
        damage = attack;
        if (damage == null)
        {
            Damaged.Clear();
            GameBattleEx.Instance.ClearDamageCollision(this);
            ChangeAttack(false);
            Damaged2.Clear();
            return;
        }

        if (Attr.IsPlayer && lockTarget == null && GameBattleEx.Instance.autoTarget != null)
        {
            if (GetWeaponType() == (int)EquipWeaponType.Gun || GetWeaponType() == (int)EquipWeaponType.Dart || GetWeaponType() == (int)EquipWeaponType.Guillotines)
            {
                //远程武器不能锁定
            }
            else
            {
                lockTarget = GameBattleEx.Instance.autoTarget;
                GameBattleEx.Instance.ChangeLockedTarget(lockTarget);
            }
        }

        //遍历受击盒根据攻击定义，生成相应的攻击盒。
        for (int i = 0; i < hitList.Count; i++)
        {
            if (attack.bones.Contains(hitList[i].name))
            {
                GameBattleEx.Instance.AddDamageCollision(this, hitList[i]);
            }
        }
        //如果包含武器和特效.
        if (attack.bones.Contains("weapon"))
        {
            for (int i = 0; i < weaponLoader.weaponDamage.Count; i++)
                GameBattleEx.Instance.AddDamageCollision(this, weaponLoader.weaponDamage[i]);
        }

        if (attack.bones.Contains("effect"))
        {
            for (int i = 0; i < sfxList.Count; i++)
            {
                if (!sfxList[i].PlayDone)
                    GameBattleEx.Instance.AddDamageCollision(this, sfxList[i].damageBox);
            }
        }

        //枪械射击之类,1,枪，无轨迹，2，飞镖，自由落体轨迹 3，飞轮，贝塞尔曲线/B样条线轨迹.
        //这些武器的大招，一定是带effect攻击特效的.所以可以通过伤害里的骨骼列表判断是否属于
        if (attack.bones.Count == 0)
            GameBattleEx.Instance.AddDamageCheck(this, attack);

        ChangeAttack(true);
    }

    public void ChangeAttack(bool allow)
    {
        weaponLoader.ChangeAttack(allow);
        //allowAttack = allow;
        for (int i = 0; i < sfxList.Count; i++)
            sfxList[i].ChangeAttack(allow);
        if (!allow)
            damage = null;
    }

    //public void OnCollisionEnter(Collision collision)
    //{
    //    Debug.LogError("OnCollisionEnter occur on meteorunit");
    //}

    //public void OnCollisionStay(Collision collision)
    //{
    //    Debug.LogError("OnCollisionStay occur on meteorunit");
    //}

    //public void OnCollisionExit(Collision collision)
    //{
    //    Debug.LogError("OnCollisionExit occur on meteorunit");
    //}

    //成功碰撞到敌人，没有检测防御状态。
    public void Attack(MeteorUnit other)
    {
        canControlOnAir = false;
        Damaged.Add(other, refreshTick);
    }

    //成功碰撞到场景物件
    public void Attack(SceneItemAgent other)
    {
        canControlOnAir = false;
        Damaged2.Add(other, refreshTick);
    }
    
    public void OnAttack(MeteorUnit other, AttackDes des)
    {
        canControlOnAir = false;
        OnDamage(other, des);
    }
    //成功被人攻击到，没有检测防御状态.
    public void OnAttack(MeteorUnit other)
    {
        canControlOnAir = false;
        //Debug.LogError("unit:" + name + " was attacked by:" + other.name);
        OnDamage(other);
    }

    //只负责一些机关，例如滚石，摆斧，撞到角色时的伤害处理
    Dictionary<SceneItemAgent, float> attackDelay = new Dictionary<SceneItemAgent, float>();
    Dictionary<SceneItemAgent, float> touchDelay = new Dictionary<SceneItemAgent, float>();//部分回血阵的回血间隔
    public void OnTriggerEnter(Collider other)
    {
        if (Dead)
            return;
        //Debug.LogError("ontrigger enter:" + other.gameObject.name);
        if (other.tag == "SceneItemAgent" || (other.transform.parent != null && other.transform.parent.tag == "SceneItemAgent"))
        {
            SceneItemAgent trigger = other.gameObject.GetComponentInParent<SceneItemAgent>();
            if (trigger == null)
                return;

            if (trigger != null)
            {
                //如果是一个攻击道具，陷阱或者机关之类的。
                if (trigger.HasDamage())
                {
                    if (attackDelay.ContainsKey(trigger))
                        return;
                    if (!Attr.Dead)
                    {
                        OnDamage(null, trigger.DamageValue());
                        attackDelay[trigger] = 2.0f;
                    }
                }
                else
                {
                    if (touchDelay.ContainsKey(trigger))
                        return;
                    if (trigger.root != null)
                    {
                        trigger.OnPickup(this);
                        touchDelay[trigger] = 2.0f;
                        if (TargetItem == trigger)
                            TargetItem = null;
                    }
                }
            }
        }
        else if (other.tag.Equals("PickupItemAgent") || (other.transform.parent != null && other.transform.parent.tag.Equals("PickupItemAgent")))
        {
            PickupItemAgent trigger = other.gameObject.GetComponentInParent<PickupItemAgent>();
            if (trigger == null)
                return;

            if (trigger != null)
            {
                trigger.OnPickup(this);
                if (TargetItem == trigger)
                    TargetItem = null;
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (Dead)
            return;
        if (other.tag == "SceneItemAgent" || (other.transform.parent != null && other.transform.parent.tag == "SceneItemAgent"))
        {
            SceneItemAgent trigger = other.gameObject.GetComponentInParent<SceneItemAgent>();
            if (trigger == null)
                return;
            if (trigger != null)
            {
                if (trigger.HasDamage())
                {
                    //受到陷阱攻击应该有无敌间隔.
                    if (!attackDelay.ContainsKey(trigger))
                    {
                        if (!Attr.Dead)
                        {
                            OnDamage(null, trigger.DamageValue());
                            attackDelay[trigger] = 2.0f;
                        }
                    }
                }
                else
                {
                    //持续血阵，状态
                    if (touchDelay.ContainsKey(trigger))
                        return;
                    if (trigger.root != null)
                    {
                        trigger.OnPickup(this);
                        touchDelay[trigger] = 2.0f;
                        if (TargetItem == trigger)
                            TargetItem = null;
                    }
                }
            }
        }
        else if (other.tag.Equals("PickupItemAgent") || (other.transform.parent != null && other.transform.parent.tag.Equals("PickupItemAgent")))
        {
            PickupItemAgent trigger = other.gameObject.GetComponentInParent<PickupItemAgent>();
            if (trigger == null)
                return;

            if (trigger != null)
            {
                trigger.OnPickup(this);
                if (TargetItem == trigger)
                    TargetItem = null;
            }
        }

    }

    //public void OnTriggerExit(Collider other)
    //{
    //    if (Dead)
    //        return;
    //    SceneItemAgent trigger = other.gameObject.GetComponentInParent<SceneItemAgent>();
    //    if (trigger == null)
    //        return;
    //    if (trigger != null && attackDelay.ContainsKey(trigger))
    //        attackDelay.Remove(trigger);
    //}

    List<SFXUnit> sfxList = new List<SFXUnit>();
    public void AddAttackSFX(SFXUnit sfx)
    {
        sfxList.Add(sfx);
    }

    public void OnSFXDestroy(SFXUnit sfx)
    {
        sfxList.Remove(sfx);
        if (GameBattleEx.Instance != null)
            GameBattleEx.Instance.OnSFXDestroy(this, sfx.damageBox);
    }

    //受击盒列表
    public List<BoxCollider> hitList = new List<BoxCollider>();
    public void AddHitBox(BoxCollider co)
    {
        hitList.Add(co);
    }

    public void ChangeWeaponTrail(DragDes drag)
    {
        weaponLoader.ChangeWeaponTrail(drag);
    }

    int CalcDamage(MeteorUnit attacker, AttackDes des = null)
    {
        //(((武器攻击力 + buff攻击力) x 招式攻击力） / 100) - （敌方武器防御力 + 敌方buff防御力） / 10
        //你的攻击力，和我的防御力之间的计算
        //attacker.damage.PoseIdx;
        //以匕首259作为示例，普通武器防具
        //匕首武器攻击=90
        //BUFF无
        //招式259，怒意-大招 伤害900
        //普通武器防御 15
        //BUFF防御0
        //(90 * 900 / 100 - 15); 810 - 15 = 795 / 10 = 79.5;
        int WeaponDef = CalcDef();
        int BuffDef = Attr.CalcBuffDef();
        AttackDes atk = des == null ? attacker.damage : des;
        int WeaponDamage = attacker.CalcDamage();
        int PoseDamage = MenuResLoader.Instance.FindOpt(atk.PoseIdx, 3).second[0].flag[6];
        int BuffDamage = attacker.Attr.CalcBuffDamage();
        int realDamage = Mathf.Abs(Mathf.CeilToInt(((WeaponDamage * (1.0f + BuffDamage / 100.0f)) * PoseDamage) / 100.0f - (WeaponDef * (1.0f + BuffDef / 100.0f))));
        return realDamage;
    }

    //计算其他人在我的哪一个方位，每个方位控制90度范围。
    int CalcDirection(MeteorUnit other)
    {
        Vector3 otherVec = new Vector3(other.mPos.x, 0, other.mPos.z);
        Vector3 Vec = new Vector3(mPos.x, 0, mPos.z);
        Vector3 VecOffset = Vector3.Normalize(otherVec - Vec);
        float angle = Vector3.Dot(VecOffset, -transform.forward);
        float angleLeft = Vector3.Dot(VecOffset, transform.right);
        //unity精度问题容易得到nan
        if (angle > 1.0f)
            angle = 1.0f;
        else if (angle < -1.0f)
            angle = -1.0f;
        float degree =  Mathf.Acos(Mathf.Clamp(angle, -1.0f, 1.0f)) * Mathf.Rad2Deg;
        //Debug.LogError("角度:" + degree);
        if (degree <= 45)
        {
            //Debug.LogError("正面");
            return 0;
        }
        if (degree <= 135 && angleLeft > 0)
        {
            //Debug.LogError("左侧");
            return 2;
        }
        if (degree <= 135 && angleLeft < 0)
        {
            //Debug.LogError("右侧");
            return 3;
        }
        //Debug.LogError("背面");
        return 1;
    }

    public void OnBuffDamage(int buffDamage)
    {
        if (NGUICameraJoystick.instance != null)
            NGUICameraJoystick.instance.ResetJoystick();//防止受到攻击时还可以移动视角
        SetGunReady(false);
        Attr.ReduceHp(buffDamage);
        posMng.OnChangeAction(CommonAction.OnDrugHurt);
    }

    /*
     * 
     * 每一个动作开始的时候，都会刷新一个数组，
     * 当你在这个动作中，任何一帧碰撞过一个角色之后，
     * 会记录一个结构
     * {target:被碰撞的角色, 
     * frame:第几帧}
     * 你可以利用这个结构获得：
     * 1，我已经碰撞过某个角色多少次了。
     * 2，我上次第几帧碰撞了某个角色。
     * 一些act里面会有要求说我一个动作可以连续命中同一个角色多少次，
     * 每次间隔多少的。对于这个结构的记录方式的优化，因为项目而不同。

作者：猴与花果山
链接：https://www.zhihu.com/question/44675181/answer/98073941
来源：知乎
著作权归作者所有。商业转载请联系作者获得授权，非商业转载请注明出处。
     */

    //飞镖或飞轮的，含受击定义,即使飞镖拥有者死了，仍调用.
    void OnDamage(MeteorUnit attacker, AttackDes attackdes)
    {
        if (Dead)
            return;
        if (NGUICameraJoystick.instance != null && Attr.IsPlayer)
            NGUICameraJoystick.instance.ResetJoystick();//防止受到攻击时还可以移动视角

        //Debug.Log(string.Format("player:{0} attacked by:{1}", name, attacker == null ? "null" : attacker.name));

        if (Attr.IsPlayer && GameData.Instance.gameStatus.Undead)
            return;
        //任意受击，都会让角色退出持枪预备姿势
        SetGunReady(false);
        if (attacker == null)
        {
            //
        }
        else
        {
            //到此处均无须判读阵营等。
            AttackDes dam = attackdes;
            int direction = CalcDirection(attacker);
            int directionAct = dam.TargetPose;
            switch (direction)
            {
                case 0: directionAct = dam.TargetPoseFront; break;//这个是前后左右，武器防御受击是 上下左右，上下指角色面朝方向头顶和底部
                case 1: directionAct = dam.TargetPoseBack; break;
                case 2: directionAct = dam.TargetPoseLeft; break;
                case 3: directionAct = dam.TargetPoseRight; break;
            }
            if (attacker.Attr.IsPlayer && GameData.Instance.gameStatus.EnableGodMode)
            {
                //一击必杀
                string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                SFXLoader.Instance.PlayEffect(attackAudio, charLoader);
                if (Robot != null)
                    Robot.OnDamaged(attacker);
                Attr.ReduceHp(Attr.HpMax);
                if (Attr.Dead)
                    OnDead(attacker);
            }
            else
            {
                if (posMng.onDefence)
                {
                    if (dam._AttackType == 0)
                    {
                        //在此时间结束前，不许使用输入设备输入.
                        if (charLoader != null)
                            charLoader.LockTime(dam.DefenseValue);
                        //Move(-attacker.transform.forward * dam.DefenseMove);
                        //通过当前武器和方向，得到防御动作ID  40+(x-1)*4类似 匕首 = 5=> 40+(5-1)*4 = 56,防御住前方攻击 57 58 59就是其他方向的
                        int TargetPos = GetGuardPose(direction);
                        string attackAudio = string.Format("W{0:D2}GD{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                        SFXLoader.Instance.PlayEffect(attackAudio, charLoader);
                        //TargetPos = 40 + ((int)idx - 1) * 4 + direction;
                        //Debug.LogError("targetPos:" + TargetPos);
                        posMng.OnChangeAction(TargetPos);
                        charLoader.SetActionScale(dam.DefenseMove);
                        //charLoader.SetActionRotation(mPos - attacker.mPos);
                        int realDamage = CalcDamage(attacker, attackdes);
                        AngryValue += realDamage / 30;//防御住伤害。则怒气增加
                    }
                    else if (dam._AttackType == 1)
                    {
                        //这个招式伤害多少?
                        //dam.PoseIdx;算伤害
                        int realDamage = CalcDamage(attacker, attackdes);
                        //Debug.Log("受到:" + realDamage + " 点伤害");
                        Option poseInfo = MenuResLoader.Instance.GetPoseInfo(dam.PoseIdx);
                        if (poseInfo.first.Length != 0 && poseInfo.first[0].flag[0] == 16)
                            GetItem(poseInfo.first[0].flag[1]);
                        //if (hurtRecord.ContainsKey(attacker))
                        //    hurtRecord[attacker] += realDamage;
                        //else
                        //    hurtRecord.Add(attacker, realDamage);

                        //当前打击者比当前目标伤害高，那么改去杀打击者.AI切换仇恨最深者
                        //if (robot != null)
                        //{
                        //    if (lockTarget != null && hurtRecord.ContainsKey(lockTarget))
                        //    {
                        //        if (hurtRecord[lockTarget] < hurtRecord[attacker])
                        //            lockTarget = attacker;
                        //    }
                        //    else
                        //        lockTarget = attacker;
                        //}

                        if (Attr != null)
                            Attr.ReduceHp(realDamage);
                        if (charLoader != null)
                            charLoader.LockTime(dam.TargetValue);

                        string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                        SFXLoader.Instance.PlayEffect(attackAudio, charLoader);
                        AngryValue += (int)((realDamage * 10) / 73.0f);
                        if (Robot != null)
                            Robot.OnDamaged(attacker);
                        if (Attr.Dead)
                            OnDead(attacker);
                        else
                        {
                            //如果攻击者是主角，而自己又没有死，那么设置一下锁定目标为自己.(匕首后A接大，自动转向)
                            if (attacker.Attr.IsPlayer)
                            {
                                if (GameBattleEx.Instance.CanLockTarget(this))
                                {
                                    GameBattleEx.Instance.ChangeLockedTarget(this);
                                    attacker.lockTarget = this;
                                }
                            }
                            else
                            {
                                //不是主角打就记录伤害，谁伤害高，就去追着谁打。
                            }
                            //播放相应特效-音效 0飞镖1

                            //被攻击后，防御相当与没有了.
                            posMng.OnChangeAction(directionAct);
                            charLoader.SetActionScale(dam.TargetMove);
                            //charLoader.SetActionRotation(mPos - attacker.mPos);
                        }
                    }
                }
                else
                {
                    int realDamage = CalcDamage(attacker, attackdes);
                    //Debug.LogError(Attr.Name + ":受到:" + attacker.Attr.name + " 的攻击 减少 " + realDamage + " 点气血" + ":f" + Time.frameCount);
                    Attr.ReduceHp(realDamage);
                    //if (hurtRecord.ContainsKey(attacker))
                    //    hurtRecord[attacker] += realDamage;
                    //else
                    //    hurtRecord.Add(attacker, realDamage);

                    //当前打击者比当前目标伤害高，那么改去杀打击者,只针对AI
                    //if (robot != null)
                    //{
                    //    if (lockTarget != null && hurtRecord.ContainsKey(lockTarget))
                    //    {
                    //        if (hurtRecord[lockTarget] < hurtRecord[attacker])
                    //            lockTarget = attacker;
                    //    }
                    //    else
                    //        lockTarget = attacker;
                    //}
                    //处理招式打人后带毒
                    Option poseInfo = MenuResLoader.Instance.GetPoseInfo(dam.PoseIdx);
                    if (poseInfo.first.Length != 0 && poseInfo.first[0].flag[0] == 16)//16受到此招式攻击会得到物品
                        GetItem(poseInfo.first[0].flag[1]);

                    if (charLoader != null)
                        charLoader.LockTime(dam.TargetValue);
                    AngryValue += (int)((realDamage * 10) / 73.0f);
                    string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                    SFXLoader.Instance.PlayEffect(attackAudio, charLoader);
                    if (Robot != null)
                        Robot.OnDamaged(attacker);
                    if (Attr.Dead)
                        OnDead(attacker);
                    else
                    {
                        //如果攻击者是主角，而自己又没有死，那么设置一下锁定目标为自己.(主角匕首后A打到我-接大，自动转向)
                        if (attacker.Attr.IsPlayer)
                        {
                            if (GameBattleEx.Instance.CanLockTarget(this))
                            {
                                GameBattleEx.Instance.ChangeLockedTarget(this);
                                attacker.lockTarget = this;
                            }
                        }
                        else
                        {
                            //不是主角打就记录伤害，谁伤害高，就去追着谁打。
                        }
                        posMng.OnChangeAction(directionAct);
                        charLoader.SetActionScale(dam.TargetMove);
                        //charLoader.SetActionRotation(mPos - attacker.mPos);
                    }
                }
            }
        }

        if (FightWnd.Exist)
        {
            //先飘血。
            if (Attr.IsPlayer)
                FightWnd.Instance.UpdatePlayerInfo();
            else if (GameData.Instance.gameStatus.ShowBlood && !SameCamp(MeteorManager.Instance.LocalPlayer))
                FightWnd.Instance.UpdateMonsterInfo(this);
        }
    }

    //除了，武器碰撞，特效碰撞，还可以是buff，机关
    public void OnDamage(MeteorUnit attacker, int buffDamage = 0)
    {
        if (Dead)
            return;

        if (NGUICameraJoystick.instance != null && Attr.IsPlayer)
            NGUICameraJoystick.instance.ResetJoystick();//防止受到攻击时还可以移动视角

        //Debug.Log(string.Format("player:{0} attacked by:{1}", name, attacker == null ? "null": attacker.name));
        //任意受击，都会让角色退出持枪预备姿势
        if (Attr.IsPlayer && GameData.Instance.gameStatus.Undead)
            return;
        SetGunReady(false);
        if (attacker == null)
        {
            if (Robot != null)
                Robot.OnDamaged(attacker);
            //环境伤害.
            Attr.ReduceHp(buffDamage);
            if (Attr.Dead)
                OnDead();
            else
                posMng.OnChangeAction(CommonAction.OnDrugHurt);
        }
        else
        {
            //到此处均无须判读阵营等。
            AttackDes dam = attacker.damage;
            int direction = CalcDirection(attacker);
            int directionAct = dam.TargetPose;
            switch (direction)
            {
                case 0: directionAct = dam.TargetPoseFront; break;//这个是前后左右，武器防御受击是 上下左右，上下指角色面朝方向头顶和底部
                case 1: directionAct = dam.TargetPoseBack; break;
                case 2: directionAct = dam.TargetPoseLeft; break;
                case 3: directionAct = dam.TargetPoseRight; break;
            }

            if (attacker.Attr.IsPlayer && GameData.Instance.gameStatus.EnableGodMode)
            {
                //一击必杀
                string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                SFXLoader.Instance.PlayEffect(attackAudio, charLoader);
                if (Robot != null)
                    Robot.OnDamaged(attacker);
                Attr.ReduceHp(Attr.HpMax);
                if (Attr.Dead)
                    OnDead(attacker);
            }
            else
            {
                if (posMng.onDefence)
                {
                    if (dam._AttackType == 0)
                    {
                        //在此时间结束前，不许使用输入设备输入.
                        if (charLoader != null)
                            charLoader.LockTime(dam.DefenseValue);
                        //Move(-attacker.transform.forward * dam.DefenseMove);
                        //通过当前武器和方向，得到防御动作ID  40+(x-1)*4类似 匕首 = 5=> 40+(5-1)*4 = 56,防御住前方攻击 57 58 59就是其他方向的
                        int TargetPos = GetGuardPose(direction);
                        string attackAudio = string.Format("W{0:D2}GD{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                        SFXLoader.Instance.PlayEffect(attackAudio, charLoader);
                        posMng.OnChangeAction(TargetPos);
                        charLoader.SetActionScale(dam.DefenseMove);
                        //charLoader.SetActionRotation(this.mPos - attacker.mPos);
                        int realDamage = CalcDamage(attacker);
                        AngryValue += (realDamage / 30);//防御住伤害。则怒气增加 200CC = 100 ANG
                    }
                    else if (dam._AttackType == 1)
                    {
                        //这个招式伤害多少?
                        //dam.PoseIdx;算伤害
                        int realDamage = CalcDamage(attacker);
                        
                        Option poseInfo = MenuResLoader.Instance.GetPoseInfo(dam.PoseIdx);
                        if (poseInfo.first.Length != 0 && poseInfo.first[0].flag[0] == 16)
                            GetItem(poseInfo.first[0].flag[1]);
                        //Debug.Log("受到:" + realDamage + " 点伤害");
                        Attr.ReduceHp(realDamage);
                        if (charLoader != null)
                            charLoader.LockTime(dam.TargetValue);
                        string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                        SFXLoader.Instance.PlayEffect(attackAudio, charLoader);
                        AngryValue += (int)(realDamage * 10 / 73.0f);
                        if (Robot != null)
                            Robot.OnDamaged(attacker);
                        if (Attr.Dead)
                            OnDead(attacker);
                        else
                        {
                            //如果攻击者是主角，而自己又没有死，那么设置一下锁定目标为自己.(匕首后A接大，自动转向)
                            if (attacker.Attr.IsPlayer)
                            {
                                if (GameBattleEx.Instance.CanLockTarget(this))
                                {
                                    GameBattleEx.Instance.ChangeLockedTarget(this);
                                    attacker.lockTarget = this;
                                }
                            }
                            else
                            {
                                //不是主角打就记录伤害，谁伤害高，就去追着谁打。
                            }
                            posMng.OnChangeAction(directionAct);
                            charLoader.SetActionScale(dam.TargetMove);
                        }
                    }
                }
                else
                {
                    int realDamage = CalcDamage(attacker);
                    //Debug.Log("受到:" + realDamage + " 点伤害" + " f:" + Time.frameCount);
                    Attr.ReduceHp(realDamage);
                    //处理招式打人后带毒
                    Option poseInfo = MenuResLoader.Instance.GetPoseInfo(dam.PoseIdx);
                    if (poseInfo.first.Length != 0 && poseInfo.first[0].flag[0] == 16)//16受到此招式攻击会得到物品
                        GetItem(poseInfo.first[0].flag[1]);

                    if (charLoader != null)
                        charLoader.LockTime(dam.TargetValue);
                    AngryValue += (int)((realDamage * 10) / 73.0f);
                    string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                    SFXLoader.Instance.PlayEffect(attackAudio, charLoader);
                    if (Robot != null)
                        Robot.OnDamaged(attacker);
                    if (Attr.Dead)
                        OnDead(attacker);
                    else
                    {
                        //如果攻击者是主角，而自己又没有死，那么设置一下锁定目标为自己.(主角匕首后A打到我-接大，自动转向)
                        if (attacker.Attr.IsPlayer)
                        {
                            if (GameBattleEx.Instance.CanLockTarget(this))
                            {
                                GameBattleEx.Instance.ChangeLockedTarget(this);
                                attacker.lockTarget = this;
                            }
                        }
                        else
                        {
                        }
                        posMng.OnChangeAction(directionAct);
                        charLoader.SetActionScale(dam.TargetMove);
                    }
                }
            }
        }
        if (FightWnd.Exist)
        {
            //先飘血。
            if (Attr.IsPlayer)
                FightWnd.Instance.UpdatePlayerInfo();
            else if (GameData.Instance.gameStatus.ShowBlood && !SameCamp(MeteorManager.Instance.LocalPlayer))
                FightWnd.Instance.UpdateMonsterInfo(this);
        }
    }

    public void CrouchRush(int dir = 0)
    {
        switch (dir)
        {
            case 0:
                posMng.ChangeAction(CommonAction.DCForw, 0.1f);
                break;
            case 1:
                posMng.ChangeAction(CommonAction.DCBack, 0.1f);
                break;
            case 2:
                posMng.ChangeAction(CommonAction.DCLeft, 0.1f);
                break;
            case 3:
                posMng.ChangeAction(CommonAction.DCRight, 0.1f);
                break;
        }
    }
    public void IdleRush(int dir = 0)
    {
        //根据DIR和武器决定动作
        switch (dir)
        {
            case 0://前
                switch (GetWeaponType())
                {
                    case (int)EquipWeaponType.Gun://火枪
                    case (int)EquipWeaponType.Dart://飞镖
                    case (int)EquipWeaponType.Guillotines://飞轮
                        CrouchRush(dir);
                        break;
                    case (int)EquipWeaponType.Hammer://锤子
                    case (int)EquipWeaponType.Brahchthrust://双刺
                        posMng.ChangeAction(CommonAction.DForw1, 0.1f);
                        break;
                    case (int)EquipWeaponType.Sword:
                    case (int)EquipWeaponType.Blade:
                        posMng.ChangeAction(CommonAction.DForw2, 0.1f);
                        break;
                    case (int)EquipWeaponType.Knife:
                    case (int)EquipWeaponType.Lance:
                    
                        posMng.ChangeAction(CommonAction.DForw3, 0.1f);
                        break;
                    case (int)EquipWeaponType.NinjaSword:
                        posMng.ChangeAction(CommonAction.DForw4, 0.1f);
                        break;
                    case (int)EquipWeaponType.HeavenLance:
                        posMng.ChangeAction(CommonAction.DForw5, 0.1f);
                        break;
                    case (int)EquipWeaponType.Gloves:
                        posMng.ChangeAction(CommonAction.DForw6, 0.1f);
                        break;
                }
                
                break;
            case 1://后
                switch (GetWeaponType())
                {
                    case (int)EquipWeaponType.Gun://火枪
                    case (int)EquipWeaponType.Dart://飞镖
                    case (int)EquipWeaponType.Guillotines://飞轮
                        CrouchRush(dir);
                        break;
                    case (int)EquipWeaponType.Hammer://锤子
                    case (int)EquipWeaponType.Brahchthrust://双刺
                        posMng.ChangeAction(CommonAction.DBack1, 0.1f);
                        break;
                    case (int)EquipWeaponType.Sword:
                    case (int)EquipWeaponType.Blade:
                        posMng.ChangeAction(CommonAction.DBack2, 0.1f);
                        break;
                    case (int)EquipWeaponType.Knife:
                    case (int)EquipWeaponType.Lance:
                        posMng.ChangeAction(CommonAction.DBack3, 0.1f);
                        break;
                    case (int)EquipWeaponType.NinjaSword:
                        posMng.ChangeAction(CommonAction.DBack4, 0.1f);
                        break;
                    case (int)EquipWeaponType.HeavenLance:
                        posMng.ChangeAction(CommonAction.DBack5, 0.1f);
                        break;
                    case (int)EquipWeaponType.Gloves:
                        posMng.ChangeAction(CommonAction.DBack6, 0.1f);
                        break;
                }
                break;
            case 2://左
                switch (GetWeaponType())
                {
                    case (int)EquipWeaponType.Gun://火枪
                    case (int)EquipWeaponType.Dart://飞镖
                    case (int)EquipWeaponType.Guillotines://飞轮
                        CrouchRush(dir);
                        break;
                    case (int)EquipWeaponType.Hammer://锤子
                    case (int)EquipWeaponType.Brahchthrust://双刺
                        posMng.ChangeAction(CommonAction.DLeft1, 0.1f);
                        break;
                    case (int)EquipWeaponType.Sword:
                    case (int)EquipWeaponType.Blade:
                        posMng.ChangeAction(CommonAction.DLeft2, 0.1f);
                        break;
                    case (int)EquipWeaponType.Knife:
                    case (int)EquipWeaponType.Lance:
                        posMng.ChangeAction(CommonAction.DLeft3, 0.1f);
                        break;
                    case (int)EquipWeaponType.NinjaSword:
                        posMng.ChangeAction(CommonAction.DLeft4, 0.1f);
                        break;
                    case (int)EquipWeaponType.HeavenLance:
                        posMng.ChangeAction(CommonAction.DLeft5, 0.1f);
                        break;
                    case (int)EquipWeaponType.Gloves:
                        posMng.ChangeAction(CommonAction.DLeft6, 0.1f);
                        break;
                }
                break;
            case 3://右
                switch (GetWeaponType())
                {
                    case (int)EquipWeaponType.Gun://火枪
                    case (int)EquipWeaponType.Dart://飞镖
                    case (int)EquipWeaponType.Guillotines://飞轮
                        CrouchRush(dir);
                        break;
                    case (int)EquipWeaponType.Hammer://锤子
                    case (int)EquipWeaponType.Brahchthrust://双刺
                        posMng.ChangeAction(CommonAction.DRight1, 0.1f);
                        break;
                    case (int)EquipWeaponType.Sword:
                    case (int)EquipWeaponType.Blade:
                        posMng.ChangeAction(CommonAction.DRight2, 0.1f);
                        break;
                    case (int)EquipWeaponType.Knife:
                    case (int)EquipWeaponType.Lance:
                        posMng.ChangeAction(CommonAction.DRight3, 0.1f);
                        break;
                    case (int)EquipWeaponType.NinjaSword:
                        posMng.ChangeAction(CommonAction.DRight4, 0.1f);
                        break;
                    case (int)EquipWeaponType.HeavenLance:
                        posMng.ChangeAction(CommonAction.DRight5, 0.1f);
                        break;
                    case (int)EquipWeaponType.Gloves:
                        posMng.ChangeAction(CommonAction.DRight6, 0.1f);
                        break;
                }
                break;
        }
    }

    //当视角开始准备拉动前,
    bool restoreIdle;
    Coroutine CheckRotate;
    public void OnCameraRotateStart()
    {
        posMng.Rotateing = true;
        checkRotateTick = 0.3f;
        if (CheckRotate == null)
            CheckRotate = StartCoroutine("CheckRotateEnd");
    }

    float checkRotateTick = 0.3f;
    IEnumerator CheckRotateEnd()
    {
        while (true)
        {
            checkRotateTick -= FrameReplay.deltaTime;
            if (checkRotateTick < 0.0f)
            {
                posMng.Rotateing = false;
                CheckRotate = null;
                yield break;
            }
            yield return 0;
        }
    }

    public void OnGameResult(int result)
    {
        if (CheckRotate != null)
        {
            StopCoroutine(CheckRotate);
            CheckRotate = null;
            posMng.Rotateing = false;
        }

        //controller.Input.ResetInput();
        //controller.Input.ResetJoy();

        if (posMng.mActiveAction.Idx < CommonAction.Crouch || posMng.mActiveAction.Idx == CommonAction.GunIdle)
        {
            //posMng.ChangeAction(CommonAction.Idle);
            posMng.ChangeAction(CommonAction.Taunt, 0.1f);
            posMng.playResultAction = true;
        }
    }

    /// BUFF处理
    //名字, 类型, 值, 持续, 间隔, 持续类型, 拾取后带特效.
    public void AddBuf(Option ItemInfo, bool repeatAdd = false)
    {
        if (!BuffMng.Instance.BufDict.ContainsKey(ItemInfo.Idx))
        {
            Buff buf = new Buff();
            buf.refresh_type = ItemInfo.second[0].flag[4];
            buf.Id = ItemInfo.Idx;
            buf.Iden = ItemInfo.Identify;
            buf.refresh_delay = ItemInfo.second[0].flag[3];
            buf.effectIdx = ItemInfo.first[3].flag[1];//710特效代表角色中了蛊毒，移动-跑步动作会变化为中毒状态.
            buf.last_time = ItemInfo.first[4].flag[1];
            buf.type = (EBUFF_Type)ItemInfo.second[0].flag[2];
            buf.value = ItemInfo.second[0].flag[6];
            BuffMng.Instance.BufDict.Add(ItemInfo.Idx, buf);
        }
        BuffMng.Instance.BufDict[ItemInfo.Idx].AddUnit(this);
    }

    bool flag = false;
    public bool GetFlag { get { return flag; } }
    public Option GetFlagItem { get { return FlagItem; } }
    SFXEffectPlay flagEffect;
    int flagEffectIdx;
    Option FlagItem;
    public void SetFlag(Option f, int effectIdx)
    {
        flag = f == null ? false:true;
        FlagItem = f;
        if (f == null)
            return;
        if (flagEffect != null)
        {
            flagEffect.OnPlayAbort();
            flagEffect = null;
        }
        if (effectIdx != 0)
        {
            flagEffect = SFXLoader.Instance.PlayEffect(effectIdx, gameObject, false);
            flagEffectIdx = effectIdx;
        }
    }

    public void OnChangeWeaponType(int type)
    {
        if (type > 4 || type < 0)
            return;
        //炼化主手武器，每一个种类的武器有60个
        int weaponType = GetWeaponType();
        int targetWeapon = 0;
        switch (weaponType)
        {
            case 0://剑
                targetWeapon = 6 + type * 9;
                break;
            case 1://匕
                targetWeapon = 5 + type * 9;
                break;
            case 2://火枪
                targetWeapon = 3 + type * 9;
                break;
            case 3://飞镖
                targetWeapon = 1 + type * 9;
                break;
            case 4://锤子
                targetWeapon = 9 + type * 9;
                break;
            case 5://刀
                targetWeapon = 8 + type * 9;
                break;
            case 6://飞轮
                targetWeapon = 2 + type * 9;
                break;
            case 7://长枪
                targetWeapon = 7 + type * 9;
                break;
            case 8://双刺
                targetWeapon = 4 + type * 9;
                break;
            case 9://乾坤
                targetWeapon = 47 + (type <= 3 ? type : 0);//无圣诞节
                break;
            case 10://指虎
                targetWeapon = 51 + (type <= 3 ? type : 0);//无圣诞节
                break;
            case 11://忍刀
                targetWeapon = 55 + (type <= 3 ? type : 0);//无圣诞节
                break;
        }
        ChangeWeaponCode(targetWeapon);
    }

    public void DropWeapon()
    {
        DropMng.Instance.DropWeapon(this);
    }

    //被某个招式打中后，加入BUFF或者物品效果
    public void GetItem(int idx)
    {
        Option it = MenuResLoader.Instance.GetItemInfo(idx);
        GetItem(it);
    }

    //角色得到任意物品的处理，包括buff,炼化,气血，怒气等
    public void GetItem(Option ItemInfo)
    {
        if (ItemInfo.first[2].flag[1] != 0)
            SFXLoader.Instance.PlayEffect(ItemInfo.first[2].flag[1], gameObject, true);
        //考虑所有物品
        if (ItemInfo.second.Length != 0)
        {
            if (ItemInfo.first != null && ItemInfo.first.Length > 4 && ItemInfo.first[4].flag[1] != 0)
            {
                AddBuf(ItemInfo);
                //BUFF可以立即导致角色死亡
                CheckUnitDead();
            }
            else
            {
                //一次性物品,燕羽，无法使用
                switch (ItemInfo.second[0].flag[2])
                {
                    case 1://最大血量??不可能，最大血量是状态，是BUFF
                    case 4://防御 BUFF
                    case 3://攻击 BUFF
                    case 5://移动速度 BUFF
                    case 10://隐身 BUFF
                        Debug.LogError("ERROR ID");
                        break;
                    case 2://现有血量 加血，扣血道具, 燕羽-无效 主要是包子 微尘
                        Attr.AddHP(ItemInfo.second[0].flag[6]);
                        //Debug.LogError("skill done");
                        CheckUnitDead();
                        break;
                    case 6://现有怒气
                        AddAngry(ItemInfo.second[0].flag[6]);
                        break;
                }
            }

            if (Attr.IsPlayer)
            {
                if (FightWnd.Exist)
                    FightWnd.Instance.UpdatePlayerInfo();
            }
        }
        else
            OnChangeWeaponType(ItemInfo.first[3].flag[1]);
    }

    void CheckUnitDead()
    {
        if (Attr.Dead)
            OnDead();
    }

    //是否隐身
    public bool Stealth { get { return _Stealth; } }
    bool _Stealth;//隐身以后，装备/切换武器/都需要重新设置武器的透明度
    public void ChangeHide(bool hide)
    {
        _Stealth = hide;
        MeshRenderer[] mr = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < mr.Length; i++)
        {
            if (!mr[i].enabled)
                continue;
            for (int j = 0; j < mr[i].materials.Length; j++)
                mr[i].materials[j].SetFloat("_Alpha", _Stealth ? 0.2f : 1.0f);
        }

        SkinnedMeshRenderer[] mrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < mrs.Length; i++)
        {
            if (!mrs[i].enabled)
                continue;
            for (int j = 0; j < mrs[i].materials.Length; j++)
                mrs[i].materials[j].SetFloat("_Alpha", _Stealth ? 0.2f : 1.0f);
        }
    }

    public int GetSkillPose(int skill = 0)
    {
        int pose = skill;
        switch (GetWeaponType())
        {
            case 0:
                pose = 368;
                break;
            case 1:
                pose = 259;
                break;
            case 2:
                pose = 216;
                break;
            case 3:
                pose = 203;
                break;
            case 4:
                pose = 325;
                break;
            case 5:
                pose = 310;
                break;
            case 6:
                pose = 224;
                break;
            case 7:
                pose = 293;
                break;
            case 8:
                pose = 244;
                break;
            case 9:
                ChangeWeaponPos(2);
                pose = 451;
                break;
            case 10:
                pose = 421;
                break;
            case 11:
                pose = 474;
                break;
        }
        return pose;
    }

    //Coroutine PlayWeaponPoseCorout;
    //void TryPlayWeaponPose(int KeyMap)
    //{
    //    if (PlayWeaponPoseCorout != null)
    //        return;
    //    PlayWeaponPoseCorout = StartCoroutine(PlayWeaponPose(KeyMap));
    //}

    //IEnumerator PlayWeaponPose(int KeyMap)
    //{
        //List<VirtualInput> skill = VirtualInput.CalcPoseInput(KeyMap);
        //for (int i = 0; i < skill.Count; i++)
        //{
        //    controller.Input.OnKeyDown(skill[i].key, true);
        //    yield return 0;
        //    controller.Input.OnKeyUp(skill[i].key);
        //    yield return 0;
        //}
        //PlayWeaponPoseCorout = null;
    //}

    //0大绝，其他的就是pose号,仅主角用.
    public void PlaySkill(int skill = 0)
    {
        //技能0为当前武器绝招
        if (skill == 0)
        {
            if (AngryValue >= 100 || GameData.Instance.gameStatus.EnableInfiniteAngry)
            {
                //得到武器的大绝pose号码。
                AngryValue -= GameData.Instance.gameStatus.EnableInfiniteAngry ? 0 : 100;
                int skillPose = ActionInterrupt.Instance.GetSkillPose(this);
                if (skillPose != 0)
                {
                    posMng.ChangeAction(skillPose);
                }
            }
            else if (Attr.IsPlayer)
                U3D.InsertSystemMsg("怒气不足");
        }
        else if (skill == 1)
        {
            //当前武器小技能1
        }
        else if (skill == 2)
        {
            //当前武器小技能2
        }
    }
}
