using PMD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
//using UnityEditor;
using UnityEngine;
namespace PMD
{
    // PMDのフォーマットクラス
    public class PMDFormat
    {
        public string path;         // フルパス
        public string name;         // 拡張子とパス抜きのファイルの名前
        public string folder;       // ファイル名抜きのパス

        public Header head;
        public VertexList vertex_list;
        public FaceVertexList face_vertex_list;
        public MaterialList material_list;
        public BoneList bone_list;
        public IKList ik_list;
        public SkinList skin_list;
        public SkinNameList skin_name_list;
        public BoneNameList bone_name_list;
        public BoneDisplayList bone_display_list;
        public EnglishHeader eg_head;
        public EnglishBoneNameList eg_bone_name_list;
        public EnglishSkinNameList eg_skin_name_list;
        public EnglishBoneDisplayList eg_bone_display_list;
        public ToonTextureList toon_texture_list;
        public RigidbodyList rigidbody_list;
        public RigidbodyJointList rigidbody_joint_list;

        public class Header
        {
            public byte[] magic; // "Pmd"
            public float version; // 00 00 80 3F == 1.00
            public string model_name;
            public string comment;
        }

        public class VertexList
        {
            public uint vert_count; // 頂点数
            public Vertex[] vertex;  // 頂点データ(38bytes/頂点)
        }

        public class Vertex
        {
            public Vector3 pos; // x, y, z // 座標
            public Vector3 normal_vec; // nx, ny, nz // 法線ベクトル
            public Vector2 uv; // u, v // UV座標 // MMDは頂点UV
            public ushort[] bone_num; // ボーン番号1、番号2 // モデル変形(頂点移動)時に影響
            public byte bone_weight; // ボーン1に与える影響度 // min:0 max:100 // ボーン2への影響度は、(100 - bone_weight)
            public byte edge_flag; // 0:通常、1:エッジ無効 // エッジ(輪郭)が有効の場合
        }

        // 面頂点リスト
        public class FaceVertexList
        {
            public uint face_vert_count; // 頂点数
            public ushort[] face_vert_index; // 頂点番号(3個/面)
        }

        public class MaterialList
        {
            public uint material_count; // 材質数
            public Material[] material; // 材質データ(70bytes/material)
        }

        public class Material
        {
            public Color diffuse_color; // dr, dg, db // 減衰色
            public float alpha;
            public float specularity;
            public Color specular_color; // sr, sg, sb // 光沢色
            public Color mirror_color; // mr, mg, mb // 環境色(ambient)
            public byte toon_index; // toon??.bmp // 0.bmp:0xFF, 1(01).bmp:0x00 ・・・ 10.bmp:0x09
            public byte edge_flag; // 輪郭、影
            public uint face_vert_count; // 面頂点数 // インデックスに変換する場合は、材質0から順に加算
            public string texture_file_name; // テクスチャファイル名またはスフィアファイル名 // 20バイトぎりぎりまで使える(終端の0x00は無くても動く)
            public string sphere_map_name;  // スフィアマップ用

            /*
            テクスチャファイル名またはスフィアファイル名の補足：

            テクスチャファイルにスフィアファイルを乗算または加算する場合
            (MMD 5.12以降)
            "テクスチャ名.bmp*スフィア名.sph" で乗算
            "テクスチャ名.bmp*スフィア名.spa" で加算

            (MMD 5.11)
            "テクスチャ名.bmp/スフィア名.sph" で乗算

            (MMD 5.09あたり-)
            "テクスチャ名.bmp" または "スフィア名.sph"
            */
        }

        public class BoneList
        {
            public ushort bone_count; // ボーン数
            public Bone[] bone; // ボーンデータ(39bytes/bone)
        }

        public class Bone
        {
            public string bone_name; // ボーン名
            public ushort parent_bone_index; // 親ボーン番号(ない場合は0xFFFF)
            public ushort tail_pos_bone_index; // tail位置のボーン番号(チェーン末端の場合は0xFFFF) // 親：子は1：多なので、主に位置決め用
            public byte bone_type; // ボーンの種類
            public ushort ik_parent_bone_index; // IKボーン番号(影響IKボーン。ない場合は0)
            public Vector3 bone_head_pos; // x, y, z // ボーンのヘッドの位置

            /*
            ・ボーンの種類
            0:回転 1:回転と移動 2:IK 3:不明 4:IK影響下 5:回転影響下 6:IK接続先 7:非表示 8:捻り 9:回転運動
            */
        }

        public class IKList
        {
            public ushort ik_data_count; // IKデータ数
            public IK[] ik_data; // IKデータ((11+2*ik_chain_length)/IK)
        }

        public class IK
        {
            public ushort ik_bone_index; // IKボーン番号
            public ushort ik_target_bone_index; // IKターゲットボーン番号 // IKボーンが最初に接続するボーン
            public byte ik_chain_length; // IKチェーンの長さ(子の数)
            public ushort iterations; // 再帰演算回数 // IK値1
            public float control_weight; // IKの影響度 // IK値2
            public ushort[] ik_child_bone_index; // IK影響下のボーン番号
        }

        public class SkinList
        {
            public ushort skin_count; // 表情数
            public SkinData[] skin_data; // 表情データ((25+16*skin_vert_count)/skin)
        }

        public class SkinData
        {
            public string skin_name; //　表情名
            public uint skin_vert_count; // 表情用の頂点数
            public byte skin_type; // 表情の種類 // 0：base、1：まゆ、2：目、3：リップ、4：その他
            public SkinVertexData[] skin_vert_data; // 表情用の頂点のデータ(16bytes/vert)
        }

        public class SkinVertexData
        {
            // 実際の頂点を参照するには
            // int num = vertex_count - skin_vert_count;
            // skin_vert[num]みたいな形で参照しないと無理
            public uint skin_vert_index; // 表情用の頂点の番号(頂点リストにある番号)
            public Vector3 skin_vert_pos; // x, y, z // 表情用の頂点の座標(頂点自体の座標)
        }

        // 表情用枠名
        public class SkinNameList
        {
            public byte skin_disp_count;
            public ushort[] skin_index;     // 表情番号
        }

        // ボーン用枠名
        public class BoneNameList
        {
            public byte bone_disp_name_count;
            public string[] disp_name;      // 50byte
        }

        // ボーン枠用表示リスト
        public class BoneDisplayList
        {
            public uint bone_disp_count;
            public BoneDisplay[] bone_disp;
        }

        public class BoneDisplay
        {
            public ushort bone_index;       // 枠用ボーン番号 
            public byte bone_disp_frame_index;  // 表示枠番号 
        }

