using UnityEngine;
using System.Collections;

//仿原smoothfollow脚本，第三人称跟随相机，总在人物背后跟随着角色
/*
Nikki Ma
奔流河的守候。
7 人赞同了该回答
本人从事电视媒体行业，但对电影电视剧有一些些小涉猎。
一般都是单机拍摄的，一是：多机位导演看不过来。
二是：近景、远景打光都不一样。拍近景的时候，演员旁边、以及下方经常会有一种叫做“米菠萝” （音）的白色泡沫板反光，让演员的轮廓立体、脸色孜然、妆容更好看。就像拍婚纱照，新娘边上的柔光板类似。
三是：摄影机真的挺贵的。多一台机器，多一个摄像、摄助，多一个场记，多一个导演看画面。
所以，一般都是演员演好几遍~~ 也有多机位的，那也是在光照条件、和经费富裕的情况下，还有就是一些只能一次性完成的镜头，比如好贵的一辆车爆炸啊什么的。
四是：电影的摄像机是定焦的，而不是变焦。所谓定焦，就是说拍近景和拍远景，只能依靠变化摄像机和人物的距离来完成。摄像机离你近了，取景范围就小，你的脸就大了。摄像机离你远了，取景范围加大，你全身就露出来了。所以，想像爸爸去哪儿一样，十几个机位一字排开，各自拍不同的景别是不可能的。相对于电影，电视台录节目一般采用变焦头，最大优点就是方便、快捷。但是定焦头拍出来，无论是色彩、画面质感都和定焦头没得比。电影屏幕大，要求高，一般都是定焦头啦。所以如果双机拍，拍全景的时候，前面肯定会有一个摄像机挡在那里拍近景。。。穿帮是肯定的。电影里面用双机的有钱剧组，一般都是同景别不同角度的拍摄，或者像前面说的，只能拍一次的镜头。。。
第一次知乎回答哈，还请指正。
 */
public class CameraFollow : MonoBehaviour {
    public static CameraFollow Ins { get { return Instance; } }
    static CameraFollow Instance;
    [HideInInspector]
    //public Transform[] m_Targets;//摄像机的各个视角的调试对象.
    public float followDistance = 50;//在角色身后多远
    public float followHeight = 0;//离跟随点多高。
    public float followRotationDamping = 5;
    public float followHeightDamping = 5;
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

    bool DisableLockTarget;

    public void DisableLock()
    {
        DisableLockTarget = true;
    }
    public void EnableLock()
    {
        DisableLockTarget = false;
    }

    public void Init()
    {
        DisableLockTarget = GameData.Instance.gameStatus.DisableLock;
        smoothIntensity = 15.0f;//移动平滑倍数
        RotateIntensity = 8.0f;
        Unlocked = false;
        followHeight = 6;
        followDistance = 55.0f;
        BodyHeight = 30;
        m_MinSize = 60;
        LookAtAngle = 10.0f;
        m_Camera = GetComponent<Camera>();
        m_Camera.fieldOfView = m_MinSize;
        fRadis = Mathf.Sqrt(followDistance * followDistance + followHeight * followHeight);
        lastAngle = Mathf.Atan2(followHeight, followDistance) * Mathf.Rad2Deg;
        //m_Targets = new Transform[3];
        //for (int i = 0; i < 3; i++)
        //{
        //    m_Targets[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        //    m_Targets[i].GetComponent<Renderer>().material.color = Color.white;
        //    m_Targets[i].GetComponent<Collider>().enabled = false;
        //    m_Targets[i].gameObject.layer = LayerMask.NameToLayer("Debug");
        //    m_Targets[i].localScale = 10 * Vector3.one;
        //}

        if (MeteorManager.Instance.LocalPlayer != null)
        {
            cameraLookAt = new Vector3(MeteorManager.Instance.LocalPlayer.mPos.x, MeteorManager.Instance.LocalPlayer.transform.position.y + BodyHeight, MeteorManager.Instance.LocalPlayer.mPos.z);
            Vector3 vpos = new Vector3(0, followHeight, 0) + cameraLookAt + followDistance * (MeteorManager.Instance.LocalPlayer.transform.forward);
            transform.position = vpos;
            Vector3 vdiff = MeteorManager.Instance.LocalPlayer.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(new Vector3(vdiff.x, 0, vdiff.z), Vector3.up);
        }
    }

    public bool Smooth = true;
    public void ForceUpdate()
    {
        if (MeteorManager.Instance.LocalPlayer != null && !Global.PauseAll)
            CameraSmoothFollow(Smooth);
    }

    void Update()
    {
        if (MeteorManager.Instance.LocalPlayer != null && !Global.PauseAll)
            CameraSmoothFollow(Smooth);
    }

    //为真则下一帧摄像机要看向目标.
    bool Unlocked;
    public void OnUnlockTarget()
    {
        Unlocked = true;
    }

