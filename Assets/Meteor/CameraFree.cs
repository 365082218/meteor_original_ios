using UnityEngine;
using System.Collections;
//自由相机，当主角挂掉以后，在场景内随机找1个目标，优先找正在打斗的其中一个.
public class CameraFree : NetBehaviour {
    [HideInInspector]
    //public Transform[] m_Targets;//摄像机的各个视角的调试对象.
    public Transform CameraLookAt;//摄像机注视的目标
    public Transform CameraPosition;//摄像机经过缓动后的期望位置
    public Transform Target;
    public Transform LockTarget;//锁定目标，战斗系统里的摄像机针对的除了主角外的第二目标.
    public float followDistance = 50;//在角色身后多远
    public float followHeight = 0;//离跟随点多高。
    public float m_MinSize = 55;//fov最小55
    public float f_speedMax = 150.0f;//移动速度最大限制
    public float f_DampTime = 0.1f;
    public float f_eulerMax = 60.0f;//角速度最大值
    [HideInInspector]
    public Camera m_Camera;
    public float fRadis;
    public float lastAngle;
    float angleMax = 75.0f;
    float angleMin = -75.0f;
    private new void Awake()
    {
        base.Awake();
        CameraPosition = new GameObject("CameraPosition").transform;
        CameraLookAt = new GameObject("CameraLookAt").transform;
    }

    private new void OnDestroy()
    {
        base.OnDestroy();
    }

    MeteorUnit UnitTarget;
    int CameraType = 0;
    public void Init(MeteorUnit target)
    {
        CameraType = target != null ? 1 : 0;
        //GameObject.Destroy(Main.Instance.playerListener);
        //Main.Instance.playerListener = gameObject.AddComponent<AudioListener>();
        UnitTarget = target;
        Target = target.transform;
        animationPlay = false;
        animationTick = 0.0f;
        followHeight = 6;
        followDistance = 55.0f;
        BodyHeight = 30;
        m_MinSize = 60;
        LookAtAngle = 10.0f;
        m_Camera = GetComponent<Camera>();
        m_Camera.fieldOfView = m_MinSize;
        fRadis = Mathf.Sqrt(followDistance * followDistance + followHeight * followHeight);
        lastAngle = Mathf.Atan2(followHeight, followDistance) * Mathf.Rad2Deg;

        if (target != null)
        {
            CameraLookAt.position = new Vector3(Target.position.x, Target.position.y + BodyHeight, Target.position.z);
            CameraPosition.position = new Vector3(0, followHeight, 0) + CameraLookAt.position + followDistance * (Target.forward);
            transform.position = CameraPosition.position;
            Vector3 vdiff = CameraLookAt.position - transform.position;
            transform.rotation = Quaternion.LookRotation(new Vector3(vdiff.x, 0, vdiff.z), Vector3.up);
        }
    }

    public bool Smooth = true;
    public void ForceUpdate()
    {
        if (Target != null && !Main.Ins.CombatData.PauseAll)
            CameraSmoothFollow(Smooth);
    }

    void LateUpdate()
    {
        if (UnitTarget != null)
        {
            if (UnitTarget.Dead)
            {
                RefreshTarget();
            }
        }
        if (UnitTarget != null && !Main.Ins.CombatData.PauseAll)
        {
            CameraSmoothFollow(Smooth);
        }
    }

