using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Idevgame.Util;

public enum EAIStatus
{
    None,
    Idle,//不动.
    RebornFriend,//复活队友.
    Fight,//包括所有战斗情况
    Kill,//强制杀死指定敌人，无视距离，一直跟随
    Guard,//防御
    Follow,//跟随
    Think,//没发觉目标时左右观察
    Patrol,//巡逻。
    Wait,//，类似于Search
    Dodge,//逃跑
    Look,//四处看
    Mew,//喵
    GetItem,//取得场景道具-最近的，可拾取的(未打碎的箱子不算在内).
    AttackTarget,//攻击指定位置.
    FindWay,//寻路状态，寻路完毕后，恢复到前一个状态,使用寻路的数据
    //PushByWall,//被墙壁推挤.
}

public enum EAISubStatus
{
    None,
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

    StartFind,//开始寻路
    FindWait,//等待寻路完成,
    FindWaitPatrol,//等待巡逻所有路点间的寻路完成.
}

//寻路流程，得到自己和目标所在路点，取得路点序列，向下一个路点进发，到达最后一个路点->往目的进发->到达目的，结束.
//比较麻烦的是，距离下个路点的判定不能仅使用距离，因为若一帧内直接超过下个路点，那么又会反过来走.而永远无法到达目的路点.
//所以这里使用当前位置，目标位置(或下个路点位置)，用2个向量的夹角是逆向还是同向决定，是否已经到达下个路点了。

//Think 10 / (Think = 100) = 0.1f思考一次行动.
//Think 10 / (Think = 40) = 0.25f思考一次行动.
public class MeteorAI {
    public MeteorAI(MeteorUnit user)
    {
        owner = user;
        Status = EAIStatus.None;
        StatusOnComplete = EAIStatus.Wait;
        SubStatus = EAISubStatus.None;
    }

    public void PathReset()
    {
        
    }

    //寻路相关数据
    public SortedDictionary<int, List<PathNode>> PathInfo = new SortedDictionary<int, List<PathNode>>();
    public PathNode[] nodeContainer;
    public List<WayPoint> Path = new List<WayPoint>();//存储寻路找到的路线点

    public EAIStatus Status { get; set; }
    public EAIStatus StatusOnComplete { get; set; }//当寻路完成时，切换回去的主状态，若中途没有被打断.
    public EAISubStatus SubStatusOnComplete { get; set; }//当寻路完成时，切换回去的子状态
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
    
