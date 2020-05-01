using Assets.Code.Idevgame.Common.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    //组行为-差异化各个角色的行为，否则各个AI行为一致，感觉机械.
    //只是控制各个StateMachine的某个行为是否允许随机到.
    //战斗圈-
    //包围圈-不允许进入战斗，在角色附近移动
    public class StateMachineGroup
    {

    }

    public enum NavPathStatus
    {
        NavPathNone,//还未开始寻找
        NavPathCalc,//进行查询中
        NavPathInvalid,//路径不存在
        NavPathComplete,//成功找到路径
        NavPathOrient,//朝向检查过程
        NavPathIterator,//遍历路点过程中
        NavPathFinished,//寻路过程完整结束
    }

    public enum NavType
    {
        NavFindUnit,//寻找角色-靠近到攻击范围内时停止寻路
        NavFindPosition,//寻找位置-靠近到非常近时停止寻路
    }

    public class StateMachine
    {
        //当Think为100时,0.1S一个行为检测,行为频率慢,则连招可能连不起来.行为频率快, 则每个招式在可切换招式的时机, 进行连招的几率越大.
        public EventBus EventBus;
        static readonly float ThinkRound = 1000;
        float ThickTick = 0.0f;
        List<AIVirtualInput> InputKeys = new List<AIVirtualInput>();
        public MeteorUnit Player;
        private State PreviousState;
        private State NextState;
        public State CurrentState;

        public State ReviveState;//队长复活队友
        public WaitState WaitState;//等待状态-原地发呆
        public IdleState IdleState;//在原地等待目标进入视野
        public GuardState GuardState;//原地防御
        public LookState LookState;//四周观察-未发现敌人时.
        public DodgeState DodgeState;//处于危险中，逃跑.如果仍被敌人追上，有可能触发决斗-如果脱离了战斗视野，可能继续逃跑
        public KillState KillState;//强制追杀 无视视野.
        public PatrolState PatrolState;//巡逻.
        public FollowState FollowState;//跟随
        public FightOnGroundState FightOnGroundState;//在地面攻击状态-主要状态
        public FightOnAirState FightOnAirState;//在空中-次要
        public FindState FindState;//寻找目标，并寻路到附近
        public FaceToState FaceToState;//面向目标过程,可能是路点，可能是目标角色.被其他状态使用的
        public PickUpState PickUpState;//拾取道具
        public LeaveState LeaveState;//离开目标-使用远程武器，且无法切换到近战武器时.
        public ActionType ActionIndex;//角色当前的行为编号-轻招式 重招式 绝招

        public float BaseTime;//角色当前动作的归一化时间 大于0部分是循环次数，小于0部分是单次播放百分比.
        public int AnimationIndex;//角色当前动画编号
        public int WeaponIndex;//角色当前武器ID

        //战斗中
        //目标置入初始状态.
        public void Init(MeteorUnit Unit)
        {
            EventBus = new EventBus();
            Player = Unit;
            WaitState = new WaitState(this);
            IdleState = new IdleState(this);
            GuardState = new GuardState(this);
            KillState = new KillState(this);
            PatrolState = new PatrolState(this);
            ReviveState = new ReviveState(this);
            FollowState = new FollowState(this);
            FightOnGroundState = new FightOnGroundState(this);
            FightOnAirState = new FightOnAirState(this);
            LookState = new LookState(this);
            DodgeState = new DodgeState(this);
            FindState = new FindState(this);
            FaceToState = new FaceToState(this);
            PickUpState = new PickUpState(this);
            LeaveState = new LeaveState(this);
            InitFightWeight();
            int dis = Player.Attr.View;
            DistanceFindUnit = dis * dis;
            DistanceMissUnit = (dis + dis / 2) * (dis + dis / 2);
            this.EnterDefaultState();
            stoped = false;
        }

        //得到寻路结果，所有状态共用.
        public PathPameter Path;
        public void OnPathCalcFinished(PathPameter pameter)
        {
            Path = pameter;
        }

        bool HasInput()
        {
            return InputKeys.Count != 0;
        }

        //输入中.
        void OnInput(int timer)
        {
            Player.controller.Input.OnKeyDownProxy(InputKeys[0].key, true);
            Player.controller.Input.OnKeyUpProxy(InputKeys[0].key);
            InputKeys.RemoveAt(0);
        }

        void EnterDefaultState()
        {
            CurrentState = IdleState;
            CurrentState.OnEnter(null);
        }

        public bool IsFighting()
        {
            if (CurrentState != null)
            {
                FightOnGroundState fs = CurrentState as FightOnGroundState;
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
                //StopCoroutine();
            }
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
                Player.controller.Input.AIMove(0, 0);
                pause_tick -= FrameReplay.deltaTime;
                if (pause_tick <= 0.0f)
                    paused = false;
                return;
            }

            ThickTick -= Player.Attr.Think;

            if (Player.OnTouchWall)
                touchLast += FrameReplay.deltaTime;
            else
                touchLast = 0.0f;

            //如果在硬直中
            if (Player.charLoader.IsInStraight())
                return;

            //输入队列中存在指令.(有待输入的指令能否继续?-应该是只能等待被强制打断，否则一定要等输入完毕)
            if (HasInput())
            {
                UpdateInput();
                return;
            }

            CurrentState.Update();
            ThickTick -= Player.Attr.Think;
            if (ThickTick > 0)
                return;
            ThickTick = ThinkRound;

            //更新当前状态，内部自带状态切换.
            CurrentState.Think();//每当暗杀模式时，若自己是队长，寻找到死亡同伴，有一定概率去复活同伴
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
        bool CanChangeToRevive()
        {
            if (Player.posMng.mActiveAction.Idx == CommonAction.Idle || PoseStatus.IsReadyAction(Player.posMng.mActiveAction.Idx) || Player.posMng.mActiveAction.Idx == CommonAction.Run || Player.posMng.mActiveAction.Idx == CommonAction.RunOnDrug)
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
        List<ActionWeight> Actions = new List<ActionWeight>();
        public void InitFightWeight()
        {
            Actions.Clear();
            Actions.Add(new ActionWeight(ActionType.Attack1, Player.Attr.Attack1));//攻击1
            Actions.Add(new ActionWeight(ActionType.Attack2, Player.Attr.Attack2));//连击2
            Actions.Add(new ActionWeight(ActionType.Attack3, Player.Attr.Attack3));//连击3
            Actions.Add(new ActionWeight(ActionType.Guard, Player.Attr.Guard));//防御
            Actions.Add(new ActionWeight(ActionType.Dodge, Player.Attr.Dodge));//闪避
            Actions.Add(new ActionWeight(ActionType.Jump, Player.Attr.Jump));//跳跃
            Actions.Add(new ActionWeight(ActionType.Look, Player.Attr.Look));//观看四周
            Actions.Add(new ActionWeight(ActionType.Burst, Player.Attr.Burst));//速冲
            Actions.Add(new ActionWeight(ActionType.GetItem, Player.Attr.GetItem));//视野范围内有物品-去拾取的几率
        }

        //设置全部行为重置.都可挑选
        public void ResetAction()
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i].enable = true;
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
        public void UpdateEnviroument()
        {
            //如果周围有可拾取的道具，那么更新可拾取状态
            //如果使用远程武器，更新是否可防御
            
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
            int rand = UnityEngine.Random.Range(0, WeightSum);
            int idx = 0;
            for (; idx < Actions.Count - 1; idx++)
            {
                if (!Actions[idx].enable)
                    continue;
                rand -= Actions[idx].weight;
                if (rand < 0)
                    break;
            }
            ActionIndex = Actions[idx].action;
        }

        public void DoAction()
        {
            switch (ActionIndex)
            {
                //按键操作.
                case ActionType.Attack1:
                    break;
                case ActionType.Attack2:
                    break;
                case ActionType.Attack3:
                    break;
                case ActionType.Guard:
                    break;
                case ActionType.Burst:
                    break;
                case ActionType.Jump:
                    break;
                //状态切换.
                case ActionType.Dodge:
                    ChangeState(DodgeState);
                    break;
                case ActionType.GetItem:
                    ChangeState(PickUpState);
                    break;
                case ActionType.Look:
                    ChangeState(LookState);
                    break;
            }
        }

        //软切-状态自上次状态继续运行.
        public void ResumeState(State Target, object data = null)
        {
            if (CurrentState != Target)
            {
                NextState = Target;
                CurrentState.OnPause(NextState);
                PreviousState = CurrentState;
                Target.OnResume(PreviousState, data);
                CurrentState = Target;
            }
        }

        //硬切-强制重置状态.
        public void ChangeState(State Target, object data = null)
        {
            if (CurrentState != Target)
            {
                NextState = Target;
                CurrentState.OnExit(NextState);
                PreviousState = CurrentState;
                Target.OnEnter(PreviousState, data);
                CurrentState = Target;
            }
        }

        float DistanceFindUnit;//进入视野范围可观察到
        float DistanceMissUnit;//离开视野范围后丢失
        public void RefreshTarget()
        {
            if (Player.LockTarget == null || Player.LockTarget.Dead)
                SelectEnemy();
            else
            {
                //死亡，失去视野，超出视力范围，重新选择
                if (Player.KillTarget == Player.LockTarget)
                {
                    //强制杀死还存活的角色时，不会丢失目标.
                }
                else
                {
                    float d = Vector3.SqrMagnitude(Player.LockTarget.transform.position - Player.transform.position);
                    if (Player.LockTarget.HasBuff(EBUFF_Type.HIDE))
                    {
                        //隐身60码内可发现，2个角色紧贴着
                        if (d >= 3600.0f)
                        {
                            Player.LockTarget = null;
                        }
                    }
                    else
                    {
                        if (d >= DistanceMissUnit)//超过距离以免不停的切换目标
                            Player.LockTarget = null;
                    }
                }
            }

            if (Player.TargetItem == null)
                SelectSceneItem();
            else
            {
                if (!Player.TargetItem.CanPickup())
                {
                    Player.TargetItem = null;
                }
                else
                {
                    float d = Vector3.SqrMagnitude(Player.TargetItem.transform.position - Player.transform.position);
                    if (d > DistanceMissUnit)
                        Player.TargetItem = null;
                }
            }
        }

        //选择一个敌方目标
        public void SelectEnemy()
        {
            Player.LockTarget = null;
            float dis = DistanceFindUnit;//视野，可能指的是直径，这里变为半径,平方比开方快.
            int index = -1;
            MeteorUnit tar = null;
            Collider[] other = Physics.OverlapSphere(Player.transform.position, Player.Attr.View, 1 << LayerMask.NameToLayer("Monster") | 1 << LayerMask.NameToLayer("LocalPlayer"));

            //直接遍历算了
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

                float d = Vector3.SqrMagnitude(Player.transform.position - unit.transform.position);
                //隐身只能在60码内发现目标
                if (unit.HasBuff(EBUFF_Type.HIDE))
                {
                    if (d > 3600.0f)
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
                Player.LockTarget = tar;
        }

        void SelectSceneItem()
        {
            float dis = DistanceFindUnit;
            int index = -1;
            SceneItemAgent tar = null;
            //直接遍历算了
            for (int i = 0; i < Main.Ins.MeteorManager.SceneItems.Count; i++)
            {
                SceneItemAgent item = Main.Ins.MeteorManager.SceneItems[i].gameObject.GetComponent<SceneItemAgent>();
                if (item == null)
                    continue;
                if (!item.CanPickup())
                    continue;
                float d = Vector3.SqrMagnitude(Player.transform.position - item.transform.position);
                if (dis > d)
                {
                    dis = d;
                    index = i;
                    tar = item;
                }
            }
            if (index >= 0 && index < Main.Ins.MeteorManager.SceneItems.Count && tar != null)
                Player.TargetItem = tar;
        }

        public void Stop()
        {
            Player.controller.Input.AIMove(0, 0);
        }

        public void OnUnitDead(MeteorUnit dead)
        {

        }

        //自动状态切换
        public void AutoChangeState()
        {
            if (Player.KillTarget != null && !Player.KillTarget.Dead)
            {
                ChangeState(KillState);
            }
            else if (Player.LockTarget != null && !Player.LockTarget.Dead)
            {
                ChangeState(FightOnGroundState);
            }
            else if (Player.FollowTarget != null && !Player.FollowTarget.Dead)
            {
                ChangeState(FollowState);
            }
        }
        //可能是被卡住.
        float touchLast = 0.0f;
        const float touchWallLimit = 1.0f;
        public void CheckStatus()
        {
            if (touchLast >= touchWallLimit)
            {
                
                touchLast = 0.0f;
            }
        }

        //如果attacker为空，则代表是非角色伤害了自己
        Dictionary<MeteorUnit, float> Hurts = new Dictionary<MeteorUnit, float>();//仇恨值.战斗圈内谁的仇恨高，就选择谁.
        public void OnDamaged(MeteorUnit attacker)
        {
            if (attacker == null)
            {

            }
            else
            {
                //计算攻击者对我造成的仇恨
                //attacker
            }

            Stop();
            Player.controller.Input.ResetInput();
            //攻击者在视野内，切换为战斗姿态，否则
            if (Player.Find(attacker))
            {
                //如果当前有攻击目标，是否切换目标
                if (Player.LockTarget == null)
                {

                }
            }
        }

        public void FollowTarget(int target)
        {
            MeteorUnit followed = U3D.GetUnit(target);
            if (Main.Ins.MeteorManager.LeavedUnits.ContainsKey(target))
                return;
            Player.FollowTarget = followed;
            ChangeState(FollowState);
            //SubStatus = EAISubStatus.FollowGotoTarget;
        }

        public void SetPatrolPath(List<int> path)
        {
            PatrolState.SetPatrolPath(path);
        }

        public void UpdateInput()
        {
            if (InputKeys[0].state == 0)
            {
                Player.controller.Input.OnKeyDownProxy(InputKeys[0].key, true);
                InputKeys[0].state = 1;
            }
            else if (InputKeys[0].state == 1)
            {
                Player.controller.Input.OnKeyUpProxy(InputKeys[0].key);
                InputKeys[0].state = 2;
            }
            else if (InputKeys[0].state == 2)
            {
                InputKeys.RemoveAt(0);
            }
        }

        public void ReceiveInput(EKeyList key)
        {
            if (HasInput())
            {
                UnityEngine.Debug.Log("error");
                InputKeys.Add(new AIVirtualInput(key));
            }
        }

        public void ReceiveInputs(List<EKeyList> key)
        {
            if (HasInput())
                UnityEngine.Debug.Log("error");
            if (key.Count != 0)
            {
                for (int i = 0; i < key.Count; i++)
                {
                    InputKeys.Add(new AIVirtualInput(key[i]));
                }
            }
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
        protected List<AIVirtualInput> InputQueue = new List<AIVirtualInput>();
        protected NavPathStatus navPathStatus;
        protected int wayIndex;//当前遍历到的路点
        //进入时缓存下角色当前的位置，以后每个Think更新一次位置，未更新则用上次缓存的位置，一直到离角色很近为止.
        protected Vector3 positionStart;
        protected Vector3 positionEnd;
        protected Vector3 TargetPos;
        protected NavType NavType;
        public State(StateMachine machine)
        {
            Machine = machine;
        }

        public virtual void OnEnter(State pevious, object data = null)
        {
            Previous = pevious;
        }

        public virtual void OnPause(State next, object data = null)
        {
            
        }

        public virtual void OnResume(State previous, object data = null)
        {

        }

        public virtual void OnExit(State next)
        {
            Next = next;
        }

        //每一帧执行一次
        public virtual void Update()
        {

        }

        //一段时间执行一次
        public virtual void Think()
        {
            Machine.RefreshTarget();
            Machine.AutoChangeState();
        }

        public virtual void OnChangeWeapon()
        {
            //武器发生变化,
        }

        public void ChangeState(State target, object data = null)
        {
            Machine.ChangeState(target, data);
        }

        public MeteorUnit GetKillTarget()
        {
            return Machine.Player.GetKillTarget();
        }

        public MeteorUnit GetLockTarget()
        {
            return Machine.Player.LockTarget;
        }

        //得到某个角色的面向向量与某个位置的夹角,不考虑Y轴 
        protected float GetAngleBetween(Vector3 vec)
        {
            vec.y = 0;
            //同位置，无法计算夹角.
            if (vec.x == Player.transform.position.x && vec.z == Player.transform.position.z)
                return 0;

            Vector3 vec1 = -Player.transform.forward;
            Vector3 vec2 = (vec - Player.mPos2d).normalized;
            vec2.y = 0;
            float radian = Vector3.Dot(vec1, vec2);
            float degree = Mathf.Acos(Mathf.Clamp(radian, -1.0f, 1.0f)) * Mathf.Rad2Deg;
            return degree;
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
                case NavPathStatus.NavPathNone:
                    navPathStatus = NavPathStatus.NavPathCalc;
                    PathHelper.Ins.CalcPath(Machine, positionStart, positionEnd);
                    break;
                case NavPathStatus.NavPathCalc:
                    //等待寻路线程的处理.
                    if (Machine.Path != null)
                        navPathStatus = NavPathStatus.NavPathComplete;
                    break;
                case NavPathStatus.NavPathComplete:
                    navPathStatus = NavPathStatus.NavPathOrient;
                    if (Machine.Path.ways.Count == 0)
                    {
                        //寻路可直达，
                        wayIndex = 0;
                        TargetPos = positionEnd;
                    }
                    else
                    {
                        wayIndex = 0;
                        TargetPos = Machine.Path.ways[wayIndex].pos;
                    }
                    //Machine.ChangeState(Machine.WaitState);
                    //for (int i = 0; i < Machine.Path.ways.Count; i++)
                    //{
                    //    GameObject obj = new GameObject(string.Format("{0}", i));
                    //    Idevgame.Util.ObjectUtils.Identity(obj);
                    //    obj.transform.position = Machine.Path.ways[i].pos;
                    //}
                    break;
                case NavPathStatus.NavPathInvalid:
                    navPathStatus = NavPathStatus.NavPathIterator;
                    ChangeState(Previous);
                    break;
                case NavPathStatus.NavPathIterator:
                    //调度过程-从当前位置，到目标位置的寻路过程.
                    break;
            }
        }

        protected void NavUpdate()
        {
            if (navPathStatus == NavPathStatus.NavPathOrient){
                //如果方向不对，先切换到转向状态
                if (GetAngleBetween(TargetPos) >= Main.Ins.CombatData.AimDegree)
                {
                    navPathStatus = NavPathStatus.NavPathIterator;
                    Machine.ChangeState(Machine.FaceToState, TargetPos);
                    UnityEngine.Debug.Log("进入转向状态");
                    return;
                }
                else{
                    navPathStatus = NavPathStatus.NavPathIterator;
                }
            }
            else if (navPathStatus == NavPathStatus.NavPathIterator)
            {
                if (wayIndex == Machine.Path.ways.Count - 1){
                    //最后一个路点
                    float distance = NavType == NavType.NavFindUnit ? CombatData.AttackRange : CombatData.StopDistance;
                    Vector3 vector = TargetPos;
                    vector.y = 0;
                    if (Vector3.SqrMagnitude(vector - Player.mPos2d) <= distance){
                        navPathStatus = NavPathStatus.NavPathFinished;
                        UnityEngine.Debug.Log("寻路完毕，进入战斗状态");
                        Player.controller.Input.AIMove(0, 0);
                        return;
                    }
                }else{
                    Vector3 vector = TargetPos;
                    vector.y = 0;
                    //不是最后一个路点
                    if (Vector3.SqrMagnitude(vector - Player.mPos2d) <= CombatData.StopDistance){
                        wayIndex += 1;
                        TargetPos = Machine.Path.ways[wayIndex].pos;
                        return;
                    }
                }
                Player.FaceToTarget(TargetPos);
                Player.controller.Input.AIMove(0, 1);
            }
        }
    }
}
