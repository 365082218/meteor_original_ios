//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using ProtoBuf;

////读取skc文件并且绘制骨骼，有一定问题，那个原始模型和骨骼是右手坐标系的
////如果什么都不改，那么最后左右腿是互换了。但是改了半天，左右手坐标系并不好转换
////负责处理动画帧的播放
//public class CharacterLoaderDebug : MonoBehaviour
//{
//    SkinnedMeshRenderer rend;//绘制顶点UV,贴图，骨骼权重
//    Material[] mat;
//    List<Transform> bo;
//    List<Transform> dummy;
//    GameObject Skin;
//    int CharacterIdx = 0;
//    public Transform rootBone;
//    //根骨骼初始位置和旋转，用于RootMotion到上一级
//    public Vector3 RootPos;
//    public Quaternion RootQuat;


//    public void LoadCharactor(int id)
//    {
//        posMng = GetComponent<PoseStatusDebug>();
//        owner = GetComponent<MeteorUnitDebug>();
//        CharacterIdx = id;
//        Skin = new GameObject();
//        Skin.transform.SetParent(transform);
//        Skin.transform.localRotation = Quaternion.identity;
//        Skin.transform.localScale = Vector3.one;
//        Skin.transform.localPosition = Vector3.zero;
//        SkcFile skc = SkcLoader.Instance.Load(id);
//        BncFile bnc = BncLoader.Instance.Load(id);
//        Skin.name = skc.Skin;
//        rend = Skin.AddComponent<SkinnedMeshRenderer>();
//        rend.localBounds = skc.mesh.bounds;
//        rend.materials = skc.Material(id, owner.Camp);
//        rend.sharedMesh = skc.mesh;
//        rend.sharedMesh.RecalculateBounds();
//        bo = new List<Transform>();
//        dummy = new List<Transform>();
//        List<Matrix4x4> bindPos = new List<Matrix4x4>();
//        bnc.GenerateBone(transform, ref bo, ref dummy, ref bindPos, ref rootBone);
//        rend.bones = bo.ToArray();
//        rend.sharedMesh.bindposes = bindPos.ToArray();
//        rend.rootBone = rootBone;
//        RootPos = rootBone.localPosition;
//        RootQuat = rootBone.localRotation;
//        AmbLoader.Ins.LoadCharacterAmb(id);
//        LoadBoxDef(id);
//    }

//    //换算后，人物轴向朝Z的负方向，当武器挂载点OK后，可以把轴向调整为朝Z正方向，在之前会引起武器挂载点不对的问题
//    public void WeaponInitDone()
//    {
//        rootBone.localRotation = new Quaternion(0, 1f, 0, 0);
//    }
//    public PoseStatusDebug posMng;
//    public MeteorUnitDebug owner;
//    public MeteorUnitDebug mOwner { get { return owner; }}
//    void Awake()
//    {

//    }
//    void LoadBoxDef(int idx)
//    {
//        return;
//        idx = 0;
//        TextAsset asset = Resources.Load<TextAsset>("boxdef" + idx);
//        if (asset == null)
//            return;
//        MemoryStream ms = new MemoryStream(asset.bytes);
//        List<BoxColliderDef> boxdef = Serializer.Deserialize<List<BoxColliderDef>>(ms);
//        for (int i = 0; i < bo.Count; i++)
//        {
//            for (int j = 0; j < boxdef.Count; j++)
//            {
//                if (boxdef[j].name == bo[i].name)
//                {
//                    var bodef = bo[i].gameObject.AddComponent<BoxCollider>();
//                    bodef.center = boxdef[j].center;
//                    bodef.size = boxdef[j].size;
//                    bodef.isTrigger = true;
//                }
//            }
//        }
//    }

//    void Start()
//    {
//    }

//    int lastFrameToKey = 0;//当前的Key帧播放了多久游戏帧.
//    int lastFrameIndex = 1;//上一帧序号
//    int lastSource = 1;//上一帧使用的动作文件.初始使用角色动画
//    int lastPosIdx = 0;
//    //Quaternion lastDBaseQuat = Quaternion.identity;
//    //一种是用来动作融合的，指定时间内，从当前POS，转化到目标POS的第一帧
//    //一种是用帧融合的，表明2个关键帧之间有多少普通帧，这些普通帧都插值

//    //POSE切换时，不插值位移，只插值 旋转等
//    //pose关键帧切换时，每个POSE第一帧作为TPOSE,如果后面的帧相对当前TPOSE有移动，旋转，那么计算每2个插值帧之间的差距
//    //用这个差距来调用charController.Move
//    //特效时间不是太准，需要考虑如何让特效和动作同步.
//    void PlayNextKeyFrame(float delta)
//    {
//        TryPlayEffect();
//        ChangeAttack();
//        ChangeWeaponTrail();

