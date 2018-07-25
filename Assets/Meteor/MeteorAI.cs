using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Idevgame.Util;

//大状态
public enum EAIStatus
{
    Idle,//不动.
    Fight,//包括所有战斗情况
    Kill,//强制杀死指定敌人，无视距离，一直跟随
    Guard,//防御
    Follow,//跟随
    Aim,//远程武器瞄准
    Think,//没发觉目标时左右观察
    GotoPatrol,//去巡逻路径的第一个位置.
    Patrol,//巡逻。
    PatrolInPlace,//单点巡逻
    Wait,//，类似于Search
    Dodge,//逃跑
    Look,//四处看
    GetItem,//取得场景道具-最近的，可拾取的(未打碎的箱子不算在内)。
    AttackTarget,//攻击指定位置.
}

/*
 * Think = 50;
Attack1 = 55;
Attack2 = 40;
Attack3 = 0;
Guard = 5;
Dodge = 5;
Jump = 5;
Look = 60;
Burst = 1;
Aim = 50;
GetItem = 0;
 */

public enum EAISubStatus
{
    FightInDanger,//血量小于20
    FightGotoTarget,//在打斗前，必须走到目标范围附近
    FightSubRotateToTarget,//走到路点后，旋转朝向下一个路点.
    FightSubLeavetoTarget,//离开、逃跑.
    FightThink,//远程武器射击，
    FightAttack1,//普攻
    FightAttack2,//连击
    FightAttack3,//绝招
    FightGuard,//战斗中防御-按概率
    FightDodge,//躲闪，双击某个方向键
    FightJump,//跳跃招式-概率
    FightLook,//战斗中旋转-
    FightBurst,//战斗中曝气
    FightAim,//战斗中若使用远程武器，瞄准敌方的几率，否则就随机
    //FightGetItem,//战斗中去捡起物品-镖物-补给-武器-BUFF道具
    SubStatusIdle,
    SubStatusWait,
    FollowGotoTarget,
    FollowSubRotateToTarget,
    Patrol,
    PatrolSubInPlace,
    PatrolSubRotateInPlace,//原地随机旋转.
    PatrolSubRotateToTarget,//原地一定时间内旋转到指定方向
    PatrolSubGotoTarget,//跑向指定位置
    KillGetTarget,//离角色很近了，可以攻击
    KillGotoTarget,//离角色一定距离，需要先跑过去
    KillOnHurt,//被敌人击中
    AttackGotoTarget,//攻击指定位置-寻路中.
    AttackTarget,//攻击指定位置-攻击中
    AttackTargetSubRotateToTarget,//攻击指定位置，到达中间某个寻路点时.
    GetItemGotoItem,//朝物件走的路线中
    GetItemSubRotateToItem,//到达中间某个寻路点，朝向下个寻路点
    GetItemMissItem,//可能目标已无效（被其他NPC拾取、目标发生转换（镖物某NPC死后，落到地面-数秒内归位））
}

//每种距离，有无目标的2种情况下的AI设置.
public class AISet
{
    public List<AISlot> Target;
    public List<AISlot> NoneTarget;
}

public class AISlot
{
    public int Distance;
    public int Ratio;//
    public virtual int Pose() { return 0; }//动作->武器，或者技能ID
    public virtual bool CheckCondition() { return false; }
}

//各种AI下保存各种状态检查函数
public class WeaponAI : AISlot
{
    public override int Pose()
    {
        return 0;
    }
    public override bool CheckCondition()
    {
        return base.CheckCondition();
    }
}

public class MeteorAI {
    public MeteorAI(MeteorUnit user)
    {
        owner = user;
        Status = EAIStatus.Idle;
        SubStatus = EAISubStatus.SubStatusIdle;
    }
    public EAIStatus Status { get; set; }
    public EAISubStatus SubStatus { get; set; }
    MeteorUnit owner;//自己
    MeteorUnit followTarget;//跟随目标
    public MeteorUnit killTarget;//无视视野
    MeteorUnit fightTarget;//视野内的，选择目标
    MeteorUnit lastAttacker;//时间最近攻击自己的人
    public Vector3 AttackTarget;//要攻击的位置.
    public int AttackCount;//要攻击的次数.
    public bool stoped { get; set; }
    bool paused = false;
    float pause_tick;
    public Dictionary<int, AISet> AIData;
    //当前目标路径
    int pathIdx = -1;
    Vector3 targetPos = Vector3.zero;
    List<WayPoint> targetPath = new List<WayPoint>();//固定点位置.当只有单点的时候，表示可以直接走过去.也就是
    // Update is called once per frame
    public void Update () {
        //是否暂停AI。
        if (stoped)
            return;

        if (owner.Dead)
            return;

        //这个暂停是部分行为需要停止AI一段指定时间间隔
        if (paused)
        {
            pause_tick -= Time.deltaTime;
            if (pause_tick <= 0.0f)
                paused = false;
            return;
        }

        //如果玩家正在进行一些动作，或者预定义行为，则不运行AI。
        if (GameBattleEx.Instance != null)
        {
            if (GameBattleEx.Instance.IsPerforming(owner.InstanceId))
            {
                Debug.Log(string.Format("unit:{0} IsPerforming", owner.name));
                return;
            }
        }

        AIJumpDelay += Time.deltaTime;
        AIFollowRefresh += Time.deltaTime;

        if (Status == EAIStatus.Patrol || Status == EAIStatus.PatrolInPlace || Status == EAIStatus.GotoPatrol)
        {
            if (owner.GetLockedTarget() != null)
                ChangeState(EAIStatus.Wait);
        }

        if (Status == EAIStatus.Fight)
        {
            //目标可能离开范围.
            if (owner.GetLockedTarget() == null)
            {
                if (lastAttacker != null)
                {
                    fightTarget = lastAttacker;
                }
                else if (followTarget != null)
                {
                    ChangeState(EAIStatus.Follow);
                }
                else
                {
                    fightTarget = null;
                    ChangeState(EAIStatus.Wait);
                }
            }
        }

        if (Status == EAIStatus.Wait)
        {
            //一些事情优先级比较高，先处理
            //视线内存在可碰触的场景道具
            if (owner.GetSceneItemTarget() != null)
            {
                int getItem = Random.Range(0, 100);
                if (getItem < owner.Attr.GetItem)
                {
                    ChangeState(EAIStatus.GetItem);
                    return;
                }
            }

            if (owner.GetLockedTarget() != null)
            {
                //killTarget = owner.GetLockedTarget();
                fightTarget = owner.GetLockedTarget();
                Status = EAIStatus.Fight;
                SubStatus = EAISubStatus.FightThink;
            }
        }

        if (owner.IsOnGround() && owner.OnGroundTick >= 5)
        {
            switch (Status)
            {
                //case EAIStatus.AttackTarget:
                //    OnAttackTarget();
                //    break;
                case EAIStatus.Fight:
                    FightOnGround();
                    break;
                case EAIStatus.Idle:
                    OnIdle();
                    break;
                case EAIStatus.Wait:
                    OnWait();
                    break;
                case EAIStatus.Guard:
                    OnGuard();
                    break;
                case EAIStatus.Look:
                    OnLook();
                    break;
                case EAIStatus.GetItem:
                    OnGetItem();
                    break;
                case EAIStatus.Dodge:
                    OnDodge();
                    break;
                case EAIStatus.PatrolInPlace:
                    OnPatrolInPlace();
                    break;
                case EAIStatus.GotoPatrol:
                    //Debug.LogError("gotopatrol");
                    OnGotoPatrol();
                    break;
                case EAIStatus.Patrol:
                    //Debug.LogError("patrol");
                    OnPatrol();
                    break;
                case EAIStatus.Follow:
                    MovetoTarget(followTarget);
                    break;
                case EAIStatus.Kill:
                    switch (SubStatus)
                    {
                        case EAISubStatus.KillGotoTarget:
                            if (killTarget == null)
                                killTarget = owner.GetLockedTarget();
                            if (killTarget != null)
                                MovetoTarget(killTarget);
                            else
                            {
                                Status = EAIStatus.Idle;
                                //SubStatus = EAISubStatus.Think;
                            }
                            break;
                        case EAISubStatus.KillGetTarget:
                            if (killTarget == null || killTarget.Dead)
                                killTarget = owner.GetLockedTarget();
                            KillTarget(killTarget);
                            break;
                        case EAISubStatus.KillOnHurt:
                            OnHurt();
                            break;
                    }
                    break;
            }
        }
        else
        {
            switch (Status)
            {
                //在空中，有目标时，可能会使用空中技能，例如飞镖，飞轮空中绝招，以及部分武器在空中的招式
                //也有可能在受击中，使用曝气，或无法处理AI的状态
                case EAIStatus.Fight:
                    FightOnAir();
                    break;
                case EAIStatus.Kill:;
                    break;
               
            }
        }
    }

