using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Idevgame.Meteor.AI
{
    //待机状态.
    public class IdleState : State
    {
        public IdleState(StateMachine mathine) : base(mathine)
        {
            
        }

        public override void OnEnter(Object data)
        {

        }

        public override void OnExit()
        {

        }
    }

    //防御状态.
    public class GuardState:State
    {
        public GuardState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(Object data)
        {

        }

        public override void OnExit()
        {

        }
    }

    public class LookState:State
    {
        public LookState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(Object data)
        {

        }

        public override void OnExit()
        {

        }
    }

    public class DangerState:State
    {
        public DangerState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(Object data)
        {

        }

        public override void OnExit()
        {

        }
    }

    //无视距离追杀状态.
    public class KillState:State
    {
        public KillState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(Object data)
        {

        }

        public override void OnExit()
        {

        }
    }

    public class ReviveState : State
    {
        public ReviveState(StateMachine mgr):base(mgr)
        {

        }

        public override void OnEnter(Object data)
        {
            UnityEngine.Debug.Log("enter patrol path");
        }

        public override void OnExit()
        {

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


        public override void OnEnter(Object data)
        {
            UnityEngine.Debug.Log("enter patrol path");
        }

        public override void OnExit()
        {

        }

        public override void Update()
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

    public class FollowState:State
    {
        public FollowState(StateMachine mathine) : base(mathine)
        {

        }

        public override void OnEnter(Object data)
        {

        }

        public override void OnExit()
        {

        }
    }

    public enum ActionType
    {
        Attack1,//连招起始
        Attack2,//连招接+1
        Attack3,//连招接+2
        Guard,//防守
        Dodge,//闪避
        Jump,//跳跃
        Look,//观察四周
        Burst,//爆发
        Aim,//瞄准
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

        public override void OnEnter(Object data)
        {

        }

        public override void OnExit()
        {

        }

        public override void Update()
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

        public override void OnEnter(Object data)
        {

        }

        public override void OnExit()
        {

        }

        //行为优先级 
        //AI强制行为(攻击指定位置，Kill追杀（不论视野）攻击 ) > 战斗(中随机拾取道具-若道具可拾取) > 跟随 > 巡逻 > 
        //丢失目标时，判断是否有跟随目标，有切换到跟随状态
        //没有跟随目标，判断是否有巡逻设定，有切换到巡逻
        //没有巡逻设定.原地待机，等待敌人经过.
        public override void Update()
        {
            //看权重执行对应的事情.
            //若丢失目标，切换到待机
        }

        //完成攻击指定目标行为.
        public bool AttackTargetComplete()
        {
            return false;
        }
    }
}