    public float smoothIntensity = 20.0f;//移动平滑倍数
    public float RotateIntensity = 8.0f;//旋转平滑倍数
    public float LookAtAngle = 0.0f;//朝目标俯视角度
    public float BodyHeight = 10.0f;//目标脊椎往上多少
    Vector3 LasthitOffset = Vector3.zero;
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
        if (MeteorManager.Instance.LocalPlayer.GetLockedTarget() != null && !DisableLockTarget)
        {
            cameraLookAt = (MeteorManager.Instance.LocalPlayer.mPos + MeteorManager.Instance.LocalPlayer.GetLockedTarget().mPos) / 2 + new Vector3(0, 25, 0);
            CameraRadis = Vector3.Distance(MeteorManager.Instance.LocalPlayer.mPos, MeteorManager.Instance.LocalPlayer.GetLockedTarget().mPos) / 2 + 50;
            float dis = Vector3.Distance(new Vector3(MeteorManager.Instance.LocalPlayer.mPos.x, 0, MeteorManager.Instance.LocalPlayer.mPos.z), new Vector3(MeteorManager.Instance.LocalPlayer.GetLockedTarget().mPos.x, 0, MeteorManager.Instance.LocalPlayer.GetLockedTarget().mPos.z));
            Vector3 vecDiff = MeteorManager.Instance.LocalPlayer.mPos - MeteorManager.Instance.LocalPlayer.GetLockedTarget().mPos;
            Vector3 vecForward = Vector3.Normalize(new Vector3(vecDiff.x, 0, vecDiff.z));

            //最远时，15度，最近时95度，其他值。10码 = 80度 140码 15度 约为 y = -0.5x + 85
            //半径最低65，最高
            angleY = -0.5f * dis + 95;
            float yHeight = Mathf.Tan(LookAtAngle * Mathf.Deg2Rad) * CameraRadis;
            newViewIndex[0] = cameraLookAt + Quaternion.AngleAxis(-angleY, Vector3.up) * vecForward * CameraRadis;
            newViewIndex[0].y += yHeight;

            newViewIndex[1] = cameraLookAt + Quaternion.AngleAxis(angleY, Vector3.up) * vecForward * CameraRadis;
            newViewIndex[1].y += yHeight;
            newViewIndex[2] = cameraLookAt + Quaternion.AngleAxis(-angleY, MeteorManager.Instance.LocalPlayer.transform.right) * vecForward * CameraRadis;//顶部最后考虑

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
            if (smooth)
            {
                float x = Mathf.Lerp(transform.position.x, newPos.x, smoothIntensity * Time.smoothDeltaTime);
                float y = Mathf.Lerp(transform.position.y, newPos.y, smoothIntensity * Time.smoothDeltaTime);
                float z = Mathf.Lerp(transform.position.z, newPos.z, smoothIntensity * Time.smoothDeltaTime);
                newPos.x = x;
                newPos.y = y;
                newPos.z = z;
            }
            transform.position = newPos;
            //摄像机朝向观察目标
            if (smooth)
            {
                Quaternion to = Quaternion.LookRotation(cameraLookAt - transform.position, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, to, RotateIntensity * Time.smoothDeltaTime);
                Vector3 vec = transform.eulerAngles;
                vec.z = 0;
                transform.eulerAngles = vec;
            }
            else
            {
                Quaternion to = Quaternion.LookRotation(cameraLookAt - transform.position, Vector3.up);
                transform.rotation = to;//看向b骨骼顶部25码处
                Vector3 vec = transform.eulerAngles;
                vec.z = 0;
                transform.eulerAngles = vec;
            }
        }
        else
        {
            //没有缓动
            //cameraLookAt = new Vector3(MeteorManager.Instance.LocalPlayer.mPos.x, MeteorManager.Instance.LocalPlayer.transform.position.y + BodyHeight, MeteorManager.Instance.LocalPlayer.mPos.z);//朝向焦点
            //newPos = cameraLookAt + MeteorManager.Instance.LocalPlayer.transform.forward * followDistance + new Vector3(0, followHeight, 0);
            //transform.position = newPos;
            //transform.LookAt(cameraLookAt);
            //return;


            //摄像机缓动功能先不管，否则抖动很厉害。
            float yRotate = 0.0f;
            //Y轴旋转，受到是否可旋转，以及当前是否有锁定对象决定
            if (MeteorManager.Instance.LocalPlayer.posMng.CanRotateY)
            {
#if STRIP_KEYBOARD
                //Debug.LogError(string.Format("deltaLast.x:{0}", NGUICameraJoystick.instance.deltaLast.x));
                yRotate = NGUICameraJoystick.instance.deltaLast.x * GameData.Instance.gameStatus.AxisSensitivity.x;
#else
                yRotate = 2 * Input.GetAxis("Mouse X");
#endif
                if (yRotate != 0)
                    MeteorManager.Instance.LocalPlayer.SetOrientation(yRotate);
            }
            float xRotate = 0;
#if STRIP_KEYBOARD
            xRotate = NGUICameraJoystick.instance.deltaLast.y * GameData.Instance.gameStatus.AxisSensitivity.y;
#else
            xRotate = 2 * Input.GetAxis("Mouse Y");
#endif
            if (xRotate != 0.0f)
            {
                //根据参数调整高度，和距离.
                //最高俯仰75度。
                lastAngle -= xRotate;//鼠标往上，是仰视，往下是俯视
                if (lastAngle >= angleMax)
                {
                    lastAngle = angleMax;
                    followDistance = Mathf.Abs(fRadis * Mathf.Cos(lastAngle * Mathf.Deg2Rad));
                    followHeight = Mathf.Abs(fRadis * Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                }
                else if (lastAngle <= angleMin)
                {
                    lastAngle = angleMin;
                    followDistance = Mathf.Abs(fRadis * Mathf.Cos(lastAngle * Mathf.Deg2Rad));
                    followHeight = -Mathf.Abs(fRadis * Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                }
                else
                {
                    followDistance = Mathf.Abs(fRadis * Mathf.Cos(lastAngle * Mathf.Deg2Rad));
                    followHeight = lastAngle == 0.0f ? 0 : (lastAngle / Mathf.Abs(lastAngle)) * Mathf.Abs(fRadis * Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                }
            }

            cameraLookAt.x = MeteorManager.Instance.LocalPlayer.mPos.x;
            cameraLookAt.y = MeteorManager.Instance.LocalPlayer.transform.position.y + BodyHeight;//朝向焦点
            cameraLookAt.z = MeteorManager.Instance.LocalPlayer.mPos.z;

            //Debug.Log("followHeight:" + followHeight);
            newPos = cameraLookAt + MeteorManager.Instance.LocalPlayer.transform.forward * followDistance;
            newPos.y += followHeight;
            RaycastHit wallHit;
            bool hitWall = false;
            if (Physics.Linecast(cameraLookAt, newPos, out wallHit,
                1 << LayerMask.NameToLayer("Scene") |
                (1 << LayerMask.NameToLayer("Default")) |
                (1 << LayerMask.NameToLayer("Wall")) |
                (1 << LayerMask.NameToLayer("Water"))))
            {
                hitWall = true;
                //Debug.LogError("hitWall" + wallHit.transform.name);
                //摄像机与角色间有物件遮挡住角色，开始自动计算摄像机位置.
                //Debug.LogError("camera linecast with:" + wallHit.transform.name);
                newPos = wallHit.point + Vector3.Normalize(cameraLookAt - wallHit.point) * 5;
                //if (Physics.Linecast(cameraLookAt, newPos, out wallHit,
                //1 << LayerMask.NameToLayer("Scene") |
                //(1 << LayerMask.NameToLayer("Default")) |
                //(1 << LayerMask.NameToLayer("Wall")) |
                //(1 << LayerMask.NameToLayer("Water"))))
                //{
                //    m_Targets[2].position = wallHit.point;
                //    Debug.LogError("?????");
                //}
            }

            //只有高度是缓动的，其他轴上都是即刻
            if (Unlocked)
            {
                //当锁定目标丢失，或者死亡时.
                transform.position = newPos;
                //Quaternion to = Quaternion.LookRotation(cameraLookAt - transform.position, Vector3.up);
                //transform.rotation = to;
                transform.LookAt(cameraLookAt);
                Vector3 vec = transform.eulerAngles;
                vec.z = 0;
                transform.eulerAngles = vec;
                Unlocked = false;
                return;
            }
            else
            {
                if (xRotate != 0.0f || yRotate != 0.0f)
                {
                    //Debug.LogError("有x y轴偏移");
                    transform.position = newPos;
                    //Quaternion to = Quaternion.LookRotation(cameraLookAt - transform.position, Vector3.up);
                    //transform.rotation = to;
                    transform.LookAt(cameraLookAt);
                    Vector3 vec = transform.eulerAngles;
                    vec.z = 0;
                    transform.eulerAngles = vec;
                    Unlocked = false;
                    return;
                }
                else
                {
                    //没被墙壁阻隔，可以缓动Y
                    if (smooth && !hitWall)
                    {
                        newPos.y = Mathf.Lerp(transform.position.y, newPos.y, smoothIntensity * Time.smoothDeltaTime);
                        //Debug.LogError("平滑且无墙壁间隔时");
                    }

                    transform.position = newPos;
                    //Quaternion to = Quaternion.LookRotation(cameraLookAt - transform.position, Vector3.up);
                    //transform.rotation = to;
                    transform.LookAt(cameraLookAt);
                    Vector3 vec = transform.eulerAngles;
                    vec.z = 0;
                    transform.eulerAngles = vec;
                    return;
                }
            }
        }
    }

    //由玩家Y轴25位置向摄像机期望点发射线，如果碰到墙壁返回false，否则true
    bool CameraCanLookTarget(Vector3 pos, out Vector3 hit)
    {
        RaycastHit wallHit;
        Vector3 targetPos = MeteorManager.Instance.LocalPlayer.mPos + new Vector3(0, 25, 0);
        if (Physics.Linecast(targetPos, pos, out wallHit,
            1 << LayerMask.NameToLayer("Scene") |
            (1 << LayerMask.NameToLayer("Default")) |
            (1 << LayerMask.NameToLayer("Wall")) |
            (1 << LayerMask.NameToLayer("Water"))))
        {
            hit = wallHit.point + Vector3.Normalize(targetPos - wallHit.point);
            return false;
        }
        hit = pos;
        return true;
    }
}
