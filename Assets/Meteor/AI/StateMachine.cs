using Assets.Code.Idevgame.Common.Util;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public enum EAISubStatus {
    None,
    //第二版状态机
    GetItemGotoItem,//朝物件走的路线中
    GetItemComplete,//拾取结束
    AttackGotoTarget,//攻击指定位置-寻路中.
    AttackTarget,//攻击指定位置-攻击中
    FindWaitPatrol,//等待巡逻所有路点间的寻路完成.

    PatrolInPlace,//原地巡逻
    PatrolNextPoint,//下一段巡逻，2个巡逻点之间是断开的
    RotateToPatrolPoint,//旋转朝向巡逻点
    RotateToWayPoint,//旋转朝向路点
    GotoWayPoint,//走向下一个路点.
    GotoPatrolPoint,//走向下一个巡逻点.
    Dodge,//躲避.
}

public enum VirtualKeyState {
    None,
    Press,//压下当帧
    Release,//抬起当帧
    Pressing,//持续压下-蓄力
}

public class VirtualInput {
    public EKeyList key;
    public VirtualKeyState state;
    public int delay = 0;//压下和抬起的间隔，一般情况下是1帧
    public VirtualInput(EKeyList k, int frameDelay = 1) {
        key = k;
        state = VirtualKeyState.None;
        delay = frameDelay;
    }

    public VirtualInput() {
        delay = 1;
        state = VirtualKeyState.None;
    }

    static bool InGroup(int[] group, int target) {
        for (int i = 0; i < group.Length; i++) {
            if (group[i] == target)
                return true;
        }
        return false;
    }

