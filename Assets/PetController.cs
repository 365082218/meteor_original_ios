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
        ProcessGravity();
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
                if (IsOnGround())
                {
                    if (FollowTarget != null)
                    {
                        checkTick -= Time.deltaTime;
                        if (checkTick <= 0.0f)
                        {
                            if (Vector3.Distance(transform.position, FollowTarget.mPos) >= Global.FollowDistanceStart)
                            {
                                
                                RefreshPath(transform.position, FollowTarget.mPos);
                                checkTick = checkDelay;
                            }
                        }
                    }

                }
            break;
        }
        
    }

    //改变主状态，则清空寻路数据，否则不用.
    public void ChangeState(EAIStatus type)
    {
        Status = type;
        Path.Clear();
        if (type == EAIStatus.Follow)
            SubStatus = EAISubStatus.FollowGotoTarget;
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
                animator.Play("tobituki1");
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