//        if (PoseEvent.ContainsKey(po.Idx))
//        {
//            //当218发射飞轮，很快返回，还未到219动作时，下次播放219，就得立即取消循环，221 223
//            PoseEvent.Remove(po.Idx);
//            loop = false;
//            curIndex = po.LoopEnd;
//        }

//        //有连招.
//        if (TestInputLink())
//            return;
//        if (loop)
//        {
//            if (curIndex > po.LoopEnd)
//            {
//                if (curIndex > po.LoopStart)
//                {
//                    PlayPosEvent();
//                    curIndex = po.LoopStart;
//                    if (loop)
//                        return;
//                }
//                curIndex = po.LoopStart;
//            }
//        }
//        else
//        {
//            if (curIndex > po.End)
//            {
//                if (single)
//                    Pause = true;
//                else
//                    posMng.OnActionFinished();
//                return;
//            }

//            if (TheFirstFrame == curIndex)
//                ActionEvent.HandlerFirstActionFrame(mOwner, po.Idx);
//            if (TheLastFrame == curIndex)
//                ActionEvent.HandlerFinalActionFrame(mOwner, po.Idx);
//        }

//        BoneStatus status = null;
//        if (po.SourceIdx == 0)
//            status = AmbLoader.CharCommon[curIndex];
//        else if (po.SourceIdx == 1)
//            status = AmbLoader.FrameBoneAni[CharacterIdx][curIndex];

//        if (mOwner.Attr.IsPlayer && FightWnd.Exist)
//            FightWnd.Instance.UpdatePoseStatus(po.Idx, curIndex);

//        for (int i = 0; i < bo.Count; i++)
//        {
//            bo[i].localRotation = status.BoneQuat[i];
//            if (i == 0)
//                bo[i].localPosition = status.BonePos;
//        }

//        bool IgnoreActionMoves = IgnoreActionMove(po.Idx);
//        for (int i = 0; i < dummy.Count; i++)
//        {
//            if (i == 0)
//            {
//                if (lastPosIdx == po.Idx)
//                {
//                    Vector3 targetPos = status.DummyPos[i];
//                    Vector3 vec = transform.rotation * (targetPos - lastDBasePos) * moveScale;
//                    if (IgnoreActionMoves)
//                    {
//                        vec.x = 0;
//                        vec.z = 0;
//                        vec.y = 0;
//                    }
//                    else
//                    {
//                    }
//                    moveDelta += vec;
//                    lastDBasePos = targetPos;
//                }
//            }
//            else
//            {
//                dummy[i].localRotation = status.DummyQuat[i];
//                dummy[i].localPosition = status.DummyPos[i];
//            }
//        }

//        lastFrameIndex = curIndex;
//        curIndex++;
//        lastSource = po.SourceIdx;
//        lastPosIdx = po.Idx;
//    }

//    float GetSpeedScale()
//    {
//        float speedScale = 1.0f;
//        for (int i = 0; i < po.ActionList.Count; i++)
//        {
//            if (curIndex >= po.ActionList[i].Start && curIndex <= po.ActionList[i].End)
//            {
//                speedScale = (po.ActionList[i].Speed == 0.0f ? 1.0f : po.ActionList[i].Speed);
//                break;
//            }
//        }
//        return speedScale;
//    }

//    public Vector3 moveDelta;//上一帧的位移
//    float lastFramePlayedTimes;

//    void TryPlayEffect()
//    {
//        if (!effectPlayed)
//        {
//            //try
//            //{
//            PlayEffect();
//            //}
//            //catch (System.Exception exp)
//            //{
//            //    Debug.LogError("Effect: [" + po.EffectID + " ] Contains Error" + exp.StackTrace);
//            //    effectPlayed = true;
//            //}
//            effectPlayed = true;
//        }
//    }

//    void ChangeAttack()
//    {
//        bool open = false;
//        for (int i = 0; i < po.Attack.Count; i++)
//        {
//            if (curIndex >= po.Attack[i].Start && curIndex <= po.Attack[i].End)
//            {
//                //当前处于不允许攻击，才能切换到允许攻击
//                //if (!mOwner.allowAttack)
//                mOwner.ChangeAttack(po.Attack[i]);
//                open = true;
//                break;
//            }
//        }

//        if (!open)
//        {
//            //if (owner.allowAttack)
//            mOwner.ChangeAttack(null);
//        }
//    }

//    void ChangeWeaponTrail()
//    {
//        //开启武器拖尾
//        if (po.Drag != null)
//        {
//            if (curIndex >= po.Drag.Start && curIndex <= po.Drag.End)
//                mOwner.ChangeWeaponTrail(po.Drag);
//            else
//                mOwner.ChangeWeaponTrail(null);
//        }
//        else
//            mOwner.ChangeWeaponTrail(null);
//    }

