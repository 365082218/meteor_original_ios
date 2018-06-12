using UnityEngine;
using System.Collections.Generic;
using System.Collections;

//大状态
public enum EAIStatus
{
    Idle,//空闲->决策下一步，或继续空闲.
    Kill,//与目标近距离对打
    Guard,//防御
    Follow,//跟随
    Aim,//远程武器瞄准
    Think,//没发觉目标时左右观察
    Patrol,//巡逻。
    Wait,//脚本：等待
}

public enum EAISubStatus
{
    Patrol = EAIStatus.Patrol,
    PatrolSubRotateInPlace,//原地随机旋转.
    PatrolSubRotateToTarget,//原地一定时间内旋转到指定方向
    PatrolSubGotoTarget,//跑向指定位置
    KillGetTarget,//离角色很近了，可以攻击
    KillGotoTarget,//离角色一定距离，需要先跑过去
    KillOnHurt,//被敌人击中
    Think,//思考
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

public class EAIStatusChange
{
    public EAIStatus type;
    public float last;
    public EAIStatusChange(EAIStatus t, float l)
    {
        type = t;
        last = l;
    }
}

public class MeteorAI {
    public MeteorAI(MeteorUnit user)
    {
        owner = user;
        Status = EAIStatus.Wait;
    }
    public EAIStatus Status { get; set; }
    public EAISubStatus SubStatus { get; set; }
    MeteorUnit owner;
    MeteorUnit followTarget;
    MeteorUnit killTarget;
    bool stoped = false;
    bool paused = false;
    float pause_tick;
    public Dictionary<int, AISet> AIData;
    public List<EAIStatusChange> StateStack = new List<EAIStatusChange>();
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
        //return;
        if (paused)
        {
            pause_tick -= Time.deltaTime;
            if (pause_tick <= 0.0f)
                paused = false;
            return;
        }

        //if (true)
        //{
        //    if (owner.posMng.mActiveAction.Idx != CommonAction.BreakOut)
        //    {
        //        owner.AngryValue = 100;
        //        owner.DoBreakOut();
        //    }
        //    return;
        //}
        if (StateStack.Count != 0)
        {
            StateStack[StateStack.Count - 1].last -= Time.deltaTime;
            if (StateStack[StateStack.Count - 1].last <= 0.0f)
            {
                StateStack.RemoveAt(StateStack.Count - 1);
                if (StateStack.Count == 0)
                {
                    ResetAIKey();
                    if (owner.GetLockedTarget() != null)
                    {
                        killTarget = owner.GetLockedTarget();
                        Status = EAIStatus.Kill;
                        SubStatus = EAISubStatus.KillGotoTarget;
                    }
                    else if (PatrolPath.Count != 0)
                    {
                        Status = EAIStatus.Patrol;
                        SubStatus = EAISubStatus.Patrol;
                    }
                    else
                        Status = EAIStatus.Wait;
                }
            }
            else
                Status = StateStack[StateStack.Count - 1].type;
        }

        switch (Status)
        {
            case EAIStatus.Idle:
                OnIdle();
                break;
            case EAIStatus.Guard:
                owner.controller.Input.OnKeyDown(EKeyList.KL_Defence, true);//防御
                break;
            case EAIStatus.Wait:
                //Debug.LogError("wait");
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
                            SubStatus = EAISubStatus.Think;
                        }
                        break;
                    case EAISubStatus.KillGetTarget:
                        
