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
public class CameraFollow:NetBehaviour {
    //public Transform[] m_Targets;//摄像机的各个视角的调试对象.
    public Transform CameraLookAt;//摄像机注视的目标
    public Transform CameraPosition;//摄像机经过缓动后的期望位置
    public Transform Target;//主角
    public Transform LockTarget;//锁定目标，战斗系统里的摄像机针对的除了主角外的第二目标.
    public float followDistance = 55;//在角色身后多远
    public float followHeight = 0;//离跟随点多高。
    public float m_MinSize = 55;//fov最小55
    public float f_speedMax = 150.0f;//移动速度最大限制
    public float f_DampTime = 0.1f;
    [HideInInspector]
    public Camera m_Camera;
    public float fRadis;
    public float lastAngle;
    float angleMax = 75.0f;
    float angleMin = -75.0f;

    SceneItemAgent occlusionItem;//遮蔽住主角与相机间的物件
    new void Awake()
    {
        base.Awake();
        CameraPosition = new GameObject("CameraPosition").transform;
        CameraLookAt = new GameObject("CameraLookAt").transform;
        enabled = false;
    }

    new void OnDestroy() {
        base.OnDestroy();
    }

    private void Start() {
        
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

    /// <summary>
    /// 当跟随目标发生俯仰或旋转
    /// </summary>
    float yRotate = 0;
    float xRotate = 0;
    public void OnTargetRotate(float xDelta, float yDelta)
    {
        yRotate = yDelta;
        xRotate = xDelta;
    }

    /// <summary>
    /// 当跟随目标切换了锁定目标(战斗对象)时
    /// </summary>
    public void OnChangeLockTarget(Transform trans)
    {
        if (DisableLockTarget)
        {
            LockTarget = null;
            return;
        }
        LockTarget = trans;
    }

    //跟踪对象
    public void FollowTarget(Transform followTarget)
    {
        Target = followTarget;
        if (Target != null)
        {
            CameraLookAt.position = new Vector3(Target.position.x, Target.position.y + BodyHeight, Target.position.z);
            CameraPosition.position = (new Vector3(0, followHeight, 0) + CameraLookAt.position + followDistance * (Target.forward));
            transform.position = CameraPosition.position;
            Vector3 vdiff = CameraLookAt.position - transform.position;
            transform.rotation = Quaternion.LookRotation(new Vector3(vdiff.x, 0, vdiff.z), Vector3.up);
        }
        enabled = true;
    }

    public void Init()
    {
        DisableLockTarget = !GameStateMgr.Ins.gameStatus.AutoLock;
        animationPlay = false;
        animationTick = 0.0f;
        followHeight = 6;
        followDistance = 55.0f;
        BodyHeight = 30;
        m_MinSize = 60;
        LookAtAngle = 10.0f;
        SmoothDampYTime = 0.18f;
        SmoothDampTime = 0.18f;
        m_Camera = GetComponent<Camera>();
        m_Camera.fieldOfView = m_MinSize;
        fRadis = Mathf.Sqrt(followDistance * followDistance + followHeight * followHeight);
        lastAngle = Mathf.Atan2(followHeight, followDistance) * Mathf.Rad2Deg;
    }

    static float AutoSwitchTarget = 10.0f;//观察一个战斗团体10秒左右然后检查是否要切换观察位置.
    float lastWatchTick;
    bool freeCamera;
    public bool FreeCamera
    {
        set
        {
            lastWatchTick = 0;
            freeCamera = value;
        }
        get
        {
            return freeCamera;
        }
    }

    public override void NetLateUpdate()
    {
        //如果切换到其他相机,这一步是可以不计算的
        if (m_Camera && !m_Camera.enabled) {
            return;
        }
        if (!enabled)
            return;
        //Debug.LogError("lockupdate:" + Time.frameCount);
        if (Main.Ins.LocalPlayer != null && !CombatData.Ins.PauseAll)
        {
            CameraSmoothFollow();
        }
        else
        {
            if (FreeCamera)
            {
                //摄像机挑选
                OnAutoMove();
            }
        }
        ResetState();
    }

    private void ResetState()
    {
        xRotate = 0;
        yRotate = 0;
    }

    //该角色背后肩膀处
    Vector3 randomOffset;
    Vector3 CalcLookat(MeteorUnit unit)
    {
        if (unit != null)
            return unit.mSkeletonPivot;
        return Vector3.zero;//不改变视向
    }

    Vector3 CalcPosition(MeteorUnit unit)
    {
        if (unit != null)
        {
            return unit.mSkeletonPivot + unit.transform.forward * -55;
        }
        //
        randomOffset.x = 100;
        randomOffset.z = 100;
        randomOffset.y = 0;
        return transform.position + randomOffset;
    }

    void OnAutoMove()
    {
        lastWatchTick += FrameReplay.deltaTime;
        if (lastWatchTick >= AutoSwitchTarget)
        {
            lastWatchTick = 0.0f;
            ChangeAutoTarget();
        }

        if (AutoTarget != null)
        {
            Vector3 newPos = CalcPosition(AutoTarget);
            CameraLookAt.position = CalcLookat(AutoTarget);
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
                    transform.position = Vector3.SmoothDamp(transform.position, newPos, ref currentVelocity, SmoothDampTime, f_speedMax);
                    if (CameraLookAt.position != Vector3.zero)
                    {
                        Quaternion to = Quaternion.LookRotation(CameraLookAt.position - transform.position);
                        transform.rotation = Quaternion.Lerp(transform.rotation, to, SmoothDampTime);
                    }
                }
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, newPos, ref currentVelocity, SmoothDampTime, f_speedMax);
                if (CameraLookAt.position != Vector3.zero)
                {
                    Quaternion to = Quaternion.LookRotation(CameraLookAt.position - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, to, SmoothDampTime);
                }
            }
        }
    }

    MeteorUnit AutoTarget;
    void ChangeAutoTarget()
    {
        OnLockTarget();
        if (MeteorManager.Ins.UnitInfos.Count == 0)
            return;
        int j = -1;
        for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
        {
            if (!MeteorManager.Ins.UnitInfos[i].Dead)
                break;
            j = i;
        }
        if (j >= 0 && j < MeteorManager.Ins.UnitInfos.Count)
            AutoTarget = MeteorManager.Ins.UnitInfos[j];
    }

    //为真则下一帧摄像机要切换视角模式.
    bool animationPlay = false;
    float animationTick = 0.0f;
    public void OnLockTarget()
    {
        animationTick = 0.0f;
        animationPlay = true;
        currentVelocityY = 0;
        currentVelocityY2 = 0;
        currentVelocity = Vector3.zero;
    }

    public void OnUnlockTarget()
    {
        animationTick = 0.0f;
        animationPlay = false;
        currentVelocityY = 0;
        currentVelocityY2 = 0;
        currentVelocity = Vector3.zero;
        LockTarget = null;
    }

    public float SmoothDampTime = 0.18f;//平滑到稳定所需时间.
    public float SmoothDampYTime = 0.18f;//垂直平滑到稳定所需时间
    public float LookAtAngle = 0.0f;//朝目标俯视角度
    public float BodyHeight = 10.0f;//目标脊椎往上多少

    Vector3 currentVelocity = Vector3.zero;
    float currentVelocityY = 0;
    float currentVelocityY2 = 0;

    public Vector3[] newViewIndex = new Vector3[3];
    int ViewIndex = 0;
    //只允许改摄像机的矩阵，不允许修改其他场景对象的矩阵
    protected void CameraSmoothFollow(bool smooth = true)
    {
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

        if (LockTarget != null && !DisableLockTarget)
        {
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
            newViewIndex[0] = (CameraLookAt.position + Quaternion.AngleAxis(-angleY, Vector3.up) * vecForward * CameraRadis);
            newViewIndex[0].y += yHeight;

            newViewIndex[1] = (CameraLookAt.position + Quaternion.AngleAxis(angleY, Vector3.up) * vecForward * CameraRadis);
            newViewIndex[1].y += yHeight;
            newViewIndex[2] = (CameraLookAt.position + Quaternion.AngleAxis(-angleY, Target.right) * vecForward * CameraRadis);//顶部最后考虑

            //vecTarget[0] = newViewIndex[ViewIndex];
            //vecTarget[1] = newViewIndex[(ViewIndex + 1) % 3];
            //vecTarget[2] = newViewIndex[(ViewIndex + 2) % 3];

            //重新排。当前排第一，顶部排最后，剩下的排第二。这样切换就不会那么频繁.
            //for (int i = 0; i < 3; i++)
            //    m_Targets[i].position = newViewIndex[i];
            dis = 1000.0f;
            for (int i = 0; i < 2; i++)
            {
                if (Utility.CameraCanLookTarget(Main.Ins.LocalPlayer, newViewIndex[i], out wallHitPos))
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
                if (Utility.CameraCanLookTarget(Main.Ins.LocalPlayer, newViewIndex[2], out wallHitPos))
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
            if (xRotate != 0.0f) {
                //根据参数调整高度，和距离.
                //最高俯仰75度。
                lastAngle -= xRotate;//鼠标往上，是仰视，往下是俯视
                if (lastAngle >= angleMax) {
                    lastAngle = angleMax;
                    followDistance = Mathf.Abs(fRadis * Mathf.Cos(lastAngle * Mathf.Deg2Rad));
                    followHeight = Mathf.Abs(fRadis * Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                } else if (lastAngle <= angleMin) {
                    lastAngle = angleMin;
                    followDistance = Mathf.Abs(fRadis * Mathf.Cos(lastAngle * Mathf.Deg2Rad));
                    followHeight = -Mathf.Abs(fRadis * Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                } else {
                    followDistance = Mathf.Abs(fRadis * Mathf.Cos(lastAngle * Mathf.Deg2Rad));
                    followHeight = lastAngle == 0.0f ? 0 : (lastAngle / Mathf.Abs(lastAngle)) * Mathf.Abs(fRadis * Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                }
            }

            //相机位置，未插值的
            newPos.x = Target.position.x;
            newPos.y = Target.position.y + BodyHeight + followHeight;
            newPos.z = Target.position.z;
            newPos += Main.Ins.LocalPlayer.transform.forward * followDistance;

            //相机观察的目标-未经过插值的
            vecTarget.x = Target.position.x;
            vecTarget.y = Target.position.y + BodyHeight;
            vecTarget.z = Target.position.z;

            //如果新的位置，与目标注视点中间隔着墙壁
            RaycastHit wallHit;
            bool hitWall = false;
            if (Physics.Linecast(CameraLookAt.position, newPos, out wallHit,
                LayerManager.AllSceneMask))
            {
                hitWall = true;
                //摄像机与角色间有物件遮挡住角色，开始自动计算摄像机位置.
                //场景道具挡住了角色和相机
                Transform trans_parent = wallHit.collider.gameObject.transform.parent;
                bool parent = trans_parent != null;
                bool sceneItem = parent ? trans_parent.CompareTag("SceneItemAgent"):false;
                SceneItemAgent item = sceneItem ? wallHit.collider.gameObject.GetComponentInParent<SceneItemAgent>():null;
                if (item != null) {
                    //不需要移动相机的位置
                    if (occlusionItem != null) {
                        if (item != occlusionItem) {
                            occlusionItem.RestoreAlpha();
                            occlusionItem = item;
                            occlusionItem.SetAlpha(0.3f);
                        }
                    } else {
                        occlusionItem = item;
                        item.SetAlpha(0.3f);
                    }
                }
                else {
                    newPos = wallHit.point + Vector3.Normalize(CameraLookAt.position - wallHit.point) * 5;
                }
            }
            else {
                if (occlusionItem != null) {
                    occlusionItem.RestoreAlpha();
                    occlusionItem = null;
                }
            }
            //没有撞墙
            if (!hitWall)
            {
                newPos.x = Target.position.x;
                newPos.z = Target.position.z;
                float y = Mathf.SmoothDamp(CameraPosition.position.y, newPos.y, ref currentVelocityY, SmoothDampYTime / 2);
                newPos.y = y;
                newPos += Main.Ins.LocalPlayer.transform.forward * followDistance;
            }
            else
            {
                float y = Mathf.SmoothDamp(CameraPosition.position.y, newPos.y, ref currentVelocityY, SmoothDampYTime / 2);
                newPos.y = y;
            }

            CameraPosition.position = newPos;

            vecTarget.x = Target.position.x;
            vecTarget.y = Mathf.SmoothDamp(CameraLookAt.position.y, Target.position.y + BodyHeight, ref currentVelocityY2, SmoothDampYTime / 2);
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
                transform.LookAt(CameraLookAt.position, Vector3.up);
            }
        }
    }
}
