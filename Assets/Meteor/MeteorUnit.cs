using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using protocol;
using Idevgame.GameState;
using Idevgame.Meteor.AI;
using UnityEngine.AI;
using UnityStandardAssets.ImageEffects;
using Excel2Json;

using System.IO;
using Idevgame.GameState.DialogState;

public enum NinjaState {
    None,//
    WaitPress,//等待按下跳跃蓄力
    Ready,//等待触发超级跳
    Interrupt,//打断无法继续，招式可以重置掉这个.(类似失误在空中没有接上超级跳，然后输入了一个动作，动作重置此状态后，可以继续使用超级跳)
}

public enum Direct {
    Back = 0,
    Front = 1,
    Right = 2,
    Left = 3
}

public enum EBUFF_ID {
    Drug = 10,//钝筋
    DrugEx = 13,//错骨，
}

public enum EBUFF_Type {
    MaxHP = 1,//最大气血+
    HP,//现有气血
    ATT,
    DEF,
    SPEED,
    ANG,
    HIDE = 10,
}

public class BuffMng:Singleton<BuffMng> {
    public SortedDictionary<int, Buff> BufDict = new SortedDictionary<int, Buff>();
    public void Clear() {
        BufDict.Clear();
    }

    //取得某对象是否拥有某BUFF
    public bool HasBuff(int id, MeteorUnit unit) {
        foreach (var each in BufDict) {
            if (each.Value.Units.ContainsKey(unit) && each.Value.Id == id)
                return true;
        }
        return false;
    }

    public bool HasBuff(EBUFF_Type type, MeteorUnit unit) {
        foreach (var each in BufDict) {
            if (each.Value.Units.ContainsKey(unit) && each.Value.type == type)
                return true;
        }
        return false;
    }

    //某个角色删除全部BUFF
    public void RemoveUnit(MeteorUnit unit) {
        foreach (var each in BufDict) {
            if (each.Value.Units.ContainsKey(unit)) {
                each.Value.ClearBuff(unit);
                each.Value.Units.Remove(unit);
            }
        }
    }
}

public class BuffContainer {
    public float refresh_tick;
    public float refresh_round_tick;
    public SFXEffectPlay effect;
}

public class Buff {
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
    public void AddUnit(MeteorUnit unit) {
        if (!Units.ContainsKey(unit)) {
            BuffContainer con = new BuffContainer();
            con.effect = effectIdx == 0 ? null : SFXLoader.Ins.PlayEffect(effectIdx, unit.gameObject, false);
            Units.Add(unit, con);
            DoBuff(unit);
        } else {
            //重复一次BUFF。要看这个BUFF是否可叠加,
            //生命上限，无法叠加
            //生命值，无法叠加
            //
        }

        Units[unit].refresh_tick = last_time / 10;
        Units[unit].refresh_round_tick = refresh_delay;
        if (refresh_type == 99999)//按固定时间0.1秒刷新.
            Units[unit].refresh_round_tick = (refresh_delay == 0 ? 0.1f : refresh_delay);//没有值就每0.1S刷新，否则就以值为刷新

        if (unit.Attr.IsPlayer) {
            if (FightState.Exist())
                FightState.Instance.UpdatePlayerInfo();
        }
    }

    public void Clear() {
        foreach (var each in Units) {
            if (each.Value != null)
                ClearBuff(each.Key);
        }
        Units.Clear();
    }

    //清除一个对象的某个BUFF，除了状态值都不需要处理,但是要把特效清理掉
    public void ClearBuff(MeteorUnit unit) {
        if (!Units.ContainsKey(unit))
            return;
        if (Units[unit].effect != null) {
            Units[unit].effect.OnPlayAbort();
            Units[unit].effect = null;
        }
        //战斗UI把BUFF元素清除掉
        //if (unit.Attr.IsPlayer)
        //    FightWnd.Instance.RemoveBuff(this);
        //else
        //    FightWnd.Instance.RemoveBuff(this, unit);
        switch (type) {
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
                unit.Attr.MultiplySpeed(100.0f / value);
                break;
        }
    }

    void DoBuff(MeteorUnit unit) {
        //无论如何都要执行一次BUFF
        switch (type) {
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
                unit.Attr.MultiplySpeed(value / 100.0f);
                break;
        }
    }

    List<MeteorUnit> unitRemoved = new List<MeteorUnit>();
    public void NetUpdate() {
        unitRemoved.Clear();
        switch (refresh_type) {
            case 1:
            case 99999://间隔多久刷一次，整体时间到了，删除对象
                foreach (var each in Units) {
                    each.Value.refresh_tick -= FrameReplay.deltaTime;
                    if (each.Value.refresh_tick <= 0.0f) {
                        unitRemoved.Add(each.Key);//整体时间到了，就不要算单轮了
                        continue;
                    }
                    each.Value.refresh_round_tick -= FrameReplay.deltaTime;
                    if (each.Value.refresh_round_tick <= 0.0f) {
                        each.Value.refresh_round_tick = refresh_delay;
                        DoBuff(each.Key);
                        if (each.Key.Attr.Dead) {
                            unitRemoved.Add(each.Key);
                            continue;
                        }
                    }

                    //要用订阅者模式把这里的耦合解除一下
                    if (FightState.Exist()) {
                        if (each.Key.Attr.IsPlayer)
                            FightState.Instance.UpdatePlayerInfo();
                        else if (!each.Key.SameCamp(Main.Ins.LocalPlayer) && GameStateMgr.Ins.gameStatus.ShowBlood)
                            FightState.Instance.UpdateMonsterInfo(each.Key);
                    } else if (ReplayState.Exist()) {
                        if (each.Key.Attr.IsPlayer)
                            ReplayState.Instance.UpdatePlayerInfo();
                        else if (!each.Key.SameCamp(Main.Ins.LocalPlayer) && GameStateMgr.Ins.gameStatus.ShowBlood)
                            ReplayState.Instance.UpdateMonsterInfo(each.Key);
                    }
                }
                break;
            case -1://状态，持续时间到了取消状态，且删除对象
                foreach (var each in Units) {
                    each.Value.refresh_tick -= FrameReplay.deltaTime;
                    if (each.Value.refresh_tick <= 0.0f)
                        unitRemoved.Add(each.Key);
                }
                break;
            case 0://不可能走这里，0代表不是BUFF
                break;
        }
        //把超时的BUFF都删除掉
        for (int i = 0; i < unitRemoved.Count; i++) {
            ClearBuff(unitRemoved[i]);
            Units.Remove(unitRemoved[i]);
            if (unitRemoved[i].Attr.Dead)
                unitRemoved[i].OnDead();
            if (FightState.Exist()) {
                if (unitRemoved[i].Attr.IsPlayer)
                    FightState.Instance.UpdatePlayerInfo();
                else if (unitRemoved[i] == Main.Ins.LocalPlayer.LockTarget && FightState.Exist())
                    FightState.Instance.UpdateMonsterInfo(unitRemoved[i]);
            } else if (ReplayState.Exist()) {
                if (unitRemoved[i].Attr.IsPlayer)
                    ReplayState.Instance.UpdatePlayerInfo();
                else if (unitRemoved[i] == Main.Ins.LocalPlayer.LockTarget && FightState.Exist())
                    ReplayState.Instance.UpdateMonsterInfo(unitRemoved[i]);
            }
        }
    }
}

public enum RushDirection {
    Front,
    Back,
    Left,
    Right,
}

public partial class MeteorUnit : NetBehaviour {
    public virtual bool IsDebugUnit() { return false; }
    public int ModelId;
    public int InstanceId;
    public Transform WeaponL;//右手骨骼
    public Transform WeaponR;
    public Transform ROOTNull;
    public Transform RootdBase;
    public Transform HeadBone;//头部骨骼.在自动目标存在时,头部骨骼朝向自动目标
    public Transform D_top;//头顶挂点.
    public EUnitCamp Camp = EUnitCamp.EUC_FRIEND;
    [SerializeField]
    public ActionManager ActionMgr;
    public CharacterLoader characterLoader;
    public MeteorController meteorController;
    public WeaponLoader weaponLoader;
    public float rotateTick = 0;//角色开始旋转后，计时，一定时间内没有再旋转，则视为不再旋转，跳出循环动作(待能切换动作时)
    //与主角的距离-的平方
    public float distance;
    //与主角的夹角
    public float angle;
    public float TargetWeight() {
        //以此判定-越小表示更该被设定为自动目标
        return distance + angle * angle;
    }

    public float RebornTick = 0;//复活需要在死亡后多久间隔
    public bool WaitReborn = false;//盟主模式-等待系统复活
    public StateMachine StateMachine;//状态机
    

    //单机关卡设置位置到路点.
    public void SetPosition(Vector3 position) {
        transform.position = position;
    }

    public void SetPosition(int spawnPoint) {
        if (spawnPoint >= CombatData.Ins.wayPoints.Count)
            return;
        transform.position = CombatData.Ins.wayPoints[spawnPoint].pos;
    }

    public void SetRotationImmediate(Quaternion quat) {
        transform.rotation = quat;
    }

    public void AIPause(bool pause, float t) {
        if (StateMachine != null)
            StateMachine.Pause(pause, t);
    }

    //public System.Action<InventoryItem> OnEquipChanged;
    //public MeteorUnit NGUIJoystick_skill_TargetUnit;//技能目标
    //public List<SkillInput> SkillList = new List<SkillInput>();
    //MeteorUnit wantTarget = null;//绿色目标.只有主角拥有。

    public bool Pause = false;
    Vector3 pos2 = Vector3.zero;
    public Vector3 mPos2d { get { pos2 = transform.position; pos2.y = 0; return pos2; } }
    public Vector3 mShootPivot {
        get {
            if (Mathf.Abs(D_top.transform.position.y - transform.position.y) <= 20) {
                return transform.position + Vector3.up * 5;
            }
            return mSkeletonPivot;
        }
    }
    //被射击中的点，低一点，否则部分动作的时候，没法击中
    public Vector3 mSkeletonPivot { get { return transform.position + CombatData.Ins.BodyHeight; } }
    public bool Crouching { get { return ActionMgr.mActiveAction.Idx == CommonAction.Crouch || (ActionMgr.mActiveAction.Idx >= CommonAction.CrouchForw && ActionMgr.mActiveAction.Idx <= CommonAction.CrouchBack); } }
    //攀爬
    public bool Climbing { get { return ActionMgr.mActiveAction.Idx == CommonAction.ClimbLeft || ActionMgr.mActiveAction.Idx == CommonAction.ClimbRight || ActionMgr.mActiveAction.Idx == CommonAction.ClimbUp; } }
    //左侧右侧蹬墙
    public bool ClimbJumping { get { return ActionMgr.mActiveAction.Idx == CommonAction.WallRightJump || ActionMgr.mActiveAction.Idx == CommonAction.WallLeftJump; } }
    //int mCacheLayerMask;
    public float ClimbingTime;//限制爬墙Y轴速度持续
    public bool Dead = false;
    public bool OnTopGround = false;//顶部顶着了,无法向上继续
    public bool OnGround = false;//控制器是否收到阻隔无法前进.踩在敌方或友方身体上也算
    public bool MoveOnGroundEx = false;//移动的瞬间，射线是否与地相聚不到4M。下坡的时候很容易离开地面
    public bool OnTouchWall = false;//贴着墙壁

    //本体跟随影子(同步的)
    public Vector3 ShadowPosition;
    public Quaternion ShadowRotation;

    public GameObject blobShadow;
    //MoveSpeed
    public float GetMoveSpeedScale() {
        return (CalcSpeed() / 100.0f);
    }

    public int ClimbingSpeed { get { return 1250; } }//这个值是慢慢调出来的，没有实际参考意义，不会1S就跑这么多
    public int ClimbingSpeedY { get { return 800; } }
    public int MoveSpeed { get { return (int)(Attr.Speed * GetMoveSpeedScale()); } }
    //远程近战武器，攻击距离
    public float AttackRange {
        get {
            if (U3D.IsSpecialWeapon(Attr.Weapon))
                return float.MaxValue;
            return CombatData.AttackRange;
        }
    }

    public float DeadZone = 0.25f;
    public bool WillDead {
        get {
            return Attr.hpCur <= (DeadZone * Attr.HpMax);
        }
    }

    public bool angryMax { get; set; }
    public int AngryValue {
        get {
            if (angryMax)
                return CombatData.ANGRYMAX;
            return Attr.AngryValue;
        }
        set {
            if (angryMax)
                Attr.AngryValue = CombatData.ANGRYMAX;
            else {
                Attr.AngryValue = Mathf.Clamp(value, 0, 100);
            }
            if (Attr.IsPlayer && FightState.Exist())
                FightState.Instance.UpdateAngryBar();
        }
    }

    //当前武器
    public int GetWeaponSubType() { return weaponLoader == null ? 0 : weaponLoader.WeaponSubType(); }
    public int GetWeaponType() { return weaponLoader == null ? -1 : weaponLoader.WeaponType(); }
    public int GetGuardPose(Direct direction) {
        switch ((EquipWeaponType)GetWeaponType()) {
            case EquipWeaponType.Brahchthrust:
                if (direction == Direct.Back)
                    return 52;
                else if (direction == Direct.Front)
                    return 53;
                else if (direction == Direct.Right)
                    return 54;
                else if (direction == Direct.Left)
                    return 55;
                break;
            case EquipWeaponType.Knife:
                if (direction == Direct.Back)//背后
                    return 56;
                else if (direction == Direct.Front)//正面
                    return 57;
                else if (direction == Direct.Right)//右侧
                    return 58;
                else if (direction == Direct.Left)//左侧
                    return 59;
                break;
            case EquipWeaponType.Sword:
                if (direction == Direct.Back)
                    return 60;
                else if (direction == Direct.Front)
                    return 61;
                else if (direction == Direct.Right)
                    return 62;
                else if (direction == Direct.Left)
                    return 63;
                break;
            case EquipWeaponType.Lance:
                if (direction == Direct.Back)
                    return 64;
                else if (direction == Direct.Front)
                    return 65;
                else if (direction == Direct.Right)
                    return 66;
                else if (direction == Direct.Left)
                    return 67;
                break;
            case EquipWeaponType.Blade:
                if (direction == Direct.Back)
                    return 68;
                else if (direction == Direct.Front)
                    return 69;
                else if (direction == Direct.Right)
                    return 70;
                else if (direction == Direct.Left)
                    return 71;
                break;
            case EquipWeaponType.Gloves:
                if (direction == Direct.Back)
                    return 490;//491防地
                else if (direction == Direct.Front)
                    return 491;
                else if (direction == Direct.Right)
                    return 492;
                else if (direction == Direct.Left)
                    return 493;
                break;
            case EquipWeaponType.Hammer:
                if (direction == Direct.Back)
                    return 72;
                else if (direction == Direct.Front)
                    return 73;
                else if (direction == Direct.Right)
                    return 74;
                else if (direction == Direct.Left)
                    return 75;
                break;
            case EquipWeaponType.NinjaSword:
                if (direction == Direct.Back)
                    return 516;
                else if (direction == Direct.Front)
                    return 517;
                else if (direction == Direct.Right)
                    return 518;
                else if (direction == Direct.Left)
                    return 519;
                break;
            case EquipWeaponType.HeavenLance:
                if (direction == Direct.Back)
                    return 494;
                else if (direction == Direct.Front)
                    return 495;
                else if (direction == Direct.Right)
                    return 496;
                else if (direction == Direct.Left)
                    return 497;
                break;
            case EquipWeaponType.Gun: Debug.LogError("Gun can't guard"); break;
            case EquipWeaponType.Dart: Debug.LogError("Dart can't guard"); break;
            case EquipWeaponType.Guillotines: Debug.LogError("Guillotines can't guard"); break;
        }
        return 0;
    }