                        OnIdle();
                        break;
                    case EAISubStatus.KillOnHurt:
                        OnHurt();
                        break;
                }
                break;
        }
    }

    //在其他角色上使用的插槽位置，
    Dictionary<MeteorUnit, int> SlotCache = new Dictionary<MeteorUnit, int>();
    Vector3 vecEnd;
    Vector3 vecStart;

    GameObject[] Pos = new GameObject[100];
    GameObject[] debugPos = new GameObject[100];
    int curIndex = 0;
    void MovetoTarget(MeteorUnit target)
    {
        //开始寻路，找到目标与之最近的路点，从当前点如何到达-目标点
        int freeSlot = -1;
        if (true)
        {
            List<WayPoint> path = GameBattleEx.Instance.FindPath(owner.mPos, owner, target, out freeSlot, out vecEnd);
            if (path.Count == 1)
            {
                vecStart = vecEnd;
                if (Pos[0] == null)
                    Pos[0] = GameObject.Instantiate(Resources.Load("FreePos"), vecStart, Quaternion.identity, null) as GameObject;
                Pos[0].transform.position = vecStart;
                if (debugPos[0] == null)
                    debugPos[0] = WsGlobal.AddDebugLine(vecStart, vecStart + Vector3.up * 20, Color.red, string.Format("{0} pos:{1}", owner.name, 0));
                debugPos[0].transform.position = vecStart;
                LineRenderer l = debugPos[0].GetComponent<LineRenderer>();
                l.SetPosition(0, vecStart);
                l.SetPosition(1, vecStart + Vector3.up * 20);
            }
            else if (path.Count > 1)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    if (Pos[i] == null)
                        Pos[i] = GameObject.Instantiate(Resources.Load("FreePos"), path[i].pos, Quaternion.identity, null) as GameObject;
                    if (path[i] == null || Pos[i] == null)
                        Debug.LogError("error");
                    Pos[i].transform.position = path[i].pos;
                    if (debugPos[i] == null)
                        debugPos[i] = WsGlobal.AddDebugLine(path[i].pos, path[i].pos + Vector3.up * 20, Color.red, string.Format("{0} pos:{1}", owner.name, i));
                    debugPos[i].transform.position = path[i].pos;
                    LineRenderer le = debugPos[i].GetComponent<LineRenderer>();
                    le.SetPosition(0, path[i].pos);
                    le.SetPosition(1, path[i].pos + Vector3.up * 20);
                }

                //最后一个点
                if (Pos[path.Count] == null)
                    Pos[path.Count] = GameObject.Instantiate(Resources.Load("FreePos"), vecEnd, Quaternion.identity, null) as GameObject;
                Pos[path.Count].transform.position = vecEnd;
                if (debugPos[path.Count] == null)
                    debugPos[path.Count] = WsGlobal.AddDebugLine(vecEnd, vecEnd + Vector3.up * 20, Color.red, string.Format("{0} pos:{1}", owner.name, path.Count));
                debugPos[path.Count].transform.position = vecEnd + Vector3.up * 20;
                LineRenderer le2 = debugPos[path.Count].GetComponent<LineRenderer>();
                le2.SetPosition(0, vecEnd);
                le2.SetPosition(1, vecEnd + Vector3.up * 20);
                if (curIndex == path.Count)
                {
                    vecStart = vecEnd;
                }
                else if (curIndex < path.Count)
                {
                    float dismin = Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(path[curIndex].pos.x, 0, path[curIndex].pos.z));
                    if (dismin <= 5)
                        curIndex++;
                    vecStart = path[curIndex].pos;
                }
            }

            //成功找到路径.且占据了目标上的一个位置槽
            if (freeSlot != -1)
            {
                if (SlotCache.ContainsKey(target))
                    SlotCache[target] = freeSlot;
                else
                    SlotCache.Add(target, freeSlot);
                SubStatus = EAISubStatus.KillGotoTarget;
            }
        }

        owner.FaceToTarget(vecStart);
        float dis = Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(vecEnd.x, 0, vecEnd.z));
        if (dis <= 35)//小于35码停止跟随
        {
            owner.controller.Input.AIMove(0, 0);
            if (Status == EAIStatus.Kill)
                SubStatus = EAISubStatus.KillGetTarget;
            else if (Status == EAIStatus.Follow)
            {
                Status = EAIStatus.Idle;
                SubStatus = EAISubStatus.Think;
            }
            return;
        }
        else if (dis >= 50)//距离大于50码开始跟随移动
        {
            if (SubStatus == EAISubStatus.KillGetTarget)
                SubStatus = EAISubStatus.KillGotoTarget;
            owner.controller.Input.AIMove(0, 1);
        }
    }

    public void FollowTarget(int target)
    {
        followTarget = U3D.GetUnit(target);
        ChangeState(EAIStatus.Follow, 1000.0f);
    }
    //防御时遭到攻击，也有防御动作
    void OnDefencePlaying()
    {

    }

    Coroutine struggleCoroutine;
    void OnHurt()
    {
        owner.controller.Input.ResetInput();
        if (owner.posMng.mActiveAction.Idx == CommonAction.Struggle || owner.posMng.mActiveAction.Idx == CommonAction.Struggle0)
        {
            if (struggleCoroutine == null && !owner.charLoader.InStraight)
                struggleCoroutine = owner.StartCoroutine(ProcessStruggle());
        }
        SubStatus = EAISubStatus.KillGotoTarget;
    }

    IEnumerator ProcessStruggle()
    {
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
            followTarget = null;
    }

    //更新路径间隔
    const float updateDelta = 1.0f;
    float tick = 0.0f;
    float waitCrouch;
    float waitDefence;
    void OnIdle()
    {
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
        switch (SubStatus)
        {
            case EAISubStatus.Think://采取一个什么行动,朝目标丢招式,转向目标
                
                break;
        }
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
                    TryPlayWeaponPose();//使用武器招式.
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
            TryPlayNextWeaponPose();
        }
    }

    void Defence(float delta)
    {
        //检查与目标的距离，检查与目标攻击物件
        //MeteorUnit u = owner.GetLockedTarget();
        //if (u == null)
        //{
        //    owner.SelectEnemy();
        //    u = owner.GetLockedTarget();
        //}
        //if (u != null)
        //{
        //    if (Vector3.Distance(u.transform.position, transform.position) > 85)
        //    {
        //        owner.ReleaseDefence();
        //        Status = EAIStatus.Idle;
        //    }
        //    else
        //    {
        //        if (owner.posMng.mActiveAction.Idx == CommonAction.Idle)
        //            owner.Defence();
        //    }
        //}
        int random = Random.Range(0, 2);
        switch (random)
        {
            case 0:
                owner.ReleaseDefence();
                break;
            case 1:
                break;
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
    public void Pause(int pause_time)
    {
        paused = true;
        pause_tick = pause_time;
    }

    public void EnableAI(bool enable)
    {
        stoped = !enable;
    }

    public void PushState(EAIStatus type, float t)
    {
        EAIStatusChange newState = new EAIStatusChange(type, t);
        StateStack.Add(newState);
    }

    public void ChangeState(EAIStatus type, float t)
    {
        EAIStatusChange newState = new EAIStatusChange(type, t);
        StateStack.Clear();
        StateStack.Add(newState);
        if (type == EAIStatus.Kill)
        {
            SubStatus = EAISubStatus.KillGotoTarget;
            killTarget = owner.GetLockedTarget();
        }
        ResetAIKey();
    }

    void ResetAIKey()
    {
        owner.controller.Input.AIMove(0, 0);
        owner.controller.Input.OnKeyUp(EKeyList.KL_Defence);
    }

    Coroutine PlayWeaponPoseCorout;
    //触发连招（如果有)
    void TryPlayNextWeaponPose()
    {
        if (PlayWeaponPoseCorout != null)
            return;
        //得到当前招式的下一招式.
        List<int> pose = owner.GetNextWeaponPos();
        if (pose.Count == 0)
            return;
        int rand = Global.Rand.Next(0, pose.Count);
        PlayWeaponPoseCorout = owner.StartCoroutine(PlayWeaponPose(rand));
    }

    //触发首招
    void TryPlayWeaponPose()
    {
        if (PlayWeaponPoseCorout != null)
            return;
        List<int> pose = owner.GetWeaponPos();
        int rand = Global.Rand.Next(0, pose.Count);
        PlayWeaponPoseCorout = owner.StartCoroutine(PlayWeaponPose(rand));
    }

    IEnumerator PlayWeaponPose(int pose)
    {
        int nowpose = owner.posMng.mActiveAction.Idx;
        List<VirtualInput> skill = VirtualInput.CalcSkillInput(pose);
        for (int i = 0; i < skill.Count; i++)
        {
            //受击中断招式
            if (owner.posMng.mActiveAction.Idx != nowpose)
                yield break;

            if (skill[i].type == 1)
                owner.controller.Input.OnKeyDown(skill[i].key, true);
            else if (skill[i].type == 0)
                owner.controller.Input.OnKeyUp(skill[i].key);

            yield return 0;
        }
        PlayWeaponPoseCorout = null;
    }

    public void OnDamaged()
    {
        //模拟出招被其他敌方角色攻击打断
        if (PlayWeaponPoseCorout != null)
        {
            owner.StopCoroutine(PlayWeaponPoseCorout);
            PlayWeaponPoseCorout = null;
        }

        Status = EAIStatus.Kill;
        SubStatus = EAISubStatus.KillOnHurt;
    }

    //寻路相关的.
    int curPatrolIndex;
    bool reverse = false;
    int targetPatrolIndex;
    List<WayPoint> PatrolPath = new List<WayPoint>();
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
                List<WayPoint> r = GameBattleEx.Instance.FindPath(PathTmp[i].index, PathTmp[i + 1].index);
                if (r.Count != 0)
                {
                    if (idx.Count != 0 && (idx[idx.Count - 1] != r[0].index))
                        idx.Add(r[0].index);
                    for (int j = 1; j < r.Count; j++)
                        idx.Add(r[j].index);
                }
            }
        }
        for (int i = 0; i < idx.Count; i++)
            PatrolPath.Add(Global.GLevelItem.wayPoint[idx[i]]);
        Debug.LogError(string.Format("{0}设置巡逻路径", owner.name));
        for (int i = 0; i < PatrolPath.Count; i++)
        {
            Debug.LogError(string.Format("{0}", PatrolPath[i].index));
        }
        //-1代表在当前角色所在位置
        curPatrolIndex = -1;
        SubStatus = EAISubStatus.Patrol;
    }

    //绕原地
    Coroutine PatrolRotateCoroutine;//巡逻到达某个目的点后，会随机旋转1-5次，每次都会
    IEnumerator PatrolRotate()
    {
        float rotateAngle = Random.Range(30, 180);
        Quaternion quat = Quaternion.AngleAxis(rotateAngle, Vector3.up);
        Vector3 target = owner.transform.position + quat  * - owner.transform.forward;
        Vector3 diff = (target - owner.mPos);
        diff.y = 0;
        float dot = Vector3.Dot(new Vector3(-owner.transform.forward.x, 0, -owner.transform.forward.z).normalized, diff.normalized);
        float dot2 = Vector3.Dot(new Vector3(-owner.transform.right.x, 0, -owner.transform.right.z).normalized, diff.normalized);
        float angle = Mathf.Abs(Mathf.Acos(dot) * Mathf.Rad2Deg);
        bool rightRotate = dot2 < 0;
        float offset = 0.0f;
        float offsetmax = GetAngleBetween(target);
        float timeTotal = offsetmax / 150.0f;
        float timeTick = 0.0f;
        while (true)
        {
            timeTick += Time.deltaTime;
            float yOffset = Mathf.Lerp(0, offsetmax, timeTick / timeTotal);
            owner.SetOrientation((rightRotate ? -1 : 1) * (yOffset - offset));
            offset = yOffset;
            if (timeTick > timeTotal)
            {
                owner.FaceToTarget(target);
                if (owner.posMng.mActiveAction.Idx == CommonAction.WalkRight || owner.posMng.mActiveAction.Idx == CommonAction.WalkLeft)
                    owner.posMng.ChangeAction(0, 0.1f);
                break;
            }
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

    //朝向指定目标，一定时间内
    Coroutine PatrolRotateToTargetCoroutine;
    IEnumerator PatrolRotateToTarget(Vector3 vec)
    {
        WsGlobal.AddDebugLine(vec, vec + Vector3.up * 10, Color.red, "PatrolPoint", 20.0f);
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
            owner.SetOrientation((rightRotate ? -1 : 1) * (yOffset - offset));
            offset = yOffset;
            if (timeTick > timeTotal)
            {
                owner.FaceToTarget(vec);
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

                                if (Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(PatrolPath[targetPatrolIndex].pos.x, 0, PatrolPath[targetPatrolIndex].pos.z)) <= 5)
                                {
                                    owner.controller.Input.AIMove(0, 0);
                                    RotateRound = Random.Range(1, 3);
                                    SubStatus = EAISubStatus.PatrolSubRotateInPlace;//到底指定地点后旋转
                                    curPatrolIndex = targetPatrolIndex;
                                    //Debug.LogError("进入巡逻子状态-到底指定地点后原地旋转.PatrolSubRotateInPlace");
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

                            //中断寻路，当距离小于下一帧移动的距离时.
                            if (Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(PatrolPath[targetPatrolIndex].pos.x, 0, PatrolPath[targetPatrolIndex].pos.z)) <= owner.Speed * Time.deltaTime * 0.13f)
                            {
                                owner.controller.Input.AIMove(0, 0);
                                RotateRound = Random.Range(1, 3);
                                SubStatus = EAISubStatus.PatrolSubRotateInPlace;//到底指定地点后旋转
                                curPatrolIndex = targetPatrolIndex;
                                //Debug.LogError("进入巡逻子状态-到底指定地点后原地旋转.PatrolSubRotateInPlace");
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
                    PatrolRotateToTargetCoroutine = owner.StartCoroutine(PatrolRotateToTarget(PatrolPath[targetPatrolIndex].pos));
                }
                break;
            case EAISubStatus.PatrolSubGotoTarget:
                //Debug.LogError("进入巡逻子状态-朝目标输入移动");
                if (Vector3.Distance(new Vector3(owner.mPos.x, 0, owner.mPos.z), new Vector3(PatrolPath[targetPatrolIndex].pos.x, 0, PatrolPath[targetPatrolIndex].pos.z)) <= owner.Speed * Time.deltaTime * 0.13f)
                {
                    RotateRound = Random.Range(1, 3);
                    SubStatus = EAISubStatus.PatrolSubRotateInPlace;//到底指定地点后旋转
                    curPatrolIndex = targetPatrolIndex;
                    //Debug.LogError("进入巡逻子状态-到底指定地点后原地旋转.PatrolSubRotateInPlace");
                    owner.controller.Input.AIMove(0, 0);
                    return;
                }
                owner.FaceToTarget(new Vector3(PatrolPath[targetPatrolIndex].pos.x, 0, PatrolPath[targetPatrolIndex].pos.z));
                owner.controller.Input.AIMove(0, 1);
                break;
        }
    }
}

public class VirtualInput
{
    public EKeyList key;
    public int type;//1 down 0 up
    public static List<VirtualInput> CalcSkillInput(int pose)
    {
        List<VirtualInput> skill = new List<VirtualInput>();
        return skill;
    }
}