//    bool TestInputLink()
//    {
//        //有连招等待播放
//        if (posMng.LinkInput.ContainsKey(po.Idx))
//        {
//            //当前正处于融合帧中，可以立即切换动画
//            for (int i = 0; i < po.ActionList.Count; i++)
//            {
//                if (po.ActionList[i].Type == "Blend")
//                {
//                    if (curIndex >= po.ActionList[i].Start && curIndex <= po.ActionList[i].End)
//                    {
//                        int targetIdx = po.Idx;
//                        if (po.Next != null)
//                            posMng.ChangeAction(posMng.LinkInput[targetIdx], po.Next.Time);//
//                        else
//                            posMng.ChangeAction(posMng.LinkInput[targetIdx]);
//                        posMng.LinkInput.Clear();
//                        return true;
//                    }
//                }
//            }
//        }
//        return false;
//    }

//    public void PlayFrame(float timeRatio)
//    {
//        float speedScale = GetSpeedScale();
//        TryPlayEffect();
//        ChangeAttack();
//        ChangeWeaponTrail();
//        if (TestInputLink())
//            return;
//        //超过末尾了.
//        if (loop)
//        {
//            if (curIndex >= po.LoopEnd)
//            {
//                if (curIndex >= po.LoopStart && po.LoopStart == po.LoopEnd)
//                {
//                    PlayPosEvent();
//                    curIndex = po.LoopStart;
//                    if (loop)
//                        return;
//                }
//                curIndex = po.LoopStart;
//            }
//        }
//        else
//        {
//            if (curIndex >= po.End)
//            {
//                if (single)
//                    Pause = true;
//                else
//                    posMng.OnActionFinished();
//                return;
//            }
//            if (TheFirstFrame == curIndex)
//                ActionEvent.HandlerFirstActionFrame(mOwner, po.Idx);
//            if (TheLastFrame == curIndex)
//                ActionEvent.HandlerFinalActionFrame(mOwner, po.Idx);
//        }

//        //curIndex = targetIndex;
//        BoneStatus status = null;
//        BoneStatus lastStatus = null;
//        if (lastSource == 0)
//            lastStatus = AmbLoader.CharCommon[lastFrameIndex];
//        else
//            lastStatus = AmbLoader.FrameBoneAni[CharacterIdx][lastFrameIndex];
//        if (po.SourceIdx == 0)
//            status = AmbLoader.CharCommon[curIndex];
//        else if (po.SourceIdx == 1)
//            status = AmbLoader.FrameBoneAni[CharacterIdx][curIndex];

//        if (mOwner.Attr.IsPlayer && FightWnd.Exist)
//            FightWnd.Instance.UpdatePoseStatus(po.Idx, lastFrameIndex);

//        for (int i = 0; i < bo.Count; i++)
//        {
//            //if (bo[i] == owner.HeadBone && GameBattleEx.Instance.autoTarget != null && owner.Attr.IsPlayer)
//            //{

//            //}
//            //else
//            bo[i].localRotation = Quaternion.Slerp(lastStatus.BoneQuat[i], status.BoneQuat[i], timeRatio);

//            if (i == 0)
//                bo[i].localPosition = Vector3.Lerp(lastStatus.BonePos, status.BonePos, timeRatio);
//        }

//        bool IgnoreActionMoves = IgnoreActionMove(po.Idx);
//        for (int i = 0; i < dummy.Count; i++)
//        {
//            if (i == 0)
//            {
//                if (lastPosIdx == po.Idx)
//                {
//                    Vector3 targetPos = Vector3.Lerp(lastStatus.DummyPos[i], status.DummyPos[i], timeRatio);
//                    Vector3 vec = transform.rotation * (targetPos - lastDBasePos) * moveScale;
//                    if (IgnoreActionMoves)
//                    {
//                        vec.x = 0;
//                        vec.z = 0;
//                        vec.y = 0;
//                    }
//                    else
//                    {

//                    }
//                    moveDelta += vec;
//                    lastDBasePos = targetPos;
//                }
//            }
//            else
//            {
//                dummy[i].localRotation = Quaternion.Slerp(lastStatus.DummyQuat[i], status.DummyQuat[i], timeRatio);
//                dummy[i].localPosition = Vector3.Lerp(lastStatus.DummyPos[i], status.DummyPos[i], timeRatio);
//            }
//        }
//    }