    public SceneItemAgent TargetItem;//当前选择的
    public MeteorUnit LockTarget = null;//攻击目标.主角主动攻击敌方后，没解锁前，都以这个目标作为锁定攻击目标
    public MeteorUnit KillTarget = null;//追杀目标.
    public MeteorUnit FollowTarget = null;//跟随目标.
    public void SetLockedTarget(MeteorUnit target) {
        LockTarget = target;
    }

    protected new void OnDestroy() {
        base.OnDestroy();
        PersistDialogMgr.Ins.ExitStateByOwner(this);
        if (weaponLoader != null)
            weaponLoader.RemoveTrail();
    }

    public bool ExistDamage(MeteorUnit t) {
        return HurtUnit.ContainsKey(t);
    }

    public bool ExistDamage(SceneItemAgent t) {
        return HurtSceneItem.ContainsKey(t);
    }
    public bool SameCamp(MeteorUnit t) {
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

    public void Guard(bool guard, float time) {
        if (StateMachine != null)
            StateMachine.ChangeState(guard ? StateMachine.GuardState as State : StateMachine.WaitState as State, time);
        else {//主角
            if (guard) {
                Defence();
            } else {
                ReleaseDefence();
            }
        }
    }

    //初始化的时候，设置默认选择友军是谁，所有治疗技能，增益BUFF均默认释放给他
    void SelectFriend() {
        float dis = Attr.View / 2.0f;
        if (dis <= 10)
            dis = 100.0f;
        //int index = -1;
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++) {
            MeteorUnit unit = MeteorManager.Ins.UnitInfos[i];
            if (unit == this)
                continue;
            if (!SameCamp(unit))
                continue;
            if (unit.Dead)
                continue;
            float d = Vector3.Distance(transform.position, MeteorManager.Ins.UnitInfos[i].transform.position);
            if (dis > d) {
                dis = d;
                //index = i;
            }
        }
    }

    public bool HasBuff(int id) {
        return BuffMng.Ins.HasBuff(id, this);

    }
    //查看角色是否拥有某个类型的BUF
    public bool HasBuff(EBUFF_Type type) {
        return BuffMng.Ins.HasBuff(type, this);
    }

    bool canControlOnAir;//是跳跃到空中落下，还是被击飞到空中落下.击飞落下时是不能控制出招的.
    public bool CanActionOnAir() {
        return canControlOnAir;
    }

    //是否精确在地面，由控制器返回在地面标识
    public bool IsOnGroundEx() {
        bool ret = OnGround;
        return ActionMgr.Jump ? false : ret;
    }

    public bool IsOnGround() {
        bool ret = OnGround || MoveOnGroundEx;
        return ActionMgr.Jump ? false : ret;
    }

    public void SetGround(bool onground) {
        OnGround = onground;
        MoveOnGroundEx = onground;
    }

    public MonsterEx Attr;
    protected new void Awake() {
        base.Awake();
        //单场景启动.
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

    public override void NetUpdate() {
        if (!gameObject.activeInHierarchy)
            return;
        if (Pause) {
            return;
        }

        FrameUpdate();
        Interpolate();
    }

    public bool synced = false;//是否从网络同步过
    public void Interpolate() {
        if (!synced || Attr.IsPlayer)
            return;
        transform.position = Vector3.Lerp(transform.position, ShadowPosition, FrameReplay.deltaTime * 10);
        transform.rotation = Quaternion.Slerp(transform.rotation, ShadowRotation, FrameReplay.deltaTime * 10);
    }

    public void FrameUpdate() {
        if (Climbing)
            ClimbingTime += FrameReplay.deltaTime;
        else {
            ClimbingTime = 0;
            ActionMgr.ClimbFallTick = 0;
        }
        if (ActionMgr.Jump)
            ActionMgr.JumpTick += FrameReplay.deltaTime;
        if (ActionMgr.Rotateing) {
            rotateTick += FrameReplay.deltaTime;
        }
        keyM.Clear();
        keyM.AddRange(HurtUnit.Keys);
        removedM.Clear();
        foreach (var each in keyM) {
            HurtUnit[each] -= FrameReplay.deltaTime;
            if (HurtUnit[each] < 0.0f)
                removedM.Add(each);
        }
        for (int i = 0; i < removedM.Count; i++)
            HurtUnit.Remove(removedM[i]);
        keyS.Clear();
        keyS.AddRange(HurtSceneItem.Keys);
        removedS.Clear();
        foreach (var each in keyS) {
            HurtSceneItem[each] -= FrameReplay.deltaTime;
            if (HurtSceneItem[each] < 0.0f)
                removedS.Add(each);
        }

        for (int i = 0; i < removedS.Count; i++)
            HurtSceneItem.Remove(removedS[i]);

        //机关或者尖刺打到我.
        keyD.Clear();
        keyD.AddRange(attackDelay.Keys);
        removedD.Clear();
        foreach (var each in keyD) {
            attackDelay[each] -= FrameReplay.deltaTime;
            if (attackDelay[each] < 0.0f)
                removedD.Add(each);
        }

        for (int i = 0; i < removedD.Count; i++)
            attackDelay.Remove(removedD[i]);

        keyT.Clear();
        keyT.AddRange(touchDelay.Keys);
        removedT.Clear();
        foreach (var each in keyT) {
            touchDelay[each] -= FrameReplay.deltaTime;
            if (touchDelay[each] < 0.0f)
                removedT.Add(each);
        }

        for (int i = 0; i < removedT.Count; i++)
            touchDelay.Remove(removedT[i]);

        //动画帧和位置
        if (ActionMgr != null)
            ActionMgr.NetUpdate();

        //指令播放
        //if (U3D.IsMultiplyPlayer() || CombatData.Ins.Replay)
        //    ProcessCommand();

        //控制者更新
        if (meteorController != null)
            meteorController.NetUpdate();

        if (!CombatData.Ins.PauseAll) {
            if (StateMachine != null && gameObject.activeInHierarchy)
                StateMachine.Update();
        }

        if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer && !synced && !Attr.IsPlayer) {
            //联机还未同步信息前，不要处理速度带来的移动
        } else {
            ProcessVelocity();
        }
        //除了受击，防御，其他动作在有锁定目标下，都要转向锁定目标.
        if (LockTarget != null && ActionMgr.mActiveAction.Idx == CommonAction.Run) {
            if (StateMachine == null && GameStateMgr.Ins.gameStatus.AutoLock) {
                if (GetWeaponType() != (int)EquipWeaponType.Guillotines &&
                    GetWeaponType() != (int)EquipWeaponType.Gun &&
                    GetWeaponType() != (int)EquipWeaponType.Dart) {
                    MeteorUnit target = LockTarget;
                    float dis = Vector3.SqrMagnitude(target.mPos2d - this.mPos2d);
                    //小于一定距离后，不再改变朝向.否则一直抖动
                    if (dis > 400)
                        FaceToTarget(LockTarget);//抖动是因为招式忽略了角色间的碰撞,导致的离的太近
                }
            }
        }

        if (IsDebugUnit()) {
        } else {
            if (Attr.IsPlayer) {
                if (!meteorController.InputLocked && !Main.Ins.GameBattleEx.BattleFinished()) {
                    float yRotate = 0;
                    float yRotate1 = 0;
                    float yRotate2 = 0;
                    if (ActionMgr.CanRotateY) {
                    //读取手柄上的相机左旋转，相机右旋.
                    if (Main.Ins.JoyStick.isActiveAndEnabled) {
                        yRotate1 = Main.Ins.JoyStick.CameraAxisX * 5 * GameStateMgr.Ins.gameStatus.AxisSensitivity.x;
                    } else {
                        yRotate1 = NGUICameraJoystick.Ins.deltaLast.x * GameStateMgr.Ins.gameStatus.AxisSensitivity.x;
                    }
                        yRotate2 = Input.GetAxis("Mouse X") * 5 * GameStateMgr.Ins.gameStatus.AxisSensitivity.x;
                    }
                    yRotate = GameStateMgr.Ins.gameStatus.UseMouse ? yRotate2 : yRotate1;

                    float xRotate = 0;
                    float xRotate1 = 0;
                    float xRotate2 = 0;
                    if (Main.Ins.JoyStick.isActiveAndEnabled) {
                        xRotate1 = Main.Ins.JoyStick.CameraAxisY * 2 * GameStateMgr.Ins.gameStatus.AxisSensitivity.y;
                    } else {
                        xRotate1 = NGUICameraJoystick.Ins.deltaLast.y * GameStateMgr.Ins.gameStatus.AxisSensitivity.y;
                    }
                    xRotate2 = Input.GetAxis("Mouse Y") * 2 * GameStateMgr.Ins.gameStatus.AxisSensitivity.y;
                    xRotate = GameStateMgr.Ins.gameStatus.UseMouse ? xRotate2 : xRotate1;
                    if (xRotate != 0 || yRotate != 0) {
                        //if (CombatData.Ins.Replay) {
                        //    OnPlayerMouseDelta(xRotate, yRotate);
                        //} else if (U3D.IsMultiplyPlayer()) {
                        //    if (Attr.IsPlayer)
                        //        FrameSyncServer.Ins.PushMouseDelta(InstanceId, (int)xRotate, (int)yRotate);
                        //} else {
                            //精度截断到3位小数
                            OnPlayerMouseDelta(xRotate, yRotate);
                            //FrameSyncLocal.Ins.PushDelta(MeteorMsg.Command.Mouse, InstanceId, x, y);
                        //}
                    } else {
                        //if (CombatData.Ins.Replay) {

                        //} else if (U3D.IsMultiplyPlayer()) {
                        //    if (Attr.IsPlayer)
                        //        FrameSyncServer.Ins.PushMouseDelta(InstanceId, 0, 0);
                        //} else {
                        //    FrameSyncLocal.Ins.PushDelta(MeteorMsg.Command.Mouse, InstanceId, 0, 0);
                        //}
                    }

                    if (ActionMgr.Rotateing) {
                        CheckRotateEnd();
                    }
                }
            }
        }
    }

    protected void CheckRotateEnd() {
        if (rotateTick >= 0.2f) {
            OnCameraRotateEnd();
        }
    }

    //存储了这一帧.
    //public Vector2 mouse = Vector2.zero;
    //public Vector2 joy = Vector2.zero;
    //protected void ProcessCommand() {
    //    GameFrame command = FrameReplay.Instance.frameData;
    //    if (command != null) {
    //        //得到此玩家ID在这一帧的指令
    //        List<FrameCommand> cmd = new List<FrameCommand>();
    //        for (int i = 0; i < command.commands.Count; i++) {
    //            if (command.commands[i].playerId != InstanceId)
    //                continue;
    //            if (command.commands[i].LogicFrame != FrameReplay.Instance.LogicFrames)
    //                continue;
    //            //Debug.LogError("playerid:" + command.commands[i].playerId);
    //            cmd.Add(command.commands[i]);
    //        }
    //        //执行指令
    //        for (int i = 0; i < cmd.Count; i++) { 
    //            //取得该角色得操作指令，并且执行.
    //            switch (cmd[i].command) {
    //                case MeteorMsg.Command.Mouse:
    //                case MeteorMsg.Command.Joy:
    //                    //Debug.LogError("mouse move");
    //                    MouseEvent vec = ProtoBuf.Serializer.Deserialize<MouseEvent>(new System.IO.MemoryStream(cmd[i].Data));
    //                    if (cmd[i].command == MeteorMsg.Command.Mouse) {
    //                        mouse.x = vec.x / 1000.0f;
    //                        mouse.y = vec.y / 1000.0f;
    //                    } else if (cmd[i].command == MeteorMsg.Command.Joy) {
    //                        joy.x = vec.x / 1000.0f;
    //                        joy.y = vec.y / 1000.0f;
    //                    }
    //                    break;
    //                case MeteorMsg.Command.Key:
    //                    KeyEvent kEvent = ProtoBuf.Serializer.Deserialize<KeyEvent>(new System.IO.MemoryStream(cmd[i].Data));
    //                    if (kEvent.state == (int)VirtualKeyState.Press)
    //                        meteorController.Input.SyncKeyDown((EKeyList)kEvent.key);
    //                    else if (kEvent.state == (int)VirtualKeyState.Release)
    //                        meteorController.Input.SyncKeyUp((EKeyList)kEvent.key);
    //                    else if (kEvent.state == (int)VirtualKeyState.Pressing)
    //                        meteorController.Input.SyncKeyPressing((EKeyList)kEvent.key);
    //                    break;
    //            }
    //        }
    //    }
    //}

    public void OnPlayerMouseDelta(float x, float y) {
        if (y != 0)
            Main.Ins.LocalPlayer.SetOrientation(y, false);
        if (this == Main.Ins.LocalPlayer) {
            if (Main.Ins.MainCamera == Main.Ins.CameraFollow.m_Camera) {
                Main.Ins.CameraFollow.OnTargetRotate(x, y);
            } else {
                //观察相机的俯仰
                Main.Ins.CameraFree.OnTargetRotate(x, y);
            }
        }
    }

    
    //角色跳跃高度74，是以脚趾算最低点，倒过来算出dbase,则需要减去差值。
    public bool IgnoreGravity = false;
    //物体动量(质量*速度)的改变,等于物体所受外力冲量的总和.这就是动量定理
    public Vector3 ImpluseVec = Vector3.zero;//冲量，ft = mat = mv2 - mv1,冲量在时间内让物体动量由A变化为B

    //增加一点速度，让其爬墙，或者持续跳
    public void AddYVelocity(float y) {
        //return;
        ImpluseVec.y += y;
        if (ImpluseVec.y > CombatData.ClimbLimitMax)
            ImpluseVec.y = CombatData.ClimbLimitMax;
    }

    public void ResetYVelocity() {
        ImpluseVec.y = 0;
    }

    public void IgnoreGravitys(bool ignore) {
        IgnoreGravity = ignore;
        //招式忽略重力时，等招式结束的时候，这个速度开始向下加速到落地
        if (IgnoreGravity)
            ImpluseVec.y = 0;
    }

    //处理地面摩擦力, scale地面摩擦力倍数，空中时摩擦力为0.2倍
    void ProcessFriction(float scale = 1.0f) {
        if (Climbing)
            return;
        float d = scale * CombatData.groundFriction * FrameReplay.deltaTime;
        if (ImpluseVec.x != 0) {
            if (ImpluseVec.x > 0) {
                ImpluseVec.x -= d;
                if (ImpluseVec.x < 0)
                    ImpluseVec.x = 0;
            } else {
                ImpluseVec.x += d;
                if (ImpluseVec.x > 0)
                    ImpluseVec.x = 0;
            }
        }
        if (ImpluseVec.z != 0) {
            if (ImpluseVec.z > 0) {
                ImpluseVec.z -= d;
                if (ImpluseVec.z < 0)
                    ImpluseVec.z = 0;
            } else {
                ImpluseVec.z += d;
                if (ImpluseVec.z > 0)
                    ImpluseVec.z = 0;
            }
        }
    }

    public void ResetWorldVelocity(bool reset) {
        if (reset) {
            ImpluseVec.x = ImpluseVec.x * 0.3f;
            ImpluseVec.z = ImpluseVec.z * 0.3f;
        }
    }

