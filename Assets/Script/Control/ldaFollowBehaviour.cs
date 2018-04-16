using UnityEngine;
using System.Collections;

public class ldaFollowBehaviour : MonoBehaviour {

    public enum DeactivationEnum
    {
        Deactivate,
        DestroyAfterCollision,
        DestroyAfterTime,
        Nothing
    };

    public enum ldaFollowEndModeEnum
    {
        OnTimeMustDone=1,//时间必达
        OnTime=2,//根据时间触发爆炸
        OnDistance=3,//根据追踪距离触发爆炸
        OnRangeXZ=4,//根据与发射点XZ平面距离触发爆炸
        OnRange=5//根据与发射点直线距离触发爆炸
    };

    [Tooltip("碰撞半径")]
    public float ColliderRadius = 0.1f;

    [Tooltip("碰撞目标偏移（与目标位置点的偏移）")]
    public Vector3 ColliderOffset = new Vector3(0,1.5f,0);

    [Tooltip("追踪目标")]
    public GameObject Target;

    [Tooltip("移动目标")]
    public GameObject MoveObject;

    [Tooltip("爆炸目标")]
    public GameObject ExplosionObject;

    public DeactivationEnum DeactivationMode = DeactivationEnum.Nothing;

    public ldaFollowEndModeEnum FollowEndMode = ldaFollowEndModeEnum.OnTimeMustDone;

    [Tooltip("追踪速度")]
    public float MoveSpeed = 2.0f;

    [Tooltip("追踪时间")]
    public float MoveTime = 2.0f;

    [Tooltip("追踪距离")]
    public float MoveDistance = 2.0f;

    private float mMovingTime;

    private float mMovingDistance;

    private Vector3 mLastPosition;

    //发射位置
    private Vector3 mLaunchPosition;

    [Tooltip("碰撞物层")]
    public LayerMask LayerMask = -1;

    //碰撞检测
    private SphereCollider ColliderCheck = null;
    private Rigidbody ColliderCheckRig = null;

    [Tooltip("调试碰撞盒")]
    public bool DebugShowColliderRadius = true;
    private GameObject DebugColliderRadius = null;

    public bool DebugShowDebugAttackRange = false;
    private GameObject DebugAttackRange = null;

    [Tooltip("运动结束强行爆炸（不管是否发生碰撞）")]
    public bool ForceExplosion = false;

    [Tooltip("与目标点最小距离引发碰撞（强行结束）")]
    public float ManualColliderColliderMinDis = 0.1f;

    private bool theEnd = false;

	// Use this for initialization
	void Start () {
	
	}

    void OnEnable()
    {
        Init();
    }