//    public void CharacterUpdate()
//    {
//        if (Pause)
//            return;
//        if (po != null)
//        {
//            moveDelta = Vector3.zero;
//            if (CheckStraight)
//            {
//                PoseStraight -= Time.deltaTime;
//                //检查僵直是否过期.
//                if (PoseStraight < 0.0f && loop)
//                {
//                    loop = false;
//                    curIndex = po.LoopEnd;
//                    CheckStraight = false;
//                }
//            }
//            if (blendTime == 0.0f)
//            {
//                lastFramePlayedTimes += Time.deltaTime;

//                float speedScale = GetSpeedScale();
//                float fps = FPS / (Speed * speedScale);
//                if (lastFramePlayedTimes < fps)
//                {
//                    PlayFrame(lastFramePlayedTimes / fps);
//                }
//                else
//                {
//                    while (lastFramePlayedTimes >= fps)
//                    {
//                        PlayNextKeyFrame(fps);
//                        lastFramePlayedTimes -= fps;
//                        speedScale = GetSpeedScale();
//                        fps = FPS / (Speed * speedScale);
//                    }
//                }
//            }
//            else
//            {
//                playedTime += Time.deltaTime;
//                //TryPlayEffect();
//                ChangeWeaponTrail();

//                BoneStatus status = null;
//                if (po.SourceIdx == 0)
//                    status = AmbLoader.CharCommon[blendStart];
//                else if (po.SourceIdx == 1)
//                    status = AmbLoader.FrameBoneAni[CharacterIdx][blendStart];

//                if (playedTime < blendTime && blendTime != 0.0f && lastFrameStatus != null)
//                {
//                    for (int i = 0; i < bo.Count; i++)
//                    {
//                        bo[i].localRotation = Quaternion.Slerp(lastFrameStatus.BoneQuat[i], status.BoneQuat[i], playedTime / blendTime);
//                        if (i == 0)
//                            bo[i].localPosition = Vector3.Lerp(lastFrameStatus.BonePos, status.BonePos, playedTime / blendTime);
//                    }
//                    for (int i = 0; i < dummy.Count; i++)
//                    {
//                        if (i == 0)
//                        {
//                            //混合动作，不许移动角色
//                            //if (lastPosIdx == po.Idx)
//                            //{
//                            //    Vector3 targetPos = Vector3.Lerp(lastFrameStatus.DummyPos[i], status.DummyPos[i], playedTime / blendTime);
//                            //    Vector3 vec = transform.rotation * (targetPos - lastDBasePos) * moveScale;
//                            //    if (IgnoreActionMove(po.Idx))
//                            //    {
//                            //        vec.x = 0;
//                            //        vec.z = 0;
//                            //        vec.y = 0;
//                            //    }
//                            //    else
//                            //    {
//                            //    }
//                            //    moveDelta += vec;
//                            //    lastDBasePos = targetPos;
//                            //}
//                        }
//                        else
//                        {
//                            dummy[i].localRotation = Quaternion.Slerp(lastFrameStatus.DummyQuat[i], status.DummyQuat[i], playedTime / blendTime);
//                            dummy[i].localPosition = Vector3.Lerp(lastFrameStatus.DummyPos[i], status.DummyPos[i], playedTime / blendTime);
//                        }
//                    }
//                }
//                else
//                {
//                    blendTime = 0.0f;
//                    curIndex = lastFrameIndex = blendStart;
//                    playedTime = 0;
//                    lastFramePlayedTimes = 0;
//                    lastSource = po.SourceIdx;
//                }
//            }
//        }
//    }

//    bool IgnoreActionMove(int idx)
//    {
//        ActionBase act = GameData.actionMng.GetRowByIdx(idx) as ActionBase;
//        if (act == null)
//            return false;
//        return act.IgnoreMove == 1;
//    }

//    //已经在一个状态中，还在持续收到输入，就看这个动作有没有中断，没有就播放NextPose，有则播放中断动作，都没有就回到IDLE，
//    //1倍速度 = 1秒30帧
//    Pose po;
//    int curIndex = 0;
//    public int GetCurrentFrameIndex() { return curIndex; }
//    bool loop = false;
//    bool Pause = false;
//    //调试版本禁用融合和一切过渡算法
//    public void SetPosData(Pose pos)
//    {
//        //保存当前帧的姿势，用于和下个动作融合
//        //当前状态下有姿势，且帧存在状态缓存
//        if (po != null)
//            lastPosIdx = po.Idx;
//        else
//            lastPosIdx = pos.Idx;
//        po = pos;//如果Pos为空，那么姿势就会在当前帧锁定.之前的POS无法更改状态
//        loop = (pos.LoopStart != pos.LoopEnd);//2帧相同不为0，则是固定帧事件，类似切换武器，在固定一帧触发，Action是播放特效的时机
//        curIndex = pos.Start;
//        lastFrameToKey = 0;
//    }
//}