    public virtual void ProcessVelocity() {
        if (charController == null || !charController.enabled)
            return;
        //Debug.Log("f:" + Time.frameCount);
        int gScale = CombatData.Ins.gGravity;
        Vector3 v;
        v.x = ImpluseVec.x * FrameReplay.deltaTime;
        //过渡帧不计算重力.
        v.y = IgnoreGravity ? 0 : ImpluseVec.y * FrameReplay.deltaTime;
        v.z = ImpluseVec.z * FrameReplay.deltaTime;
        v += ActionMgr.moveDelta;
        if (v != Vector3.zero)
            Move(v);
        if (IgnoreGravity) {

        } else {
            ImpluseVec.y = ImpluseVec.y - gScale * FrameReplay.deltaTime;
            if (ImpluseVec.y < -CombatData.SpeedMax)
                ImpluseVec.y = -CombatData.SpeedMax;
        }

        if (IsOnGround()) {
            if (Climbing) {

            } else {
                ImpluseVec.y = -CombatData.Ins.gOnGroundCheck;
                if (OnGround && MoveOnGroundEx)//如果在地面,且没有悬空，或者顶到天花板，那么应用摩擦力.
                    ProcessFriction();
                else if (MoveOnGroundEx)
                    ProcessFriction(0.9f);//没贴着地面，还是要有摩擦力，否则房顶滑动太厉害
                else if (!Floating) {
                    ProcessFriction();//没有浮空，浮空时会被峭壁边缘推动，不要在推动时用摩擦力，否则就推不动了
                }
            }
        } else if (OnTouchWall) {
            if (Climbing) {

            } else
                ProcessFriction(0.3f);//爬墙或者在墙面滑动
        }
    }

    public bool CanBreakout() {
        return (AngryValue >= CombatData.ANGRYBURST);
    }

