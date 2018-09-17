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
    Think,//没发觉目标时左右观察
    GotoPatrol,//去巡逻路径的第一个位置.
    Patrol,//巡逻。
    PatrolInPlace,//单点巡逻
    Wait,//，类似于Search
    Dodge,//逃跑
    Look,//四处看
    GetItem,//取得场景道具-最近的，可拾取的(未打碎的箱子不算在内)。
    AttackTarget,//攻击指定位置
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
    FightGotoTarget,//在打斗前，必须走到目标范围附近,就是朝目标移动
    FightGotoPosition,//朝目的地移动，需要寻路.
    FightSubRotateToTarget,//走到路点后，旋转朝向下一个路点.
    FightSubLeavetoTarget,//离开、逃跑.

    Fight,//实际出招/向其他状态切换，
    FightNear,//距离已经在一个寻路点以内的移动直接走过去.
    FightNearTarget,//距离指定目标已是直线距离
    FightAim,//战斗中若使用远程武器，瞄准敌方的几率，否则就随机
    //FightChangeWeapon,//战斗中切换武器->可能会跑近（使用近战武器），或跑远（使用远程武器）
    FightLeave,//跑远
    FightGetItem,//战斗中去捡起物品-镖物-补给-武器-BUFF道具
    SubStatusIdle,
    SubStatusWait,
    FollowGotoTarget,
    FollowSubRotateToTarget,
    Patrol,
    PatrolSubInPlace,
    PatrolSubRotateInPlace,//原地随机旋转.
    PatrolSubRotateToTarget,//原地一定时间内旋转到指定方向
    PatrolSubGotoTarget,//跑向指定位置
    KillThink,//思考是走近还是切换状态
    KillGetTarget,//离角色很近了，可以攻击
    KillGotoTarget,//离角色一定距离，需要先跑过去
    
    AttackGotoTarget,//攻击指定位置-寻路中.
    AttackTarget,//攻击指定位置-攻击中
    AttackTargetSubRotateToTarget,//攻击指定位置，到达中间某个寻路点时.
    GetItemGotoItem,//朝物件走的路线中
    GetItemSubRotateToItem,//到达中间某个寻路点，朝向下个寻路点
    GetItemMissItem,//可能目标已无效（被其他NPC拾取、目标发生转换（镖物某NPC死后，落到地面-数秒内归位））
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
    //public Dictionary<int, AISet> AIData;
    //当前目标路径
    int pathIdx = -1;
    Vector3 TargetPos = Vector3.zero;
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
            Stop();
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
                //Debug.Log(string.Format("unit:{0} IsPerforming", owner.name));
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
                if (killTarget != null)
                    fightTarget = killTarget;
                else
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
                    SubStatus = EAISubStatus.SubStatusWait;
                }
            }
        }

        if (Status == EAIStatus.Wait)
        {
            //一些事情优先级比较高，先处理
            //视线内存在可碰触的场景道具
            if (owner.GetSceneItemTarget() != null && owner.GetSceneItemTarget().CanPickup())
            {
                int getItem = Random.Range(0, 100);
                if (getItem < owner.Attr.GetItem)
                {
                    ChangeState(EAIStatus.GetItem);
                    return;
                }
            }

            if (killTarget != null)
            {
                ChangeState(EAIStatus.Kill);
                return;
            }

            if (owner.GetLockedTarget() != null)
            {
                //killTarget = owner.GetLockedTarget();
                fightTarget = owner.GetLockedTarget();
                Status = EAIStatus.Fight;
                SubStatus = EAISubStatus.FightAim;
                return;
            }

            if (followTarget != null)
            {
                ChangeState(EAIStatus.Follow);
                return;
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
                        case EAISubStatus.KillThink:
                            OnKillThink();
                            break;
                        case EAISubStatus.KillGotoTarget:
                        case EAISubStatus.FollowGotoTarget:
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
                        case EAISubStatus.FollowSubRotateToTarget:
                            if (FollowRotateToTargetCoroutine == null)
                            {
                                FollowRotateToTargetCoroutine = owner.StartCoroutine(FollowRotateToTarget(FollowPath[targetIndex].pos, EAISubStatus.FollowGotoTarget));
                            }
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
        FightInDanger();
    }

    public void OnChangeWeapon()
    {
        
    }

    //捡取场景道具
    void OnGetItem()
    {
        switch (SubStatus)
        {
            case EAISubStatus.GetItemMissItem:
                if (owner.GetSceneItemTarget() == null)
                {
                    //上一帧拾取了物品，这一帧退回去
                    ChangeState(EAIStatus.Wait);
                    return;
                }
                Path = RefreshPath(owner.mPos, owner.GetSceneItemTarget().transform.position);
                SubStatus = EAISubStatus.GetItemGotoItem;
                break;
            case EAISubStatus.GetItemGotoItem:
                if (Path.Count == 0 || AIFollowRefresh >= 3.0f)
                {
                    if (owner.GetSceneItemTarget() != null)
                        Path = RefreshPath(owner.mPos, owner.GetSceneItemTarget().transform.position);
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
                        return;
                    }
                    //尝试几率跳跃，否则可能会被卡住.
                    int random = Random.Range(0, 100);
                    if (random < owner.Attr.Jump && AIJumpDelay >= 2.5f)
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

    //四处看.
    Coroutine LookRotateToTargetCoroutine;
    void OnLook()
    {
        if (LookRotateToTargetCoroutine == null && owner.GetLockedTarget() != null)
        {
            float angle = Random.Range(-60, 60);
            Stop();//停止移动.
            owner.posMng.ChangeAction();
            LookRotateToTargetCoroutine = owner.StartCoroutine(LookRotateToTarget(angle));
        }
    }

    IEnumerator LookRotateToTarget(float leaveAngle, float angularspeed = 150.0f)
    {
        bool rightRotate = Random.Range(-1, 2) >= 0;
        float offset = 0.0f;
        float offsetmax = leaveAngle;
        float timeTotal = offsetmax / angularspeed;
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += Time.deltaTime;
            if (rightRotate)
                yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            else
                yOffset = -Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
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
        LookRotateToTargetCoroutine = null;
        Status = EAIStatus.Fight;
        SubStatus = EAISubStatus.Fight;
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
                        if (burst <= Global.BreakChange)
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
        //给一定几率让处于危险状态逃跑中，切换到与角色对打
        int random = Random.Range(0, 100);
        if (random < 5)
            ChangeState(EAIStatus.Wait);
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
            float yOffset = 0.0f;
            timeTick += Time.deltaTime;
            if (rightRotate)
                yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            else
                yOffset = -Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
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
    }

    public void Move()
    {
        owner.controller.Input.AIMove(0, 1);
    }

    public void Stop()
    {
        owner.controller.Input.AIMove(0, 0);
    }

    void TryAttack(int attack = 0)
    {
        if (attack == 0)
            attack = Random.Range(0, 100);
        //已经在攻击招式中.后接招式挑选
        //把可以使用的招式放到集合里，A类放普攻，B类放搓招， C类放绝招
        ActionNode act = ActionInterrupt.Instance.GetActions(owner.posMng.mActiveAction.Idx);
        if (act != null)
        {
            //int attack = Random.Range(0, 100);
            //owner.Attr.Attack1 = 0;
            //owner.Attr.Attack2 = 0;
            //owner.Attr.Attack3 = 100;
            //owner.AngryValue = 100;
            if (attack < owner.Attr.Attack1)
            {
                //普通攻击
                //Debug.LogError("try attack 1");
                ActionNode attack1 = ActionInterrupt.Instance.GetNormalNode(owner, act);
                if (attack1 != null)
                {
                    TryPlayWeaponPose(attack1.KeyMap);
                }
            }
            else if (attack < (owner.Attr.Attack1 + owner.Attr.Attack2))
            {
                //Debug.LogError("try attack2");
                //连招
                List<ActionNode> attack2 = ActionInterrupt.Instance.GetSlashNode(owner, act);
                if (attack2.Count != 0)
                {
                    TryPlayWeaponPose(attack2[Random.Range(0, attack2.Count)].KeyMap);
                }
            }
            else if (attack < (owner.Attr.Attack1 + owner.Attr.Attack2 + owner.Attr.Attack3))
            {
                //绝招
                //Debug.LogError("try attack3");
                ActionNode attack3 = ActionInterrupt.Instance.GetSkillNode(owner, act);
                if (attack3 != null)
                {
                    TryPlayWeaponPose(attack3.KeyMap);
                }
            }
        }
    }

    //攻击目标位置
    public bool OnAttackTarget()
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
                        return false;
                    }
                    else
                    {
                        curIndex = targetIndex;
                        targetIndex += 1;
                    }
                    RotateRound = Random.Range(1, 3);
                    SubStatus = EAISubStatus.AttackTargetSubRotateToTarget;//到指定地点后旋转到目标.
                    return false;
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
                        return false;
                    }
                    //尝试几率跳跃，否则可能会被卡住.
                    int random = Random.Range(0, 100);
                    if (random < owner.Attr.Jump && AIJumpDelay >= 2.5f)
                    {
                        AIJump();
                        AIJumpDelay = 0.0f;
                        return false;
                    }
                };
                break;
            case EAISubStatus.AttackTarget:
                if (AttackCount != 0)
                {
                    if (AttackTargetCoroutine == null)
                    {
                        owner.FaceToTarget(AttackTarget);
                        AttackTargetCoroutine = owner.StartCoroutine(Attack());
                        AttackCount -= 1;
                        if (AttackCount == 0)
                        {
                            ChangeState(EAIStatus.Wait);
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            case EAISubStatus.AttackTargetSubRotateToTarget:
                if (AttackRotateToTargetCoroutine == null)
                    AttackRotateToTargetCoroutine = owner.StartCoroutine(AttackRotateToTarget(Path[targetIndex].pos, EAIStatus.Wait, EAISubStatus.AttackGotoTarget, false, true));
                break;
        }
        return false;
    }

    //地面攻击.
    float ThinkCheckTick = 0.0f;
    void FightOnGround()
    {
        switch (SubStatus)
        {
            case EAISubStatus.Fight:
                OnFight();
                break;
            case EAISubStatus.FightAim:
                OnFightAim();//当前状态是瞄准.
                break;
            case EAISubStatus.FightLeave:
                OnFightLeave();
                break;
            case EAISubStatus.FightGotoTarget:
                OnFightGotoTarget();
                break;
            case EAISubStatus.FightGotoPosition:
                OnFightGotoPosition();
                break;
            case EAISubStatus.FightSubRotateToTarget:
                OnFightSubRotateToTarget();
                break;
            //case EAISubStatus.FightNear:
            //    OnFightNear();
            //    break;
            case EAISubStatus.FightNearTarget:
                OnFightNearTarget();
                break;
        }
    }

    void OnFightNearTarget()
    {
        if (Time.realtimeSinceStartup - statusTick >= 3.0f)
        {
            Stop();
            Status = EAIStatus.Fight;
            SubStatus = EAISubStatus.Fight;
            return;
        }

        owner.FaceToTarget(fightTarget);
        owner.controller.Input.AIMove(0, 1);
        float dis = Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(fightTarget.mPos.x, 0, fightTarget.mPos.z));
        float disMin = 30;
        if (dis <= disMin || U3D.IsSpecialWeapon(owner.Attr.Weapon))
        {
            Stop();
            Status = EAIStatus.Fight;
            SubStatus = EAISubStatus.Fight;
        }
    }

    //在最后一段直线距离内，直接走过去即可
    void OnFightNear()
    {
        owner.controller.Input.AIMove(0, 1);
        float dis = Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), TargetPos);
        float disMin = owner.Speed * Time.deltaTime * 0.15f;
        if (dis <= disMin)
        {
            Stop();
            Status = EAIStatus.Fight;
            SubStatus = EAISubStatus.Fight;
        }
    }

    void OnFightSubRotateToTarget()
    {
        if (FollowRotateToTargetCoroutine == null && FollowPath.Count > targetIndex)
        {
            FollowRotateToTargetCoroutine = owner.StartCoroutine(FollowRotateToTarget(FollowPath[targetIndex].pos, EAISubStatus.FightGotoPosition));
        }
    }

    float gotoPositionTick = 0.0f;
    Vector3 lastPos = Vector3.zero;
    void OnFightGotoPosition()
    {
        if (Time.realtimeSinceStartup > gotoPositionTick)
        {
            gotoPositionTick = Time.realtimeSinceStartup + 3.0f;
            FollowPath = RefreshPath(owner.mPos, TargetPos);
        }

        if (curIndex == -1)
            targetIndex = 0;

        if (targetIndex >= FollowPath.Count)
        {
            UnityEngine.Debug.LogError(string.Format("targetIndex:{0}, FollowPath:{1}", targetIndex, FollowPath.Count));
            Debug.DebugBreak();
        }

        Assert.IsTrue(targetIndex < FollowPath.Count);
        float dis = Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(FollowPath[targetIndex].pos.x, 0, FollowPath[targetIndex].pos.z));
        float disMin = owner.Speed * Time.deltaTime * 0.15f;
        if (lastPos != Vector3.zero && Vector3.Dot(new Vector3(owner.mPos.x, 0, owner.mPos.z) - new Vector3(lastPos.x, 0, lastPos.z), new Vector3(FollowPath[targetIndex].pos.x, 0, FollowPath[targetIndex].pos.z) - new Vector3(owner.mPos.x, 0, owner.mPos.z)) < 0)
        {
            Debug.DebugBreak();
        }

        if (dis <= disMin)
        {
            //Debug.Log(string.Format("dis:{0} dis min:{1}", dis, disMin));
            owner.controller.Input.AIMove(0, 0);
            if (targetIndex == FollowPath.Count - 1)
            {
                Status = EAIStatus.Fight;
                SubStatus = EAISubStatus.Fight;
                return;
            }
            else
            {
                curIndex = targetIndex;
                targetIndex += 1;
            }
            RotateRound = Random.Range(1, 3);
            SubStatus = EAISubStatus.FightSubRotateToTarget;//到指定地点后旋转到目标.
            return;
        }

        lastPos = owner.mPos;
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
            //尝试几率跳跃，否则可能会被卡住.
            int random = Random.Range(0, 100);
            if (random < owner.Attr.Jump && AIJumpDelay >= 2.5f)
            {
                AIJump();
                AIJumpDelay = 0.0f;
            }
        }
    }

    float statusTick = 0.0f;
    void OnFightGotoTarget()
    {
        TargetPos = fightTarget.mPos;
        if (Time.realtimeSinceStartup > gotoPositionTick)
        {
            gotoPositionTick = Time.realtimeSinceStartup + 3.0f;
            FollowPath = RefreshPath(owner.mPos, TargetPos);
        }

        if (curIndex == -1)
            targetIndex = 0;

        if (targetIndex >= FollowPath.Count)
        {
            UnityEngine.Debug.LogError(string.Format("targetIndex:{0}, FollowPath:{1}", targetIndex, FollowPath.Count));
            Debug.DebugBreak();
        }

        Assert.IsTrue(targetIndex < FollowPath.Count);
        float dis = Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(FollowPath[targetIndex].pos.x, 0, FollowPath[targetIndex].pos.z));
        float disMin = owner.Speed * Time.deltaTime * 0.15f;
        if (dis <= disMin)
        {
            owner.controller.Input.AIMove(0, 0);
            if (targetIndex == FollowPath.Count - 1)
            {
                //到达终点.
                owner.controller.Input.AIMove(0, 0);
                SubStatus = EAISubStatus.FightNearTarget;//要先转向面向攻击目标.
                statusTick = Time.realtimeSinceStartup;
                return;
            }
            else
            {
                curIndex = targetIndex;
                targetIndex += 1;
            }
            RotateRound = Random.Range(1, 3);
            SubStatus = EAISubStatus.FightSubRotateToTarget;//到指定地点后旋转到目标.
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
            //尝试几率跳跃，否则可能会被卡住.
            int random = Random.Range(0, 100);
            if (random < owner.Attr.Jump && AIJumpDelay >= 2.5f)
            {
                AIJump();
                AIJumpDelay = 0.0f;
            }
        }
    }

    void OnFight()
    {
        //如果处于跌倒状态.A处理从地面站立,僵直过后才能正常站立 B，在后面的逻辑中，决定是否用爆气解除跌倒状态
        if (owner.posMng.mActiveAction.Idx == CommonAction.Struggle || owner.posMng.mActiveAction.Idx == CommonAction.Struggle0)
        {
            if (struggleCoroutine == null && !owner.charLoader.InStraight)
            {
                struggleCoroutine = owner.StartCoroutine(ProcessStruggle());
                return;
            }
        }

        //有任意输入时，不要进入攻击状态，否则状态切换可能过快.
        if (PlayWeaponPoseCorout != null || InputCorout != null)
            return;

        if (owner.controller.Input.AcceptInput())
        {
            if (owner.posMng.IsAttackPose())
            {
                //在攻击姿势内可连击，不许转动方向
                if (PlayWeaponPoseCorout == null)
                {
                    //检查几率，0.5几率连招
                    //如果是远程武器，先检查角度是否相差过大.过大先调整角度
                    if (U3D.IsSpecialWeapon(owner.Attr.Weapon))
                    {
                        if (GetAngleBetween(fightTarget.mPos) >= 30)
                        {
                            //停止连击，方向需要调整
                            return;
                        }
                        
                    }
                    //近战武器，2%几率接前一招出招
                    float chance = Random.Range(1, 100);
                    if (chance <= 2)
                    {
                        //UnityEngine.Debug.LogError(string.Format("{0}:chance attack", chance));
                        TryAttack();
                    }
                    else
                    {
                        //Status = EAIStatus.Fight;
                        //SubStatus = EAISubStatus.FightAim;
                        return;
                    }
                }
                else
                {
                    //输出中不许切换状态.
                }
            }
            else if (owner.posMng.IsHurtPose())
            {
                //无输入状态时.
                if (InputCorout == null)
                {
                    //不响应输入.指一个动作正在前摇或后摇，只能响应曝气,曝气只有
                    //如果可以曝气
                    if (owner.CanBurst())
                    {
                        int breakOut = Random.Range(0, 100);
                        //20时，就为20几率，0-19 共20
                        if (breakOut <= Global.BreakChange)
                            InputCorout = owner.StartCoroutine(VirtualKeyEvent(EKeyList.KL_BreakOut));
                    }
                }
            }
            else
            {
                //打印当前动作是啥，在多少帧-帧之间，是否能正常过渡到攻击动作.
                //Debug.LogError(string.Format("pos:{0} frame:{1}", owner.posMng.mActiveAction.Idx, owner.charLoader.GetCurrentFrameIndex()));

                //这些动作能不能切换到目标姿势.???

                //在一些动作姿势里，走，跑，跳 等
                //if ()//在一些特殊姿势里，比如落地，等待飞轮回来，火枪上子弹。
                //特殊应对，对距离的监测，
                //触发近战状态下  跑到远处去使用远程武器打架
                //或者远程状态下，使用近战武器跑近身打架
                //或者远程状态下, 切换远程武器打架
                //使用计时器，如果未到下一个不判断
                if (Time.realtimeSinceStartup > ThinkCheckTick)
                {
                    //更新状态.
                    ThinkCheckTick = Time.realtimeSinceStartup + owner.Attr.Think / 100.0f;
                    if (fightTarget == null)
                        Debug.LogError("fightTarget == null");
                    float dis = Vector3.Distance(owner.mPos, fightTarget.mPos);
                    //距离战斗目标不同，选择不同方式应对.
                    if (dis >= Global.AttackRange)
                    {
                        //大部分几率是跑过去，走到跟前对打。
                        //小部分几率是切换远程武器打目标。
                        //1是否拥有远程武器
                        if (U3D.IsSpecialWeapon(owner.Attr.Weapon))
                        {
                            //主手是远程武器-.直接攻击,不处理，后续会处理到底是打还是啥.
                            if (owner.Attr.Weapon2 == 0 || U3D.IsSpecialWeapon(owner.Attr.Weapon2))
                            {
                                //站撸，因为只有远程武器？这个状态可能是丢掉武器发生的，应该算一个错误状态
                                SubStatus = EAISubStatus.FightAim;
                                return;
                            }

                            int random = Random.Range(0, 100);
                            if (random > 10)
                            {
                                //直接使用当前武器开打，需要瞄准.
                                SubStatus = EAISubStatus.FightAim;//监测
                                return;
                            }
                            else
                            {
                                //切换武器，跑过去，开打,换到武器2
                                owner.ChangeWeapon();
                                return;
                            }
                        }
                        else if (U3D.IsSpecialWeapon(owner.Attr.Weapon2))
                        {
                            //副手是远程武器-.一定几率切换武器，再攻击.
                            int random = Random.Range(0, 100);
                            if (random > 10)
                            {
                                //切换武器，开打(不跑过去),换到武器2
                                //SubStatus = EAISubStatus.FightChangeWeapon;
                                owner.ChangeWeapon();
                                return;
                            }
                            else
                            {
                                //直接使用当前武器开打，跑过去，需要面对.
                                SubStatus = EAISubStatus.FightGotoTarget;
                                return;
                            }
                        }
                        else
                        {
                            //双手全近战武器.在视野内 攻击距离外则跑近到攻击距离
                            SubStatus = (EAISubStatus.FightGotoTarget);
                            return;
                        }

                    }
                    else//在攻击范围内.
                    {
                        //检查有没有远程武器.
                        //是否跑远使用远程武器.
                        bool useSpecialWeapon = false;
                        bool changeWeapon = false;
                        if (U3D.IsSpecialWeapon(owner.Attr.Weapon))
                        {
                            useSpecialWeapon = true;
                            changeWeapon = false;
                        }
                        else if (U3D.IsSpecialWeapon(owner.Attr.Weapon2))
                        {
                            useSpecialWeapon = true;
                            changeWeapon = true;
                        }
                        else
                        {
                            //全近战武器，不使用远程武器，也不需要跑远，也不需要切换武器
                        }

                        if (useSpecialWeapon)
                        {
                            int random = Random.Range(0, 100);
                            int limit = (int)(Global.AttackRange - (dis - 18));
                            if (random < limit)
                            {
                                SubStatus = (EAISubStatus.FightLeave);
                                return;
                            }
                            if (changeWeapon)
                            {
                                //SubStatus = (EAISubStatus.FightChangeWeapon);
                                owner.ChangeWeapon();
                                return;
                            }
                        }
                    }
                }

                //使用近战武器，一定在攻击范围内。
                int attack = Random.Range(0, 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look + owner.Attr.Burst + owner.Attr.GetItem);
                if (attack <= owner.Attr.Attack1 + owner.Attr.Attack2 + owner.Attr.Attack3)
                {
                    //先判断是否面朝目标，不是则先转动方向朝向目标
                    if (fightTarget == null)
                        Debug.LogError("fightTarget == null");
                    if (GetAngleBetween(fightTarget.mPos) >= 30)
                    {
                        Stop();
                        Status = EAIStatus.Fight;
                        SubStatus = EAISubStatus.FightAim;
                        return;
                    }
                    TryAttack(attack);
                }
                else
                {
                    if (attack > 100 && attack < 100 + owner.Attr.Dodge)
                    {
                        //逃跑，在危险状态下
                        if (owner.InDanger())
                        {
                            LeaveReset();
                            ChangeState(EAIStatus.Dodge);
                            
                        }
                    }
                    else
                    if (JumpCoroutine == null && attack > 100 + owner.Attr.Dodge && attack < 100 + owner.Attr.Dodge + owner.Attr.Jump)
                    {
                        //最好不要带方向跳跃，否则可能跳到障碍物外被场景卡住
                        Stop();
                        AIJump();
                    }
                    else if (!U3D.IsSpecialWeapon(owner.Attr.Weapon) && (attack > 100 + owner.Attr.Dodge + owner.Attr.Jump && attack < 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard))
                    {
                        //远程武器无法防御.
                        Stop();
                        owner.Guard(true);
                    }
                    else if (attack > 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look && attack < 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look + owner.Attr.Burst)
                    {
                        //快速移动.
                        AIBurst((EKeyList.KL_KeyA) + attack % 4);
                    }//不判断拾取物品，拾取物品发生在物品出现在视野内时
                    else if (attack > 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look + owner.Attr.Burst && attack < 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look + owner.Attr.Burst + owner.Attr.GetItem)
                    {
                        //去捡包子.
                        if (owner.GetSceneItemTarget() != null && owner.GetSceneItemTarget().CanPickup())
                            ChangeState(EAIStatus.GetItem);
                    }
                    else
                    {

                    }
                }
                return;
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
            //不接受输入，在动作前摇或后摇,且无法取消
        }
    }

    //进入这个状态前，需要检查当前能否旋转.
    void OnFightAim()
    {
        //看是否面向目标，在夹角大于30度时，触发旋转面向目标
        if (fightTarget == null)
        {
            Status = EAIStatus.Wait;
            SubStatus = EAISubStatus.SubStatusWait;
            return;
        }

        if (GetAngleBetween(fightTarget.mPos) >= 30.0f)
        {
            if (AttackRotateToTargetCoroutine == null)
            {
                owner.posMng.ChangeAction(0);
                AttackRotateToTargetCoroutine = owner.StartCoroutine(AttackRotateToTarget(fightTarget.mPos, EAIStatus.Fight, EAISubStatus.Fight, false, true));
            }
        }
        else
        {
            if (AttackRotateToTargetCoroutine == null)
            {
                Status = EAIStatus.Fight;
                SubStatus = EAISubStatus.Fight;
            }
        }
    }

    //离开战斗目标。可能是拿的远程武器，也可能是状态不好要逃跑.
    void OnFightLeave()
    {
        if (fightTarget == null)
        {
            Status = EAIStatus.Wait;
            SubStatus = EAISubStatus.SubStatusWait;
            return;
        }

        float dis = Vector3.Distance(owner.mPos, fightTarget.mPos);
        if (dis >= 75.0f)
        {
            Status = EAIStatus.Fight;
            SubStatus = EAISubStatus.Fight;
            return;
        }

        //离开攻击目标
        Vector3 vec = owner.mPos - fightTarget.mPos;
        if ((int)vec.x == 0 && (int)vec.z == 0)
            vec = fightTarget.transform.forward;
        vec.y = 0;
        if (AttackRotateToTargetCoroutine == null)
        {
            //Debug.LogError("fight leave");
            Stop();
            TargetPos = fightTarget.mPos + vec.normalized * 125.0f;
            //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //go.transform.localScale = Vector3.one * 10;
            //go.transform.position = TargetPos;
            owner.posMng.ChangeAction(0);
            AttackRotateToTargetCoroutine = owner.StartCoroutine(AttackRotateToTarget(TargetPos, EAIStatus.Fight, EAISubStatus.FightGotoPosition, false, true));
        }
    }

    List<WayPoint> Path = new List<WayPoint>();//这个是攻击指定位置时的寻路，这个是不会随着角色移动改变位置的.
    List<WayPoint> RefreshPath(Vector3 now, Vector3 target)
    {
        List<WayPoint> ret = PathMng.Instance.FindPath(now, target);
        if (Path.Count == 0)
        {
            Debug.DebugBreak();
        }
        curIndex = -1;
        targetIndex = -1;
        return ret;
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

    void OnKillThink()
    {
        if (killTarget == null || killTarget.Dead)
        {
            ChangeState(EAIStatus.Wait);
            return;
        }

        Vector3 vec = killTarget.mPos;
        vec.y = 0;
        Vector3 vec2 = owner.mPos;
        vec2.y = 0;
        if (Vector3.Distance(killTarget.mPos, owner.mPos) >= Global.AttackRange)
        {
            SubStatus = EAISubStatus.KillGotoTarget;
            return;
        }

        SubStatus = EAISubStatus.KillGetTarget;
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
                        //Debug.Log("stop follow until 35 meters");
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
                        //尝试几率跳跃，否则可能会被卡住.
                        int random = Random.Range(0, 100);
                        if (random < owner.Attr.Jump && AIJumpDelay >= 2.5f)
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
                    FollowRotateToTargetCoroutine = owner.StartCoroutine(FollowRotateToTarget(FollowPath[targetIndex].pos, EAISubStatus.FollowGotoTarget));
                }
                break;
        }
    }

    //已经距离该角色很近了.开始打架
    public void KillTarget(MeteorUnit target)
    {
        owner.SetLockedTarget(target);
        fightTarget = target;
        ChangeState(EAIStatus.Fight);
        SubStatus = EAISubStatus.Fight;
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

    IEnumerator ProcessStruggle()
    {
        //Debug.Log("ProcessStruggle");
        //或者按方向滚动或者跳跃（大小）。或者按防御起身。
        yield return 0;
        while (true)
        {
            yield return 0;
            yield return 0;
            owner.controller.Input.OnKeyDown(EKeyList.KL_Defence, true);
            yield return 0;
            yield return 0;
            owner.controller.Input.OnKeyUp(EKeyList.KL_Defence);
            break;
        }
        struggleCoroutine = null;
        SubStatus = EAISubStatus.Fight;
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
    float idleTick = 0.0f;
    float waitCrouch;
    float waitDefence;
    float lookTick = 0.0f;

    //原地不动-
    void OnWait()
    {
        int lookChance = Random.Range(0, 100);
        if (Time.realtimeSinceStartup - lookTick >= 5.0f && (lookChance > 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard && lookChance < 100 + owner.Attr.Dodge + owner.Attr.Jump + owner.Attr.Guard + owner.Attr.Look))
        {
            lookTick = Time.realtimeSinceStartup;//5秒内最多能进行一次四处看.
            ChangeState(EAIStatus.Look);
        }
    }

    //空闲动画
    void OnIdle()
    {
        if (idleTick >= 0.2f)
        {
            ChangeState(EAIStatus.Wait);
            idleTick = 0.0f;
            return;
        }
        idleTick += Time.deltaTime;
    }

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
        if (paused)
        {
            Stop();
            StopCoroutine();
        }
        //Debug.Log(string.Format("unit:{0} pause:{1}", owner.name, pause_tick));
    }

    public void EnableAI(bool enable)
    {
        stoped = !enable;
        if (stoped)
        {
            Stop();
            StopCoroutine();
        }
    }

    public void ChangeState(EAIStatus type)
    {
        Status = type;
        StopCoroutine();
        Stop();
        ResetAIKey();
        if (type == EAIStatus.Kill)
        {
            //Debug.Log("changestatus:kill");
            SubStatus = EAISubStatus.KillThink;
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
            Path = RefreshPath(owner.mPos, AttackTarget);
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

        //普通控制输入.
        if (InputCorout != null)
        {
            owner.StopCoroutine(InputCorout);
            InputCorout = null;
        }

        //攻击招式输入.
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

        if (LookRotateToTargetCoroutine != null)
        {
            owner.StopCoroutine(LookRotateToTargetCoroutine);
            LookRotateToTargetCoroutine = null;
        }

        if (struggleCoroutine != null)
        {
            owner.StopCoroutine(struggleCoroutine);
            struggleCoroutine = null;
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
        Stop();
        ResetAIKey();
        StopCoroutine();
        owner.controller.Input.ResetInput();
        if (attacker != null)
        {
            Status = EAIStatus.Fight;
            SubStatus = EAISubStatus.Fight;
            fightTarget = attacker;
        }
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
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / 150.0f;
        float timeTick = 0.0f;
        
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += Time.deltaTime;
            if (rightRotate)
                yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            else
                yOffset = -Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
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
    IEnumerator AttackRotateToTarget(Vector3 vec, EAIStatus status, EAISubStatus subStatus, bool setStatus, bool setSubStatus)
    {
        Vector3 diff = (vec - owner.mPos);
        diff.y = 0;
        float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        float angle = Mathf.Abs(Mathf.Acos(dot) * Mathf.Rad2Deg);
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / 150.0f;
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += Time.deltaTime;
            if (rightRotate)
                yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            else
                yOffset = -Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
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
        if (setSubStatus)
            SubStatus = subStatus;
        if (setStatus)
            Status = status;
        OnStateChange(Status, SubStatus);
    }

    void OnStateChange(EAIStatus status, EAISubStatus substatus)
    {
        if (substatus == EAISubStatus.FightGotoPosition)
            lastPos = Vector3.zero;
    }

    Coroutine FollowRotateToTargetCoroutine;
    IEnumerator FollowRotateToTarget(Vector3 vec, EAISubStatus onFinishSubStatus)
    {
        Vector3 diff = (vec - owner.mPos);
        diff.y = 0;
        float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        float angle = Mathf.Abs(Mathf.Acos(dot) * Mathf.Rad2Deg);
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / 150.0f;
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += Time.deltaTime;
            if (rightRotate)
                yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            else
                yOffset = -Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
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
        SubStatus = onFinishSubStatus;
        //SubStatus = EAISubStatus.FollowGotoTarget;
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
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / 150.0f;
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += Time.deltaTime;
            if (rightRotate)
                yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            else
                yOffset = -Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
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
                            //尝试几率跳跃，否则可能会被卡住.
                            int random = Random.Range(0, 100);
                            if (random < owner.Attr.Jump && AIJumpDelay >= 2.5f)
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
                {
                    if (PatrolPath.Count == 0)
                    {
                        ChangeState(EAIStatus.Wait);
                        SubStatus = EAISubStatus.SubStatusWait;
                        return;
                    }
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

                            //尝试几率跳跃，否则可能会被卡住.
                            int random = Random.Range(0, 100);
                            if (random < owner.Attr.Jump && AIJumpDelay >= 2.5f)
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

    
    //指定角色攻击某个位置，例如一线天山顶的大石头.
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

                    if (idx == curPatrolIndex)
                        return;
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

                    if (idx == curPatrolIndex)
                        return;
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