        /// <summary>
        /// 英語表記用ヘッダ
        /// </summary>
        public class EnglishHeader
        {
            public byte english_name_compatibility; // 01で英名対応 
            public string model_name_eg;    // 20byte
            public string comment_eg;   // 256byte
        }

        /// <summary>
        /// 英語表記用ボーンの英語名
        /// </summary>
        public class EnglishBoneNameList
        {
            public string[] bone_name_eg;   // 20byte * bone_count
        }

        public class EnglishSkinNameList
        {
            // baseは英名が登録されない 
            public string[] skin_name_eg;   // 20byte * skin_count-1
        }

        public class EnglishBoneDisplayList
        {
            public string[] disp_name_eg;   // 50byte * bone_disp_name_count
        }

        public class ToonTextureList
        {
            public string[] toon_texture_file;  // 100byte * 10個固定 
        }

        public class RigidbodyList
        {
            public uint rigidbody_count;
            public PMD.PMDFormat.Rigidbody[] rigidbody;
        }

        /// <summary>
        /// 剛体
        /// </summary>
        public class Rigidbody
        {
            public string rigidbody_name; // 諸データ：名称 ,20byte
            public int rigidbody_rel_bone_index;// 諸データ：関連ボーン番号 
            public byte rigidbody_group_index; // 諸データ：グループ 
            public ushort rigidbody_group_target; // 諸データ：グループ：対象 // 0xFFFFとの差
            public byte shape_type;  // 形状：タイプ(0:球、1:箱、2:カプセル)  
            public float shape_w;   // 形状：半径(幅) 
            public float shape_h;   // 形状：高さ 
            public float shape_d;   // 形状：奥行 
            public Vector3 pos_pos;  // 位置：位置(x, y, z) 
            public Vector3 pos_rot;  // 位置：回転(rad(x), rad(y), rad(z)) 
            public float rigidbody_weight; // 諸データ：質量 // 00 00 80 3F // 1.0
            public float rigidbody_pos_dim; // 諸データ：移動減 // 00 00 00 00
            public float rigidbody_rot_dim; // 諸データ：回転減 // 00 00 00 00
            public float rigidbody_recoil; // 諸データ：反発力 // 00 00 00 00
            public float rigidbody_friction; // 諸データ：摩擦力 // 00 00 00 00
            public byte rigidbody_type; // 諸データ：タイプ(0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)) // 00 // Bone追従
        }

        public class RigidbodyJointList
        {
            public uint joint_count;
            public Joint[] joint;
        }

        public class Joint
        {
            public string joint_name;   // 20byte
            public uint joint_rigidbody_a; // 諸データ：剛体A 
            public uint joint_rigidbody_b; // 諸データ：剛体B 
            public Vector3 joint_pos; // 諸データ：位置(x, y, z) // 諸データ：位置合せでも設定可 
            public Vector3 joint_rot; // 諸データ：回転(rad(x), rad(y), rad(z)) 
            public Vector3 constrain_pos_1; // 制限：移動1(x, y, z) 
            public Vector3 constrain_pos_2; // 制限：移動2(x, y, z) 
            public Vector3 constrain_rot_1; // 制限：回転1(rad(x), rad(y), rad(z)) 
            public Vector3 constrain_rot_2; // 制限：回転2(rad(x), rad(y), rad(z)) 
            public Vector3 spring_pos; // ばね：移動(x, y, z) 
            public Vector3 spring_rot; // ばね：回転(rad(x), rad(y), rad(z)) 
        }
    }
}
public class PMDSave : MonoBehaviour {
    //PMDFormat format_;
    [SerializeField]
    public string Model;//最长20字节的字符串，模型名称
    [SerializeField]
    public string Descript;//最长255字节
    [SerializeField]
    public Transform[] bones;
    [SerializeField]
    public Transform GenMapRoot;
    public void Save(string file)
    {
        //shift-jis下20字节最长
        byte[] mModel = System.Text.Encoding.GetEncoding(932).GetBytes(Model);
        if (mModel.Length > 20)
        {
            Debug.LogError("模型名称在日语编码下不许超过20字节");
            return;
        }
        mModel = System.Text.Encoding.GetEncoding(932).GetBytes(Descript);
        if (mModel.Length > 255)
        {
            Debug.LogError("描述不能超过255字节,日语编码下");
            return;
        }
        bones = GetComponentInChildren<SkinnedMeshRenderer>().bones;
        file_path_ = file;

        using (FileStream stream = new FileStream(file_path_, FileMode.OpenOrCreate, FileAccess.Write))
        using (BinaryWriter bin = new BinaryWriter(stream))
        {
            FileInfo f = new FileInfo(file_path_);
            fold_path_ = f.Directory.FullName;
            binary_writer_ = bin;
            SaveContent();
        }
    }

