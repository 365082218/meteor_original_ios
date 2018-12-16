using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Idevgame.Util;

public enum EAIStatus
{
    Idle,//不动.
    Fight,//包括所有战斗情况
    Kill,//强制杀死指定敌人，无视距离，一直跟随
    Guard,//防御
    Follow,//跟随
    Think,//没发觉目标时左右观察
    Patrol,//巡逻。
    Wait,//，类似于Search
    Dodge,//逃跑
    Look,//四处看
    GetItem,//取得场景道具-最近的，可拾取的(未打碎的箱子不算在内)。
    AttackTarget,//攻击指定位置
}

public enum EAISubStatus
{
    FightGotoTarget,//在打斗前，必须走到目标范围附近,就是朝目标移动
    FightGotoPosition,//朝目的地移动，需要寻路.
    FightSubRotateToTarget,//走到路点后，旋转朝向下一个路点.
    FightSubRotateToPosition,//朝向下一个位置或路点
    FightSubLeavetoTarget,//离开、逃跑.

    Fight,//实际出招/向其他状态切换，
    FightAim,//战斗中若使用远程武器，瞄准敌方的几率，否则就随机
    //FightChangeWeapon,//战斗中切换武器->可能会跑近（使用近战武器），或跑远（使用远程武器）
    FightLeave,//跑远
    FightGetItem,//战斗中去捡起物品-镖物-补给-武器-BUFF道具
    SubStatusIdle,
    SubStatusWait,
    FollowGotoTarget,
    FollowSubRotateToTarget,
    Patrol,
    PatrolGotoFirstPoint,//跑到第一个寻路点
    PatrolSubInPlace,//原地
    PatrolSubRotateInPlace,//原地随机旋转.
    PatrolSubRotateToTarget,//原地一定时间内旋转到指定方向
    PatrolSubRotateToPatrolPoint,
    PatrolSubGotoTarget,//跑向指定位置
    KillThink,//思考是走近还是切换状态
    KillGotoTarget,//离角色一定距离，需要先跑过去
    KillSubRotateToTarget,
    AttackGotoTarget,//攻击指定位置-寻路中.
    AttackTarget,//攻击指定位置-攻击中
    AttackTargetSubRotateToTarget,//攻击指定位置，到达中间某个寻路点时.
    GetItemGotoItem,//朝物件走的路线中
    GetItemSubRotateToItem,//到达中间某个寻路点，朝向下个寻路点
}

//寻路流程，得到自己和目标所在路点，取得路点序列，向下一个路点进发，到达最后一个路点->往目的进发->到达目的，结束.
//比较麻烦的是，距离下个路点的判定不能仅使用距离，因为若一帧内直接超过下个路点，那么又会反过来走.而永远无法到达目的路点.
//所以这里使用当前位置，目标位置(或下个路点位置)，用2个向量的夹角是逆向还是同向决定，是否已经到达下个路点了。

public class MeteorAI {
    public MeteorAI(MeteorUnit user)
    {
        owner = user;
        Status = EAIStatus.Idle;
        SubStatus = EAISubStatus.SubStatusIdle;
        int count = Global.GLevelItem.wayPoint.Count;
        nodeContainer = new PathNode[count];
        ThinkCheckTick = MeteorAI.ThinkDelay;
        for (int i = 0; i < count; i++)
            nodeContainer[i] = new PathNode();
    }
    //寻路相关数据
    public SortedDictionary<int, List<PathNode>> PathInfo = new SortedDictionary<int, List<PathNode>>();
    public PathNode[] nodeContainer;
    public List<WayPoint> Path = new List<WayPoint>();//存储寻路找到的路线点

    public EAIStatus Status { get; set; }
    public EAISubStatus SubStatus { get; set; }
    float FollowRefreshTick = 0.0f;
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
    static readonly float ThinkDelay = 200;//思考延迟，每一帧这个延迟都会被配置里的Think减去一个数，减少到0时，就触发一次行为，否则返回
    // Update is called once per frame
    public void Update () {
        ThinkCheckTick -= owner.Attr.Think;//
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

        ChangeWeaponTick -= Time.deltaTime;
        AIJumpDelay += Time.deltaTime;
        lookTick += Time.deltaTime;
        FollowRefreshTick -= Time.deltaTime;

        //如果在硬直中
        if (owner.charLoader.IsInStraight())
            return;
        //如果处于跌倒状态.A处理从地面站立,僵直过后才能正常站立 B，在后面的逻辑中，决定是否用爆气解除跌倒状态
        if (owner.posMng.mActiveAction.Idx == CommonAction.Struggle || owner.posMng.mActiveAction.Idx == CommonAction.Struggle0)
        {
            if (struggleCoroutine == null)
                struggleCoroutine = owner.StartCoroutine(ProcessStruggle());
            return;
        }

        //行为优先级 
        //AI强制行为(攻击指定位置，Kill追杀（不论视野）攻击 ) > 战斗 > 跟随 > 巡逻 > 
        if (Status == EAIStatus.Patrol)
        {
            //巡逻状态如果找到了敌人，与敌人搏斗.
            if (killTarget != null)
                ChangeState(EAIStatus.Kill);
            else
            if (owner.GetLockedTarget() != null)
            {
                //Debug.LogError("locked target is not null change to wait");
                ChangeState(EAIStatus.Wait);
            }
            else
            if (followTarget != null)
                ChangeState(EAIStatus.Follow);
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
                    float dis = Vector3.SqrMagnitude(owner.mSkeletonPivot - followTarget.mSkeletonPivot);
                    if (dis >= Global.FollowDistanceStart)//距离65码开始跟随
                        ChangeState(EAIStatus.Follow);
                    else
                    {
                        ChangeState(EAIStatus.Wait);
                        SubStatus = EAISubStatus.SubStatusWait;
                    }
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
                fightTarget = owner.GetLockedTarget();
                ChangeState(EAIStatus.Fight);
                return;
            }

            if (followTarget != null)
            {
                float dis = Vector3.SqrMagnitude(owner.mSkeletonPivot - followTarget.mSkeletonPivot);
                if (dis >= Global.FollowDistanceStart)//距离65码开始跟随
                {
                    ChangeState(EAIStatus.Follow);
                    return;
                }
            }

            //如果巡逻数据不为空，再次进入巡逻状态.
            if (patrolData != null && patrolData.Count != 0)
            {
                //Debug.LogError("enter patrol");
                ChangeState(EAIStatus.Patrol);
                return;
            }
        }