    float RebornTick = 30.0f;
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
            StopMove();
            pause_tick -= FrameReplay.deltaTime;
            if (pause_tick <= 0.0f)
                paused = false;
            return;
        }

        ThickTick -= owner.Attr.Think;

        if (owner.OnTouchWall)
            touchLast += FrameReplay.deltaTime;
        else
            touchLast = 0.0f;
        RebornTick -= FrameReplay.deltaTime;
        ChangeWeaponTick -= FrameReplay.deltaTime;
        AIJumpDelay += FrameReplay.deltaTime;
        lookTick += FrameReplay.deltaTime;
        //RefreshFollowPathTick -= FrameReplay.deltaTime;
        //如果在硬直中
        if (owner.charLoader.IsInStraight())
            return;

        //如果处于跌倒状态.A处理从地面站立,僵直过后才能正常站立 B，在后面的逻辑中，决定是否用爆气解除跌倒状态
        if (DoProcessStruggle())
            return;

        //行为优先级 
        //AI强制行为(攻击指定位置，Kill追杀（不论视野）攻击 ) > 战斗 > 跟随 > 巡逻 > 
        
        //找到身旁是否有死亡队友，
        if (Main.Ins.CombatData.GGameMode == GameMode.ANSHA && owner.IsLeader && RebornTick <= 0)
        {
            //如果50码以内有死亡队友.且当前在前进/待机/预备动作
            if (CanChangeToRebornStatus())
                ChangeState(EAIStatus.RebornFriend);
        }

        if (Status == EAIStatus.Patrol)
        {
            //巡逻状态如果找到了敌人，与敌人搏斗.
            if (killTarget != null)
                ChangeState(EAIStatus.Kill);
            else
            if (owner.LockTarget != null)
            {
                //拥有锁定目标-等待与敌人发生战斗
                ChangeState(EAIStatus.Wait);
            }
            else
            if (followTarget != null)
                ChangeState(EAIStatus.Follow);
        }

        if (Status == EAIStatus.Fight)
        {
            //所有目标离开范围时.看有无追杀对象.
            if (owner.LockTarget == null)
            {
                if (killTarget != null)
                {
                    fightTarget = killTarget;
                }
                else if (followTarget != null)
                {
                    float dis = Vector3.SqrMagnitude(owner.mSkeletonPivot - followTarget.mSkeletonPivot);
                    if (dis >= CombatData.FollowDistanceStart)//距离65码开始跟随
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
            if (owner.TargetItem != null && owner.TargetItem.CanPickup())
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

            if (owner.LockTarget != null)
            {
                fightTarget = owner.LockTarget;
                ChangeState(EAIStatus.Fight);
                return;
            }

            if (followTarget != null)
            {
                float dis = Vector3.SqrMagnitude(owner.mSkeletonPivot - followTarget.mSkeletonPivot);
                if (dis >= CombatData.FollowDistanceStart)//距离65码开始跟随
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
            case EAIStatus.RebornFriend:
                OnRebornFriend();
                break;
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
            case EAIStatus.FindWay:
                OnFindWay();
                break;
        }


    }
    public bool FindWayFinished = false;
    public bool FindPatrolFinished = false;
    void OnFindWay()
    {
        switch (SubStatus)
        {
            case EAISubStatus.StartFind:
                switch (StatusOnComplete)
                {
                    case EAIStatus.GetItem:
                        RefreshPath(owner.mSkeletonPivot, owner.TargetItem.transform.position);
                        SubStatus = EAISubStatus.FindWait;
                        break;
                    case EAIStatus.Patrol:
                        switch (SubStatusOnComplete)
                        {
                            case EAISubStatus.Patrol:
                                //RefreshPathCoroutine = Main.Ins.StartCoroutine(RefreshPatrolPath());
                                SubStatus = EAISubStatus.FindWaitPatrol;
                                break;
                            case EAISubStatus.PatrolGotoFirstPoint:
                                RefreshPath(owner.mSkeletonPivot, PatrolPath[targetPatrolIndex].pos);
                                SubStatus = EAISubStatus.FindWait;
                                break;
                        }
                        break;
                    case EAIStatus.Follow:
                        RefreshPathEx(followTarget);
                        SubStatus = EAISubStatus.FindWait;
                        break;
                    case EAIStatus.AttackTarget:
                        RefreshPath(owner.transform.position, AttackTarget);
                        SubStatus = EAISubStatus.FindWait;
                        break;
                    case EAIStatus.Fight:
                        switch (SubStatusOnComplete)
                        {
                            case EAISubStatus.FightGotoPosition:
                                RefreshPath(owner.mSkeletonPivot, TargetPos);
                                SubStatus = EAISubStatus.FindWait;
                                break;
                            case EAISubStatus.FightGotoTarget:
                                RefreshPathEx(fightTarget);
                                SubStatus = EAISubStatus.FindWait;
                                break;
                        }
                        break;
                    case EAIStatus.Kill:
                        RefreshPathEx(killTarget);
                        SubStatus = EAISubStatus.FindWait;
                        break;
                }
                break;
            case EAISubStatus.FindWait:
                if (Path.Count != 0 && FindWayFinished)
                {
                    OnFindWayEnd();
                    break;
                }
                if (Path.Count == 0 && RefreshPathCoroutine == null)
                {
                    OnFindWayEnd();
                    break;
                }
                break;
            case EAISubStatus.FindWaitPatrol:
                if (PatrolPath.Count != 0 && FindPatrolFinished)
                {
                    OnFindWayEnd();
                    break;
                }
                if (PatrolPath.Count == 0 && RefreshPathCoroutine == null)
                {
                    OnFindWayEnd();
                    break;
                }
                break;
        }   
    }
    //GameObject line = null;
    //LineRenderer l;
    //List<GameObject> wayDebug = new List<GameObject>();
    void OnFindWayEnd()
    {
        //Debug.LogError("寻路完成，路径点个数:" + Mathf.Max(Path.Count, PatrolPath.Count));
        //if (line == null)
        //{
        //    line = new GameObject();
        //    l = line.AddComponent<LineRenderer>();
        //    l.startWidth = 1.0f;
        //    l.endWidth = 1.0f;
        //    l.startColor = Color.red;
        //    l.endColor = Color.red;
        //    line.name = "debugPath";
        //}
        //l.numPositions = Path.Count;
        //for (int i = 0; i < wayDebug.Count; i++)
        //    GameObject.Destroy(wayDebug[i].gameObject);
        //wayDebug.Clear();
        //for (int i = 0; i < Path.Count; i++)
        //{
        //    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    obj.name = string.Format("Way{0}:WayIndex{1}", i, Path[i].index);
        //    BoxCollider b = obj.GetComponent<BoxCollider>();
        //    if (b != null)
        //        b.enabled = false;
        //    obj.transform.position = Path[i].pos;
        //    l.SetPosition(i, Path[i].pos);
        //    wayDebug.Add(obj);
        //}

        if (RefreshPathCoroutine != null)
        {
            Main.Ins.StopCoroutine(RefreshPathCoroutine);
            RefreshPathCoroutine = null;
        }
        ChangeState(StatusOnComplete);
        SubStatus = SubStatusOnComplete;
    }

    void RefreshPatrolPath()
    {
        PatrolTemp.Clear();
        for (int i = 0; i < patrolData.Count; i++)
        {
            if (patrolData[i] < Main.Ins.CombatData.wayPoints.Count)
                PatrolTemp.Add(Main.Ins.CombatData.wayPoints[patrolData[i]]);
        }

        //计算从第一个点到最后一个点的完整路径，放到完整巡逻点钟
        List<int> idx = new List<int>();
        //单点巡逻
        if (PatrolTemp.Count == 1)
            idx.Add(PatrolTemp[0].index);
        else
        {
            //多点巡逻
            for (int i = 0; i < PatrolTemp.Count - 1; i++)
            {
                PatrolTemp2.Clear();
                //yield return Main.Ins.PathMng.FindPath(owner, PatrolTemp[i].index, PatrolTemp[i + 1].index, PatrolTemp2);
                if (PatrolTemp2.Count != 0)
                {
                    if (idx.Count == 0)
                        idx.Add(PatrolTemp2[0].index);
                    for (int j = 1; j < PatrolTemp2.Count; j++)
                        idx.Add(PatrolTemp2[j].index);
                }
            }
        }
        for (int i = 0; i < idx.Count; i++)
            PatrolPath.Add(Main.Ins.CombatData.wayPoints[idx[i]]);
        FindPatrolFinished = true;
    }

    bool CanChangeToRebornStatus()
    {
        if (owner.posMng.mActiveAction.Idx == CommonAction.Idle || PoseStatus.IsReadyAction(owner.posMng.mActiveAction.Idx) || owner.posMng.mActiveAction.Idx == CommonAction.Run || owner.posMng.mActiveAction.Idx == CommonAction.RunOnDrug)
        {
            if (owner.HasRebornTarget())
                return true;
        }
        return false;
    }

    //处理倒地后的挣扎起身
    bool DoProcessStruggle()
    {
        if (owner.posMng.mActiveAction.Idx == CommonAction.Struggle || owner.posMng.mActiveAction.Idx == CommonAction.Struggle0)
        {
            if (struggleCoroutine == null)
                struggleCoroutine = owner.StartCoroutine(ProcessStruggle());
            else
            {
                if (waitStruggleDone != 0)
                {
                    waitStruggleDone--;
                    return true;
                }
                owner.StopCoroutine(struggleCoroutine);
                struggleCoroutine = null;
                waitStruggleDone = 2 * Main.Ins.AppInfo.LinkDelay();
            }
            return true;
        }
        return false;
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

        leaveTick += FrameReplay.deltaTime;
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
                if (owner.TargetItem == null)
                {
                    StopMove();
                    ChangeState(EAIStatus.Wait);
                    return;
                }

                if (Path.Count == 0)
                {
                    //Debug.LogError("拾取物品进入寻路");
                    ChangeState(EAIStatus.FindWay, EAIStatus.GetItem, EAISubStatus.GetItemGotoItem);
                    return;
                }

                if (curIndex == -1)
                    targetIndex = 0;

                if (targetIndex < Path.Count)
                {
                    NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
                    {
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
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
                        if (Main.Ins.PathMng.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump && owner.IsOnGround())
                        {
                            owner.controller.Input.AIMove(0, 0);
                            AIJump(Path[targetIndex].pos);
                            AIJumpDelay = 0.0f;
                            return;
                        }
                    }
                }
                else
                {
                    NextFramePos = owner.TargetItem.transform.position - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
                    {
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
                        float s = GetAngleBetween(Vector3.Normalize(NextFramePos - owner.mSkeletonPivot), Vector3.Normalize(owner.TargetItem.transform.position - NextFramePos));
                        //反向
                        if (s < 0)
                        {
                            owner.controller.Input.AIMove(0, 0);
                            ChangeState(EAIStatus.Wait);
                            return;
                        }
                    }
                    owner.FaceToTarget(owner.TargetItem.transform.position);
                    owner.controller.Input.AIMove(0, 1);
                }
                break;
            case EAISubStatus.GetItemSubRotateToItem:
                if (GetItemRotateToTargetCoroutine == null)
                {
                    if (owner.TargetItem == null)
                    {
                        owner.controller.Input.AIMove(0, 0);
                        ChangeState(EAIStatus.Wait);
                        return;
                    }
                    Vector3 targetPosition = Vector3.zero;
                    if (targetIndex >= Path.Count)
                        targetPosition = owner.TargetItem.transform.position;
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
        waitDefence -= FrameReplay.deltaTime;
    }

    //四处看.
    Coroutine LookRotateToTargetCoroutine;
    void OnLook()
    {
        if (owner.LockTarget != null && LookRotateToTargetCoroutine == null)
        {
            StopMove();
            ChangeState(EAIStatus.Wait);
            return;
        }
        if (LookRotateToTargetCoroutine == null && owner.LockTarget == null)
        {
            if (owner.posMng.mActiveAction.Idx != CommonAction.Idle)
                return;
            float angle = Random.Range(-60, 60);
            StopMove();//停止移动.
            LookRotateToTargetCoroutine = owner.StartCoroutine(LookRotateToTarget(angle));
        }
    }

    IEnumerator LookRotateToTarget(float leaveAngle, float angularspeed = CombatData.AngularVelocity)
    {
        bool rightRotate = Random.Range(-1, 2) >= 0;
        float offset = 0.0f;
        float offsetmax = leaveAngle;
        float timeTotal = offsetmax / angularspeed;
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += FrameReplay.deltaTime;
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
        StopMove();
    }

    IEnumerator LeaveRotateAngle(float leaveAngle, float angularspeed = CombatData.AngularVelocity)
    {
        bool rightRotate = Random.Range(-1, 2) >= 0;
        float offset = 0.0f;
        float offsetmax = leaveAngle;
        float timeTotal = offsetmax / angularspeed;
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += FrameReplay.deltaTime;
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

    public void StopMove()
    {
        owner.controller.Input.AIMove(0, 0);
    }

    //可能存在怒气不足，这个时候就会失败.
    bool TryAttack(int attack = 0)
    {
        if (attack == 0)
            attack = Random.Range(0, owner.Attr.Attack1 + owner.Attr.Attack2 + owner.Attr.Attack3);
        //仅仅是因为枪的待机姿势，没有设定可以接哪些姿势，导致这里要硬写.
        if (owner.posMng.mActiveAction.Idx == CommonAction.GunIdle || owner.weaponLoader.WeaponType() == (int)EquipWeaponType.Gun)
        {
            //枪只有 A， 上A， 下A， 下上A， 下上上A
            if (attack <= owner.Attr.Attack1)
            {
                //普通攻击,A,上A,下A 枪的输入映射 5-206-普通攻击 6-207-上射击
                if (attack > owner.Attr.Attack1 / 2)
                    TryPlayWeaponPose(5);//5或6
                else
                    TryPlayWeaponPose(6);//5或6
            }
            else if (attack <= (owner.Attr.Attack1 + owner.Attr.Attack2))
            {
                //重攻击,耗费气20 7-208-枪杀射击
                if (owner.AngryValue >= 20)
                    TryPlayWeaponPose(7);
                else
                    return false;
            }
            else if (attack <= (owner.Attr.Attack1 + owner.Attr.Attack2 + owner.Attr.Attack3))
            {
                //绝招,SWWJ，SWJ
                //owner.AddAngry(Global.ANGRYBURST);
                if (owner.AngryValue == CombatData.ANGRYMAX)
                    TryPlayWeaponPose(8);//火枪大招.
                else if (owner.AngryValue >= CombatData.ANGRYBURST)
                    TryPlayWeaponPose(147);//火枪小绝招
                else
                    return false;
            }
        }
        else
        {
            //已经在[攻击招式/预备姿势]中.后接招式挑选，类似140-148，蹲下是可以接所有招式的.所以火枪不仅仅走上面.
            //把可以使用的招式放到集合里，A类放普攻，B类放搓招， C类放绝招
            ActionNode act = Main.Ins.ActionInterrupt.GetActions(owner.posMng.mActiveAction.Idx);
            if (act != null)
            {
                if (attack < owner.Attr.Attack1)
                {
                    //普通攻击
                    ActionNode attack1 = Main.Ins.ActionInterrupt.GetNormalNode(owner, act);
                    if (attack1 != null)
                    {
                        TryPlayWeaponPose(attack1.KeyMap);
                    }
                    else
                        return false;
                }
                else if (attack < (owner.Attr.Attack1 + owner.Attr.Attack2))
                {
                    //连招-有可能也是小绝招.如果怒气不足可能无法释放小绝.
                    List<ActionNode> attack2 = Main.Ins.ActionInterrupt.GetSlashNode(owner, act);
                    if (attack2.Count != 0)
                    {
                        TryPlayWeaponPose(attack2[Random.Range(0, attack2.Count)].KeyMap);
                    }
                    else
                        return false;
                }
                else if (attack < (owner.Attr.Attack1 + owner.Attr.Attack2 + owner.Attr.Attack3))
                {
                    //绝招.
                    //怒气不足时，可能无法释放出
                    if (owner.AngryValue == CombatData.ANGRYMAX)
                    {
                        ActionNode attack3 = Main.Ins.ActionInterrupt.GetSkillNode(owner, act);
                        if (attack3 != null)
                        {
                            TryPlayWeaponPose(attack3.KeyMap);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //无法释放大绝招，那么尝试释放连招或小绝招.
                        List<ActionNode> attack2 = Main.Ins.ActionInterrupt.GetSlashNode(owner, act);
                        if (attack2.Count != 0)
                        {
                            TryPlayWeaponPose(attack2[Random.Range(0, attack2.Count)].KeyMap);
                        }
                        else
                            return false;
                    }
                }
            }
        }
        return true;
    }

    //攻击目标位置
    public bool OnAttackTarget()
    {
        switch (SubStatus)
        {
            default:
            case EAISubStatus.AttackGotoTarget:
                if (Path.Count == 0)
                {
                    //Debug.LogError("攻击指定位置进入寻路");
                    ChangeState(EAIStatus.FindWay, EAIStatus.AttackTarget, EAISubStatus.AttackGotoTarget);
                    return false;
                }
                if (curIndex == -1)
                    targetIndex = 0;

                //已经到达过最后一个寻路点.
                if (targetIndex >= Path.Count)
                {
                    //检查这一帧是否会走过目标，因为跨步太大.【这一段有问题，只有离目标非常近的时候再判断才行，远的话，可能会绕路，导致下一帧距离目标的位置越来越远】
                    NextFramePos = AttackTarget - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    //33码距离内.
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
                    {
                        NextFramePos.y = 0;
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
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
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
                    {
                        NextFramePos.y = 0;
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
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
                    
                    //模拟跳跃键，移动到下一个位置.还得按住上
                    if (curIndex != -1)
                    {
                        if (Main.Ins.PathMng.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump)
                        {
                            if (owner.IsOnGround())
                            {
                                owner.FaceToTarget(Path[targetIndex].pos);
                                owner.controller.Input.AIMove(0, 0);
                                AIJump(Path[targetIndex].pos);
                                AIJumpDelay = 0.0f;
                                return false;
                            }
                        }
                        else
                        {
                            owner.FaceToTarget(Path[targetIndex].pos);
                            owner.controller.Input.AIMove(0, 1);
                        }
                    }
                    else
                    {
                        owner.FaceToTarget(Path[targetIndex].pos);
                        owner.controller.Input.AIMove(0, 1);
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

    void OnRebornFriend()
    {
        owner.posMng.ChangeAction(CommonAction.Reborn);
        ChangeState(EAIStatus.Wait);
        RebornTick = CombatData.RebornDelay;
    }

    //地面攻击.思考仅处理战斗状态
    float ThickTick = 0.0f;
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
        float disMin = owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
        if (dis <= disMin)
        {
            StopMove();
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
        {
            //Debug.LogError("战斗走向指定位置，路径为空进入寻路");
            ChangeState(EAIStatus.FindWay, EAIStatus.Fight, EAISubStatus.FightGotoPosition);
            return;
        }
        if (curIndex == -1)
            targetIndex = 0;

        if (targetIndex >= Path.Count)
        {
            NextFramePos = TargetPos - owner.mSkeletonPivot;
            NextFramePos.y = 0;
            if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
            {
                NextFramePos.y = 0;
                NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
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
            if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
            {
                NextFramePos.y = 0;
                NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
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
            
            //模拟跳跃键，移动到下一个位置.还得按住上
            if (curIndex != -1)
            {
                if (Main.Ins.PathMng.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump)
                {
                    if (owner.IsOnGround())
                    {
                        owner.FaceToTarget(Path[targetIndex].pos);
                        owner.controller.Input.AIMove(0, 0);
                        AIJump(Path[targetIndex].pos);
                        AIJumpDelay = 0.0f;
                        return;
                    }
                }
                else
                {
                    owner.FaceToTarget(Path[targetIndex].pos);
                    owner.controller.Input.AIMove(0, 1);
                }
            }
            else
            {
                owner.FaceToTarget(Path[targetIndex].pos);
                owner.controller.Input.AIMove(0, 1);
            }
        }
    }

    void OnFightGotoTarget()
    {
        if (Path.Count == 0)
        {
            //Debug.LogError("战斗走向目标，路径为空进入寻路");
            ChangeState(EAIStatus.FindWay, EAIStatus.Fight, EAISubStatus.FightGotoTarget);
            return;
        }

        //间隔一段时间，刷新下攻击目标的路点，如果跟随目标所在路点发生变化.
        //if (RefreshFollowPathTick <= 0.0f)
        //{
        //    int FollowTargetWayIndex = PathMng.Instance.GetWayIndex(fightTarget.mSkeletonPivot, fightTarget);
        //    RefreshFollowPathTick = Global.RefreshFollowPathDelay;
        //    if (FollowTargetWayIndex != lastFollowTargetIndex)
        //    {
        //        Path.Clear();
        //        Debug.LogError("目标角色所在路点刷新，重新计算寻路路径");
        //        ChangeState(EAIStatus.FindWay, EAIStatus.Fight, EAISubStatus.FightGotoTarget);
        //        return;
        //    }
        //}

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
            if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
            {
                owner.controller.Input.AIMove(0, 0);
                SubStatus = EAISubStatus.Fight;
                return;
            }
            else
            {
                owner.controller.Input.AIMove(0, 1);
            }
        }
        else
        if (targetIndex < Path.Count && Path.Count > 2)
        {
            NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
            NextFramePos.y = 0;
            //35码距离内.
            if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
            {
                NextFramePos.y = 0;
                NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
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
                if (Main.Ins.PathMng.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump)
                {
                    if (owner.IsOnGround())
                    {
                        owner.FaceToTarget(Path[targetIndex].pos);
                        owner.controller.Input.AIMove(0, 0);
                        AIJump(Path[targetIndex].pos);
                        AIJumpDelay = 0.0f;
                        return;
                    }
                }
            }
        }
        else if (Path.Count <= 1)
        {
            //直接面向目标,2者处于同一个路点.
            NextFramePos = fightTarget.mSkeletonPivot - owner.mSkeletonPivot;
            NextFramePos.y = 0;
            //33码距离内.
            if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
            {
                owner.controller.Input.AIMove(0, 0);
                SubStatus = EAISubStatus.Fight;
                return;
            }
            else
            {
                owner.controller.Input.AIMove(0, 1);
            }
        }

        if (targetIndex < Path.Count && Path.Count > 2)
            owner.FaceToTarget(Path[targetIndex].pos);
        else
        {
            NextFramePos = fightTarget.mSkeletonPivot - owner.mSkeletonPivot;
            NextFramePos.y = 0;
            //33码距离内.
            if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
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

        if (ThickTick > 0)
            return;
        //ThickTick = MeteorAI.ThinkRound;

        //控制火枪-飞镖射击频率,最迟要停一秒后才能再发射，远程武器
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
                        if (GetAngleBetween(fightTarget.transform.position) >= Main.Ins.CombatData.AimDegree)
                        {
                            //停止连击，方向需要调整
                            return;
                        }
                        
                    }
                    //近战武器，在可输入帧，每一帧30几率接前一招出招，如果这个输入帧大于50帧，基本就可以连上去
                    int chance = Random.Range(0, 100);
                    if (chance < Main.Ins.CombatData.ComboProbability)
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
                        if (breakOut < CombatData.BreakChange)
                            InputCorout = owner.StartCoroutine(VirtualKeyEvent(EKeyList.KL_BreakOut));
                    }
                }
            }
            else
            {
                //如果是远程武器，且距离上次攻击间隔不足一秒,返回等待下次
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
                if (dis > CombatData.AttackRange)
                {
                    //大部分几率是跑过去，走到跟前对打。
                    //小部分几率是切换远程武器打目标。
                    //1是否拥有远程武器
                    if (U3D.IsSpecialWeapon(owner.Attr.Weapon))
                    {
                        //主手是远程武器-.直接攻击,不处理，后续会处理到底是打还是啥.
                        //站撸
                        if (GetAngleBetween(fightTarget.mSkeletonPivot) > Main.Ins.CombatData.AimDegree)
                        {
                            SubStatus = EAISubStatus.FightAim;
                            return;
                        }
                    }
                    else if (U3D.IsSpecialWeapon(owner.Attr.Weapon2))
                    {
                        //副手是远程武器-.一定几率切换武器，再攻击.90几率切换到远程武器
                        int random = Random.Range(0, 100);
                        if (random > Main.Ins.CombatData.SpecialWeaponProbability)
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
                        if (GetAngleBetween(fightTarget.mSkeletonPivot) > Main.Ins.CombatData.AimDegree)
                        {
                            SubStatus = EAISubStatus.FightAim;
                            return;
                        }
                    }
                    else if (owner.Attr.Weapon2 != 0 && !U3D.IsSpecialWeapon(owner.Attr.Weapon2))//主手为远程武器，副手有武器且不为远程武器
                    {
                        //主手远程武器,副手近战武器
                        //1切换为近战武器，2跑远
                        int random = Random.Range(1, 100);
                        int limit = 2;//%2几率跑远
                        if (random <= limit)
                        {
                            SubStatus = (EAISubStatus.FightLeave);
                            return;
                        }
                        //98%几率换武器打
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
                        if (dis < CombatData.StopDistance)
                        {
                            SubStatus = EAISubStatus.FightLeave;
                            return;
                        }

                        //角度相差太大，重新瞄准
                        if (GetAngleBetween(fightTarget.mSkeletonPivot) > Main.Ins.CombatData.AimDegree)
                        {
                            SubStatus = EAISubStatus.FightAim;
                            return;
                        }
                    }
                }
                //使用近战武器，一定在攻击范围内。一次骰子判断所有概率.
                int attack = Random.Range(0, 100);
                bool attacked = false;
                bool doother = false;
                if (attack < owner.Attr.Attack1)
                {
                    //判定攻击
                    //if (fightTarget == null)
                    //    Debug.LogError("fightTarget == null");
                    attacked = TryAttack(attack);
                }
                else
                {
                    //攻击的几率分开算，不能合并到一起，否则攻击几率太大.
                    if (attack < owner.Attr.Attack1 + owner.Attr.Attack2)
                    {
                        attacked = TryAttack(attack);
                    }
                    else
                    {
                        if (attack < owner.Attr.Attack1 + owner.Attr.Attack2 + owner.Attr.Attack3)
                        {
                            attacked = TryAttack(attack);
                        }
                        else
                        {
                            //判定除了攻击外的其他动作概率.
                            DoOtherAction();
                            doother = false;
                        }
                    }
                }

                if (!attacked && !doother)
                    DoOtherAction();
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

    //除了攻击以外的概率,此刻一定在地面
    private void DoOtherAction()
    {
        int defence = Random.Range(0, 100);
        if (!U3D.IsSpecialWeapon(owner.Attr.Weapon) && defence < owner.Attr.Guard)
        {
            //远程武器无法防御.
            StopMove();
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
                int burst = Random.Range(0, 100);
                //判断速冲.
                if (burst < owner.Attr.Burst)
                    AIBurst((EKeyList.KL_KeyA) + defence % 4);
                else
                {
                    int pickup = Random.Range(0, 100);
                    if (pickup < owner.Attr.GetItem)
                    {
                        if (owner.TargetItem != null && owner.TargetItem.CanPickup())
                            ChangeState(EAIStatus.GetItem);
                    }
                }
            }
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

        if (GetAngleBetween(fightTarget.mSkeletonPivot) > Main.Ins.CombatData.AimDegree)
        {
            if (AttackRotateToTargetCoroutine == null)
            {
                //owner.posMng.ChangeAction(0);
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
    float touchLast = 0.0f;
    const float touchWallLimit = 1.0f;
    public void CheckStatus()
    {
        if (touchLast >= touchWallLimit)
        {
            //Debug.Log("长时间靠墙壁引发完全重新寻路.");
            if (Status == EAIStatus.Fight && (SubStatus == EAISubStatus.FightGotoPosition || SubStatus == EAISubStatus.FightGotoTarget))
            {
                StopCoroutine();
                StopMove();
                ResetAIKey();
                ChangeState(EAIStatus.Wait);
                Path.Clear();
            }
            else if (Status == EAIStatus.Follow)
            {
                StopCoroutine();
                StopMove();
                ResetAIKey();
                ChangeState(EAIStatus.Wait);
                Path.Clear();
            }
            else if (Status == EAIStatus.Patrol)
            {
                StopCoroutine();
                StopMove();
                ResetAIKey();
                ChangeState(EAIStatus.Wait);
                curIndex = -1;
                targetIndex = -1;
            }
            else if (Status == EAIStatus.GetItem)
            {
                StopCoroutine();
                StopMove();
                ResetAIKey();
                ChangeState(EAIStatus.Wait);
                Path.Clear();
            }
            else if (Status == EAIStatus.AttackTarget)
            {
                StopCoroutine();
                StopMove();
                ResetAIKey();
                ChangeState(EAIStatus.Wait);
                Path.Clear();
            }
            touchLast = 0.0f;
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
            StopMove();
            TargetPos = Main.Ins.PathMng.GetNearestWayPoint(fightTarget.mSkeletonPivot);
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

    Coroutine RefreshPathCoroutine;
    void RefreshPath(Vector3 now, Vector3 target)
    {
        if (Path.Count == 0)
        {
            if (RefreshPathCoroutine == null)
            {
                //RefreshPathCoroutine = Main.Ins.StartCoroutine(Main.Ins.PathMng.FindPath(owner, now, target, Path));
                curIndex = -1;
                targetIndex = -1;
            }
            else
            {
                Debug.Log("寻路中");
            }
        }
        else
        {

        }
    }

    void RefreshPathEx(MeteorUnit target)
    {
        if (Path.Count == 0)
        {
            if (RefreshPathCoroutine == null)
            {
                //RefreshPathCoroutine = Main.Ins.StartCoroutine(Main.Ins.PathMng.FindPathEx(owner, target, Path));
                curIndex = -1;
                targetIndex = -1;
            }
            else
            {
                Debug.Log("寻路中");
            }
        }
        else
        {

        }
    }

    //在其他角色上使用的插槽位置，
    Dictionary<MeteorUnit, int> SlotCache = new Dictionary<MeteorUnit, int>();
    Vector3 vecTarget;
    //GameObject[] Pos = new GameObject[100];
    //GameObject[] debugPos = new GameObject[100];
    public int curIndex = 0;
    public int targetIndex = 0;

    void OnKillThink()
    {
        if (killTarget == null || killTarget.Dead)
        {
            ChangeState(EAIStatus.Wait);
            return;
        }

        if (Vector3.SqrMagnitude(killTarget.mSkeletonPivot - owner.mSkeletonPivot) >= CombatData.AttackRange)
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
                if (dis < CombatData.FollowDistanceEnd)
                {
                    owner.controller.Input.AIMove(0, 0);
                    ChangeState(EAIStatus.Wait);
                    return;
                }

                if (Path.Count == 0)
                {
                    //Debug.LogError("跟随目标，距离足够大，路径为空进入寻路");
                    ChangeState(EAIStatus.FindWay, EAIStatus.Follow, EAISubStatus.FollowGotoTarget);
                    return;
                }

                //间隔一段时间，刷新下跟随目标的路点，如果跟随目标所在路点发生变化.
                //if (RefreshFollowPathTick <= 0.0f)
                //{
                //    int FollowTargetWayIndex = PathMng.Instance.GetWayIndex(followTarget.mSkeletonPivot, followTarget);
                //    RefreshFollowPathTick = Global.RefreshFollowPathDelay;
                //    if (FollowTargetWayIndex != lastFollowTargetIndex)
                //    {
                //        Path.Clear();
                //        Debug.LogError("目标角色所在路点刷新，重新计算寻路路径");
                //        ChangeState(EAIStatus.FindWay, EAIStatus.Follow, EAISubStatus.FollowGotoTarget);
                //        return;
                //    }
                //}

                if (targetIndex >= Path.Count)
                {
                    owner.FaceToTarget(followTarget.mSkeletonPivot);
                    owner.controller.Input.AIMove(0, 1);
                }
                else
                {
                    if (curIndex == -1)
                        targetIndex = 0;

                    if (targetIndex < 0 || targetIndex >= Path.Count)
                    {
                        //Debug.LogError("follow path idx error");
                        return;
                    }
                    //检查这一帧是否会走过目标，因为跨步太大.
                    NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    //33码距离内.
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
                    {
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
                        float s = GetAngleBetween((NextFramePos - owner.mSkeletonPivot).normalized, (Path[targetIndex].pos - NextFramePos).normalized);
                        //方向相反，表面下一帧内即可到达目标路点
                        if (s < 0)
                        {
                            //到达后转向下一个路点.
                            owner.controller.Input.AIMove(0, 0);
                            curIndex = targetIndex;
                            targetIndex += 1;
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.FollowSubRotateToTarget;//到指定地点后旋转到下个目标.
                            return;
                        }
                    }

                    //模拟跳跃键，移动到下一个位置.还得按住上
                    if (curIndex != -1)
                    {
                        if (Main.Ins.PathMng.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump)
                        {
                            if (owner.IsOnGround())
                            {
                                owner.FaceToTarget(Path[targetIndex].pos);
                                owner.controller.Input.AIMove(0, 0);
                                AIJump(Path[targetIndex].pos);
                                AIJumpDelay = 0.0f;
                                return;
                            }
                        }
                        else
                        {
                            owner.FaceToTarget(Path[targetIndex].pos);
                            owner.controller.Input.AIMove(0, 1);
                        }
                    }
                    else
                    {
                        owner.FaceToTarget(Path[targetIndex].pos);
                        owner.controller.Input.AIMove(0, 1);
                    }
                    break;
                }
                break;
            case EAISubStatus.FollowSubRotateToTarget:
                if (targetIndex >= Path.Count || targetIndex < 0)
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
                    killTarget = owner.GetKillTarget();
                if (killTarget != null)
                {
                    float dis = 0.0f;
                    dis = Vector3.SqrMagnitude(owner.mSkeletonPivot - killTarget.mSkeletonPivot);
                    if (dis < CombatData.AttackRange)//小于50码停止跟随，不需要计算路径
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
                    if (Vector3.SqrMagnitude(vec) < CombatData.AttackRange)//小于50码停止跟随，不需要计算路径
                    {
                        owner.controller.Input.AIMove(0, 0);
                        ChangeState(EAIStatus.Wait);//开始寻找敌人
                        return;
                    }

                    if (Path.Count == 0)
                    {
                        //Debug.LogError("战斗-追杀敌人，路径为空进入寻路");
                        ChangeState(EAIStatus.FindWay, EAIStatus.Kill, EAISubStatus.KillGotoTarget);
                        return;
                    }
                    if (curIndex == -1)
                        targetIndex = 0;

                    //已经到达过最后一个寻路点.
                    if (targetIndex < Path.Count && Path.Count > 1)
                    {
                        //检查这一帧是否会走过目标，因为跨步太大.【这一段有问题，只有离目标非常近的时候再判断才行，远的话，可能会绕路，导致下一帧距离目标的位置越来越远】
                        NextFramePos = Path[targetIndex].pos - owner.mSkeletonPivot;
                        NextFramePos.y = 0;
                        //33码距离内.
                        if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
                        {
                            NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
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

                        
                        //模拟跳跃键，移动到下一个位置.还得按住上
                        if (curIndex != -1)
                        {
                            if (Main.Ins.PathMng.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump)
                            {
                                if (owner.IsOnGround())
                                {
                                    owner.FaceToTarget(Path[targetIndex].pos);
                                    owner.controller.Input.AIMove(0, 0);
                                    AIJump(Path[targetIndex].pos);
                                    AIJumpDelay = 0.0f;
                                    return;
                                }
                            }
                            else
                            {
                                owner.FaceToTarget(Path[targetIndex].pos);
                                owner.controller.Input.AIMove(0, 1);
                            }
                        }
                        else
                        {
                            owner.FaceToTarget(Path[targetIndex].pos);
                            owner.controller.Input.AIMove(0, 1);
                        }
                    }
                    else
                    {
                        NextFramePos = killTarget.mSkeletonPivot - owner.mSkeletonPivot;
                        NextFramePos.y = 0;
                        if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
                        {
                            NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
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
                    if (targetIndex >= Path.Count || targetIndex < 0)
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
        MeteorUnit want = U3D.GetUnit(target);
        if (Main.Ins.MeteorManager.LeavedUnits.ContainsKey(target))
            return;
        followTarget = want;
        ChangeState(EAIStatus.Follow);
        SubStatus = EAISubStatus.FollowGotoTarget;
    }

    int waitStruggleDone = 0;//每次挣扎给10帧，10帧如果未完成此步骤，则重试
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
        int k = Random.Range(0, struggleKey.Length);
        while (true)
        {
            yield return 0;
            owner.controller.Input.OnKeyDownProxy(struggleKey[k], true);
            yield return 0;
            owner.controller.Input.OnKeyUpProxy(struggleKey[k]);
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
        //当动作不是待机时，不许进入四处看之类的状态.
        if (owner.posMng.mActiveAction.Idx != CommonAction.Idle)
            return;
        if (lookTick >= 3.0f)
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
        idleTick += FrameReplay.deltaTime;
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
            StopMove();
            StopCoroutine();
        }
        //Debug.Log(string.Format("unit:{0} pause:{1}", owner.name, pause_tick));
    }

    public void EnableAI(bool enable)
    {
        stoped = !enable;
        if (stoped)
        {
            StopMove();
            StopCoroutine();
        }
    }

    //改变主状态,后者决定当前状态结束后，是否转到后者
    public void ChangeState(EAIStatus type, EAIStatus onComplete = EAIStatus.Wait, EAISubStatus onCompleteSub = EAISubStatus.None)
    {
        Status = type;
        StopCoroutine();
        StopMove();
        ResetAIKey();
        //Path.Clear();
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
            owner.controller.Input.OnKeyDownProxy(EKeyList.KL_Defence, true);//防御
        }
        else if (type == EAIStatus.AttackTarget)
        {
            //朝目标处攻击数次
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
        else if (type == EAIStatus.RebornFriend)
            SubStatus = EAISubStatus.SubStatusWait;
        else if (type == EAIStatus.FindWay)
        {
            SubStatus = EAISubStatus.StartFind;
            SubStatusOnComplete = onCompleteSub;
            StatusOnComplete = onComplete;
            Path.Clear();
            FindWayFinished = false;
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

        if (RefreshPathCoroutine != null)
        {
            Main.Ins.StopCoroutine(RefreshPathCoroutine);
            RefreshPathCoroutine = null;
        }
    }

    public void ResetAIKey()
    {
        owner.controller.Input.OnKeyUpProxy(EKeyList.KL_Defence);
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
        owner.controller.Input.OnKeyDownProxy(key, true);
        yield return 0;
        yield return 0;
        owner.controller.Input.OnKeyUpProxy(key);
        yield return 0;
        yield return 0;
        owner.controller.Input.OnKeyDownProxy(key, true);
        yield return 0;
        yield return 0;
        owner.controller.Input.OnKeyUpProxy(key);
        yield return 0;
        yield return 0;
        Burst = null;
    }

    //单键输入.
    IEnumerator VirtualKeyEvent(EKeyList key)
    {
        owner.controller.Input.OnKeyDownProxy(key, true);
        yield return 0;
        owner.controller.Input.OnKeyUpProxy(key);
        yield return 0;
        InputCorout = null;
    }

    Coroutine AttackTargetCoroutine;
    IEnumerator Attack()
    {
        owner.controller.Input.OnKeyDownProxy(EKeyList.KL_Attack, true);
        yield return 0;
        yield return 0;
        owner.controller.Input.OnKeyUpProxy(EKeyList.KL_Attack);
        yield return 0;
        yield return 0;
        AttackTargetCoroutine = null;
    }

    IEnumerator PlayWeaponPose(int KeyMap)
    {
        List<VirtualInput> skill = VirtualInput.CalcPoseInput(KeyMap);
        for (int i = 0; i < skill.Count; i++)
        {
            owner.controller.Input.OnKeyDownProxy(skill[i].key, true);
            yield return 0;
            owner.controller.Input.OnKeyUpProxy(skill[i].key);
            yield return 0;
        }
        PlayWeaponPoseCorout = null;
    }

    //如果attacker为空，则代表是非角色伤害了自己
    int lastCombo = 0;//最后谁一直打我，会导致我切换过去打他.
    public void OnDamaged(MeteorUnit attacker)
    {
        //寻路数据要清空.
        Path.Clear();
        if (attacker == null)
        {

        }
        else
        {
            if (lastAttacker != attacker)
            {
                //我在和A战斗，但这时B打我
                if (owner.LockTarget != attacker)
                    lastAttacker = attacker;
                lastCombo = 0;
            }
            else
            {
                //遭遇同一个角色攻击数次,会导致切换目标.
                lastCombo++;
                if (lastCombo >= 3 && owner.LockTarget != lastAttacker)
                {
                    //切换目标规则是同一个敌方攻击你5次
                    owner.SetLockedTarget(attacker);
                }
            }
        }

        StopMove();
        ResetAIKey();
        StopCoroutine();
        owner.controller.Input.ResetInput();
        if (attacker != null)
        {
            //攻击者在视野内，切换为战斗姿态，否则
            if (owner.Find(attacker))
            {
                //如果当前有攻击目标，是否切换目标
                if (owner.LockTarget == null)
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
    List<WayPoint> PatrolTemp = new List<WayPoint>();
    List<WayPoint> PatrolTemp2 = new List<WayPoint>();
    public List<int> patrolData = new List<int>();
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

    //绕原地
    Coroutine PatrolRotateCoroutine;//巡逻到达某个目的点后，会随机旋转1-5次，每次都会停留不固定的时间
    IEnumerator PatrolRotate(float angleSpeed = (CombatData.AngularVelocity / 2))
    {
        float rotateAngle1 = Random.Range(-90, -45);
        float rotateAngle2 = Random.Range(45, 90);
        bool right = Random.Range(0, 100) >= 50;
        float rotateAngle = right ? rotateAngle2 : rotateAngle1;
        //Quaternion quat = Quaternion.AngleAxis(rotateAngle, Vector3.up);
        float offset = 0.0f;
        float timeTotal = Mathf.Abs(rotateAngle / angleSpeed);
        float timeTick = 0.0f;
        while (true)
        {
            timeTick += FrameReplay.deltaTime;
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
            wait -= FrameReplay.deltaTime;
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
        if (vec.x == owner.transform.position.x && vec.z == owner.transform.position.z)
            return 0;

        Vector3 vec1 = -owner.transform.forward;
        Vector3 vec2 = (vec - owner.mPos2d).normalized;
        vec2.y = 0;
        float radian = Vector3.Dot(vec1, vec2);
        float degree = Mathf.Acos(Mathf.Clamp(radian, -1.0f, 1.0f)) * Mathf.Rad2Deg;
        //Debug.LogError("夹角:" + degree);
        //if (float.IsNaN(degree))
        //    Debug.LogError("NAN");
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
        if (vec.x == owner.transform.position.x && vec.z == owner.transform.position.z)
        {
            GetItemRotateToTargetCoroutine = null;
            SubStatus = EAISubStatus.GetItemGotoItem;
            yield break;
        }
        Vector3 diff = (vec - owner.transform.position);
        diff.y = 0;
        //float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        //float angle = Mathf.Abs(Mathf.Acos(Mathf.Clamp(dot, -1.0f, 1.0f)) * Mathf.Rad2Deg);
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / (CombatData.AngularVelocity / 2);
        float timeTick = 0.0f;
        
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += FrameReplay.deltaTime;
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
        if (vec.x == owner.transform.position.x && vec.z == owner.transform.position.z)
        {
            AttackRotateToTargetCoroutine = null;
            SubStatus = subStatus;
            yield break;
        }
        Vector3 diff = (vec - owner.transform.position);
        diff.y = 0;
        //float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        //float angle = Mathf.Abs(Mathf.Acos(Mathf.Clamp(dot, -1.0f, 1.0f)) * Mathf.Rad2Deg);
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / CombatData.AngularVelocity;
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += FrameReplay.deltaTime;
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
        if (vec.x == owner.transform.position.x && vec.z == owner.transform.position.z)
        {
            FollowRotateToTargetCoroutine = null;
            SubStatus = onFinishSubStatus;
            yield break;
        }
        Vector3 diff = (vec - owner.transform.position);
        diff.y = 0;
        //float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        //float angle = Mathf.Abs(Mathf.Acos(Mathf.Clamp(dot, -1.0f, 1.0f)) * Mathf.Rad2Deg);
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / CombatData.AngularVelocity;
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += FrameReplay.deltaTime;
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
        if (vec.x == owner.transform.position.x && vec.z == owner.transform.position.z)
        {
            FollowRotateToTargetCoroutine = null;
            SubStatus = onFinishSubStatus;
            yield break;
        }
        Vector3 diff = (vec - owner.transform.position);
        diff.y = 0;
        //float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        //float angle = Mathf.Abs(Mathf.Acos(Mathf.Clamp(dot, -1.0f, 1.0f)) * Mathf.Rad2Deg);
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / CombatData.AngularVelocity;//转速快一点，否则感觉AI很弱智
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += FrameReplay.deltaTime;
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
        if (vec.x == owner.transform.position.x && vec.z == owner.transform.position.z)
        {
            FollowRotateToTargetCoroutine = null;
            SubStatus = onFinishSubStatus;
            yield break;
        }
        Vector3 diff = (vec - owner.transform.position);
        diff.y = 0;
        //float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        //float angle = Mathf.Abs(Mathf.Acos(Mathf.Clamp(dot, -1.0f, 1.0f)) * Mathf.Rad2Deg);
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / (CombatData.AngularVelocity / 2);
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += FrameReplay.deltaTime;
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
        if (vec.x == owner.transform.position.x && vec.z == owner.transform.position.z)
        {
            FollowRotateToTargetCoroutine = null;
            SubStatus = onFinishSubStatus;
            yield break;
        }
        Vector3 diff = (vec - owner.transform.position);
        diff.y = 0;
        //float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        //float angle = Mathf.Abs(Mathf.Acos(Mathf.Clamp(dot, -1.0f, 1.0f)) * Mathf.Rad2Deg);
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / (CombatData.AngularVelocity / 2);
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += FrameReplay.deltaTime;
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
        if (vec.x == owner.transform.position.x && vec.z == owner.transform.position.z)
        {
            PatrolRotateToTargetCoroutine = null;
            SubStatus = EAISubStatus.PatrolSubGotoTarget;
            yield break;
        }
        //WsGlobal.AddDebugLine(vec, vec + Vector3.up * 10, Color.red, "PatrolPoint", 20.0f);
        Vector3 diff = (vec - owner.transform.position);
        diff.y = 0;
        //float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        //float angle = Mathf.Abs(Mathf.Acos(Mathf.Clamp(dot, -1.0f, 1.0f)) * Mathf.Rad2Deg);
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / (CombatData.AngularVelocity / 2);
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += FrameReplay.deltaTime;
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
        if (vec.x == owner.transform.position.x && vec.z == owner.transform.position.z)
        {
            PatrolRotateToPatrolPointCoroutine = null;
            SubStatus = EAISubStatus.PatrolGotoFirstPoint;
            yield break;
        }
        //WsGlobal.AddDebugLine(vec, vec + Vector3.up * 10, Color.red, "PatrolPoint", 20.0f);
        Vector3 diff = (vec - owner.transform.position);
        diff.y = 0;
        //float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        //float angle = Mathf.Abs(Mathf.Acos(Mathf.Clamp(dot, -1.0f, 1.0f)) * Mathf.Rad2Deg);
        bool rightRotate = dot2 > 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(vec);
        float timeTotal = offsetmax / (CombatData.AngularVelocity / 2);
        float timeTick = 0.0f;
        while (true)
        {
            float yOffset = 0.0f;
            timeTick += FrameReplay.deltaTime;
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
        int k = Main.Ins.PathMng.GetWayIndex(owner.mSkeletonPivot);
        if (k == -1)
            return -1;
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
                    if (PatrolPath.Count == 0)
                    {
                        //Debug.LogError("巡逻，路径为空进入寻路");
                        ChangeState(EAIStatus.FindWay, EAIStatus.Patrol, EAISubStatus.Patrol);
                        return;
                    }

                    int k = GetPatrolIndex();
                    if (k == -1)
                    {
                        int n = GetNearestPatrolPoint();
                        targetPatrolIndex = n;//目的地
                        //当不在任何一个巡逻点中时，跑到第一个巡逻点的过程
                        //SubStatus = EAISubStatus.PatrolGotoFirstPoint;
                        //Debug.LogError("巡逻，从当前位置走到第一个巡逻点寻路");
                        ChangeState(EAIStatus.FindWay, EAIStatus.Patrol, EAISubStatus.PatrolGotoFirstPoint);
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
                
                //模拟跳跃键，移动到下一个位置.还得按住上
                if (curPatrolIndex != -1)
                {
                    if (Main.Ins.PathMng.GetWalkMethod(PatrolPath[curPatrolIndex].index, PatrolPath[targetPatrolIndex].index) == WalkType.Jump)
                    {
                        if (owner.IsOnGround())
                        {
                            owner.FaceToTarget(PatrolPath[targetPatrolIndex].pos);
                            owner.controller.Input.AIMove(0, 0);
                            AIJump(PatrolPath[targetPatrolIndex].pos);
                            AIJumpDelay = 0.0f;
                        }
                    }
                    else
                    {
                        owner.FaceToTarget(PatrolPath[targetPatrolIndex].pos);
                        owner.controller.Input.AIMove(0, 1);
                    }
                }
                else
                {
                    owner.FaceToTarget(PatrolPath[targetPatrolIndex].pos);
                    owner.controller.Input.AIMove(0, 1);
                }
                break;
            case EAISubStatus.PatrolGotoFirstPoint:
                if (curIndex == -1)
                    targetIndex = 0;

                if (targetIndex >= Path.Count)
                {
                    NextFramePos = PatrolPath[targetPatrolIndex].pos - owner.mSkeletonPivot;
                    NextFramePos.y = 0;
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
                    {
                        NextFramePos.y = 0;
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
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
                    if (Vector3.SqrMagnitude(NextFramePos) <= CombatData.StopDistance)
                    {
                        NextFramePos = owner.mSkeletonPivot + NextFramePos.normalized * owner.MoveSpeed * FrameReplay.deltaTime * 0.15f;
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

                    //模拟跳跃键，移动到下一个位置.还得按住上
                    if (curIndex != -1)
                    {
                        if (Main.Ins.PathMng.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump)
                        {
                            if (owner.IsOnGround())
                            {
                                owner.FaceToTarget(Path[targetIndex].pos);
                                owner.controller.Input.AIMove(0, 0);
                                AIJump(Path[targetIndex].pos);
                                AIJumpDelay = 0.0f;
                                return;
                            }
                        }
                        else
                        {
                            owner.FaceToTarget(Path[targetIndex].pos);
                            owner.controller.Input.AIMove(0, 1);
                        }
                    }
                    else
                    {
                        owner.FaceToTarget(Path[targetIndex].pos);
                        owner.controller.Input.AIMove(0, 1);
                    }
                }
                break;
            case EAISubStatus.PatrolSubInPlace:
                if (PatrolRotateCoroutine == null)
                    PatrolRotateCoroutine = owner.StartCoroutine(PatrolRotate(90.0f));
                break;
        }
    }

    void AIJump2()
    {

    }
    /// <summary>
    /// 寻路中模拟跳跃.跳到指定位置
    /// </summary>
    /// 不要随便关掉owner，否则其上所有协程都会失效.
    void AIJump(Vector3 vec)
    {
        float height = vec.y - owner.transform.position.y;
        vec.y = 0;
        Vector3 vec2 = owner.transform.position;
        vec2.y = 0;
        float sz = Vector3.Distance(vec, vec2);
        owner.Jump2(Mathf.Abs(height));
        owner.SetVelocity(sz / (2 * owner.ImpluseVec.y / Main.Ins.CombatData.gGravity), 0);
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

//AI的输入队列，放到StateMachine中，所有状态公用
public class AIVirtualInput
{
    public AIVirtualInput(EKeyList k)
    {
        key = k;
        state = 0;
    }
    public EKeyList key;
    public byte state;//0无状态 1按下 2抬起-完成
}

public class VirtualInput
{
    public EKeyList key;
    static bool InGroup(int[] group, int target)
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