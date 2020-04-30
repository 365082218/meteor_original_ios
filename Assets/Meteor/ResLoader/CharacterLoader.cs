using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using System.Linq;
using System;

public class AABBVector
{
    public Vector3 min;
    public Vector3 max;
}

public enum PoseEvt
{
    WeaponIsReturned = 1,
}

//读取skc文件并且绘制骨骼
//负责处理动画帧的播放
public class CharacterLoader
{
    SkinnedMeshRenderer render;//绘制顶点UV,贴图，骨骼权重
    //Material[] mat;
    public List<Transform> bo;
    List<Transform> dummy;
    public GameObject Skin;
    public Transform rootBone;
    //根骨骼初始位置和旋转，用于RootMotion到上一级
    public Vector3 RootPos;
    public Quaternion RootQuat;
    public Transform Target;
    public void LoadCharactor(int id, Transform Tri)
    {
        Target = Tri;
        owner = Target.GetComponent<MeteorUnit>();
        posMng = owner.posMng;
        Skin = new GameObject();
        Skin.transform.SetParent(Target);
        Skin.transform.localRotation = Quaternion.identity;
        Skin.transform.localScale = Vector3.one;
        Skin.transform.localPosition = Vector3.zero;
        SkcFile skc = Main.Ins.SkcLoader.Load(id);
        BncFile bnc = Main.Ins.BncLoader.Load(id);
        Skin.name = skc.Skin;
        render = Skin.AddComponent<SkinnedMeshRenderer>();
        render.localBounds = skc.mesh.bounds;
        render.materials = skc.Material(id, owner.Camp);
        render.sharedMesh = skc.mesh;
        render.sharedMesh.RecalculateBounds();
        bo = new List<Transform>();
        dummy = new List<Transform>();
        List<Matrix4x4> bindPos = new List<Matrix4x4>();

        if (owner.Attr.IsPlayer)
            Skin.layer = LayerMask.NameToLayer("LocalPlayer");
        else
            Skin.layer = LayerMask.NameToLayer("Monster");

        bnc.GenerateBone(Target, ref bo, ref dummy, ref bindPos, ref rootBone);
        WsGlobal.SetObjectLayer(Target.gameObject, Skin.layer);
        render.bones = bo.ToArray();
        render.sharedMesh.bindposes = bindPos.ToArray();
        render.rootBone = rootBone;
        RootPos = rootBone.localPosition;
        RootQuat = rootBone.localRotation;
        AmbLoader.Ins.LoadCharacterAmb(id);
        //GenerateBounds(skc.mesh, bo);
        LoadBoxDef(id);
    }

    public PoseStatus posMng;
    public MeteorUnit owner;
    public MeteorUnit mOwner { get { return owner; } }

