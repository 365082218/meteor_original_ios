using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetController : MonoBehaviour {
    public MeteorUnit FollowTarget;
    CharacterController petController;
    Animator animator;
    AnimatorStateInfo current;
    float checkDelay = 3.0f;//3喵检查一次.
    float checkTick = 0.0f;
    public EAIStatus Status { get; set; }
    public EAISubStatus SubStatus { get; set; }
    private void Awake()
    {
        petController = this.GetComponent<CharacterController>();
        animator = this.GetComponent<Animator>();
        current = animator.GetCurrentAnimatorStateInfo(0);
        int count = Global.GLevelItem.wayPoint.Count;
        nodeContainer = new PathNode[count];
        for (int i = 0; i < count; i++)
            nodeContainer[i] = new PathNode();
        Status = EAIStatus.Idle;
        SubStatus = EAISubStatus.SubStatusIdle;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("t:" + current.normalizedTime + " l:" + current.length);
        UpdateMoveInput();
        ProcessGravity();

        mewTick += Time.deltaTime;

        if (Status == EAIStatus.Wait)
        {
            if (FollowTarget != null)
            {
                float dis = Vector3.SqrMagnitude(transform.position - FollowTarget.mSkeletonPivot);
                if (dis >= Global.FollowDistanceStart)//距离65码开始跟随
                {
                    ChangeState(EAIStatus.Follow);
                    return;
                }
            }
        }
        switch (Status)
        {
            case EAIStatus.Follow:
                OnFollow();
                animator.SetBool("mew", false);
                break;
            case EAIStatus.Idle:
                OnIdle();
                animator.SetBool("mew", false);
                break;
            case EAIStatus.Wait:
                OnWait();
                animator.SetBool("mew", false);
                break;
            case EAIStatus.Mew:
                OnMew();
                break;
        }
        
    }

    void UpdateMoveInput()
    {
        direction.Normalize();
        //跑的速度 1000 = 145M/S 按原来游戏计算
        Vector2 runTrans = direction * MoveSpeed;
        float x = runTrans.x * 0.085f, y = runTrans.y * 0.135f;
        SetVelocity(y, x);
    }

    public void SetVelocity(float z, float x)
    {
        Vector3 vec = z * transform.forward + x * transform.right;
        ImpluseVec.z = vec.z;
        ImpluseVec.x = vec.x;
    }

    //跟随状态
    //1：在路线中
    //2：到达最终一个路点，朝角色走去.
    //3：离跟随目标距离较近，停止跟随，转为Wait状态.
    //4: 目标一直在移动中，要一段时间刷新一次跟随路线.

    Vector2 direction = Vector2.zero;
    void AIMove(float x, float z)
    {
        direction.x = x;
        direction.y = z;
    }

    public void FaceToTarget(Vector3 target)
    {
        Vector3 vdiff = target - transform.position;
        vdiff.y = 0;
        transform.rotation = Quaternion.LookRotation(new Vector3(vdiff.x, 0, vdiff.z), Vector3.up);
    }

    public float MoveSpeed = 50;
    public int lastFollowWayPointIdx = -1;//上一次寻路时，目标所处与路点，如果该路点发生变化，则需要重新寻路，否则不需要
    Vector3 NextFramePos = Vector3.zero;
    int RotateRound = 0;
    float AIJumpDelay = 0.0f;

    //设置跳跃动画
    void AIJump()
    {
        animator.SetBool("jump", true);
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

    void OnFollow()
    {
        Vector3 vec;
        //跟随目标为空
        if (FollowTarget == null)
        {
            ChangeState(EAIStatus.Wait);
            return;
        }

        float dis = 0.0f;
        switch (SubStatus)
        {
            case EAISubStatus.FollowGotoTarget:
                dis = Vector3.SqrMagnitude(transform.position - FollowTarget.mSkeletonPivot);
                if (dis < Global.FollowDistanceEnd)
                {
                    AIMove(0, 0);
                    ChangeState(EAIStatus.Wait);
                    return;
                }
                //快速
                if (dis > 200 * 200)
                {
                    MoveSpeed = 200; 
                }
                else
                if (dis > 100 * 100)
                {
                    MoveSpeed = 100;
                }
                else
                {
                    MoveSpeed = 49;
                }
                vec = transform.position - FollowTarget.mSkeletonPivot;
                vec.y = 0;
                if (Vector3.SqrMagnitude(vec) <= Global.FollowDistanceEnd)
                {
                    AIMove(0, 0);
                    ChangeState(EAIStatus.Wait);
                    return;
                }

                int cur = PathMng.Instance.GetWayIndex(FollowTarget.mSkeletonPivot);
                if (Path.Count == 0)
                    RefreshPath(transform.position, FollowTarget.mSkeletonPivot);
                else if (lastFollowWayPointIdx != cur)
                {
                    RefreshPath(transform.position, FollowTarget.mSkeletonPivot);
                    return;
                }

                if (targetIndex >= Path.Count)
                {
                    //朝角色走即可.
                    dis = Vector3.SqrMagnitude(FollowTarget.mSkeletonPivot - transform.position);
                    //不计算高度的距离.30码
                    if (dis < Global.AttackRangeMinD)
                    {
                        AIMove(0, 0);
                        ChangeState(EAIStatus.Wait);
                        return;
                    }
                    FaceToTarget(FollowTarget.mSkeletonPivot);
                    AIMove(0, 1);
                }
                else
                {
                    if (curIndex == -1)
                        targetIndex = 0;

                    //检查这一帧是否会走过目标，因为跨步太大.
                    NextFramePos = Path[targetIndex].pos - transform.position;
                    NextFramePos.y = 0;
                    //33码距离内.
                    if (Vector3.SqrMagnitude(NextFramePos) <= Global.AttackRangeMinD)
                    {
                        NextFramePos = transform.position + NextFramePos.normalized * MoveSpeed * Time.deltaTime * 0.15f;
                        float s = GetAngleBetween((NextFramePos - transform.position).normalized, (Path[targetIndex].pos - NextFramePos).normalized);
                        if (s < 0)
                        {
                            //其他路点，到达后转向下一个路点.
                            AIMove(0, 0);
                            curIndex = targetIndex;
                            targetIndex += 1;
                            RotateRound = Random.Range(1, 3);
                            SubStatus = EAISubStatus.FollowSubRotateToTarget;//到指定地点后旋转到目标.
                            return;
                        }
                    }
                    FaceToTarget(Path[targetIndex].pos);
                    AIMove(0, 1);
                    //模拟跳跃键，移动到下一个位置.还得按住上
                    if (curIndex != -1)
                    {
                        if (PathMng.Instance.GetWalkMethod(Path[curIndex].index, Path[targetIndex].index) == WalkType.Jump && IsOnGround() && AIJumpDelay > 2.5f)
                        {
                            AIJump();
                            AIJumpDelay = 0.0f;
                            return;
                        }
                        //尝试几率跳跃，否则可能会被卡住.
                        int random = Random.Range(0, 100);
                        if (AIJumpDelay >= 2.5f && random < 2)
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
                    vec = FollowTarget.mSkeletonPivot;
                else
                    vec = Path[targetIndex].pos;
                FaceToTarget(vec);
                SubStatus = EAISubStatus.FollowGotoTarget;
                break;
        }
    }

    //原地不动-喵
    float mewTick = 0.0f;
    void OnWait()
    {
        if (mewTick >= 5.0f)
        {
            int mewChance = Random.Range(0, 100);
            if (mewChance < 2)
            {
                mewTick = 0;//5秒内最多能进行一次四处看.
                ChangeState(EAIStatus.Mew);
            }
        }
    }

    //空闲动画
    float idleTick = 0.0f;
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

    //改变主状态，则清空寻路数据，否则不用.
    public void ChangeState(EAIStatus type)
    {
        Status = type;
        Path.Clear();
        if (type == EAIStatus.Follow)
            SubStatus = EAISubStatus.FollowGotoTarget;
        else
        {
            animator.SetBool("walk", false);
        }
    }

    //喵叫.
    void OnMew()
    {
        animator.SetBool("mew", true);
        SoundManager.Instance.PlaySound("cat.wav");
        Status = EAIStatus.Wait;
    }

    public bool IgnoreGravity = false;
    public Vector3 ImpluseVec = Vector3.zero;
    public bool OnTouchWall = false;
    public bool OnTopGround = false;//顶部顶着了,无法向上继续
    public bool OnGround = false;//控制器是否收到阻隔无法前进.
    public bool MoveOnGroundEx = false;//移动的瞬间，射线是否与地相聚不到4M。下坡的时候很容易离开地面
    public const float groundFriction = 2000.0f;//地面摩擦力，在地面不是瞬间停止下来的。
    public const float yLimitMin = -500f;//最大向下速度
    public const float yLimitMax = 500;//最大向上速度

    void ProcessGravity()
    {
        if (!petController.enabled)
            return;
        //计算运动方向
        //角色forward指向人物背面
        //根据角色状态计算重力大小，在墙壁，空中，以及空中水平轴的阻力
        float gScale = MeteorUnit.gGravity;
        Vector3 v;
        v.x = ImpluseVec.x * Time.deltaTime;
        v.y = animator.GetBool("Jump") ? 0 : ImpluseVec.y * Time.deltaTime;
        v.z = ImpluseVec.z * Time.deltaTime;
        if (v != Vector3.zero)
            Move(v);
        else
        {
            animator.SetBool("walk", false);
            animator.SetFloat("speed", MoveSpeed);
        }   

        if (!IsOnGround())
        {
            if (IgnoreGravity)
            {

            }
            else
            {
                ImpluseVec.y = ImpluseVec.y - gScale * Time.deltaTime;
                if (ImpluseVec.y < yLimitMin)
                    ImpluseVec.y = yLimitMin;
            }
        }
    }

    public void Move(Vector3 trans)
    {
        if (petController != null && petController.enabled)
        {
            CollisionFlags collisionFlags = petController.Move(trans);
            animator.SetBool("walk", true);
            animator.SetFloat("speed", MoveSpeed);
            UpdateFlags(collisionFlags);
        }
        else
            transform.position += trans;
    }

    public bool IsOnGround()
    {
        bool ret = OnGround || MoveOnGroundEx;
        return animator.GetBool("Jump") ? false : ret;
    }

    float floatTick = -1.0f;
    void UpdateFlags(CollisionFlags flag)
    {
        if ((flag & CollisionFlags.Sides) != 0)
            OnTouchWall = true;
        else
            OnTouchWall = false;
        if ((flag & CollisionFlags.Above) != 0)
            OnTopGround = true;
        if (flag == CollisionFlags.None)
            OnTopGround = OnGround = OnTouchWall = false;
        if ((flag & CollisionFlags.Below) != 0)
            OnGround = true;

        //减少射线发射次数.
        bool Floating = false;
        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out hit, 1000, 1 << LayerMask.NameToLayer("Scene")))
        {
            MoveOnGroundEx = hit.distance <= 3f;
            //Debug.Log(string.Format("distance:{0}", hit.distance));
            Floating = hit.distance >= 8.0f;
        }
        else
            MoveOnGroundEx = false;


        //如果Y轴速度向下，但是已经接触地面了
        if (ImpluseVec.y <= 0 && !IgnoreGravity)
        {
            if (MoveOnGroundEx || OnGround)
            {
                //animator.Play("tobituki1");
                ResetYVelocity();
            }
        }

        //如果撞到天花板了.
        if (OnTopGround)
            if (ImpluseVec.y > 0)
                ImpluseVec.y = 0;
    }

    public void ResetYVelocity()
    {
        ImpluseVec.y = 0;
    }

    public void SetWorldVelocity(Vector3 vec)
    {
        ImpluseVec.x = vec.x;
        ImpluseVec.y = vec.y;
        ImpluseVec.z = vec.z;
    }

    Vector3 hitPoint;//最近一次碰撞的点.用这个点和法线来算一些轻功的东西
    Vector3 hitNormal;//碰撞面法线
    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.transform.root.tag.Equals("meteorUnit"))
        {
            MeteorUnit hitUnit = hit.gameObject.transform.root.GetComponent<MeteorUnit>();
            Vector3 vec = hitUnit.mPos - transform.position;
            vec.y = 0;
            GotoPosition(transform.position + Vector3.Normalize(vec) * 100);
        }
        else
        {
            hitPoint = hit.point;
            hitNormal = hit.normal;
        }
    }

    Vector3 wantPosition;
    void GotoPosition(Vector3 vec)
    {
        wantPosition = vec;
    }

    //寻路相关数据
    public SortedDictionary<int, List<PathNode>> PathInfo = new SortedDictionary<int, List<PathNode>>();
    public List<WayPoint> Path = new List<WayPoint>();//存储寻路找到的路线点
    int curIndex = 0;
    int targetIndex = 0;
    public PathNode[] nodeContainer;
    void RefreshPath(Vector3 now, Vector3 target)
    {
        PathMng.Instance.FindPathForPet(this, now, target, ref Path);
        if (Path.Count == 0)
        {
            Debug.DebugBreak();
        }
        curIndex = -1;
        targetIndex = -1;
    }
}