    void OnDisable()
    {
        transform.position = Vector3.zero;
        if (MoveObject != null)
            MoveObject.SetActive(true);
        if (ExplosionObject!=null)
            ExplosionObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.N))
        {
            this.gameObject.SetActive(false);
            this.gameObject.SetActive(true);
        }

        if (theEnd)
            return;


        //时间必达
        if (FollowEndMode == ldaFollowEndModeEnum.OnTimeMustDone)
        {
            if (mMovingTime > Time.deltaTime)
            {
                //Vector3 targetPos = new Vector3(Target.transform.position.x, Target.transform.position.y, Target.transform.position.z);
                //transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime / mMovingTime);
                //mMovingTime -= Time.deltaTime;

                Vector3 targetPos = new Vector3(Target.transform.position.x, Target.transform.position.y, Target.transform.position.z) + ColliderOffset;
                Vector3 targetPos22 = Vector3.Lerp(transform.position, targetPos, MoveSpeed * Time.deltaTime / mMovingTime);
                //transform.position = Vector3.Lerp(transform.position, targetPos, MoveSpeed * mMovingTime / Time.deltaTime);

                //if (Vector3.Distance(targetPos, transform.position) <= ColliderRadius)
                //{
                //    OnTriggerEnter(null);
                //    return;
                //}

                transform.LookAt(targetPos22);
                transform.position = targetPos22;

                mMovingTime -= MoveSpeed * Time.deltaTime;
            }
            else
            {
                theEnd = true;
                ShowMoveObject(false);
                if (ForceExplosion)
                    ShowExplosionObject(true);
            }
        }
        //根据时间触发爆炸
        else if (FollowEndMode == ldaFollowEndModeEnum.OnTime)
        {
            if (mMovingTime > 0)
            {
                Vector3 targetPos = new Vector3(Target.transform.position.x, Target.transform.position.y, Target.transform.position.z) + ColliderOffset;
                transform.position = Vector3.Lerp(transform.position, targetPos, MoveSpeed * Time.deltaTime);

                mMovingTime -= Time.deltaTime;
            }
            else
            {
                theEnd = true;
                ShowMoveObject(false);
                if (ForceExplosion)
                    ShowExplosionObject(true);
            }
        }
        //根据追踪距离触发爆炸
        else if (FollowEndMode == ldaFollowEndModeEnum.OnDistance)
        {
            if(mMovingDistance>0)
            {
                Vector3 targetPos = new Vector3(Target.transform.position.x, Target.transform.position.y, Target.transform.position.z) + ColliderOffset;
                transform.position = Vector3.Lerp(transform.position, targetPos, MoveSpeed * Time.deltaTime);
                mMovingDistance -= Vector3.Distance(mLastPosition, transform.position);
                mLastPosition = transform.position;
            }
            else
            {
                theEnd = true;
                ShowMoveObject(false);
                if (ForceExplosion)
                    ShowExplosionObject(true);
            }
        }
        //根据与发射点XZ平面距离触发爆炸 或 //根据与发射点直线距离触发爆炸
        else if (FollowEndMode == ldaFollowEndModeEnum.OnRangeXZ || FollowEndMode == ldaFollowEndModeEnum.OnRange)
        {
            Vector3 curPosition = this.transform.position;
            if(FollowEndMode == ldaFollowEndModeEnum.OnRangeXZ)//根据与发射点XZ平面距离触发爆炸
                curPosition.y = mLaunchPosition.y;//根据与发射点XZ平面距离触发爆炸
            float dis = Vector3.Distance(curPosition, mLaunchPosition);

            if (dis < MoveDistance)
            {
                Vector3 targetPos = new Vector3(Target.transform.position.x, Target.transform.position.y, Target.transform.position.z) + ColliderOffset;
                transform.position = Vector3.Lerp(transform.position, targetPos, MoveSpeed * Time.deltaTime);
                mMovingDistance -= Vector3.Distance(mLastPosition, transform.position);
                mLastPosition = transform.position;
            }
            else
            {
                theEnd = true;
                ShowMoveObject(false);
                if (ForceExplosion)
                    ShowExplosionObject(true);
            }
        }

	}

    //爆炸
    void ShowExplosionObject(bool show)
    {
        if (ExplosionObject!=null)
            ExplosionObject.SetActive(show);
        //MoveObject.SetActive(false);
    }

    //运动
    void ShowMoveObject(bool show)
    {
        if (MoveObject != null)
            MoveObject.SetActive(show);

        //关碰撞
        if(!show)
        {
            if (ColliderCheck!=null)
                ColliderCheck.enabled = show;
            //ColliderCheckRig.
        }
    }

    public void Init()
    {
        if (DebugShowDebugAttackRange)
        {
            if (DebugAttackRange == null)
            {
                DebugAttackRange = (GameObject)Instantiate(Resources.Load("DebugAttackRange1"));
                DebugAttackRange.transform.parent = null;//this.transform;
                DebugAttackRange.transform.localPosition = Vector3.zero;
            }
            DebugAttackRange.SetActive(true);
            DebugAttackRange.transform.localScale = new Vector3(MoveDistance, MoveDistance, MoveDistance);
            Vector3 tmepPos = this.transform.position;
            tmepPos.y = -3;
            DebugAttackRange.transform.position = tmepPos;
            DebugAttackRange.GetComponent<DebugAttackRange1>().ShowAttackRange("范围" + MoveDistance.ToString(), 1);
        }

        if (DebugShowColliderRadius)
        {
            if (DebugColliderRadius == null)
            {
                DebugColliderRadius = (GameObject)Instantiate(Resources.Load("HitDefinitionSphere"));
                DebugColliderRadius.transform.parent = this.transform;
                DebugColliderRadius.transform.localPosition = Vector3.zero;
            }
            DebugColliderRadius.SetActive(true);
            DebugColliderRadius.transform.localScale = new Vector3(ColliderRadius, ColliderRadius, ColliderRadius);
        }
        else
        {
            if (DebugColliderRadius != null)
                DestroyImmediate(DebugColliderRadius);
        }


        mMovingTime = MoveTime;
        mLastPosition = transform.position;
        mMovingDistance = MoveDistance;

        if (ColliderCheck == null)
            ColliderCheck = this.gameObject.AddComponent<SphereCollider>();
        ColliderCheck.radius = ColliderRadius * 2;
        ColliderCheck.center = Vector3.zero;
        ColliderCheck.isTrigger = true;

        if (ColliderCheckRig == null)
            ColliderCheckRig = this.gameObject.AddComponent<Rigidbody>();
        ColliderCheckRig.useGravity = false;
        ColliderCheckRig.isKinematic = true;

        if (MoveObject != null)
            MoveObject.SetActive(true);
        if (ExplosionObject != null)
            ExplosionObject.SetActive(false);

        //记录下发射点位置
        mLaunchPosition = this.transform.position;

        theEnd = false;
    }


    void OnTriggerEnter(Collider other)
    {
        Debug.Log("ldaFollowBehaviour OnTriggerEnter");
        if (ExplosionObject != null)
            ExplosionObject.SetActive(true);
        if (MoveObject != null)
            MoveObject.SetActive(false);
        theEnd = true;
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("ldaFollowBehaviour OnTriggerExit");
    }
}
