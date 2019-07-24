using UnityEngine;
using System.Collections;
//自由相机，当主角挂掉以后，在场景内随机找1个目标，优先找正在打斗的其中一个.
public class CameraFree : MonoBehaviour {
    public static CameraFree Ins { get { return Instance; } }
    static CameraFree Instance;
    [HideInInspector]
    //public Transform[] m_Targets;//摄像机的各个视角的调试对象.
    public float followDistance = 50;//在角色身后多远
    public float followHeight = 0;//离跟随点多高。
    public float m_MinSize = 55;//fov最小55
    [HideInInspector]
    public Camera m_Camera;
    public Vector3 cameraLookAt = Vector3.zero;
    //Transform cameraPosFollow;
    public float fRadis;
    public float lastAngle;
    float angleMax = 75.0f;
    float angleMin = -75.0f;
    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    MeteorUnit Target;
    public void Init(MeteorUnit target)
    {
        GameObject.Destroy(Startup.ins.playerListener);
        Startup.ins.playerListener = gameObject.AddComponent<AudioListener>();
        Target = target;
        animationPlay = false;
        animationTick = 0.0f;
        followHeight = 10;
        followDistance = 85.0f;
        BodyHeight = 30;
        m_MinSize = 60;
        LookAtAngle = 10.0f;
        m_Camera = GetComponent<Camera>();
        m_Camera.fieldOfView = m_MinSize;
        fRadis = Mathf.Sqrt(followDistance * followDistance + followHeight * followHeight);
        lastAngle = Mathf.Atan2(followHeight, followDistance) * Mathf.Rad2Deg;

        if (target != null)
        {
            cameraLookAt = new Vector3(target.transform.position.x, target.transform.position.y + BodyHeight, target.transform.position.z);
            Vector3 vpos = new Vector3(0, followHeight, 0) + cameraLookAt + followDistance * (target.transform.forward);
            transform.position = vpos;
            Vector3 vdiff = target.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(new Vector3(vdiff.x, 0, vdiff.z), Vector3.up);
        }
    }

    public bool Smooth = true;
    public void ForceUpdate()
    {
        if (Target != null && !Global.Instance.PauseAll)
            CameraSmoothFollow(Smooth);
    }

    void LateUpdate()
    {
        if (Target != null)
        {
            if (Target.Dead)
            {
                RefreshTarget();
            }
        }
        if (Target != null && !Global.Instance.PauseAll)
        {
            CameraSmoothFollow(Smooth);
        }
    }

