using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Idevgame.Meteor.AI
{
    //待机状态
    public class IdleState : State
    {
        public IdleState(StateMachine mathine) : base(mathine)
        {
            
        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }
    }

    //防御状态.
    public class GuardState:State
    {
        public GuardState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }
    }

    public class LookState:State
    {
        public LookState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }
    }

    public class DodgeState:State
    {
        public DodgeState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }
    }

    //无视距离追杀状态.
    public class KillState:State
    {
        public KillState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }
    }

    public class ReviveState : State
    {
        public ReviveState(StateMachine mgr):base(mgr)
        {

        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }
    }

    public class PatrolState : State
    {
        //巡逻.
        //1：如果角色不在任意巡逻点上，那么需要先导航 从角色走向最近的巡逻点。
        //2：如果角色在某个巡逻点上，那么可以开始巡逻，可以正向，或者反向.

        bool reverse = false;//反转巡逻.
        public int curPatrolIndex;//当前处于哪个巡逻节点
        public int targetPatrolIndex;//下一个巡逻节点是哪个


        public List<int> patrolData = new List<int>();
        public List<WayPoint> patrolPath = new List<WayPoint>();
        public PatrolState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
            curPatrolIndex = -1;
            targetPatrolIndex = -1;
            curIndex = -1;
            targetIndex = -1;
            int k = GetPatrolIndex();
            if (k == -1) {
                targetPatrolIndex = GetNearestPatrolPoint();
            }
            
            if (k == -1) {
                //当前角色需要跑到最近的巡逻点开始巡逻.
                Debug.Log("最近的巡逻点:" + targetPatrolIndex);
                if (Machine.Path.state != 0) {
                    //不在查询中.则查询这次到最近巡逻点的路径
                    PathHelper.Ins.CalcPath(Machine, Player.mSkeletonPivot, targetPatrolIndex);
                }
            } else {
                //在巡逻点上.
                targetPatrolIndex = k;//下一个目标点就在这个点，且不用寻路，直接转向走过去.
                subState = EAISubStatus.RotateToPatrolPoint;
            }

            Debug.Log("enter patrol state");
        }

        //寻路阶段完成，当前点到第一个点的路径
        public void OnPathCalcFinished() {
            Debug.Log("calc finished on patrolstate");
            targetIndex = 0;
            subState = EAISubStatus.RotateToWayPoint;
        }

        int GetPatrolIndex() {
            int k = Main.Ins.PathMng.GetWayIndex(Player.mSkeletonPivot);
            if (k == -1)
                return -1;
            for (int i = 0; i < patrolPath.Count; i++) {
                if (patrolPath[i].index == k)
                    return i;
            }
            return -1;
        }

        int GetNearestPatrolPoint() {
            float dis = 25000000.0f;
            int ret = -1;
            for (int i = 0; i < patrolPath.Count; i++) {
                float d = Vector3.SqrMagnitude(patrolPath[i].pos - Player.mSkeletonPivot);
                if (d < dis) {
                    dis = d;
                    ret = i;
                }
            }
            return ret;
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }

        //每一帧，检查是否有目标，若有目标，则不再继续跑
        public override void Update() {
            base.Update();
            if (Machine.CurrentState != this) {
                return;
            }

            switch (subState) {
                case EAISubStatus.None:
                    break;
                case EAISubStatus.PatrolInPlace:
                    PatrolInPlace();
                    break;
                case EAISubStatus.RotateToWayPoint:
                    RotateToWayPoint();
                    break;
                case EAISubStatus.RotateToPatrolPoint:
                    RotateToPatrolPoint();
                    break;
                case EAISubStatus.GotoWayPoint:
                    GotoWayPoint();
                    break;
                case EAISubStatus.GotoPatrolPoint:
                    GotoPatrolPoint();
                    break;
            }
            //原地巡逻
            if (patrolPath.Count == 1 && patrolData.Count == 1) {
                //原地巡逻.四周旋转.
                return;
            }
        }

        //一个思考周期
        public override void Think()
        {
            //主要负责巡逻事务
        }

        public void SetPatrolPath(List<int> path)
        {
            patrolData.Clear();
            patrolPath.Clear();
            for (int i = 0; i < path.Count; i++) {
                patrolData.Add(path[i]);
                patrolPath.Add(Main.Ins.CombatData.wayPoints[path[i]]);
            }
        }

        //到达某个巡逻点后.四周查看,
        //退出时,如果仅一个巡逻点,状态不变,如果多个巡逻点,切换到
        float rotateDuration = 0.0f;//这一圈转完需要的时间 
        float rotateTick = 0.0f;//当前圈旋转的时长
        float rotateDelay = 2.0f;//每圈之间间隔时间
        float rotateFrozen = 0.0f;//旋转CD间隔
        int rotateRound = -1;//旋转圈数 -1代表还未计算过.
        float rotateAngle;
        bool rightRotate;//是否向右侧转动
        bool rotating = false;//旋转中.
        bool frozening = false;//旋转冷却中.
        float rotateOffset = 0.0f;
        void PatrolInPlace() {
            if (rotateRound == -1) {
                rotating = false;
                frozening = false;
                rotateFrozen = rotateDelay;
                rotateTick = 0.0f;
                rotateRound = Random.Range(1, 3);
                return;
            }
            if (rotating) {
                rotateTick += FrameReplay.deltaTime;
                float yOffset = 0.0f;
                if (rightRotate)
                    yOffset = Mathf.Lerp(0, rotateAngle, rotateTick / rotateDuration);
                else
                    yOffset = -Mathf.Lerp(0, rotateAngle, rotateTick / rotateDuration);
                Player.SetOrientation(yOffset - rotateOffset);
                rotateOffset = yOffset;
                if (rotateTick >= rotateDuration) {
                    if (Player.posMng.mActiveAction.Idx == CommonAction.WalkRight || Player.posMng.mActiveAction.Idx == CommonAction.WalkLeft)
                        Player.posMng.ChangeAction(0, 0.1f);
                    rotating = false;//转圈转完了,进入冷却状态.
                    frozening = true;
                }
            } else {
                if (frozening) {
                    rotateFrozen -= FrameReplay.deltaTime;
                    if (rotateFrozen <= 0.0f) {
                        //判断是否还能旋转.
                        rotateRound -= 1;
                        rotateFrozen = rotateDelay;
                        if (rotateRound > 0) {
                            //计算下一次旋转
                            frozening = false;//开始下次旋转
                        } else {
                            if (patrolData.Count > 1) {
                                subState = EAISubStatus.RotateToPatrolPoint;
                                return;
                            }
                        }
                    }
                } else {
                    //既没转圈,又没进入CD,那么开始转圈
                    rotateAngle = Random.Range(-180, 180);
                    rightRotate = Random.Range(-1, 2) >= 0;
                    rotating = true;
                    rotateOffset = 0.0f;
                    rotateDuration = rotateAngle / CombatData.AngularVelocity;
                }
            }
        }

        void RotateToWayPoint() {
            Player.FaceToTarget(Machine.Path.ways[targetIndex].pos);
            subState = EAISubStatus.GotoWayPoint;
        }

        void RotateToPatrolPoint() {
            Player.FaceToTarget(patrolPath[targetPatrolIndex].pos);
            subState = EAISubStatus.GotoPatrolPoint;
        }

        void GotoPatrolPoint() {
            NextFramePos = patrolPath[targetPatrolIndex].pos - Player.mSkeletonPivot;
            NextFramePos.y = 0;
            if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                NextFramePos.y = 0;
                NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * 0.15f;
                float s = GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(patrolPath[targetPatrolIndex].pos - NextFramePos));
                if (s < 0) {
                    Stop();
                    curPatrolIndex = targetPatrolIndex;
                    //仅一个巡逻点,切换状态.
                    if (patrolPath.Count == 1) {
                        subState = EAISubStatus.PatrolInPlace;
                        rotateRound = -1;
                        return;
                    }
                    //多个巡逻点,到达这个巡逻点后,切换为巡视中.四周看.
                    subState = EAISubStatus.PatrolInPlace;
                    rotateRound = -1;
                    if (reverse) {
                        if (targetPatrolIndex == 0) {
                            targetPatrolIndex += 1;
                            reverse = false;
                            return;
                        } else {
                            targetPatrolIndex -= 1;
                            return;
                        }
                    } else {
                        if (targetPatrolIndex == patrolPath.Count - 1) {
                            reverse = true;
                            targetPatrolIndex -= 1;
                        } else {
                            targetPatrolIndex += 1;
                        }
                    }
                    if (targetPatrolIndex < 0 || targetPatrolIndex >= patrolPath.Count) {
                        Debug.LogError("下个巡逻点计算错误,越界");
                    }
                    return;
                }
            }

            if (curPatrolIndex != -1) {
                if (Main.Ins.PathMng.GetWalkMethod(patrolPath[curPatrolIndex].index, patrolPath[targetPatrolIndex].index) == WalkType.Jump) {
                    if (Player.IsOnGround()) {
                        Player.FaceToTarget(patrolPath[targetPatrolIndex].pos);
                        Stop();
                        Jump(patrolPath[targetPatrolIndex].pos);
                    }
                } else {
                    Player.FaceToTarget(patrolPath[targetPatrolIndex].pos);
                    Move();
                }
            } else {
                Player.FaceToTarget(patrolPath[targetPatrolIndex].pos);
                Move();
            }
        }

        void GotoWayPoint() {
            NextFramePos = Machine.Path.ways[targetIndex].pos - Player.mSkeletonPivot;
            NextFramePos.y = 0;
            if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                NextFramePos.y = 0;
                NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * 0.15f;
                float s = GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(Machine.Path.ways[targetIndex].pos - NextFramePos));
                if (s < 0) {
                    Stop();
                    curIndex = targetIndex;
                    targetIndex += 1;
                    if (targetIndex >= Machine.Path.ways.Count) {
                        subState = EAISubStatus.RotateToPatrolPoint;
                    } else {
                        subState = EAISubStatus.RotateToWayPoint;
                    }
                    return;
                }
            }

            if (curIndex != -1) {
                if (Main.Ins.PathMng.GetWalkMethod(Machine.Path.ways[curIndex].index, Machine.Path.ways[targetIndex].index) == WalkType.Jump) {
                    if (Player.IsOnGround()) {
                        Player.FaceToTarget(Machine.Path.ways[targetIndex].pos);
                        Stop();
                        Jump(Machine.Path.ways[targetIndex].pos);
                    }
                } else {
                    Player.FaceToTarget(Machine.Path.ways[targetIndex].pos);
                    Move();
                }
            } else {
                Player.FaceToTarget(Machine.Path.ways[targetIndex].pos);
                Move();
            }
        }
    }

    public class LeaveState:State
    {
        public LeaveState(StateMachine machine) : base(machine)
        {

        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }
    }

    //拾取道具状态.
    public class PickUpState:State
    {
        public PickUpState(StateMachine machine) : base(machine)
        {

        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }
    }

    //朝向目标状态.
    public class FaceToState:State
    {
        public FaceToState(StateMachine machine):base(machine)
        {

        }

        Vector3 faceto;
        float duration;
        float time;
        float offset0;
        float offset1;
        float angle;
        bool rightRotate;
        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
            if (data is Vector3)
            {
                faceto = (Vector3)data;
            }
            else
                faceto = LockTarget.mSkeletonPivot;
            angle = GetAngleBetween(faceto);
            duration = angle / CombatData.AngularVelocity;
            time = 0;
            offset0 = 0;
            offset1 = 0;
            Vector3 diff = (faceto - Player.transform.position);
            diff.y = 0;
            float dot2 = Vector3.Dot(new Vector3(-Player.transform.right.x, 0, -Player.transform.right.z).normalized, diff.normalized);
            rightRotate = dot2 > 0;
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
            Player.posMng.Rotateing = false;
            if (Player.posMng.mActiveAction.Idx == CommonAction.WalkRight || Player.posMng.mActiveAction.Idx == CommonAction.WalkLeft)
                Player.posMng.ChangeAction(0, 0.1f);
        }

        public override void Update()
        {
            time += FrameReplay.deltaTime;
            if (time < duration)
            {
                RotateToTarget(faceto, time, duration, angle, ref offset0, ref offset1, rightRotate);
            }
            else
            {
                Machine.ResumeState(Previous);
                return;
            }
        }
    }

    //寻找目标,与目标位置的寻路过程，无论这个位置是否发生变化.
    public class FindState:State
    {
        public FindState(StateMachine mathine) : base(mathine)
        {

        }

        //状态被恢复.
        public override void OnResume(State prev, object data)
        {
            base.OnResume(prev, data);
        }

        //状态被暂停-切换到其他状态
        public override void OnPause(State next, object data)
        {
            base.OnPause(next, data);
        }

        public override void OnEnter(State prev,object data)
        {
            base.OnEnter(prev, data);
            if (data is Vector3)
            {
                positionEnd = (Vector3)data;//向目的点寻路
                NavType = NavType.NavFindPosition;
            }
            else
            {
                if (LockTarget == null)
                    UnityEngine.Debug.LogError("还未确定目标");
                positionEnd = LockTarget.transform.position;//向锁定目标寻路
                NavType = NavType.NavFindUnit;
            }
            positionStart = Player.transform.position;
            navPathStatus = NavPathStatus.NavPathNone;
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }

        //计算出路径-分帧处理/或者放到寻路线程.计算出的结果，拿到
        public override void Think()
        {
            NavThink();
        }

        
        public override void Update()
        {
            NavUpdate();
        }
    }

    public class FollowState:State
    {
        public FollowState(StateMachine mathine) : base(mathine)
        {

        }


        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
            if (data is Vector3)
            {
                positionEnd = (Vector3)data;//向目的点寻路
                NavType = NavType.NavFindPosition;
            }
            else
            {
                if (FollowTarget == null)
                    UnityEngine.Debug.LogError("还未确定目标");
                positionEnd = FollowTarget.transform.position;
                NavType = NavType.NavFindUnit;
            }
            positionStart = Player.transform.position;
            navPathStatus = NavPathStatus.NavPathNone;
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }

        public override void Think()
        {
            NavThink();
        }

        public override void Update()
        {
            NavUpdate();
        }
    }

    public enum ActionType
    {
        Attack1,//连招起始
        Attack2,//连招接+1
        Attack3,//连招接+2
        Guard,//防守
        Dodge,//逃跑-切换状态
        Jump,//跳跃
        Look,//观察四周
        Burst,//速冲
        GetItem,//拾取
    }

    class ActionWeight
    {
        public ActionWeight(ActionType t, int w)
        {
            action = t;
            weight = w;
            enable = true;
        }
        public ActionType action;
        public int weight;
        public bool enable;
    }

    //基类-包围圈-围而不攻.
    public class SurroundState:State
    {
        public SurroundState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }

        public override void Think()
        {

        }
    }

    public class FightOnAirState:State
    {
        public Vector3 AttackTarget;//攻击目标-指定位置.
        public FightOnAirState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(State previous, object data)
        {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }

        public override void Think()
        {

        }
    }
    //战斗基类
    //需要处理倒地起身状态.
    public class FightState:State
    {
        public int AttackCount;//攻击次数.
        public Vector3 AttackTarget;//攻击目标-指定位置.
        public FightState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(State prev, object data)
        {
            base.OnEnter(prev, data);
            //战斗中，基本不会四周看
            Machine.ResetAction();
            Machine.SetActionTriggered(ActionType.Look, false);
        }

        public override void OnExit(State next)
        {
            base.OnExit(next);
        }

        //行为优先级 
        //AI强制行为(攻击指定位置，Kill追杀（不论视野）攻击 ) > 战斗(中随机拾取道具-若道具可拾取) > 跟随 > 巡逻 > 
        //丢失目标时，判断是否有跟随目标，有切换到跟随状态
        //没有跟随目标，判断是否有巡逻设定，有切换到巡逻
        //没有巡逻设定.原地待机，等待敌人经过.
        public override void Think()
        {
            Machine.SetActionTriggered(ActionType.Dodge, Player.WillDead);//能否随机到逃跑，以当前状态是否濒危决定.
            if (LockTarget.Dead)
            {
                ChangeState(Machine.IdleState);
            }
            else
            {
                //距离太远
                float d = Util.MathUtility.DistanceSqr(Player.mSkeletonPivot, LockTarget.mSkeletonPivot);
                //取得攻击距离
                if (d > Player.AttackRange)
                {
                    //距离较远，需要靠近.
                    if (U3D.IsSpecialWeapon(Player.Attr.Weapon2))
                    {
                        //如果拥有远程武器，切换武器 ???这样会导致敌方很多角色都使用远程武器，不好的体验.
                        int r = Random.Range(1, 100);
                        if (r > Main.Ins.CombatData.SpecialWeaponProbability)
                            Player.ChangeWeapon();
                        else
                            ChangeState(Machine.FindState);
                    }
                    else
                        ChangeState(Machine.FindState);
                }
                else
                {
                    //如果拿着远程武器，距离却太近，需要远离目标或者切换武器
                    if (U3D.IsSpecialWeapon(Player.Attr.Weapon))
                    {
                        if (d <= CombatData.AttackRange)
                        {
                            if (!U3D.IsSpecialWeapon(Player.Attr.Weapon2))
                            {
                                Player.ChangeWeapon();
                                return;
                            }
                            else
                            {
                                ChangeState(Machine.LeaveState);
                                return;
                            }
                        }
                    }

                    //如果拿着远程武器，与目标角度相差过大，需要先面对目标.
                    if (U3D.IsSpecialWeapon(Player.Attr.Weapon) && GetAngleBetween(LockTarget.mSkeletonPivot) > Main.Ins.CombatData.AimDegree)
                    {
                        ChangeState(Machine.FaceToState);
                    }
                    else
                    {
                        //已经面对着目标了,如果接收输入
                        if (Player.controller.Input.AcceptInput())
                        {
                            if (Player.posMng.IsAttackPose())
                            {
                                //处于攻击动作中.连招几率
                                int chance = Random.Range(0, 100);
                                if (chance < Main.Ins.CombatData.ComboProbability)
                                {
                                    //禁止部分行为
                                    Machine.SetActionTriggered(ActionType.Burst, false);
                                    Machine.SetActionTriggered(ActionType.Dodge, false);
                                    Machine.SetActionTriggered(ActionType.GetItem, false);
                                    Machine.SetActionTriggered(ActionType.Guard, false);
                                    Machine.SetActionTriggered(ActionType.Jump, false);
                                    Machine.SetActionTriggered(ActionType.Look, false);
                                    Machine.SetActionTriggered(ActionType.Attack3, Player.AngryValue >= 100);
                                    Machine.UpdateActionIndex();
                                    Machine.DoAction();
                                }
                            }
                            else if (Player.posMng.IsHurtPose())
                            {
                                //处于受击动作中,暴气几率.
                                //爆气几率.
                                if (InputQueue.Count == 0)
                                {
                                    if (Player.CanBurst())
                                    {
                                        int breakOut = Random.Range(0, 100);
                                        //20时，就为20几率，0-19 共20
                                        if (breakOut < CombatData.BreakChange)
                                            Machine.ReceiveInput(EKeyList.KL_BreakOut);
                                    }
                                }
                            }
                            else
                            {
                                //距离在范围内.攻击行为的挑选,可选择的行为较多
                                Machine.UpdateEnviroument();
                                Machine.UpdateActionIndex();
                                Machine.DoAction();
                            }
                        }
                    }
                }
            }
        }

        //完成攻击指定目标行为.
        public bool AttackTargetComplete()
        {
            return false;
        }


    }
}