    public static List<VirtualInput> CalcPoseInput(int KeyMap) {
        List<VirtualInput> skill = new List<VirtualInput>();
        //普通攻击.
        int[] GroundKeyMap = new int[] { 1, 5, 9, 13, 25, 35, 48, 61, 73, 91, 95, 101, 108, 123 };
        int[] AirKeyMap = new int[] { 85, 22, 32, 46, 59, 107, 122, 105 };
        if (InGroup(GroundKeyMap, KeyMap) || InGroup(AirKeyMap, KeyMap)) {
            VirtualInput v = new VirtualInput();
            v.key = EKeyList.KL_Attack;
            skill.Add(v);
            return skill;
        }

        //其他带方向连招的.
        int[] slash0Ground = new int[] { 3, 7, 11, 19, 37, 84, 49, 63, 75, 92, 98, 113, 125, 24 };//下A地面
        int[] slash0Air = new int[] { 24, 33, 47, 60, 72, 83, 106, 109, 126 };//下A空中
        if (InGroup(slash0Ground, KeyMap) || InGroup(slash0Air, KeyMap)) {
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyS;
            skill.Add(s);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash1Ground = new int[] { 16, 28, 38, 64, 89, 100 };//左攻击
        if (InGroup(slash1Ground, KeyMap)) {
            VirtualInput a = new VirtualInput();
            a.key = EKeyList.KL_KeyA;
            skill.Add(a);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }
        //int[] slash1Air = new int[] { };//左攻击无空中招式
        int[] slash2Ground = new int[] { 15, 29, 39, 65, 93, 99 };//右攻击
        if (InGroup(slash2Ground, KeyMap)) {
            VirtualInput d = new VirtualInput();
            d.key = EKeyList.KL_KeyD;
            skill.Add(d);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        //int[] slash2Air = new int[] { };//右攻击无空中招式
        int[] slash3Ground = new int[] { 2, 6, 10, 14, 26, 36, 50, 62, 74, 96, 124 };//上攻击
        int[] slash3Air = new int[] { 87, 23 };//上攻击空中招式
        if (InGroup(slash3Ground, KeyMap) || InGroup(slash3Air, KeyMap)) {
            VirtualInput w = new VirtualInput();
            w.key = EKeyList.KL_KeyW;
            skill.Add(w);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash4Ground = new int[] { 88, 40, 52, 68, 78, 94, 114, 140, 90 };//下下攻击
        int[] slash4Air = new int[] { 150, 133 };//下下攻击空中
        if (InGroup(slash4Ground, KeyMap) || InGroup(slash4Air, KeyMap)) {
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyS;
            VirtualInput ss = new VirtualInput();
            ss.key = EKeyList.KL_KeyS;
            skill.Add(s);
            skill.Add(ss);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash5Ground = new int[] { 20, 27, 43, 54, 66, 77, 97, 117, 137, 160 };//上上攻击
        int[] slash5Air = new int[] { 130 };//上上攻击空中
        if (InGroup(slash5Ground, KeyMap) || InGroup(slash5Air, KeyMap)) {
            VirtualInput w = new VirtualInput();
            w.key = EKeyList.KL_KeyW;
            VirtualInput ww = new VirtualInput();
            ww.key = EKeyList.KL_KeyW;
            skill.Add(w);
            skill.Add(ww);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash6Ground = new int[] { 139, 111 };//左左攻击
        int[] slash6Air = new int[] { 132 };//左左攻击空中
        if (InGroup(slash6Ground, KeyMap) || InGroup(slash6Air, KeyMap)) {
            VirtualInput a = new VirtualInput();
            a.key = EKeyList.KL_KeyA;
            VirtualInput aa = new VirtualInput();
            aa.key = EKeyList.KL_KeyA;
            skill.Add(a);
            skill.Add(aa);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash7Ground = new int[] { 138, 112 };//右右攻击
        int[] slash7Air = new int[] { 131 };//右右攻击空中
        if (InGroup(slash7Ground, KeyMap) || InGroup(slash7Air, KeyMap)) {
            VirtualInput d = new VirtualInput();
            d.key = EKeyList.KL_KeyD;
            VirtualInput dd = new VirtualInput();
            dd.key = EKeyList.KL_KeyD;
            skill.Add(d);
            skill.Add(dd);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash8Ground = new int[] { 143, 147, 145, 18, 30, 41, 53, 67, 76, 102, 159, 116, 134 };//下上攻击
        int[] slash8Air = new int[] { 144, 146 };//下上攻击空中
        if (InGroup(slash8Ground, KeyMap) || InGroup(slash8Air, KeyMap)) {
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyS;
            VirtualInput w = new VirtualInput();
            w.key = EKeyList.KL_KeyW;
            skill.Add(s);
            skill.Add(w);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash9Ground = new int[] { 42, 103, 118, 127 };//上下攻击
        int[] slash9Air = new int[] { 34 };//上下攻击空中
        if (InGroup(slash9Ground, KeyMap) || InGroup(slash9Air, KeyMap)) {
            VirtualInput w = new VirtualInput();
            w.key = EKeyList.KL_KeyW;
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyS;
            skill.Add(w);
            skill.Add(s);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash10Ground = new int[] { 17, 55, 151, 69, 79, 110, 142 };//左右攻击
        if (InGroup(slash10Ground, KeyMap)) {
            VirtualInput a = new VirtualInput();
            a.key = EKeyList.KL_KeyA;
            VirtualInput d = new VirtualInput();
            d.key = EKeyList.KL_KeyD;
            skill.Add(a);
            skill.Add(d);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        //int[] slash10Air = new int[] {  };//左右攻击空中
        int[] slash11Ground = new int[] { 152, 51, 155, 80 };//右左攻击
        if (InGroup(slash11Ground, KeyMap)) {
            VirtualInput a = new VirtualInput();
            a.key = EKeyList.KL_KeyA;
            VirtualInput d = new VirtualInput();
            d.key = EKeyList.KL_KeyD;
            skill.Add(d);
            skill.Add(a);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        //int[] slash11Air = new int[] { };//右左攻击空中
        int[] slash12Ground = new int[] { 71, 81, 120, 128, 154 };//左右下攻击
        if (InGroup(slash12Ground, KeyMap)) {
            VirtualInput a = new VirtualInput();
            a.key = EKeyList.KL_KeyA;
            VirtualInput d = new VirtualInput();
            d.key = EKeyList.KL_KeyD;
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyS;
            skill.Add(a);
            skill.Add(d);
            skill.Add(s);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash13Ground = new int[] { 21 };//下左右A
        if (InGroup(slash13Ground, KeyMap)) {
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyS;
            VirtualInput a = new VirtualInput();
            a.key = EKeyList.KL_KeyA;
            VirtualInput d = new VirtualInput();
            d.key = EKeyList.KL_KeyD;
            skill.Add(s);
            skill.Add(a);
            skill.Add(d);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash14Ground = new int[] { 70, 115, 148, 45, 57, 157, 135 };//左右上
        if (InGroup(slash14Ground, KeyMap)) {
            VirtualInput a = new VirtualInput();
            a.key = EKeyList.KL_KeyA;
            VirtualInput d = new VirtualInput();
            d.key = EKeyList.KL_KeyD;
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyW;
            skill.Add(a);
            skill.Add(d);
            skill.Add(s);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash15Ground = new int[] { 121, 56, 104, 129, 156, 82, 149 };//下下上
        if (InGroup(slash15Ground, KeyMap)) {
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyS;
            VirtualInput ss = new VirtualInput();
            ss.key = EKeyList.KL_KeyS;
            VirtualInput w = new VirtualInput();
            w.key = EKeyList.KL_KeyW;
            skill.Add(s);
            skill.Add(ss);
            skill.Add(w);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }

        int[] slash16Ground = new int[] { 158, 58, 119, 4, 8, 12, 31, 44, 141 };//下上上
        if (InGroup(slash16Ground, KeyMap)) {
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyS;
            VirtualInput w = new VirtualInput();
            w.key = EKeyList.KL_KeyW;
            VirtualInput ww = new VirtualInput();
            ww.key = EKeyList.KL_KeyW;
            skill.Add(s);
            skill.Add(w);
            skill.Add(ww);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }
        int[] slash17Ground = new int[] { 153 };//上上上-枪绝招
        if (InGroup(slash17Ground, KeyMap)) {
            VirtualInput w = new VirtualInput();
            w.key = EKeyList.KL_KeyW;
            VirtualInput ww = new VirtualInput();
            ww.key = EKeyList.KL_KeyW;
            VirtualInput www = new VirtualInput();
            www.key = EKeyList.KL_KeyW;
            skill.Add(w);
            skill.Add(ww);
            skill.Add(www);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }
        int[] slash18Ground = new int[] { 136 };//左右下上-忍刀绝招
        if (InGroup(slash18Ground, KeyMap)) {
            VirtualInput a = new VirtualInput();
            a.key = EKeyList.KL_KeyA;
            VirtualInput d = new VirtualInput();
            d.key = EKeyList.KL_KeyD;
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyS;
            VirtualInput w = new VirtualInput();
            w.key = EKeyList.KL_KeyW;
            skill.Add(a);
            skill.Add(d);
            skill.Add(s);
            skill.Add(w);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j);
            return skill;
        }
        return skill;
    }
}
//切换武器规则
//2类
/*
 * 1：按射程决定使用近战或者远程武器
 * 2：切换到追杀模式，强制使用近战武器.
 */
/*
 * 	class.Name = "火铳兵﹒乙";
	class.Model =	5;
	class.Weapon = 12;
	class.Weapon2 = 14;
	class.Team = 2;=>0不分队伍 1流星 2蝴蝶
	class.View = 1500;=>视野100`2000
	class.Think = 100;=>反应 0`100
	class.Attack1	= 10;轻攻击  (攻击+防守几率< 100)
	class.Attack2 = 50;中攻击
	class.Attack3 = 20;重攻击
	class.Guard =	20;防御几率 0`100
	class.Dodge =	0;逃跑几率 0`100
	class.Jump = 10;跳跃几率
	class.Look = 60;张望几率
	class.Burst = 10;速冲几率
	class.Aim = 90;枪械射击命中率.
	class.GetItem = 0;夺宝几率
	class.Spawn = 88;
	class.HP = 1500;
 */
namespace Idevgame.Meteor.AI
{
    //状态切换时，如果要在完成时，再切回来，要填充这个
    public class StateContext {
        public State returnState;
        public object returnData;

    }
    //组行为-差异化各个角色的行为，否则各个AI行为一致，感觉机械.
    //只是控制各个StateMachine的某个行为是否允许随机到.
    //战斗圈-
    //包围圈-不允许进入战斗，在角色附近移动
    public class StateMachineGroup
    {

    }

    public enum NavPathStatus
    {
        NavPathNone,//还未有寻找所需数据
        NavPathNew,//开始新的寻找
        NavPathCalc,//进行查询中
        NavPathInvalid,//路径不存在
        NavPathComplete,//成功找到路径
        NavPathOrient,//朝向检查过程
        NavPathIterator,//遍历路点过程中
        NavPathToTarget,//从最后一个路点，走向目标的过程
        NavPathFinished,//寻路过程完整结束
    }

    public enum NavType
    {
        NavFindUnit,//寻找角色-靠近到攻击范围内时停止寻路
        NavFindPosition,//寻找位置-靠近到非常近时停止寻路
    }

    public class StateMachine
    {
        //当Think为100时,1S一个行为检测,1000时0.1s为1个
        public EventBus EventBus;
        float ThinkRound = 1;//1 / Think
        float ThickTick = 0;
        public MeteorUnit Player;
        private State PreviousState;
        private State NextState;
        public State CurrentState;


        public ReviveState ReviveState;//队长复活队友
        public IdleState IdleState;//原地等-不扫描目标
        public WaitState WaitState;//在原地等-扫描目标
        public GuardState GuardState;//原地防御
        public DodgeState DodgeState;//处于危险中，逃跑.如果仍被敌人追上，有可能触发决斗-如果脱离了战斗视野，可能继续逃跑
        public KillState KillState;//强制追杀 无视视野.
        public PatrolState PatrolState;//巡逻.
        public FollowState FollowState;//跟随
        public FightState FightState;//在地面攻击状态-主要状态
        public FindState FindState;//寻找目标，并寻路到附近
        public FaceToState FaceToState;//面向目标过程,可能是路点，可能是目标角色.被其他状态使用的
        public PickUpState PickUpState;//拾取道具
        public LookState LookState;//离开目标-使用远程武器，且无法切换到近战武器时.
        public AttackState AttackState;//击打某个物件多少次
        public ActionType ActionIndex;//角色当前的行为编号-轻招式 重招式 绝招
        public float Time;//角色当前所处状态经过时间长度
        public float ExitTime;//状态持续的时间-到达后退出该状态.
        protected List<VirtualInput> InputQueue = new List<VirtualInput>();
        protected int InputIndex = 0;
        //战斗中
        //目标置入初始状态.
        public void Init(MeteorUnit Unit)
        {
            EventBus = new EventBus();
            Player = Unit;
            IdleState = new IdleState(this);
            WaitState = new WaitState(this);
            GuardState = new GuardState(this);
            KillState = new KillState(this);
            PatrolState = new PatrolState(this);
            ReviveState = new ReviveState(this);
            FollowState = new FollowState(this);
            FightState = new FightState(this);
            DodgeState = new DodgeState(this);
            FindState = new FindState(this);
            FaceToState = new FaceToState(this);
            PickUpState = new PickUpState(this);
            LookState = new LookState(this);
            AttackState = new AttackState(this);
            Path = new PathPameter(this);
            InitFightWeight();
            ThinkRound = 10.0f / Player.Attr.Think;
            ThinkRound = Mathf.Clamp(ThinkRound, 0.05f, 0.5f);//Think限制在20-100内，0.5秒1次/0.05秒1次
            float dis = Player.Attr.View / 2;
            DistanceFindUnit = dis * dis;
            DistanceMissUnit = (dis + dis / 2) * (dis + dis / 2);
            stoped = false;
        }

        //得到寻路结果，所有状态共用.
        public PathPameter Path;
        public bool IsFighting() {
            if (CurrentState != null) {
                FightState fs = CurrentState as FightState;
                return fs != null;
            }
            return false;
        }

        public bool IsAttacking()
        {
            if (CurrentState != null)
            {
                AttackState fs = CurrentState as AttackState;
                return fs != null;
            }
            return false;
        }

        public bool stoped { get; set; }
        bool paused = false;
        float pause_tick;

        //受到攻击，处于受击动作或者防御姿态硬直中
        public void Pause(bool pause, float pause_time)
        {
            paused = pause;
            pause_tick = pause_time;
            if (paused)
            {
                Stop();
            }
        }

        //处理输入队列
        bool ProcessInput() {
            if (InputQueue.Count == 0)
                return false;
            if (InputQueue[InputIndex].state == VirtualKeyState.None) {
                Player.meteorController.Input.OnKeyDownProxy(InputQueue[InputIndex].key);
                InputQueue[InputIndex].state = VirtualKeyState.Press;
            } else if (InputQueue[InputIndex].state == VirtualKeyState.Press) {
                if (InputQueue[InputIndex].delay > 0)
                    InputQueue[InputIndex].delay -= 1;
                if (InputQueue[InputIndex].delay <= 0) {
                    Player.meteorController.Input.OnKeyUpProxy(InputQueue[InputIndex].key);
                    InputQueue[InputIndex].state = VirtualKeyState.Release;
                }
            } else if (InputQueue[InputIndex].state == VirtualKeyState.Release) {
                InputIndex += 1;
                if (InputIndex == InputQueue.Count) {
                    InputQueue.Clear();
                    return false;
                }
            }
            return true;
        }

        public void Update()
        {
            if (stoped)
                return;

            if (Player.Dead)
                return;

            //这个暂停是部分行为需要停止AI一段指定时间间隔
            if (paused)
            {
                Stop();
                pause_tick -= FrameReplay.deltaTime;
                if (pause_tick <= 0.0f)
                    paused = false;
                return;
            }

            if (Player.OnTouchWall)
                touchLast += FrameReplay.deltaTime;
            else
                touchLast = 0.0f;

            //动作还未初始化.
            if (Player.ActionMgr.mActiveAction == null)
                return;

            //如果在硬直中
            if (Player.ActionMgr.IsInStraight())
                return;

            if (ProcessInput())
                return;

            //切换武器中
            if (Player.ActionMgr.mActiveAction.Idx == CommonAction.ChangeWeapon || Player.ActionMgr.mActiveAction.Idx == CommonAction.AirChangeWeapon)
                return;

            State stateOld = CurrentState;
            //刷新状态的切换，在输入完毕后，再切换.
            if (CurrentState != null) {
                CurrentState.Update();
            }

            //即使Update里状态发生了切换，也继续进行.
            ThickTick -= FrameReplay.deltaTime;
            if (ThickTick > 0)
                return;
            ThickTick = ThinkRound;

            //倒地挣扎处理.放到Think里，间隔一会，不然硬直消失就可以在下一帧起身了
            if ((Player.ActionMgr.mActiveAction.Idx == CommonAction.Struggle || Player.ActionMgr.mActiveAction.Idx == CommonAction.Struggle0 || Player.ActionMgr.mActiveAction.Idx == CommonAction.Dead) && !Player.ActionMgr.InTransition()) {
                //随机输入方向-攻击-跳跃其中的一个按键
                List<VirtualInput> keys = new List<VirtualInput> { new VirtualInput(EKeyList.KL_Attack),new VirtualInput(EKeyList.KL_Jump),
                    new VirtualInput(EKeyList.KL_KeyW), new VirtualInput(EKeyList.KL_KeyS), new VirtualInput(EKeyList.KL_KeyA), new VirtualInput(EKeyList.KL_KeyD)};
                ReceiveInput(keys[Utility.Range(0, keys.Count)]);
            }

            //更新当前状态，内部自带状态切换.
            if (CurrentState != null) {
                CurrentState.Think();
            }
            //每当暗杀模式时，若自己是队长，寻找到死亡同伴，有一定概率去复活同伴
            //复活队友状态
            //战斗状态
            //待机状态
            //防御状态
            //四处看状态
            //拾取周围道具状态
            //躲避状态
            //巡逻状态
            //追杀状态-无视距离限定
            //跟随状态
            //寻路状态
        }

        //在空闲，跑步，武器准备，带毒跑时，可以复活队友.
        public bool CanChangeToRevive()
        {
            if (CombatData.Ins.GLevelMode == LevelMode.SinglePlayerTask) {
                return false;
            }

            if ((Player.ActionMgr.mActiveAction.Idx == CommonAction.Idle || 
                ActionManager.IsReadyAction(Player.ActionMgr.mActiveAction.Idx) ||
                Player.ActionMgr.mActiveAction.Idx == CommonAction.GunIdle ||
                Player.ActionMgr.mActiveAction.Idx == CommonAction.Run || 
                Player.ActionMgr.mActiveAction.Idx == CommonAction.RunOnDrug) && !Player.ActionMgr.InTransition())
            {
                if (Player.HasRebornTarget())
                    return true;
            }
            return false;
        }

        public void Enable(bool enable)
        {
            stoped = !enable;
            if (stoped)
                Stop();
        }

        //当切换了武器后.
        public void OnChangeWeapon()
        {
            if (CurrentState != null)
            {
                CurrentState.OnChangeWeapon();
            }
        }

        //初始化角色行为的权重,更新状态的每一帧，都在设置各个行为是否起效，不起效的
        public List<ActionWeight> Actions = new List<ActionWeight>();
        public void InitFightWeight()
        {
            Actions.Clear();
            Actions.Add(new ActionWeight(ActionType.Attack1, Player.Attr.Attack1));//攻击1
            Actions.Add(new ActionWeight(ActionType.Attack2, Player.Attr.Attack2));//连击2
            Actions.Add(new ActionWeight(ActionType.Attack3, Player.Attr.Attack3));//连击3
            Actions.Add(new ActionWeight(ActionType.Guard, Player.Attr.Guard));//防御
            Actions.Add(new ActionWeight(ActionType.Dodge, Player.Attr.Dodge));//闪避
            Actions.Add(new ActionWeight(ActionType.Jump, Player.Attr.Jump));//跳跃
            Actions.Add(new ActionWeight(ActionType.Burst, Player.Attr.Burst));//速冲
            Actions.Add(new ActionWeight(ActionType.GetItem, Player.Attr.GetItem));//视野范围内有物品-去拾取的几率
            Actions.Add(new ActionWeight(ActionType.Look, Player.Attr.Look));//视野范围内有物品-去拾取的几率
        }

        public void ResetInput() {
            InputQueue.Clear();
            InputIndex = 0;
        }

        //设置全部行为重置.都不可挑选
        public void ResetAction()
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i].enable = false;
            }
        }

        //单独设置某个行为在当前状态是无法使用的.
        public void SetActionTriggered(ActionType action, bool trigger)
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i].action == action)
                {
                    Actions[i].enable = trigger;
                    break;
                }
            }
        }

        //根据角色当前的环境，设置一些行为可否进行，比如拾取道具，复活队友.
        public void UpdateActionTriggers(bool allowAttack = true, bool allowAction = true)
        {
            //如果周围有可拾取的道具，那么更新可拾取状态
            //如果使用远程武器，更新是否可防御
            ResetAction();
            if (allowAction) {//普通行为
                SetActionTriggered(ActionType.Dodge, Player.Attr.Dodge != 0 && Player.WillDead && Player.IsOnGround());//能否随机到逃跑，以当前状态是否濒危决定.(敌方靠近而只有远程武器也会躲避)
                SetActionTriggered(ActionType.Guard, Player.Attr.Guard != 0 && !U3D.IsSpecialWeapon(Player.Attr.Weapon) && Player.IsOnGround());
                SetActionTriggered(ActionType.Burst, Player.Attr.Burst != 0 && Player.IsOnGround());
                SetActionTriggered(ActionType.GetItem, Player.Attr.GetItem != 0 && Player.TargetItem != null);
                SetActionTriggered(ActionType.Jump, Player.Attr.Jump != 0 && Player.IsOnGround());
                SetActionTriggered(ActionType.Look, Player.Attr.Look != 0 && Player.IsOnGround() && Player.LockTarget == null);
            }
            if (allowAttack) {//攻击行为
                SetActionTriggered(ActionType.Attack1, Player.Attr.Attack1 != 0);
                SetActionTriggered(ActionType.Attack2, Player.Attr.Attack2 != 0);
                SetActionTriggered(ActionType.Attack3, Player.Attr.Attack3 != 0);
            }
        }

        //按随机权重比，设置接下来的动作
        public void UpdateActionIndex()
        {
            int WeightSum = 0;
            for (int i = 0; i < Actions.Count; i++)
            {
                if (!Actions[i].enable)
                    continue;
                WeightSum += Actions[i].weight;
            }
            int rand = Utility.Range(0, WeightSum);
            ActionType ret = ActionType.None;
            int idx = 0;
            for (; idx < Actions.Count; idx++)
            {
                if (!Actions[idx].enable)
                    continue;
                rand -= Actions[idx].weight;
                if (rand < 0) {
                    ret = Actions[idx].action;
                    break;
                }
            }
            ActionIndex = ret;
        }

        void GunHeavyShoot() {
            //6/7行对应上A，下A，但是要看有没有气
            //7需要20气
            //能用7-下A就用
            if (Player.meteorController.Input.CheckPos(7, CommonAction.GunHeavyShootMax)) {
                InputQueue = VirtualInput.CalcPoseInput(7);
                InputIndex = 0;
                return;
            }

            InputQueue = VirtualInput.CalcPoseInput(6);
            InputIndex = 0;
        }

        bool GunSkillShoot() {
            //8/147对应，下上上A，下上A，分别需要100气/60气
            if (Player.meteorController.Input.CheckPos(8, CommonAction.GunSkillShootMax)) {
                InputQueue = VirtualInput.CalcPoseInput(8);
                InputIndex = 0;
                return true;
            }
            if (Player.meteorController.Input.CheckPos(147, CommonAction.GunSkillShoot)) {
                InputQueue = VirtualInput.CalcPoseInput(147);
                InputIndex = 0;
                return true;
            }
            return false;
        }

        //后前A，或者前后A，收刀-快速拔刀
        void Quick() {
            int[] pose = new int[] {102,103};//KeyMap
            int[] action = new int[] { 449, 450 };//Action
            int rand = Utility.Range(0, 2);
            if (Player.meteorController.Input.CheckPos(pose[rand], action[rand])){
                InputQueue = VirtualInput.CalcPoseInput(pose[rand]);
                InputIndex = 0;
            }
        }

        public bool DoAction()
        {
            ActionNode act = ActionInterrupt.Ins.GetActions(Player.ActionMgr.mActiveAction.Idx);
            ActionNode node = null;
            List<ActionNode> nodelist = null;
            if (act == null && ActionIndex >= ActionType.Attack1 && ActionIndex <= ActionType.Attack3) {
                //如果是远程武器，看是哪个远程武器，选择对应的招式集合，使用
                if (Player.GetWeaponType() == (int)EquipWeaponType.Gun) {
                    switch (ActionIndex) {
                        case ActionType.Attack1:
                            ReceiveInput(new VirtualInput(EKeyList.KL_Attack));
                            return true;
                        case ActionType.Attack2:
                            GunHeavyShoot();
                            return true;
                        case ActionType.Attack3:
                            bool use = GunSkillShoot();
                            return use;
                    }
                } else if (Player.ActionMgr.mActiveAction.Idx == 486 || Player.ActionMgr.mActiveAction.Idx == 485) {
                    //乾坤刀收刀姿势，可以接后前A，和前后A，快速拔刀
                    Quick();
                    return true;
                }
                else{
                    //AI是不具备快速拔刀的，乾坤刀485-486收刀，没有
                    Debug.LogError("某些武器，找不到起手招式:" + Player.ActionMgr.mActiveAction.Idx + " weapon:" + Player.GetWeaponType());
                }
                return false;
            }
            switch (ActionIndex)
            {
                case ActionType.None://没挑选到任何行为
                    break;
                //按键操作.
                case ActionType.Attack1:
                    node = ActionInterrupt.Ins.GetNormalNode(Player, act);
                    if (node != null) {
                        InputQueue = VirtualInput.CalcPoseInput(node.KeyMap);
                        InputIndex = 0;
                        return true;
                    }
                    break;
                case ActionType.Attack2:
                    nodelist = ActionInterrupt.Ins.GetSlashNode(Player, act);
                    //遍历一次，如果该招式无法使用，比如缺少气，那么从候选动作抛弃
                    if (nodelist != null && nodelist.Count != 0) {
                        int ran = Utility.Range(0, nodelist.Count);
                        node = nodelist[ran];
                        InputQueue = VirtualInput.CalcPoseInput(node.KeyMap);
                        InputIndex = 0;
                        return true;
                    }
                    break;
                case ActionType.Attack3:
                    node = ActionInterrupt.Ins.GetSkillNode(Player, act);
                    if (node != null) {
                        InputQueue = VirtualInput.CalcPoseInput(node.KeyMap);
                        InputIndex = 0;
                        return true;
                    }
                    break;
                case ActionType.Guard:
                    Player.Guard(true, Utility.Range(1, 4));
                    return true;
                case ActionType.Burst:
                    AIRush();
                    return true;
                case ActionType.Jump:
                    //需要精确的输入间隔，控制跳跃高度.
                    ReceiveInput(new VirtualInput(EKeyList.KL_Jump, 4));
                    return true;
                //状态切换.
                case ActionType.Dodge:
                    ChangeState(DodgeState, Player.LockTarget);
                    return true;
                case ActionType.GetItem:
                    ChangeState(PickUpState);
                    return true;
                case ActionType.Look:
                    ChangeState(LookState);
                    return true;
            }
            return false;
        }

        void AIRush() {
            int key = Utility.Range((int)EKeyList.KL_KeyW, (int)EKeyList.KL_KeyD + 1);
            InputQueue = new List<VirtualInput> { new VirtualInput((EKeyList)key), new VirtualInput((EKeyList)key) };
            InputIndex = 0;
        }

        void Rush() {
            int dir = Utility.Range(0, 4);
            if (Player.Crouching)
                Player.CrouchRush((RushDirection)dir);
            else
                Player.IdleRush((RushDirection)dir);
        }

        //硬切-强制重置状态.
        public void ChangeState(State Target, object data = null)
        {
            //if (CurrentState == FightState && Target == WaitState)
            //    Debug.Log("ChangeState:" + Target);
            if (CurrentState != Target)
            {
                NextState = Target;
                if (CurrentState != null) {
                    CurrentState.OnExit(NextState);
                }
                
                PreviousState = CurrentState;
                if (Target != null) {
                    Target.OnEnter(PreviousState, data);
                }
                
                CurrentState = Target;
            }
        }

        //丢失目标
        public bool LostTarget() {
            if (Player.LockTarget != null) {
                if (Player.LockTarget.Dead) {
                    Player.LockTarget = null;
                } else {
                    float d = (Player.LockTarget.transform.position - Player.transform.position).sqrMagnitude;
                    if (Player.LockTarget.HasBuff(EBUFF_Type.HIDE)) {
                        //隐身60码内可发现，2个角色紧贴着
                        if (d >= CombatData.PlayerLeave) {
                            //Debug.LogError("隐身角色离开视野");
                            Player.LockTarget = null;
                        }
                    } else {
                        if (d >= DistanceMissUnit)//超过距离以免不停的切换目标
                        {
                            //Debug.LogError("角色离开视野");
                            Player.LockTarget = null;
                        }
                    }
                }
            }
            return Player.LockTarget == null;
        }

        public bool HasInput() {
            return InputQueue.Count != 0;
        }

        public void ReceiveInput(VirtualInput input) {
            InputQueue.Clear();
            InputQueue.Add(input);
            InputIndex = 0;
        }

        //东西被其他人拾取了
        public bool LostItem() {
            if (!Player.TargetItem.CanPickup()) {
                Player.TargetItem = null;
            } else {
                float d = (Player.TargetItem.transform.position - Player.transform.position).sqrMagnitude;
                if (d > DistanceMissUnit)
                    Player.TargetItem = null;
            }
            return Player.TargetItem == null;
        }

        float DistanceFindUnit;//进入视野范围可观察到
        float DistanceMissUnit;//离开视野范围后丢失
        public void SelectTarget()
        {
            if (Player.LockTarget == null || Player.LockTarget.Dead)
                SelectEnemy();
            else {
                LostTarget();
            }
        }

        public void SelectItem() {
            if (Player.TargetItem == null) {
                SelectSceneItem();
            } else {
                LostItem();
            }
        }

        //选择一个敌方目标
        public void SelectEnemy()
        {
            Player.LockTarget = null;
            float angleMax = 75;//cos值越大，角度越小
            Collider[] other = Physics.OverlapSphere(Player.transform.position, Player.Attr.View / 2, LayerManager.PlayerMask);
            Vector3 vecPlayer = -1 * Player.transform.forward;
            vecPlayer.y = 0;
            //直接遍历算了,要计算面向，如果面向角度差大于75度，则无法选择该目标
            for (int i = 0; i < other.Length; i++)
            {
                MeteorUnit unit = other[i].GetComponent<MeteorUnit>();
                if (unit == null)
                    continue;
                if (unit == Player)
                    continue;
                if (Player.SameCamp(unit))
                    continue;
                if (unit.Dead)
                    continue;
                Vector3 vec = unit.transform.position - Player.transform.position;
                float v = vec.sqrMagnitude;
                //隐身只能在60码内发现目标
                if (unit.HasBuff(EBUFF_Type.HIDE))
                {
                    if (v > CombatData.PlayerEnter)
                        continue;
                }
                //如果玩家开启隐身模式，无法被找到
                if (GameStateMgr.Ins.gameStatus.HidePlayer) {
                    if (unit == Main.Ins.LocalPlayer)
                        continue;
                }

                //飞轮时，无限角度距离
                if (v > DistanceFindUnit && Main.Ins.LocalPlayer.GetWeaponType() != (int)EquipWeaponType.Guillotines)
                    continue;
                //高度差2个角色身高，无法选择
                if (Mathf.Abs(vec.y) >= 75 && Main.Ins.LocalPlayer.GetWeaponType() != (int)EquipWeaponType.Guillotines)
                    continue;
                vec.y = 0;
                //先判断夹角是否在限制范围内.
                vec = Vector3.Normalize(vec);
                float angle = Mathf.Acos(Vector3.Dot(vecPlayer.normalized, vec)) * Mathf.Rad2Deg;

                //角度小于75才可以选择.
                if (angle > angleMax && v > CombatData.DistanceSkipAngle) {
                    //Debug.LogError("角度大于75度,距离大于最小可感知范围时，过滤");
                    continue;
                }

                Vector3 vecDir = new Vector3();
                vecDir = unit.mSkeletonPivot - Player.mSkeletonPivot;
                //期望目标与自己之间有墙壁阻隔
                if (Physics.Raycast(Player.mSkeletonPivot, vecDir.normalized, vecDir.magnitude, LayerManager.AllSceneMask)) {
                    //Debug.LogError("与角色间有障碍物阻挡");
                    continue;
                }
                //可以选择
                //Debug.LogError(Player.name + "选择了敌方目标:" + unit.name);
                Player.LockTarget = unit;
            }
        }

        void SelectSceneItem()
        {
            float dis = DistanceFindUnit;
            int index = -1;
            SceneItemAgent tar = null;
            Collider[] coliiders = Physics.OverlapSphere(Player.transform.position, Player.Attr.View / 2, 1 << LayerManager.Trigger);
            //直接遍历算了
            for (int i = 0; i < coliiders.Length; i++)
            {
                SceneItemAgent item = coliiders[i].gameObject.GetComponentInParent<SceneItemAgent>();
                if (item == null)
                    continue;
                if (!item.CanPickup())
                    continue;
                float d = (Player.transform.position - item.transform.position).sqrMagnitude;
                if (dis > d)
                {
                    dis = d;
                    index = i;
                    tar = item;
                }
            }
            if (index >= 0 && index < MeteorManager.Ins.SceneItems.Count && tar != null)
                Player.TargetItem = tar;
        }

        public void Move() {
            Player.meteorController.Input.AIMove(0, 1);
        }

        public void Stop()
        {
            Player.meteorController.Input.AIMove(0, 0);
        }

        //角色挂了的时候
        public void OnUnitDead(MeteorUnit dead)
        {

        }

        //可能是被卡住.
        float touchLast = 0.0f;
        static float touchWallLimit = 1.0f;
        public void CheckStatus()
        {
            if (touchLast >= touchWallLimit)
            {
                //尝试跳跃一下
                ReceiveInput(new VirtualInput(EKeyList.KL_Jump, 5));
                ChangeState(WaitState);
                touchLast = 0.0f;
            }
        }

        //如果attacker为空，则代表是非角色伤害了自己
        Dictionary<MeteorUnit, float> Hurts = new Dictionary<MeteorUnit, float>();//仇恨值.战斗圈内谁的仇恨高，就选择谁.
        public void OnDamaged(MeteorUnit attacker)
        {
            if (attacker == null)
            {
                //Debug.LogError("attacker == null");
            }
            else
            {
                //计算攻击者对我造成的仇恨
                //attacker
            }

            ResetInput();
            Stop();
            Player.meteorController.Input.ResetInput();
            //攻击者在视野内，仅判断距离
            if (attacker != null) {
                if (Find(attacker)) {
                    if (Player.LockTarget == null)
                        Player.LockTarget = attacker;
                }
            }

            //某些状态被中断
            if (CurrentState == ReviveState)
                ChangeState(WaitState);
            if (CurrentState == LookState)
                ChangeState(WaitState);
        }

        //指定的对象是否在自己视野内
        public bool Find(MeteorUnit unit) {
            float d = (unit.transform.position - Player.transform.position).sqrMagnitude;
            if (unit.HasBuff(EBUFF_Type.HIDE)) {
                //隐身20码内可发现，2个角色相距较近
                if (d >= CombatData.PlayerEnter)
                    return false;
            } else {
                if (d >= (DistanceFindUnit))
                    return false;
            }
            return true;
        }

        public void FollowTarget(int target)
        {
            MeteorUnit followed = U3D.GetUnit(target);
            if (MeteorManager.Ins.LeavedUnits.ContainsKey(target))
                return;
            Player.FollowTarget = followed;
            ChangeState(FollowState);
        }

        public List<int> GetPatrolPath() {
            return PatrolState.patrolData;
        }

        public void SetPatrolPath(List<int> path)
        {
            PatrolState.SetPatrolPath(path);
        }
    }

    public abstract class State
    {
        public StateMachine Machine;
        protected State Previous;
        protected State Next;
        public MeteorUnit Player { get { return Machine.Player; } }
        public MeteorUnit LockTarget { get { return Machine.Player.LockTarget; } }
        public MeteorUnit FollowTarget { get { return Machine.Player.FollowTarget; } }
        public MeteorUnit KillTarget { get { return Machine.Player.KillTarget; } }
        //FindState与FollowState公用数据
        protected NavPathStatus navPathStatus;
        public int curIndex = 0;//为-1时，表示还未归位（走到自己所处路点位置.）
        public int targetIndex = 0;
        //进入时缓存下角色当前的位置，以后每个Think更新一次位置，未更新则用上次缓存的位置，一直到离角色很近为止.
        protected Vector3 positionStart;
        protected Vector3 positionEnd;
        protected Vector3 TargetPos;
        protected int positionStartIndex;
        protected int positionEndIndex;//终点所在的路点.
        protected NavType NavType;

        protected EAISubStatus subState;//子状态        
        protected Vector3 NextFramePos = Vector3.zero;//计算出下一帧的位置.

        public State(StateMachine machine)
        {
            Machine = machine;
            subState = EAISubStatus.None;
        }

        //朝角色面向跑
        public void Move() {
            Machine.Move();
        }

        public void Stop() {
            Machine.Stop();
        }

        //朝目标点跳跃
        protected void Jump(Vector3 vec) {
            float height = vec.y - Player.transform.position.y;
            vec.y = 0;
            Vector3 vec2 = Player.transform.position;
            vec2.y = 0;
            float sz = Vector3.Distance(vec, vec2);
            Player.Jump2(Mathf.Abs(height));
            Player.SetVelocity(sz / (2 * Player.ImpluseVec.y / CombatData.Ins.gGravity), 0);
        }

        public virtual void OnEnter(State pevious, object data = null)
        {
            Previous = pevious;
        }

        public virtual void OnExit(State next)
        {
            Next = next;
            Machine.Stop();
        }

        //每一帧执行一次
        public virtual void Update()
        {
            AutoChangeState();
        }

        public virtual void AutoChangeState() {
            if (Player.IsLeader && Machine.CanChangeToRevive()) {
                ChangeState(Machine.ReviveState);
            }
            else if (Player.KillTarget != null && !Player.KillTarget.Dead) {
                ChangeState(Machine.KillState);
            } else if (Player.LockTarget != null && !Player.LockTarget.Dead) {
                ChangeState(Machine.FightState);
            } else if (Player.FollowTarget != null && !Player.FollowTarget.Dead &&
                Vector3.SqrMagnitude(FollowTarget.mSkeletonPivot - Player.mSkeletonPivot) > CombatData.FollowDistanceStart) {
                ChangeState(Machine.FollowState);
            } else if (Machine.PatrolState.HasPatrolData()) {
                ChangeState(Machine.PatrolState);
            } else {
                //啥也不干
            }
        }

        //当我向前方10米往下发射线可以碰到死亡区域时,不要继续去寻找物品了.
        protected bool NavCheck() {
            //forward指向角色背后朝向
            Vector3 pos = Player.transform.position - Player.transform.forward * 85 + Vector3.up * 5;
            RaycastHit hit;
            if (Physics.Raycast(pos, Vector3.down, out hit, 1000, LayerManager.AllSceneMask)) {
                MapArea mapArea = hit.collider.GetComponent<MapArea>();
                if (mapArea != null && mapArea.type == MapAreaType.Die) {
                    //方向旋转一下,再进入待机状态
                    Vector3 lookAt = Player.transform.position + Player.transform.forward * 10;
                    ChangeState(Machine.FaceToState, lookAt);
                    return true;
                }
            }
            return false;
        }

        //一段时间执行一次,出招的频率等
        public virtual void Think()
        {
            
        }

        public virtual void OnChangeWeapon()
        {
            //武器发生变化,
        }

        public void ChangeState(State target, object data = null)
        {
            Machine.ChangeState(target, data);
        }

        protected void RotateToTarget(Vector3 vec, float time, float duration, float angle, ref float offset0, ref float offset1, bool rightRotate)
        {
            if (vec.x == Player.transform.position.x && vec.z == Player.transform.position.z)
                return;
            float offsetmax = angle;
            float timeTotal = duration;
            float timeTick = time;
            if (rightRotate)
                offset1 = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            else
                offset1 = -Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            Player.SetOrientation(offset1 - offset0);
            offset0 = offset1;
        }

        protected void NavThink()
        {
            switch (navPathStatus)
            {
                case NavPathStatus.NavPathNew:
                    navPathStatus = NavPathStatus.NavPathCalc;
                    PathHelper.Ins.CalcPath(Machine, positionStart, positionEnd);
                    break;
                case NavPathStatus.NavPathCalc:
                    //等待寻路线程的处理.
                    if (Machine.Path.state != 0)
                        navPathStatus = NavPathStatus.NavPathComplete;
                    break;
                case NavPathStatus.NavPathComplete:
                    navPathStatus = NavPathStatus.NavPathOrient;
                    if (Machine.Path.ways.Count == 0)
                    {
                        //寻路可直达，
                        curIndex = 0;
                        TargetPos = positionEnd;
                    }
                    else
                    {
                        curIndex = 0;
                        targetIndex = curIndex + 1;
                        TargetPos = Machine.Path.ways[curIndex].pos;
                    }
                    break;
                case NavPathStatus.NavPathInvalid:
                    ChangeState(Machine.WaitState);
                    break;
                case NavPathStatus.NavPathIterator:
                    //调度过程-从当前位置，到目标位置的寻路过程.
                    break;
            }
        }

        protected virtual void OnNavFinished() {

        }

        protected virtual void NavUpdate()
        {
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
                    if (NextFramePos.sqrMagnitude <= CombatData.AttackRange) {
                        navPathStatus = NavPathStatus.NavPathFinished;
                        Stop();
                        OnNavFinished();
                        return;
                    }
                } else {
                    if (curIndex == Machine.Path.ways.Count - 1) {
                        NextFramePos = TargetPos - Player.mSkeletonPivot;
                        NextFramePos.y = 0;
                        if (NextFramePos.sqrMagnitude <= CombatData.StopDistance) {
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
                        if (NextFramePos.sqrMagnitude <= CombatData.StopDistance) {
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
                if (NextFramePos.sqrMagnitude <= CombatData.AttackRange) {
                    navPathStatus = NavPathStatus.NavPathFinished;
                    //UnityEngine.Debug.LogError("寻路完毕");
                    Stop();
                    OnNavFinished();
                    return;
                }
                Player.FaceToTarget(TargetPos);
                Move();
            }
        }
    }
}
