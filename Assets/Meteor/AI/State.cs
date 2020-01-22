using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Idevgame.Meteor.AI
{
    //待机状态.
    public class IdleState : State
    {
        public IdleState(string sn, StateMachine mathine) : base(sn, mathine)
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

    //待启动，原地播放待机动画.不主动切换.
    public class WaitState:State
    {
        public WaitState(string sn, StateMachine mathine) : base(sn, mathine)
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
        public GuardState(string sn, StateMachine mathine) : base(sn, mathine)
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
        public LookState(string sn, StateMachine mathine) : base(sn, mathine)
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
        public DangerState(string sn, StateMachine mathine) : base(sn, mathine)
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
        public KillState(string sn, StateMachine mathine) : base(sn, mathine)
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
        public PatrolState(string sn, StateMachine mathine) : base(sn, mathine)
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

    public class FollowState:State
    {
        public FollowState(string sn, StateMachine mathine) : base(sn, mathine)
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
        public SurroundState(string sn, StateMachine mathine) : base(sn, mathine)
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

    //战斗基类-战斗圈
    public class FightState:State
    {
        public int AttackCount;//攻击次数.
        public Vector3 AttackTarget;//攻击目标-指定位置.
        public FightState(string sn, StateMachine mathine) : base(sn, mathine)
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

    //使用剑(战斗圈).
    public class FightWithSwordState: FightState
    {
        public FightWithSwordState(string sn, StateMachine mathine) : base(sn, mathine)
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

    //使用刀(战斗圈).
    public class FightWithBladeState: FightState
    {
        public FightWithBladeState(string sn, StateMachine mathine) : base(sn, mathine)
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
}