    void GenerateBounds(Mesh me, List<Transform> bo)
    {
        GameObject meshTrace = new GameObject("Trace");
        string[] bonesBatch = new string[] { "bau_L_Hand", "bau_R_Hand" };//子节点作为AABB盒合并的
        string[] bonesIgnore = new string[] { "bad_Pelvis", "bau_Neck", "bau_L_Clavicle", "bau_R_Clavicle" };//忽略的
        List<Transform> tr = new List<Transform>();
        List<Transform> ig = new List<Transform>();
        for (int i = 0; i < bonesBatch.Length; i++)
            tr.Add(NodeHelper.Control(bonesBatch[i], bo[0].gameObject).transform);
        for (int i = 0; i < bonesIgnore.Length; i++)
            ig.Add(NodeHelper.Control(bonesIgnore[i], bo[0].gameObject).transform);

        meshTrace.transform.position = Vector3.zero;
        meshTrace.transform.rotation = Quaternion.identity;
        //MeshFilter meTrace = meshTrace.AddComponent<MeshFilter>();
        //Mesh me2 = new Mesh();
        //List<Vector3> vert = new List<Vector3>();
        //List<int> triangle = new List<int>();
        Dictionary<Transform, AABBVector> bounds = new Dictionary<Transform, AABBVector>();
        for (int i = 0; i < me.vertexCount; i++)
        {
            float weightMax = 0.0f;
            int boneIdx = -1;
            if (weightMax < me.boneWeights[i].weight0)
            {
                weightMax = me.boneWeights[i].weight0;
                boneIdx = me.boneWeights[i].boneIndex0;
            }
            if (weightMax < me.boneWeights[i].weight1)
            {
                weightMax = me.boneWeights[i].weight1;
                boneIdx = me.boneWeights[i].boneIndex1;
            }
            if (weightMax < me.boneWeights[i].weight2)
            {
                weightMax = me.boneWeights[i].weight2;
                boneIdx = me.boneWeights[i].boneIndex2;
            }
            if (weightMax < me.boneWeights[i].weight3)
            {
                weightMax = me.boneWeights[i].weight3;
                boneIdx = me.boneWeights[i].boneIndex3;
            }
            if (bounds.ContainsKey(bo[boneIdx]))
            {
                AABBVector b = bounds[bo[boneIdx]];
                Vector3 p = bo[boneIdx].worldToLocalMatrix * me.vertices[i];
                if (b.min.x > p.x)
                    b.min.x = p.x;
                if (b.min.y > p.y)
                    b.min.y = p.y;
                if (b.min.z > p.z)
                    b.min.z = p.z;

                if (b.max.x < p.x)
                    b.max.x = p.x;
                if (b.max.y < p.y)
                    b.max.y = p.y;
                if (b.max.z < p.z)
                    b.max.z = p.z;
                //vert.Add(me.vertices[i]);
            }
            else
            {
                //如果忽略了该骨骼，就不要执行下面的
                if (ig.Contains(bo[boneIdx]))
                    continue;
                //如果被父级合并，则用父级骨骼
                bool batched = false;
                for (int j = 0; j < tr.Count; j++)
                {
                    GameObject obj = NodeHelper.Control(bo[boneIdx].name, tr[j].gameObject);
                    if (obj != null)
                    {
                        if (bounds.ContainsKey(tr[j]))
                        {
                            AABBVector b = bounds[tr[j]];
                            Vector3 p = tr[j].worldToLocalMatrix * me.vertices[i];
                            if (b.min.x > p.x)
                                b.min.x = p.x;
                            if (b.min.y > p.y)
                                b.min.y = p.y;
                            if (b.min.z > p.z)
                                b.min.z = p.z;

                            if (b.max.x < p.x)
                                b.max.x = p.x;
                            if (b.max.y < p.y)
                                b.max.y = p.y;
                            if (b.max.z < p.z)
                                b.max.z = p.z;
                        }
                        else
                        {
                            AABBVector b = new AABBVector();
                            Vector3 p = tr[j].worldToLocalMatrix * me.vertices[i];
                            b.min.x = p.x;
                            b.min.y = p.y;
                            b.min.z = p.z;
                            b.max.x = p.x;
                            b.max.y = p.y;
                            b.max.z = p.z;
                            bounds.Add(tr[j], b);
                            //vert.Add(me.vertices[i]);
                        }
                        batched = true;
                        break;
                    }
                }

                if (batched)
                    continue;
                else
                {
                    AABBVector b = new AABBVector();
                    Vector3 p = bo[boneIdx].worldToLocalMatrix * me.vertices[i];
                    b.min.x = p.x;
                    b.min.y = p.y;
                    b.min.z = p.z;
                    b.max.x = p.x;
                    b.max.y = p.y;
                    b.max.z = p.z;
                    bounds.Add(bo[boneIdx], b);
                    //vert.Add(me.vertices[i]);
                }
            }
        }
        //for (int i = 0; i < me.triangles.Length; i++)
        //    triangle.Add(me.triangles[i]);

        //me2.vertices = vert.ToArray();
        //me2.triangles = triangle.ToArray();
        //meTrace.mesh = me2;
        //meTrace.gameObject.AddComponent<MeshRenderer>();
        foreach (var each in bounds)
        {
            BoxCollider b = each.Key.gameObject.AddComponent<BoxCollider>();
            b.center = Vector3.zero;
            b.size = each.Value.max - each.Value.min;
            b.enabled = true;
            b.isTrigger = true;
            //int idx = WsGlobal.AddDebugLine(each.Value.max, each.Value.min, Color.red);
            //BoxCollider box = WsGlobal.DebugLine[idx].AddComponent<BoxCollider>();
            //box.center = Vector3.zero;
            //box.size = each.Value.max - each.Value.min;
        }
    }

