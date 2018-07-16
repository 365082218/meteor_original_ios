using UnityEngine;
using System.Collections;

//仿原smoothfollow脚本，第三人称跟随相机，总在人物背后跟随着角色
//问题：没有鼠标，没法进行视角转换，无法旋转，原流星镜头的旋转是由鼠标相对的滑动控制的。
//而现在没有鼠标了，改为左控制位移，右控制镜头旋转.
/*
 * Nikki Ma
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
    //Vector3 m_DesiredPosition;
    //Vector3 m_MoveVelocity;
    //float m_DampTime;
    //float m_ZoomSpeed;//fov
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

    public void Init()
    {
        smoothIntensity = 15.0f;//移动平滑倍数
        RotateIntensity = 8.0f;
        Unlocked = false;
        followHeight = 6;
        followDistance = 55.0f;
        BodyHeight = 32;
        m_MinSize = 60;
        LookAtAngle = 5.0f;
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

    public void ForceUpdate()
    {
        LateUpdate();
    }

    void LateUpdate()
    {
        if (MeteorManager.Instance.LocalPlayer != null && !Global.PauseAll)
            CameraSmoothFollow();
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
        if (MeteorManager.Instance.LocalPlayer.GetLockedTarget() != null)
        {
            cameraLookAt = (MeteorManager.Instance.LocalPlayer.mPos + MeteorManager.Instance.LocalPlayer.GetLockedTarget().mPos) / 2 + new Vector3(0, 25, 0);
            CameraRadis = Vector3.Distance(MeteorManager.Instance.LocalPlayer.mPos, MeteorManager.Instance.LocalPlayer.GetLockedTarget().mPos) / 2 + 55;
            float dis = Vector3.Distance(new Vector3(MeteorManager.Instance.LocalPlayer.mPos.x, 0, MeteorManager.Instance.LocalPlayer.mPos.z), new Vector3(MeteorManager.Instance.LocalPlayer.GetLockedTarget().mPos.x, 0, MeteorManager.Instance.LocalPlayer.GetLockedTarget().mPos.z));
            Vector3 vecDiff = MeteorManager.Instance.LocalPlayer.mPos - MeteorManager.Instance.LocalPlayer.GetLockedTarget().mPos;
            Vector3 vecForward = Vector3.Normalize(new Vector3(vecDiff.x, 0, vecDiff.z));

            //最远时，15度，最近时85度，其他值。10码 = 80度 140码 15度 约为 y = -0.5x + 85
            //半径最低65，最高
            angleY = -0.5f * dis + 85;
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
            //以下方法，无法避免角色跳跃过锁定目标后，摄像机大幅度转动的问题
            /*
            if (CameraCanLookTarget(vecTarget[0], out wallHitPos))
            {
                //dis = Vector3.Distance(vecTarget[0], cameraLookAt);
                newPos = vecTarget[0];
            }
            else
            {
                dis = Vector3.Distance(wallHitPos, cameraLookAt);
                //如果视角太近了，就切换一下视角.
                if (dis < 40.0f)
                {
                    //如果太近了，在跟前无法看全，尝试切换视角.
                    //如果右侧视角被墙壁遮挡
                    if (CameraCanLookTarget(vecTarget[1], out wallHitPos))
                    {
                        newPos = vecTarget[1];
                        ViewIndex = (ViewIndex + 1) % 3;
                    }
                    else
                    {
                        dis = Vector3.Distance(wallHitPos, cameraLookAt);
                        if (dis < 40.0f)
                        {
                            //如果左侧视角也被墙壁遮挡
                            if (CameraCanLookTarget(vecTarget[2], out wallHitPos))
                            {
                                ViewIndex = (ViewIndex + 2) % 3;
                                newPos = vecTarget[2];
                            }
                            else
                            {
                                ViewIndex = (ViewIndex + 2) % 3;
                                newPos = wallHitPos;
                            }
                        }
                        else
                        {
                            ViewIndex = (ViewIndex + 1) % 3;
                            newPos = wallHitPos;
                        }
                        //如果顶部视角同样被墙壁遮挡,无较好的视角了，使用顶部透墙视角.
                    }
                }
                else
                    newPos = wallHitPos;
            }
            */

            //整个视角都是缓动的
            if (smooth)
                newPos = Vector3.Lerp(transform.position, newPos, smoothIntensity * Time.deltaTime);
            transform.position = newPos;

            //摄像机朝向观察目标
            if (smooth)
            {
                Quaternion to = Quaternion.LookRotation(cameraLookAt - transform.position, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, to, RotateIntensity * Time.deltaTime);
            }
            else
            {
                Quaternion to = Quaternion.LookRotation(cameraLookAt - transform.position, Vector3.up);
                transform.rotation = to;//看向b骨骼顶部25码处
            }

        }
        else
        {
            //没有缓动
            cameraLookAt = new Vector3(MeteorManager.Instance.LocalPlayer.mPos.x, MeteorManager.Instance.LocalPlayer.transform.position.y + BodyHeight, MeteorManager.Instance.LocalPlayer.mPos.z);//朝向焦点
            newPos = cameraLookAt + MeteorManager.Instance.LocalPlayer.transform.forward * followDistance + new Vector3(0, followHeight, 0);
            transform.position = newPos;
            transform.LookAt(cameraLookAt);
            return;


            //摄像机缓动功能先不管，否则抖动很厉害。
            float yRotate = 0.0f;
            //Y轴旋转，受到是否可旋转，以及当前是否有锁定对象决定
            if (MeteorManager.Instance.LocalPlayer.posMng.CanRotateY)
            {
                if (Application.isMobilePlatform)
                    yRotate = NGUICameraJoystick.instance.deltaLast.x * GameData.gameStatus.AxisSensitivity.x;
                else
                    yRotate = 2 * Input.GetAxis("Mouse X");
                if (yRotate != 0)
                    MeteorManager.Instance.LocalPlayer.SetOrientation(yRotate);
            }
            float xRotate = 0;
            if (Application.isMobilePlatform)
                xRotate = NGUICameraJoystick.instance.deltaLast.y * GameData.gameStatus.AxisSensitivity.y;
            else
                xRotate = 2 * Input.GetAxis("Mouse Y");
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
                    followHeight = (lastAngle / Mathf.Abs(lastAngle)) * Mathf.Abs(fRadis * Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                }
            }

            cameraLookAt = new Vector3(MeteorManager.Instance.LocalPlayer.mPos.x, MeteorManager.Instance.LocalPlayer.transform.position.y + BodyHeight, MeteorManager.Instance.LocalPlayer.mPos.z);//朝向焦点
            newPos = cameraLookAt + MeteorManager.Instance.LocalPlayer.transform.forward * followDistance + new Vector3(0, followHeight, 0);
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
                if (Physics.Linecast(cameraLookAt, newPos, out wallHit,
                1 << LayerMask.NameToLayer("Scene") |
                (1 << LayerMask.NameToLayer("Default")) |
                (1 << LayerMask.NameToLayer("Wall")) |
                (1 << LayerMask.NameToLayer("Water"))))
                {
                    //m_Targets[2].position = wallHit.point;
                    //Debug.LogError("?????");
                }
            }

            //只有高度是缓动的，其他轴上都是即刻
            if (Unlocked)
            {
                //当锁定目标丢失，或者死亡时.
                transform.position = newPos;
                //Quaternion to = Quaternion.LookRotation(cameraLookAt - transform.position, Vector3.up);
                //transform.rotation = to;
                transform.LookAt(cameraLookAt);
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
                    Unlocked = false;
                    return;
                }
                else
                {
                    //没被墙壁阻隔，可以缓动Y
                    if (smooth && !hitWall)
                    {
                        newPos.y = Mathf.Lerp(transform.position.y, newPos.y, smoothIntensity * Time.deltaTime);
                        //Debug.LogError("平滑且无墙壁间隔时");
                    }

                    transform.position = newPos;
                    //Quaternion to = Quaternion.LookRotation(cameraLookAt - transform.position, Vector3.up);
                    //transform.rotation = to;
                    transform.LookAt(cameraLookAt);
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

    //private void Move()
    //{
    //    // Find the average position of the targets.
    //    FindAveragePosition();

    //    // Smoothly transition to that position.
    //    transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    //}


    private void FindAveragePosition()
    {
        //Vector3 averagePos = new Vector3();
        //int numTargets = 0;

        //// Go through all the targets and add their positions together.
        //for (int i = 0; i < m_Targets.Length; i++)
        //{
        //    // If the target isn't active, go on to the next one.
        //    if (!m_Targets[i].gameObject.activeSelf)
        //        continue;

        //    // Add to the average and increment the number of targets in the average.
        //    averagePos += m_Targets[i].position;
        //    numTargets++;
        //}

        //// If there are targets divide the sum of the positions by the number of them to find the average.
        //if (numTargets > 0)
        //    averagePos /= numTargets;

        //// Keep the same y value.
        //averagePos.y = transform.position.y;

        //// The desired position is the average position;
        //m_DesiredPosition = averagePos;
    }


    private void Zoom()
    {
        // Find the required size based on the desired position and smoothly transition to that size.
        //float requiredSize = FindRequiredSize();
        //m_Camera.fieldOfView = Mathf.SmoothDamp(m_Camera.fieldOfView, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }


    private float FindRequiredSize()
    {
        //// Find the position the camera rig is moving towards in its local space.
        //Vector3 desiredLocalPos = m_DesiredPosition;

        //// Start the camera's size calculation at zero.
        //float size = 0f;

        //// Go through all the targets...
        //for (int i = 0; i < m_Targets.Length; i++)
        //{
        //    // ... and if they aren't active continue on to the next target.
        //    if (!m_Targets[i].gameObject.activeSelf)
        //        continue;

        //    // Otherwise, find the position of the target in the camera's local space.
        //    Vector3 targetLocalPos = m_Targets[i].position;

        //    // Find the position of the target from the desired position of the camera's local space.
        //    Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

        //    // Choose the largest out of the current size and the distance of the tank 'up' or 'down' from the camera.
        //    size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

        //    // Choose the largest out of the current size and the calculated size based on the tank being to the left or right of the camera.
        //    size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
        //}

        //// Add the edge buffer to the size.
        ////size += m_ScreenEdgeBuffer;

        //// Make sure the camera's size isn't below the minimum.
        //size = Mathf.Max(size, m_MinSize);

        //return size;
        return 0;
    }


    //public void SetStartPositionAndSize()
    //{
    //    // Find the desired position.
    //    FindAveragePosition();

    //    // Set the camera's position to the desired position without damping.
    //    transform.position = m_DesiredPosition;

    //    // Find and set the required size of the camera.
    //    m_Camera.fieldOfView = FindRequiredSize();
    //}

    //这种状态下。由自动控制模块计算位置，当角色距离敌人视距内，且当前无目标时。选择目标。
    public void SetAutoControl()
    {

    }

    //期望位置在角色背面。
    float yOffset;//绕Y轴角度偏移+-90
    float xOffset;//绕X轴角度偏移+-60
    //计算摄像机在角色外围的圆球体中的位置。
    //x轴，y轴 每个轴
    public void AutoMove()
    {
        transform.LookAt(cameraLookAt);//身高的约2/3 36.7
    }
}
