using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Idevgame.Meteor.AI
{
    public enum ParaIndex
    {
        BaseTime,//动画归一化时间
        AnimationIndex,//动画ID
        ActionIndex,//随机行为ID-按权重随机算，一个Think周期内只进行一次.
        WeaponIndex,//当前武器ID
    }

    public enum ParameterType
    {
        Int,
        Float,
        Bool,
        Enum,
        BoolTrigger,
        IntTrigger,
        EnumTrigger,
        EnumBitMask,
    }

    //组行为-差异化各个角色的行为，否则各个AI行为一致，感觉机械.
    //只是控制各个StateMachine的某个行为是否允许随机到.
    //战斗圈-
    //包围圈-不允许进入战斗，在角色附近移动
    public class StateMachineGroup
    {

    }

    public class StateMachine
    {
        //当Think为100时,0.1S一个行为检测,行为频率慢,则连招可能连不起来.行为频率快, 则每个招式在可切换招式的时机, 进行连招的几率越大.
        static readonly float ThinkRound = 1000;
        float ThickTick = 0.0f;
        //加载一份AI数据，初始化各个AI状态，包括初始状态序号，各个状态有多少个转移.
        //每个状态的转移条件，条件里的参数还未和游戏中的参数绑定.
        public List<State> States;
        public MeteorUnit Player;

        public State CurrentState;
        State NextState;

        public WaitState WaitState;
        public IdleState IdleState;
        public GuardState GuardState;
        public LookState LookState;//四周观察-未发现敌人时.
        public DangerState DangerState;//处于危险中，逃跑.如果仍被敌人追上，有可能触发决斗-如果脱离了战斗视野，可能继续逃跑
        public KillState KillState;//强制追杀 无视视野.
        public PatrolState PatrolState;//巡逻.
        public FollowState FollowState;
        public FightWithBladeState FightWithBladeState;

        public float BaseTime;//角色当前动作的归一化时间 大于0部分是循环次数，小于0部分是单次播放百分比.
        public int AnimationIndex;//角色当前动画编号
        public int ActionIndex;//角色当前的行为编号-轻招式 重招式 绝招
        public int WeaponIndex;//角色当前武器ID
        //战斗中
        //目标置入初始状态.
        public void Init(MeteorUnit Unit)
        {
            Player = Unit;
            States = new List<State>();
            IdleState = new IdleState("IdleState", this);
            States.Add(IdleState);
            GuardState = new GuardState("GuardState", this);
            States.Add(GuardState);
            KillState = new KillState("KillState", this);
            States.Add(KillState);
            for (int i = 0; i < States.Count; i++)
                States[i].Init();
            CurrentState = WaitState;
            int dis = Player.Attr.View / 2;
            AttackRangeMin = dis * dis;
            AttackRangeMax = (dis + dis / 2) * (dis + dis / 2);
            stoped = true;
        }

        public bool IsFighting()
        {
            if (CurrentState != null)
            {
                FightState fs = CurrentState as FightState;
                return fs != null;
            }
            return false;
        }


        //根据武器系统，各类武器在战斗中使用的人工智能分别处理.
        //但是核心是战斗部分.
        public void OnWeaponChange(int weapon)
        {
            WeaponIndex = weapon;
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
            //Debug.Log(string.Format("unit:{0} pause:{1}", owner.name, pause_tick));
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
            if (ThickTick > 0)
                return;
            ThickTick = ThinkRound;
            NextState = CurrentState.Update();
            if (NextState != null)
            {
                CurrentState.Exit();
                NextState.Enter();
                CurrentState = NextState;
                NextState = null;
            }
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
            Actions.Add(new ActionWeight(ActionType.Burst, Player.Attr.Burst));//爆气
            Actions.Add(new ActionWeight(ActionType.Aim, Player.Attr.Aim));//瞄准
            Actions.Add(new ActionWeight(ActionType.GetItem, Player.Attr.GetItem));//视野范围内有物品-去拾取的几率
        }

        //设置某个行为在当前状态是无法使用的.
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
            ActionIndex = (int)Actions[idx].action;
        }

        //硬切
        public void ChangeState(State Target, Object data = null)
        {
            if (CurrentState != Target)
            {
                CurrentState.Exit();
                Target.Enter(data);
                CurrentState = Target;
            }
        }

        float AttackRangeMin;//视野范围最小值的平方-近战武器，在此范围内可发现 View的一半的平方  View为400时，此值为200^2
        float AttackRangeMax;//视野范围最大值的平方-近战武器，超过此范围可丢失 View的一半的平方+一半 View为400时，此值为300^2
        void RefreshTarget()
        {
            //MeteorUnit temp = lockTarget;
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
                        if (d >= AttackRangeMax)//超过距离以免不停的切换目标
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
                    if (d > AttackRangeMax)
                        Player.TargetItem = null;
                }
            }
        }

        //选择一个敌方目标
        public void SelectEnemy()
        {
            Player.LockTarget = null;
            float dis = (Player.Attr.View / 2) * (Player.Attr.View / 2);//视野，可能指的是直径，这里变为半径,平方比开方快.
            if (U3D.IsSpecialWeapon(Player.Attr.Weapon))
                dis = Player.Attr.View * Player.Attr.View;//远程武器，视野距离翻倍.
            int index = -1;
            MeteorUnit tar = null;
            Collider[] other = Physics.OverlapSphere(Player.transform.position, dis, 1 << LayerMask.NameToLayer("Monster") | 1 << LayerMask.NameToLayer("LocalPlayer"));

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
            float dis = AttackRangeMin;
            int index = -1;
            SceneItemAgent tar = null;
            //直接遍历算了
            for (int i = 0; i < Main.Instance.MeteorManager.SceneItems.Count; i++)
            {
                SceneItemAgent item = Main.Instance.MeteorManager.SceneItems[i].gameObject.GetComponent<SceneItemAgent>();
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
            if (index >= 0 && index < Main.Instance.MeteorManager.SceneItems.Count && tar != null)
                Player.TargetItem = tar;
        }

        public int FindStateIndex(string name)
        {
            for (int i = 0; i < States.Count; i++)
            {
                if (States[i].Name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Stop()
        {
            Player.controller.Input.AIMove(0, 0);
        }

        public void OnUnitDead(MeteorUnit dead)
        {

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
        public List<WayPoint> Path = new List<WayPoint>();//存储寻路找到的路线点
        public void OnDamaged(MeteorUnit attacker)
        {
            //寻路数据要清空.
            Path.Clear();
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
            if (Main.Instance.MeteorManager.LeavedUnits.ContainsKey(target))
                return;
            Player.FollowTarget = followed;
            ChangeState(FollowState);
            //SubStatus = EAISubStatus.FollowGotoTarget;
        }

        public void SetPatrolPath(List<int> path)
        {
            //patrolData.Clear();
            //for (int i = 0; i < path.Count; i++)
            //    patrolData.Add(path[i]);
            //PatrolPath.Clear();
            //FindPatrolFinished = false;
            //curPatrolIndex = -1;
            //targetPatrolIndex = -1;
            //curIndex = -1;
            //targetIndex = -1;
        }

        //转换到近身攻击状态，按照角色当前的武装，决定切换目标状态.
        public void ChangeFightState()
        {

        }
    }

    public abstract class State
    {
        public string Name;
        public StateMachine Machine;
        public int Index;//在状态机内的序号.
        public State(string sn, StateMachine machine)
        {
            Name = sn;
            Machine = machine;
        }

        public abstract void Init();
        public abstract void Enter(Object data = null);
        public abstract void Exit();

        //默认状态-待机，调用待机动作.
        //动作结束后
        public virtual State Update()
        {
            return null;
        }


        public virtual void OnChangeWeapon()
        {
            //武器发生变化,
        }
    }
}