    //使用同一个攻击定义盒
    void LoadBoxDef(int idx)
    {
        idx = 0;
        TextAsset asset = Resources.Load<TextAsset>("boxdef0");
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
                    BoxCollider bodef = bo[i].gameObject.AddComponent<BoxCollider>();
                    bodef.center = boxdef[j].center;
                    bodef.size = boxdef[j].size;
                    bodef.isTrigger = true;
                    bodef.enabled = true;
                    owner.AddHitBox(bodef);//受击盒.固定的
                    bo[i].gameObject.layer = LayerMask.NameToLayer("Bone");
                }
            }
        }
    }

    bool LockCurrentFrame = false;
    public void LockTime(float t)
    {
        PoseStraight = t;
        //Debug.LogError("straight:" + t);
    }

    public bool IsInStraight()
    {
        return PoseStraight > 0;
    }

    public bool InTransition()
    {
        return false;
        //return blendTime != 0 && owner.posMng.mActiveAction.Idx != 0;
    }

    public float FPS = 1.0f / 30.0f;
    int lastFrameIndex = 1;
    int lastSource = 1;
    int lastPosIdx = 0;
    Vector3 lastDBasePos = Vector3.zero;//上一帧的d_base骨骼坐标.
    Vector3 nextDBasePos = Vector3.zero;//下一帧的d_base骨骼坐标.

    Vector3 attackTarget;//受击后
    //设置动作位移的根骨骼移动比例,比如完整动作，会让角色Y轴移动10，那么比例为2时，这个动作就会让角色移动20，而帧数不变
    public void SetActionRotation(Vector3 vec)
    {
        //在xz轴上动作的方向
        vec.y = 0;
        attackTarget = vec.normalized;
    }

    float moveScale = 1.0f;
    public void SetActionScale(float scale)
    {
        moveScale = scale;
    }

    public void ChangeFrame(int source, int frame)
    {
        Pause = true;
        BoneStatus status = null;
        if (source == 0)
            status = AmbLoader.CharCommon[frame];
        else if (source == 1)
            status = AmbLoader.PlayerAnimation[owner.UnitId][frame];
        else
            status = AmbLoader.GetBoneStatus(po.SourceIdx, owner.UnitId, frame);
        for (int i = 0; i < bo.Count; i++)
        {
            bo[i].localRotation = status.BoneQuat[i];
            if (i == 0)
                bo[i].localPosition = status.BonePos;
        }
        for (int i = 0; i < dummy.Count; i++)
        {
            //if (i == 0)
            //{
                //Debug.LogError("action move");
                //if (lastPosIdx == po.Idx && !inlooping)
                //{
                //    Vector3 targetPos = status.DummyPos[i];
                //    Vector3 vec = Target.rotation * (targetPos - lastDBasePos) * moveScale;
                //    //如果忽略位移，或者在动作的循环帧中，即第一次从循环头开始播放后，不再计算位移.
                //    moveDelta += vec;
                //    //if (po.Idx == 151)
                //    //    Debug.LogError(string.Format("pose:{0} frame:{1} move: x ={2}, y ={3} z = {4}", po.Idx, curIndex, moveDelta.x, moveDelta.y, moveDelta.z));
                //    lastDBasePos = targetPos;
                //}
           // }
           // else
            {
                dummy[i].localRotation = status.DummyQuat[i];
                dummy[i].localPosition = status.DummyPos[i];
            }
        }

    }


    void PlayNextKeyFrame()
    {
        TryPlayEffect();
        ChangeAttack();
        ChangeWeaponTrail();
        ActionEvent.HandlerActionEvent(owner, po.Idx, curIndex);
        if (PoseEvent.ContainsKey(po.Idx))
        {
            //当218发射飞轮，很快返回，还未到219动作时，下次播放219，就得立即取消循环，221 223
            PoseEvent.Remove(po.Idx);
            loop = false;
            curIndex = po.LoopEnd;
        }

        //有连招.
        if (TestInputLink())
            return;

        if (loop)
        {
            if (LockCurrentFrame)
            {
                if (PoseStraight <= 0.0f)
                {
                    loop = false;
                    curIndex = po.LoopEnd + 1;
                    LockCurrentFrame = false;
                    return;
                }
            }
            if (curIndex > po.LoopEnd)
            {
                if (curIndex > po.LoopStart)
                {
                    LoopCount++;
                    PlayPosEvent();
                    if (loop)
                        curIndex = po.LoopStart;
                    return;
                }
                curIndex = po.LoopStart;
            }
        }
        else
        {
            if (curIndex > po.End)
            {
                posMng.OnActionFinished();
                return;
            }

            if (TheFirstFrame <= curIndex && TheFirstFrame != -1)
            {
                ActionEvent.HandlerFirstActionFrame(mOwner, po.Idx);
                TheFirstFrame = -1;
            }
            if (TheLastFrame <= curIndex && TheLastFrame != -1)
            {
                ActionEvent.HandlerFinalActionFrame(mOwner, po.Idx);
                TheLastFrame = -1;
            }
        }
        //Debug.Log("PlayKeyFrame:" + Time.frameCount);
        BoneStatus status = null;
        if (po.SourceIdx == 0)
            status = AmbLoader.CharCommon[curIndex];
        else if (po.SourceIdx == 1)
            status = AmbLoader.PlayerAnimation[owner.UnitId][curIndex];
        else
            status = AmbLoader.GetBoneStatus(po.SourceIdx, owner.UnitId, curIndex);

        //Debug.LogError("play keyframe " + " idx:" + curIndex);
        if (bo.Count != 0)
        {
            bo[0].localRotation = status.BoneQuat[0];
            bo[0].localPosition = status.BonePos;
        }

        for (int i = 1; i < bo.Count; i++)
            bo[i].localRotation = status.BoneQuat[i];

        bool IgnoreActionMoves = PoseStatus.IgnoreActionMove(po.Idx);
        //if (owner.IsDebugUnit())
        //    IgnoreActionMoves = false;
        bool IgnoreActionXZMove = PoseStatus.IgnoreXZMove(po.Idx);

        if (lastPosIdx == po.Idx)
        {
            Vector3 targetPos = status.DummyPos[0];
            Vector3 vec = Target.rotation * (targetPos - lastDBasePos) * moveScale;
            //如果忽略位移，或者在动作的循环帧中，即第一次从循环头开始播放后，不再计算位移.
            if (IgnoreActionMoves)
            {
                vec.x = 0;
                vec.z = 0;
                vec.y = 0;
            }
            else if (IgnoreActionXZMove)
            {
                vec.x = vec.z = 0;
            }
            moveDelta += vec;
            //if (po.Idx == 151)
            //    Debug.LogError(string.Format("pose:{0} frame:{1} move: x ={2}, y ={3} z = {4}", po.Idx, curIndex, moveDelta.x, moveDelta.y, moveDelta.z));
            lastDBasePos = targetPos;
        }

        for (int i = 1; i < dummy.Count; i++)
        {
            dummy[i].localRotation = status.DummyQuat[i];
            dummy[i].localPosition = status.DummyPos[i];
        }

        lastFrameIndex = curIndex;
        curIndex++;
        lastSource = po.SourceIdx;
        lastPosIdx = po.Idx;
    }

    float GetSpeedScale()
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
        return speedScale;
    }

    public Vector3 moveDelta;//上一帧的位移
    float lastFramePlayedTimes;

    void TryPlayEffect()
    {
        if (!effectPlayed)
        {
            try
            {
                //在其他调试场景里屏蔽掉特效
                if (Main.Ins.GameBattleEx != null)
                    PlayEffect();
            }
            catch (System.Exception exp)
            {
                Debug.LogError("Effect: [" + po.EffectID + " ] Contains Error" + exp.StackTrace);
                effectPlayed = true;
            }
            effectPlayed = true;
        }
    }

    void ChangeAttack()
    {
        bool open = false;
        for (int i = 0; i < po.Attack.Count; i++)
        {
            if (curIndex >= po.Attack[i].Start && curIndex <= po.Attack[i].End)
            {
                //当前处于不允许攻击，才能切换到允许攻击
                //if (!mOwner.allowAttack)
                mOwner.ChangeAttack(po.Attack[i]);
                open = true;
                break;
            }
        }

        if (!open)
        {
            mOwner.ChangeAttack(null);
        }
    }

    void ChangeWeaponTrail()
    {
        if (U3D.IsSpecialWeapon(mOwner.weaponLoader.WeaponSubType()))
            return;
        //开启武器拖尾
        if (po.Drag != null)
        {
            if (curIndex >= po.Drag.Start && curIndex <= po.Drag.End)
                mOwner.ChangeWeaponTrail(po.Drag);
            else
                mOwner.ChangeWeaponTrail(null);
        }
        else
            mOwner.ChangeWeaponTrail(null);
    }

    bool TestInputLink()
    {
        //有连招等待播放
        if (posMng.LinkInput.ContainsKey(po.Idx))
        {
            //当前正处于融合帧中，可以立即切换动画
            for (int i = 0; i < po.ActionList.Count; i++)
            {
                if (po.ActionList[i].Type == "Blend")
                {
                    if (curIndex >= po.ActionList[i].Start && curIndex <= po.ActionList[i].End)
                    {
                        //Debug.LogError("TestInputLink");
                        int targetIdx = po.Idx;
                        if (po.Next != null)
                            posMng.ChangeAction(posMng.LinkInput[targetIdx], po.Next.Time);
                        else
                            posMng.ChangeAction(posMng.LinkInput[targetIdx]);
                        posMng.LinkInput.Clear();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public System.Action<int, int, int, float> OnAnimationFrame;
    public void PlayFrame(float timeRatio)
    {
        TryPlayEffect();
        ChangeAttack();
        ChangeWeaponTrail();
        ActionEvent.HandlerActionEvent(owner, po.Idx, curIndex);
        if (PoseEvent.ContainsKey(po.Idx))
        {
            //当218发射飞轮，很快返回，还未到219动作时，下次播放219，就得立即取消循环，221 223
            PoseEvent.Remove(po.Idx);
            loop = false;
            curIndex = po.LoopEnd;
        }
        if (TestInputLink())
            return;
        if (loop)
        {
            if (LockCurrentFrame)
            {
                if (PoseStraight <= 0.0f)
                {
                    loop = false;
                    curIndex = po.LoopEnd + 1;
                    LockCurrentFrame = false;
                    return;
                }
            }

            if (curIndex >= po.LoopEnd)
            {
                LoopCount ++;
                PlayPosEvent();
                if (loop)
                    curIndex = po.LoopStart;
                return;
            }
        }
        else
        {
            if (curIndex > po.End)
            {
                posMng.OnActionFinished();
                return;
            }
            if (TheFirstFrame <= curIndex && TheFirstFrame != -1)
            {
                ActionEvent.HandlerFirstActionFrame(mOwner, po.Idx);
                TheFirstFrame = -1;
            }
            if (TheLastFrame <= curIndex && TheLastFrame != -1)
            {
                ActionEvent.HandlerFinalActionFrame(mOwner, po.Idx);
                TheLastFrame = -1;
            }
        }
        //Debug.Log("PlayKeyFrame:" + Time.frameCount);
        //curIndex = targetIndex;
        BoneStatus status = null;
        BoneStatus lastStatus = null;
        if (lastSource == 0 && AmbLoader.CharCommon.Count > lastFrameIndex && lastFrameIndex >= 0)
            lastStatus = AmbLoader.CharCommon[lastFrameIndex];
        else if (AmbLoader.PlayerAnimation.ContainsKey(owner.UnitId) && AmbLoader.PlayerAnimation[owner.UnitId].Count > lastFrameIndex && lastFrameIndex >= 0)
            lastStatus = AmbLoader.PlayerAnimation[owner.UnitId][lastFrameIndex];
        if (po.SourceIdx == 0)
            status = AmbLoader.CharCommon[curIndex];
        else if (po.SourceIdx == 1)
            status = AmbLoader.PlayerAnimation[owner.UnitId][curIndex];
        else
            status = AmbLoader.GetBoneStatus(po.SourceIdx, owner.UnitId, curIndex);

        if (status != null && lastStatus != null)
        {
            bo[0].localRotation = Quaternion.Slerp(lastStatus.BoneQuat[0], status.BoneQuat[0], timeRatio);
            bo[0].localPosition = Vector3.Lerp(lastStatus.BonePos, status.BonePos, timeRatio);
            for (int i = 1; i < bo.Count; i++)
            {
                bo[i].localRotation = Quaternion.Slerp(lastStatus.BoneQuat[i], status.BoneQuat[i], timeRatio);
            }
        }

        bool IgnoreActionMoves = PoseStatus.IgnoreActionMove(po.Idx);
        if (owner.IsDebugUnit())
        {
            if (OnAnimationFrame != null)
                OnAnimationFrame(po.SourceIdx, po.Idx, curIndex, timeRatio);
            //IgnoreActionMoves = false;
        }

        bool IgnoreActionXZMove = PoseStatus.IgnoreXZMove(po.Idx);

        if (lastStatus != null && status != null)
        {
            if (lastPosIdx == po.Idx)
            {
                Vector3 targetPos = Vector3.Lerp(lastStatus.DummyPos[0], status.DummyPos[0], timeRatio);
                Vector3 vec = Target.rotation * (targetPos - lastDBasePos) * moveScale;
                if (IgnoreActionMoves)
                {
                    vec.x = 0;
                    vec.z = 0;
                    vec.y = 0;
                }
                else if (IgnoreActionXZMove)
                {
                    vec.x = vec.z = 0;
                }
                moveDelta += vec;
                lastDBasePos = targetPos;
            }

            for (int i = 1; i < dummy.Count; i++)
            { 
                dummy[i].localRotation = Quaternion.Slerp(lastStatus.DummyQuat[i], status.DummyQuat[i], timeRatio);
                dummy[i].localPosition = Vector3.Lerp(lastStatus.DummyPos[i], status.DummyPos[i], timeRatio);
            }
        }
    }

    public void NetUpdate()
    {
        if (Pause)
            return;
        if (po != null)
        {
            moveDelta = Vector3.zero;
            if (blendTime == 0.0f)
            {
                //过渡时间不算硬直，非过渡时间，算在硬直内.
                if (PoseStraight > 0.0f)
                    PoseStraight -= FrameReplay.deltaTime;
                lastFramePlayedTimes += FrameReplay.deltaTime;
                float speedScale = owner.ActionSpeed * GetSpeedScale();
                float fps = Main.Ins.CombatData.FPS / speedScale;
                while (lastFramePlayedTimes >= fps)
                {
                    PlayNextKeyFrame();
                    lastFramePlayedTimes -= fps;
                    speedScale = GetSpeedScale();
                    fps = Main.Ins.CombatData.FPS / speedScale;
                }

                if (lastFramePlayedTimes < fps && lastFramePlayedTimes > 0)
                    PlayFrame(lastFramePlayedTimes / fps);
            }
            else
            {
                playedTime += FrameReplay.deltaTime;
                ChangeWeaponTrail();

                BoneStatus status = null;
                if (po.SourceIdx == 0 && AmbLoader.CharCommon.Count > blendStart && blendStart >= 0)
                    status = AmbLoader.CharCommon[blendStart];
                else if (po.SourceIdx == 1 && AmbLoader.PlayerAnimation.ContainsKey(mOwner.UnitId) && AmbLoader.PlayerAnimation[mOwner.UnitId].Count > blendStart && blendStart >= 0)
                    status = AmbLoader.PlayerAnimation[mOwner.UnitId][blendStart];
                else
                    status = AmbLoader.GetBoneStatus(po.SourceIdx, owner.UnitId, blendStart);
                if (playedTime < blendTime && blendTime != 0.0f && lastFrameStatus != null && status != null)
                {
                    bo[0].localRotation = Quaternion.Slerp(lastFrameStatus.BoneQuat[0], status.BoneQuat[0], playedTime / blendTime);
                    bo[0].localPosition = Vector3.Lerp(lastFrameStatus.BonePos, status.BonePos, playedTime / blendTime);
                    for (int i = 1; i < bo.Count; i++)
                    {
                        bo[i].localRotation = Quaternion.Slerp(lastFrameStatus.BoneQuat[i], status.BoneQuat[i], playedTime / blendTime);
                    }
                    lastDBasePos = status.DummyPos[0];
                    for (int i = 1; i < dummy.Count; i++)
                    {
                        dummy[i].localRotation = Quaternion.Slerp(lastFrameStatus.DummyQuat[i], status.DummyQuat[i], playedTime / blendTime);
                        dummy[i].localPosition = Vector3.Lerp(lastFrameStatus.DummyPos[i], status.DummyPos[i], playedTime / blendTime);
                    }
                }
                else
                {
                    blendTime = 0.0f;
                    curIndex = lastFrameIndex = blendStart;
                    playedTime = 0;
                    lastFramePlayedTimes = 0;
                    lastSource = po.SourceIdx;
                }
            }
        }
    }

    public float PoseStraight = 0.0f;
    //循环动作锁定，（一直在2个帧段之间播放，待特定条件打成就退出该帧段）
    void PlayPosEvent()
    {
        //有硬直时间存在，但是还没开始消耗硬直时间时，先消耗硬直时间
        if (IsInStraight())
        {
            //能否开始计算硬直?如果角色在空中播放循环落地动作,这个硬直是无法开始计算的，只有落地后，才能开始计算硬直
            if (!mOwner.IsOnGround())
                return;
            //已经开始计算硬直时间的流逝
            if (LockCurrentFrame)
                return;

            LockCurrentFrame = true;//开始计算硬直流逝，一直到硬直时间结束.
        }
        else
        {
            if (po.Idx == CommonAction.ChangeWeapon)
            {
                owner.ChangeNextWeapon();
                loop = false;
            }
            else if (po.Idx == CommonAction.AirChangeWeapon)
            {
                owner.ChangeNextWeapon();
                loop = false;
            }
            else if ((po.Idx >= CommonAction.Idle && po.Idx <= 21) || (po.Idx >= CommonAction.WalkForward && po.Idx <= CommonAction.RunOnDrug))
            {
                if (owner.GameFinished)
                    loop = false;
            }
            else if (po.Idx == 219 || po.Idx == 221 || po.Idx == 223)//飞轮出击后等待接回飞轮
            {
                //等着收回武器
            }
            else
            {
                if (mOwner.IsOnGround())
                {
                    loop = false;
                    //Debug.Log("PlayPosEvent:" + po.Idx);
                }
            }
        }
    }

    float GetTimePlayed(int frame)
    {
        float frameCost = 0.0f;
        for (int i = po.Start; i < frame; i++)
        {
            float speedScale = 1.0f;
            for (int j = 0; j < po.ActionList.Count; j++)
            {
                if (i >= po.ActionList[j].Start && i <= po.ActionList[j].End)
                {
                    speedScale = (po.ActionList[j].Speed == 0.0f ? 1.0f : po.ActionList[j].Speed);
                    break;
                }
            }
            frameCost += Main.Ins.CombatData.FPS / speedScale ;
        }
        return frameCost;
    }

    //特效时间不是太准，需要考虑如何让特效和动作同步.
    public SFXEffectPlay sfxEffect { get; set; }//当前动作特效，用于飞镖/飞轮的挂点查询
    void PlayEffect()
    {
        float timePlayed = 0;
        //锤绝-匕首A+上上A，音效有点问题 剑 前前A2
        //if (po.Idx == 325 || po.Idx == 253 || po.Idx == 276 || po.Idx == 560 || po.Idx == 471)
        //if (po.Idx != 325)
        timePlayed = GetTimePlayed(curIndex);
        if (!string.IsNullOrEmpty(po.EffectID) && !string.Equals(po.EffectID, "0"))
        {
            sfxEffect = Main.Ins.SFXLoader.PlayEffect(string.Format("{0}.ef", po.EffectID), this, timePlayed);

            //表明特效是由动作触发的,不在该动作中关闭特效的攻击盒时,特效攻击盒仍存在
            //这种一般是特效出来后，在角色受到攻击前打开了特效的攻击盒，但角色受到攻击打断了动作，会立刻关闭攻击特效的攻击属性，这种应该是不对的.
            //类似雷电斩，特效出来后只要攻击盒被打开过，一旦动作被打断，那么攻击特效会一直到特效完毕.
        }
        effectPlayed = true;
    }

    public float blendTime = 0.3f;
    float playedTime = 0;

    Pose po;
    public int curIndex = 0;//帧编号
    public int curPos = 0;//动作编号
    int blendStart = 0;
    public void SetCurrentFrameIndex(int v) { curIndex = v; }
    public int GetCurrentFrameIndex() { return curIndex; }

    //僵直清空/飞轮回收，等一些情况时，取消循环,立即切换到动作结束
    public void SetLoop(bool looped)
    {
        loop = looped;
        curIndex = po.LoopEnd;
    }

    Dictionary<int, int> PoseEvent = new Dictionary<int, int>();
    public void LinkEvent(int pose, PoseEvt evt)
    {
        if (PoseEvent.ContainsKey(pose))
            PoseEvent[pose] = (int)evt;
        else
            PoseEvent.Add(pose, (int)evt);
    }

    bool loop = false;
    BoneStatus lastFrameStatus;
    //这2个用来实现一些技能
    int TheFirstFrame = -1;//第一个Action的第一帧，0则无
    int TheLastFrame = -1;//最后一个Action的最后一帧，0则无
    public int LoopCount = 0;
    bool effectPlayed = false;
    public bool Pause = false;
    bool single = false;
    public void SetPosData(Pose pos, float BlendTime = 0.0f)
    {
        //一些招式，需要把尾部事件执行完才能切换武器.
        LoopCount = 0;
        LockCurrentFrame = false;
        if (TheLastFrame != -1 && po != null)
        {
            ActionEvent.HandlerFinalActionFrame(mOwner, po.Idx);
            TheLastFrame = -1;
        }
        else
        {
            if (po != null && po.Link != 0)
                //一些动作，默认连接其他动作，类似,486第一帧会收刀，收刀会切换武器为2
                ActionEvent.HandlerPoseAction(mOwner, po.Link);
        }
        //一些招式，动作结束会给使用者加上BUFF，另外一些招式，会让受击方得到BUFF
        int lastPosIdx = 0;
        if (po != null && po.Idx != 0)
            lastPosIdx = po.Idx;

        moveScale = 1.0f;
        attackTarget = Vector3.zero;
        //重置速度
        bool isAttackPos = false;
        if (po == null)
            isAttackPos = false;
        else
            //isAttackPos = po.Idx >= CommonAction.AttackActStart && po.Idx != CommonAction.GunIdle;
            isAttackPos = posMng.IsAttackPose(po.Idx);
        //从IDLE往IDLE是不许融合的，否则武器位置插值非常难看
        if (pos != null && po != null && pos.Idx == po.Idx && pos.Idx == 0)
            BlendTime = 0.0f;
        //保存当前帧的姿势，用于和下个动作融合
        //当前状态下有姿势，且帧存在状态缓存
        if (po != null)
            lastPosIdx = po.Idx;
        else
            lastPosIdx = pos.Idx;
        if (BlendTime != 0.0f)
        {
            GetCurrentSnapShot();
            //如果有混合，重置混合位置.
            nextDBasePos = lastFrameStatus.DummyPos[0];
        }
        //动画帧率与运行帧率可能要转换一下.
        blendTime = BlendTime;
        lastFramePlayedTimes = 0;
        po = pos;
        loop = (pos.LoopStart != 0 && pos.LoopEnd != 0);//2帧相同不为0

        //查看第一个blend的最后一帧，如果有，切换目标帧设置为这个,若第一个是act则目标帧为起始帧
        //PosAction blend = null;
        PosAction act = null;
        if (pos.ActionList.Count != 0)
        {
            if (isAttackPos)
            {
                for (int i = 0; i < pos.ActionList.Count; i++)
                {
                    //过滤掉565，刀雷电斩的头一个 第一个混合段与整个动画一致.
                    if (pos.ActionList[i].Start == pos.Start && pos.ActionList[i].End == pos.End)
                        continue;
                    act = pos.ActionList[i];
                    break;
                }
            }
            else
                act = pos.ActionList[0];
        }

        TheLastFrame = pos.End - 1;
        TheFirstFrame = pos.Start;

        //算第一个过渡帧条件很多，有切换目的帧是否设定，第一个过渡帧是否存在，上一个动作是否攻击动作，锤绝325BUG，其他招式接325，还要在地面等，应该不需要在地面等
        //curIndex = targetFrame != 0 ? targetFrame : (act != null ? (act.Type == "Action" ? act.Start: (isAttackPos ? act.End : pos.Start)): pos.Start);
        curIndex = act != null ? (act.Type == "Action" ? (isAttackPos ? act.Start : pos.Start) : (isAttackPos ? act.End : act.Start)) : pos.Start;
        //部分动作混合帧比开始帧还靠前
        if (curIndex < pos.Start)
            curIndex = pos.Start;
        blendStart = curIndex;
        //if (blendTime != 0.0f && (pos.Idx == CommonAction.JumpFall || pos.Idx == CommonAction.Jump || pos.Idx == CommonAction.JumpFallOnGround))
        //    Debug.Log("过渡帧为" + blendStart + " 转换到动作:" + pos.Idx);
        effectPlayed = false;
        sfxEffect = null;
        playedTime = 0;
        
        //下一个动作的第一帧所有虚拟物体
        if (po.SourceIdx == 0)
            lastDBasePos = AmbLoader.CharCommon[curIndex].DummyPos[0];
        else if (po.SourceIdx == 1)
            lastDBasePos = AmbLoader.PlayerAnimation[owner.UnitId][curIndex].DummyPos[0];

        if (lastPosIdx != 0)
        {
            Option poseInfo = Main.Ins.MenuResLoader.GetPoseInfo(lastPosIdx);
            if (poseInfo.first.Length != 0 && poseInfo.first[0].flag[0] == 18 && lastPosIdx != 468)//18为，使用招式后获取物品ID 468-469都会调用微尘，hack掉468pose的
                owner.GetItem(poseInfo.first[0].flag[1]);//忍刀小绝，同归于尽，会获得微尘物品，会即刻死亡
        }

        curPos = pos.Idx;
    }

    public void GetCurrentSnapShot()
    {
        if (lastFrameStatus == null)
        {
            lastFrameStatus = new BoneStatus();
            lastFrameStatus.Init();
            for (int i = 0; i < bo.Count; i++)
            {
                lastFrameStatus.BoneQuat.Add(new MyQuaternion());
                if (i == 0)
                    lastFrameStatus.BonePos = new MyVector();
            }
            for (int i = 0; i < dummy.Count; i++)
            {
                lastFrameStatus.DummyQuat.Add(new MyQuaternion());
                lastFrameStatus.DummyPos.Add(new MyVector());
            }
        }
        for (int i = 0; i < bo.Count; i++)
        {
            lastFrameStatus.BoneQuat[i] = bo[i].localRotation;
            if (i == 0)
                lastFrameStatus.BonePos = bo[i].localPosition;
        }
        for (int i = 0; i < dummy.Count; i++)
        {
            lastFrameStatus.DummyQuat[i] = dummy[i].localRotation;
            lastFrameStatus.DummyPos[i] = dummy[i].localPosition;
        }
    }
}

//包括30个骨骼的位置 旋转
[ProtoBuf.ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class BoneStatus
{
    public int startflag;
    public MyVector BonePos;//相对位置,每一帧只有首骨骼有
    public List<MyVector> DummyPos;//虚拟对象相对位置
    public List<MyQuaternion> BoneQuat;//相对旋转.
    public List<MyQuaternion> DummyQuat;//虚拟对象相对旋转
    public void Init()
    {
        DummyPos = new List<MyVector>();
        BoneQuat = new List<MyQuaternion>();
        DummyQuat = new List<MyQuaternion>();
    }
}

public class AmbLoader
{
    AmbLoader()
    {
    }

    static AmbLoader _Ins;
    public static AmbLoader Ins
    {
        get
        {
            if (_Ins == null)
                _Ins = new AmbLoader();
            return _Ins;
        }
    }

    public static BoneStatus GetBoneStatus(int source, int unitId, int frame)
    {
        if (!PlayerAnimationEx.ContainsKey(source))
            return null;
        if (!PlayerAnimationEx[source].ContainsKey(unitId))
            return null;
        if (!PlayerAnimationEx[source][unitId].ContainsKey(frame))
            return null;
        return PlayerAnimationEx[source][unitId][frame]; 
    }

    //所有角色公用的招式.
    public static Dictionary<int, BoneStatus> CharCommon = new Dictionary<int, BoneStatus>();//source 0
    //角色ID-角色动画帧编号-骨骼状态.
    public static Dictionary<int, Dictionary<int, BoneStatus>> PlayerAnimation = new Dictionary<int, Dictionary<int, BoneStatus>>();//source 1
    //后期自定义动画，表格里对应.
    //source - unit - frame - boneStatus
    public static Dictionary<int, Dictionary<int, Dictionary<int, BoneStatus>>> PlayerAnimationEx = new Dictionary<int, Dictionary<int, Dictionary<int, BoneStatus>>>();
    //加载个人自身的动作
    public void LoadCharacterAmb(int idx)
    {
        if (PlayerAnimation.ContainsKey(idx))
            return;
        //大于20的是新角色，新角色只读skc其他男性角色读0号位数据 女性角色读1号位数据
        if (idx >= 20)
        {
            ModelItem m = DlcMng.GetPluginModel(idx);
            if (m != null && m.Installed)
            {
                for (int i = 0; i < m.resPath.Length; i++)
                {
                    if (m.resPath[i].ToLower().EndsWith(".amb"))
                    {
                        byte[] memory = System.IO.File.ReadAllBytes(m.resPath[i]);
                        PlayerAnimation.Add(idx, Parse(memory));
                        return;
                    }
                }
            }
            PlayerAnimation.Add(idx, PlayerAnimation[0]);
            return;
        }
        //11和9文件重复了.
        if (idx == 11 || idx == 9)
            idx = 9;
        Dictionary<int, BoneStatus> ret = LoadAmbSync("p" + idx + ".amb");
        if (!PlayerAnimation.ContainsKey(idx))
            PlayerAnimation.Add(idx, ret);

        //9号文件和11号一样，复用
        if (idx == 9)
        {
            if (!PlayerAnimation.ContainsKey(11))
                PlayerAnimation.Add(11, ret);
        }
    }

    //加载配表动作-外部新动作.
    public void LoadCharacterAmbEx()
    {
        //AnimationBase [] items = AnimationMng.Instance.GetAllItem();
        //for (int i = 0; i < items.Length; i++)
        //{
        //    if (!string.IsNullOrEmpty(items[i].TableName))
        //    {
        //        if (!PlayerAnimationEx.ContainsKey(items[i].Source))
        //            PlayerAnimationEx[items[i].Source] = new Dictionary<int, Dictionary<int, BoneStatus>>();
        //        if (!PlayerAnimationEx[items[i].Source].ContainsKey(items[i].Unit))
        //        {
        //            TextAsset asset = Resources.Load<TextAsset>(items[i].TableName);
        //            PlayerAnimationEx[items[i].Source][items[i].Unit] = Parse(asset.bytes);
        //        }
        //    }
        //}
    }

    //加载通用动作
    public void LoadCharacterAmb()
    {
        if (CharCommon == null || CharCommon.Count == 0)
            CharCommon = LoadAmbSync("characteramb");
    }

    public Dictionary<int, BoneStatus> Parse(byte[] memory)
    {
        MemoryStream ms = new MemoryStream(memory);
        BinaryReader binRead = new BinaryReader(ms);
        binRead.BaseStream.Seek(5, SeekOrigin.Begin);
        int bone = binRead.ReadInt32();
        int dummy = binRead.ReadInt32();
        int frames = binRead.ReadInt32();
        binRead.ReadInt32();
        Dictionary<int, BoneStatus> innerValue = new Dictionary<int, BoneStatus>();
        for (int i = 0; i < frames; i++)
        {
            BoneStatus status = new BoneStatus();
            status.Init();
            status.startflag = binRead.ReadInt32();
            if (status.startflag != -1)
                Debug.LogError("frame:" + i + " startflag:" + status.startflag);
            int frameindex = binRead.ReadInt32();
            float x = binRead.ReadSingle();
            float y = binRead.ReadSingle();
            float z = binRead.ReadSingle();
            status.BonePos = new Vector3(x, z, y);//首骨骼的相对坐标.
            for (int j = 0; j < bone; j++)
            {
                float w = binRead.ReadSingle();
                float xx = -binRead.ReadSingle();
                float zz = -binRead.ReadSingle();
                float yy = -binRead.ReadSingle();
                Quaternion quat = new Quaternion(xx, yy, zz, w);
                status.BoneQuat.Add(quat);
            }
            for (int k = 0; k < dummy; k++)
            {
                binRead.BaseStream.Seek(5, SeekOrigin.Current);
                float dx = binRead.ReadSingle();
                float dy = binRead.ReadSingle();
                float dz = binRead.ReadSingle();
                float dw = binRead.ReadSingle();
                float dxx = -binRead.ReadSingle();
                float dzz = -binRead.ReadSingle();
                float dyy = -binRead.ReadSingle();
                status.DummyPos.Add(new Vector3(dx, dz, dy));
                status.DummyQuat.Add(new Quaternion(dxx, dyy, dzz, dw));
            }
            innerValue.Add(frameindex, status);
        }

        //豪微秒 10^-7秒
        //Debug.Log(string.Format("{0}", (double)(System.DateTime.Now.Ticks - s1) / 10000000.0));
        return innerValue;
    }

    //人物自身动作，0帧为TPose
    //招式通用动作，从1帧开始，没有0帧
    //一帧内返回.
    private Dictionary<int, BoneStatus> LoadAmbSync(string file)
    {
        long s1 = System.DateTime.Now.Ticks;
        TextAsset asset = Resources.Load<TextAsset>(file);
        if (asset == null)
        {
            Debug.LogError("amb file:" + file + " can not found");
            return null;
        }
        return Parse(asset.bytes);
    }
}
[ProtoContract]
public class BoxColliderDef
{
    [ProtoMember(1)]
    public string name;
    [ProtoMember(2)]
    public MyVector center = new Vector3(0, 0, 0);
    [ProtoMember(3)]
    public MyVector size = new Vector3(0, 0, 0);
}