    public void RefreshTarget()
    {
        MeteorUnit watchTarget = null;
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].Dead)
                continue;
            if (watchTarget == null)
                watchTarget = MeteorManager.Instance.UnitInfos[i];
            if (MeteorManager.Instance.UnitInfos[i].GetLockedTarget() != null)
            {
                watchTarget = MeteorManager.Instance.UnitInfos[i];
                break;
            }
        }
        Target = watchTarget;
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

    public float SmoothDampTime = 0.1f;//平滑到稳定所需时间.
    public float LookAtAngle = 0.0f;//朝目标俯视角度
    public float BodyHeight = 10.0f;//目标脊椎往上多少

    float currentVelocityX;
    float currentVelocityY;
    float currentVelocityZ;
    float currentEulerVelocityX;
    float currentEulerVelocityY;
    float currentEulerVelocityZ;

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

        if (Target.GetLockedTarget() != null)
        {
            //有锁定目标
            //开启摄像机锁定系统
            cameraLookAt = (Target.transform.position + Target.GetLockedTarget().transform.position) / 2 + new Vector3(0, 25, 0);
            CameraRadis = Vector3.Distance(Target.transform.position, Target.GetLockedTarget().transform.position) / 2 + 60;
            float dis = Vector3.Distance(new Vector3(Target.transform.position.x, 0, Target.transform.position.z), new Vector3(Target.GetLockedTarget().transform.position.x, 0, Target.GetLockedTarget().transform.position.z));
            Vector3 vecDiff = Target.transform.position - Target.GetLockedTarget().transform.position;
            Vector3 vecForward = Vector3.Normalize(new Vector3(vecDiff.x, 0, vecDiff.z));

            //最远时，15度，最近时95度，其他值。10码 = 80度 140码 15度 约为 y = -0.5x + 85
            //半径最低65，最高
            angleY = -0.5f * dis + 95;
            float yHeight = Mathf.Tan(LookAtAngle * Mathf.Deg2Rad) * CameraRadis;
            newViewIndex[0] = cameraLookAt + Quaternion.AngleAxis(-angleY, Vector3.up) * vecForward * CameraRadis;
            newViewIndex[0].y += yHeight;

            newViewIndex[1] = cameraLookAt + Quaternion.AngleAxis(angleY, Vector3.up) * vecForward * CameraRadis;
            newViewIndex[1].y += yHeight;
            newViewIndex[2] = cameraLookAt + Quaternion.AngleAxis(-angleY, Target.transform.right) * vecForward * CameraRadis;//顶部最后考虑

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
                    float disWall = Vector3.Distance(wallHitPos, cameraLookAt);//墙壁与看向目标的距离一定要足够，否则视角堵着很难受.
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
            newPos.x = Mathf.SmoothDamp(transform.position.x, newPos.x, ref currentVelocityX, SmoothDampTime, 150);
            newPos.y = Mathf.SmoothDamp(transform.position.y, newPos.y, ref currentVelocityY, SmoothDampTime, 150);
            newPos.z = Mathf.SmoothDamp(transform.position.z, newPos.z, ref currentVelocityZ, SmoothDampTime, 150);
            transform.position = newPos;
            //摄像机朝向观察目标,平滑的旋转视角
            Quaternion to = Quaternion.LookRotation(cameraLookAt - transform.position, Vector3.up);
            Vector3 vec = to.eulerAngles;
            vec.x = Mathf.SmoothDampAngle(transform.eulerAngles.x, to.eulerAngles.x, ref currentEulerVelocityX, SmoothDampTime, 60);
            vec.y = Mathf.SmoothDampAngle(transform.eulerAngles.y, to.eulerAngles.y, ref currentEulerVelocityY, SmoothDampTime, 60);
            vec.z = 0;
            transform.eulerAngles = vec;
        }
        else
        {
            cameraLookAt.x = Target.transform.position.x;
            cameraLookAt.y = Target.transform.position.y + BodyHeight;//朝向焦点
            cameraLookAt.z = Target.transform.position.z;
            newPos = cameraLookAt + Target.transform.forward * followDistance;
            newPos.y += followHeight;
            RaycastHit wallHit;
            bool hitWall = false;
            if (Physics.Linecast(cameraLookAt, newPos, out wallHit,
                1 << LayerMask.NameToLayer("Scene") |
                (1 << LayerMask.NameToLayer("Default")) |
                (1 << LayerMask.NameToLayer("Wall")) |
                (1 << LayerMask.NameToLayer("Water"))))
            {
                if (!wallHit.collider.isTrigger)
                {
                    hitWall = true;
                    newPos = wallHit.point + Vector3.Normalize(cameraLookAt - wallHit.point) * 5;
                }
            }

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
                    animationTick += Time.deltaTime;
                    //当锁定目标丢失，或者死亡时.从双人视角转换为单人视角的摄像机过渡动画.
                    newPos.x = Mathf.SmoothDamp(transform.position.x, newPos.x, ref currentVelocityX, SmoothDampTime);
                    newPos.y = Mathf.SmoothDamp(transform.position.y, newPos.y, ref currentVelocityY, SmoothDampTime);
                    newPos.z = Mathf.SmoothDamp(transform.position.z, newPos.z, ref currentVelocityZ, SmoothDampTime);
                    transform.position = newPos;
                    transform.LookAt(cameraLookAt);
                    Vector3 vec = transform.eulerAngles;
                    vec.z = 0;
                    transform.eulerAngles = vec;
                    return;
                }
            }
            else
            {

                //没被墙壁阻隔，可以缓动Y
                if (smooth && !hitWall)
                    newPos.y = Mathf.SmoothDamp(transform.position.y, newPos.y, ref currentVelocityY, SmoothDampTime);
                transform.position = newPos;
                Vector3 vec = transform.eulerAngles;
                transform.LookAt(cameraLookAt);
                    
                vec.x = Mathf.SmoothDampAngle(vec.x, transform.eulerAngles.x, ref currentEulerVelocityX, SmoothDampTime);
                vec.y = transform.eulerAngles.y;
                vec.z = 0;
                transform.eulerAngles = vec;
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