    public void SaveMap(string file)
    {
        byte[] mModel = System.Text.Encoding.GetEncoding(932).GetBytes(Model);
        if (mModel.Length > 20)
        {
            Debug.LogError("模型名称在日语编码下不许超过20字节");
            return;
        }
        mModel = System.Text.Encoding.GetEncoding(932).GetBytes(Descript);
        if (mModel.Length > 255)
        {
            Debug.LogError("描述不能超过255字节,日语编码下");
            return;
        }
        
        if (GenMapRoot == null)
        {
            Debug.LogError("生成的合并模型不能为空");
            return;
        }
        file_path_ = file;
        CombineMapMesh();
        using (FileStream stream = new FileStream(file_path_, FileMode.OpenOrCreate, FileAccess.Write))
        using (BinaryWriter bin = new BinaryWriter(stream))
        {
            FileInfo f = new FileInfo(file_path_);
            fold_path_ = f.Directory.FullName;
            binary_writer_ = bin;
            SaveMapContent();
        }
    }
    //把所有面片信息和材质全部合并到一个gameobject上
    public int vertexCnt;
    void CombineMapMesh()
    {
        if (GenMapRoot != null)
        {
            MeshFilter[] mfs = GetComponentsInChildren<MeshFilter>();
            MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();

            MeshFilter mf = GenMapRoot.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = GenMapRoot.gameObject.AddComponent<MeshRenderer>();
            //统计材质
            List<Material> mat = new List<Material>();
            for (int i = 0; i < mrs.Length; i++)
            {
                if (!mrs[i].enabled)
                    continue;
                mat.AddRange(mrs[i].sharedMaterials);
                //for (int j = 0; j < mrs[i].sharedMaterials.Length; j++)
                //    mat.Add(mrs[i].sharedMaterials[j]);
            }
            mr.sharedMaterials = mat.ToArray();
            int subMeshIndex = 0;
            List<Vector3> vertex = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector3> normal = new List<Vector3>();
            List<Color> co = new List<Color>();
            SortedDictionary<int, int[]> subMeshIndices = new SortedDictionary<int, int[]>();
            //统计顶点.
            for (int i = 0; i < mfs.Length; i++)
            {
                //隐藏了的不要
                if (!mrs[i].enabled)
                    continue;
                //推入顶点
                int vertexIndex = vertex.Count;//记录推入之前的索引起始
                if (mfs[i] == null)
                    Debug.DebugBreak();
                for (int j = 0; j < mfs[i].sharedMesh.vertexCount; j++)
                    vertex.Add(mfs[i].transform.localToWorldMatrix * mfs[i].sharedMesh.vertices[j]);
                //推入颜色,UV,法线
                int uv_len = mfs[i].sharedMesh.uv.Length;
                for (int j = 0; j < uv_len; j++)
                    uv.Add(mfs[i].sharedMesh.uv[j]);
                int normal_len = mfs[i].sharedMesh.normals.Length;
                for (int j = 0; j < normal_len; j++)
                    normal.Add(mfs[i].sharedMesh.normals[j]);
                int color_len = mfs[i].sharedMesh.colors.Length;
                for (int j = 0; j < color_len; j++)
                    co.Add(mfs[i].sharedMesh.colors[j]);

                for (int j = 0; j < mfs[i].sharedMesh.subMeshCount; j++)
                {
                    int[] indices = mfs[i].sharedMesh.GetIndices(j);
                    //把这些索引，都加上全局索引下标
                    for (int k = 0; k < indices.Length; k++)
                        indices[k] += vertexIndex;
                    //mf.sharedMesh.SetTriangles(triangle, subMeshIndex);
                    //mf.sharedMesh.SetIndices(indices, MeshTopology.Triangles, subMeshIndex);
                    subMeshIndices.Add(subMeshIndex, indices);
                    subMeshIndex++;
                }
                //mfs[i].sharedMesh.vertices.Length;
                //mfs[i].sharedMesh.colors;
                //mfs[i].sharedMesh.uv;
                //mfs[i].sharedMesh.normals;
                //mfs[i].sharedMesh.subMeshCount;
                //mfs[i].sharedMesh.triangles;
                //mfs[i].sharedMesh.GetIndices();
            }
            mf.sharedMesh = new Mesh();
            mf.sharedMesh.SetVertices(vertex);
            mf.sharedMesh.SetColors(co);
            mf.sharedMesh.uv = uv.ToArray();
            mf.sharedMesh.SetNormals(normal);
            mf.sharedMesh.subMeshCount = subMeshIndices.Count;
            vertexCnt = vertex.Count;
            foreach (var each in subMeshIndices)
            {
                Debug.Log("setIndices:" + each.Key);
                mf.sharedMesh.SetIndices(each.Value, MeshTopology.Triangles, each.Key);
            }
        }
    }

    void SaveMapContent()
    {
        SaveHeader();
        WriteVertexListMap();
        WriteFaceVertexListMap();
        WriteMaterialListMap();

        binary_writer_.Write((ushort)0);//writeBoneList
        binary_writer_.Write((ushort)0); //WriteIKList();
        binary_writer_.Write((ushort)0); //WriteSkinList();
        binary_writer_.Write((byte)0); //WriteSkinName();
        binary_writer_.Write((byte)0); //WriteBoneName();
        binary_writer_.Write((uint)0); //WriteSkinNameList();
        binary_writer_.Write((byte)0); //WriteBoneNameList();
        byte[] bToon = new byte[1000];
        binary_writer_.Write(bToon);
        binary_writer_.Write((uint)0);
        binary_writer_.Write((uint)0);
    }

    private void SaveContent()
    {
        //format_ = new PMDFormat();
        //EntryPathes();
        //binary_reader_ = 
        try
        {
            SaveHeader();// format_.head = ReadHeader();
            WriteVertexList();
            WriteFaceVertexList();
            WriteMaterialList();
            WriteBoneList();
            binary_writer_.Write((ushort)0); //WriteIKList();
            binary_writer_.Write((ushort)0); //WriteSkinList();
            binary_writer_.Write((byte)0); //WriteSkinName();
            binary_writer_.Write((byte)0); //WriteBoneName();
            binary_writer_.Write((uint)0); //WriteSkinNameList();
            binary_writer_.Write((byte)0); //WriteBoneNameList();

            //format_.vertex_list = ReadVertexList();
            //format_.face_vertex_list = ReadFaceVertexList();
            //format_.material_list = ReadMaterialList();
            //format_.bone_list = ReadBoneList();
            //format_.ik_list = ReadIKList();
            //format_.skin_list = ReadSkinList();
            //format_.skin_name_list = ReadSkinNameList();
            //format_.bone_name_list = ReadBoneNameList();
            //format_.bone_display_list = ReadBoneDisplayList();
            //format_.eg_head = ReadEnglishHeader();
            //if (format_.eg_head.english_name_compatibility != 0)
            //{
            //    format_.eg_bone_name_list = ReadEnglishBoneNameList(format_.bone_list.bone_count);
            //    format_.eg_skin_name_list = ReadEnglishSkinNameList(format_.skin_list.skin_count);
            //    format_.eg_bone_display_list = ReadEnglishBoneDisplayList(format_.bone_name_list.bone_disp_name_count);
            //}
            byte[] bToon = new byte[1000];
            binary_writer_.Write(bToon);
            //format_.toon_texture_list = ReadToonTextureList();
            //format_.rigidbody_list = ReadRigidbodyList();
            binary_writer_.Write((uint)0);
            binary_writer_.Write((uint)0);
            //format_.rigidbody_joint_list = ReadRigidbodyJointList();
        }
        catch
        {
            //Debug.Log("Don't read full format");
        }
        binary_writer_.Close();
    }
    //private void EntryPathes()
    //{
    //    format_.path = file_path_;
    //    format_.name = Path.GetFileNameWithoutExtension(file_path_); // .pmdを抜かす
    //    format_.folder = Path.GetDirectoryName(file_path_); // PMDが格納されているフォルダ
    //}

