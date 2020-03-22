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

        public override void Init()
        {

        }

        public override void Enter(Object data)
        {

        }

        public override void Exit()
        {

        }
    }

    //防御状态.
    public class GuardState:State
    {
        public GuardState(StateMachine mathine) : base(mathine)
        {

        }

        public override void Init()
        {

        }

        public override void Enter(Object data)
        {

        }

        public override void Exit()
        {

        }
    }

    public class LookState:State
    {
        public LookState(StateMachine mathine) : base(mathine)
        {

        }

        public override void Init()
        {

        }

        public override void Enter(Object data)
        {

        }

        public override void Exit()
        {

        }
    }

    public class DangerState:State
    {
        public DangerState(StateMachine mathine) : base(mathine)
        {

        }

        public override void Init()
        {

        }

        public override void Enter(Object data)
        {

        }

        public override void Exit()
        {

        }
    }

    //无视距离追杀状态.
    public class KillState:State
    {
        public KillState(StateMachine mathine) : base(mathine)
        {

        }

        public override void Init()
        {

        }

        public override void Enter(Object data)
        {

        }

        public override void Exit()
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

        public override void Init()
        {

        }

        public override void Enter(Object data)
        {
            UnityEngine.Debug.Log("enter patrol path");
        }

        public override void Exit()
        {

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

        public override void Init()
        {

        }

        public override void Enter(Object data)
        {

        }

        public override void Exit()
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

        public override void Init()
        {

        }

        public override void Enter(Object data)
        {

        }

        public override void Exit()
        {

        }

        public override State Update()
        {
            return null;
        }
    }

    //战斗基类
    public class FightState:State
    {
        public int AttackCount;//攻击次数.
        public Vector3 AttackTarget;//攻击目标-指定位置.
        public FightState(StateMachine mathine) : base(mathine)
        {

        }

        public override void Init()
        {

        }

        public override void Enter(Object data)
        {

        }

        public override void Exit()
        {

        }

        public override State Update()
        {
            return null;
        }

        //完成攻击指定目标行为.
        public bool AttackTargetComplete()
        {
            return false;
        }
    }
}