    //逃跑
    void OnDodge()
    {
        ChangeState(EAIStatus.Wait);
    }

    //捡取场景道具
    void OnGetItem()
    {
        switch (SubStatus)
        {
            case EAISubStatus.GetItemMissItem:
                Assert.IsTrue(owner.GetSceneItemTarget() != null);
                RefreshPath(owner.mPos, owner.GetSceneItemTarget().transform.position);
                SubStatus = EAISubStatus.GetItemGotoItem;
                break;
            case EAISubStatus.GetItemGotoItem:
                if (Path.Count == 0 || AIFollowRefresh >= 3.0f)
                {
                    if (owner.GetSceneItemTarget() != null)
                        RefreshPath(owner.mPos, owner.GetSceneItemTarget().transform.position);
                    AIFollowRefresh = 0.0f;
                }

                if (curIndex == -1)
                    targetIndex = 0;

                if (targetIndex >= Path.Count)
                {
                    UnityEngine.Debug.LogError(string.Format("targetIndex:{0}, FollowPath:{1}", targetIndex, Path.Count));
                    Debug.DebugBreak();
                }

                Assert.IsTrue(targetIndex < Path.Count);
                float dis = Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(Path[targetIndex].pos.x, 0, Path[targetIndex].pos.z));
                if (dis <= owner.Speed * Time.deltaTime * 0.13f)
                {
                    owner.controller.Input.AIMove(0, 0);
                    if (targetIndex == Path.Count - 1)
                    {
                        //到达终点.
                        owner.controller.Input.AIMove(0, 0);
                        ChangeState(EAIStatus.Wait);
                        return;
                    }
                    else
                    {
                        curIndex = targetIndex;
                        targetIndex += 1;
                    }
                    RotateRound = Random.Range(1, 3);
                    SubStatus = EAISubStatus.GetItemSubRotateToItem;//到指定地点后旋转到目标.
                    return;
                }

                owner.FaceToTarget(new Vector3(Path[targetIndex].pos.x, 0, Path[targetIndex].pos.z));
                owner.controller.Input.AIMove(0, 1);
                //模拟跳跃键，移动到下一个位置.还得按住上
                if (curIndex != -1)
                {
                    if (PathMng.Instance.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump && owner.IsOnGround() && AIJumpDelay > 2.5f)
                    {
                        AIJump();
                        AIJumpDelay = 0.0f;
                    }
                };
                break;
            case EAISubStatus.GetItemSubRotateToItem:
                if (GetItemRotateToTargetCoroutine == null)
                    GetItemRotateToTargetCoroutine = owner.StartCoroutine(GetItemRotateToTarget(Path[targetIndex].pos));
                break;
        }
    }

    void OnGuard()
    {
        if (waitDefence <= 0.0f)
        {
            int random = Random.Range(0, 100);
            if (random < owner.Attr.Guard)
                owner.Guard(false);
        }
        waitDefence -= Time.deltaTime;
        //owner.controller.Input.OnKeyDown(EKeyList.KL_Defence, true);//防御
    }

    //四处看未实现.
    void OnLook()
    {
        Stop();//停止移动.
        ChangeState(EAIStatus.Wait);
    }

    #region 输入
    Coroutine InputCorout;//普通的单个键的
    #endregion
    //空中.
    void FightOnAir()
    {
        //1 先检查当前是否处于接收输入状态，这种都是连招方式的。
        if (owner.controller.Input.AcceptInput())
        {
            //依次判断
            //状态 - 残血[捡物品/逃跑(旋转、跑)]、攻击出招、闪躲、跳跃、防御、捡物品、
            //武器类型 - 近战武器/远程武器
            //距离 - 近战武器距离是否够近 30-50码以内，不一定必须
            //面向 - 是否面对角色不至于

            //能输入招式之类的。
            //1 随机决定是否 攻击（平A）/连招（左右A，上下A之类，多个方向键+攻击）/绝招
            //根据当前招式可以接哪些招式
        }
        else
        {
            //无输入状态时.
            if (InputCorout == null)
            {
                //不响应输入.指一个动作正在前摇或后摇，只能响应曝气,曝气只有
                if (owner.posMng.IsHurtPose())
                {
                    //如果可以曝气
                    if (owner.CanBurst())
                    {
                        int burst = Random.Range(0, 100);
                        //20时，就为20几率，0-19 共20
                        if (burst >= Mathf.Max(0, 100 - owner.Attr.Burst))
                            InputCorout = owner.StartCoroutine(VirtualKeyEvent(EKeyList.KL_BreakOut));
                    }
                }
            }
        }
    }

    //危机中，尽量离开战斗目标.
    float leaveTick;
    Coroutine leaveRotateCorout;
    float leaveAngle;

    void LeaveReset()
    {
        //重新朝一个面向，跑1-2秒
        leaveTick = 0.0f;
        leaveAngle = Random.Range(15.0f, 60.0f);
        Stop();
    }

    void FightInDanger()
    {
        if (leaveTick >= 2.0f)
        {
            if (leaveRotateCorout != null)
            {
                owner.StopCoroutine(leaveRotateCorout);
                leaveRotateCorout = null;
            }
            LeaveReset();
        }
        else
        {
            //没有旋转，先旋转到这个角度。
            if (leaveRotateCorout == null)
                leaveRotateCorout = owner.StartCoroutine(LeaveRotateAngle(leaveAngle));
        }

        leaveTick += Time.deltaTime;
    }

    IEnumerator LeaveRotateAngle(float leaveAngle, float angularspeed = 150.0f)
    {
        bool rightRotate = Random.Range(-1, 2) >= 0;
        float offset = 0.0f;
        float offsetmax = leaveAngle;
        float timeTotal = offsetmax / angularspeed;
        float timeTick = 0.0f;
        while (true)
        {
            timeTick += Time.deltaTime;
            float yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            owner.SetOrientation(yOffset - offset);
            offset = yOffset;
            if (timeTick > timeTotal)
            {
                if (owner.posMng.mActiveAction.Idx == CommonAction.WalkRight || owner.posMng.mActiveAction.Idx == CommonAction.WalkLeft)
                    owner.posMng.ChangeAction(0, 0.1f);
                break;
            }
            yield return 0;
        }

        while (true)
        {
            Move();
            yield return 0;
        }
        leaveRotateCorout = null;
    }

    public void Move()
    {
        owner.controller.Input.AIMove(0, 1);
    }

    public void Stop()
    {
        owner.controller.Input.AIMove(0, 0);
    }

    void TryAttack()
    {
        Debug.LogError("try attack");
        //已经在攻击招式中.后接招式挑选
        //把可以使用的招式放到集合里，A类放普攻，B类放搓招， C类放绝招
        ActionNode act = ActionInterrupt.Instance.GetActions(owner.posMng.mActiveAction.Idx);
        if (act != null)
        {
            int attack = Random.Range(0, 100);
            //owner.Attr.Attack1 = 0;
            //owner.Attr.Attack2 = 0;
            //owner.Attr.Attack3 = 100;
            //owner.AngryValue = 100;
            if (attack < owner.Attr.Attack1)
            {
                //普通攻击
                Debug.LogError("try attack 1");
                ActionNode attack1 = ActionInterrupt.Instance.GetNormalNode(owner, act);
                if (attack1 != null)
                {
                    SubStatus = EAISubStatus.FightAttack1;
                    TryPlayWeaponPose(attack1.KeyMap);
                }
            }
            else if (attack < (owner.Attr.Attack1 + owner.Attr.Attack2))
            {
                Debug.LogError("try attack2");
                //连招
                List<ActionNode> attack2 = ActionInterrupt.Instance.GetSlashNode(owner, act);
                if (attack2.Count != 0)
                {
                    SubStatus = EAISubStatus.FightAttack2;
                    TryPlayWeaponPose(attack2[Random.Range(0, attack2.Count)].KeyMap);
                }
            }
            else if (attack < (owner.Attr.Attack1 + owner.Attr.Attack2 + owner.Attr.Attack3))
            {
                //绝招
                Debug.LogError("try attack3");
                ActionNode attack3 = ActionInterrupt.Instance.GetSkillNode(owner, act);
                if (attack3 != null)
                {
                    SubStatus = EAISubStatus.FightAttack3;
                    if (attack3.ActionIdx == 310)
                        Debug.Log("use blade skill");
                    TryPlayWeaponPose(attack3.KeyMap);
                }
            }
        }
    }

    //攻击目标位置
    public void OnAttackTarget()
    {
        switch (SubStatus)
        {
            case EAISubStatus.AttackGotoTarget:
                if (curIndex == -1)
                    targetIndex = 0;

                if (targetIndex >= Path.Count)
                {
                    UnityEngine.Debug.LogError(string.Format("targetIndex:{0}, FollowPath:{1}", targetIndex, Path.Count));
                    Debug.DebugBreak();
                }

                Assert.IsTrue(targetIndex < Path.Count);
                float dis = Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(Path[targetIndex].pos.x, 0, Path[targetIndex].pos.z));
                if (dis <= owner.Speed * Time.deltaTime * 0.13f)
                {
                    owner.controller.Input.AIMove(0, 0);
                    if (targetIndex == Path.Count - 1)
                    {
                        //到达终点.
                        owner.controller.Input.AIMove(0, 0);
                        SubStatus = EAISubStatus.AttackTarget;
                        return;
                    }
                    else
                    {
                        curIndex = targetIndex;
                        targetIndex += 1;
                    }
                    RotateRound = Random.Range(1, 3);
                    SubStatus = EAISubStatus.AttackTargetSubRotateToTarget;//到指定地点后旋转到目标.
                    return;
                }

                owner.FaceToTarget(new Vector3(Path[targetIndex].pos.x, 0, Path[targetIndex].pos.z));
                owner.controller.Input.AIMove(0, 1);
                //模拟跳跃键，移动到下一个位置.还得按住上
                if (curIndex != -1)
                {
                    if (PathMng.Instance.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump && owner.IsOnGround() && AIJumpDelay > 2.5f)
                    {
                        AIJump();
                        AIJumpDelay = 0.0f;
                    }
                };
                break;
            case EAISubStatus.AttackTarget:
                if (AttackCount != 0 && AttackTargetCoroutine == null)
                {
                    owner.FaceToTarget(AttackTarget);
                    AttackTargetCoroutine = owner.StartCoroutine(Attack());
                    AttackCount -= 1;
                    if (AttackCount == 0)
                        ChangeState(EAIStatus.Wait);
                }
                else
                {
                    Debug.Log("wait change status wait");
                }
                break;
            case EAISubStatus.AttackTargetSubRotateToTarget:
                if (AttackRotateToTargetCoroutine == null)
                    AttackRotateToTargetCoroutine = owner.StartCoroutine(AttackRotateToTarget(Path[targetIndex].pos));
                break;
        }
    }

    //地面攻击.
    void FightOnGround()
    {
        Debug.LogError("fightonground");
        //1 先检查当前是否处于接收输入状态，这种都是连招方式的。
        if (owner.controller.Input.AcceptInput())
        {
            //依次判断
            Debug.LogError("accept input");
            if (owner.posMng.IsAttackPose() && PlayWeaponPoseCorout == null)
            {
                Debug.LogError("attack");
                TryAttack();
            }
            else if (owner.posMng.IsHurtPose())
            {
                //无输入状态时.
                if (InputCorout == null)
                {
                    //不响应输入.指一个动作正在前摇或后摇，只能响应曝气,曝气只有
                    if (owner.posMng.IsHurtPose())
                    {
                        //如果可以曝气
                        if (owner.CanBurst())
                        {
                            int burst = Random.Range(0, 100);
                            //20时，就为20几率，0-19 共20
                            if (burst >= Mathf.Max(0, 100 - owner.Attr.Burst))
                                InputCorout = owner.StartCoroutine(VirtualKeyEvent(EKeyList.KL_BreakOut));
                        }
                    }
                }
            }
            else
            {
                //在一些动作姿势里，走，跑，跳 等
                //if ()//在一些特殊姿势里，比如落地，等待飞轮回来，火枪上子弹。
                //在危险中
                if (owner.InDanger())
                {
                    if (SubStatus != EAISubStatus.FightInDanger)
                    {
                        LeaveReset();
                        SubStatus = EAISubStatus.FightInDanger;
                    }

                    switch (SubStatus)
                    {
                        case EAISubStatus.FightInDanger:
                            //随机朝一个面向。跑大概1-2秒，重复
                            FightInDanger();
                            break;
                    }
                }
                else
                {
                    float dis = Vector3.Distance(owner.mPos, fightTarget.mPos);
                    //距离战斗目标不同，选择不同方式应对.
                    if (dis >= Global.AttackRange)
                    {
                        //大部分几率是跑过去，走到跟前对打。
                        //小部分几率是切换远程武器打目标。

                        //1是否拥有远程武器
                        if (U3D.IsSpecialWeapon(owner.Attr.Weapon))
                        {
                            //主手是远程武器
                        }
                        else if (U3D.IsSpecialWeapon(owner.Attr.Weapon2))
                        {
                            //副手是远程武器
                        }
                        else
                        {
                            //双手全近战武器.

                        }
                    
                        //切换武器的几率多大
                        //
                        //A:切换远程武器战斗
                        //A.1:监测能否切换远程武器.
                        //B:跑到近距离战斗
                    }
                    else
                    {
                        //小部分几率跑到远处使用远程武器，
                        //大部分几率，走到角色跟前打
                        //A:跑到远处切换远程武器战斗
                        //B:开始随机动作 A 躲避 B 跳跃 C 防御 D 捡去物品 E 普攻 F 搓招 G 绝招 
                    }

                    //检查状态是否应该切换为GotoTarget;
                    int attack = Random.Range(0, 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look + owner.Attr.Burst + owner.Attr.GetItem);
                    if (attack <= owner.Attr.Attack1 + owner.Attr.Attack2 + owner.Attr.Attack3)
                        TryAttack();
                    else
                    {
                        if (attack > 100 && attack < 100 + owner.Attr.Dodge)
                        {
                            //逃跑
                            ChangeState(EAIStatus.Dodge);
                        }
                        else
                        if (JumpCoroutine == null && attack > 100 + owner.Attr.Dodge && attack < 100 + owner.Attr.Dodge + owner.Attr.Jump)
                        {
                            AIJump();
                        }
                        else if (attack > 100 + owner.Attr.Dodge + owner.Attr.Jump && attack < 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard)
                        {
                            Stop();
                            owner.Guard(true);
                        }
                        else if (attack > 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard  && attack <  100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look)
                        {
                            ChangeState(EAIStatus.Look);
                        }
                        else if (attack > 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look && attack < 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look + owner.Attr.Burst)
                        {
                            //快速移动.
                            AIBurst((EKeyList.KL_KeyA) + attack % 4);
                        }//不判断拾取物品，拾取物品发生在物品出现在视野内时
                        //else if (attack > 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look + owner.Attr.Burst && attack < 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look + owner.Attr.Burst + owner.Attr.GetItem)
                        //{
                        //    ChangeState(EAIStatus.GetItem);
                        //}
                        else
                        {
                            //
                        }
                    }
                    return;
                    //根据当前动作，选择对应操作。
                    //主要逻辑
                    
                }
            }
            //状态 - 残血[捡物品/逃跑(旋转、跑)]、攻击出招、闪躲、跳跃、防御、捡物品、
            //武器类型 - 近战武器/远程武器
            //距离 - 近战武器距离是否够近
            //面向 - 是否面对角色不至于

            //能输入招式之类的。
            //1 随机决定是否 攻击（平A）/连招（左右A，上下A之类，多个方向键+攻击）/绝招
            //根据当前招式可以接哪些招式
        }
        else
        {
            //不接受输入，在动作前摇或后摇
        }
    }

    List<WayPoint> Path = new List<WayPoint>();//这个是攻击指定位置时的寻路，这个是不会随着角色移动改变位置的.
    void RefreshPath(Vector3 now, Vector3 target)
    {
        Path = PathMng.Instance.FindPath(now, target);
        if (Path.Count == 0)
        {
            Debug.DebugBreak();
        }
        curIndex = -1;
        targetIndex = -1;
    }

    //在其他角色上使用的插槽位置，
    Dictionary<MeteorUnit, int> SlotCache = new Dictionary<MeteorUnit, int>();
    Vector3 vecTarget;
    //GameObject[] Pos = new GameObject[100];
    //GameObject[] debugPos = new GameObject[100];
    int curIndex = 0;
    int targetIndex = 0;
    List<WayPoint> FollowPath = new List<WayPoint>();
    //要做一个移动缓存，不必要每次都用最新的位置去寻路.\
    void RefreshFollowPath(MeteorUnit user, MeteorUnit target)
    {
        FollowPath = PathMng.Instance.FindPath(owner.mPos, owner, target);
        curIndex = -1;
        targetIndex = -1;
        AIFollowRefresh = 0.0f;
    }

    void MovetoTarget(MeteorUnit target)
    {
        float dis = 0.0f;
        switch (SubStatus)
        {
            case EAISubStatus.FollowGotoTarget:
            case EAISubStatus.KillGotoTarget:
                if (AIFollowRefresh >= 10.0f)
                {
                    //先确定是否重新刷新路线.
                    dis = Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(target.mPos.x, 0, target.mPos.z));
                    if (dis < Global.FollowDistance)//小于50码停止跟随，不需要计算路径
                    {
                        //FollowPath.Clear();
                        Debug.Log("stop follow until 35 meters");
                        owner.controller.Input.AIMove(0, 0);
                        AIFollowRefresh = 5.0f;//5秒后再计算是否跟随.
                        ChangeState(EAIStatus.Wait);//开始寻找敌人
                        return;
                    }
                    RefreshFollowPath(owner, target);
                }
                else
                {
                    if (targetIndex >= FollowPath.Count)
                        RefreshFollowPath(owner, target);

                    if (curIndex == -1)
                        targetIndex = 0;

                    if (targetIndex >= FollowPath.Count)
                    {
                        UnityEngine.Debug.LogError(string.Format("targetIndex:{0}, FollowPath:{1}", targetIndex, FollowPath.Count));
                        Debug.DebugBreak();
                    }
                    Assert.IsTrue(targetIndex < FollowPath.Count);
                    dis = Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(FollowPath[targetIndex].pos.x, 0, FollowPath[targetIndex].pos.z));
                    if (dis <= owner.Speed * Time.deltaTime * 0.13f)
                    {
                        owner.controller.Input.AIMove(0, 0);
                        if (targetIndex == FollowPath.Count - 1)
                        {
                            //到达终点.
                            owner.controller.Input.AIMove(0, 0);
                            AIFollowRefresh = 5.0f;
                            ChangeState(EAIStatus.Wait);//开始寻找敌人
                            return;
                        }
                        else
                        {
                            curIndex = targetIndex;
                            targetIndex += 1;
                        }
                        RotateRound = Random.Range(1, 3);
                        SubStatus = EAISubStatus.FollowSubRotateToTarget;//到指定地点后旋转到目标.
                        return;
                    }

                    owner.FaceToTarget(new Vector3(FollowPath[targetIndex].pos.x, 0, FollowPath[targetIndex].pos.z));
                    owner.controller.Input.AIMove(0, 1);
                    //模拟跳跃键，移动到下一个位置.还得按住上
                    if (curIndex != -1)
                    {
                        if (PathMng.Instance.GetWalkMethod(FollowPath[curIndex].index, FollowPath[targetIndex].index) == WalkType.Jump && owner.IsOnGround() && AIJumpDelay > 2.5f)
                        {
                            AIJump();
                            AIJumpDelay = 0.0f;
                        }
                    }
                }
                break;
            case EAISubStatus.FollowSubRotateToTarget:
                if (FollowRotateToTargetCoroutine == null)
                {
                    FollowRotateToTargetCoroutine = owner.StartCoroutine(FollowRotateToTarget(FollowPath[targetIndex].pos));
                }
                break;
        }
    }

    //已经距离该角色很近了.开始打架
    public void KillTarget(MeteorUnit target)
    {

    }

    public void FollowTarget(int target)
    {
        followTarget = U3D.GetUnit(target);
        ChangeState(EAIStatus.Follow);
        SubStatus = EAISubStatus.FollowGotoTarget;
    }

    //防御时遭到攻击，也有防御动作
    void OnDefencePlaying()
    {

    }

    Coroutine struggleCoroutine;
    void OnHurt()
    {
        Debug.LogError("OnHurt");
        owner.controller.Input.ResetInput();
        if (owner.posMng.mActiveAction.Idx == CommonAction.Struggle || owner.posMng.mActiveAction.Idx == CommonAction.Struggle0)
        {
            if (struggleCoroutine == null && !owner.charLoader.InStraight)
                struggleCoroutine = owner.StartCoroutine(ProcessStruggle());
        }
        //SubStatus = EAISubStatus.KillGotoTarget;
    }

    IEnumerator ProcessStruggle()
    {
        Debug.Log("ProcessStruggle");
        yield return 0;
        while (true)
        {
            yield return 0;
            yield return 0;
            owner.controller.Input.OnKeyDown(EKeyList.KL_Attack, true);
            yield return 0;
            yield return 0;
            owner.controller.Input.OnKeyUp(EKeyList.KL_Attack);
            break;
        }
        struggleCoroutine = null;
    }

    //受伤僵直完毕后，切换为Idle
    void OnHurtDone()
    {
        owner.posMng.ChangeAction(CommonAction.Idle);
    }

    public void OnUnitDead(MeteorUnit deadunit)
    {
        tick = updateDelta;//下一次进入空闲立即刷新对象位置和方向。
        if (followTarget == deadunit)
        {
            if (Status == EAIStatus.Follow)
            {
                Status = EAIStatus.Wait;
                SubStatus = EAISubStatus.SubStatusWait;
            }
            followTarget = null;
        }
    }

    //更新路径间隔
    const float updateDelta = 1.0f;
    float tick = 0.0f;
    float attacktick = 0.0f;
    float waitCrouch;
    float waitDefence;

    //原地不动-
    void OnWait()
    {

    }

    //不动播放空闲动画 
    void OnIdle()
    {
        return;
        tick += Time.deltaTime;
        waitCrouch -= Time.deltaTime;
        waitDefence -= Time.deltaTime;
        if (tick > updateDelta)
        {
            MeteorUnit u = owner.GetLockedTarget();
            if (u == null)
            {
                targetPath.Clear();
                pathIdx = -1;
                targetPos = Vector3.zero;
            }
            else
            {
                //targetPath = GameBattleEx.Instance.FindPath(owner.transform.position, u);
                //pathIdx = 0;
                //targetPos = u.transform.position;
            }
            tick = 0.0f;
        }

        //if (targetPos != Vector3.zero && Vector3.Distance(targetPos, owner.transform.position) < 80)
        //{
            //owner.Defence();
            //Status = EAIStatus.Defence;
        //}
        //else
        //{
            //大于50M，尝试走过去。
            //if (targetPos != Vector3.zero)
            //{
            //    Status = EAIStatus.GotoTarget;//先朝目标转向，然后跑过去.不寻路先.
            //}
        //}
        //模拟AI计算下一步该做什么。
        //AI分为发现目标，和未发现目标2种情况下的行为概率.
        //if (targetPath != null)//targetPos != Vector3.zero)
        //{
            //有目标
            //Quaternion cur = Quaternion.LookRotation(targetPos - transform.position, Vector3.up);
            //if (Quaternion.Angle(transform.rotation, Quaternion.Inverse(targetQuat)) <= 10.0f)
            //{
                //使用什么武器？

                //走到什么位置？距离多远，是否存在需要跳跃才能过去的沟渠。

                //发什么招式？武器招式->对方是否跳跃 /技能 ->蓝是否充足

                //是否气血小于健康范围
                //是否没有足够蓝

                //距离应该是最重要的.
            //    float d = Vector3.Distance(transform.position, targetPos);
            //    if (AIData != null)
            //    {
            //        for (int i = 0; i < AIData.Count; i++)
            //        {
            //            if (d > AIData[i])
            //        }
            //    }
                
            //    Status = EAIStatus.Run;
            //    //owner.posMng.ChangeAction(CommonAction.Run);
            //}
            //else
            //{
            //    Status = EAIStatus.Rotate;
            //}
        //}
        //else
        //{

        //}
        int random = Global.Rand.Next(0, 101);
        //switch (SubStatus)
        //{
        //    //case EAISubStatus.Think://采取一个什么行动,朝目标丢招式,转向目标
                
        //        //break;
        //}
        //WSLog.LogFrame("随机0-7:得到" + random);
        if (PlayWeaponPoseCorout != null)
        {

        }
        else
        if (owner.posMng.mActiveAction.Idx == CommonAction.Crouch && waitCrouch <= 0.0f)
        {
            switch (random)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    owner.controller.Input.OnKeyUp(EKeyList.KL_Crouch);
                    break;
            }
        }
        else
        if (owner.posMng.mActiveAction.Idx <= 10)
        {
            //owner.posMng.ChangeAction(CommonAction.Taunt);
            switch (random)
            {
                case 0:
                case 1:
                    owner.controller.Input.OnKeyDown(EKeyList.KL_Defence, true);//防御
                    waitDefence = 1.0f;
                    break;
                case 2:
                case 3:
                    owner.controller.Input.OnKeyUp(EKeyList.KL_Attack);//攻击收起
                    break;
                case 4:
                case 5:
                    owner.controller.Input.OnKeyDown(EKeyList.KL_Attack, true);//攻击
                    break;
                case 6:
                    if (owner.AngryValue >= Global.ANGRYMAX)
                        owner.PlaySkill();//开大
                    break;
                case 7:
                    //TryPlayWeaponPose();//使用武器招式.
                    break;
                case 8:
                    owner.controller.Input.OnKeyDown(EKeyList.KL_Crouch, true);//蹲下
                    waitCrouch = 3.0f;
                    break;
                //case 9://双击某个方向键2次
                //    TryAvoid();
                //    break;
            }
        }
        else if (((owner.posMng.mActiveAction.Idx >= CommonAction.BrahchthrustDefence) && 
            (owner.posMng.mActiveAction.Idx <= CommonAction.HammerDefence)) || 
            ((owner.posMng.mActiveAction.Idx >= CommonAction.ZhihuDefence) &&
            (owner.posMng.mActiveAction.Idx <= CommonAction.RendaoDefence)))
        {
            if (random < 2 && waitDefence <= 0.0f)
                owner.ReleaseDefence();
            else if (random >= 3 && random <= 4 && waitDefence <= 0.0f && owner.AngryValue >= Global.ANGRYMAX)
                owner.DoBreakOut();
            else if (waitDefence <= 0.0f && owner.AngryValue >= Global.ANGRYMAX && random == 5)
                owner.PlaySkill();
            //else if (waitDefence <= 0.0f && random == 6)
            //    owner.posMng.ChangeAction(CommonAction.DCForw);
        }
        else
        if (owner.posMng.mActiveAction.Idx == CommonAction.Struggle || owner.posMng.mActiveAction.Idx == CommonAction.Struggle0)
        {
            if (!owner.charLoader.InStraight)
            {
                //可以从僵直状态切换出
                if (struggleCoroutine == null)
                    struggleCoroutine = owner.StartCoroutine(ProcessStruggle());
            }
        }
        else if (owner.posMng.IsAttackPose() && owner.controller.Input.AcceptInput())
        {
            //尝试输出下一个连招
        }
    }



    void GotoTarget(Vector3 pos)
    {
        switch (SubStatus)
        {

        }

    }

    //void Move(float deltaTime)
    //{
    //    //笔直朝前走就好了.
    //    Vector2 direction = new Vector2(-owner.transform.forward.x, -owner.transform.forward.z);
    //    if (direction == Vector2.zero)
    //        return;
    //    //如果摇杆按着边缘的方向键，触发任意方向，则移动，否则，旋转目标。
    //    //跳跃的时候，方向轴移动不受控制，模拟跳跃
    //    if (owner.IsOnGround())
    //    {
    //        if (owner.posMng.CanMove && owner.Speed > 0)
    //        {
    //            direction.Normalize();
    //            float runSpeed = owner.Speed;//跑的速度 1000 = 145M/S 按原来游戏计算

    //            Vector2 runTrans = direction * runSpeed * deltaTime;
    //            //怪物和AI在预览中无法跑动

    //            owner.Move(new Vector3((runTrans * 0.130f).x, 0, (runTrans * 0.130f).y));
    //            if (owner.posMng.mActiveAction.Idx != CommonAction.Run)
    //                owner.posMng.ChangeAction(CommonAction.Run);

    //            //小于30码防御吧。后面配置好数据
    //            if (Vector3.Distance(targetPos, owner.transform.position) <= 30)
    //            {
    //                //还是防御先。
    //                //Status = EAIStatus.Defence;
    //                owner.Defence();
    //                return;
    //            }
    //        }
    //    }
    //}

    //倒在地面上。判断是否在地面多躺一会，第二，哪个方向起身，第三，是否跳跃起身.第四，是否带方向速度.
    void FallDown(float time)
    {
        owner.Defence();
        //Status = EAIStatus.Defence;
    }

    //起身过程中（滚动起身），检查是否可以使用sc，取消掉当前起身动作，且切换到攻击动作。
    void FallUp(float time)
    {
        owner.Defence();
        //Status = EAIStatus.Defence;
    }

    //受到攻击，处于受击动作或者防御姿态硬直中
    public void Pause(bool pause, float pause_time)
    {
        paused = pause;
        pause_tick = pause_time;
        Debug.Log(string.Format("unit:{0} pause:{1}", owner.name, pause_tick));
    }

    public void EnableAI(bool enable)
    {
        stoped = !enable;
    }

    public void ChangeState(EAIStatus type)
    {
        Status = type;
        StopCoroutine();
        Stop();
        ResetAIKey();
        if (type == EAIStatus.Kill)
        {
            Debug.Log("changestatus:kill");
            SubStatus = EAISubStatus.KillGotoTarget;
            //killTarget = owner.GetLockedTarget();
        }
        else if (type == EAIStatus.GotoPatrol)
        {
            SubStatus = EAISubStatus.Patrol;
        }
        else if (type == EAIStatus.PatrolInPlace)
        {
            SubStatus = EAISubStatus.PatrolSubInPlace;
        }
        else if (type == EAIStatus.Follow)
        {
            SubStatus = EAISubStatus.FollowGotoTarget;
        }
        else if (type == EAIStatus.Guard)
        {
            waitDefence = 2.0f;//2秒后开始判断能否释放防御键
            owner.controller.Input.OnKeyDown(EKeyList.KL_Defence, true);//防御
        }
        else if (type == EAIStatus.AttackTarget)
        {
            //朝目标处攻击数次
            RefreshPath(owner.mPos, AttackTarget);
            SubStatus = EAISubStatus.AttackGotoTarget;
        }
        else if (type == EAIStatus.GetItem)
        {
            SubStatus = EAISubStatus.GetItemMissItem;
        }
    }

    void StopCoroutine()
    {
        if (PlayWeaponPoseCorout != null)
        {
            owner.StopCoroutine(PlayWeaponPoseCorout);
            PlayWeaponPoseCorout = null;
        }

        if (leaveRotateCorout != null)
        {
            owner.StopCoroutine(leaveRotateCorout);
            leaveRotateCorout = null;
        }

        if (PatrolRotateCoroutine != null)
        {
            owner.StopCoroutine(PatrolRotateCoroutine);
            PatrolRotateCoroutine = null;
        }

        if (FollowRotateToTargetCoroutine != null)
        {
            owner.StopCoroutine(FollowRotateToTargetCoroutine);
            FollowRotateToTargetCoroutine = null;
        }

        if (JumpCoroutine != null)
        {
            owner.StopCoroutine(JumpCoroutine);
            JumpCoroutine = null;
        }

        if (InputCorout != null)
        {
            owner.StopCoroutine(InputCorout);
            InputCorout = null;
        }

        if (AttackTargetCoroutine != null)
        {
            owner.StopCoroutine(AttackTargetCoroutine);
            AttackTargetCoroutine = null;
        }

        if (AttackRotateToTargetCoroutine != null)
        {
            owner.StopCoroutine(AttackRotateToTargetCoroutine);
            AttackRotateToTargetCoroutine = null;
        }
    }

    public void ResetAIKey()
    {
        owner.controller.Input.OnKeyUp(EKeyList.KL_Defence);
    }

    Coroutine PlayWeaponPoseCorout;

    //触发首招
    void TryPlayWeaponPose(int KeyMap)
    {
        if (PlayWeaponPoseCorout != null)
            return;
        PlayWeaponPoseCorout = owner.StartCoroutine(PlayWeaponPose(KeyMap));
    }

    Coroutine Burst;
    void AIBurst(EKeyList key)
    {
        if (Burst == null)
            Burst = owner.StartCoroutine(VirtualKeyEvent2(key));
    }

    IEnumerator VirtualKeyEvent2(EKeyList key)
    {
        owner.controller.Input.OnKeyDown(key, true);
        yield return 0;
        yield return 0;
        owner.controller.Input.OnKeyUp(key);
        yield return 0;
        yield return 0;
        owner.controller.Input.OnKeyDown(key, true);
        yield return 0;
        yield return 0;
        owner.controller.Input.OnKeyUp(key);
        yield return 0;
        yield return 0;
        Burst = null;
    }

    //单键输入.
    IEnumerator VirtualKeyEvent(EKeyList key)
    {
        owner.controller.Input.OnKeyDown(key, true);
        yield return 0;
        yield return 0;
        yield return 0;
        owner.controller.Input.OnKeyUp(key);
        yield return 0;
        yield return 0;
        yield return 0;
        InputCorout = null;
    }

    Coroutine AttackTargetCoroutine;
    IEnumerator Attack()
    {
        owner.controller.Input.OnKeyDown(EKeyList.KL_Attack, true);
        yield return 0;
        yield return 0;
        owner.controller.Input.OnKeyUp(EKeyList.KL_Attack);
        yield return 0;
        yield return 0;
        AttackTargetCoroutine = null;
    }

    IEnumerator PlayWeaponPose(int KeyMap)
    {
        List<VirtualInput> skill = VirtualInput.CalcPoseInput(KeyMap);
        for (int i = 0; i < skill.Count; i++)
        {
            owner.controller.Input.OnKeyDown(skill[i].key, true);
            yield return 0;
            owner.controller.Input.OnKeyUp(skill[i].key);
            yield return 0;
        }
        PlayWeaponPoseCorout = null;
    }

    //如果attacker为空，则代表是非角色伤害了自己
    public void OnDamaged(MeteorUnit attacker)
    {
        //受到非目标的攻击后，记下来，一会找他算账
        if (lastAttacker != attacker && owner.GetLockedTarget() != attacker && attacker != null)
            lastAttacker = attacker;
    }

    //寻路相关的.
    public int curPatrolIndex;
    int startPathIndex;
    bool reverse = false;
    public int targetPatrolIndex;
    public List<WayPoint> PatrolPath = new List<WayPoint>();
    List<WayPoint> PatrolPathBegin = new List<WayPoint>();
    public void SetPatrolPath(List<int> path)
    {
        PatrolPath.Clear();
        List<WayPoint> PathTmp = new List<WayPoint>();
        for (int i = 0; i < path.Count; i++)
            PathTmp.Add(Global.GLevelItem.wayPoint[path[i]]);

        //计算从第一个点到最后一个点的完整路径，放到完整巡逻点钟
        List<int> idx = new List<int>();
        //单点巡逻
        if (PathTmp.Count == 1)
            idx.Add(PathTmp[0].index);
        else
        {
            //多点巡逻
            for (int i = 0; i < PathTmp.Count - 1; i++)
            {
                List<WayPoint> r = PathMng.Instance.FindPath(PathTmp[i].index, PathTmp[i + 1].index);
                if (r.Count != 0)
                {
                    if (idx.Count == 0)
                        idx.Add(r[0].index);
                    for (int j = 1; j < r.Count; j++)
                        idx.Add(r[j].index);
                }
            }
        }
        for (int i = 0; i < idx.Count; i++)
            PatrolPath.Add(Global.GLevelItem.wayPoint[idx[i]]);
        //Debug.LogError(string.Format("{0}设置巡逻路径", owner.name));
        //for (int i = 0; i < PatrolPath.Count; i++)
        //{
        //    Debug.LogError(string.Format("{0}", PatrolPath[i].index));
        //}
        //-1代表在当前角色所在位置
        curPatrolIndex = -1;
        targetPatrolIndex = -1;
        //SubStatus = EAISubStatus.Patrol;
        startPathIndex = PathMng.Instance.GetWayIndex(owner.mPos);
        PatrolPathBegin = PathMng.Instance.FindPath(startPathIndex, idx[0]);
        if (PatrolPathBegin.Count != 0)
            PatrolPathBegin.RemoveAt(PatrolPathBegin.Count - 1);
    }

    //绕原地
    Coroutine PatrolRotateCoroutine;//巡逻到达某个目的点后，会随机旋转1-5次，每次都会停留不固定的时间
    IEnumerator PatrolRotate(float angleSpeed = 90.0f)
    {
        float rotateAngle1 = Random.Range(-90, -45);
        float rotateAngle2 = Random.Range(45, 90);
        bool right = Random.Range(0, 100) >= 50;
        float rotateAngle = right ? rotateAngle2 : rotateAngle1;
        Quaternion quat = Quaternion.AngleAxis(rotateAngle, Vector3.up);
        float offset = 0.0f;
        float timeTotal = Mathf.Abs(rotateAngle / angleSpeed);
        float timeTick = 0.0f;
        while (true)
        {
            timeTick += Time.deltaTime;
            float yOffset = Mathf.Lerp(0, rotateAngle, timeTick / timeTotal);
            float r = yOffset - offset;
            owner.SetOrientation(r);
            offset = yOffset;
            if (timeTick > timeTotal)
            {
                //owner.FaceToTarget(target);
                if (owner.posMng.mActiveAction.Idx == CommonAction.WalkRight || owner.posMng.mActiveAction.Idx == CommonAction.WalkLeft)
                    owner.posMng.ChangeAction(0, 0.1f);
                break;
            }
            yield return 0;
        }

        //随机停留0.8-4秒，下一次旋转间隔
        float wait = Random.Range(0.8f, 3.0f);
        while (true)
        {
            wait -= Time.deltaTime;
            if (wait <= 0.0f)
                break;
            yield return 0;
        }

        RotateRound--;
        PatrolRotateCoroutine = null;
    }

    //得到某个角色的面向向量与某个位置的夹角,不考虑Y轴
    float GetAngleBetween(Vector3 vec)
    {
        Vector3 vec1 = -owner.transform.forward;
        Vector3 vec2 = (vec - owner.mPos).normalized;
        float radian = Vector3.Dot(vec1, vec2);
        float degree = Mathf.Acos(radian) * Mathf.Rad2Deg;
        //Debug.LogError("夹角:" + degree);
        return degree;
    }

    Coroutine GetItemRotateToTargetCoroutine;
    IEnumerator GetItemRotateToTarget(Vector3 vec)
    {
        Vector3 diff = (vec - owner.mPos);
        diff.y = 0;
        float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        float angle = Mathf.Abs(Mathf.Acos(dot) * Mathf.Rad2Deg);
        bool rightRotate = dot2 < 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / 150.0f;
        float timeTick = 0.0f;
        while (true)
        {
            timeTick += Time.deltaTime;
            float yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            owner.SetOrientation(yOffset - offset);
            offset = yOffset;
            if (timeTick > timeTotal)
            {
                if (owner.posMng.mActiveAction.Idx == CommonAction.WalkRight || owner.posMng.mActiveAction.Idx == CommonAction.WalkLeft)
                    owner.posMng.ChangeAction(0, 0.1f);
                break;
            }
            yield return 0;
        }
        GetItemRotateToTargetCoroutine = null;
        SubStatus = EAISubStatus.GetItemGotoItem;
    }

    Coroutine AttackRotateToTargetCoroutine;
    IEnumerator AttackRotateToTarget(Vector3 vec)
    {
        Vector3 diff = (vec - owner.mPos);
        diff.y = 0;
        float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        float angle = Mathf.Abs(Mathf.Acos(dot) * Mathf.Rad2Deg);
        bool rightRotate = dot2 < 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / 150.0f;
        float timeTick = 0.0f;
        while (true)
        {
            timeTick += Time.deltaTime;
            float yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            owner.SetOrientation(yOffset - offset);
            offset = yOffset;
            if (timeTick > timeTotal)
            {
                //owner.FaceToTarget(vec);
                if (owner.posMng.mActiveAction.Idx == CommonAction.WalkRight || owner.posMng.mActiveAction.Idx == CommonAction.WalkLeft)
                    owner.posMng.ChangeAction(0, 0.1f);
                break;
            }
            yield return 0;
        }
        AttackRotateToTargetCoroutine = null;
        SubStatus = EAISubStatus.AttackGotoTarget;
    }

    Coroutine FollowRotateToTargetCoroutine;
    IEnumerator FollowRotateToTarget(Vector3 vec)
    {
        Vector3 diff = (vec - owner.mPos);
        diff.y = 0;
        float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        float angle = Mathf.Abs(Mathf.Acos(dot) * Mathf.Rad2Deg);
        bool rightRotate = dot2 < 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / 150.0f;
        float timeTick = 0.0f;
        while (true)
        {
            timeTick += Time.deltaTime;
            float yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            owner.SetOrientation(yOffset - offset);
            offset = yOffset;
            if (timeTick > timeTotal)
            {
                //owner.FaceToTarget(vec);
                if (owner.posMng.mActiveAction.Idx == CommonAction.WalkRight || owner.posMng.mActiveAction.Idx == CommonAction.WalkLeft)
                    owner.posMng.ChangeAction(0, 0.1f);
                break;
            }
            yield return 0;
        }
        FollowRotateToTargetCoroutine = null;
        SubStatus = EAISubStatus.FollowGotoTarget;
    }

    //朝向指定目标，一定时间内
    Coroutine PatrolRotateToTargetCoroutine;
    IEnumerator PatrolRotateToTarget(Vector3 vec)
    {
        //WsGlobal.AddDebugLine(vec, vec + Vector3.up * 10, Color.red, "PatrolPoint", 20.0f);
        Vector3 diff = (vec - owner.mPos);
        diff.y = 0;
        float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        float angle = Mathf.Abs(Mathf.Acos(dot) * Mathf.Rad2Deg);
        bool rightRotate = dot2 < 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / 150.0f;
        float timeTick = 0.0f;
        while (true)
        {
            timeTick += Time.deltaTime;
            float yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            owner.SetOrientation(yOffset - offset);
            offset = yOffset;
            if (timeTick > timeTotal)
            {
                //owner.FaceToTarget(vec);
                if (owner.posMng.mActiveAction.Idx == CommonAction.WalkRight || owner.posMng.mActiveAction.Idx == CommonAction.WalkLeft)
                    owner.posMng.ChangeAction(0, 0.1f);
                break;
            }
            yield return 0;
        }
        PatrolRotateToTargetCoroutine = null;
        SubStatus = EAISubStatus.PatrolSubGotoTarget;
    }

    int RotateRound;
    float AIJumpDelay = 0.0f;
    float AIFollowRefresh = 0.0f;

    void OnPatrolInPlace()
    {
        switch (SubStatus)
        {
            case EAISubStatus.PatrolSubInPlace:
                if (PatrolRotateCoroutine == null)
                {
                    int random = Random.Range(0, 100);
                    if (random < 100)
                        PatrolRotateCoroutine = owner.StartCoroutine(PatrolRotate(90.0f));
                }
                break;
        }
    }

    void OnGotoPatrol()
    {
        switch (SubStatus)
        {
            case EAISubStatus.Patrol:
                {
                    //顺序巡逻
                    if (curPatrolIndex == PatrolPathBegin.Count - 1)
                    {
                        Status = PatrolPath.Count == 1 ? EAIStatus.PatrolInPlace : EAIStatus.Patrol;
                        if (PatrolPath.Count == 1)
                            SubStatus = EAISubStatus.PatrolSubInPlace;
                        curPatrolIndex = -1;
                        targetPatrolIndex = -1;
                        PatrolPathBegin.Clear();
                        break;
                    }
                    else
                        targetPatrolIndex = (curPatrolIndex + 1) % PatrolPathBegin.Count;
                    if (targetPatrolIndex != curPatrolIndex)
                    {
                        if (PatrolPathBegin.Count <= targetPatrolIndex)
                        {
                            //Debug.LogError("PatrolPath->OnIdle");
                            OnIdle();
                            return;
                        }

                        //中断寻路，当距离小于下一帧移动的距离时.
                        //if (Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(PatrolPathBegin[targetPatrolIndex].pos.x, 0, PatrolPathBegin[targetPatrolIndex].pos.z)) <= owner.Speed * Time.deltaTime * 0.13f)
                        //{
                        //    owner.controller.Input.AIMove(0, 0);
                        //    RotateRound = Random.Range(1, 3);
                        //    SubStatus = EAISubStatus.PatrolSubRotateInPlace;//到指定地点后旋转
                        //    curPatrolIndex = targetPatrolIndex;
                        //    //Debug.LogError("进入巡逻子状态-到底指定地点后原地旋转.PatrolSubRotateInPlace");
                        //    return;
                        //}
                        //Debug.LogError("进入巡逻子状态-朝目标旋转");
                        SubStatus = EAISubStatus.PatrolSubRotateToTarget;//准备先对准目标
                    }
                    else
                    {
                        RotateRound = Random.Range(1, 3);
                        SubStatus = EAISubStatus.PatrolSubRotateInPlace;
                    }
                }
                break;
            case EAISubStatus.PatrolSubRotateInPlace:
                if (RotateRound > 0)
                {
                    if (PatrolRotateCoroutine == null)
                    {
                        //Debug.LogError("进入巡逻子状态-到底指定地点后旋转.启动协程");
                        PatrolRotateCoroutine = owner.StartCoroutine(PatrolRotate());
                    }
                }
                else
                {
                    //旋转轮次使用完毕，下一次巡逻
                    SubStatus = EAISubStatus.Patrol;
                }
                break;
            case EAISubStatus.PatrolSubRotateToTarget:
                if (PatrolRotateToTargetCoroutine == null)
                {
                    //Debug.LogError("进入巡逻子状态-朝目标旋转.启动协程");
                    //switch (Status)
                    //{
                    //    case EAIStatus.GotoPatrol:
                            PatrolRotateToTargetCoroutine = owner.StartCoroutine(PatrolRotateToTarget(PatrolPathBegin[targetPatrolIndex].pos));
                            //break;
                        //case EAIStatus.Patrol:
                        //    PatrolRotateToTargetCoroutine = owner.StartCoroutine(PatrolRotateToTarget(PatrolPath[targetPatrolIndex].pos));
                        //    break;
                    //}
                    //PatrolRotateToTargetCoroutine = owner.StartCoroutine(PatrolRotateToTarget(PatrolPath[targetPatrolIndex].pos));
                }
                break;
            case EAISubStatus.PatrolSubGotoTarget:
                //Debug.LogError("进入巡逻子状态-朝目标输入移动");
                //switch (Status)
                //{
                    //case EAIStatus.Patrol:
                    //    owner.FaceToTarget(new Vector3(PatrolPath[targetPatrolIndex].pos.x, 0, PatrolPath[targetPatrolIndex].pos.z));
                    //    owner.controller.Input.AIMove(0, 1);
                    //    //模拟跳跃键，移动到下一个位置.还得按住上
                    //    if (curPatrolIndex != -1)
                    //    {
                    //        if (PathMng.Instance.GetWalkMethod(PatrolPath[curPatrolIndex].index, PatrolPath[targetPatrolIndex].index) == WalkType.Jump && owner.IsOnGround() && AIJumpDelay > 2.5f)
                    //        {
                    //            AIJump();
                    //            AIJumpDelay = 0.0f;
                    //        }
                    //    }
                    //    break;
                    //case EAIStatus.GotoPatrol:
                        owner.FaceToTarget(new Vector3(PatrolPathBegin[targetPatrolIndex].pos.x, 0, PatrolPathBegin[targetPatrolIndex].pos.z));
                        owner.controller.Input.AIMove(0, 1);
                        //模拟跳跃键，移动到下一个位置.还得按住上
                        if (curPatrolIndex != -1)
                        {
                            if (PathMng.Instance.GetWalkMethod(PatrolPathBegin[curPatrolIndex].index, PatrolPathBegin[targetPatrolIndex].index) == WalkType.Jump && owner.IsOnGround() && AIJumpDelay > 2.5f)
                            {
                                AIJump();
                                AIJumpDelay = 0.0f;
                            }
                        }
                        //break;
                //}
                break;
        }
    }

    void OnPatrol()
    {
        switch (SubStatus)
        {
            case EAISubStatus.Patrol:
                //Debug.LogError("进入巡逻子状态-EAISubStatus.Patrol");
                {
                    //逆序巡逻
                    if (reverse)
                    {
                        if (curPatrolIndex == 0)
                        {
                            reverse = false;
                            break;
                        }
                        else
                        {
                            targetPatrolIndex = (curPatrolIndex - 1) % PatrolPath.Count;
                            if (targetPatrolIndex != curPatrolIndex)
                            {
                                if (PatrolPath.Count <= targetPatrolIndex)
                                {
                                    OnIdle();
                                    return;
                                }

                                //Debug.LogError("进入巡逻子状态-朝目标旋转");
                                SubStatus = EAISubStatus.PatrolSubRotateToTarget;//准备先对准目标
                            }
                            else
                            {
                                RotateRound = Random.Range(1, 3);
                                SubStatus = EAISubStatus.PatrolSubRotateInPlace;
                            }
                        }
                    }
                    else
                    {
                        //顺序巡逻
                        if (curPatrolIndex == PatrolPath.Count - 1)
                        {
                            reverse = true;
                            break;
                        }
                        else
                            targetPatrolIndex = (curPatrolIndex + 1) % PatrolPath.Count;
                        if (targetPatrolIndex != curPatrolIndex)
                        {
                            if (PatrolPath.Count <= targetPatrolIndex)
                            {
                                //Debug.LogError("PatrolPath->OnIdle");
                                OnIdle();
                                return;
                            }

                            //Debug.LogError("进入巡逻子状态-朝目标旋转");
                            SubStatus = EAISubStatus.PatrolSubRotateToTarget;//准备先对准目标
                        }
                        else
                        {
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.PatrolSubRotateInPlace;
                        }
                    }
                }
                break;
            case EAISubStatus.PatrolSubRotateInPlace:
                if (RotateRound > 0)
                {
                    if (PatrolRotateCoroutine == null)
                    {
                        //Debug.LogError("进入巡逻子状态-到底指定地点后旋转.启动协程");
                        PatrolRotateCoroutine = owner.StartCoroutine(PatrolRotate());
                    }
                }
                else
                {
                    //旋转轮次使用完毕，下一次巡逻
                    SubStatus = EAISubStatus.Patrol;
                }
                break;
            case EAISubStatus.PatrolSubRotateToTarget:
                if (PatrolRotateToTargetCoroutine == null)
                {
                    //Debug.LogError("进入巡逻子状态-朝目标旋转.启动协程");
                    //switch (Status)
                    //{
                        //case EAIStatus.GotoPatrol:
                        //    PatrolRotateToTargetCoroutine = owner.StartCoroutine(PatrolRotateToTarget(PatrolPathBegin[targetPatrolIndex].pos));
                        //    break;
                        //case EAIStatus.Patrol:
                            PatrolRotateToTargetCoroutine = owner.StartCoroutine(PatrolRotateToTarget(PatrolPath[targetPatrolIndex].pos));
                            //break;
                    //}
                    //PatrolRotateToTargetCoroutine = owner.StartCoroutine(PatrolRotateToTarget(PatrolPath[targetPatrolIndex].pos));
                }
                break;
            case EAISubStatus.PatrolSubGotoTarget:
                //Debug.LogError("进入巡逻子状态-朝目标输入移动");
                switch (Status)
                {
                    case EAIStatus.Patrol:
                        owner.FaceToTarget(new Vector3(PatrolPath[targetPatrolIndex].pos.x, 0, PatrolPath[targetPatrolIndex].pos.z));
                        owner.controller.Input.AIMove(0, 1);
                        //模拟跳跃键，移动到下一个位置.还得按住上
                        //UnityEngine.Debug.LogError(string.Format("PatrolPath.Count:{0}- curPatrolIndex:{1}，targetPatrolIndex:{2}", PatrolPath.Count, curPatrolIndex, targetPatrolIndex));
                        if (curPatrolIndex != -1)
                        {
                            if (PathMng.Instance.GetWalkMethod(PatrolPath[curPatrolIndex].index, PatrolPath[targetPatrolIndex].index) == WalkType.Jump && owner.IsOnGround() && AIJumpDelay > 2.5f)
                            {
                                AIJump();
                                AIJumpDelay = 0.0f;
                            }
                        }
                        break;
                    //case EAIStatus.GotoPatrol:
                    //    owner.FaceToTarget(new Vector3(PatrolPathBegin[targetPatrolIndex].pos.x, 0, PatrolPathBegin[targetPatrolIndex].pos.z));
                    //    owner.controller.Input.AIMove(0, 1);
                    //    //模拟跳跃键，移动到下一个位置.还得按住上
                    //    if (curPatrolIndex != -1)
                    //    {
                    //        if (PathMng.Instance.GetWalkMethod(PatrolPathBegin[curPatrolIndex].index, PatrolPathBegin[targetPatrolIndex].index) == WalkType.Jump && owner.IsOnGround() && AIJumpDelay > 2.5f)
                    //        {
                    //            AIJump();
                    //            AIJumpDelay = 0.0f;
                    //        }
                    //    }
                    //    break;
                }
                break;
                
        }
    }

    //AI模拟按键

    /// <summary>
    /// 寻路中模拟跳跃.爬向房顶之类的
    /// </summary>
    /// 不要随便关掉owner，否则其上所有协程都会失效.
    Coroutine JumpCoroutine;
    void AIJump()
    {
        if (JumpCoroutine == null)
            JumpCoroutine = owner.StartCoroutine(AIJumpCorout());
    }

    IEnumerator AIJumpCorout()
    {
        owner.controller.Input.OnKeyDown(EKeyList.KL_Jump, true);
        for (int i = 0; i < 12; i++)
            yield return 0;
        owner.controller.Input.OnKeyUp(EKeyList.KL_Jump);
        JumpCoroutine = null;
    }

    
    public void OnGotoTargetPoint(int targetIndex)
    {
        if (Status == EAIStatus.AttackTarget)
        {
            if (Vector3.Distance(owner.mPos, AttackTarget) <= 5)
            {
                Debug.LogError("goto target point");
                SubStatus = EAISubStatus.AttackTarget;
            }
        }
    }

    public void OnGotoWayPoint(int wayIndex)
    {
        switch (Status)
        {
            case EAIStatus.GotoPatrol:
                {
                    int idx = -1;
                    for (int i = 0; i < PatrolPathBegin.Count; i++)
                    {
                        if (PatrolPathBegin[i].index == wayIndex)
                        {
                            idx = i;
                            break;
                        }
                    }

                    switch (SubStatus)
                    {
                        case EAISubStatus.PatrolSubGotoTarget:
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.PatrolSubRotateInPlace;//到底指定地点后旋转
                            curPatrolIndex = idx;
                            owner.controller.Input.AIMove(0, 0);
                            break;
                        case EAISubStatus.Patrol:
                            owner.controller.Input.AIMove(0, 0);
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.PatrolSubRotateInPlace;//到指定地点后旋转
                            curPatrolIndex = idx;
                            break;
                    }
                }
                break;
                
            case EAIStatus.Patrol:
                {
                    int idx = -1;
                    for (int i = 0; i < PatrolPath.Count; i++)
                    {
                        if (PatrolPath[i].index == wayIndex)
                        {
                            idx = i;
                            break;
                        }
                    }
                    switch (SubStatus)
                    {
                        case EAISubStatus.Patrol:
                            owner.controller.Input.AIMove(0, 0);
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.PatrolSubRotateInPlace;//到底指定地点后旋转
                            curPatrolIndex = idx;
                            break;
                        case EAISubStatus.PatrolSubGotoTarget:
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.PatrolSubRotateInPlace;//到底指定地点后旋转
                            curPatrolIndex = idx;
                            //Debug.LogError("进入巡逻子状态-到底指定地点后原地旋转.PatrolSubRotateInPlace");
                            owner.controller.Input.AIMove(0, 0);
                            break;
                    }
                }
                break;
        }
    }
}

