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
    None,
    WeaponIsReturned = 1,
    Fall = 2,//播放动作时，做掉落处理-被墙壁弹开
}

//读取skc文件并且创建正确的对象，动画全部搬到PoseStatus里处理.
public class CharacterLoader
{
    SkinnedMeshRenderer render;//绘制顶点UV,贴图，骨骼权重
    //Material[] mat;
    public List<Transform> bo;
    public List<Transform> dummy;
    public GameObject Skin;
    public Transform rootBone;
    //根骨骼初始位置和旋转，用于RootMotion到上一级
    public Vector3 RootPos;
    public Quaternion RootQuat;
    public Transform Target;
    //特效时间不是太准，需要考虑如何让特效和动作同步.
    public SFXEffectPlay sfxEffect { get; set; }//当前动作特效，用于飞镖/飞轮的挂点查询
    public void LoadCharactor(int id, Transform Tri)
    {
        Target = Tri;
        Owner = Target.GetComponent<MeteorUnit>();
        posMng = Owner.ActionMgr;
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
        render.materials = skc.Material(id, Owner.Camp);
        //这个方式无法分别子网格
        //if (Owner.Attr.IsPlayer) {
        //    for (int i = 0; i < render.materials.Length; i++) {
        //        render.materials[i].shader = Shader.Find("MainPlayer");
        //    }
        //}
        render.sharedMesh = skc.mesh;
        render.sharedMesh.RecalculateBounds();
        bo = new List<Transform>();
        dummy = new List<Transform>();
        List<Matrix4x4> bindPos = new List<Matrix4x4>();
        Skin.layer = LayerManager.Player;
        bnc.GenerateBone(Target, ref bo, ref dummy, ref bindPos, ref rootBone);
        //Utility.SetObjectLayer(Target.gameObject, Skin.layer);
        render.bones = bo.ToArray();
        render.sharedMesh.bindposes = bindPos.ToArray();
        render.rootBone = rootBone;
        RootPos = rootBone.localPosition;
        RootQuat = rootBone.localRotation;
        Main.Ins.AmbLoader.LoadCharacterAmb(id);
        //GenerateBounds(skc.mesh, bo);
        LoadBoxDef(id);
    }

    public ActionManager posMng;
    MeteorUnit Owner;
    public MeteorUnit mOwner { get { return Owner; } }

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
        TextAsset asset = Resources.Load<TextAsset>("boxdef2");
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
                    Owner.HurtList.Add(bodef);//受击盒.固定的,一直起效果
                }
            }
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
    public void InitWith(int bone, int dummy) {
        Init();
        for (int i = 0; i < bone; i++) {
            BoneQuat.Add(new MyQuaternion());
        }
        for (int i = 0; i < dummy; i++) {
            DummyPos.Add(new MyVector());
            DummyQuat.Add(new MyQuaternion());
        }
    }
    public void Add(BoneStatus other) {
        BonePos.x += other.BonePos.x;
        BonePos.y += other.BonePos.y;
        BonePos.z += other.BonePos.z;
        for (int i = 0; i < DummyPos.Count; i++) {
            DummyPos[i] = other.DummyPos[i] + DummyPos[i];
        }
        for (int i = 0; i < BoneQuat.Count; i++) {
            BoneQuat[i] = other.BoneQuat[i] + BoneQuat[i];
        }
        for (int i = 0; i < DummyQuat.Count; i++) {
            DummyQuat[i] = other.DummyQuat[i] + DummyQuat[i];
        }
    }

    public BoneStatus Clone() {
        return Scale(1);
    }

    public BoneStatus Scale(float scale) {
        BoneStatus clone = new BoneStatus();
        clone.InitWith(BoneQuat.Count, DummyQuat.Count);
        clone.BonePos.x = BonePos.x * scale;
        clone.BonePos.y = BonePos.y * scale;
        clone.BonePos.z = BonePos.z * scale;
        for (int i = 0; i < DummyPos.Count; i++) {
            clone.DummyPos[i] = DummyPos[i].Scale(scale);
        }
        for (int i = 0; i < BoneQuat.Count; i++) {
            clone.BoneQuat[i] = BoneQuat[i].Scale(scale);
        }
        for (int i = 0; i < DummyQuat.Count; i++) {
            clone.DummyQuat[i] = DummyQuat[i].Scale(scale);
        }
        return clone;
    }

    public void Clone(ref BoneStatus clone) {
        clone.BonePos = BonePos;
        for (int i = 0; i < DummyPos.Count; i++) {
            clone.DummyPos[i] = DummyPos[i];
        }
        for (int i = 0; i < BoneQuat.Count; i++) {
            clone.BoneQuat[i] = BoneQuat[i];
        }
        for (int i = 0; i < DummyQuat.Count; i++) {
            clone.DummyQuat[i] = DummyQuat[i];
        }
    }
}

