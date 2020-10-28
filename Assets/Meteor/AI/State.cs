using System.Collections;
using System.Collections.Generic;

using UnityEngine;
namespace Idevgame.Meteor.AI {
    public class IdleState : State {
        public IdleState(StateMachine mathine) : base(mathine) {

        }

        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
            Machine.ExitTime = Utility.Range(60, 90);
            Machine.Time = 0;
        }

        public override void OnExit(State next) {
            base.OnExit(next);
        }

        public override void Update() {
            base.Update();
            Machine.Time += FrameReplay.deltaTime;
            if (Machine.Time >= Machine.ExitTime) {
                ChangeState(Machine.WaitState);
            }
        }
    }

    //搜索敌人的待机
    public class WaitState : State {
        public WaitState(StateMachine mathine) : base(mathine) {

        }

        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
        }

        public override void OnExit(State next) {
            base.OnExit(next);
        }

        public override void Update() {
            base.Update();
        }

        public override void Think() {
            if (Player.ActionMgr.IsAttackPose()) {

            } else if (Player.ActionMgr.IsHurtPose()) {

            } else {
                //除了战斗之外的其他行为-
                if (Player.meteorController.Input.AcceptInput()) {
                    Machine.UpdateActionTriggers(false, true);
                    Machine.UpdateActionIndex();
                    Machine.DoAction();
                }
            }
        }

        public override void AutoChangeState() {
            base.AutoChangeState();
        }
    }

    //防御状态.都需要模拟按键，否则按键识别那块会影响状态的持续
    public class GuardState : State {
        float guardTime;
        public GuardState(StateMachine mathine) : base(mathine) {

        }

        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
            guardTime = (float)data;
            Machine.Stop();
            Player.meteorController.Input.OnKeyDownProxy(EKeyList.KL_Defence);
        }

        public override void OnExit(State next) {
            base.OnExit(next);
            Player.meteorController.Input.OnKeyUpProxy(EKeyList.KL_Defence);
        }

        public override void Update() {
            guardTime -= FrameReplay.deltaTime;
            if (guardTime <= 0.0f) {
                ChangeState(Machine.WaitState);
            }
        }
    }

    //一定是躲避锁定的敌方角色.
    public class DodgeState : State {
        public DodgeState(StateMachine mathine) : base(mathine) {

        }

        MeteorUnit DodgeTarget;
        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
            DodgeTarget = data as MeteorUnit;
            Machine.Time = 0;
            Machine.ExitTime = Utility.Range(5, 10);
            subState = EAISubStatus.Dodge;
            NavType = NavType.NavFindPosition;
            navPathStatus = NavPathStatus.NavPathNone;
        }

        public override void OnExit(State next) {
            base.OnExit(next);
            Stop();
        }

        public override void Think() {
            switch (subState) {
                case EAISubStatus.Dodge:
                    Dodge();
                    break;
            }
            NavThink();
        }

        public override void Update() {
            Machine.Time += FrameReplay.deltaTime;
            if (Machine.Time >= Machine.ExitTime) {
                ChangeState(Machine.WaitState);
                return;
            }
            NavUpdate();
        }

        void Dodge() {
            if (DodgeTarget == null || DodgeTarget.Dead) {
                ChangeState(Machine.WaitState);
            } else {
                positionStart = DodgeTarget.mSkeletonPivot;
                positionEndIndex = PathMng.Ins.GetDodgeWayPoint(positionStart);
                if (positionEndIndex > 0) {
                    positionEnd = CombatData.Ins.wayPoints[positionEndIndex].pos;
                    navPathStatus = NavPathStatus.NavPathNew;
                    subState = EAISubStatus.None;
                } else {
                    ChangeState(Machine.WaitState);
                }
            }
        }

        protected override void NavUpdate() {
            if (navPathStatus == NavPathStatus.NavPathOrient) {
                navPathStatus = NavPathStatus.NavPathIterator;
            } else if (navPathStatus == NavPathStatus.NavPathIterator) {
                if (Machine.Path.ways.Count == 0) {
                    NextFramePos = TargetPos - Player.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                        NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                        float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                        if (s < 0) {
                            navPathStatus = NavPathStatus.NavPathFinished;
                            Stop();
                            OnNavFinished();
                            return;
                        }
                    }
                } else {
                    if (curIndex == Machine.Path.ways.Count - 1) {
                        NextFramePos = TargetPos - Player.mSkeletonPivot;
                        NextFramePos.y = 0;
                        if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                            NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                            float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                            if (s < 0) {
                                navPathStatus = NavPathStatus.NavPathToTarget;
                                //UnityEngine.Debug.LogError("路点行走完毕");
                                TargetPos = positionEnd;
                                Stop();
                                return;
                            }
                        }
                    } else {
                        NextFramePos = TargetPos - Player.mSkeletonPivot;
                        NextFramePos.y = 0;
                        //不是最后一个路点
                        if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                            NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                            float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                            if (s < 0) {
                                curIndex += 1;
                                targetIndex = curIndex + 1;
                                TargetPos = Machine.Path.ways[curIndex].pos;
                                return;
                            }
                        }
                    }

                    if (curIndex > 0 && curIndex < Machine.Path.ways.Count) {
                        if (PathMng.Ins.GetWalkMethod(Machine.Path.ways[curIndex - 1].index, Machine.Path.ways[targetIndex - 1].index) == WalkType.Jump) {
                            if (Player.IsOnGround()) {
                                Player.FaceToTarget(Machine.Path.ways[curIndex].pos);
                                Stop();
                                //UnityEngine.Debug.LogError("Jump");
                                Jump(Machine.Path.ways[curIndex].pos);
                                return;
                            }
                        }
                    }
                }
                Player.FaceToTarget(TargetPos);
                Move();
            } else if (navPathStatus == NavPathStatus.NavPathToTarget) {
                NextFramePos = TargetPos - Player.mSkeletonPivot;
                NextFramePos.y = 0;
                if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                    NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                    float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                    if (s < 0) {
                        navPathStatus = NavPathStatus.NavPathFinished;
                        //UnityEngine.Debug.LogError("寻路完毕");
                        Stop();
                        OnNavFinished();
                        return;
                    }
                }
                Player.FaceToTarget(TargetPos);
                Move();
            }
        }

        protected override void OnNavFinished() {
            subState = EAISubStatus.Dodge;
        }
    }

    //无视距离追杀状态.
    public class KillState : State {
        public KillState(StateMachine mathine) : base(mathine) {

        }

        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
            if (KillTarget == null || KillTarget.Dead || !KillTarget.isActiveAndEnabled) {
                Debug.LogError("KillTarget is null");
                return;
            }
            positionEnd = KillTarget.mSkeletonPivot;//向锁定目标寻路
            NavType = NavType.NavFindUnit;
            positionStart = Player.mSkeletonPivot;
            positionEndIndex = PathMng.Ins.GetWayIndex(positionEnd);
            navPathStatus = NavPathStatus.NavPathNew;
        }

        public override void OnExit(State next) {
            base.OnExit(next);
        }

        public override void Think() {
            bool needRefresh = false;
            if (KillTarget == null || KillTarget.Dead) {
                ChangeState(Machine.WaitState);
                return;
            }

            if (Vector3.SqrMagnitude((KillTarget.mSkeletonPivot - Player.mSkeletonPivot)) <= CombatData.AttackRange) {
                Player.SetLockedTarget(KillTarget);
                ChangeState(Machine.FightState);
                return;
            }

            positionEnd = KillTarget.mSkeletonPivot;
            int index = PathMng.Ins.GetWayIndex(positionEnd);
            if (index != positionEndIndex) {
                needRefresh = true;
                positionEndIndex = index;
            }

            if (needRefresh) {
                positionStart = Player.mSkeletonPivot;
                navPathStatus = NavPathStatus.NavPathNew;
            }

            NavThink();
        }

        public override void Update() {
            if (KillTarget == null || KillTarget.Dead) {
                ChangeState(Machine.WaitState);
                return;
            }
            if (Vector3.SqrMagnitude((KillTarget.mSkeletonPivot - Player.mSkeletonPivot)) <= CombatData.AttackRange) {
                Player.SetLockedTarget(KillTarget);
                ChangeState(Machine.FightState);
                return;
            }
            NavUpdate();
        }

        protected override void OnNavFinished() {
            Player.SetLockedTarget(KillTarget);
            ChangeState(Machine.FightState);
        }
    }

    public class ReviveState : State {
        bool processed;
        public ReviveState(StateMachine mgr) : base(mgr) {

        }

        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
            processed = false;
        }

        public override void OnExit(State next) {
            base.OnExit(next);
        }

        public override void Update() {
            if (processed && ((Player.ActionMgr.mActiveAction.Idx == CommonAction.Idle) ||
                Player.ActionMgr.mActiveAction.Idx == CommonAction.GunIdle ||
                ActionManager.IsReadyAction(Player.ActionMgr.mActiveAction.Idx)) && !Player.ActionMgr.InTransition())
                Machine.ChangeState(Machine.WaitState);
        }

        public override void Think() {
            if (!processed) {
                Machine.ReceiveInput(new VirtualInput(EKeyList.KL_Help));
                processed = true;
            }
        }
    }

    public class PatrolState : State {
        //巡逻.
        //1：如果角色不在任意巡逻点上，那么需要先导航 从角色走向最近的巡逻点。
        //2：如果角色在某个巡逻点上，那么可以开始巡逻，可以正向，或者反向.
        //3：设置的巡逻点，一定要互相连接.
        bool reverse = false;//反转巡逻.
        public int curPatrolIndex;//当前处于哪个巡逻节点
        public int targetPatrolIndex;//下一个巡逻节点是哪个


        public List<int> patrolData = new List<int>();
        public List<WayPoint> patrolPath = new List<WayPoint>();
        public PatrolState(StateMachine mathine) : base(mathine) {

        }

        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
            //Debug.LogError(Player.name + ":enter patrol state");
            reverse = false;
            curPatrolIndex = -1;
            targetPatrolIndex = -1;
            curIndex = -1;
            targetIndex = -1;
            int k = GetPatrolIndex();
            if (k == -1) {
                targetPatrolIndex = GetNearestPatrolPoint();
            }

            if (k == -1) {
                //需要跑到最近的巡逻点.
                //Debug.Log("最近的巡逻点:" + targetPatrolIndex);
                positionStart = Player.mSkeletonPivot;
                positionEnd = patrolPath[targetPatrolIndex].pos;
                subState = EAISubStatus.None;//等待寻路得到走向第一个巡逻点的路径
            } else {
                //在巡逻点上.
                targetPatrolIndex = k;//下一个目标点就在这个点，且不用寻路，直接转向走过去.
                subState = EAISubStatus.RotateToPatrolPoint;
            }

            //Debug.Log("enter patrol state");
        }

        int GetPatrolIndex() {
            int k = PathMng.Ins.GetWayIndex(Player.mSkeletonPivot);
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
                float d = Vector3.SqrMagnitude((patrolPath[i].pos - Player.mSkeletonPivot));
                if (d < dis) {
                    dis = d;
                    ret = i;
                }
            }
            //返回的是序号，并不是路点号
            return ret;
        }

        public override void OnExit(State next) {
            base.OnExit(next);
            Machine.Stop();
        }

        public override void AutoChangeState() {
            if (Player.LockTarget != null) {
                Machine.ChangeState(Machine.FightState);
            }
        }
        //每一帧，检查是否有目标，若有目标，则不再继续跑
        public override void Update() {
            base.Update();
            if (Machine.CurrentState != this) {
                return;
            }

            switch (subState) {
                //case EAISubStatus.PatrolGotoTagret:
                //    GotoTargetPosition();
                //    break;
                //到达某个巡逻点后，转几圈
                case EAISubStatus.PatrolInPlace:
                    PatrolInPlace();
                    break;
                //旋转朝向路点
                case EAISubStatus.RotateToWayPoint:
                    RotateToWayPoint();
                    break;
                //旋转朝向巡逻点
                case EAISubStatus.RotateToPatrolPoint:
                    RotateToPatrolPoint();
                    break;
                //走向路点
                case EAISubStatus.GotoWayPoint:
                    GotoWayPoint();
                    break;
                //走向巡逻点
                case EAISubStatus.GotoPatrolPoint:
                    GotoPatrolPoint();
                    break;
                case EAISubStatus.PatrolNextPoint:
                    CheckNextPatrolPoint();
                    break;
            }
        }

        //如果当前的巡逻点和下一个巡逻点之间是直连的，那么直接进入GotoPatrolPoint
        //否则进入GotoWayPoint
        void CheckNextPatrolPoint() {
            if (curPatrolIndex >= 0 && targetPatrolIndex < patrolPath.Count) {
                if (patrolPath[curPatrolIndex].link.ContainsKey(patrolPath[targetPatrolIndex].index)) {
                    subState = EAISubStatus.GotoPatrolPoint;
                } else {
                    if (curPatrolIndex == 8 && targetPatrolIndex == 8) {
                        Debug.LogError("搜索152-18");
                    }
                    positionStart = patrolPath[curPatrolIndex].pos;
                    positionEnd = patrolPath[targetPatrolIndex].pos;
                    subState = EAISubStatus.None;
                }
            } else {
                subState = EAISubStatus.RotateToPatrolPoint;
            }
        }

        public override void Think() {
            //主要负责巡逻事务
            switch (subState) {
                case EAISubStatus.None:
                    PathHelper.Ins.CalcPath(Machine, positionStart, positionEnd);
                    subState = EAISubStatus.FindWaitPatrol;
                    break;
                case EAISubStatus.FindWaitPatrol:
                    if (Machine.Path.state != 0) {
                        if (Machine.Path.ways.Count == 0) {
                            //寻路可直达，
                            curIndex = 0;
                            targetIndex = 0;
                            TargetPos = positionEnd;
                            subState = EAISubStatus.RotateToPatrolPoint;
                        } else {
                            curIndex = 0;
                            targetIndex = curIndex + 1;
                            TargetPos = Machine.Path.ways[curIndex].pos;
                            subState = EAISubStatus.RotateToWayPoint;
                        }
                        
                    }
                    break;
            }
        }

        public void SetPatrolPath(List<int> path) {
            patrolData.Clear();
            patrolPath.Clear();
            for (int i = 0; i < path.Count; i++) {
                //后传五爪峰有巡逻路点设置问题.
                if (path[i] < CombatData.Ins.wayPoints.Count && path[i] >= 0) {
                    if (patrolPath.Count > 0) {//多个相同的路点，放到相邻，最好不要这样
                        if (patrolPath[patrolPath.Count - 1].index == path[i])
                            continue;
                    }
                    patrolData.Add(path[i]);
                    patrolPath.Add(CombatData.Ins.wayPoints[path[i]]);
                }
            }
        }

        //是否拥有巡逻数据
        public bool HasPatrolData() {
            return patrolData.Count != 0;
        }

        //到达某个巡逻点后.四周查看,
        //退出时,如果仅一个巡逻点,状态不变,如果多个巡逻点,切换到
        float rotateDuration = 0.0f;//这一圈转完需要的时间 
        float rotateTick = 0.0f;//当前圈旋转的时长
        float rotateDelay = 1.5f;//每圈之间间隔时间
        float rotateFrozen = 0.0f;//旋转CD间隔
        int rotateRound = -1;//旋转圈数 -1代表还未计算过.
        float rotateAngle;
        bool rightRotate;//是否向右侧转动
        bool frozening = false;//旋转冷却中.
        float rotateOffset = 0.0f;
        void PatrolInPlace() {
            if (rotateRound == -1) {
                frozening = false;
                rotateFrozen = rotateDelay;
                rotateTick = 0.0f;
                rotateRound = Utility.Range(1, 3);
                return;
            }
            if (Player.ActionMgr.Rotateing) {
                rotateTick += FrameReplay.deltaTime;
                float yOffset = 0.0f;
                if (rightRotate)
                    yOffset = Mathf.Lerp(0, rotateAngle, rotateTick / rotateDuration);
                else
                    yOffset = -Mathf.Lerp(0, rotateAngle, rotateTick / rotateDuration);
                Player.SetOrientation((yOffset - rotateOffset));
                rotateOffset = yOffset;
                if (rotateTick >= rotateDuration) {
                    frozening = true;
                    Player.OnCameraRotateEnd();
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
                                //检查2个巡逻点间能否到达
                                subState = EAISubStatus.PatrolNextPoint;
                                return;
                            }
                            else {
                                ChangeState(Machine.WaitState);
                            }
                        }
                    }
                } else {
                    //既没转圈,又没进入CD,那么开始转圈
                    rotateAngle = Utility.Range(75, 180);
                    rightRotate = Utility.Range(-50, 50) >= 0;
                    rotateOffset = 0.0f;
                    rotateTick = 0.0f;
                    rotateDuration = rotateAngle / CombatData.AngularVelocity;
                    Player.OnCameraRotateStart();
                }
            }
        }

        void RotateToWayPoint() {
            Player.FaceToTarget(Machine.Path.ways[targetIndex].pos);
            subState = EAISubStatus.GotoWayPoint;
        }

        void RotateToPatrolPoint() {
            Player.FaceToTarget(patrolPath[targetPatrolIndex].pos);
            //如果当前处于的巡逻点，可直接走向下一个巡逻点，否则重新寻路
            subState = EAISubStatus.GotoPatrolPoint;
        }

        void GotoPatrolPoint() {
            NextFramePos = patrolPath[targetPatrolIndex].pos - Player.mSkeletonPivot;
            NextFramePos.y = 0;
            if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                NextFramePos.y = 0;
                NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(patrolPath[targetPatrolIndex].pos - NextFramePos));
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
                    StepPatrolPoint();
                    return;
                }
            }

            if (curPatrolIndex >= 0 && targetPatrolIndex < patrolPath.Count) {
                if (PathMng.Ins.GetWalkMethod(patrolPath[curPatrolIndex].index, patrolPath[targetPatrolIndex].index) == WalkType.Jump) {
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

        void StepPatrolPoint() {
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
        }
        void GotoWayPoint() {
            NextFramePos = Machine.Path.ways[targetIndex].pos - Player.mSkeletonPivot;
            NextFramePos.y = 0;
            if (NextFramePos.sqrMagnitude <= CombatData.StopDistance) {
                NextFramePos.y = 0;
                NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(Machine.Path.ways[targetIndex].pos - NextFramePos));
                if (s < 0) {
                    Stop();
                    curIndex = targetIndex;
                    targetIndex += 1;
                    if (targetIndex >= Machine.Path.ways.Count) {
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
                        StepPatrolPoint();
                    } else {
                        subState = EAISubStatus.RotateToWayPoint;
                    }
                    return;
                }
            }

            if (curIndex > 0 && targetIndex < Machine.Path.ways.Count) {
                if (PathMng.Ins.GetWalkMethod(Machine.Path.ways[curIndex].index, Machine.Path.ways[targetIndex].index) == WalkType.Jump) {
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

    public class LookState : State {
        public LookState(StateMachine machine) : base(machine) {

        }

        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
            Reset();
            Machine.Time = 0.0f;
            Machine.ExitTime = Utility.Range(2, 4.5f);
        }

        public override void OnExit(State next) {
            base.OnExit(next);
        }

        public override void AutoChangeState() {
            if (Player.LockTarget != null) {
                Machine.ChangeState(Machine.FightState);
            }
        }

        public override void Update() {
            Machine.Time += FrameReplay.deltaTime;
            if (Machine.Time >= Machine.ExitTime) {
                if (Player.ActionMgr.Rotateing) {
                    frozening = true;
                    Player.OnCameraRotateEnd();
                }
                ChangeState(Machine.WaitState);
                return;
            }
            RotateInPlace();
        }

        void Reset() {
            rotateDuration = Machine.ExitTime;
            rotateTick = 0;
            frozening = false;
            rotateFrozen = rotateDelay;
            rotateRound = Utility.Range(1, 4);
        }
        //到达某个巡逻点后.四周查看,
        //退出时,如果仅一个巡逻点,状态不变,如果多个巡逻点,切换到
        float rotateDuration = 0.0f;//这一圈转完需要的时间 
        float rotateTick = 0.0f;//当前圈旋转的时长
        float rotateDelay = 0.8f;//每圈之间间隔时间
        float rotateFrozen = 0.0f;//旋转CD间隔
        int rotateRound = -1;//旋转圈数 -1代表还未计算过.
        float rotateAngle;
        bool rightRotate;//是否向右侧转动
        bool frozening = false;//旋转冷却中.
        float rotateOffset = 0.0f;
        void RotateInPlace() {
            if (Player.ActionMgr.Rotateing) {
                rotateTick += FrameReplay.deltaTime;
                float yOffset = 0.0f;
                if (rightRotate)
                    yOffset = Mathf.Lerp(0, rotateAngle, rotateTick / rotateDuration);
                else
                    yOffset = -Mathf.Lerp(0, rotateAngle, rotateTick / rotateDuration);
                Player.SetOrientation((yOffset - rotateOffset));
                rotateOffset = yOffset;
                if (rotateTick >= rotateDuration) {
                    frozening = true;
                    Player.OnCameraRotateEnd();
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
                            ChangeState(Machine.WaitState);
                        }
                    }
                } else {
                    //既没转圈,又没进入CD,那么开始转圈
                    rotateAngle = Utility.Range(75, 180);
                    rightRotate = Utility.Range(-50, 50) >= 0;
                    rotateOffset = 0.0f;
                    rotateTick = 0.0f;
                    rotateDuration = rotateAngle / CombatData.AngularVelocity;
                    Player.OnCameraRotateStart();
                }
            }
        }
    }

    //拾取道具状态.
    public class PickUpState : State {
        public PickUpState(StateMachine machine) : base(machine) {

        }

        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
            //检查周围可拾取的道具.
            if (Player.TargetItem != null) {
                subState = EAISubStatus.GetItemGotoItem;
                positionEnd = Player.TargetItem.transform.position;
                NavType = NavType.NavFindPosition;
                positionStart = Player.mSkeletonPivot;
                positionEndIndex = PathMng.Ins.GetWayIndex(positionEnd);
                navPathStatus = NavPathStatus.NavPathNew;
            }
            else {
                subState = EAISubStatus.GetItemComplete;
                navPathStatus = NavPathStatus.NavPathNew;
            }
        }

        public override void OnExit(State next) {
            base.OnExit(next);
        }

        public override void Update() {
            switch (subState) {
                case EAISubStatus.GetItemGotoItem:
                    NavUpdate();
                    break;
                case EAISubStatus.GetItemComplete:
                    ChangeState(Machine.WaitState);
                    break;
            }
        }

        public override void Think() {
            if (NavCheck())
                return;
            switch (subState) {
                case EAISubStatus.GetItemGotoItem:
                    NavThink();
                    break;
            }
        }

        protected override void OnNavFinished() {
            subState = EAISubStatus.GetItemComplete;
        }

        protected override void NavUpdate() {
            if (navPathStatus == NavPathStatus.NavPathOrient) {
                navPathStatus = NavPathStatus.NavPathIterator;
            } else if (navPathStatus == NavPathStatus.NavPathIterator) {
                if (Machine.Path.ways.Count == 0) {
                    NextFramePos = TargetPos - Player.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                        NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                        float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                        if (s < 0) {
                            navPathStatus = NavPathStatus.NavPathFinished;
                            Stop();
                            OnNavFinished();
                            return;
                        }
                    }
                } else {
                    if (curIndex == Machine.Path.ways.Count - 1) {
                        NextFramePos = TargetPos - Player.mSkeletonPivot;
                        NextFramePos.y = 0;
                        if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                            NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                            float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                            if (s < 0) {
                                navPathStatus = NavPathStatus.NavPathToTarget;
                                //UnityEngine.Debug.LogError("路点行走完毕");
                                TargetPos = positionEnd;
                                Stop();
                                return;
                            }
                        }
                    } else {
                        NextFramePos = TargetPos - Player.mSkeletonPivot;
                        NextFramePos.y = 0;
                        //不是最后一个路点
                        if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                            NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                            float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                            if (s < 0) {
                                curIndex += 1;
                                targetIndex = curIndex + 1;
                                TargetPos = Machine.Path.ways[curIndex].pos;
                                return;
                            }
                        }
                    }

                    if (curIndex > 0 && curIndex < Machine.Path.ways.Count) {
                        if (PathMng.Ins.GetWalkMethod(Machine.Path.ways[curIndex - 1].index, Machine.Path.ways[targetIndex - 1].index) == WalkType.Jump) {
                            if (Player.IsOnGround()) {
                                Player.FaceToTarget(Machine.Path.ways[curIndex].pos);
                                Stop();
                                //UnityEngine.Debug.LogError("Jump");
                                Jump(Machine.Path.ways[curIndex].pos);
                                return;
                            }
                        }
                    }
                }
                Player.FaceToTarget(TargetPos);
                Move();
            } else if (navPathStatus == NavPathStatus.NavPathToTarget) {
                NextFramePos = TargetPos - Player.mSkeletonPivot;
                NextFramePos.y = 0;
                if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                    NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                    float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                    if (s < 0) {
                        navPathStatus = NavPathStatus.NavPathFinished;
                        //UnityEngine.Debug.LogError("寻路完毕");
                        Stop();
                        OnNavFinished();
                        return;
                    }
                }
                Player.FaceToTarget(TargetPos);
                Move();
            }
        }
    }

    //朝向目标状态.
    public class FaceToState : State {
        public FaceToState(StateMachine machine) : base(machine) {

        }

        Vector3 faceto;
        float duration;
        float time;
        float offset0;
        float offset1;
        float angle;
        bool rightRotate;
        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
            if (data is Vector3) {
                faceto = (Vector3)data;
            } else
                faceto = LockTarget.mSkeletonPivot;
            angle = Utility.GetAngleBetween(Player, faceto);
            duration = angle / CombatData.AngularVelocity;
            time = 0;
            offset0 = 0;
            offset1 = 0;
            Vector3 diff = (faceto - Player.transform.position);
            diff.y = 0;
            float dot2 = Vector3.Dot(new Vector3(-Player.transform.right.x, 0, -Player.transform.right.z).normalized, diff.normalized);
            rightRotate = dot2 > 0;
        }

        public override void OnExit(State next) {
            base.OnExit(next);
            Player.ActionMgr.Rotateing = false;
            if (Player.ActionMgr.mActiveAction.Idx == CommonAction.WalkRight || 
                Player.ActionMgr.mActiveAction.Idx == CommonAction.WalkLeft || 
                Player.ActionMgr.mActiveAction.Idx == CommonAction.CrouchLeft ||
                Player.ActionMgr.mActiveAction.Idx == CommonAction.CrouchRight)
                Player.ActionMgr.ChangeAction(0, 0.1f);
        }

        public override void Update() {
            time += FrameReplay.deltaTime;
            if (time < duration) {
                float offset00 = offset0;
                float offset01 = offset1;
                RotateToTarget(faceto, time, duration, angle, ref offset00, ref offset01, rightRotate);
                offset0 = offset00;
                offset1 = offset01;
            } else {
                Machine.ChangeState(Machine.WaitState);
                return;
            }
        }
    }

    //寻找目标,与目标位置的寻路过程，无论这个位置是否发生变化.
    public class FindState : State {
        public FindState(StateMachine mathine) : base(mathine) {

        }

        public override void OnEnter(State prev, object data) {
            base.OnEnter(prev, data);
            //Debug.LogError("进入寻找目标状态");
            if (data is Vector3) {
                positionEnd = (Vector3)data;//向目的点寻路
                NavType = NavType.NavFindPosition;
            } else {
                if (LockTarget == null)
                    UnityEngine.Debug.LogError("还未确定目标");
                positionEnd = LockTarget.mSkeletonPivot;//向锁定目标寻路
                NavType = NavType.NavFindUnit;
            }
            positionStart = Player.mSkeletonPivot;
            positionStartIndex = PathMng.Ins.GetWayIndex(positionStart);
            positionEndIndex = PathMng.Ins.GetWayIndex(positionEnd);
            //如果该地图没有设置路点，那么直接移动过去.
            if ((positionEndIndex == -1 || positionStartIndex == -1) && CombatData.Ins.wayPoints.Count != 0)
                navPathStatus = NavPathStatus.NavPathInvalid;
            else
                navPathStatus = NavPathStatus.NavPathNew;
        }

        public override void OnExit(State next) {
            base.OnExit(next);
        }

        //计算出路径-分帧处理/或者放到寻路线程.计算出的结果，拿到
        public override void Think() {
            //是否需要重新寻路,目标所在路点已经发生变化.
            bool needRefresh = false;
            if (NavType == NavType.NavFindUnit) {
                if (LockTarget == null || LockTarget.Dead) {
                    ChangeState(Machine.WaitState);
                    return;
                }

                if (Vector3.SqrMagnitude((LockTarget.mSkeletonPivot - Player.mSkeletonPivot)) <= CombatData.AttackRange) {
                    ChangeState(Machine.WaitState);
                    return;
                }

                positionEnd = LockTarget.transform.position;
                int index = PathMng.Ins.GetWayIndex(positionEnd);
                if (index != positionEndIndex) {
                    needRefresh = true;
                    positionEndIndex = index;
                }
            }
            if (needRefresh) {
                positionStart = Player.transform.position;
                navPathStatus = NavPathStatus.NavPathNew;
            }
            NavThink();
        }

        public override void Update() {
            if (NavType == NavType.NavFindUnit) {
                if (LockTarget == null || LockTarget.Dead) {
                    ChangeState(Machine.WaitState);
                    return;
                }
                if (Vector3.SqrMagnitude((LockTarget.mSkeletonPivot - Player.mSkeletonPivot)) <= CombatData.AttackRange) {
                    ChangeState(Machine.WaitState);
                    return;
                }
            }
            NavUpdate();
        }

        //如果
        protected override void OnNavFinished() {
            switch (NavType) {
                case NavType.NavFindPosition:
                    ChangeState(Machine.AttackState);
                    break;
                case NavType.NavFindUnit:
                    ChangeState(Machine.WaitState);
                    break;
            }
        }
    }

    public class FollowState : State {
        public FollowState(StateMachine mathine) : base(mathine) {

        }

        float refreshTick;
        public override void OnEnter(State previous, object data) {
            base.OnEnter(previous, data);
            positionEnd = FollowTarget.mSkeletonPivot;//向锁定目标寻路
            NavType = NavType.NavFindUnit;
            positionStart = Player.mSkeletonPivot;
            positionEndIndex = PathMng.Ins.GetWayIndex(positionEnd);
            navPathStatus = NavPathStatus.NavPathNew;
            refreshTick = CombatData.RefreshFollowPath;//每15S清理一次跟随角色的坐标所在路点
        }

        public override void OnExit(State next) {
            base.OnExit(next);
            Machine.Stop();
        }

        public override void Think() {
            if (refreshTick < 0) {
                positionEnd = FollowTarget.mSkeletonPivot;
                int index = PathMng.Ins.GetWayIndex(positionEnd);
                if (index != positionEndIndex) {
                    positionStart = Player.mSkeletonPivot;
                    navPathStatus = NavPathStatus.NavPathNew;
                    positionEndIndex = index;
                }
                refreshTick = CombatData.RefreshFollowPath;
            }
            NavThink();
        }

        public override void Update() {
            if (FollowTarget == null || FollowTarget.Dead) {
                ChangeState(Machine.WaitState);
                return;
            }

            refreshTick -= FrameReplay.deltaTime;
            if (Vector3.SqrMagnitude((FollowTarget.mSkeletonPivot - Player.mSkeletonPivot)) <= CombatData.FollowDistanceEnd) {
                ChangeState(Machine.WaitState);
                return;
            }
            //检查与FollowTarget的距离，如果比较靠近了，就可以直接退出此状态.
            NavUpdate();
        }

        protected override void NavUpdate() {
            if (navPathStatus == NavPathStatus.NavPathOrient) {
                //如果方向不对，先切换到转向状态
                //if (GetAngleBetween(TargetPos) >= CombatData.Ins.AimDegree) {
                //    navPathStatus = NavPathStatus.NavPathIterator;
                //    Machine.ChangeState(Machine.FaceToState, TargetPos);
                //    return;
                //} else {
                navPathStatus = NavPathStatus.NavPathIterator;
                //}
            } else if (navPathStatus == NavPathStatus.NavPathIterator) {
                if (Machine.Path.ways.Count == 0) {
                    NextFramePos = TargetPos - Player.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.FollowDistanceEnd) {
                        navPathStatus = NavPathStatus.NavPathFinished;
                        Stop();
                        OnNavFinished();
                        return;
                    }
                } else {
                    if (curIndex == Machine.Path.ways.Count - 1) {
                        //先走到最后一个路点，再朝角色前进
                        NextFramePos = TargetPos - Player.mSkeletonPivot;
                        NextFramePos.y = 0;
                        if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                            NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                            float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                            if (s < 0) {
                                navPathStatus = NavPathStatus.NavPathToTarget;
                                TargetPos = positionEnd;
                                Stop();
                                return;
                            }
                        }
                    } else {
                        NextFramePos = TargetPos - Player.mSkeletonPivot;
                        NextFramePos.y = 0;
                        //不是最后一个路点
                        if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                            NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                            float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                            if (s < 0) {
                                curIndex += 1;
                                targetIndex = curIndex + 1;
                                TargetPos = Machine.Path.ways[curIndex].pos;
                                Stop();
                                return;
                            }
                        }
                    }
                    if (curIndex > 0 && curIndex < Machine.Path.ways.Count) {
                        if (PathMng.Ins.GetWalkMethod(Machine.Path.ways[curIndex - 1].index, Machine.Path.ways[targetIndex - 1].index) == WalkType.Jump) {
                            if (Player.IsOnGround()) {
                                Player.FaceToTarget(Machine.Path.ways[curIndex].pos);
                                Stop();
                                Jump(Machine.Path.ways[curIndex].pos);
                                return;
                            }
                        }
                    }
                }

                Player.FaceToTarget(TargetPos);
                Move();
            } else if (navPathStatus == NavPathStatus.NavPathToTarget) {
                NextFramePos = TargetPos - Player.mSkeletonPivot;
                NextFramePos.y = 0;
                if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.FollowDistanceEnd) {
                    navPathStatus = NavPathStatus.NavPathFinished;
                    Stop();
                    OnNavFinished();
                    return;
                }
                Player.FaceToTarget(TargetPos);
                Move();
            }
        }

        protected override void OnNavFinished() {
            switch (NavType) {
                case NavType.NavFindUnit:
                    ChangeState(Machine.WaitState);
                    break;
            }
        }
    }

    public enum ActionType {
        None = -1,//无任何动作
        Attack1,//普通攻击-不带方向的，但是可以一直普攻3连，如刀，枪
        Attack2,//连招接-轻 单个方向键的招式，比如大刀上A，左A，右A，后A
        Attack3,//连招接-重 有技能出技能，没有就出重连招
        Guard,//防守
        Dodge,//逃跑-切换状态
        Jump,//跳跃
        Burst,//速冲
        GetItem,//拾取
        Look,//原地四处看
    }

    public class ActionWeight {
        public ActionWeight(ActionType t, int w) {
            action = t;
            weight = w;
            enable = true;
        }
        public ActionType action;
        public int weight;
        public bool enable;
    }

    //战斗基类
    //需要处理倒地起身状态.
    public class FightState : State {
        public FightState(StateMachine mathine) : base(mathine) {

        }

        public override void OnEnter(State prev, object data) {
            base.OnEnter(prev, data);
        }

        public override void OnExit(State next) {
            base.OnExit(next);
        }

        public override void Update() {
        }

        //行为优先级 
        //AI强制行为(攻击指定位置，Kill追杀（不论视野）攻击 ) > 战斗(中随机拾取道具-若道具可拾取) > 跟随 > 巡逻 > 
        //丢失目标时，判断是否有跟随目标，有切换到跟随状态
        //没有跟随目标，判断是否有巡逻设定，有切换到巡逻
        //没有巡逻设定.原地待机，等待敌人经过.
        public override void Think() {
            if (Machine.LostTarget()) {
                //丢失目标后，等动作切换到待机或者准备，再切换到待机姿势.
                if ((Player.ActionMgr.mActiveAction.Idx == CommonAction.Idle ||
                    Player.ActionMgr.mActiveAction.Idx == CommonAction.GunIdle ||
                    ActionManager.IsReadyAction(Player.ActionMgr.mActiveAction.Idx)) && !Player.ActionMgr.InTransition())
                    ChangeState(Machine.WaitState);
                return;
            }

            //未丢失目标，但是目标离开超过了攻击距离.去追寻目标，或者切换远程武器
            if ((Player.ActionMgr.mActiveAction.Idx == CommonAction.Idle || 
                Player.ActionMgr.mActiveAction.Idx == CommonAction.GunIdle || 
                ActionManager.IsReadyAction(Player.ActionMgr.mActiveAction.Idx)) && !Player.ActionMgr.InTransition()) {
                if (CheckWeapon())
                    return;
            }

            if (Player.ActionMgr.IsAttackPose()) {
                //已经面对着目标了,如果接收输入
                if (Player.meteorController.Input.AcceptInput()) {
                    //取得当前招式可以连接哪些招式-对应的每一招的输入是什么.
                    //处于攻击动作中.连招几率，
                    int weight = Utility.Range(0, 100);
                    if (weight < Player.Attr.CombatChance) {
                        Machine.UpdateActionTriggers(true, false);
                        Machine.UpdateActionIndex();
                        Machine.DoAction();
                    }
                }
            } else if (Player.ActionMgr.IsHurtPose()) {
                //处于受击动作中,暴气几率.
                //爆气几率.
                if (!Machine.HasInput()) {
                    if (Player.CanBreakout()) {
                        int breakOut = Utility.Range(0, 1000);
                        //20时，就为20几率，0-19 共20
                        if (breakOut < CombatData.BreakChance)
                            Machine.ReceiveInput(new VirtualInput(EKeyList.KL_BreakOut));
                    }
                }
            } else {
                if (Player.meteorController.Input.AcceptInput()) {
                    if (CheckWeapon())
                        return;
                    //没有出招，可以救人先救人.
                    if (Player.IsLeader && Machine.CanChangeToRevive()) {
                        int chance = Utility.Range(0, 100);
                        if (chance < CombatData.RebornChance) {
                            ChangeState(Machine.ReviveState);
                            return;
                        }
                    }
                    //如果是远程武器，在射线照不到目标的时候，应该靠近
                    Machine.UpdateActionTriggers(true, true);
                    Machine.UpdateActionIndex();
                    Machine.DoAction();
                }
            }
        }

        //是否要发生状态切换，或者切换武器
        bool CheckWeapon() {
            if (U3D.IsSpecialWeapon(Player.Attr.Weapon)) {
                //如果距离太短，要切换到近战武器
                float d = Util.MathUtility.DistanceSqr(Player.mSkeletonPivot, LockTarget.mSkeletonPivot);
                if (d < CombatData.AttackRange) {
                    if (Player.Attr.Weapon2 == 0 || U3D.IsSpecialWeapon(Player.Attr.Weapon2)) {
                        //副手武器为空，只有远程武器，逃避。
                        ChangeState(Machine.DodgeState, LockTarget);
                        return true;
                    }
                    Player.ChangeWeapon();
                    return true;
                }
                if (Utility.GetAngleBetween(Player, LockTarget.mSkeletonPivot) > CombatData.AimDegree) {
                    ChangeState(Machine.FaceToState);
                    return true;
                }


            } else {
                //距离太远
                float d = Util.MathUtility.DistanceSqr(Player.mSkeletonPivot, LockTarget.mSkeletonPivot);
                //取得攻击距离
                if (d > Player.AttackRange) {
                    //距离较远，需要靠近.
                    if (Player.Attr.Weapon2 != 0 && U3D.IsSpecialWeapon(Player.Attr.Weapon2)) {
                        //如果拥有远程武器，切换武器 ???这样会导致敌方很多角色都使用远程武器，不好的体验.
                        int r = Utility.Range(1, 100);
                        if (r > CombatData.Ins.NormalWeaponProbability)
                            Player.ChangeWeapon();
                        else {
                            ChangeState(Machine.FindState);
                        }
                    } else {
                        ChangeState(Machine.FindState);
                    }
                    return true;
                }
            }
            return false;
        }
    }

    public class AttackState : State {
        public int AttackCount;//攻击次数.
        public Vector3 AttackTarget;//攻击目标-指定位置.
        public AttackState(StateMachine mathine) : base(mathine) {

        }

        public override void OnEnter(State prev, object data) {
            base.OnEnter(prev, data);
            //Debug.LogError(Player.name + ":enter attacktarget state");
        }

        public override void OnExit(State next) {
            base.OnExit(next);
        }

        //只处理状态的切换，实际的战斗都发生在Think里
        public override void Update() {
            switch (subState) {
                case EAISubStatus.AttackGotoTarget:
                    NavUpdate();
                    break;
            }
            
        }

        public override void Think() {
            switch (subState) {
                case EAISubStatus.AttackGotoTarget:
                    NavThink();
                    break;
                case EAISubStatus.AttackTarget:
                    AttackTargetAction();
                    break;
            }
        }

        //如果离目标足够近，可以击打目标了
        protected override void OnNavFinished() {
            //检查与目标的角度，足够小-开始击打，否则切换状态朝向目标位置.
            subState = EAISubStatus.AttackTarget;
        }

        //完成攻击指定目标行为.
        public bool AttackTargetComplete() {
            return AttackCount <= 0 && subState == EAISubStatus.None;
        }

        public void StartAttack() {
            subState = EAISubStatus.AttackGotoTarget;
            positionEnd = AttackTarget;
            NavType = NavType.NavFindPosition;
            positionStart = Player.mSkeletonPivot;
            positionEndIndex = PathMng.Ins.GetWayIndex(positionEnd);
            navPathStatus = NavPathStatus.NavPathNew;
        }

        public void AttackTargetAction() {
            if (AttackCount <= 0) {
                subState = EAISubStatus.None;
                return;
            }
            if (Player.ActionMgr.IsAttackPose()) {
                if (Player.meteorController.Input.AcceptInput()) {
                    DoAttack();
                }
            } else if (Player.ActionMgr.IsHurtPose()) {

            } else {
                if (Player.meteorController.Input.AcceptInput()) {
                    DoAttack();
                }
            }
        }

        void DoAttack() {
            Player.FaceToTarget(AttackTarget + Vector3.forward * 20);
            Machine.UpdateActionTriggers(true, false);
            Machine.UpdateActionIndex();
            if (Machine.DoAction()) {
                AttackCount -= 1;
                if (AttackCount <= 0) {
                    subState = EAISubStatus.None;
                }
            }
        }
        //走到目标点，距离非常近才可以继续朝下个路点行走.
        protected override void NavUpdate() {
            if (navPathStatus == NavPathStatus.NavPathOrient) {
                navPathStatus = NavPathStatus.NavPathIterator;
            } else if (navPathStatus == NavPathStatus.NavPathIterator) {
                if (Machine.Path.ways.Count == 0) {
                    NextFramePos = TargetPos - Player.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                        NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                        float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                        if (s < 0) {
                            navPathStatus = NavPathStatus.NavPathFinished;
                            Stop();
                            OnNavFinished();
                            return;
                        }
                    }
                } else {
                    if (curIndex == Machine.Path.ways.Count - 1) {
                        NextFramePos = TargetPos - Player.mSkeletonPivot;
                        NextFramePos.y = 0;
                        if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                            NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                            float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                            if (s < 0) {
                                navPathStatus = NavPathStatus.NavPathToTarget;
                                //UnityEngine.Debug.LogError("路点行走完毕");
                                TargetPos = positionEnd;
                                Stop();
                                return;
                            }
                        }
                    } else {
                        NextFramePos = TargetPos - Player.mSkeletonPivot;
                        NextFramePos.y = 0;
                        //不是最后一个路点
                        if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                            NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                            float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                            if (s < 0) {
                                curIndex += 1;
                                targetIndex = curIndex + 1;
                                TargetPos = Machine.Path.ways[curIndex].pos;
                                return;
                            }
                        }
                    }

                    if (curIndex > 0 && curIndex < Machine.Path.ways.Count) {
                        if (PathMng.Ins.GetWalkMethod(Machine.Path.ways[curIndex - 1].index, Machine.Path.ways[targetIndex - 1].index) == WalkType.Jump) {
                            if (Player.IsOnGround()) {
                                Player.FaceToTarget(Machine.Path.ways[curIndex].pos);
                                Stop();
                                //UnityEngine.Debug.LogError("Jump");
                                Jump(Machine.Path.ways[curIndex].pos);
                                return;
                            }
                        }
                    }
                }
                Player.FaceToTarget(TargetPos);
                Move();
            } else if (navPathStatus == NavPathStatus.NavPathToTarget) {
                NextFramePos = TargetPos - Player.mSkeletonPivot;
                NextFramePos.y = 0;
                if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance) {
                    NextFramePos = Player.mSkeletonPivot + NextFramePos.normalized * Player.MoveSpeed * FrameReplay.deltaTime * CombatData.StopMove;
                    float s = Utility.GetAngleBetween(Vector3.Normalize(NextFramePos - Player.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                    if (s < 0) {
                        navPathStatus = NavPathStatus.NavPathFinished;
                        //UnityEngine.Debug.LogError("寻路完毕");
                        Stop();
                        OnNavFinished();
                        return;
                    }
                }
                Player.FaceToTarget(TargetPos);
                Move();
            }
        }
    }
}