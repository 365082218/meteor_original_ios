using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

//读取skc文件并且绘制骨骼，有一定问题，那个原始模型和骨骼是右手坐标系的
//如果什么都不改，那么最后左右腿是互换了。但是改了半天，左右手坐标系并不好转换
//负责处理动画帧的播放
public class CharacterLoaderDebug : MonoBehaviour
{
    SkinnedMeshRenderer rend;//绘制顶点UV,贴图，骨骼权重
    Material[] mat;
    List<Transform> bo;
    List<Transform> dummy;
    GameObject Skin;
    int CharacterIdx = 0;
    public Transform rootBone;
    //根骨骼初始位置和旋转，用于RootMotion到上一级
    public Vector3 RootPos;
    public Quaternion RootQuat;


    public void LoadCharactor(int id)
    {
        posMng = GetComponent<PoseStatusDebug>();
        owner = GetComponent<MeteorUnitDebug>();
        CharacterIdx = id;
        Skin = new GameObject();
        Skin.transform.SetParent(transform);
        Skin.transform.localRotation = Quaternion.identity;
        Skin.transform.localScale = Vector3.one;
        Skin.transform.localPosition = Vector3.zero;
        SkcFile skc = SkcLoader.Instance.Load(id);
        BncFile bnc = BncLoader.Instance.Load(id);
        Skin.name = skc.Skin;
        rend = Skin.AddComponent<SkinnedMeshRenderer>();
        rend.localBounds = skc.mesh.bounds;
        rend.materials = skc.Material(id, owner.Camp);
        rend.sharedMesh = skc.mesh;
        rend.sharedMesh.RecalculateBounds();
        bo = new List<Transform>();
        dummy = new List<Transform>();
        List<Matrix4x4> bindPos = new List<Matrix4x4>();
        bnc.GenerateBone(transform, ref bo, ref dummy, ref bindPos, ref rootBone);
        rend.bones = bo.ToArray();
        rend.sharedMesh.bindposes = bindPos.ToArray();
        rend.rootBone = rootBone;
        RootPos = rootBone.localPosition;
        RootQuat = rootBone.localRotation;
        AmbLoader.Ins.LoadCharacterAmb(id);
        LoadBoxDef(id);
    }

    //换算后，人物轴向朝Z的负方向，当武器挂载点OK后，可以把轴向调整为朝Z正方向，在之前会引起武器挂载点不对的问题
    public void WeaponInitDone()
    {
        rootBone.localRotation = new Quaternion(0, 1f, 0, 0);
    }
    public PoseStatusDebug posMng;
    public MeteorUnitDebug owner;
    public MeteorUnitDebug mOwner { get { return owner; }}
    void Awake()
    {

    }
    void LoadBoxDef(int idx)
    {
        return;
        idx = 0;
        TextAsset asset = Resources.Load<TextAsset>("boxdef" + idx);
        if (asset == null)
            return;
        MemoryStream ms = new MemoryStream(asset.bytes);
        List<BoxColliderDef> boxdef = Serializer.Deserialize<List<BoxColliderDef>>(ms);
        for (int i = 0; i < bo.Count; i++)
        {
            for (int j = 0; j < boxdef.Count; j++)
            {
                if (boxdef[j].name == bo[i].name)
                {
                    var bodef = bo[i].gameObject.AddComponent<BoxCollider>();
                    bodef.center = boxdef[j].center;
                    bodef.size = boxdef[j].size;
                    bodef.isTrigger = true;
                }
            }
        }
    }

    void Start()
    {
    }

    int lastFrameToKey = 0;//当前的Key帧播放了多久游戏帧.
    int lastFrameIndex = 1;//上一帧序号
    int lastSource = 1;//上一帧使用的动作文件.初始使用角色动画
    int lastPosIdx = 0;
    //Quaternion lastDBaseQuat = Quaternion.identity;
    //一种是用来动作融合的，指定时间内，从当前POS，转化到目标POS的第一帧
    //一种是用帧融合的，表明2个关键帧之间有多少普通帧，这些普通帧都插值

    //POSE切换时，不插值位移，只插值 旋转等
    //pose关键帧切换时，每个POSE第一帧作为TPOSE,如果后面的帧相对当前TPOSE有移动，旋转，那么计算每2个插值帧之间的差距
    //用这个差距来调用charController.Move
    //特效时间不是太准，需要考虑如何让特效和动作同步.
    void Update()
    {
        if (Pause)
            return;
        if (po != null)
        {
            float speedScale = 1.0f;
            for (int i = 0; i < po.ActionList.Count; i++)
            {
                if (curIndex >= po.ActionList[i].Start && curIndex <= po.ActionList[i].End)
                {
                    speedScale = (po.ActionList[i].Speed == 0.0f ? 1.0f : po.ActionList[i].Speed);
                    break;
                }
            }

            if (lastFrameToKey >= (1.0f / speedScale * Time.timeScale))
            {
                lastFrameIndex = curIndex;
                lastSource = po.SourceIdx;
                lastPosIdx = po.Idx;
                int targetIndex = curIndex + 1;
                //超过末尾了.
                if (loop)
                {
                    if (targetIndex > po.LoopEnd)
                        targetIndex = po.LoopStart;
                }
                else
                {
                    if (targetIndex > po.End)
                    {
                        posMng.TickAction();//整个动作完整播放完毕了，到下一个动作的第一帧
                        return;
                    }
                }

                //播放下一个关键帧
                curIndex = targetIndex;
                lastFrameToKey = 0;
            }
            else
            {

            }

            BoneStatus status = null;
            if (po.SourceIdx == 0)
                status = AmbLoader.CharCommon[curIndex];
            else if (po.SourceIdx == 1)
                status = AmbLoader.FrameBoneAni[CharacterIdx][curIndex];

            for (int i = 0; i < bo.Count; i++)
            {
                bo[i].localRotation = status.BoneQuat[i];
                if (i == 0)
                    bo[i].localPosition = status.BonePos;
            }
            for (int i = 0; i < dummy.Count; i++)
            {
                if (i == 0)
                {
                    if (lastPosIdx == po.Idx)
                    {
                        //Vector3 targetPos = status.DummyPos[i];
                        //transform.position = targetPos;
                    }
                }
                else
                {
                    dummy[i].localRotation = status.DummyQuat[i];
                    dummy[i].localPosition = status.DummyPos[i];
                }
            }
            lastFrameToKey += 1;
        }
    }

    //已经在一个状态中，还在持续收到输入，就看这个动作有没有中断，没有就播放NextPose，有则播放中断动作，都没有就回到IDLE，
    //1倍速度 = 1秒30帧
    Pose po;
    int curIndex = 0;
    public int GetCurrentFrameIndex() { return curIndex; }
    bool loop = false;
    bool Pause = false;
    //调试版本禁用融合和一切过渡算法
    public void SetPosData(Pose pos)
    {
        //保存当前帧的姿势，用于和下个动作融合
        //当前状态下有姿势，且帧存在状态缓存
        if (po != null)
            lastPosIdx = po.Idx;
        else
            lastPosIdx = pos.Idx;
        po = pos;//如果Pos为空，那么姿势就会在当前帧锁定.之前的POS无法更改状态
        loop = (pos.LoopStart != pos.LoopEnd);//2帧相同不为0，则是固定帧事件，类似切换武器，在固定一帧触发，Action是播放特效的时机
        curIndex = pos.Start;
        lastFrameToKey = 0;
    }
}