    private void SaveHeader()
    {
        //PMDFormat.Header result = new PMDFormat.Header();
        byte[] pmd = new byte[3] { 0X50, 0X6D, 0X64 };
        binary_writer_.Write(pmd);
        float fVersion = 1.0f;
        binary_writer_.Write(fVersion);
        //result.magic = binary_reader_.ReadBytes(3);//3字节PMD
        //result.version = binary_reader_.ReadSingle();//4字节版本号 1.0
        byte[] modelName = new byte[20];
        for (int i = 0; i < 20; i++)
            modelName[i] = 0x00;
        byte[] mModel = System.Text.Encoding.GetEncoding(932).GetBytes(Model);
        for (int i = 0; i < Mathf.Min(mModel.Length, 20); i++)//20字节模型名称
            modelName[i] = mModel[i];
        binary_writer_.Write(modelName);
        //result.model_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
        //输入描述
        byte[] des = new byte[256];
        for (int i = 0; i < des.Length; i++)
            des[i] = 0x00;
        byte[] des2 = System.Text.Encoding.GetEncoding(932).GetBytes(Descript);
        for (int i = 0; i < Mathf.Min(des2.Length, 256); i++)
            des[i] = des2[i];
        binary_writer_.Write(des);

        //result.comment = ConvertByteToString(binary_reader_.ReadBytes(256), System.Environment.NewLine);
        //return result;
    }

    //private PMDFormat.Header ReadHeader()
    //{
    //    PMDFormat.Header result = new PMDFormat.Header();
    //    result.magic = binary_reader_.ReadBytes(3);
    //    result.version = binary_reader_.ReadSingle();
    //    result.model_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
    //    result.comment = ConvertByteToString(binary_reader_.ReadBytes(256), System.Environment.NewLine);
    //    return result;
    //}

    void WriteVertexListMap()
    {
        MeshFilter render = GenMapRoot.GetComponentInChildren<MeshFilter>();
        binary_writer_.Write((uint)render.sharedMesh.vertexCount);
        for (int i = 0; i < render.sharedMesh.vertexCount; i++)
            WriteVertexMap(i);
    }

    void WriteVertexMap(int i)
    {
        MeshFilter render = GenMapRoot.GetComponentInChildren<MeshFilter>();
        Vector3 pos = render.sharedMesh.vertices[i];
        WriteVector3(binary_writer_, pos);
        Vector3 nor = render.sharedMesh.normals[i];
        WriteVector3(binary_writer_, nor);
        Vector2 uv = render.sharedMesh.uv[i];
        WriteVector2(binary_writer_, uv);
        //BoneWeight bo = render.sharedMesh.boneWeights[i];//2个最重要的骨骼的序号
        //从4个权重中挑选2个最大的，然后规格化。
        //BoneWeight boNew = GetBoneWeightTwo(i, bo);
        binary_writer_.Write((ushort)0);
        binary_writer_.Write((ushort)0);
        binary_writer_.Write((byte)(100));
        binary_writer_.Write((byte)0x00);//普通骨骼
    }

    void WriteVertexList()
    {
        SkinnedMeshRenderer render = GetComponentInChildren<SkinnedMeshRenderer>();
        binary_writer_.Write((uint)render.sharedMesh.vertexCount);
        for (int i = 0; i < render.sharedMesh.vertexCount; i++)
            WriteVertex(i);
    }

    BoneWeight GetBoneWeightTwo(int j, BoneWeight b)
    {
        BoneWeight br = new BoneWeight();
        List<int> boMin = new List<int> { b.boneIndex0, b.boneIndex1, b.boneIndex2, b.boneIndex3 };
        List<float> boWMin = new List<float> { b.weight0, b.weight1, b.weight2, b.weight3 };
        float min = 0.0f;
        int idxmin = -1;
        for (int i = 0; i < boWMin.Count; i++)
        {
            if (min < boWMin[i] && boMin[i] != 0)
            {
                min = boWMin[i];
                idxmin = boMin[i];
            }
        }
        if (idxmin != -1)
        {
            br.boneIndex0 = idxmin;
            br.weight0 = min;
            boMin.Remove(idxmin);
            boWMin.Remove(min);
        }
        min = 0.0f;
        idxmin = -1;

        for (int i = 0; i < boWMin.Count; i++)
        {
            if (min < boWMin[i] && boMin[i] != 0)
            {
                min = boWMin[i];
                idxmin = boMin[i];
            }
        }
        if (idxmin != -1)
        {
            br.boneIndex1 = idxmin;
            br.weight1 = min;
            boMin.Remove(idxmin);
            boWMin.Remove(min);
        }

        float one = br.weight0 + br.weight1; 
        br.weight0 = br.weight0 / one;
        br.weight1 = br.weight1 / one;
        return br;
    }

    void WriteVertex(int i)
    {
        SkinnedMeshRenderer render = GetComponentInChildren<SkinnedMeshRenderer>();
        Vector3 pos = render.sharedMesh.vertices[i];
        WriteVector3(binary_writer_, pos);
        Vector3 nor = render.sharedMesh.normals[i];
        WriteVector3(binary_writer_, nor);
        Vector2 uv = render.sharedMesh.uv[i];
        WriteVector2(binary_writer_, uv);
        BoneWeight bo = render.sharedMesh.boneWeights[i];//2个最重要的骨骼的序号
        //从4个权重中挑选2个最大的，然后规格化。
        BoneWeight boNew = GetBoneWeightTwo(i, bo);
        binary_writer_.Write((ushort)boNew.boneIndex0);
        binary_writer_.Write((ushort)boNew.boneIndex1);
        binary_writer_.Write((byte)(Mathf.Min(bo.weight0 * 100, 100)));
        binary_writer_.Write((byte)0x00);//普通骨骼
        float weight0, weight1;
        weight0 = bo.weight0;
        weight1 = bo.weight1;
        //第一个骨骼的权重
        //0;普通骨骼？
    }

    private PMDFormat.VertexList ReadVertexList()
    {
        PMDFormat.VertexList result = new PMDFormat.VertexList();
        //result.vert_count = binary_reader_.ReadUInt32();
        //result.vertex = new PMDFormat.Vertex[result.vert_count];
        //for (int i = 0; i < result.vert_count; i++)
        //{
        //    result.vertex[i] = ReadVertex();
        //}
        return result;
    }

    private PMDFormat.Vertex ReadVertex()
    {
        PMDFormat.Vertex result = new PMDFormat.Vertex();
        //result.pos = ReadSinglesToVector3(binary_reader_);
        //result.normal_vec = ReadSinglesToVector3(binary_reader_);
        //result.uv = ReadSinglesToVector2(binary_reader_);
        //result.bone_num = ReadUInt16s(binary_reader_, 2);
        //result.bone_weight = binary_reader_.ReadByte();
        //result.edge_flag = binary_reader_.ReadByte();
        return result;
    }

