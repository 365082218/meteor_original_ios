using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum EAIStatus
{
    Idle,//空闲->决策下一步，或继续空闲.
    Wait,//停顿->空闲状态下停顿一会.
    Walk,//巡逻
    Run,//朝发现的目标跑动。
    Rotate,//朝目标旋转过程中
    GotoTarget,//管理，转身朝向各个寻路节点，跑向寻路节点，切换到下个寻路节点
    Jump,//朝发现的目标跳跃。
    Kill,//朝目标攻击
    Guard,//防御
    FallDown,//倒地状态->（某方向滚动）起来 / （原地跳跃）起来
    FallUp,//起身状态，等动作播放完毕.
    BeHurted,
    Follow,//跟随
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
    }

    MeteorUnit owner;
    MeteorUnit followTarget;
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
        //return;
        if (paused)
        {
            pause_tick -= Time.deltaTime;
            if (pause_tick <= 0.0f)
                paused = false;
            return;
        }

        if (StateStack.Count != 0)
        {
            StateStack[StateStack.Count - 1].last -= Time.deltaTime;
            switch (StateStack[StateStack.Count - 1].type)
            {
                case EAIStatus.Guard:
                    if (owner.posMng.CanDefence)
                        owner.Defence();
                    break;
                case EAIStatus.Wait:
                    break;
                case EAIStatus.Follow:
                    MovetoTarget(Time.deltaTime);
                    break;
            }
            if (StateStack[StateStack.Count - 1].last <= 0.0f)
                StateStack.RemoveAt(StateStack.Count - 1);
            return;
        }
        //if (owner.posMng.mActiveAction.Idx == 113)
        //{
        //    //起身
        //    owner.posMng.ChangeAction(CommonAction.DCForw);
        //    Status = EAIStatus.FallUp;
        //}
        //else if (owner.posMng.mActiveAction.Idx == 0)
        //{
        //    //跑向攻击对象
        //    
        //}
        //else if (owner.posMng.mActiveAction.Idx == CommonAction.WalkForward)
        //{
        //    //走向目标点
        //}
        //else if (owner.posMng.mActiveAction.Idx == CommonAction.Run)
        //{
        //    //跑向攻击目标过程中
        //    if (Vector3.Distance(targetPos, transform.position) <= 20)
        //    {
        //        //小于攻击范围，思考对策中.
        //        Status = EAIStatus.Think;
        //    }
        //}
        OnIdle(Time.deltaTime);

        //switch (Status)
        //{
        //    case EAIStatus.Idle:
        //        OnIdle(Time.deltaTime);
        //        break;
        //    case EAIStatus.Walk:
        //        Move(Time.deltaTime);
        //        break;
        //    case EAIStatus.Run:
        //        Move(Time.deltaTime);
        //        break;
        //    case EAIStatus.FallDown:
        //        FallDown(Time.deltaTime);
        //        break;
        //    case EAIStatus.FallUp:
        //        FallUp(Time.deltaTime);
        //        break;
        //    case EAIStatus.Defence:
        //        Defence(Time.deltaTime);
        //        break;
        //    case EAIStatus.GotoTarget:
        //        GotoTarget(Time.deltaTime);
        //        break;
        //    case EAIStatus.Rotate:
        //        Rotate(Time.deltaTime);
        //        break;
        //    case EAIStatus.BeHurted:
        //        OnHurtDone();
        //        break;
        //}
    }
    //在其他角色上使用的插槽位置，
    Dictionary<MeteorUnit, int> FreeCache = new Dictionary<MeteorUnit, int>();
    void MovetoTarget(float t)
    {
        if (pathIdx == -1 && !FreeCache.ContainsKey(followTarget) && Vector3.Distance(owner.transform.position, followTarget.mPos) > 40.0f)
        {
            int FreeSlot = -1;
            targetPath = GameBattleEx.Instance.FindPath(owner.transform.position, followTarget, out FreeSlot);
            pathIdx = 0;
            targetPos = followTarget.transform.position;
        }
        tick = 0.0f;
    }

    public bool InputBlocked()
    {
        return StateStack.Count != 0;
    }

    public void FollowTarget(int target)
    {
        followTarget = U3D.GetUnit(target);
        ChangeState(EAIStatus.Follow, 3.0f);
    }
    //防御时遭到攻击，也有防御动作
    void OnDefencePlaying()
    {

    }

    //受伤僵直完毕后，切换为Idle
    void OnHurtDone()
    {
        owner.posMng.ChangeAction(CommonAction.Idle);
    }

    public void OnUnitDead(MeteorUnit deadunit)
    {
        tick = updateDelta;//下一次进入空闲立即刷新对象位置和方向。
    }

    //更新路径间隔
    const float updateDelta = 1.0f;
    float tick = 0.0f;
    float waitCrouch;
    float waitDefence;
    void OnIdle(float deltaTime)
    {
        tick += deltaTime;
        waitCrouch -= deltaTime;
        waitDefence -= deltaTime;
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

        if (targetPos != Vector3.zero && Vector3.Distance(targetPos, owner.transform.position) < 80)
        {
            //owner.Defence();
            //Status = EAIStatus.Defence;
        }
        else
        {
            //大于50M，尝试走过去。
            //if (targetPos != Vector3.zero)
            //{
            //    Status = EAIStatus.GotoTarget;//先朝目标转向，然后跑过去.不寻路先.
            //}
        }
        //模拟AI计算下一步该做什么。
        //AI分为发现目标，和未发现目标2种情况下的行为概率.
        if (targetPath != null)//targetPos != Vector3.zero)
        {
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
        }
        //else
        //{

        //}
        int random = Global.Rand.Next(0, 9);
        //Log.LogFrame("随机0-7:得到" + random);
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
        if (owner.posMng.mActiveAction.Idx == CommonAction.Idle)
        {
            switch (random)
            {
                case 0:
                    owner.posMng.ChangeAction(CommonAction.Taunt);
                    break;
                case 1:
                case 2:
                    owner.controller.Input.OnKeyDown(EKeyList.KL_Defence, true);
                    waitDefence = 4.0f;
                    break;
                case 3:
                case 4:
                    owner.controller.Input.OnKeyDown(EKeyList.KL_Attack, true);
                    break;
                case 5:
                case 6:
                    if (owner.AngryValue >= Global.ANGRYMAX)
                        owner.PlaySkill();
                    break;
                case 7:
                    TryPlayWeaponPose();
                    break;
                case 8:
                    owner.controller.Input.OnKeyDown(EKeyList.KL_Crouch, true);
                    waitCrouch = 3.0f;
                    break;
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

    //打单独一次完整旋转，需要0.5f;
    const float rotateLimit = 0.5f;
    float rotateTick = 0.0f;
    void Rotate(float delta)
    {
        Quaternion cur = Quaternion.LookRotation(owner.transform.position - targetPos, Vector3.up);
        if (Quaternion.Angle(owner.transform.rotation, cur) <= 5.0f)
        {
            //旋转接近了
            //Status = EAIStatus.Run;//向目标跑去.
            if (owner.posMng.mActiveAction.Idx == CommonAction.CrouchLeft)
                owner.posMng.ChangeAction(CommonAction.Idle);
            rotateTick = 0.0f;
        }
        else
        {
            owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, cur, rotateTick / rotateLimit);
            if (owner.posMng.mActiveAction.Idx != CommonAction.CrouchLeft)
                owner.posMng.ChangeAction(CommonAction.CrouchLeft);
            rotateTick += delta;
        }
    }

    void GotoTarget(float delta)
    {
        rotateTick -= delta;
        //if (targetPath != null && targetPath.Count > pathIdx)
        {
            Quaternion cur = Quaternion.LookRotation(owner.transform.position - targetPos, Vector3.up);
            if (Quaternion.Angle(owner.transform.rotation, cur) <= 5.0f)
            {
                //旋转接近了
                //Status = EAIStatus.Run;//向目标跑去.
                rotateTick = 0.0f;
            }
            else
            {
                //Status = EAIStatus.Rotate;
            }
        }
    }

    void Move(float deltaTime)
    {
        //笔直朝前走就好了.
        Vector2 direction = new Vector2(-owner.transform.forward.x, -owner.transform.forward.z);
        if (direction == Vector2.zero)
            return;
        //如果摇杆按着边缘的方向键，触发任意方向，则移动，否则，旋转目标。
        //跳跃的时候，方向轴移动不受控制，模拟跳跃
        if (owner.IsOnGround())
        {
            if (owner.posMng.CanMove && owner.Speed > 0)
            {
                direction.Normalize();
                float runSpeed = owner.Speed;//跑的速度 1000 = 145M/S 按原来游戏计算

                Vector2 runTrans = direction * runSpeed * deltaTime;
                //怪物和AI在预览中无法跑动

                owner.Move(new Vector3((runTrans * 0.130f).x, 0, (runTrans * 0.130f).y));
                if (owner.posMng.mActiveAction.Idx != CommonAction.Run)
                    owner.posMng.ChangeAction(CommonAction.Run);

                //小于30码防御吧。后面配置好数据
                if (Vector3.Distance(targetPos, owner.transform.position) <= 30)
                {
                    //还是防御先。
                    //Status = EAIStatus.Defence;
                    owner.Defence();
                    return;
                }
                //当与目标的角度差大于10度时，再次旋转。
                Quaternion cur = Quaternion.LookRotation(owner.transform.position - targetPos, Vector3.up);
                if (Quaternion.Angle(owner.transform.rotation, cur) > 10.0f)
                {
                    //旋转接近了
                    //Status = EAIStatus.Rotate;//向目标旋转.
                    rotateTick = 0.0f;
                }
            }
        }
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

    //加载AI设定.
    public void Init()
    {

    }

    public void OnDamaged()
    {
        //模拟出招被其他敌方角色攻击打断
        if (PlayWeaponPoseCorout != null)
        {
            owner.StopCoroutine(PlayWeaponPoseCorout);
            PlayWeaponPoseCorout = null;
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