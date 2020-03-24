using Assets.Code.Idevgame.Common.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Idevgame.Meteor.AI
{
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
        Timer<int> InputTimer;//输入计时器，在活动状态下表明，还在输入指令中.
        List<VirtualInput> InputKeys = new List<VirtualInput>();
        public MeteorUnit Player;
        public State PreviousState;
        public State CurrentState;
        public State NextState;
        public State IdleState;
        public State ReviveState;//队长复活队友
        public GuardState GuardState;
        public LookState LookState;//四周观察-未发现敌人时.
        public DangerState DangerState;//处于危险中，逃跑.如果仍被敌人追上，有可能触发决斗-如果脱离了战斗视野，可能继续逃跑
        public KillState KillState;//强制追杀 无视视野.
        public PatrolState PatrolState;//巡逻.
        public FollowState FollowState;
        public FightState FightState;

        public float BaseTime;//角色当前动作的归一化时间 大于0部分是循环次数，小于0部分是单次播放百分比.
        public int AnimationIndex;//角色当前动画编号
        public int ActionIndex;//角色当前的行为编号-轻招式 重招式 绝招
        public int WeaponIndex;//角色当前武器ID
        //战斗中
        //目标置入初始状态.
        public void Init(MeteorUnit Unit)
        {
            Player = Unit;
            IdleState = new IdleState(this);
            GuardState = new GuardState(this);
            KillState = new KillState(this);
            PatrolState = new PatrolState(this);
            ReviveState = new ReviveState(this);
            FightState = new FightState(this);
            int dis = Player.Attr.View;
            AttackRangeMin = dis * dis;
            AttackRangeMax = (dis + dis / 2) * (dis + dis / 2);
            this.EnterDefaultState();
            stoped = true;
        }

        bool HasInput()
        {
            return InputTimer != null;
        }

        //输入中.
        void OnInput(int timer)
        {
            Player.controller.Input.OnKeyDownProxy(InputKeys[0].key, true);
            Player.controller.Input.OnKeyUpProxy(InputKeys[0].key);
            InputKeys.RemoveAt(0);
            if (InputKeys.Count == 0)
            {
                InputTimer.remove(OnInput);
                InputTimer = null;
            }
        }

        void EnterDefaultState()
        {
            CurrentState = IdleState;
            CurrentState.OnEnter();
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
        }

        public void Update()
        {
            if (InputTimer != null)
                InputTimer.Update();

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
                return;

            ThickTick -= Player.Attr.Think;
            if (ThickTick > 0)
                return;
            ThickTick = ThinkRound;

            //更新当前状态，内部自带状态切换.
            CurrentState.Update();//每当暗杀模式时，若自己是队长，寻找到死亡同伴，有一定概率去复活同伴
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
                NextState = Target;
                CurrentState.OnExit();
                PreviousState = CurrentState;
                Target.OnEnter(data);
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
            float dis = AttackRangeMin;//视野，可能指的是直径，这里变为半径,平方比开方快.
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
            PatrolState.SetPatrolPath(path);
        }
    }

    public abstract class State
    {
        public StateMachine Machine;
        public State(StateMachine machine)
        {
            Machine = machine;
        }

        public abstract void OnEnter(Object data = null);
        public abstract void OnExit();

        //默认状态-待机，调用待机动作.
        //动作结束后
        public virtual void Update()
        {

        }


        public virtual void OnChangeWeapon()
        {
            //武器发生变化,
        }

        public void ChangeState(State target)
        {
            Machine.ChangeState(target);
        }

        public MeteorUnit GetKillTarget()
        {
            return Machine.Player.GetKillTarget();
        }

        public MeteorUnit GetLockTarget()
        {
            return Machine.Player.LockTarget;
        }
    }
}