    void WriteFaceVertexListMap()
    {
        MeshFilter ren = GenMapRoot.GetComponentInChildren<MeshFilter>();
        uint face_count = (uint)ren.sharedMesh.triangles.Length;
        binary_writer_.Write(face_count);
        for (int i = 0; i < face_count; i++)
            binary_writer_.Write((ushort)ren.sharedMesh.triangles[i]);
    }

    private void WriteFaceVertexList()
    {
        SkinnedMeshRenderer ren = GetComponentInChildren<SkinnedMeshRenderer>();
        uint face_count = (uint)ren.sharedMesh.triangles.Length;
        binary_writer_.Write(face_count);
        for (int i = 0; i < face_count; i++)
            binary_writer_.Write((ushort)ren.sharedMesh.triangles[i]);
        //result.face_vert_count = binary_reader_.ReadUInt32();
        //result.face_vert_index = ReadUInt16s(binary_reader_, result.face_vert_count);
        //return result;
    }

    private PMDFormat.FaceVertexList ReadFaceVertexList()
    {
        PMDFormat.FaceVertexList result = new PMDFormat.FaceVertexList();
        //result.face_vert_count = binary_reader_.ReadUInt32();
        //result.face_vert_index = ReadUInt16s(binary_reader_, result.face_vert_count);
        return result;
    }

    void WriteMaterialListMap()
    {
        MeshRenderer mr = GenMapRoot.GetComponentInChildren<MeshRenderer>();
        int unused = 0;
        for (int i = 0; i < mr.sharedMaterials.Length; i++)
            if (mr.sharedMaterials[i] == null)
                unused++;
        binary_writer_.Write(mr.sharedMaterials.Length - unused);
        for (int i = 0; i < mr.sharedMaterials.Length; i++)
        {
            if (mr.sharedMaterials[i] == null)
                continue;
            WriteMaterialMap(i);
        }
    }

    private void WriteMaterialList()
    {
        SkinnedMeshRenderer mr = GetComponentInChildren<SkinnedMeshRenderer>();
        int unused = 0;
        for (int i = 0; i < mr.sharedMaterials.Length; i++)
            if (mr.sharedMaterials[i] == null)
                unused++;
        binary_writer_.Write(mr.sharedMaterials.Length - unused);
        for (int i = 0; i < mr.sharedMaterials.Length; i++)
        {
            if (mr.sharedMaterials[i] == null)
                continue;
            WriteMaterial(i);
        }
    }

    void WriteMaterialMap(int i)
    {
        MeshFilter mr = GenMapRoot.GetComponentInChildren<MeshFilter>();
        MeshRenderer mrr = mr.GetComponent<MeshRenderer>();
        WriteColor(Color.white);
        binary_writer_.Write(1.0f);//alpha
        binary_writer_.Write(0.0f);//specularity
        WriteColor(Color.black);//specular_color
        WriteColor(Color.white);//ambcolor;
        binary_writer_.Write((byte)0x00);//toon_index;
        binary_writer_.Write((byte)0x00);//edge_flag
        binary_writer_.Write(mr.sharedMesh.GetTriangles(i).Length);//face_vert_count;
        byte[] tex = new byte[20];
        if (!string.IsNullOrEmpty(mrr.sharedMaterials[i].mainTexture.name))
        {
            //string path = AssetDatabase.GetAssetPath(mrr.sharedMaterials[i].mainTexture);
            //string full_path = path;
            //int splitIdx = path.LastIndexOf('/');
            //if (splitIdx != -1)
            //    path = path.Substring(splitIdx + 1);
            //try
            //{
            //    File.Copy(full_path, fold_path_ + "/" + path);
            //}
            //catch{
            //}
            //byte[] texb = System.Text.Encoding.GetEncoding(932).GetBytes(path);
            //for (int j = 0; j < Mathf.Min(texb.Length, 20); j++)
            //    tex[j] = texb[j];
        }
        binary_writer_.Write(tex);
    }

    void WriteMaterial(int i)
    {
        SkinnedMeshRenderer mr = GetComponentInChildren<SkinnedMeshRenderer>();
        WriteColor(Color.white);
        binary_writer_.Write(1.0f);//alpha
        binary_writer_.Write(0.0f);//specularity
        WriteColor(Color.white);//specular_color
        WriteColor(Color.white);//ambcolor;
        binary_writer_.Write((byte)0x00);//toon_index;
        binary_writer_.Write((byte)0x00);//edge_flag
        binary_writer_.Write(mr.sharedMesh.GetTriangles(i).Length);//face_vert_count;
        byte[] tex = new byte[20];
        if (!string.IsNullOrEmpty(mr.sharedMaterials[i].mainTexture.name))
        {
            //string path = AssetDatabase.GetAssetPath(mr.sharedMaterials[i].mainTexture);
            //int splitIdx = path.LastIndexOf('/');
            //if (splitIdx != -1)
            //    path = path.Substring(splitIdx + 1);
            //byte[] texb = System.Text.Encoding.GetEncoding(932).GetBytes(path);
            //for (int j = 0; j < Mathf.Min(texb.Length, 20); j++)
            //    tex[j] = texb[j];
        }
        binary_writer_.Write(tex);
    }

    void WriteColor(Color c)
    {
        binary_writer_.Write(c.r);
        binary_writer_.Write(c.g);
        binary_writer_.Write(c.b);
        //binary_writer_.Write(c.a);
    }

    private PMDFormat.MaterialList ReadMaterialList()
    {
        PMDFormat.MaterialList result = new PMDFormat.MaterialList();
        //result.material_count = binary_reader_.ReadUInt32();
        //result.material = new PMDFormat.Material[result.material_count];
        //for (int i = 0; i < result.material_count; i++)
        //{
        //    result.material[i] = ReadMaterial();
        //}
        return result;
    }