    //专门用来播放左转，右转动画的，直接面对角色不要调用这个。
    public void SetOrientation(float orient, bool ai = true) {
        if (ai && ActionMgr != null && ActionMgr.onhurt)
            return;
        Quaternion quat = Quaternion.identity;
        try {
            quat = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + orient, transform.eulerAngles.z);
        } catch (Exception exp) {
            Debug.LogError(exp.Message);
            Debug.DebugBreak();
        }
        transform.rotation = quat;
        if (meteorController.Input.OnInputMoving()) {
            //if (!Attr.IsPlayer)
            //    Debug.Log("SetOrientation returned");
            return;
        }
        OnCameraRotateStart();
        if (GetWeaponType() == (int)EquipWeaponType.Gun) {
            if (orient < 0.0f) {
                if (Crouching || ActionMgr.mActiveAction.Idx == CommonAction.GunIdle) {
                    if (ActionMgr.mActiveAction.Idx == CommonAction.Crouch || ActionMgr.mActiveAction.Idx == CommonAction.GunIdle
                    || (ActionMgr.mActiveAction.Idx == CommonAction.CrouchRight && meteorController.Input.mInputVector.x == 0))
                        ActionMgr.ChangeAction(CommonAction.CrouchLeft, 0.1f);
                } else
                if (ActionManager.IsReadyAction(ActionMgr.mActiveAction.Idx) || ActionMgr.mActiveAction.Idx == CommonAction.Idle
                    || (ActionMgr.mActiveAction.Idx == CommonAction.WalkRight && meteorController.Input.mInputVector.x == 0))
                        ActionMgr.ChangeAction(CommonAction.WalkLeft, 0.1f);
            } else
            if (orient > 0.0f) {
                if (Crouching || ActionMgr.mActiveAction.Idx == CommonAction.GunIdle) {
                    if (ActionMgr.mActiveAction.Idx == CommonAction.Crouch || ActionMgr.mActiveAction.Idx == CommonAction.GunIdle
                    || (ActionMgr.mActiveAction.Idx == CommonAction.CrouchLeft && meteorController.Input.mInputVector.x == 0))
                        ActionMgr.ChangeAction(CommonAction.CrouchRight, 0.1f);
                } else
                if (ActionManager.IsReadyAction(ActionMgr.mActiveAction.Idx) || 
                    ActionMgr.mActiveAction.Idx == CommonAction.Idle || 
                    (ActionMgr.mActiveAction.Idx == CommonAction.WalkLeft && meteorController.Input.mInputVector.x == 0))
                        ActionMgr.ChangeAction(CommonAction.WalkRight, 0.1f);
            }
        } else {
            if (orient < 0.0f) {
                if (Crouching) {
                    if (ActionMgr.mActiveAction.Idx == CommonAction.Crouch
                    || (ActionMgr.mActiveAction.Idx == CommonAction.CrouchRight && meteorController.Input.mInputVector.x == 0))
                        ActionMgr.ChangeAction(CommonAction.CrouchLeft, 0.1f);
                } else
                if (ActionManager.IsReadyAction(ActionMgr.mActiveAction.Idx) || ActionMgr.mActiveAction.Idx == CommonAction.Idle
                    || (ActionMgr.mActiveAction.Idx == CommonAction.WalkRight && meteorController.Input.mInputVector.x == 0))
                        ActionMgr.ChangeAction(CommonAction.WalkLeft, 0.1f);
            } else
            if (orient > 0.0f) {
                if (Crouching) {
                    if (ActionMgr.mActiveAction.Idx == CommonAction.Crouch
                    || (ActionMgr.mActiveAction.Idx == CommonAction.CrouchLeft && meteorController.Input.mInputVector.x == 0))
                            ActionMgr.ChangeAction(CommonAction.CrouchRight, 0.1f);
                } else
                if (ActionManager.IsReadyAction(ActionMgr.mActiveAction.Idx) || ActionMgr.mActiveAction.Idx == CommonAction.Idle
                    || (ActionMgr.mActiveAction.Idx == CommonAction.WalkLeft && meteorController.Input.mInputVector.x == 0))
                        ActionMgr.ChangeAction(CommonAction.WalkRight, 0.1f);
            }
        }
    }


    public bool IsFacetoTarget(MeteorUnit target) {
        return Vector3.Dot(-1 * transform.forward, (new Vector3(target.transform.position.x, 0, target.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized) >= 0;
    }

    public void Kill(MeteorUnit unit) {
        if (StateMachine != null) {
            if (unit != null && unit.isActiveAndEnabled) {
                KillTarget = unit;
                if (KillTarget != null && !KillTarget.Dead) {
                    StateMachine.ChangeState(StateMachine.KillState);
                }
            }
        }
    }

    public void FaceToTarget(Vector3 target) {
        Vector3 vdiff = (transform.position - target).normalized;
        vdiff.y = 0;
        if (vdiff == Vector3.zero)
            return;
        transform.rotation = Quaternion.LookRotation(new Vector3(vdiff.x, 0, vdiff.z), Vector3.up);
    }

    public void FaceToTarget(MeteorUnit unit) {
        if (unit == this)
            return;
        //UnityEngine.Debug.LogError(string.Format("{0} faceto :{1}", name, unit.name));
        FaceToTarget(unit.transform.position);
    }

    SFXEffectPlay leaderVfx;
    public void SetAsLeader(SFXEffectPlay leader) {
        leaderVfx = leader;
    }

    public CharacterController charController;
    public void Init(int modelIdx, MonsterEx mon = null, bool updateModel = false) {
        DeadZone = 0.25f;
        Pause = false;
        Vector3 vec = transform.position;
        Quaternion rotation = transform.rotation;
        
        tag = "meteorUnit";
        ModelId = modelIdx;
        Attr = mon;
        IgnoreGravity = true;
        IgnorePhysical = false;
        name = Attr.Name;
        gameObject.layer = LayerManager.Player;

        //单机模式下有ai
        if (CombatData.Ins.GLevelMode <= LevelMode.CreateWorld) {
            StateMachine = Attr.IsPlayer ? null : new StateMachine();
            if (StateMachine != null) {
                StateMachine.Init(this);
            }
            angryMax = GameStateMgr.Ins.gameStatus.EnableInfiniteAngry && Attr.IsPlayer;
        }

        if (meteorController == null)
            meteorController = new MeteorController();

        if (updateModel) {
            //切换模型把BUFF删掉，特效需要重新播放
            BuffMng.Ins.RemoveUnit(this);
            HurtList.Clear();
            if (flag)
                flagEffect.OnPlayAbort();

            if (characterLoader != null) {
                GameObject.Destroy(characterLoader.rootBone.parent.gameObject);
                GameObject.Destroy(characterLoader.Skin.gameObject);
                characterLoader = null;
            }
        }

        if (characterLoader == null)
            characterLoader = new CharacterLoader();
        if (ActionMgr == null)
            ActionMgr = new ActionManager();

        if (updateModel) {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
        characterLoader.LoadCharactor(ModelId, transform);
        try {
            ActionMgr.Init(this);
        } catch {
            Debug.LogError("unit id:" + ModelId);
        }
        WeaponR = NodeHelper.Find("d_wpnR", characterLoader.rootBone.gameObject).transform;
        WeaponL = NodeHelper.Find("d_wpnL", characterLoader.rootBone.gameObject).transform;
        ROOTNull = NodeHelper.Find("b", characterLoader.rootBone.gameObject).transform;
        HeadBone = NodeHelper.Find("bau_Head", characterLoader.rootBone.gameObject).transform;
        D_top = NodeHelper.Find("d_top", characterLoader.rootBone.gameObject).transform;
        RootdBase = characterLoader.rootBone;

        weaponLoader = gameObject.GetComponent<WeaponLoader>();
        if (updateModel) {
            Destroy(weaponLoader);
            weaponLoader = null;
        }
        if (weaponLoader == null)
            weaponLoader = gameObject.AddComponent<WeaponLoader>();
        weaponLoader.Init(this);

        if (meteorController != null)
            meteorController.Init(this);

        InventoryItem itWeapon = GameStateMgr.Ins.MakeEquip(Attr.Weapon);
        if (itWeapon == null) {
            Debug.LogError("需要加载的武器不存在:" + Attr.Weapon);
            itWeapon = GameStateMgr.Ins.MakeEquip(306);//裴旻剑
            Attr.Weapon = 306;
        }
        weaponLoader.EquipWeapon(itWeapon);

        ActionMgr.ChangeAction(CommonAction.Idle, 0);
        //换主角模型用
        if (updateModel) {
            SetPosition(vec);
            SetRotationImmediate(rotation);
        }

        //换主角模型用
        if (flag)
            SetFlag(FlagItem, flagEffectIdx);

        //如果有队长标志特效，还原
        if (IsLeader && updateModel) {
            leaderVfx.OnPlayAbort();
            leaderVfx = SFXLoader.Ins.PlayEffect(Camp == EUnitCamp.EUC_Meteor ? "vipblue.ef": "vipred.ef", gameObject, false);
        }

        //切换模型，锁定特效等重建
        if (updateModel) {
            if (Main.Ins.GameBattleEx.autoEffect != null) {
                if (Main.Ins.GameBattleEx.autoTarget == this) {
                    Main.Ins.GameBattleEx.autoEffect.OnPlayAbort();
                    Main.Ins.GameBattleEx.autoEffect = SFXLoader.Ins.PlayEffect("Track.ef", gameObject);
                }
            }

            if (Main.Ins.GameBattleEx.lockedEffect != null) {
                if (Main.Ins.GameBattleEx.lockedTarget == this || Main.Ins.GameBattleEx.lockedTarget2 == this) {
                    Main.Ins.GameBattleEx.lockedEffect.OnPlayAbort();
                    Main.Ins.GameBattleEx.lockedEffect = SFXLoader.Ins.PlayEffect("lock.ef", gameObject);
                }
            }
        }

        GunReady = false;
        IsPlaySkill = false;

        //初始化阴影
        if (blobShadow == null) {
            blobShadow = GameObject.Instantiate(Resources.Load("BlobShadow"), transform, false) as GameObject;
            blobShadow.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            blobShadow.transform.localScale = new Vector3(2, 2, 2);
            blobShadow.transform.localPosition = new Vector3(0, 0, 0);
        }

        charController = gameObject.GetComponent<CharacterController>();
        //ModelData modeldata = DataMgr.Ins.GetModelData(ModelId);
        //float height = modeldata != null ? modeldata.Height : 32;
        //float pivot = modeldata != null ? modeldata.Pivot: 16f;
        //一定要让角色倒地的时候，还在地面上一点高度，否则很多招式就无法再打到这个倒地的角色了
        if (charController == null)
            charController = gameObject.AddComponent<CharacterController>();
        if (charController != null) {
            charController.center = new Vector3(0, 16, 0);
            charController.height = 32;
            charController.radius = 8f;
            charController.stepOffset = 6.8f;//以钟乳洞中间的高台为准
        }
        ShowAttackBox();
    }

    public bool IsPlaySkill { get; set; }
    public void EquipWeapon(InventoryItem it) {
        IndicatedWeapon = it;
        ChangeWeapon();
    }

    InventoryItem IndicatedWeapon;
    public void ChangeWeaponCode(int weaponCode) {
        InventoryItem w = GameStateMgr.Ins.MakeEquip(weaponCode);
        if (w == null)
            return;
        if (weaponLoader != null)
            weaponLoader.UnEquipWeapon();

        Attr.Weapon = weaponCode;
        IndicatedWeapon = GameStateMgr.Ins.MakeEquip(Attr.Weapon);

        InventoryItem toEquip = IndicatedWeapon;
        if (toEquip != null && weaponLoader != null)
            weaponLoader.EquipWeapon(toEquip);
        //if (OnEquipChanged != null)
        //    OnEquipChanged.Invoke(toEquip);
        IndicatedWeapon = null;
        //没有自动目标，攻击目标，不许计算自动/锁定目标，无转向
        //血滴子即使有自动目标，切换时也应该先解锁
        if (Attr.IsPlayer && (GetWeaponType() == (int)EquipWeaponType.Gun || GetWeaponType() == (int)EquipWeaponType.Dart || GetWeaponType() == (int)EquipWeaponType.Guillotines))
            Main.Ins.GameBattleEx.Unlock();
        ShowAttackBox();
    }

    public void ChangeWeaponPos(WeaponPos pose) {
        if (weaponLoader != null)
            weaponLoader.ChangeWeaponPos(pose);
    }

    NinjaState ninjaState;
    public void UpdateNinjaState(NinjaState st) {
        if (GetWeaponType() == (int)EquipWeaponType.NinjaSword && !IsOnGround()) {
            if (ninjaState == NinjaState.None) {
                if (st == NinjaState.WaitPress) {
                    ninjaState = st;
                }
            } else if (ninjaState == NinjaState.WaitPress) {
                if (st == NinjaState.Ready) {
                    ninjaState = NinjaState.Ready;
                }
            } else if (ninjaState == NinjaState.Ready) {//当蓄力等待触发无限飞时，如果抬起，则此次被打断
                if (st == NinjaState.WaitPress) {
                    ninjaState = NinjaState.Interrupt;
                }
            } else if (ninjaState == NinjaState.Interrupt) {
                if (st == NinjaState.None) {
                    ninjaState = st;
                }
            }
        }
    }

    //松开跳跃键，如果使用的忍刀，且在松开前，有成功的招式输入，则可以无限跳跃
    public bool SuperJump() {
        if (GetWeaponType() == (int)EquipWeaponType.NinjaSword) {
            //跳跃键现在状态是按下，按下的时间超过0.3S
            if (ninjaState == NinjaState.Ready) {
                if (meteorController.Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_PressingEnough)) {
                    Jump(1, CommonAction.Jump);
                    return true;
                }
            }
        }
        return false;
    }

    //移动速度/非动作速度
    public int CalcSpeed() {
        if (weaponLoader == null || weaponLoader.GetCurrentWeapon() == null || weaponLoader.GetCurrentWeapon().Info() == null)
            return 100;
        if (weaponLoader.GetCurrentWeapon().Info().Speed == 0)
            return 100;
        return weaponLoader.GetCurrentWeapon().Info().Speed;
    }

    public int CalcDamage() {
        return weaponLoader.GetCurrentWeapon().Info().Damage;
    }

    public int CalcDef() {
        return weaponLoader.GetCurrentWeapon().Info().Def;
    }

    public int GetNextWeaponType() {
        if (Attr.Weapon2 == 0)
            return -1;
        InventoryItem it = GameStateMgr.Ins.MakeEquip(Attr.Weapon2);
        if (it != null)
            return it.Info().SubType;
        return -1;
    }

    public void SyncWeapon(int weapon1, int weapon2) {
        if (weaponLoader == null && Attr.Weapon == weapon1) {
            Attr.Weapon2 = weapon2;
            return;
        }
        weaponLoader.UnEquipWeapon();
        Attr.Weapon = weapon1;
        Attr.Weapon2 = weapon2;
        IndicatedWeapon = GameStateMgr.Ins.MakeEquip(Attr.Weapon);
        if (IndicatedWeapon != null && weaponLoader != null)
            weaponLoader.EquipWeapon(IndicatedWeapon);
        IndicatedWeapon = null;
        ShowAttackBox();
    }

    //丢掉主武器，切换到武器2
    public void DropAndChangeWeapon() {
        if (Attr.Weapon2 != 0) {
            if (weaponLoader != null)
                weaponLoader.UnEquipWeapon();
            Attr.Weapon = Attr.Weapon2;
            Attr.Weapon2 = 0;
            IndicatedWeapon = GameStateMgr.Ins.MakeEquip(Attr.Weapon);
        }
        if (IndicatedWeapon != null && weaponLoader != null)
            weaponLoader.EquipWeapon(IndicatedWeapon);
        IndicatedWeapon = null;
        //没有自动目标，攻击目标，不许计算自动/锁定目标，无转向
        if (Attr.IsPlayer &&
            (GetWeaponType() == (int)EquipWeaponType.Gun ||
             GetWeaponType() == (int)EquipWeaponType.Dart ||
             GetWeaponType() == (int)EquipWeaponType.Guillotines))
            Main.Ins.GameBattleEx.Unlock();
        if (StateMachine != null)
            StateMachine.OnChangeWeapon();
        ShowAttackBox();
    }

    public void ChangeNextWeapon() {
        //Debug.Log("ChangeNextWeapon");
        if (Attr.Weapon2 != 0) {
            if (weaponLoader != null)
                weaponLoader.UnEquipWeapon();
            int weapon = Attr.Weapon;
            Attr.Weapon = Attr.Weapon2;
            Attr.Weapon2 = weapon;
            IndicatedWeapon = GameStateMgr.Ins.MakeEquip(Attr.Weapon);
        }
        if (IndicatedWeapon != null && weaponLoader != null)
            weaponLoader.EquipWeapon(IndicatedWeapon);
        IndicatedWeapon = null;
        //没有自动目标，攻击目标，不许计算自动/锁定目标，无转向
        if (Attr.IsPlayer &&
            (GetWeaponType() == (int)EquipWeaponType.Gun ||
             GetWeaponType() == (int)EquipWeaponType.Dart ||
             GetWeaponType() == (int)EquipWeaponType.Guillotines))
            Main.Ins.GameBattleEx.Unlock();
        if (StateMachine != null)
            StateMachine.OnChangeWeapon();
        ShowAttackBox();
    }

    public void ShowAttackBox() {
        if (U3D.showBox) {
            for (int i = 0; i < HurtList.Count; i++) {
                BoundsGizmos.Instance.AddCollider(HurtList[i]);
            }
            BoundsGizmos.Instance.AddColliders(WeaponL.GetComponentsInChildren<Collider>());
            BoundsGizmos.Instance.AddColliders(WeaponR.GetComponentsInChildren<Collider>());
            BoundsGizmos.Instance.AddTransform(transform);
        }
    }

    public void ChangeWeapon() {
        ActionMgr.ChangeAction(IsOnGround() ? CommonAction.ChangeWeapon : CommonAction.AirChangeWeapon, 0.1f);
    }

    public void OnCrouch() {
        ActionMgr.ChangeAction(CommonAction.Crouch, 0.1f);
    }

    public void Defence() {
        int weapon = GetWeaponType();
        if (weapon == (int)EquipWeaponType.Gun || weapon == (int)EquipWeaponType.Dart || weapon == (int)EquipWeaponType.Guillotines)
            return;
        ActionMgr.ChangeAction(CommonAction.Defence, 0.1f);
    }

    //火枪是否进入预备状态，这个状态下，除了跳跃，爆气，受击之外都会继续回到预备姿势，而不进入默认的IDLE
    public bool GunReady { get; set; }
    public void SetGunReady(bool ready) {
        GunReady = ready;
    }


    public void DoBreakOut() {
        if (AngryValue >= 60) {
            ActionMgr.ChangeAction(CommonAction.BreakOut, 0);
            ActionMgr.LockTime(0);
            AngryValue -= 60;
            if (Attr.IsPlayer) {
                if (FightState.Exist())
                    FightState.Instance.UpdateAngryBar();
            }
        }
    }

    //处理上升途中的跳跃键，查看周围有无可伸腿踩的点，如果有，则判断方向，切换姿势，并给一个速度
    //面向左45，面向，面向右45，查看是否
    public void ProcessJump2() {
        if (Climbing) {
            if (ActionMgr.mActiveAction.Idx == CommonAction.ClimbUp) {
                //倒跳
                SetWorldVelocity((transform.forward + transform.up) * CombatData.Jump2Velocity);
                Jump3(CommonAction.JumpBack);
            } else if (ActionMgr.mActiveAction.Idx == CommonAction.ClimbRight) {
                //像右
                SetWorldVelocity((-1 * transform.right + transform.up) * CombatData.Jump2Velocity);
                Jump3(CommonAction.WallLeftJump);
            } else if (ActionMgr.mActiveAction.Idx == CommonAction.ClimbLeft) {
                SetWorldVelocity((transform.right + transform.up) * CombatData.Jump2Velocity);
                Jump3(CommonAction.WallRightJump);
            }
        } else {
            RaycastHit hit;
            Vector3 vec = Vector3.zero;
            Vector3 nearest = Vector3.zero;
            float length = 100.0f;
            int idx = -3;//0左侧，1中间，2右侧
            for (int i = -1; i < 2; i++) {
                if (Physics.Raycast(transform.position, (Quaternion.AngleAxis(i * 45, Vector3.up) * (-1 * transform.forward)), out hit, charController.radius + 5, LayerManager.SceneMask)) {
                    vec = transform.position - hit.point;
                    vec.y = 0;
                    // = vec.magnitude;
                    if (length > vec.magnitude) {
                        length = vec.magnitude;
                        nearest = vec;
                        idx = i;
                    }
                }
            }

            nearest = Vector3.Normalize(nearest);
            Vector3 dir = nearest * CombatData.Jump2Velocity;
            dir.y = CalcVelocity(CombatData.Jump2Velocity);
            switch (idx) {
                case -1:
                    SetWorldVelocity(dir);
                    Jump3(CommonAction.WallLeftJump);
                    break;
                case 0:
                    SetWorldVelocity(dir);
                    Jump3(CommonAction.JumpBack);
                    break;
                case 1:
                    SetWorldVelocity(dir);
                    Jump3(CommonAction.WallRightJump);
                    break;
            }
        }
    }
    //处理贴墙轻功时,给XY初速度，再给Y初速度
    public void ProcessTouchWallJump(bool minVelocity) {
        //Debug.LogError("hit Wall Jump");
        Vector3 vecNormal = transform.forward + transform.up;
        if (ActionMgr.mActiveAction.Idx == CommonAction.ClimbUp) {
            //倒跳
            SetWorldVelocity((vecNormal) * CombatData.Jump2Velocity);
            Jump3(CommonAction.JumpBack);
        } else if (ActionMgr.mActiveAction.Idx == CommonAction.ClimbRight) {
            //右 -2为惯性模拟-不能让速度直接替换之前爬墙的速度，否则看起来不符合物理规律
            SetWorldVelocity((-2 * transform.right + hitNormal + 2 * transform.up) * CombatData.Jump2Velocity);
            Jump3(CommonAction.WallLeftJump);
        } else if (ActionMgr.mActiveAction.Idx == CommonAction.ClimbLeft) {
            SetWorldVelocity((2 * transform.right + hitNormal + 2 * transform.up) * CombatData.Jump2Velocity);
            Jump3(CommonAction.WallRightJump);
        }
    }

    //3个场景要考虑
    //1:浮空站在陡峭墙壁上,无法站立的那种
    //2:站立在地表的边缘,一侧身体浮空
    //3:站在凹陷的坑内    
    public bool ProcessFall(bool lockMove = true, bool change = true) {
        //是否需要推动，要看这个推力方向上是否有阻碍，如果有阻碍，就不要推动了.
        Vector3 vec = transform.position - hitPoint;
        vec.y = 0;
        //恰好碰撞点，就是自己所处的坐标,把角色向前推一定距离.
        if (vec == Vector3.zero)
            vec = transform.forward;
        float speed = CombatData.WallForce * FrameReplay.deltaTime;
        
        if (floatTick + 0.5f < FrameReplay.Ins.time) {
            floatTick = FrameReplay.Ins.time;
            //没碰到其他角色时，才执行被推开之类的
            if (contactTarget == null) {
                AddWorldVelocity(Vector3.Normalize(vec) * speed);
                if (lockMove)
                    meteorController.LockMove(true);//在恢复到正常状态前，不可以再输入方向键.
                if (change)
                    ActionMgr.ChangeAction(CommonAction.JumpFall, 0.1f);//短时间内落地姿势
                return true;
            }
        } else {
            //没碰到其他角色时，才执行被推开之类的
            if (contactTarget == null) {
                AddWorldVelocity(Vector3.Normalize(vec) * speed);
            }
        }
        return false;
    }

    float floatTick = -1.0f;//浮空时刻
    RaycastHit hit;//射线打到了谁身上，如果是角色，那么在processfall时，不要处理速度
    bool Floating = false;//是否浮空，往脚底发射线
    void UpdateFlags(CollisionFlags flag) {
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

        if (OnGround || OnTopGround || OnTouchWall) {
            if (ActionMgr.JumpTick != 0.0f) {
                ActionMgr.CanAdjust = false;
                ActionMgr.AirAttacked = false;
            }
        }

        //减少射线发射次数.
        if (Physics.SphereCast(transform.position + Vector3.up * 2, 0.5f, Vector3.down, out hit, 1000, LayerManager.AllSceneMask)) {
            MoveOnGroundEx = hit.distance <= 4f;
            //Debug.Log(string.Format("distance:{0}", hit.distance));
            Floating = hit.distance >= 8.0f;
            if (blobShadow != null) {
                blobShadow.transform.position = hit.point + new Vector3(0, 0.2f, 0);
                blobShadow.transform.forward = hit.normal;
            }
        } else {
            MoveOnGroundEx = false;
            Floating = true;
            if (blobShadow != null) {
                blobShadow.transform.position = new Vector3(10000, 10000, 10000);
            }
        }

        if (OnGround) {
            //无法真正意义的表明在地面，踩在其他角色头顶或真身体上也算
            //检测脚底是否踩住地面了
            //Y轴速度下降到速度超过能爬墙的速度.停止攀爬.被墙壁弹开.
            if (Climbing) {
                //爬墙
                if (!MoveOnGroundEx && ImpluseVec.y < CombatData.yClimbEndLimit) {
                    ActionMgr.ClimbFallTick += FrameReplay.deltaTime;
                    if (ActionMgr.ClimbFallTick > CombatData.ClimbFallLimit) {
                        //Debug.LogError("爬墙速度低于最低速度-爬墙落下-持续半秒");
                        ActionMgr.ClimbFallTick = 0.0f;
                        ProcessFall(true);
                    }
                } else if (MoveOnGroundEx) {
                    //Debug.LogError("爬墙碰到地面-落到地面");
                    ProcessFall(true);
                    //ActionMgr.ChangeAction(CommonAction.JumpFall, 0.1f);//短时间内落地姿势
                } else {
                    //Debug.LogError("爬墙中。。。");
                    ProcessFall(true);
                }
            } else if (OnTouchWall && Floating && FrameReplay.Ins.time - floatTick >= 0.5f)//贴墙浮空，被墙壁推开
              {
                if (ActionMgr.mActiveAction.Idx == CommonAction.Idle ||
                    ActionMgr.mActiveAction.Idx == CommonAction.WalkLeft ||
                    ActionMgr.mActiveAction.Idx == CommonAction.Run ||
                    ActionMgr.mActiveAction.Idx == CommonAction.RunOnDrug ||
                        ActionMgr.mActiveAction.Idx == CommonAction.WalkRight ||
                        ActionMgr.mActiveAction.Idx == CommonAction.WalkBackward ||
                        ActionMgr.mActiveAction.Idx == CommonAction.Jump ||
                        ActionMgr.mActiveAction.Idx == CommonAction.JumpLeft ||
                        ActionMgr.mActiveAction.Idx == CommonAction.JumpRight ||
                        ActionMgr.mActiveAction.Idx == CommonAction.JumpBack ||
                        ActionMgr.mActiveAction.Idx == CommonAction.JumpLeftFall ||
                        ActionMgr.mActiveAction.Idx == CommonAction.JumpRightFall ||
                        ActionMgr.mActiveAction.Idx == CommonAction.JumpBackFall ||
                        ActionMgr.mActiveAction.Idx == CommonAction.JumpFallOnGround) {
                    ProcessFall(true, false);
                }
            } else if (Floating) {
                ProcessFall(true, false);
            } else {
            }
        } else {
            if (OnTouchWall) {
                //碰到墙壁
                if (!MoveOnGroundEx) {
                    //却没碰到地面.
                    if (ImpluseVec.y > 0) {
                        //向上跳，轻功处理。??
                        if (Climbing) {
                        } else {
                            //只有当前跳时,或者爬墙时.或者从墙壁弹开时，可以继续爬墙.|| ClimbJumping
                            if (ActionMgr.JumpTick >= CombatData.JumpTimeLimit &&
                                (ActionMgr.mActiveAction.Idx == CommonAction.Jump || Climbing || ActionMgr.mActiveAction.Idx == CommonAction.JumpFallOnGround && Floating) &&
                                ImpluseVec.y > 100.0f &&
                                ActionMgr.CheckClimb)//速度最少要达到多少才能轻功
                            {
                                //3条射线，-5°面向 5°左边近就调用右爬，中间则上爬，右边近则左爬.
                                //Debug.LogError("轻功开始");
                                ActionMgr.CheckClimb = false;//单次爬墙不重复检测
                                ActionMgr.ClimbFallTick = 0.0f;
                                float left = 100;
                                float middle = 100;
                                float right = 100;
                                if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-15, Vector3.up) * -transform.forward, out hit, charController.radius + 5, LayerManager.SceneMask))
                                    left = Vector3.Distance(hit.point, transform.position);
                                if (Physics.Raycast(transform.position, -transform.forward, out hit, charController.radius + 5, LayerManager.SceneMask))
                                    middle = Vector3.Distance(hit.point, transform.position);
                                if (Physics.Raycast(transform.position, Quaternion.AngleAxis(15, Vector3.up) * -transform.forward, out hit, charController.radius + 5, LayerManager.SceneMask))
                                    right = Vector3.Distance(hit.point, transform.position);
                                float fMin = Mathf.Min(left, middle, right);
                                if (fMin != 100) {
                                    if (fMin == left)
                                        ActionMgr.ChangeAction(CommonAction.ClimbRight, 0.1f);
                                    else if (fMin == right)
                                        ActionMgr.ChangeAction(CommonAction.ClimbLeft, 0.1f);
                                    else if (fMin == middle)
                                        ActionMgr.ChangeAction(CommonAction.ClimbUp, 0.1f);
                                }
                            }
                        }

                    } else//下落
                    if (Climbing) {
                        //爬墙
                        if (!MoveOnGroundEx && ImpluseVec.y < CombatData.yClimbEndLimit) {
                            ActionMgr.ClimbFallTick += FrameReplay.deltaTime;
                            if (ActionMgr.ClimbFallTick > CombatData.ClimbFallLimit) {
                                //Debug.LogError("爬墙速度低于最低速度-爬墙落下");
                                ActionMgr.ClimbFallTick = 0.0f;
                                ProcessFall();
                            }
                        } else if (MoveOnGroundEx) {
                            //Debug.LogError("爬墙碰到地面-落到地面");
                            ActionMgr.ChangeAction(CommonAction.JumpFall, 0.1f);
                        }
                    } else //落地时候碰到墙壁.给一个反向的推力,
                      {
                        //!!!!贴墙落下，不能站在墙壁上，必须被弹开，否则城墙直接跳就可以上去
                        if (ActionMgr.mActiveAction.Idx == CommonAction.Idle ||
                            ActionMgr.mActiveAction.Idx == CommonAction.Run ||
                            ActionMgr.mActiveAction.Idx == CommonAction.RunOnDrug ||
                            ActionMgr.mActiveAction.Idx == CommonAction.WalkLeft ||
                            ActionMgr.mActiveAction.Idx == CommonAction.WalkRight ||
                            ActionMgr.mActiveAction.Idx == CommonAction.WalkBackward ||
                            ActionMgr.mActiveAction.Idx == CommonAction.JumpLeft ||
                            ActionMgr.mActiveAction.Idx == CommonAction.JumpRight ||
                            ActionMgr.mActiveAction.Idx == CommonAction.JumpBack ||
                            ActionMgr.mActiveAction.Idx == CommonAction.JumpLeftFall ||
                            ActionMgr.mActiveAction.Idx == CommonAction.JumpRightFall ||
                            ActionMgr.mActiveAction.Idx == CommonAction.JumpBackFall ||
                            ActionMgr.mActiveAction.Idx == CommonAction.JumpFallOnGround) {
                            //Debug.LogError("被墙壁轻微推开，避免悬挂在墙壁上");
                            ProcessFall();
                        }
                    }
                } else {
                    //碰到墙壁，也碰到地面（离地面不足4M）。贴着墙壁走，没什么需要处理的.
                }
            } else if (Climbing) {
                //爬墙过程中忽然没贴着墙壁了???直接落下
                //Debug.LogError("爬墙没有贴着墙壁-结束爬墙");
                meteorController.LockMove(true);
                ActionMgr.QueueAction(CommonAction.JumpFall);
            } else if (Floating) {
                //与四周毫无接触，浮空，切换为跳跃动作.151-152-180
                if (ActionMgr.mActiveAction.Idx == CommonAction.Run ||
                    ActionMgr.mActiveAction.Idx == CommonAction.Idle ||
                    ActionMgr.mActiveAction.Idx == CommonAction.RunOnDrug ||
                    ActionMgr.mActiveAction.Idx == CommonAction.WalkLeft ||
                    ActionMgr.mActiveAction.Idx == CommonAction.WalkRight ||
                    ActionMgr.mActiveAction.Idx == CommonAction.WalkBackward) {
                    ActionMgr.ChangeAction(CommonAction.JumpFall, 0.1f);
                }
            }
        }

        //如果Y轴速度向下，但是已经接触地面了
        if (ImpluseVec.y <= 0 && !IgnoreGravity) {
            if (MoveOnGroundEx || OnGround) {
                if (ActionMgr.mActiveAction.Idx == CommonAction.JumpFallOnGround) {
                    //如果在跳跃落地锁帧过程，如果与墙壁接触，但是未触底，悬挂着
                    if (Floating) {
                        //ProcessFall();
                        ActionMgr.ChangeAction(0, 0.1f);
                    } else {
                        //是切换到待机，还是被墙壁给推开，或者继续轻工
                        ActionMgr.ChangeAction(0, 0.1f);
                    }
                } else 
                if ((ActionMgr.mActiveAction.Idx == CommonAction.Jump || ActionMgr.mActiveAction.Idx == CommonAction.JumpLeft || ActionMgr.mActiveAction.Idx == CommonAction.JumpRight || ActionMgr.mActiveAction.Idx == CommonAction.JumpBack)) {
                    ActionMgr.ChangeAction(ActionMgr.mActiveAction.Idx + 1, 0.1f);
                } else if ((ActionMgr.mActiveAction.Idx == CommonAction.JumpFall || ActionMgr.mActiveAction.Idx == CommonAction.JumpLeftFall || ActionMgr.mActiveAction.Idx == CommonAction.JumpRightFall || ActionMgr.mActiveAction.Idx == CommonAction.JumpBackFall)) {
                    if (Floating && OnGround && !MoveOnGroundEx) {
                        //ProcessFall();
                        //ActionMgr.ChangeAction(CommonAction.JumpFall, 0.1f);
                    } else {
                        ActionMgr.ChangeAction(0, 0.1f);//如果跳跃落下动作时，已经接触地面，切换为待机
                    }
                }
            }
        }

        //如果撞到天花板了.
        if (OnTopGround)
            if (ImpluseVec.y > 0)
                ImpluseVec.y = 0;
    }

    public void Move(Vector3 trans) {
        if (charController != null) {
            if (charController != null && charController.enabled) {
                contactTarget = null;//如果在这次的Move中发生碰撞，记录下碰撞到的对象
                objCollider = null;
                CollisionFlags collisionFlags = charController.Move(trans);
                UpdateFlags(collisionFlags);
            } else
                SetPosition(transform.position + trans);
        } else {
            //本来是其他组件的，取消了.NavMeshAgent
        }
    }

    public void AddWorldVelocityExcludeY(Vector3 vec) {
        ImpluseVec.x += vec.x;
        ImpluseVec.z += vec.z;
        ImpluseVec.x = Mathf.Clamp(ImpluseVec.x, -CombatData.ClimbLimitMax, CombatData.ClimbLimitMax);
        ImpluseVec.z = Mathf.Clamp(ImpluseVec.z, -CombatData.ClimbLimitMax, CombatData.ClimbLimitMax);
    }

    public void SetWorldVelocityExcludeY(Vector3 vec) {
        ImpluseVec.x = vec.x;
        ImpluseVec.z = vec.z;
        ImpluseVec.x = Mathf.Clamp(ImpluseVec.x, -CombatData.ClimbLimitMax, CombatData.ClimbLimitMax);
        ImpluseVec.z = Mathf.Clamp(ImpluseVec.z, -CombatData.ClimbLimitMax, CombatData.ClimbLimitMax);
    }

    public void SetWorldVelocity(Vector3 vec) {
        //Debug.LogError("vec1");
        ImpluseVec.x = vec.x;
        ImpluseVec.y = vec.y;
        ImpluseVec.z = vec.z;
    }

    public void AddWorldVelocity(Vector3 vec) {
        //Debug.LogError("vec2");
        ImpluseVec.x += vec.x;
        ImpluseVec.y += vec.y;
        ImpluseVec.z += vec.z;
        Utility.Clamp(ref ImpluseVec, -CombatData.SpeedMax, CombatData.SpeedMax);
    }

    //设置世界坐标系的速度,z向人物面前，x向人物右侧
    public void SetJumpVelocity(Vector2 velocityM) {
        float z = 0;
        if (velocityM.y > 0) {
            z = velocityM.y * CombatData.JumpVelocityForward;
        } else {
            z = velocityM.y * CombatData.JumpVelocityOther;
        }
        float x = velocityM.x * CombatData.JumpVelocityOther;
        Vector3 vec = z * -1 * transform.forward + x * -1 * transform.right;
        ImpluseVec.z = vec.z;
        ImpluseVec.x = vec.x;
    }

    public void SetVelocity(float z, float x) {
        Vector3 vec = z * -1 * transform.forward + x * -1 * transform.right;
        ImpluseVec.z = vec.z;
        ImpluseVec.x = vec.x;
    }

    public float CalcVelocity(float h) {
        //vt2 - v02 = 2AS;
        float ret = Mathf.Sqrt(2 * h * CombatData.Ins.gGravity);
        if (ret > CombatData.SpeedMax)
            ret = CombatData.SpeedMax;
        return ret;
    }


    //踏墙壁跳跃,y为正常跳跃高度倍数.
    public void Jump2(float y, int act = CommonAction.Jump) {
        canControlOnAir = true;
        OnGround = false;
        ImpluseVec.y = CalcVelocity(y);
        ActionMgr.JumpTick = 0.0f;
        ActionMgr.CanAdjust = true;
        ActionMgr.CheckClimb = true;
        ActionMgr.ChangeAction(act, 0.1f);
        meteorController.Input.ResetInput();
        ninjaState = NinjaState.None;
    }

    public void Jump3(int act = CommonAction.Jump) {
        canControlOnAir = true;
        OnGround = false;
        ActionMgr.JumpTick = 0.0f;
        ActionMgr.CanAdjust = true;
        ActionMgr.CheckClimb = true;
        ActionMgr.ChangeAction(act, 0.1f);
        meteorController.Input.ResetInput();
        ninjaState = NinjaState.None;
        meteorController.LockMove(true);
    }

    //小跳版本0.12高度
    //中跳版本0.32高度
    public void Jump(float ShortScale, int act = CommonAction.Jump) {
        canControlOnAir = true;
        OnGround = false;
        //float ShortScale = ShortJump ? 0.32f:1.0f;
        if (StateMachine != null)
            ShortScale = 1.0f;
        float h = CombatData.JumpLimit * ShortScale;
        ImpluseVec.y = CalcVelocity(h);
        ActionMgr.JumpTick = 0.0f;
        ActionMgr.CanAdjust = true;
        ActionMgr.CheckClimb = true;
        ActionMgr.AirAttacked = false;
        ActionMgr.ChangeAction(act, 0.1f);
        //IgnoreGravity = true;
        meteorController.Input.ResetInput();
        ninjaState = NinjaState.None;
    }

    public void ReleaseDefence() {
        if ((ActionMgr.mActiveAction.Idx >= CommonAction.BrahchthrustDefence
              && ActionMgr.mActiveAction.Idx <= CommonAction.HammerDefence) ||
              (ActionMgr.mActiveAction.Idx >= CommonAction.ZhihuDefence
              && ActionMgr.mActiveAction.Idx <= CommonAction.RendaoDefence)) {
            //Debug.LogError("release defence");
            ActionMgr.ChangeAction(CommonAction.Idle, 0.1f);
        }
    }

    MeteorUnit RebornTarget = null;
    SFXEffectPlay RebornEffect = null;
    //附近有队友挂了，队长可以复活
    public bool HasRebornTarget() {
        //创建房间-盟主-死斗-无法复活队友
        if (CombatData.Ins.GLevelMode == LevelMode.CreateWorld) {
            if (CombatData.Ins.GGameMode == GameMode.MENGZHU || CombatData.Ins.GGameMode == GameMode.SIDOU)
                return false;
        }
        float dis = CombatData.RebornRange;
        int index = -1;
        //没法碰到已经挂的角色，还是遍历看谁最近，1队最多8人,队友7人
        for (int i = 0; i < MeteorManager.Ins.DeadUnits.Count; i++) {
            MeteorUnit unit = MeteorManager.Ins.DeadUnits[i];
            if (unit == this)
                continue;
            if (!SameCamp(unit))
                continue;
            if (!unit.Dead)
                continue;
            float d = Vector3.Distance(transform.position, unit.transform.position);
            if (dis > d) {
                dis = d;
                index = i;
            }
        }
        return index != -1;
    }

    public void SelectRebornTarget() {
        //创建房间-盟主-死斗-无法复活队友
        if (CombatData.Ins.GLevelMode == LevelMode.CreateWorld || U3D.IsMultiplyPlayer()) {
            if (CombatData.Ins.GGameMode == GameMode.MENGZHU || CombatData.Ins.GGameMode == GameMode.SIDOU)
                return;
        }
        RebornTarget = null;
        float dis = CombatData.RebornRange;
        int index = -1;
        for (int i = 0; i < MeteorManager.Ins.DeadUnits.Count; i++) {
            MeteorUnit unit = MeteorManager.Ins.DeadUnits[i];
            if (unit == this)
                continue;
            if (!SameCamp(unit))
                continue;
            if (!unit.Dead)
                continue;
            float d = Vector3.Distance(transform.position, unit.transform.position);
            if (dis > d) {
                dis = d;
                index = i;
                RebornTarget = unit;
            }
        }
        if (RebornTarget != null) {
            RebornEffect = SFXLoader.Ins.PlayEffect("ReBorn.ef", RebornTarget.transform.position, true);
        }
    }

    public void RebornFriend() {
        if (RebornTarget != null) {
            RebornTarget.OnReborn(0.3f);
            RebornTarget = null;
        }
        if (RebornEffect != null) {
            GameObject.Destroy(RebornEffect.gameObject);
            RebornEffect = null;
        }
    }

    public void ResetPosition() {
        if (CombatData.Ins.GLevelMode == LevelMode.CreateWorld) {
            if (CombatData.Ins.GGameMode == GameMode.MENGZHU) {
                //16个点
                transform.position = CombatData.Ins.GLevelSpawn[CombatData.Ins.SpawnIndex];
                CombatData.Ins.SpawnIndex++;
                CombatData.Ins.SpawnIndex %= 16;
            } else if (CombatData.Ins.GGameMode == GameMode.ANSHA || CombatData.Ins.GGameMode == GameMode.SIDOU) {
                //2个队伍8个点.
                if (Camp == EUnitCamp.EUC_FRIEND) {
                    transform.position = CombatData.Ins.GCampASpawn[CombatData.Ins.CampASpawnIndex];
                    CombatData.Ins.CampASpawnIndex++;
                    CombatData.Ins.CampASpawnIndex %= 8;
                } else if (Camp == EUnitCamp.EUC_ENEMY) {
                    transform.position = CombatData.Ins.GCampASpawn[CombatData.Ins.CampBSpawnIndex];
                    CombatData.Ins.CampBSpawnIndex++;
                    CombatData.Ins.CampBSpawnIndex %= 8;
                }
            }
        } else if (CombatData.Ins.GLevelMode == LevelMode.SinglePlayerTask) {
            if (Attr.SpawnPoint < CombatData.Ins.wayPoints.Count)
                transform.position = CombatData.Ins.wayPoints[Attr.SpawnPoint].pos;
            else
                transform.position = CombatData.Ins.wayPoints[0].pos;
        } else if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer) {
            if (CombatData.Ins.GGameMode == GameMode.MENGZHU) {
                //16个点
                transform.position = CombatData.Ins.GLevelSpawn[Attr.SpawnPoint];
            } else if (CombatData.Ins.GGameMode == GameMode.ANSHA || CombatData.Ins.GGameMode == GameMode.SIDOU) {
                //2个队伍8个点.
                if (Camp == EUnitCamp.EUC_FRIEND) {
                    transform.position = CombatData.Ins.GCampASpawn[Attr.SpawnPoint];
                } else if (Camp == EUnitCamp.EUC_ENEMY) {
                    transform.position = CombatData.Ins.GCampASpawn[Attr.SpawnPoint];
                }
            }
        }
    }

    //被复活.
    public void OnReborn(float max) {
        ResetPosition();
        //取消黑白屏
        if (Attr.IsPlayer) {
            if (Main.Ins.MainCamera != null) {
                Grayscale gray = Main.Ins.MainCamera.GetComponent<Grayscale>();
                if (gray != null) {
                    gray.enabled = false;
                }
            }
        }
        Dead = false;
        ActionMgr.OnReborn();
        EnableAI(true);
        meteorController.LockInput(false);
        meteorController.LockMove(false);
        MeteorManager.Ins.UnitInfos.Add(this);
        MeteorManager.Ins.DeadUnits.Remove(this);
        Attr.OnReborn(max);
        AngryValue = 0;
        charController.enabled = true;
        MeteorManager.Ins.PhysicalIgnore(this, false);
        MoveOnGroundEx = false;
        OnGround = false;
        IgnoreGravity = false;
        SetWorldVelocity(new Vector3(0, -100, 0));
        if (Attr.IsPlayer) {
            if (FightState.Exist())
                FightState.Instance.UpdatePlayerInfo();
            if (ReplayState.Exist())
                ReplayState.Instance.UpdatePlayerInfo();
        }
        U3D.InsertSystemMsg(name + "进入战场");
        Pause = false;
    }

    //盟主模式下的自动复活.
    public void RebornUpdate() {
        RebornTick += FrameReplay.deltaTime;
        if (RebornTick >= 5.0f) {
            OnReborn(1.0f);
            RebornTick = 0.0f;
        }
    }

    //暗杀模式，是否是队长，队长是一方阵营进入战场的第一个角色.
    public bool IsLeader { get; set; }
    public void OnDead(MeteorUnit killer = null) {
        if (!Dead) {
            if (Attr.IsPlayer && GameStateMgr.Ins.gameStatus.Undead)
                return;
            Dead = true;

            //盟主模式，
            //联机-暗杀/死斗.
            if (CombatData.Ins.GGameMode == GameMode.MENGZHU || U3D.IsMultiplyPlayer() && (CombatData.Ins.GGameMode == GameMode.ANSHA || CombatData.Ins.GGameMode == GameMode.SIDOU)) {
                RebornTick = 0;
            }

            if (killer == null) {
                U3D.InsertSystemMsg(string.Format("{0}死亡", name));
            } else {
                U3D.InsertSystemMsg(string.Format("{0}击败{1}", killer.name, name));
            }
            ActionMgr.ChangeAction(CommonAction.Dead, 0.1f);
            ActionMgr.OnDead();
            Attr.ReduceHp(Attr.hpCur);
            EnableAI(false);
            BuffMng.Ins.RemoveUnit(this);
            MeteorManager.Ins.OnUnitDead(this);
            Main.Ins.GameBattleEx.OnUnitDead(this, killer);
            if (Attr.IsPlayer) {
                if (FightState.Exist()) {
                    FightState.Instance.UpdatePlayerInfo();
                }
                if (ReplayState.Exist()) {
                    ReplayState.Instance.UpdatePlayerInfo();
                }
                if (Main.Ins.MainCamera != null) {
                    Grayscale gray = Main.Ins.MainCamera.GetComponent<Grayscale>();
                    if (gray == null) {
                        gray = Main.Ins.MainCamera.gameObject.AddComponent<Grayscale>();
                    }
                    gray.enabled = true;
                }
            }
            if (Attr.IsPlayer && NGUICameraJoystick.Ins)
                NGUICameraJoystick.Ins.ResetJoystick();
        }
    }

    public void EnableAI(bool enable) {
        if (StateMachine != null) {
            StateMachine.Enable(enable);
        }
    }

    public void EnableUpdate(bool enable) {
        Pause = !enable;
    }

    public void AddAngry(int angry) {
        int ang = AngryValue + angry;
        AngryValue = Mathf.Clamp(ang, 0, CombatData.ANGRYMAX);
    }

    //其他单位挂了.或者自己挂了。
    public void OnUnitDead(MeteorUnit deadunit) {
        if (deadunit == null)//自己挂了
        {
            SFXEffectPlay[] play = GetComponents<SFXEffectPlay>();
            for (int i = 0; i < play.Length; i++)
                play[i].OnPlayAbort();
            return;
        }
        if (LockTarget == deadunit && LockTarget != null)
            LockTarget = null;

        if (StateMachine != null)
            StateMachine.OnUnitDead(deadunit);
    }

    //特殊招式时，关闭碰撞，让角色互相穿透,
    //设置角色间characterController不互相碰撞.
    public bool IgnorePhysical;
    public void PhysicalIgnore(MeteorUnit unit, bool ignore) {
        if (charController == null || unit.charController == null) {
            return;
        }
        Physics.IgnoreCollision(charController, unit.charController, ignore);
    }

    Vector3 hitPoint;//最近一次碰撞的点.用这个点和法线来算一些轻功的东西
    Vector3 hitNormal;//碰撞面法线
    MeteorUnit contactTarget;//这一帧移动后发生碰撞的角色
    GameObject objCollider;//这一帧其他物件与我相撞
    public void OnControllerColliderHit(ControllerColliderHit hit) {
        hitPoint = hit.point;
        hitNormal = hit.normal;
        objCollider = hit.gameObject;
        if (hit.gameObject.transform.root.tag.Equals("meteorUnit")) {
            MeteorUnit hitUnit = hit.gameObject.transform.root.GetComponent<MeteorUnit>();
            contactTarget = hitUnit;
            Vector3 vec = hitUnit.transform.position - transform.position;
            vec.y = 0;
            //在防御中.不受推挤.
            if (hitUnit.ActionMgr != null && hitUnit.ActionMgr.onDefence)
                return;
            if (hitUnit.MoveOnGroundEx) {
                hitUnit.AddWorldVelocity(Vector3.Normalize(vec) * 20);
            }
        } else if (hit.gameObject.transform.root.tag.Equals("SceneItemAgent")) {
            SceneItemAgent agent = hit.gameObject.GetComponentInParent<SceneItemAgent>();
            if (agent != null && agent.HasDamage() && (!attackDelay.ContainsKey(agent))) {
                if (!Attr.Dead) {
                    OnDamage(null, agent.DamageValue());
                    attackDelay[agent] = 2.0f;
                }
            }
        } else {
            if (StateMachine != null) {
                StateMachine.CheckStatus();
            }
        }
    }

    void IgnoreCollision(MeteorUnit unit, Collider box) {
        //攻击盒与受击盒之间不要碰撞
        if (unit == this || unit.SameCamp(this)) {
            for (int i = 0; i < unit.HurtList.Count; i++) {
                Physics.IgnoreCollision(box, unit.HurtList[i], true);
            }
        }
    }

    public void IgnoreOthers(Collider box) {
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++) {
            MeteorUnit unit = MeteorManager.Ins.UnitInfos[i];
            IgnoreCollision(unit, box);
        }
    }

    public void WeaponReturned(int poseIdx) {
        //219等待回收武器.
        //扔出武器后，就会等待收回武器219，但是如果还没有到这个动作武器就返回了。那么会让
        if (characterLoader != null && ActionMgr != null && ActionMgr.mActiveAction != null) {
            if (Main.Ins.AppInfo.MeteorVersion.Equals("1.07")) {
                if (ActionMgr.mActiveAction.Idx == (poseIdx + 1)) {
                    ActionMgr.SetLoop(false);
                } else {
                    ActionMgr.LinkEvent(poseIdx + 1, PoseEvt.WeaponIsReturned);
                }
            } else if (Main.Ins.AppInfo.MeteorVersion.Equals("9.07")) {
                if (ActionMgr.mActiveAction.Idx == 219)
                    ActionMgr.SetLoop(false);
                else
                    ActionMgr.LinkEvent(219, PoseEvt.WeaponIsReturned);
            }
        }
    }

    public AttackDes CurrentDamage { get { return damage; } }
    AttackDes damage;
    Dictionary<SceneItemAgent, float> HurtSceneItem = new Dictionary<SceneItemAgent, float>();
    Dictionary<MeteorUnit, float> HurtUnit = new Dictionary<MeteorUnit, float>();
    void ChangeLayer(BoxCollider co, bool detectAll) {
        if (detectAll) {
            if (co.gameObject.layer != LayerManager.DetectAll) {
                co.gameObject.layer = LayerManager.DetectAll;
                FightBox fb = co.gameObject.GetComponent<FightBox>();
                if (fb == null) {
                    fb = co.gameObject.AddComponent<FightBox>();
                }
                if (CurrentDamage == null)
                    Debug.LogError("error");
                fb.Init(this, CurrentDamage);
                fb.enabled = true;
            }
        }
        else {
            if (co.gameObject.layer != LayerManager.Bone) {
                co.gameObject.layer = LayerManager.Bone;
                FightBox fb = co.gameObject.GetComponent<FightBox>();
                if (fb != null) {
                    fb.enabled = false;
                    Destroy(fb);
                }
            }
        }
    }

    public void ChangeAttack(AttackDes attack) {
        if (attack != null && attack.PoseIdx < 200)
            return;
        if (Main.Ins.GameBattleEx == null)
            return;
        if (damage == attack && attack != null)
            return;
        damage = attack;
        if (damage == null) {
            //HurtUnit.Clear();
            weaponLoader.ChangeAttack(false);
            for (int i = 0; i < HurtList.Count; i++) {
                ChangeLayer(HurtList[i], false);
            }
            for (int i = 0; i < sfxList.Count; i++)
                sfxList[i].ChangeAttack(false);
            //HurtSceneItem.Clear();
            return;
        }

        if (!IsOnGround()) {
            ActionMgr.AirAttacked = true;
        }

        if (Attr.IsPlayer && LockTarget == null && Main.Ins.GameBattleEx.autoTarget != null) {
            if (GetWeaponType() == (int)EquipWeaponType.Gun || GetWeaponType() == (int)EquipWeaponType.Dart || GetWeaponType() == (int)EquipWeaponType.Guillotines) {
                //远程武器不能锁定
            } else {
                LockTarget = Main.Ins.GameBattleEx.autoTarget;
                Main.Ins.GameBattleEx.ChangeLockedTarget(LockTarget);
            }
        }

        //遍历受击盒根据攻击定义，生成相应的攻击盒。
        //身体的部位作为攻击盒实际上效果很差--先去掉试试
        for (int i = 0; i < HurtList.Count; i++) {
            bool containsHurtBox = attack.bones.Contains(HurtList[i].name);
            //把层次设置为既可受击又可攻击，同时加上攻击盒定义，且告知这个盒子是用来做
            ChangeLayer(HurtList[i], containsHurtBox);
            if (containsHurtBox)
                HurtList[i].GetComponent<FightBox>().ChangeAttack(true);
            //HurtList[i].ChangeAttack(true);
        }

        //如果包含武器和特效.
        bool containsWeapon = attack.bones.Contains("weapon");
        weaponLoader.ChangeAttack(containsWeapon);

        bool containsEffect = attack.bones.Contains("effect");
        for (int i = 0; i < sfxList.Count; i++)
            sfxList[i].ChangeAttack(containsEffect);
        

        //枪械射击之类,1,枪，无轨迹，2，飞镖，自由落体轨迹 3，飞轮，贝塞尔曲线/B样条线轨迹.
        //这些武器的大招，一定是带effect攻击特效的.所以可以通过伤害里的骨骼列表判断是否属于
        if (attack.bones.Count == 0)
            Main.Ins.GameBattleEx.AddDamageCheck(this, attack);
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
    public void Attack(MeteorUnit other) {
        canControlOnAir = false;
        HurtUnit.Add(other, CombatData.DamageDetectDelay);
    }

    //成功碰撞到场景物件-一定是身体/武器上的攻击盒
    public void Attack(SceneItemAgent other) {
        canControlOnAir = false;
        HurtSceneItem.Add(other, CombatData.DamageDetectDelay);
    }

    //是否处于联机保护中，即其他联机玩家在对方的客户端还为完全的进入到战斗场景里
    bool InProteced() {
        if (U3D.IsMultiplyPlayer() && !synced && !Attr.IsPlayer) {
            return true;
        }
        return false;
    }

    public void OnAttack(MeteorUnit other, AttackDes des) {
        if (InProteced())
            return;
        canControlOnAir = false;
        OnDamage(other, des);
    }

    ////测试摔倒
    //public void OnDebugDamage() {
    //    SetGunReady(false);
    //    ActionMgr.ChangeAction(114, 0.1f);
    //    OnDamage(this, ActionManager.ActionList[0][200].Attack[0]);
    //}

    //成功被人攻击到，没有检测防御状态.
    public void OnAttack(MeteorUnit other) {
        canControlOnAir = false;
        //Debug.LogError("unit:" + name + " was attacked by:" + other.name);
        OnDamage(other);
    }

    //只负责一些机关，例如滚石，摆斧，撞到角色时的伤害处理
    Dictionary<SceneItemAgent, float> attackDelay = new Dictionary<SceneItemAgent, float>();
    Dictionary<SceneItemAgent, float> touchDelay = new Dictionary<SceneItemAgent, float>();//部分回血阵的回血间隔
    public void OnTriggerEnter(Collider other) {
        if (Dead)
            return;
        if (other.tag == "SceneItemAgent" || (other.transform.parent != null && other.transform.parent.tag == "SceneItemAgent")) {
            SceneItemAgent trigger = other.gameObject.GetComponentInParent<SceneItemAgent>();
            if (trigger == null)
                return;

            if (trigger != null) {
                //如果是一个攻击道具，陷阱或者机关之类的。
                if (trigger.HasDamage()) {
                    OnDamageByItem(trigger);
                } else {
                    if (touchDelay.ContainsKey(trigger))
                        return;
                    if (trigger.root != null) {
                        trigger.OnPickUped(this);
                        touchDelay[trigger] = 2.0f;
                        if (TargetItem == trigger)
                            TargetItem = null;
                    }
                }
            }
        } else if (other.tag.Equals("PickupItemAgent") || (other.transform.parent != null && other.transform.parent.tag.Equals("PickupItemAgent"))) {
            PickupItemAgent trigger = other.gameObject.GetComponentInParent<PickupItemAgent>();
            if (trigger == null)
                return;

            if (trigger != null) {
                trigger.OnPickup(this);
                if (TargetItem == trigger)
                    TargetItem = null;
            }
        }
    }

    public void OnTriggerStay(Collider other) {
        if (Dead)
            return;
        if (other.tag == "SceneItemAgent" || (other.transform.parent != null && other.transform.parent.tag == "SceneItemAgent")) {
            SceneItemAgent trigger = other.gameObject.GetComponentInParent<SceneItemAgent>();
            if (trigger == null)
                return;
            if (trigger != null) {
                if (trigger.HasDamage()) {
                    //受到陷阱攻击应该有无敌间隔.
                    if (!attackDelay.ContainsKey(trigger)) {
                        if (!Attr.Dead) {
                            OnDamage(null, trigger.DamageValue());
                            attackDelay[trigger] = 2.0f;
                        }
                    }
                } else {
                    //持续血阵，状态
                    if (touchDelay.ContainsKey(trigger))
                        return;
                    if (trigger.root != null) {
                        trigger.OnPickUped(this);
                        touchDelay[trigger] = 2.0f;
                        if (TargetItem == trigger)
                            TargetItem = null;
                    }
                }
            }
        } else if (other.tag.Equals("PickupItemAgent") || (other.transform.parent != null && other.transform.parent.tag.Equals("PickupItemAgent"))) {
            PickupItemAgent trigger = other.gameObject.GetComponentInParent<PickupItemAgent>();
            if (trigger == null)
                return;

            if (trigger != null) {
                trigger.OnPickup(this);
                if (TargetItem == trigger)
                    TargetItem = null;
            }
        }

    }

    List<SFXUnit> sfxList = new List<SFXUnit>();
    public void AddAttackSFX(SFXUnit sfx) {
        sfxList.Add(sfx);
    }

    public void OnSFXDestroy(SFXUnit sfx) {
        sfxList.Remove(sfx);
        //if (Main.Ins.GameBattleEx != null)
        //    Main.Ins.GameBattleEx.OnSFXDestroy(this, sfx.damageBox);
    }

    //受击盒，不会发生改变的.
    public List<BoxCollider> HurtList = new List<BoxCollider>();
    //攻击盒.
    public List<FightBox> HitList = new List<FightBox>();

    public void ChangeWeaponTrail(DragDes drag) {
        weaponLoader.ChangeWeaponTrail(drag);
    }

    int CalcDamage(MeteorUnit attacker, AttackDes des = null) {
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
        int PoseDamage = MenuResLoader.Ins.FindOpt(atk.PoseIdx, 3).second[0].flag[6];
        int BuffDamage = attacker.Attr.CalcBuffDamage();
        int realDamage = Mathf.Abs(Mathf.CeilToInt(((WeaponDamage * (1.0f + BuffDamage / 100.0f)) * PoseDamage) / 100.0f - (WeaponDef * (1.0f + BuffDef / 100.0f))));
        return realDamage;
    }

    //计算其他人在我的哪一个方位，每个方位控制90度范围。
    Direct CalcDirection(MeteorUnit other) {
        Vector3 otherVec = new Vector3(other.transform.position.x, 0, other.transform.position.z);
        Vector3 Vec = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 VecOffset = Vector3.Normalize(otherVec - Vec);
        float angle = Vector3.Dot(VecOffset, -transform.forward);
        float angleLeft = Vector3.Dot(VecOffset, transform.right);
        //unity精度问题容易得到nan
        if (angle > 1.0f)
            angle = 1.0f;
        else if (angle < -1.0f)
            angle = -1.0f;
        float degree = Mathf.Acos(Mathf.Clamp(angle, -1.0f, 1.0f)) * Mathf.Rad2Deg;
        //Debug.LogError("角度:" + degree);
        if (degree <= 45) {
            //Debug.LogError("正面");
            return Direct.Front;
        }
        if (degree <= 135 && angleLeft > 0) {
            //Debug.LogError("左侧");
            return Direct.Left;
        }
        if (degree <= 135 && angleLeft < 0) {
            //Debug.LogError("右侧");
            return Direct.Right;
        }
        //Debug.LogError("背面");
        return Direct.Back;
    }

    public void OnBuffDamage(int buffDamage) {
        if (NGUICameraJoystick.Ins != null)
            NGUICameraJoystick.Ins.ResetJoystick();//防止受到攻击时还可以移动视角
        SetGunReady(false);
        Attr.ReduceHp(buffDamage);
        ActionMgr.OnChangeAction(CommonAction.OnDrugHurt);
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
    void OnDamage(MeteorUnit attacker, AttackDes attackdes) {
        if (Dead)
            return;
        if (InProteced())
            return;
        if (NGUICameraJoystick.Ins != null && Attr.IsPlayer)
            NGUICameraJoystick.Ins.ResetJoystick();//防止受到攻击时还可以移动视角

        //Debug.Log(string.Format("player:{0} attacked by:{1}", name, attacker == null ? "null" : attacker.name +  " frame:" + Time.frameCount));

        if (Attr.IsPlayer && GameStateMgr.Ins.gameStatus.Undead)
            return;
        //任意受击，都会让角色退出持枪预备姿势
        SetGunReady(false);
        if (attacker == null) {
            //
        } else {
            //到此处均无须判读阵营等。
            AttackDes dam = attackdes;
            if (dam == null)
                dam = attacker.CurrentDamage;
            if (dam == null) {
                Debug.LogError("攻击者:" + attacker.name + " 用pose:" + attacker.ActionMgr.mActiveAction.Idx + " 攻击:" + name + " 此动作无攻击定义，攻击失效!!! 1");
                return;
            }
            Direct direction = CalcDirection(attacker);
            int directionAct = dam.TargetPose;
            switch (direction) {
                case Direct.Front: directionAct = dam.TargetPoseFront; break;//这个是前后左右，武器防御受击是 上下左右，上下指角色面朝方向头顶和底部
                case Direct.Back: directionAct = dam.TargetPoseBack; break;
                case Direct.Left: directionAct = dam.TargetPoseLeft; break;
                case Direct.Right: directionAct = dam.TargetPoseRight; break;
            }
            if (attacker.Attr.IsPlayer && GameStateMgr.Ins.gameStatus.EnableGodMode) {
                //一击必杀
                string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                SFXLoader.Ins.PlayEffect(attackAudio, this, 0);
                if (StateMachine != null)
                    StateMachine.OnDamaged(attacker);
                Attr.ReduceHp(Attr.HpMax);
                if (Attr.Dead)
                    OnDead(attacker);
            } else {
                if (ActionMgr.onDefence) {
                    if (dam._AttackType == 0) {
                        //Move(-attacker.transform.forward * dam.DefenseMove);
                        //通过当前武器和方向，得到防御动作ID  40+(x-1)*4类似 匕首 = 5=> 40+(5-1)*4 = 56,防御住前方攻击 57 58 59就是其他方向的
                        int TargetPos = GetGuardPose(direction);
                        string attackAudio = string.Format("W{0:D2}GD{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                        SFXLoader.Ins.PlayEffect(attackAudio, this, 0);
                        //TargetPos = 40 + ((int)idx - 1) * 4 + direction;
                        //Debug.LogError("targetPos:" + TargetPos);
                        ActionMgr.OnChangeAction(TargetPos);
                        ActionMgr.SetActionScale(dam.DefenseMove);
                        if (ActionMgr != null)
                            ActionMgr.LockTime(dam.DefenseValue);
                        int realDamage = CalcDamage(attacker, attackdes);
                        AngryValue += realDamage / CombatData.DefConvertAngry;//防御住伤害。则怒气增加
                    } else if (dam._AttackType == 1) {
                        //这个招式伤害多少?
                        //dam.PoseIdx;算伤害
                        int realDamage = CalcDamage(attacker, attackdes);
                        //Debug.Log("受到:" + realDamage + " 点伤害");
                        Option poseInfo = MenuResLoader.Ins.GetPoseInfo(dam.PoseIdx);
                        if (poseInfo.first.Length != 0 && poseInfo.first[0].flag[0] == 16)
                            GetItem(poseInfo.first[0].flag[1]);
                        Attr.ReduceHp(realDamage);
                        string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                        SFXLoader.Ins.PlayEffect(attackAudio, this, 0);
                        AngryValue += (int)((realDamage * 10) / 73.0f);
                        if (StateMachine != null)
                            StateMachine.OnDamaged(attacker);
                        if (Attr.Dead)
                            OnDead(attacker);
                        else {
                            //如果攻击者是主角，而自己又没有死，那么设置一下锁定目标为自己.(匕首后A接大，自动转向)
                            if (attacker.Attr.IsPlayer) {
                                if (Main.Ins.GameBattleEx.CanLockTarget(this)) {
                                    Main.Ins.GameBattleEx.ChangeLockedTarget(this);
                                    attacker.LockTarget = this;
                                }
                            } else {
                                //不是主角打就记录伤害，谁伤害高，就去追着谁打。
                            }
                            //播放相应特效-音效 0飞镖1

                            //被攻击后，防御相当与没有了.
                            ActionMgr.OnChangeAction(directionAct);
                            ActionMgr.LockTime(dam.TargetValue);
                            ActionMgr.SetActionScale(dam.TargetMove);
                        }
                    }
                } else {
                    int realDamage = CalcDamage(attacker, attackdes);
                    //Debug.LogError(Attr.Name + ":受到:" + attacker.Attr.name + " 的攻击 减少 " + realDamage + " 点气血" + ":f" + Time.frameCount);
                    Attr.ReduceHp(realDamage);
                    //处理招式打人后带毒
                    Option poseInfo = MenuResLoader.Ins.GetPoseInfo(dam.PoseIdx);
                    if (poseInfo.first.Length != 0 && poseInfo.first[0].flag[0] == 16)//16受到此招式攻击会得到物品
                        GetItem(poseInfo.first[0].flag[1]);
                    AngryValue += (int)((realDamage * 10) / 73.0f);
                    string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                    SFXLoader.Ins.PlayEffect(attackAudio, this, 0);
                    if (StateMachine != null)
                        StateMachine.OnDamaged(attacker);
                    if (Attr.Dead)
                        OnDead(attacker);
                    else {
                        //如果攻击者是主角，而自己又没有死，那么设置一下锁定目标为自己.(主角匕首后A打到我-接大，自动转向)
                        if (attacker.Attr.IsPlayer) {
                            if (Main.Ins.GameBattleEx.CanLockTarget(this)) {
                                Main.Ins.GameBattleEx.ChangeLockedTarget(this);
                                attacker.LockTarget = this;
                            }
                        } else {
                            //不是主角打就记录伤害，谁伤害高，就去追着谁打。
                        }
                        ActionMgr.OnChangeAction(directionAct);
                        ActionMgr.LockTime(dam.TargetValue);
                        ActionMgr.SetActionScale(dam.TargetMove);
                    }
                }
            }
        }

        if (FightState.Exist()) {
            //先飘血。
            if (Attr.IsPlayer)
                FightState.Instance.UpdatePlayerInfo();
            else if (GameStateMgr.Ins.gameStatus.ShowBlood && !SameCamp(Main.Ins.LocalPlayer))
                FightState.Instance.UpdateMonsterInfo(this);
        } else if (ReplayState.Exist()) {
            if (Attr.IsPlayer)
                ReplayState.Instance.UpdatePlayerInfo();
            else if (GameStateMgr.Ins.gameStatus.ShowBlood && !SameCamp(Main.Ins.LocalPlayer))
                ReplayState.Instance.UpdateMonsterInfo(this);
        }
    }

    public void OnDamageByItem(SceneItemAgent trigger) {
        if (attackDelay.ContainsKey(trigger))
            return;
        if (!Attr.Dead) {
            OnDamage(null, trigger.DamageValue());
            attackDelay[trigger] = 2.0f;
        }
    }

    //除了，武器碰撞，特效碰撞，还可以是buff，机关
    public void OnDamage(MeteorUnit attacker, int buffDamage = 0) {
        if (Dead)
            return;
        if (InProteced())
            return;

        if (NGUICameraJoystick.Ins != null && Attr.IsPlayer)
            NGUICameraJoystick.Ins.ResetJoystick();//防止受到攻击时还可以移动视角

        //Debug.Log(string.Format("player:{0} attacked by:{1}", name, attacker == null ? "null": attacker.name));
        //任意受击，都会让角色退出持枪预备姿势
        if (Attr.IsPlayer && GameStateMgr.Ins.gameStatus.Undead)
            return;
        SetGunReady(false);
        if (attacker == null) {
            if (StateMachine != null)
                StateMachine.OnDamaged(attacker);
            //环境伤害.
            Attr.ReduceHp(buffDamage);
            if (Attr.Dead)
                OnDead();
            else
                ActionMgr.OnChangeAction(CommonAction.OnDrugHurt);
        } else {
            //到此处均无须判读阵营等。
            AttackDes dam = attacker.damage;
            if (dam == null) {
                Debug.LogError("攻击者:" + attacker.name + " 用pose:" + attacker.ActionMgr.mActiveAction.Idx + " 攻击:" + name + " 此动作无攻击定义，攻击失效!!! 2");
                return;
            }
            Direct direction = CalcDirection(attacker);
            int directionAct = dam.TargetPose;
            switch (direction) {
                case Direct.Front: directionAct = dam.TargetPoseFront; break;//这个是前后左右，武器防御受击是 上下左右，上下指角色面朝方向头顶和底部
                case Direct.Back: directionAct = dam.TargetPoseBack; break;
                case Direct.Left: directionAct = dam.TargetPoseLeft; break;
                case Direct.Right: directionAct = dam.TargetPoseRight; break;
            }

            if (attacker.Attr.IsPlayer && GameStateMgr.Ins.gameStatus.EnableGodMode) {
                //一击必杀
                string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                SFXLoader.Ins.PlayEffect(attackAudio, this, 0);
                if (StateMachine != null)
                    StateMachine.OnDamaged(attacker);
                Attr.ReduceHp(Attr.HpMax);
                if (Attr.Dead)
                    OnDead(attacker);
            } else {
                if (ActionMgr.onDefence) {
                    if (dam._AttackType == 0) {
                        ActionMgr.LockTime(dam.DefenseValue);
                        //Move(-attacker.transform.forward * dam.DefenseMove);
                        //通过当前武器和方向，得到防御动作ID  40+(x-1)*4类似 匕首 = 5=> 40+(5-1)*4 = 56,防御住前方攻击 57 58 59就是其他方向的
                        int TargetPos = GetGuardPose(direction);
                        string attackAudio = string.Format("W{0:D2}GD{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                        SFXLoader.Ins.PlayEffect(attackAudio, this, 0);
                        ActionMgr.OnChangeAction(TargetPos);
                        ActionMgr.SetActionScale(dam.DefenseMove);
                        int realDamage = CalcDamage(attacker);
                        AngryValue += (realDamage / CombatData.DefConvertAngry);//防御住伤害。则怒气增加 200CC = 100 ANG
                    } else if (dam._AttackType == 1) {
                        //这个招式伤害多少?
                        //dam.PoseIdx;算伤害
                        int realDamage = CalcDamage(attacker);

                        Option poseInfo = MenuResLoader.Ins.GetPoseInfo(dam.PoseIdx);
                        if (poseInfo.first.Length != 0 && poseInfo.first[0].flag[0] == 16)
                            GetItem(poseInfo.first[0].flag[1]);
                        //Debug.Log("受到:" + realDamage + " 点伤害");
                        Attr.ReduceHp(realDamage);
                        ActionMgr.LockTime(dam.TargetValue);
                        string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                        SFXLoader.Ins.PlayEffect(attackAudio, this, 0);
                        AngryValue += (int)(realDamage * 10 / 73.0f);
                        if (StateMachine != null)
                            StateMachine.OnDamaged(attacker);
                        if (Attr.Dead)
                            OnDead(attacker);
                        else {
                            //如果攻击者是主角，而自己又没有死，那么设置一下锁定目标为自己.(匕首后A接大，自动转向)
                            if (attacker.Attr.IsPlayer) {
                                if (Main.Ins.GameBattleEx.CanLockTarget(this)) {
                                    Main.Ins.GameBattleEx.ChangeLockedTarget(this);
                                    attacker.LockTarget = this;
                                }
                            } else {
                                //不是主角打就记录伤害，谁伤害高，就去追着谁打。
                            }
                            ActionMgr.OnChangeAction(directionAct);
                            ActionMgr.SetActionScale(dam.TargetMove);
                        }
                    }
                } else {
                    int realDamage = CalcDamage(attacker);
                    //Debug.Log("受到:" + realDamage + " 点伤害" + " f:" + Time.frameCount);
                    Attr.ReduceHp(realDamage);
                    //处理招式打人后带毒
                    Option poseInfo = MenuResLoader.Ins.GetPoseInfo(dam.PoseIdx);
                    if (poseInfo.first.Length != 0 && poseInfo.first[0].flag[0] == 16)//16受到此招式攻击会得到物品
                        GetItem(poseInfo.first[0].flag[1]);

                    ActionMgr.LockTime(dam.TargetValue);
                    AngryValue += (int)((realDamage * 10) / 73.0f);
                    string attackAudio = string.Format("W{0:D2}BL{1:D3}.ef", attacker.GetWeaponType(), directionAct);
                    SFXLoader.Ins.PlayEffect(attackAudio, this, 0);
                    if (StateMachine != null)
                        StateMachine.OnDamaged(attacker);
                    if (Attr.Dead)
                        OnDead(attacker);
                    else {
                        //如果攻击者是主角，而自己又没有死，那么设置一下锁定目标为自己.(主角匕首后A打到我-接大，自动转向)
                        if (attacker.Attr.IsPlayer) {
                            if (Main.Ins.GameBattleEx.CanLockTarget(this)) {
                                Main.Ins.GameBattleEx.ChangeLockedTarget(this);
                                attacker.LockTarget = this;
                            }
                        } else {
                        }
                        ActionMgr.OnChangeAction(directionAct);
                        ActionMgr.SetActionScale(dam.TargetMove);
                    }
                }
            }
        }
        if (FightState.Exist()) {
            //先飘血。
            if (Attr.IsPlayer)
                FightState.Instance.UpdatePlayerInfo();
            else if (GameStateMgr.Ins.gameStatus.ShowBlood && !SameCamp(Main.Ins.LocalPlayer))
                FightState.Instance.UpdateMonsterInfo(this);
        } else if (ReplayState.Exist()) {
            if (Attr.IsPlayer)
                ReplayState.Instance.UpdatePlayerInfo();
            else if (GameStateMgr.Ins.gameStatus.ShowBlood && !SameCamp(Main.Ins.LocalPlayer))
                ReplayState.Instance.UpdateMonsterInfo(this);
        }
    }

    public void CrouchRush(RushDirection dir = RushDirection.Front) {
        switch (dir) {
            case RushDirection.Front:
                ActionMgr.ChangeAction(CommonAction.DCForw, 0.1f);
                break;
            case RushDirection.Back:
                ActionMgr.ChangeAction(CommonAction.DCBack, 0.1f);
                break;
            case RushDirection.Left:
                ActionMgr.ChangeAction(CommonAction.DCLeft, 0.1f);
                break;
            case RushDirection.Right:
                ActionMgr.ChangeAction(CommonAction.DCRight, 0.1f);
                break;
        }
    }
    public void IdleRush(RushDirection dir = RushDirection.Front) {
        //根据DIR和武器决定动作
        switch (dir) {
            case RushDirection.Front://前
                switch (GetWeaponType()) {
                    case (int)EquipWeaponType.Gun://火枪
                    case (int)EquipWeaponType.Dart://飞镖
                    case (int)EquipWeaponType.Guillotines://飞轮
                        CrouchRush(0);
                        break;
                    case (int)EquipWeaponType.Hammer://锤子
                    case (int)EquipWeaponType.Brahchthrust://双刺
                        ActionMgr.ChangeAction(CommonAction.DForw1, 0.1f);
                        break;
                    case (int)EquipWeaponType.Blade:
                    case (int)EquipWeaponType.Sword:
                        ActionMgr.ChangeAction(CommonAction.DForw2, 0.1f);
                        break;
                    case (int)EquipWeaponType.Knife:
                    case (int)EquipWeaponType.Lance:
                        ActionMgr.ChangeAction(CommonAction.DForw3, 0.1f);
                        break;
                    case (int)EquipWeaponType.NinjaSword:
                        ActionMgr.ChangeAction(CommonAction.DForw4, 0.1f);
                        break;
                    case (int)EquipWeaponType.HeavenLance:
                        ActionMgr.ChangeAction(CommonAction.DForw5, 0.1f);
                        break;
                    case (int)EquipWeaponType.Gloves:
                        ActionMgr.ChangeAction(CommonAction.DForw6, 0.1f);
                        break;
                }

                break;
            case RushDirection.Back://后
                switch (GetWeaponType()) {
                    case (int)EquipWeaponType.Gun://火枪
                    case (int)EquipWeaponType.Dart://飞镖
                    case (int)EquipWeaponType.Guillotines://飞轮
                        CrouchRush(RushDirection.Back);
                        break;
                    case (int)EquipWeaponType.Hammer://锤子
                    case (int)EquipWeaponType.Brahchthrust://双刺
                        ActionMgr.ChangeAction(CommonAction.DBack1, 0.1f);
                        break;
                    case (int)EquipWeaponType.Blade:
                    case (int)EquipWeaponType.Sword:
                        ActionMgr.ChangeAction(CommonAction.DBack2, 0.1f);
                        break;
                    case (int)EquipWeaponType.Knife:
                    case (int)EquipWeaponType.Lance:
                        ActionMgr.ChangeAction(CommonAction.DBack3, 0.1f);
                        break;
                    case (int)EquipWeaponType.NinjaSword:
                        ActionMgr.ChangeAction(CommonAction.DBack4, 0.1f);
                        break;
                    case (int)EquipWeaponType.HeavenLance:
                        ActionMgr.ChangeAction(CommonAction.DBack5, 0.1f);
                        break;
                    case (int)EquipWeaponType.Gloves:
                        ActionMgr.ChangeAction(CommonAction.DBack6, 0.1f);
                        break;
                }
                break;
            case RushDirection.Left://左
                switch (GetWeaponType()) {
                    case (int)EquipWeaponType.Gun://火枪
                    case (int)EquipWeaponType.Dart://飞镖
                    case (int)EquipWeaponType.Guillotines://飞轮
                        CrouchRush(RushDirection.Left);
                        break;
                    case (int)EquipWeaponType.Hammer://锤子
                    case (int)EquipWeaponType.Brahchthrust://双刺
                        ActionMgr.ChangeAction(CommonAction.DLeft1, 0.1f);
                        break;
                    case (int)EquipWeaponType.Blade:
                    case (int)EquipWeaponType.Sword:
                        ActionMgr.ChangeAction(CommonAction.DLeft2, 0.1f);
                        break;
                    case (int)EquipWeaponType.Knife:
                    case (int)EquipWeaponType.Lance:
                        ActionMgr.ChangeAction(CommonAction.DLeft3, 0.1f);
                        break;
                    case (int)EquipWeaponType.NinjaSword:
                        ActionMgr.ChangeAction(CommonAction.DLeft4, 0.1f);
                        break;
                    case (int)EquipWeaponType.HeavenLance:
                        ActionMgr.ChangeAction(CommonAction.DLeft5, 0.1f);
                        break;
                    case (int)EquipWeaponType.Gloves:
                        ActionMgr.ChangeAction(CommonAction.DLeft6, 0.1f);
                        break;
                }
                break;
            case RushDirection.Right://右
                switch (GetWeaponType()) {
                    case (int)EquipWeaponType.Gun://火枪
                    case (int)EquipWeaponType.Dart://飞镖
                    case (int)EquipWeaponType.Guillotines://飞轮
                        CrouchRush(RushDirection.Right);
                        break;
                    case (int)EquipWeaponType.Hammer://锤子
                    case (int)EquipWeaponType.Brahchthrust://双刺
                        ActionMgr.ChangeAction(CommonAction.DRight1, 0.1f);
                        break;
                    case (int)EquipWeaponType.Blade:
                    case (int)EquipWeaponType.Sword:
                        ActionMgr.ChangeAction(CommonAction.DRight2, 0.1f);
                        break;
                    case (int)EquipWeaponType.Knife:
                    case (int)EquipWeaponType.Lance:
                        ActionMgr.ChangeAction(CommonAction.DRight3, 0.1f);
                        break;
                    case (int)EquipWeaponType.NinjaSword:
                        ActionMgr.ChangeAction(CommonAction.DRight4, 0.1f);
                        break;
                    case (int)EquipWeaponType.HeavenLance:
                        ActionMgr.ChangeAction(CommonAction.DRight5, 0.1f);
                        break;
                    case (int)EquipWeaponType.Gloves:
                        ActionMgr.ChangeAction(CommonAction.DRight6, 0.1f);
                        break;
                }
                break;
        }
    }

    public void OnCameraRotateEnd() {
        ActionMgr.Rotateing = false;
        rotateTick = 0;
        if (ActionMgr.mActiveAction.Idx == CommonAction.WalkLeft || ActionMgr.mActiveAction.Idx == CommonAction.WalkRight) {
            ActionMgr.ChangeAction(CommonAction.Idle, 0.1f);
            return;
        }

        if (ActionMgr.mActiveAction.Idx == CommonAction.CrouchLeft || ActionMgr.mActiveAction.Idx == CommonAction.CrouchRight) {
            ActionMgr.ChangeAction(CommonAction.Crouch, 0.1f);
            return;
        }
    }

    //当视角开始准备拉动前,
    public void OnCameraRotateStart() {
        ActionMgr.Rotateing = true;
        rotateTick = 0;
    }

    public void OnGameResult(int result) {
        if (ActionManager.IsReadyAction(ActionMgr.mActiveAction.Idx) || ActionMgr.mActiveAction.Idx < CommonAction.Crouch || ActionMgr.mActiveAction.Idx == CommonAction.GunIdle) {
            ActionMgr.ChangeAction(CommonAction.Taunt, 0.1f);
            ActionMgr.playResultAction = true;
        }
    }

    /// BUFF处理
    //名字, 类型, 值, 持续, 间隔, 持续类型, 拾取后带特效.
    public void AddBuf(Option ItemInfo, bool repeatAdd = false) {
        if (!BuffMng.Ins.BufDict.ContainsKey(ItemInfo.Idx)) {
            Buff buf = new Buff();
            buf.refresh_type = ItemInfo.second[0].flag[4];
            buf.Id = ItemInfo.Idx;
            buf.Iden = ItemInfo.Identify;
            buf.refresh_delay = ItemInfo.second[0].flag[3];
            buf.effectIdx = ItemInfo.first[3].flag[1];//710特效代表角色中了蛊毒，移动-跑步动作会变化为中毒状态.
            buf.last_time = ItemInfo.first[4].flag[1];
            buf.type = (EBUFF_Type)ItemInfo.second[0].flag[2];
            buf.value = ItemInfo.second[0].flag[6];
            BuffMng.Ins.BufDict.Add(ItemInfo.Idx, buf);
        }
        BuffMng.Ins.BufDict[ItemInfo.Idx].AddUnit(this);
    }

    bool flag = false;
    public bool GetFlag { get { return flag; } }
    public Option GetFlagItem { get { return FlagItem; } }
    SFXEffectPlay flagEffect;
    int flagEffectIdx;
    Option FlagItem;
    public void SetFlag(Option f, int effectIdx) {
        flag = f == null ? false : true;
        FlagItem = f;
        if (f == null)
            return;
        if (flagEffect != null) {
            flagEffect.OnPlayAbort();
            flagEffect = null;
        }
        if (effectIdx != 0) {
            flagEffect = SFXLoader.Ins.PlayEffect(effectIdx, gameObject, false);
            flagEffectIdx = effectIdx;
        }
    }

    public void OnChangeWeaponType(int type) {
        if (type > 4 || type < 0)
            return;
        //炼化主手武器，每一个种类的武器有60个
        int weaponType = GetWeaponType();
        int targetWeapon = 0;
        switch (weaponType) {
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

    public void DropWeapon() {
        DropMng.Ins.DropWeapon(this);
    }

    //被某个招式打中后，加入BUFF或者物品效果
    public void GetItem(int idx) {
        Option it = MenuResLoader.Ins.GetItemInfo(idx);
        if (it != null)
            GetItem(it);
    }

    //角色得到任意物品的处理，包括buff,炼化,气血，怒气等
    public void GetItem(Option ItemInfo) {
        if (ItemInfo.first[2].flag[1] != 0)
            SFXLoader.Ins.PlayEffect(ItemInfo.first[2].flag[1], gameObject, true);
        //考虑所有物品
        if (ItemInfo.second.Length != 0) {
            if (ItemInfo.first != null && ItemInfo.first.Length > 4 && ItemInfo.first[4].flag[1] != 0) {
                AddBuf(ItemInfo);
                //BUFF可以立即导致角色死亡
                CheckUnitDead();
            } else {
                //一次性物品,燕羽，无法使用
                switch (ItemInfo.second[0].flag[2]) {
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

            if (Attr.IsPlayer) {
                if (FightState.Exist())
                    FightState.Instance.UpdatePlayerInfo();
                if (ReplayState.Exist())
                    ReplayState.Instance.UpdatePlayerInfo();
            }
        } else
            OnChangeWeaponType(ItemInfo.first[3].flag[1]);
    }

    void CheckUnitDead() {
        if (Attr.Dead)
            OnDead();
    }

    //是否隐身
    public bool Stealth { get { return _Stealth; } }
    bool _Stealth;//隐身以后，装备/切换武器/都需要重新设置武器的透明度
    public void ChangeHide(bool hide) {
        _Stealth = hide;
        MeshRenderer[] mr = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < mr.Length; i++) {
            if (!mr[i].enabled)
                continue;
            for (int j = 0; j < mr[i].materials.Length; j++)
                mr[i].materials[j].SetFloat("_Alpha", _Stealth ? 0.2f : 1.0f);
        }

        SkinnedMeshRenderer[] mrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < mrs.Length; i++) {
            if (!mrs[i].enabled)
                continue;
            for (int j = 0; j < mrs[i].materials.Length; j++)
                mrs[i].materials[j].SetFloat("_Alpha", _Stealth ? 0.2f : 1.0f);
        }
    }

    public int GetSkillPose(int skill = 0) {
        int pose = skill;
        switch (GetWeaponType()) {
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
                ChangeWeaponPos(WeaponPos.PosB);
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
    public void PlaySkill(int skill = 0) {
        //技能0为当前武器绝招
        if (skill == 0) {
            if (AngryValue >= 100) {
                //得到武器的大绝pose号码。
                AngryValue -= 100;
                int skillPose = ActionInterrupt.Ins.GetSkillPose(this);
                if (skillPose != 0) {
                    ActionMgr.ChangeAction(skillPose, 0.1f);
                }
            } else if (Attr.IsPlayer)
                U3D.InsertSystemMsg("怒气不足");
        } else if (skill == 1) {
            //当前武器小技能1
        } else if (skill == 2) {
            //当前武器小技能2
        }
    }

    public override void Write(BinaryWriter writer) {
        transform.Write(writer);
        //writer.Write(ImpluseVec.x.AsFloat());
        //writer.Write(ImpluseVec.y.AsFloat());
        //writer.Write(ImpluseVec.z.AsFloat());
        //writer.Write(IgnoreGravity);
        //ActionMgr.Write(writer);
    }

    public override void Read(BinaryReader reader) {

    }
}