public class VirtualInput
{
    public EKeyList key;
    static bool InGroup(int [] group, int target)
    {
        for (int i = 0; i < group.Length; i++)
        {
            if (group[i] == target)
                return true;
        }
        return false;
    }

    public static List<VirtualInput> CalcPoseInput(int KeyMap)
    {
        List<VirtualInput> skill = new List<VirtualInput>();
        //普通攻击.
        int[] GroundKeyMap = new int[] { 1, 5, 9, 13, 25, 35, 48, 61, 73, 91, 95, 101, 108, 123 };
        int[] AirKeyMap = new int[] { 85, 22, 32, 46, 59, 107, 122, 105 };
        if (InGroup(GroundKeyMap, KeyMap) || InGroup(AirKeyMap, KeyMap))
        {
            VirtualInput v = new VirtualInput();
            v.key = EKeyList.KL_Attack;
            skill.Add(v);
            return skill;
        }

        //其他带方向连招的.
        int[] slash0Ground = new int[] { 3, 7, 11, 19, 37, 84, 49, 63, 75, 92, 98, 113, 125, 24 };//下A地面
        int[] slash0Air = new int[] { 24, 33, 47, 60, 72, 83, 106, 109, 126 };//下A空中
        if (InGroup(slash0Ground, KeyMap) || InGroup(slash0Air, KeyMap))
        {
            VirtualInput s = new VirtualInput();
            s.key = EKeyList.KL_KeyS;
            skill.Add(s);
            VirtualInput j = new VirtualInput();
            j.key = EKeyList.KL_Attack;
            skill.Add(j); 
            return skill;
        }

        int[] slash1Ground = new int[] { 16, 28, 38, 64, 89, 100 };//左攻击
        if (InGroup(slash1Ground, KeyMap))
        {
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
        if (InGroup(slash2Ground, KeyMap))
        {
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
        if (InGroup(slash3Ground, KeyMap) || InGroup(slash3Air, KeyMap))
        {
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
        if (InGroup(slash4Ground, KeyMap) || InGroup(slash4Air, KeyMap))
        {
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
        if (InGroup(slash5Ground, KeyMap) || InGroup(slash5Air, KeyMap))
        {
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
        if (InGroup(slash6Ground, KeyMap) || InGroup(slash6Air, KeyMap))
        {
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
        if (InGroup(slash7Ground, KeyMap) || InGroup(slash7Air, KeyMap))
        {
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
        if (InGroup(slash8Ground, KeyMap) || InGroup(slash8Air, KeyMap))
        {
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
        if (InGroup(slash9Ground, KeyMap) || InGroup(slash9Air, KeyMap))
        {
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
        if (InGroup(slash10Ground, KeyMap))
        {
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
        if (InGroup(slash11Ground, KeyMap))
        {
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
        if (InGroup(slash12Ground, KeyMap))
        {
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
        if (InGroup(slash13Ground, KeyMap))
        {
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
        if (InGroup(slash14Ground, KeyMap))
        {
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
        if (InGroup(slash15Ground, KeyMap))
        {
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
        if (InGroup(slash16Ground, KeyMap))
        {
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
        if (InGroup(slash17Ground, KeyMap))
        {
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
        if (InGroup(slash18Ground, KeyMap))
        {
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