    private PMDFormat.Material ReadMaterial()
    {
        PMDFormat.Material result = new PMDFormat.Material();
//        result.diffuse_color = ReadSinglesToColor(binary_reader_, 1);
//        result.alpha = binary_reader_.ReadSingle();
//        result.specularity = binary_reader_.ReadSingle();
//        result.specular_color = ReadSinglesToColor(binary_reader_, 1);
//        result.mirror_color = ReadSinglesToColor(binary_reader_, 1);
//        result.toon_index = binary_reader_.ReadByte();
//        result.edge_flag = binary_reader_.ReadByte();
//        result.face_vert_count = binary_reader_.ReadUInt32();

//        // テクスチャ名の抜き出し
//        // スフィアマップも行う
//        string buf = ConvertByteToString(binary_reader_.ReadBytes(20), "");

//        //Debug by Wilfrem: テクスチャが無い場合を考慮していない
//        //Debug by Wilfrem: テクスチャはfoo.bmp*bar.sphのパターンだけなのか？ bar.sph*foo.bmpのパターンがあり得るのでは？ 対策をしておくべき
//        //Debug by GRGSIBERIA: スフィアマップとテクスチャが逆になる現象が発生したので修正
//        //Debug by GRGSIBERIA: "./テクスチャ名"で始まるモデルで異常発生したので修正
//        if (!string.IsNullOrEmpty(buf.Trim()))
//        {
//            string[] textures = buf.Trim().Split('*');
//            foreach (var tex in textures)
//            {
//                string texNameEndAssignVar = "";
//                string ext = Path.GetExtension(tex);
//                if (ext == ".sph" || ext == ".spa")
//                {
//                    result.sphere_map_name = tex;
//                    /*} else if (string.IsNullOrEmpty(tex)) {
//                        result.texture_file_name=""; */
//                }
//                else
//                {
//                    if (tex.Split('/')[0] == ".")
//                    {
//                        // テクスチャ名の後端に"./"があった場合の回避処理 
//                        string[] texNameBuf = tex.Split('/');
//                        for (int i = 1; i < texNameBuf.Length - 1; i++)
//                        {
//                            texNameEndAssignVar += texNameBuf[i] + "/";
//                        }
//                        texNameEndAssignVar += texNameBuf[texNameBuf.Length - 1];
//                    }
//                    else
//                    {
//                        // 特に異常がない場合はそのまま代入 
//                        texNameEndAssignVar = tex;
//                    }
//#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
//                    // MACの場合，濁点のあるひらがなを使うと動かないらしいので対策
//                    // http://sourceforge.jp/ticket/browse.php?group_id=6158&tid=31929
//                    texNameEndAssignVar = texNameEndAssignVar.Normalize(NormalizationForm.FormKD);
//#endif
//                    result.texture_file_name = texNameEndAssignVar;
//                }
//            }
//        }
//        else
//        {
//            result.sphere_map_name = "";
//            result.texture_file_name = "";
//        }
//        if (string.IsNullOrEmpty(result.texture_file_name))
//        {
//            result.texture_file_name = "";
//        }
        return result;
    }

    void WriteBoneList()
    {
        SkinnedMeshRenderer ren = GetComponentInChildren<SkinnedMeshRenderer>();
        binary_writer_.Write((ushort)ren.bones.Length);
        for (int i = 0; i < ren.bones.Length; i++)
            WriteBone(ren.bones, ren.rootBone, i);
    }