    public void RefreshTarget()
    {
        MeteorUnit watchTarget = null;
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].Dead)
                continue;
            if (watchTarget == null)
                watchTarget = Main.Ins.MeteorManager.UnitInfos[i];
            if (Main.Ins.MeteorManager.UnitInfos[i].LockTarget != null)
            {
                watchTarget = Main.Ins.MeteorManager.UnitInfos[i];
                break;
            }
        }
        UnitTarget = watchTarget;
    }

    //为真则下一帧摄像机要切换视角模式.
    bool animationPlay = false;
    float animationTick = 0.0f;
    public void OnLockTarget()
    {
        animationTick = 0.0f;
        animationPlay = true;
        currentVelocityX = 0;
        currentVelocityY = 0;
        currentVelocityZ = 0;
        currentEulerVelocityX = 0;
        currentEulerVelocityY = 0;
        currentEulerVelocityZ = 0;
    }

    public void OnUnlockTarget()
    {
        animationTick = 0.0f;
        animationPlay = true;
        currentVelocityX = 0;
        currentVelocityY = 0;
        currentVelocityZ = 0;
        currentEulerVelocityX = 0;
        currentEulerVelocityY = 0;
        currentEulerVelocityZ = 0;
    }

    public float SmoothDampTime = 0.15f;//平滑到稳定所需时间.
    public float SmoothDampYTime = 0.1f;//垂直平滑到稳定所需时间
    public float LookAtAngle = 0.0f;//朝目标俯视角度
    public float BodyHeight = 10.0f;//目标脊椎往上多少

    float currentVelocityX;
    float currentVelocityY;
    float currentVelocityZ;
    float currentEulerVelocityX;
    float currentEulerVelocityY;
    float currentEulerVelocityZ;
    Vector3 currentVelocity = Vector3.zero;
    float currentVelocityY2;
    public Vector3[] newViewIndex = new Vector3[3];
    int ViewIndex = 0;

    protected void CameraSmoothFollow(bool smooth = true)
    {
        //y = ax + b;
        //a = 10 y = 75;
        //a = 140 y = 60;
        float CameraRadis = 0.0f;
        float angleY = 0;//此角度由目标与玩家间的距离的来计算，距离越近越大
        Vector3 newPos = Vector3.zero;
        Vector3 vecTarget = Vector3.zero;
        Vector3 wallHitPos = Vector3.zero;//撞到墙壁的位置

        //固定视角
        //视角1：背后固定跟随角色

        //电影视角
        //视角2: 左侧望着格斗的2人
        //视角3: 右侧望着格斗的2人
        //视角4：顶部望着格斗的2人

        //过程1：由固定视角，切换到电影视角
        //过程2: 由电影视角，切换到固定视角

        //不是AI观察相机时
        if (UnitTarget.LockTarget != null && this.CameraType != 1)
        {
            LockTarget = UnitTarget.LockTarget.transform;
            //有锁定目标
            //开启摄像机锁定系统
            CameraLookAt.position = (Target.position + LockTarget.position) / 2 + new Vector3(0, 25, 0);
            CameraRadis = Vector3.Distance(Target.position, LockTarget.position) / 2 + 60;
            float dis = Vector3.Distance(new Vector3(Target.position.x, 0, Target.position.z), new Vector3(LockTarget.position.x, 0, LockTarget.position.z));
            Vector3 vecDiff = Target.position - LockTarget.position;
            Vector3 vecForward = Vector3.Normalize(new Vector3(vecDiff.x, 0, vecDiff.z));

            //最远时，15度，最近时95度，其他值。10码 = 80度 140码 15度 约为 y = -0.5x + 85
            //半径最低65，最高
            angleY = -0.5f * dis + 95;
            float yHeight = Mathf.Tan(LookAtAngle * Mathf.Deg2Rad) * CameraRadis;
            newViewIndex[0] = CameraLookAt.position + Quaternion.AngleAxis(-angleY, Vector3.up) * vecForward * CameraRadis;
            newViewIndex[0].y += yHeight;

            newViewIndex[1] = CameraLookAt.position + Quaternion.AngleAxis(angleY, Vector3.up) * vecForward * CameraRadis;
            newViewIndex[1].y += yHeight;
            newViewIndex[2] = CameraLookAt.position + Quaternion.AngleAxis(-angleY, Target.right) * vecForward * CameraRadis;//顶部最后考虑

            //vecTarget[0] = newViewIndex[ViewIndex];
            //vecTarget[1] = newViewIndex[(ViewIndex + 1) % 3];
            //vecTarget[2] = newViewIndex[(ViewIndex + 2) % 3];

            //重新排。当前排第一，顶部排最后，剩下的排第二。这样切换就不会那么频繁.
            //for (int i = 0; i < 3; i++)
            //    m_Targets[i].position = newViewIndex[i];
            dis = 1000.0f;
            for (int i = 0; i < 2; i++)
            {
                if (CameraCanLookTarget(newViewIndex[i], out wallHitPos))
                {
                    float fdis = Vector3.Distance(newViewIndex[i], transform.position);
                    if (fdis < dis)
                    {
                        dis = fdis;
                        vecTarget = newViewIndex[i];
                    }
                }
                else
                {
                    float fdis = Vector3.Distance(wallHitPos, transform.position);
                    float disWall = Vector3.Distance(wallHitPos, CameraLookAt.position);//墙壁与看向目标的距离一定要足够，否则视角堵着很难受.
                    if (fdis < dis && disWall > 40.0f)
                    {
                        dis = fdis;
                        vecTarget = wallHitPos;
                    }
                }
            }

            //左侧右侧都被墙壁堵住，并且太近
            if (vecTarget == Vector3.zero)
            {
                if (CameraCanLookTarget(newViewIndex[2], out wallHitPos))
                    vecTarget = newViewIndex[2];
                else
                    vecTarget = wallHitPos;
            }

            newPos = vecTarget;
            //整个视角都是缓动的
            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref currentVelocity, SmoothDampTime);
            //摄像机朝向观察目标,平滑的旋转视角
            Quaternion to = Quaternion.LookRotation(CameraLookAt.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, to, SmoothDampTime);
        }
        else
        {
            newPos.x = Target.transform.position.x;
            newPos.y = Target.position.y + BodyHeight + followHeight;
            newPos.z = Target.transform.position.z;
            newPos += Main.Ins.LocalPlayer.transform.forward * followDistance;

            vecTarget.x = Target.position.x;
            vecTarget.y = Target.position.y + BodyHeight;
            vecTarget.z = Target.position.z;

            CameraLookAt.position = vecTarget;

            //如果新的位置，与目标注视点中间隔着墙壁
            RaycastHit wallHit;
            bool hitWall = false;
            if (Physics.Linecast(CameraLookAt.position, newPos, out wallHit,
                1 << LayerMask.NameToLayer("Scene") |
                (1 << LayerMask.NameToLayer("Default")) |
                (1 << LayerMask.NameToLayer("Wall")) |
                (1 << LayerMask.NameToLayer("Water"))))
            {
                if (!wallHit.collider.isTrigger)
                {
                    hitWall = true;
                    //Debug.LogError("hitWall" + wallHit.transform.name);
                    //摄像机与角色间有物件遮挡住角色，开始自动计算摄像机位置.
                    newPos = wallHit.point + Vector3.Normalize(CameraLookAt.position - wallHit.point) * 5;
                }
            }

            //没有撞墙
            if (!hitWall)
            {
                newPos.x = Target.transform.position.x;
                newPos.z = Target.transform.position.z;
                float y = Mathf.SmoothDamp(CameraPosition.position.y, Target.position.y + BodyHeight + followHeight, ref currentVelocityY, f_DampTime);
                newPos.y = y;
                newPos += Main.Ins.LocalPlayer.transform.forward * followDistance;
            }
            else
            {
                float y = Mathf.SmoothDamp(CameraPosition.position.y, newPos.y, ref currentVelocityY, f_DampTime);
                newPos.y = y;
            }

            CameraPosition.position = newPos;

            vecTarget.x = Target.position.x;
            vecTarget.y = Mathf.SmoothDamp(CameraLookAt.position.y, Target.position.y + BodyHeight, ref currentVelocityY2, f_DampTime);
            vecTarget.z = Target.position.z;

            CameraLookAt.position = vecTarget;

            //由电影视角，切换到单人视角之间的动画.
            if (animationPlay)
            {
                if (animationTick >= SmoothDampTime)
                {
                    animationTick = 0.0f;
                    animationPlay = false;
                }
                else
                {
                    //当锁定目标丢失，或者死亡时.从双人视角转换为单人视角的摄像机过渡动画.
                    animationTick += FrameReplay.deltaTime;
                    transform.position = Vector3.SmoothDamp(transform.position, newPos, ref currentVelocity, SmoothDampTime, f_speedMax);
                    Quaternion to = Quaternion.LookRotation(CameraLookAt.position - transform.position, Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, to, SmoothDampTime);
                    return;
                }
            }
            else
            {
                transform.position = CameraPosition.position;
                transform.LookAt(CameraLookAt.position);
            }
        }
    }

    //由玩家Y轴25位置向摄像机期望点发射线，如果碰到墙壁返回false，否则true
    bool CameraCanLookTarget(Vector3 pos, out Vector3 hit)
    {
        RaycastHit wallHit;
        Vector3 targetPos = Target.transform.position + new Vector3(0, 25, 0);
        if (Physics.Linecast(targetPos, pos, out wallHit,
            1 << LayerMask.NameToLayer("Scene") |
            (1 << LayerMask.NameToLayer("Default")) |
            (1 << LayerMask.NameToLayer("Wall")) |
            (1 << LayerMask.NameToLayer("Water"))))
        {
            if (!wallHit.collider.isTrigger)
            {
                hit = wallHit.point + Vector3.Normalize(targetPos - wallHit.point);
                return false;
            }
        }
        hit = pos;
        return true;
    }
}