        switch (Status)
        {
            case EAIStatus.Fight:
                OnFightStatus();
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
            case EAIStatus.Patrol:
                OnPatrol();
                break;
            case EAIStatus.Follow:
                OnFollow();
                break;
            case EAIStatus.Kill:
                OnKill();
                break;
        }


    }

    //逃跑
    void OnDodge()
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

    public void OnChangeWeapon()
    {
        
    }

    //捡取场景道具
    void OnGetItem()
    {
        switch (SubStatus)
        {
            case EAISubStatus.GetItemGotoItem:
                if (owner.GetSceneItemTarget() == null)
                {
                    Stop();
                    ChangeState(EAIStatus.Wait);
                    return;
                }

                if (Path.Count == 0)
                {
                    if (owner.GetSceneItemTarget() != null)
                        RefreshPath(owner.mSkeletonPivot, owner.GetSceneItemTarget().transform.position);
                }

                if (curIndex == -1)
                    targetIndex = 0;

                if (targetIndex < Path.Count)
                {
                    NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
                    {
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                        float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(Path[targetIndex].pos - NextFramePos));
                        //反向
                        if (s < 0)
                        {
                            owner.controller.Input.AIMove(0, 0);
                            curIndex = targetIndex;
                            targetIndex += 1;
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.GetItemSubRotateToItem;//到指定地点后旋转到目标.
                            return;
                        }
                    }
                    owner.FaceToTarget(Path[targetIndex].pos);
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
                        if (AIJumpDelay >= 2.5f && random < owner.Attr.Jump)
                        {
                            AIJump();
                            AIJumpDelay = 0.0f;
                        }
                    }
                }
                else
                {
                    NextFramePos = owner.GetSceneItemTarget().transform.position - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
                    {
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                        float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(owner.GetSceneItemTarget().transform.position - NextFramePos));
                        //反向
                        if (s < 0)
                        {
                            owner.controller.Input.AIMove(0, 0);
                            ChangeState(EAIStatus.Wait);
                            return;
                        }
                    }
                    owner.FaceToTarget(owner.GetSceneItemTarget().transform.position);
                    owner.controller.Input.AIMove(0, 1);
                }
                break;
            case EAISubStatus.GetItemSubRotateToItem:
                if (GetItemRotateToTargetCoroutine == null)
                {
                    if (owner.GetSceneItemTarget() == null)
                    {
                        owner.controller.Input.AIMove(0, 0);
                        ChangeState(EAIStatus.Wait);
                        return;
                    }
                    Vector3 targetPosition = Vector3.zero;
                    if (targetIndex >= Path.Count)
                        targetPosition = owner.GetSceneItemTarget().transform.position;
                    else if (targetIndex >= 0)
                        targetPosition = Path[targetIndex].pos;
                    else
                        return;
                    GetItemRotateToTargetCoroutine = owner.StartCoroutine(GetItemRotateToTarget(targetPosition));
                }
                break;
        }
    }

    void OnGuard()
    {
        if (waitDefence <= 0.0f)
        {
            int random = Random.Range(0, 100);
            owner.Guard(random < owner.Attr.Guard);
        }
        waitDefence -= Time.deltaTime;
    }

    //四处看.
    Coroutine LookRotateToTargetCoroutine;
    void OnLook()
    {
        if (owner.GetLockedTarget() != null && LookRotateToTargetCoroutine == null)
        {
            Stop();
            ChangeState(EAIStatus.Wait);
            return;
        }
        if (LookRotateToTargetCoroutine == null && owner.GetLockedTarget() == null)
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
        ChangeState(EAIStatus.Wait);
    }

    #region 输入
    Coroutine InputCorout;//普通的单个键的
    #endregion

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
            attack = Random.Range(0, owner.Attr.Attack1 + owner.Attr.Attack2 + owner.Attr.Attack3);
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
        else
        {
            if (owner.posMng.mActiveAction.Idx == CommonAction.GunIdle)
            {
                //枪只有 A， 上A， 下A， 下上A， 下上上A
                ////Debug.LogError("shoot");
            }
        }
    }

    //攻击目标位置
    public bool OnAttackTarget()
    {
        switch (SubStatus)
        {
            case EAISubStatus.AttackGotoTarget:
                if (Path.Count == 0)
                    RefreshPath(owner.mSkeletonPivot, AttackTarget);

                if (curIndex == -1)
                    targetIndex = 0;

                //已经到达过最后一个寻路点.
                if (targetIndex >= Path.Count)
                {
                    //检查这一帧是否会走过目标，因为跨步太大.【这一段有问题，只有离目标非常近的时候再判断才行，远的话，可能会绕路，导致下一帧距离目标的位置越来越远】
                    NextFramePos = AttackTarget - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    //33码距离内.
                    if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
                    {
                        NextFramePos.y = 0;
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                        float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(AttackTarget - NextFramePos));
                        if (s < 0)
                        {
                            //不在寻路点上，说明已经到达终点
                            owner.controller.Input.AIMove(0, 0);
                            SubStatus = EAISubStatus.AttackTarget;
                            return false;
                        }
                    }
                    owner.FaceToTarget(AttackTarget);
                    owner.controller.Input.AIMove(0, 1);
                }
                else
                {
                    //检查这一帧是否会走过目标，因为跨步太大.【这一段有问题，只有离目标非常近的时候再判断才行，远的话，可能会绕路，导致下一帧距离目标的位置越来越远】
                    NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    //33码距离内.
                    if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
                    {
                        NextFramePos.y = 0;
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                        float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(Path[targetIndex].pos - NextFramePos));
                        if (s < 0)
                        {
                            owner.controller.Input.AIMove(0, 0);
                            curIndex = targetIndex;
                            targetIndex += 1;
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.AttackTargetSubRotateToTarget;//到指定地点后旋转到目标.
                            return false;
                        }
                    }
                    owner.FaceToTarget(Path[targetIndex].pos);
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
                        if (AIJumpDelay >= 2.5f && random < owner.Attr.Jump)
                        {
                            AIJump();
                            AIJumpDelay = 0.0f;
                            return false;
                        }
                    }
                }
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
                {
                    Vector3 vec = Vector3.zero;
                    if (targetIndex >= Path.Count)
                        vec = AttackTarget;
                    else if (targetIndex >= 0)
                        vec = Path[targetIndex].pos;
                    else
                        return false;
                    AttackRotateToTargetCoroutine = owner.StartCoroutine(AttackRotateToTarget(vec, EAISubStatus.AttackGotoTarget));
                }
                break;
        }
        return false;
    }

    //地面攻击.思考仅处理战斗状态
    float ThinkCheckTick = 0.0f;
    float ChangeWeaponTick = 0.0f;
    void OnFightStatus()
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
            case EAISubStatus.FightSubRotateToPosition:
                OnFightSubRotateToPosition();
                break;
        }
    }

    //在最后一段直线距离内，直接走过去即可
    void OnFightNear()
    {
        owner.controller.Input.AIMove(0, 1);
        float dis = Vector3.Distance(owner.mSkeletonPivot, fightTarget.mSkeletonPivot);
        float disMin = owner.MoveSpeed * Time.deltaTime * 0.15f;
        if (dis <= disMin)
        {
            Stop();
            ChangeState(EAIStatus.Fight);
        }
    }

    void OnFightSubRotateToTarget()
    {
        if (FollowRotateToTargetCoroutine == null)
        {
            Vector3 vec = Vector3.zero;
            if (targetIndex >= Path.Count)
                vec = fightTarget.mSkeletonPivot;
            else if (targetIndex >= 0)
                vec = Path[targetIndex].pos;
            else
                return;
            FollowRotateToTargetCoroutine = owner.StartCoroutine(FollowRotateToTarget(vec, EAISubStatus.FightGotoTarget));
        }
    }

    void OnFightSubRotateToPosition()
    {
        if (FollowRotateToPositionCoroutine == null)
        {
            Vector3 vec = Vector3.zero;
            if (targetIndex >= Path.Count)
                vec = TargetPos;
            else if (targetIndex >= 0)
                vec = Path[targetIndex].pos;
            else
                return;

            FollowRotateToPositionCoroutine = owner.StartCoroutine(FollowRotateToPosition(vec, EAISubStatus.FightGotoPosition));
        }
    }
    //计算出下一帧的位置.
    Vector3 NextFramePos = Vector3.zero;

    void OnFightGotoPosition()
    {
        if (Path.Count == 0)
            RefreshPath(owner.mSkeletonPivot, TargetPos);

        if (curIndex == -1)
            targetIndex = 0;

        if (targetIndex >= Path.Count)
        {
            NextFramePos = TargetPos - owner.mSkeletonPivot;
            NextFramePos.y = 0;
            if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
            {
                NextFramePos.y = 0;
                NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(TargetPos - NextFramePos));
                if (s < 0)
                {
                    //不在寻路点上，说明已经到达终点
                    owner.controller.Input.AIMove(0, 0);
                    ChangeState(EAIStatus.Fight);
                    return;
                }
            }
            owner.FaceToTarget(TargetPos);
            owner.controller.Input.AIMove(0, 1);
        }
        else
        {
            NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
            NextFramePos.y = 0;
            if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
            {
                NextFramePos.y = 0;
                NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(Path[targetIndex].pos - NextFramePos));
                if (s < 0)
                {
                    owner.controller.Input.AIMove(0, 0);
                    curIndex = targetIndex;
                    targetIndex += 1;
                    RotateRound = Random.Range(1, 3);
                    SubStatus = EAISubStatus.FightSubRotateToPosition;//到指定地点后旋转到目标.
                    return;
                }
            }
            owner.FaceToTarget(Path[targetIndex].pos);
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
                if (AIJumpDelay >= 2.5f && random < owner.Attr.Jump)
                {
                    AIJump();
                    AIJumpDelay = 0.0f;
                    return;
                }
            }
        }
    }

    void OnFightGotoTarget()
    {
        if (Path.Count == 0)
            RefreshPath(owner.mPos, fightTarget.mSkeletonPivot);

        if (curIndex == -1)
            targetIndex = 0;

        //已经到达过最后一个寻路点.
        //检查这一帧是否会走过目标，因为跨步太大.【这一段有问题，只有离目标非常近的时候再判断才行，远的话，可能会绕路，导致下一帧距离目标的位置越来越远】

        //仅有2个路点的路径，当
        if (Path.Count == 2)
        {
            //Debug.LogWarning("可以从当前位置直接朝目标移动");
            NextFramePos = fightTarget.mSkeletonPivot - owner.mSkeletonPivot;
            NextFramePos.y = 0;
            //33码距离内.
            if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
            {
                //NextFramePos.y = 0;
                //NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                //float s = GetAngleBetween((NextFramePos - owner.mSkeletonPivot).normalized, (fightTarget.mSkeletonPivot - NextFramePos).normalized);
                //if (s < 0)
                owner.controller.Input.AIMove(0, 0);
                SubStatus = EAISubStatus.Fight;
                return;
            }
            else
            {
                int random = Random.Range(0, 100);
                if (AIJumpDelay >= 2.5f && random <= owner.Attr.Jump)
                {
                    AIJump();
                    AIJumpDelay = 0.0f;
                    return;
                }
            }
        }
        else
        if (targetIndex < Path.Count && Path.Count > 2)
        {
            NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
            NextFramePos.y = 0;
            //35码距离内.
            if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
            {
                NextFramePos.y = 0;
                NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                float s = GetAngleBetween((NextFramePos - owner.mSkeletonPivot).normalized, (Path[targetIndex].pos - NextFramePos).normalized);
                if (s < 0)
                {
                    owner.controller.Input.AIMove(0, 0);
                    curIndex = targetIndex;
                    targetIndex += 1;
                    RotateRound = Random.Range(1, 3);
                    SubStatus = EAISubStatus.FightSubRotateToTarget;//到指定地点后旋转到目标.
                    return;
                }
            }

            //模拟跳跃键，移动到下一个位置.还得按住上
            if (curIndex != -1 && curIndex < Path.Count && targetIndex < Path.Count)
            {
                if (PathMng.Instance.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump && owner.IsOnGround() && AIJumpDelay > 2.5f)
                {
                    AIJump();
                    AIJumpDelay = 0.0f;
                    return;
                }
                //尝试几率跳跃，否则可能会被卡住.
                int random = Random.Range(0, 100);
                if (AIJumpDelay >= 2.5f && random <= owner.Attr.Jump)
                {
                    AIJump();
                    AIJumpDelay = 0.0f;
                    return;
                }
            }
        }
        else if (Path.Count <= 1)
        {
            //直接面向目标,2者处于同一个路点.
            NextFramePos = fightTarget.mSkeletonPivot - owner.mSkeletonPivot;
            NextFramePos.y = 0;
            //33码距离内.
            if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
            {
                //NextFramePos.y = 0;
                //NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                //float s = GetAngleBetween((NextFramePos - owner.mSkeletonPivot).normalized, (fightTarget.mSkeletonPivot - NextFramePos).normalized);
                //if (s < 0)
                owner.controller.Input.AIMove(0, 0);
                SubStatus = EAISubStatus.Fight;
                return;
            }
            else
            {
                int random = Random.Range(0, 100);
                if (AIJumpDelay >= 2.5f && random <= owner.Attr.Jump)
                {
                    AIJump();
                    AIJumpDelay = 0.0f;
                    return;
                }
            }
        }

        if (targetIndex < Path.Count && Path.Count > 2)
            owner.FaceToTarget(Path[targetIndex].pos);
        else
        {
            NextFramePos = fightTarget.mSkeletonPivot - owner.mSkeletonPivot;
            NextFramePos.y = 0;
            //33码距离内.
            if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
            {
                owner.controller.Input.AIMove(0, 0);
                SubStatus = EAISubStatus.Fight;
                return;
            }
            owner.FaceToTarget(fightTarget.mSkeletonPivot);
        }
        owner.controller.Input.AIMove(0, 1);
    }

    void OnFight()
    {
        //有任意输入时，不要进入攻击状态，否则状态切换可能过快.
        if (PlayWeaponPoseCorout != null || InputCorout != null)
            return;

        //攀爬时，可以跳跃-蹬开，但是无法出招
        if (owner.Climbing)
            return;

        //在空中时，如果是由受击姿势，转向落地姿势.
        if (!owner.IsOnGround())
        {
            //能否在空中控制自身的能力.
            if (!owner.CanActionOnAir())
                return;
        }

        if (ThinkCheckTick > 0)
            return;
        ThinkCheckTick = MeteorAI.ThinkDelay;

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
                        if (GetAngleBetween(fightTarget.mPos) >= Global.AimDegree)
                        {
                            //停止连击，方向需要调整
                            return;
                        }
                        
                    }
                    //近战武器，在可输入帧，每一帧30几率接前一招出招，如果这个输入帧大于50帧，基本就可以连上去
                    int chance = Random.Range(0, 100);
                    if (chance < Global.ComboProbability)
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
                        if (breakOut < Global.BreakChange)
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
                if (fightTarget == null)
                {
                    ChangeState(EAIStatus.Wait);
                    return;
                }
                float dis = Vector3.SqrMagnitude(owner.mSkeletonPivot - fightTarget.mSkeletonPivot);
                //距离战斗目标不同，选择不同方式应对.
                if (dis > Global.AttackRange)
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
                            if (GetAngleBetween(fightTarget.mSkeletonPivot) > Global.AimDegree)
                            {
                                SubStatus = EAISubStatus.FightAim;
                                return;
                            }
                        }
                        //else
                        //{
                        //    int random = Random.Range(0, 100);
                        //    if (random >= (100 -  Global.SpecialWeaponProbability))
                        //    {
                        //        //直接使用当前武器开打，如果非面向，需要瞄准.
                        //        if (GetAngleBetween(fightTarget.mSkeletonPivot) > Global.AimDegree)
                        //        {
                        //            SubStatus = EAISubStatus.FightAim;
                        //            return;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        //切换武器，跑过去，开打
                        //        if (ChangeWeaponTick <= 0.0f)
                        //        {
                        //            owner.ChangeWeapon();
                        //            ChangeWeaponTick = 10.0f;
                        //            return;
                        //        }
                        //    }
                        //}
                    }
                    else if (U3D.IsSpecialWeapon(owner.Attr.Weapon2))
                    {
                        //副手是远程武器-.一定几率切换武器，再攻击.90几率切换到远程武器
                        int random = Random.Range(0, 100);
                        if (random > Global.SpecialWeaponProbability)
                        {
                            //切换武器，开打(不跑过去),换到武器2
                            //SubStatus = EAISubStatus.FightChangeWeapon;
                            if (ChangeWeaponTick <= 0.0f)
                            {
                                owner.ChangeWeapon();
                                ChangeWeaponTick = 10.0f;
                                return;
                            }
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
                else
                {
                    //检查有没有远程武器.
                    //是否跑远使用远程武器.
                    if (!U3D.IsSpecialWeapon(owner.Attr.Weapon))//主手不为远程武器
                    {
                        //主手近战武器，副手远程武器
                        //无需处理，使用近战武器攻击就好。
                        if (GetAngleBetween(fightTarget.mSkeletonPivot) > Global.AimDegree)
                        {
                            SubStatus = EAISubStatus.FightAim;
                            return;
                        }
                    }
                    else if (U3D.IsWeapon(owner.Attr.Weapon2) && !U3D.IsSpecialWeapon(owner.Attr.Weapon2))//主手为远程武器，副手有武器且不为远程武器
                    {
                        //主手远程武器,副手近战武器
                        //1切换为近战武器，2跑远
                        int random = Random.Range(1, (int)Global.AttackRange + 1);
                        int limit = (int)(Global.AttackRange - (dis - 324));
                        if (random < limit)
                        {
                            SubStatus = (EAISubStatus.FightLeave);
                            return;
                        }
                        if (ChangeWeaponTick <= 0.0f)
                        {
                            owner.ChangeWeapon();
                            ChangeWeaponTick = 10.0f;
                            return;
                        }
                    }
                    else
                    {
                        //主手远程武器 副手无武器。距离太近
                        if (dis < Global.AttackRangeMin)
                        {
                            SubStatus = EAISubStatus.FightLeave;
                            return;
                        }
                    }
                }

                //使用近战武器，一定在攻击范围内。一次骰子判断所有概率.
                int attack = Random.Range(0, 100);
                if (attack < owner.Attr.Attack1)
                {
                    //判定攻击
                    if (fightTarget == null)
                        Debug.LogError("fightTarget == null");
                    TryAttack(attack);
                }
                else
                {
                    //攻击的几率分开算，不能合并到一起，否则攻击几率太大.
                    attack = Random.Range(0, 100);
                    if (attack < owner.Attr.Attack2)
                    {
                        TryAttack(owner.Attr.Attack1 + attack);
                    }
                    else
                    {
                        attack = Random.Range(0, 100);
                        if (attack < owner.Attr.Attack3)
                        {
                            TryAttack(owner.Attr.Attack1 + owner.Attr.Attack2 + attack);
                        }
                        else
                        {
                            //判定防御
                            int defence = Random.Range(0, 100);
                            if (!U3D.IsSpecialWeapon(owner.Attr.Weapon) && defence < owner.Attr.Guard)
                            {
                                //远程武器无法防御.
                                Stop();
                                owner.Guard(true);
                            }
                            else
                            {
                                //判定闪躲
                                int dodge = Random.Range(0, 100);
                                if (dodge < owner.Attr.Dodge)
                                {
                                    //逃跑，在危险状态下
                                    LeaveReset();
                                    ChangeState(EAIStatus.Dodge);
                                }
                                else
                                {
                                    int jump = Random.Range(0, 100);
                                    if (JumpCoroutine == null && jump < owner.Attr.Jump)
                                    {
                                        //最好不要带方向跳跃，否则可能跳到障碍物外被场景卡住
                                        Stop();
                                        AIJump();
                                    }
                                    else
                                    {
                                        int burst = Random.Range(0, 100);
                                        //判断速冲.
                                        if (burst < owner.Attr.Burst)
                                            AIBurst((EKeyList.KL_KeyA) + attack % 4);
                                        else
                                        {
                                            int pickup = Random.Range(0, 100);
                                            if (pickup < owner.Attr.GetItem)
                                            {
                                                if (owner.GetSceneItemTarget() != null && owner.GetSceneItemTarget().CanPickup())
                                                    ChangeState(EAIStatus.GetItem);
                                            }
                                        }
                                    }
                                }
                            }
                        }
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
            ChangeState(EAIStatus.Wait);
            return;
        }

        if (GetAngleBetween(fightTarget.mSkeletonPivot) > Global.AimDegree)
        {
            if (AttackRotateToTargetCoroutine == null)
            {
                owner.posMng.ChangeAction(0);
                AttackRotateToTargetCoroutine = owner.StartCoroutine(AttackRotateToTarget(fightTarget.mSkeletonPivot, EAISubStatus.Fight));
            }
        }
        else
        {
            if (AttackRotateToTargetCoroutine == null)
            {
                ChangeState(EAIStatus.Fight);
            }
        }
    }

    //撞到墙壁.如果在移动到目标更远处时，切换为攻击状态，避免一直对着墙跑.
    public void CheckStatus()
    {
        if (Status == EAIStatus.Fight && SubStatus == EAISubStatus.FightGotoPosition && owner.OnTouchWall)
        {
            StopCoroutine();
            Stop();
            ResetAIKey();
            ChangeState(EAIStatus.Wait);
        }
    }
    //离开战斗目标。可能是拿的远程武器，也可能是状态不好要逃跑.
    void OnFightLeave()
    {
        if (fightTarget == null)
        {
            ChangeState(EAIStatus.Wait);
            return;
        }

        float dis = Vector3.Distance(owner.mSkeletonPivot, fightTarget.mSkeletonPivot);
        if (dis >= 75.0f)
        {
            ChangeState(EAIStatus.Fight);
            return;
        }

        if (AttackRotateToTargetCoroutine == null)
        {
            //Debug.LogError("fight leave");
            Stop();
            TargetPos = PathMng.Instance.GetNearestWayPoint(fightTarget.mSkeletonPivot);
            if (TargetPos == Vector3.zero)
            {
                ChangeState(EAIStatus.Fight);
                return;
            }
            //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //go.transform.localScale = Vector3.one * 10;
            //go.transform.position = TargetPos;
            owner.posMng.ChangeAction(0);//?这里有问题
            AttackRotateToTargetCoroutine = owner.StartCoroutine(AttackRotateToTarget(TargetPos, EAISubStatus.FightGotoPosition));
        }
    }

    void RefreshPath(Vector3 now, Vector3 target)
    {
        PathMng.Instance.FindPath(owner,  now, target, ref Path);
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

    void OnKillThink()
    {
        if (killTarget == null || killTarget.Dead)
        {
            ChangeState(EAIStatus.Wait);
            return;
        }

        if (Vector3.SqrMagnitude(killTarget.mSkeletonPivot - owner.mSkeletonPivot) >= Global.AttackRange)
        {
            SubStatus = EAISubStatus.KillGotoTarget;
            return;
        }

        fightTarget = killTarget;
        ChangeState(EAIStatus.Fight);
    }

    //跟随状态
    //1：在路线中
    //2：到达最终一个路点，朝角色走去.
    //3：离跟随目标距离较近，停止跟随，转为Wait状态.
    //4: 目标一直在移动中，要一段时间刷新一次跟随路线.
    void OnFollow()
    {
        Vector3 vec;
        //跟随目标为空
        if (followTarget == null)
        {
            owner.controller.Input.AIMove(0, 0);
            ChangeState(EAIStatus.Wait);
            return;
        }

        float dis = 0.0f;
        switch (SubStatus)
        {
            case EAISubStatus.FollowGotoTarget:
                dis = Vector3.SqrMagnitude(owner.mSkeletonPivot - followTarget.mSkeletonPivot);
                if (dis < Global.FollowDistanceEnd)
                {
                    owner.controller.Input.AIMove(0, 0);
                    ChangeState(EAIStatus.Wait);
                    return;
                }

                vec = owner.mSkeletonPivot - followTarget.mSkeletonPivot;
                vec.y = 0;
                if (Vector3.SqrMagnitude(vec) <= Global.FollowDistanceEnd)
                {
                    owner.controller.Input.AIMove(0, 0);
                    ChangeState(EAIStatus.Wait);
                    return;
                }

                if (Path.Count == 0)
                    RefreshPath(owner.mSkeletonPivot, followTarget.mSkeletonPivot);
                else if (FollowRefreshTick <= 0.0f)
                {
                    FollowRefreshTick = 10.0f;
                    RefreshPath(owner.mSkeletonPivot, followTarget.mSkeletonPivot);
                    return;
                }

                if (targetIndex >= Path.Count)
                {
                    //朝角色走即可.
                    dis = Vector3.SqrMagnitude(followTarget.mSkeletonPivot - owner.mSkeletonPivot);
                    //不计算高度的距离.30码
                    if (dis < Global.AttackRangeMinD)
                    {
                        owner.controller.Input.AIMove(0, 0);
                        ChangeState(EAIStatus.Wait);
                        return;
                    }
                    owner.FaceToTarget(followTarget.mSkeletonPivot);
                    owner.controller.Input.AIMove(0, 1);
                }
                else
                {
                    if (curIndex == -1)
                        targetIndex = 0;

                    //检查这一帧是否会走过目标，因为跨步太大.
                    NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    //33码距离内.
                    if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
                    {
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                        float s = GetAngleBetween((NextFramePos - owner.mSkeletonPivot).normalized, (Path[targetIndex].pos - NextFramePos).normalized);
                        if (s < 0)
                        {
                            //其他路点，到达后转向下一个路点.
                            owner.controller.Input.AIMove(0, 0);
                            curIndex = targetIndex;
                            targetIndex += 1;
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.FollowSubRotateToTarget;//到指定地点后旋转到目标.
                            return;
                        }
                    }
                    owner.FaceToTarget(Path[targetIndex].pos);
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
                        if (AIJumpDelay >= 2.5f && random < owner.Attr.Jump)
                        {
                            AIJump();
                            AIJumpDelay = 0.0f;
                            return;
                        }
                    };
                    break;
                }
                break;
            case EAISubStatus.FollowSubRotateToTarget:
                if (targetIndex >= Path.Count)
                    vec = followTarget.mSkeletonPivot;
                else
                    vec = Path[targetIndex].pos;

                if (FollowRotateToTargetCoroutine == null)
                    FollowRotateToTargetCoroutine = owner.StartCoroutine(FollowRotateToTarget(vec, EAISubStatus.FollowGotoTarget));
                break;
        }
    }

    void OnKill()
    {
        Vector3 vec;
        switch (SubStatus)
        {
            case EAISubStatus.KillThink:
                OnKillThink();
                break;
            case EAISubStatus.KillGotoTarget:
                if (killTarget == null)
                    killTarget = owner.GetLockedTarget();
                if (killTarget != null)
                {
                    float dis = 0.0f;
                    dis = Vector3.SqrMagnitude(owner.mSkeletonPivot - killTarget.mSkeletonPivot);
                    if (dis < Global.AttackRange)//小于50码停止跟随，不需要计算路径
                    {
                        //FollowPath.Clear();
                        //Debug.Log("stop follow until 35 meters");
                        owner.controller.Input.AIMove(0, 0);
                        ChangeState(EAIStatus.Wait);//开始寻找敌人
                        return;
                    }
                    //桥上桥下，跟随抖动问题.
                    vec = owner.mSkeletonPivot - killTarget.mSkeletonPivot;
                    vec.y = 0;
                    if (Vector3.SqrMagnitude(vec) < Global.AttackRange)//小于50码停止跟随，不需要计算路径
                    {
                        //FollowPath.Clear();
                        //Debug.Log("stop follow until 35 meters");
                        owner.controller.Input.AIMove(0, 0);
                        ChangeState(EAIStatus.Wait);//开始寻找敌人
                        return;
                    }

                    if (Path.Count == 0)
                        RefreshPath(owner.mSkeletonPivot, killTarget.mSkeletonPivot);

                    if (curIndex == -1)
                        targetIndex = 0;

                    //已经到达过最后一个寻路点.
                    if (targetIndex < Path.Count && Path.Count > 1)
                    {
                        //检查这一帧是否会走过目标，因为跨步太大.【这一段有问题，只有离目标非常近的时候再判断才行，远的话，可能会绕路，导致下一帧距离目标的位置越来越远】
                        NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
                        NextFramePos.y = 0;
                        //33码距离内.
                        if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
                        {
                            NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                            float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(Path[targetIndex].pos - NextFramePos));
                            if (s < 0)
                            {
                                owner.controller.Input.AIMove(0, 0);
                                curIndex = targetIndex;
                                targetIndex += 1;
                                RotateRound = Random.Range(1, 3);
                                SubStatus = EAISubStatus.KillSubRotateToTarget;//到指定地点后旋转到目标.
                                return;
                            }
                        }

                        owner.FaceToTarget(Path[targetIndex].pos);
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
                            if (AIJumpDelay >= 2.5f && random < owner.Attr.Jump)
                            {
                                AIJump();
                                AIJumpDelay = 0.0f;
                                return;
                            }
                        }
                    }
                    else
                    {
                        NextFramePos = killTarget.mSkeletonPivot - owner.mSkeletonPivot;
                        NextFramePos.y = 0;
                        if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
                        {
                            NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                            float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(killTarget.mSkeletonPivot - NextFramePos));
                            if (s < 0)
                            {
                                owner.controller.Input.AIMove(0, 0);
                                ChangeState(EAIStatus.Wait);
                                return;
                            }
                        }
                        owner.FaceToTarget(killTarget.mSkeletonPivot);
                        owner.controller.Input.AIMove(0, 1);
                    }
                    break;
                }
                else
                {
                    ChangeState(EAIStatus.Wait);
                }
                break;
            case EAISubStatus.KillSubRotateToTarget:
                if (KillRotateToTargetCoroutine == null)
                {
                    if (targetIndex >= Path.Count)
                        vec = killTarget.mSkeletonPivot;
                    else
                        vec = Path[targetIndex].pos;
                    KillRotateToTargetCoroutine = owner.StartCoroutine(KillRotateToTarget(vec, EAISubStatus.KillGotoTarget));
                }
                break;
        }
    }

    public void FollowTarget(int target)
    {
        followTarget = U3D.GetUnit(target);
        ChangeState(EAIStatus.Follow);
        SubStatus = EAISubStatus.FollowGotoTarget;
    }

    Coroutine struggleCoroutine;
    EKeyList[] struggleKey = new EKeyList[5];
    IEnumerator ProcessStruggle()
    {
        //可以使用上下左右跳，跳A来起身.
        if (struggleKey[0] != EKeyList.KL_KeyW)
        {
            struggleKey[0] = EKeyList.KL_KeyW;
            struggleKey[1] = EKeyList.KL_KeyS;
            struggleKey[2] = EKeyList.KL_KeyA;
            struggleKey[3] = EKeyList.KL_KeyD;
            struggleKey[4] = EKeyList.KL_Jump;
        }
        int k = Random.Range(0, 5);
        yield return 0;
        while (true)
        {
            yield return 0;
            yield return 0;
            owner.controller.Input.OnKeyDown(struggleKey[k], true);
            yield return 0;
            yield return 0;
            owner.controller.Input.OnKeyUp(struggleKey[k]);
            break;
        }
        struggleCoroutine = null;
    }

    public void OnUnitDead(MeteorUnit deadunit)
    {
        tick = updateDelta;//下一次进入空闲立即刷新对象位置和方向。
        if (followTarget == deadunit)
        {
            if (Status == EAIStatus.Follow)
            {
                ChangeState(EAIStatus.Wait);
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
        
        if (lookTick >= 5.0f)
        {
            int lookChance = Random.Range(0, 100);
            if (lookChance < owner.Attr.Look)
            {
                lookTick = 0;//5秒内最多能进行一次四处看.
                ChangeState(EAIStatus.Look);
            }
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

    //改变主状态，则清空寻路数据，否则不用.
    public void ChangeState(EAIStatus type)
    {
        Status = type;
        StopCoroutine();
        Stop();
        ResetAIKey();
        Path.Clear();
        if (type == EAIStatus.Kill)
        {
            SubStatus = EAISubStatus.KillThink;
        }
        else if (type == EAIStatus.Patrol)
        {
            SubStatus = EAISubStatus.Patrol;
        }
        else if (type == EAIStatus.Follow)
        {
            SubStatus = EAISubStatus.FollowGotoTarget;
        }
        else if (type == EAIStatus.Guard)
        {
            waitDefence = 2.0f;//2秒后开始随机决定能否释放防御键
            owner.controller.Input.OnKeyDown(EKeyList.KL_Defence, true);//防御
        }
        else if (type == EAIStatus.AttackTarget)
        {
            //朝目标处攻击数次
            RefreshPath(owner.mSkeletonPivot, AttackTarget);
            SubStatus = EAISubStatus.AttackGotoTarget;
        }
        else if (type == EAIStatus.GetItem)
        {
            SubStatus = EAISubStatus.GetItemGotoItem;
        }
        else if (type == EAIStatus.Wait)
            SubStatus = EAISubStatus.SubStatusWait;
        else if (type == EAIStatus.Fight)
            SubStatus = EAISubStatus.Fight;
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

        if (PatrolRotateToPatrolPointCoroutine != null)
        {
            owner.StopCoroutine(PatrolRotateToPatrolPointCoroutine);
            PatrolRotateToPatrolPointCoroutine = null;
        }

        if (PatrolRotateToTargetCoroutine != null)
        {
            owner.StopCoroutine(PatrolRotateToTargetCoroutine);
            PatrolRotateToTargetCoroutine = null;
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
        //寻路数据要清空.
        Path.Clear();
        //受到非目标的攻击后，记下来，一会找他算账
        if (lastAttacker != attacker && owner.GetLockedTarget() != attacker && attacker != null)
            lastAttacker = attacker;
        Stop();
        ResetAIKey();
        StopCoroutine();
        owner.controller.Input.ResetInput();
        if (attacker != null)
        {
            //攻击者在视野内，切换为战斗姿态，否则
            if (owner.Find(attacker))
            {
                //如果当前有攻击目标，是否切换目标
                if (owner.GetLockedTarget() == null)
                {
                    ChangeState(EAIStatus.Fight);
                    fightTarget = attacker;
                }
            }
        }
    }

    //寻路相关的.
    public int curPatrolIndex;
    //int startPathIndex;
    bool reverse = false;
    public int targetPatrolIndex;
    public List<WayPoint> PatrolPath = new List<WayPoint>();
    //List<WayPoint> PatrolPathBegin = new List<WayPoint>();
    public List<int> patrolData = new List<int>();
    public void SetPatrolPath(List<int> path)
    {
        patrolData.Clear();
        for (int i = 0; i < path.Count; i++)
            patrolData.Add(path[i]);
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
                PatrolPath.Clear();
                PathMng.Instance.FindPath(owner, PathTmp[i].index, PathTmp[i + 1].index, ref PatrolPath);
                if (PatrolPath.Count != 0)
                {
                    if (idx.Count == 0)
                        idx.Add(PatrolPath[0].index);
                    for (int j = 1; j < PatrolPath.Count; j++)
                        idx.Add(PatrolPath[j].index);
                }
            }
        }
        PatrolPath.Clear();
        for (int i = 0; i < idx.Count; i++)
            PatrolPath.Add(Global.GLevelItem.wayPoint[idx[i]]);
        curPatrolIndex = -1;
        targetPatrolIndex = -1;
        curIndex = -1;
        targetIndex = -1;
    }

    //绕原地
    Coroutine PatrolRotateCoroutine;//巡逻到达某个目的点后，会随机旋转1-5次，每次都会停留不固定的时间
    IEnumerator PatrolRotate(float angleSpeed = 150)
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
        vec.y = 0;
        //同位置，无法计算夹角.
        if (vec.x == owner.mPos.x && vec.z == owner.mPos.z)
            return 0;

        Vector3 vec1 = -owner.transform.forward;
        Vector3 vec2 = (vec - owner.mPos2d).normalized;
        vec2.y = 0;
        float radian = Vector3.Dot(vec1, vec2);
        float degree = Mathf.Acos(radian) * Mathf.Rad2Deg;
        //Debug.LogError("夹角:" + degree);
        if (float.IsNaN(degree))
            Debug.LogError("NAN");
        return degree;
    }

    //计算夹角，不考虑Y轴
    float GetAngleBetween(Vector3 first, Vector3 second)
    {
        if (first.x == second.x && first.z == second.z)
            return 0;
        first.y = 0;
        second.y = 0;
        float s = Vector3.Dot(first, second);
        return s;//大于0，同方向，小于0 反方向
    }

    Coroutine GetItemRotateToTargetCoroutine;
    IEnumerator GetItemRotateToTarget(Vector3 vec)
    {
        if (vec.x == owner.mPos.x && vec.z == owner.mPos.z)
        {
            GetItemRotateToTargetCoroutine = null;
            SubStatus = EAISubStatus.GetItemGotoItem;
            yield break;
        }
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
    IEnumerator AttackRotateToTarget(Vector3 vec, EAISubStatus subStatus)
    {
        if (vec.x == owner.mPos.x && vec.z == owner.mPos.z)
        {
            AttackRotateToTargetCoroutine = null;
            SubStatus = subStatus;
            yield break;
        }
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
        SubStatus = subStatus;
    }

    Coroutine KillRotateToTargetCoroutine;
    Coroutine KillRotateToPositionCoroutine;
    IEnumerator KillRotateToPosition(Vector3 vec, EAISubStatus onFinishSubStatus)
    {
        if (vec.x == owner.mPos.x && vec.z == owner.mPos.z)
        {
            FollowRotateToTargetCoroutine = null;
            SubStatus = onFinishSubStatus;
            yield break;
        }
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
        KillRotateToPositionCoroutine = null;
        SubStatus = onFinishSubStatus;
    }

    IEnumerator KillRotateToTarget(Vector3 vec, EAISubStatus onFinishSubStatus)
    {
        if (vec.x == owner.mPos.x && vec.z == owner.mPos.z)
        {
            FollowRotateToTargetCoroutine = null;
            SubStatus = onFinishSubStatus;
            yield break;
        }
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
        KillRotateToTargetCoroutine = null;
        SubStatus = onFinishSubStatus;
    }

    Coroutine FollowRotateToTargetCoroutine;
    Coroutine FollowRotateToPositionCoroutine;
    IEnumerator FollowRotateToPosition(Vector3 vec, EAISubStatus onFinishSubStatus)
    {
        if (vec.x == owner.mPos.x && vec.z == owner.mPos.z)
        {
            FollowRotateToTargetCoroutine = null;
            SubStatus = onFinishSubStatus;
            yield break;
        }
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
        FollowRotateToPositionCoroutine = null;
        SubStatus = onFinishSubStatus;
    }

    IEnumerator FollowRotateToTarget(Vector3 vec, EAISubStatus onFinishSubStatus)
    {
        if (vec.x == owner.mPos.x && vec.z == owner.mPos.z)
        {
            FollowRotateToTargetCoroutine = null;
            SubStatus = onFinishSubStatus;
            yield break;
        }
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
        FollowRotateToTargetCoroutine = null;
        SubStatus = onFinishSubStatus;
    }

    //朝向指定目标，一定时间内
    Coroutine PatrolRotateToTargetCoroutine;
    IEnumerator PatrolRotateToTarget(Vector3 vec)
    {
        if (vec.x == owner.mPos.x && vec.z == owner.mPos.z)
        {
            PatrolRotateToTargetCoroutine = null;
            SubStatus = EAISubStatus.PatrolSubGotoTarget;
            yield break;
        }
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

    Coroutine PatrolRotateToPatrolPointCoroutine;
    IEnumerator PatrolRotateToPatrolPoint(Vector3 vec)
    {
        if (vec.x == owner.mPos.x && vec.z == owner.mPos.z)
        {
            PatrolRotateToPatrolPointCoroutine = null;
            SubStatus = EAISubStatus.PatrolGotoFirstPoint;
            yield break;
        }
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
        PatrolRotateToPatrolPointCoroutine = null;
        SubStatus = EAISubStatus.PatrolGotoFirstPoint;
    }

    int RotateRound;
    float AIJumpDelay = 0.0f;
    int GetPatrolIndex()
    {
        int k = PathMng.Instance.GetWayIndex(owner.mSkeletonPivot);
        for (int i = 0; i < PatrolPath.Count; i++)
        {
            if (PatrolPath[i].index == k)
                return i;
        }
        return -1;
    }

    int GetNearestPatrolPoint()
    {
        float dis = 25000000.0f;
        int ret = -1;
        for (int i = 0; i < PatrolPath.Count; i++)
        {
            float d = Vector3.SqrMagnitude(PatrolPath[i].pos - owner.mSkeletonPivot);
            if (d < dis)
            {
                dis = d;
                ret = i;
            }
        }
        return ret;
    }

    void OnPatrol()
    {
        switch (SubStatus)
        {
            case EAISubStatus.Patrol:
                {
                    if (PatrolPath.Count == 0 || patrolData.Count == 0)
                    {
                        ChangeState(EAIStatus.Wait);
                        SubStatus = EAISubStatus.SubStatusWait;
                        return;
                    }

                    int k = GetPatrolIndex();
                    if (k == -1)
                    {
                        int n = GetNearestPatrolPoint();
                        //当不在任何一个巡逻点中时，跑到第一个巡逻点的过程
                        SubStatus = EAISubStatus.PatrolGotoFirstPoint;
                        RefreshPath(owner.mSkeletonPivot, PatrolPath[n].pos);
                        targetPatrolIndex = n;//目的地
                        return;
                    }

                    //原地巡逻
                    if (PatrolPath.Count == 1 && patrolData.Count == 1)
                    {
                        SubStatus = EAISubStatus.PatrolSubInPlace;
                        return;
                    }

                    //逆序巡逻
                    if (reverse)
                    {
                        if (curPatrolIndex <= 0)
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
                    PatrolRotateToTargetCoroutine = owner.StartCoroutine(PatrolRotateToTarget(PatrolPath[targetPatrolIndex].pos));
                break;
            case EAISubStatus.PatrolSubRotateToPatrolPoint:
                if (PatrolRotateToPatrolPointCoroutine == null)
                {
                    Vector3 vec;
                    if (targetIndex >= Path.Count)
                        vec = PatrolPath[targetPatrolIndex].pos;
                    else
                        vec = Path[targetIndex].pos;
                    PatrolRotateToPatrolPointCoroutine = owner.StartCoroutine(PatrolRotateToPatrolPoint(vec));
                }
                break;
            case EAISubStatus.PatrolSubGotoTarget:
                owner.FaceToTarget(new Vector3(PatrolPath[targetPatrolIndex].pos.x, 0, PatrolPath[targetPatrolIndex].pos.z));
                owner.controller.Input.AIMove(0, 1);
                //模拟跳跃键，移动到下一个位置.还得按住上
                if (curPatrolIndex != -1)
                {
                    if (PathMng.Instance.GetWalkMethod(PatrolPath[curPatrolIndex].index, PatrolPath[targetPatrolIndex].index) == WalkType.Jump && owner.IsOnGround() && AIJumpDelay > 2.5f)
                    {
                        AIJump();
                        AIJumpDelay = 0.0f;
                    }

                    //尝试几率跳跃，否则可能会被卡住.
                    int random = Random.Range(0, 100);
                    if (AIJumpDelay >= 2.5f && random < owner.Attr.Jump)
                    {
                        AIJump();
                        AIJumpDelay = 0.0f;
                    }
                }
                break;
            case EAISubStatus.PatrolGotoFirstPoint:
                if (curIndex == -1)
                    targetIndex = 0;

                if (targetIndex >= Path.Count)
                {
                    NextFramePos = PatrolPath[targetPatrolIndex].pos - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
                    {
                        NextFramePos.y = 0;
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                        float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(PatrolPath[targetPatrolIndex].pos - NextFramePos));
                        if (s < 0)
                        {
                            //不在寻路点上，说明已经到达终点,进入巡逻队列状态
                            owner.controller.Input.AIMove(0, 0);
                            SubStatus = EAISubStatus.Patrol;
                            curPatrolIndex = targetPatrolIndex;
                            targetPatrolIndex += 1;
                            if (targetPatrolIndex >= PatrolPath.Count)
                                reverse = true;
                            return;
                        }
                    }
                    owner.FaceToTarget(PatrolPath[targetPatrolIndex].pos);
                    owner.controller.Input.AIMove(0, 1);
                }
                else
                {
                    NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
                    {
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * Time.deltaTime * 0.15f;
                        float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(Path[targetIndex].pos - NextFramePos));
                        if (s < 0)
                        {
                            owner.controller.Input.AIMove(0, 0);
                            curIndex = targetIndex;
                            targetIndex += 1;
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.PatrolSubRotateToPatrolPoint;//到指定地点后旋转到目标.
                            return;
                        }
                    }
                    owner.FaceToTarget(Path[targetIndex].pos);
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
                        if (AIJumpDelay >= 2.5f && random < owner.Attr.Jump)
                        {
                            AIJump();
                            AIJumpDelay = 0.0f;
                            return;
                        }
                    }
                }
                break;
            case EAISubStatus.PatrolSubInPlace:
                if (PatrolRotateCoroutine == null)
                    PatrolRotateCoroutine = owner.StartCoroutine(PatrolRotate(90.0f));
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

    public void OnGotoWayPoint(int wayIndex)
    {
        switch (Status)
        {
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