    int GetBoneIndex(Transform trans)
    {
        SkinnedMeshRenderer ren = GetComponentInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < ren.bones.Length; i++)
        {
            if (trans == ren.bones[i])
                return i;
        }
        return -1;
    }

    //在父级别找到自身的下一个兄弟节点，返回该节点在骨骼中的位置
    int GetSonBone(Transform trans)
    {
        if (trans.childCount == 0)
            return -1;
        return GetBoneIndex(trans.GetChild(0));
    }

    void WriteBone(Transform[] bones, Transform root, int index)
    {
        SortedDictionary<string, string> kvBoneConvert = new SortedDictionary<string, string>();
        kvBoneConvert.Add("b", "センター");
        kvBoneConvert.Add("bad_Pelvis", "下半身先");
        kvBoneConvert.Add("bau_Spine", "下半身");
        kvBoneConvert.Add("bad_L_Thigh", "左足");
        kvBoneConvert.Add("bad_L_Calf", "左ひざ");
        kvBoneConvert.Add("bad_L_Foot", "左足首");
        kvBoneConvert.Add("bad_R_Thigh", "右足");
        kvBoneConvert.Add("bad_R_Calf", "右ひざ");
        kvBoneConvert.Add("bad_R_Foot", "右足首");
        kvBoneConvert.Add("bau_Spine1", "上半身");
        kvBoneConvert.Add("bau_Neck", "首");
        kvBoneConvert.Add("bau_Head", "頭");
        kvBoneConvert.Add("bau_L_Clavicle", "左肩");
        kvBoneConvert.Add("bau_L_UpperArm", "左腕");
        kvBoneConvert.Add("bau_L_Forearm", "左ひじ");
        kvBoneConvert.Add("bau_L_Hand", "左手首");
        kvBoneConvert.Add("bbu_L_Finger0", "左親指１");
        kvBoneConvert.Add("bbu_L_Finger01", "左親指２");
        kvBoneConvert.Add("bbu_L_Finger1", "左中指１");
        kvBoneConvert.Add("bbu_L_Finger11", "左中指２");
        kvBoneConvert.Add("bau_R_Clavicle", "右肩");
        kvBoneConvert.Add("bau_R_UpperArm", "右腕");
        kvBoneConvert.Add("bau_R_Forearm", "右ひじ");
        kvBoneConvert.Add("bau_R_Hand", "右手首");
        kvBoneConvert.Add("bbu_R_Finger0", "右親指１");
        kvBoneConvert.Add("bbu_R_Finger01", "右親指２");
        kvBoneConvert.Add("bbu_R_Finger1", "右中指１");
        kvBoneConvert.Add("bbu_R_Finger11", "右中指２");
        if (kvBoneConvert.ContainsKey(bones[index].name))
            WriteString(20, kvBoneConvert[bones[index].name]);
        else
            WriteString(20, bones[index].name);
        int k = GetBoneIndex(bones[index].parent); //父节点序号
        binary_writer_.Write((ushort)k);
        k = GetSonBone(bones[index]);//同一层下一个节点序号
        binary_writer_.Write((ushort)k);
        binary_writer_.Write((byte)1);//骨骼类型
        binary_writer_.Write((ushort)0);//父IK骨骼
        binary_writer_.Write(bones[index].transform.position.x);
        binary_writer_.Write(bones[index].transform.position.y);
        binary_writer_.Write(bones[index].transform.position.z);
    }

    void WriteString(int length, string content)
    {
        byte[] c = new byte[length];
        byte[] d = System.Text.Encoding.GetEncoding(932).GetBytes(content);
        for (int i = 0; i < Mathf.Min(length, d.Length); i++)
            c[i] = d[i];
        binary_writer_.Write(c);
    }

    private PMDFormat.BoneList ReadBoneList()
    {
        PMDFormat.BoneList result = new PMDFormat.BoneList();
        //result.bone_count = binary_reader_.ReadUInt16();
        ////Debug.Log("BoneCount:"+bone_count);
        //result.bone = new PMDFormat.Bone[result.bone_count];
        //for (int i = 0; i < result.bone_count; i++)
        //{
        //    result.bone[i] = ReadBone();
        //}
        return result;
    }

    private PMDFormat.Bone ReadBone()
    {
        PMDFormat.Bone result = new PMDFormat.Bone();
        //result.bone_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
        //result.parent_bone_index = binary_reader_.ReadUInt16();
        //result.tail_pos_bone_index = binary_reader_.ReadUInt16();
        //result.bone_type = binary_reader_.ReadByte();
        //result.ik_parent_bone_index = binary_reader_.ReadUInt16();
        //result.bone_head_pos = ReadSinglesToVector3(binary_reader_);
        return result;
    }

    private PMDFormat.IKList ReadIKList()
    {
        PMDFormat.IKList result = new PMDFormat.IKList();
        //result.ik_data_count = binary_reader_.ReadUInt16();
        ////Debug.Log("IKDataCount:"+ik_data_count);
        //result.ik_data = new PMDFormat.IK[result.ik_data_count];
        //for (int i = 0; i < result.ik_data_count; i++)
        //{
        //    result.ik_data[i] = ReadIK();
        //}
        return result;
    }

    private PMDFormat.IK ReadIK()
    {
        PMDFormat.IK result = new PMDFormat.IK();
        //result.ik_bone_index = binary_reader_.ReadUInt16();
        //result.ik_target_bone_index = binary_reader_.ReadUInt16();
        //result.ik_chain_length = binary_reader_.ReadByte();
        //result.iterations = binary_reader_.ReadUInt16();
        //result.control_weight = binary_reader_.ReadSingle();
        //result.ik_child_bone_index = ReadUInt16s(binary_reader_, result.ik_chain_length);
        return result;
    }

    private PMDFormat.SkinList ReadSkinList()
    {
        PMDFormat.SkinList result = new PMDFormat.SkinList();
        //result.skin_count = binary_reader_.ReadUInt16();
        ////Debug.Log("SkinCount:"+skin_count);
        //result.skin_data = new PMDFormat.SkinData[result.skin_count];
        //for (int i = 0; i < result.skin_count; i++)
        //{
        //    result.skin_data[i] = ReadSkinData();
        //}
        return result;
    }

    private PMDFormat.SkinData ReadSkinData()
    {
        PMDFormat.SkinData result = new PMDFormat.SkinData();
        //result.skin_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
        //result.skin_vert_count = binary_reader_.ReadUInt32();
        //result.skin_type = binary_reader_.ReadByte();
        //result.skin_vert_data = new PMDFormat.SkinVertexData[result.skin_vert_count];
        //for (int i = 0; i < result.skin_vert_count; i++)
        //{
        //    result.skin_vert_data[i] = ReadSkinVertexData();
        //}
        return result;
    }

    private PMDFormat.SkinVertexData ReadSkinVertexData()
    {
        PMDFormat.SkinVertexData result = new PMDFormat.SkinVertexData();
        //result.skin_vert_index = binary_reader_.ReadUInt32();
        //result.skin_vert_pos = ReadSinglesToVector3(binary_reader_);
        return result;
    }


    private PMDFormat.SkinNameList ReadSkinNameList()
    {
        PMDFormat.SkinNameList result = new PMDFormat.SkinNameList();
        //result.skin_disp_count = binary_reader_.ReadByte();
        //result.skin_index = ReadUInt16s(binary_reader_, result.skin_disp_count);
        return result;
    }

    private PMDFormat.BoneNameList ReadBoneNameList()
    {
        PMDFormat.BoneNameList result = new PMDFormat.BoneNameList();
        //result.bone_disp_name_count = binary_reader_.ReadByte();
        //result.disp_name = new string[result.bone_disp_name_count];
        //for (int i = 0; i < result.bone_disp_name_count; i++)
        //{
        //    result.disp_name[i] = ConvertByteToString(binary_reader_.ReadBytes(50), "");
        //}
        return result;
    }

    private PMDFormat.BoneDisplayList ReadBoneDisplayList()
    {
        PMDFormat.BoneDisplayList result = new PMDFormat.BoneDisplayList();
        //result.bone_disp_count = binary_reader_.ReadUInt32();
        //result.bone_disp = new PMDFormat.BoneDisplay[result.bone_disp_count];
        //for (int i = 0; i < result.bone_disp_count; i++)
        //{
        //    result.bone_disp[i] = ReadBoneDisplay();
        //}
        return result;
    }

    private PMDFormat.BoneDisplay ReadBoneDisplay()
    {
        PMDFormat.BoneDisplay result = new PMDFormat.BoneDisplay();
        //result.bone_index = binary_reader_.ReadUInt16();
        //result.bone_disp_frame_index = binary_reader_.ReadByte();
        return result;
    }

    private PMDFormat.EnglishHeader ReadEnglishHeader()
    {
        PMDFormat.EnglishHeader result = new PMDFormat.EnglishHeader();
        //result.english_name_compatibility = binary_reader_.ReadByte();

        //if (result.english_name_compatibility != 0)
        //{
        //    // 英語名対応あり
        //    result.model_name_eg = ConvertByteToString(binary_reader_.ReadBytes(20), "");
        //    result.comment_eg = ConvertByteToString(binary_reader_.ReadBytes(256), System.Environment.NewLine);
        //}
        return result;
    }

    private PMDFormat.EnglishBoneNameList ReadEnglishBoneNameList(int boneCount)
    {
        PMDFormat.EnglishBoneNameList result = new PMDFormat.EnglishBoneNameList();
        //result.bone_name_eg = new string[boneCount];
        //for (int i = 0; i < boneCount; i++)
        //{
        //    result.bone_name_eg[i] = ConvertByteToString(binary_reader_.ReadBytes(20), "");
        //}
        return result;
    }

    private PMDFormat.EnglishSkinNameList ReadEnglishSkinNameList(int skinCount)
    {
        PMDFormat.EnglishSkinNameList result = new PMDFormat.EnglishSkinNameList();
        //result.skin_name_eg = new string[skinCount];
        //for (int i = 0; i < skinCount - 1; i++)
        //{
        //    result.skin_name_eg[i] = ConvertByteToString(binary_reader_.ReadBytes(20), "");
        //}
        return result;
    }

    private PMDFormat.EnglishBoneDisplayList ReadEnglishBoneDisplayList(int boneDispNameCount)
    {
        PMDFormat.EnglishBoneDisplayList result = new PMDFormat.EnglishBoneDisplayList();
        //result.disp_name_eg = new string[boneDispNameCount];
        //for (int i = 0; i < boneDispNameCount; i++)
        //{
        //    result.disp_name_eg[i] = ConvertByteToString(binary_reader_.ReadBytes(50), "");
        //}
        return result;
    }

    private PMDFormat.ToonTextureList ReadToonTextureList()
    {
        PMDFormat.ToonTextureList result = new PMDFormat.ToonTextureList();
        //result.toon_texture_file = new string[10];
        //for (int i = 0; i < result.toon_texture_file.Length; i++)
        //{
        //    result.toon_texture_file[i] = ConvertByteToString(binary_reader_.ReadBytes(100), "");
        //}
        return result;
    }

    private PMDFormat.RigidbodyList ReadRigidbodyList()
    {
        PMDFormat.RigidbodyList result = new PMDFormat.RigidbodyList();
        //result.rigidbody_count = binary_reader_.ReadUInt32();
        //result.rigidbody = new PMDFormat.Rigidbody[result.rigidbody_count];
        //for (int i = 0; i < result.rigidbody_count; i++)
        //{
        //    result.rigidbody[i] = ReadRigidbody();
        //}
        return result;
    }

    private PMDFormat.Rigidbody ReadRigidbody()
    {
        PMDFormat.Rigidbody result = new PMDFormat.Rigidbody();
        //result.rigidbody_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
        //result.rigidbody_rel_bone_index = binary_reader_.ReadUInt16();
        //result.rigidbody_group_index = binary_reader_.ReadByte();
        //result.rigidbody_group_target = binary_reader_.ReadUInt16();
        //result.shape_type = binary_reader_.ReadByte();
        //result.shape_w = binary_reader_.ReadSingle();
        //result.shape_h = binary_reader_.ReadSingle();
        //result.shape_d = binary_reader_.ReadSingle();
        //result.pos_pos = ReadSinglesToVector3(binary_reader_);
        //result.pos_rot = ReadSinglesToVector3(binary_reader_);
        //result.rigidbody_weight = binary_reader_.ReadSingle();
        //result.rigidbody_pos_dim = binary_reader_.ReadSingle();
        //result.rigidbody_rot_dim = binary_reader_.ReadSingle();
        //result.rigidbody_recoil = binary_reader_.ReadSingle();
        //result.rigidbody_friction = binary_reader_.ReadSingle();
        //result.rigidbody_type = binary_reader_.ReadByte();
        return result;
    }

    private PMDFormat.RigidbodyJointList ReadRigidbodyJointList()
    {
        PMDFormat.RigidbodyJointList result = new PMDFormat.RigidbodyJointList();
        //result.joint_count = binary_reader_.ReadUInt32();
        //result.joint = new PMDFormat.Joint[result.joint_count];
        //for (int i = 0; i < result.joint_count; i++)
        //{
        //    result.joint[i] = ReadJoint();
        //}
        return result;
    }

    private PMDFormat.Joint ReadJoint()
    {
        PMDFormat.Joint result = new PMDFormat.Joint();
        //result.joint_name = ConvertByteToString(binary_reader_.ReadBytes(20), "");
        //result.joint_rigidbody_a = binary_reader_.ReadUInt32();
        //result.joint_rigidbody_b = binary_reader_.ReadUInt32();
        //result.joint_pos = ReadSinglesToVector3(binary_reader_);
        //result.joint_rot = ReadSinglesToVector3(binary_reader_);
        //result.constrain_pos_1 = ReadSinglesToVector3(binary_reader_);
        //result.constrain_pos_2 = ReadSinglesToVector3(binary_reader_);
        //result.constrain_rot_1 = ReadSinglesToVector3(binary_reader_);
        //result.constrain_rot_2 = ReadSinglesToVector3(binary_reader_);
        //result.spring_pos = ReadSinglesToVector3(binary_reader_);
        //result.spring_rot = ReadSinglesToVector3(binary_reader_);
        return result;
    }

    // ShiftJISからUTF-8に変換してstringで返す
    private static string ConvertByteToString(byte[] bytes, string line_feed_code = null)
    {
        // パディングの消去, 文字を詰める
        if (bytes[0] == 0) return "";
        int count;
        for (count = 0; count < bytes.Length; count++) if (bytes[count] == 0) break;
        byte[] buf = new byte[count];       // NULL文字を含めるとうまく行かない
        for (int i = 0; i < count; i++)
        {
            buf[i] = bytes[i];
        }

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
        buf = Encoding.Convert(Encoding.GetEncoding(932), Encoding.UTF8, buf);
#else
		buf = Encoding.Convert(Encoding.GetEncoding(0), Encoding.UTF8, buf);

#endif
        string result = Encoding.UTF8.GetString(buf);
        if (null != line_feed_code)
        {
            //改行コード統一(もしくは除去)
            result = result.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", line_feed_code);
        }
        return result;
    }

    static void WriteVector3(BinaryWriter b, Vector3 pos)
    {
        b.Write(pos.x);
        b.Write(pos.y);
        b.Write(pos.z);
    }
    static void WriteVector2(BinaryWriter b, Vector2 uv)
    {
        b.Write(uv.x);
        b.Write(1 - uv.y);
    }

    private static Vector3 ReadSinglesToVector3(BinaryReader bin)
    {
        const int count = 3;
        float[] result = new float[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = bin.ReadSingle();
            if (float.IsNaN(result[i])) result[i] = 0.0f; //非数値なら回避
        }
        return new Vector3(result[0], result[1], result[2]);
    }

    private static Vector2 ReadSinglesToVector2(BinaryReader bin)
    {
        const int count = 2;
        float[] result = new float[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = bin.ReadSingle();
            if (float.IsNaN(result[i])) result[i] = 0.0f; //非数値なら回避
        }
        return new Vector2(result[0], result[1]);
    }

    //private static Color ReadSinglesToColor(BinaryReader bin)
    //{
    //    const int count = 4;
    //    float[] result = new float[count];
    //    for (int i = 0; i < count; i++)
    //    {
    //        result[i] = bin.ReadSingle();
    //    }
    //    return new Color(result[0], result[1], result[2], result[3]);
    //}

    private static Color ReadSinglesToColor(BinaryReader bin, float fix_alpha)
    {
        const int count = 3;
        float[] result = new float[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = bin.ReadSingle();
        }
        return new Color(result[0], result[1], result[2], fix_alpha);
    }

    private static ushort[] ReadUInt16s(BinaryReader bin, uint count)
    {
        ushort[] result = new ushort[count];
        for (uint i = 0; i < count; i++)
        {
            result[i] = bin.ReadUInt16();
        }
        return result;
    }

    string file_path_;
    string fold_path_;
    BinaryWriter binary_writer_;
}
