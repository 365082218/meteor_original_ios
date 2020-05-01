using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Idevgame.Meteor.AI
{
    public class WaitState :State
    {
        public WaitState(StateMachine mathine) : base(mathine)
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
    //待机状态.可识别目标
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
        //寻路相关的.
        public int curPatrolIndex;
        //int startPathIndex;
        bool reverse = false;
        public int targetPatrolIndex;
        public List<WayPoint> PatrolPath = new List<WayPoint>();
        List<WayPoint> PatrolTemp = new List<WayPoint>();
        List<WayPoint> PatrolTemp2 = new List<WayPoint>();
        public List<int> patrolData = new List<int>();
        public bool FindWayFinished = false;
        public bool FindPatrolFinished = false;
        public int curIndex = 0;
        public int targetIndex = 0;
        public PatrolState(StateMachine mathine) : base(mathine)
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
            //主要负责巡逻事务
        }

        public void SetPatrolPath(List<int> path)
        {
            patrolData.Clear();
            for (int i = 0; i < path.Count; i++)
                patrolData.Add(path[i]);
            PatrolPath.Clear();
            FindPatrolFinished = false;
            curPatrolIndex = -1;
            targetPatrolIndex = -1;
            curIndex = -1;
            targetIndex = -1;
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
    public class FightOnGroundState:State
    {
        public int AttackCount;//攻击次数.
        public Vector3 AttackTarget;//攻击目标-指定位置.
        public FightOnGroundState(StateMachine mathine) : base(mathine)
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