public enum BakeInto {
    BakeLocalDummyPosX = 1,
    BakeLocalDummyPosY,
    BakeLocalDummyPosZ,
    BakeLocalDummyRotationX,
    BakeLocalDummyRotationY,
    BakeLocalDummyRotationZ,
    BakeLocalDummyRotationW,
    BakeLocalPosX,
    BakeLocalPosY,
    BakeLocalPosZ,
    BakeLocalRotationX,
    BakeLocalRotationY,
    BakeLocalRotationZ,
    BakeLocalRotationW,
}

public class AmbLoader
{
    public AmbLoader()
    {
    }

    public BoneStatus GetBoneStatus(int source, int unitId, int frame) {
        if (source == 0) {
            if (CharCommon.ContainsKey(frame))
                return CharCommon[frame];
        }
        else if (source == 1) {
            if (PlayerAnimation.ContainsKey(unitId) && PlayerAnimation[unitId].ContainsKey(frame)) {
                return PlayerAnimation[unitId][frame];
            }
        }
        return null;
    }

    //把骨骼的Pose帧信息烘培到曲线上.
    public void BakeIntoCurve(int unitId, Pose source, AnimationCurve curve, int boneIndex, BakeInto bakeopt, bool keyFrameReduction = true, float epsilon = 0.00001f) {
        BoneStatus prevFrame = null;
        BoneStatus nextFrame = null;
        float time = 0.0f;
        float value = 0.0f;
        for (int i = source.Start; i <= source.End; i++) {
            nextFrame = GetBoneStatus(source.SourceIdx, unitId, i);
            if (prevFrame != null) {
                //根据烘培选项，决定如果误差小于某个值，就丢弃掉这个关键帧
                switch (bakeopt) {
                    case BakeInto.BakeLocalDummyPosX:
                        if (Utility.CompareApproximately(prevFrame.DummyPos[boneIndex].x, nextFrame.DummyPos[boneIndex].x, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.DummyPos[boneIndex].x;
                        break;
                    case BakeInto.BakeLocalDummyPosY:
                        if (Utility.CompareApproximately(prevFrame.DummyPos[boneIndex].y, nextFrame.DummyPos[boneIndex].y, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.DummyPos[boneIndex].y;
                        break;
                    case BakeInto.BakeLocalDummyPosZ:
                        if (Utility.CompareApproximately(prevFrame.DummyPos[boneIndex].z, nextFrame.DummyPos[boneIndex].z, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.DummyPos[boneIndex].z;
                        break;
                    case BakeInto.BakeLocalDummyRotationX:
                        if (Utility.CompareApproximately(prevFrame.DummyQuat[boneIndex].x, nextFrame.DummyQuat[boneIndex].x, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.DummyQuat[boneIndex].x;
                        break;
                    case BakeInto.BakeLocalDummyRotationY:
                        if (Utility.CompareApproximately(prevFrame.DummyQuat[boneIndex].y, nextFrame.DummyQuat[boneIndex].y, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.DummyQuat[boneIndex].y;
                        break;
                    case BakeInto.BakeLocalDummyRotationZ:
                        if (Utility.CompareApproximately(prevFrame.DummyQuat[boneIndex].z, nextFrame.DummyQuat[boneIndex].z, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.DummyQuat[boneIndex].z;
                        break;
                    case BakeInto.BakeLocalDummyRotationW:
                        if (Utility.CompareApproximately(prevFrame.DummyQuat[boneIndex].w, nextFrame.DummyQuat[boneIndex].w, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.DummyQuat[boneIndex].w;
                        break;
                    case BakeInto.BakeLocalPosX:
                        if (Utility.CompareApproximately(prevFrame.BonePos.x, nextFrame.BonePos.x, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.BonePos.x;
                        break;
                    case BakeInto.BakeLocalPosY:
                        if (Utility.CompareApproximately(prevFrame.BonePos.y, nextFrame.BonePos.y, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.BonePos.y;
                        break;
                    case BakeInto.BakeLocalPosZ:
                        if (Utility.CompareApproximately(prevFrame.BonePos.z, nextFrame.BonePos.z, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.BonePos.z;
                        break;
                    case BakeInto.BakeLocalRotationX:
                        if (Utility.CompareApproximately(prevFrame.BoneQuat[boneIndex].x, nextFrame.BoneQuat[boneIndex].x, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.BoneQuat[boneIndex].x;
                        break;
                    case BakeInto.BakeLocalRotationY:
                        if (Utility.CompareApproximately(prevFrame.BoneQuat[boneIndex].y, nextFrame.BoneQuat[boneIndex].y, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.BoneQuat[boneIndex].y;
                        break;
                    case BakeInto.BakeLocalRotationZ:
                        if (Utility.CompareApproximately(prevFrame.BoneQuat[boneIndex].z, nextFrame.BoneQuat[boneIndex].z, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.BoneQuat[boneIndex].z;
                        break;
                    case BakeInto.BakeLocalRotationW:
                        if (Utility.CompareApproximately(prevFrame.BoneQuat[boneIndex].w, nextFrame.BoneQuat[boneIndex].w, epsilon))
                            nextFrame = null;
                        else
                            value = nextFrame.BoneQuat[boneIndex].w;
                        break;
                }
            } else {
                //根据烘培选项，决定如果误差小于某个值，就丢弃掉这个关键帧
                switch (bakeopt) {
                    case BakeInto.BakeLocalDummyPosX:
                        value = nextFrame.DummyPos[boneIndex].x;
                        break;
                    case BakeInto.BakeLocalDummyPosY:
                        value = nextFrame.DummyPos[boneIndex].y;
                        break;
                    case BakeInto.BakeLocalDummyPosZ:
                        value = nextFrame.DummyPos[boneIndex].z;
                        break;
                    case BakeInto.BakeLocalDummyRotationX:
                        value = nextFrame.DummyQuat[boneIndex].x;
                        break;
                    case BakeInto.BakeLocalDummyRotationY:
                        value = nextFrame.DummyQuat[boneIndex].y;
                        break;
                    case BakeInto.BakeLocalDummyRotationZ:
                        value = nextFrame.DummyQuat[boneIndex].z;
                        break;
                    case BakeInto.BakeLocalDummyRotationW:
                        value = nextFrame.DummyQuat[boneIndex].w;
                        break;
                    case BakeInto.BakeLocalPosX:
                        value = nextFrame.BonePos.x;
                        break;
                    case BakeInto.BakeLocalPosY:
                        value = nextFrame.BonePos.y;
                        break;
                    case BakeInto.BakeLocalPosZ:
                        value = nextFrame.BonePos.z;
                        break;
                    case BakeInto.BakeLocalRotationX:
                        value = nextFrame.BoneQuat[boneIndex].x;
                        break;
                    case BakeInto.BakeLocalRotationY:
                        value = nextFrame.BoneQuat[boneIndex].y;
                        break;
                    case BakeInto.BakeLocalRotationZ:
                        value = nextFrame.BoneQuat[boneIndex].z;
                        break;
                    case BakeInto.BakeLocalRotationW:
                        value = nextFrame.BoneQuat[boneIndex].w;
                        break;
                }
            }

            //存在关键帧，记录关键帧的信息
            if (nextFrame != null) {
                curve.AddKey(time, value);
                prevFrame = keyFrameReduction ? nextFrame:null;
            }
            time += (1.0f / Pose.FPS);
        }

        if (curve.keys.Count() == 1) {
            curve.AddKey(time, value);
        }
    }

    public void Clear() {
        CharCommon.Clear();
        PlayerAnimation.Clear();
    }
    //所有角色公用的招式.
    public Dictionary<int, BoneStatus> CharCommon = new Dictionary<int, BoneStatus>();//source 0
    //角色ID-角色动画帧编号-骨骼状态.
    public Dictionary<int, Dictionary<int, BoneStatus>> PlayerAnimation = new Dictionary<int, Dictionary<int, BoneStatus>>();//source 1
    //加载个人自身的动作
    public void LoadCharacterAmb(int idx)
    {
        if (PlayerAnimation.ContainsKey(idx))
            return;
        
        if (Main.Ins.CombatData.Chapter != null) {
            Dictionary<int, string> models = Main.Ins.CombatData.GScript.GetModel();
            if (models != null && models.ContainsKey(idx)) {
                string path = Main.Ins.CombatData.Chapter.GetResPath(FileExt.Amb, models[idx]);
                if (!string.IsNullOrEmpty(path)) {
                    Dictionary<int, BoneStatus> pluginAmb = LoadAmbSync(path);
                    PlayerAnimation.Add(idx, pluginAmb);
                    return;
                }
            }
        }
        //大于20的是新角色，新角色只读skc其他男性角色读0号位数据 女性角色读1号位数据
        if (idx >= 20)
        {
            ModelItem m = DlcMng.GetPluginModel(idx);
            if (m != null && m.Installed)
            {
                for (int i = 0; i < m.resPath.Count; i++)
                {
                    if (m.resPath[i].ToLower().EndsWith(".amb"))
                    {
                        Dictionary<int, BoneStatus> pluginAmb = LoadAmbSync(m.resPath[i]);
                        PlayerAnimation.Add(idx, pluginAmb);
                        return;
                    }
                }
            }
            if (m != null) {
                if (m.useFemalePos)
                    PlayerAnimation.Add(idx, PlayerAnimation[1]);
                else
                    PlayerAnimation.Add(idx, PlayerAnimation[0]);
            }
            
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
        //BANIM=BoneAnimation
        binRead.BaseStream.Seek(5, SeekOrigin.Begin);
        int bone = binRead.ReadInt32();
        int dummy = binRead.ReadInt32();
        int frames = binRead.ReadInt32();
        Pose.FPS = binRead.ReadInt32();
        Pose.FCS = 1.0f / Pose.FPS;
        //Debug.Log("Fps:" + Pose.FPS);
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
        //long s1 = System.DateTime.Now.Ticks;
        byte[] body = null;
        if (File.Exists(file)) {
            body = File.ReadAllBytes(file);
        }
        if (body == null) {
            TextAsset asset = Resources.Load<TextAsset>(file);
            if (asset == null) {
                Debug.LogError("amb file:" + file + " can not found");
                return null;
            }
            body = asset.bytes;
        }
        return Parse(body);
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

