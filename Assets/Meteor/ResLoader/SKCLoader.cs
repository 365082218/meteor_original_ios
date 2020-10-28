using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;

public class SkcLoader:Singleton<SkcLoader>
{
    SortedDictionary<string, SkcFile> SkcFile = new SortedDictionary<string, global::SkcFile>();
    SortedDictionary<int, SkcFile> GlobalSkcFile = new SortedDictionary<int, global::SkcFile>();
    public void Clear() {
        SkcFile.Clear();
        GlobalSkcFile.Clear();
    }
    public SkcFile Load(string file)
    {
        if (SkcFile.ContainsKey(file))
            return SkcFile[file];
        SkcFile f = new global::SkcFile();
        f.Load(file);
        if (f.error)
            return null;
        SkcFile[file] = f;
        return f;
    }

    SkcFile LoadPluginModel(int modelIdx)
    {
        if (GlobalSkcFile.ContainsKey(modelIdx))
            return GlobalSkcFile[modelIdx];
        SkcFile f = new global::SkcFile();
        f.LoadModel(modelIdx);
        if (f.error)
            return null;
        GlobalSkcFile.Add(modelIdx, f);
        return f;
    }

    public SkcFile Load(int characterIdx)
    {
        if (CombatData.Ins.Chapter != null) {
            SortedDictionary<int, string> models = CombatData.Ins.GScript.GetModel();
            if (models != null && models.ContainsKey(characterIdx)) {
                string modelPath = CombatData.Ins.Chapter.GetResPath(FileExt.Skc, models[characterIdx]);
                return Load(modelPath);
            }
        } 
        //下载安装的模型或客户端自带的
        string BoneCnt = "";
        if (Main.Ins.GameStateMgr != null) {
            if (GameStateMgr.Ins.gameStatus != null) {
                switch (GameStateMgr.Ins.gameStatus.Quality) {
                    case 0:
                        BoneCnt = ""; break;
                    case 1:
                        BoneCnt = "_800"; break;
                    case 2:
                        BoneCnt = "_300"; break;
                }
                //如果选择 范旋-他300面的模型 骨骼权重最大是5，不好手动调整,用800面的代替
                if (characterIdx == 16 && GameStateMgr.Ins.gameStatus.Quality == 2)
                    BoneCnt = "_800";
            }
        }

        if (characterIdx >= CombatData.Ins.MaxModel)
            return LoadPluginModel(characterIdx);

        return Load(string.Format("p{0}{1}.skc", characterIdx, BoneCnt));
    }
}

public enum ParseError
{
    None,
    Miss,
    ParseError,
}

public class MaterialUnit
{
    public string Texture;
    public Color ColorKey;
    public Color Ambient;
    public Color Diffuse;
    public Color Specular;
    public Color Emissive;
    public float Opacity;
    public string Option;
    public bool TwoSide;
}

public class BoneWeightEx
{
    public int BoneIndex;
    public float Weight;
}
public class SkcFile
{
    int StaticSkins = 0;
    int DynmaicSkins = 0;
    public string Skin;
    public MaterialUnit[] materials;
    public Mesh mesh;
    ParseError errorno = ParseError.None;
    public bool error { get { return errorno != ParseError.None; } }

    //加载外接式模型
    public void LoadModel(int model)
    {
        ModelItem Target = DlcMng.GetPluginModel(model);
        if (Target == null)
        {
            Debug.LogError("cannot load model:" + model);
            return;
        }
        string skc = "";
        for (int i = 0; i < Target.resPath.Count; i++)
        {
            if (Target.resPath[i].ToLower().EndsWith(".skc"))
            {
                skc = Target.resPath[i];
                break;
            }
        }
        if (!System.IO.File.Exists(skc))
            errorno = ParseError.Miss;
        string text = System.IO.File.ReadAllText(skc);
        Parse(text);
    }

    public void Load(string file)
    {
        TextAsset assets = Resources.Load<TextAsset>(file);
        string text = null;
        if (assets == null) {
            if (!System.IO.File.Exists(file)) {
                errorno = ParseError.Miss;
                return;
            }
            text = System.IO.File.ReadAllText(file);
        } else
            text = assets.text; 
        Parse(text);
    }

    void Parse(string textSkc)
    {
        //预处理，把\t转换为空格
        textSkc = textSkc.Replace("\t", " ");
        try
        {
            string[] line = textSkc.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < line.Length; i++)
            {
                if (string.IsNullOrEmpty(line[i]))
                    continue;
                if (line[i].StartsWith("#"))
                    continue;
                string[] lineobj = line[i].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (lineobj[0] == "Static" && lineobj[1] == "Skins:")
                {
                    StaticSkins = int.Parse(lineobj[2]);
                    int pos = i + 1;
                    for (int j = 0; j < StaticSkins; j++)
                    {
                        pos = ReadEach(line, pos, j);
                    }
                }
            }
        }
        catch (Exception exp)
        {
            Debug.LogError(exp.StackTrace);
            errorno = ParseError.ParseError;
        }
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    //考虑网格合并，把使用一个材质的子网格，合并到一起
    int ReadEach(string[] line, int start, int idx)
    {
        //int end = start;
        int left = 0;
        int matidx = -1;
        for (int i = start; i < line.Length; i++)
        {
            string[] lineobj = line[i].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            lineobj[0] = lineobj[0].Trim(new char[] { '\t' });
            if (lineobj[0].StartsWith("#"))
                continue;
            else
            if (lineobj[0] == "Static" && lineobj[1] == "Skin" && lineobj.Length == 3)
            {
                Skin = lineobj[2];
            }
            else if (lineobj[0] == "{")
            {
                left++;
            }
            else if (lineobj[0] == "}")
            {
                left--;
                if (left == 0)
                    return i + 1;
            }
            else if (lineobj[0] == "Materials:")
            {
                materials = new MaterialUnit[int.Parse(lineobj[1])];
            }
            else if (lineobj[0] == "Material")
            {
                matidx++;
                materials[matidx] = new MaterialUnit();
                //mat[matidx] = new Material(Shader.Find("Unlit/Texture"));
            }
            else if (lineobj[0] == "Texture")
            {
                string text = lineobj[1];
                int dot = text.LastIndexOf(".");
                if (dot != -1)
                    text = text.Substring(0, dot);
                materials[matidx].Texture = text;
            }
            else if (lineobj[0] == "ColorKey")
            {
                materials[matidx].ColorKey = new Color(float.Parse(lineobj[1]), float.Parse(lineobj[2]), float.Parse(lineobj[3]), float.Parse(lineobj[4]));
            }
            else if (lineobj[0] == "Ambient")
            {
                materials[matidx].Ambient = new Color(float.Parse(lineobj[1]), float.Parse(lineobj[2]), float.Parse(lineobj[3]));
            }
            else if (lineobj[0] == "Diffuse")
            {
                materials[matidx].Diffuse = new Color(float.Parse(lineobj[1]), float.Parse(lineobj[2]), float.Parse(lineobj[3]));
            }
            else if (lineobj[0] == "Specular")
            {
                materials[matidx].ColorKey = new Color(float.Parse(lineobj[1]), float.Parse(lineobj[2]), float.Parse(lineobj[3]));
            }
            else if (lineobj[0] == "Emissive")
            {
                materials[matidx].ColorKey = new Color(float.Parse(lineobj[1]), float.Parse(lineobj[2]), float.Parse(lineobj[3]));
            }
            else if (lineobj[0] == "Opacity")
            {
                materials[matidx].Opacity = float.Parse(lineobj[1]);
            }
            else if (lineobj[0] == "Option")
            {
                materials[matidx].Option = lineobj[1];
            }
            else if (lineobj[0] == "TwoSide")
            {
                materials[matidx].TwoSide = (lineobj[1].ToUpper() == "TRUE");
            }
            else if (lineobj[0] == "Vertices:")
            {
                mesh = new Mesh();
                mesh.name = Skin;
                int count = int.Parse(lineobj[1]);
                List<BoneWeight> boneWeight = new List<BoneWeight>();
                List<Vector3> vec = new List<Vector3>();
                List<Vector2> uv = new List<Vector2>();
                for (int s = i + 1; s < i + 1 + count; s++)
                {
                    string line2 = line[s];
                    string[] subline = line2.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (subline.Length == 1)
                        subline = subline[0].Split(new char[] { '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (subline[0] == "v")
                    {
                        Vector3 v = new Vector3();
                        v.x = float.Parse(subline[1]);
                        v.z = float.Parse(subline[2]);
                        v.y = float.Parse(subline[3]);
                        //右手坐标系，换左手坐标系

                        Vector2 uvv = new Vector2();
                        uvv.x = float.Parse(subline[5]);
                        uvv.y = float.Parse(subline[6]);
                        BoneWeight weight = new BoneWeight();
                        List<BoneWeightEx> boneW = new List<BoneWeightEx>();
                        int boneCtrlNum = int.Parse(subline[8]);
                        for (int z = 9; z < 9 + 2 * boneCtrlNum; z += 2)
                        {
                            int b = (int)float.Parse(subline[z]);
                            float w = float.Parse(subline[z + 1]);
                            //部分骨骼权重太低，剪掉这个骨骼
                            if (w <= 0.005f)
                            {
                                //Debug.LogError("忽略了权重为0的骨骼,");
                                continue;
                            }
                            BoneWeightEx e = new BoneWeightEx();
                            e.BoneIndex = b;
                            e.Weight = w;
                            boneW.Add(e);
                        }
                        //重新按权重设置各自的比例
                        float weightTotal = 0.0f;
                        for (int k = 0; k < boneW.Count; k++)
                            weightTotal += boneW[k].Weight;
                        weight.boneIndex0 = boneW.Count >= 1 ? boneW[0].BoneIndex : 0;
                        weight.weight0 = boneW.Count >= 1 ? boneW[0].Weight / weightTotal : 0;
                        weight.boneIndex1 = boneW.Count >= 2 ? boneW[1].BoneIndex : 0;
                        weight.weight1 = boneW.Count >= 2 ? boneW[1].Weight / weightTotal : 0;
                        weight.boneIndex2 = boneW.Count >= 3 ? boneW[2].BoneIndex : 0;
                        weight.weight2 = boneW.Count >= 3 ? boneW[2].Weight / weightTotal : 0;
                        weight.boneIndex3 = boneW.Count >= 4 ? boneW[3].BoneIndex : 0;
                        weight.weight3 = boneW.Count >= 4 ? boneW[3].Weight / weightTotal : 0;
                        boneWeight.Add(weight);
                        vec.Add(v);
                        uv.Add(uvv);
                    }
                }
                mesh.SetVertices(vec);
                mesh.uv = uv.ToArray();

                mesh.boneWeights = boneWeight.ToArray();
                i += count;
            }
            else if (lineobj[0] == "Triangles:")
            {
                int triNum = int.Parse(lineobj[1]);
                SortedDictionary<int, SortedDictionary<int, int[]>> indic = new SortedDictionary<int, SortedDictionary<int, int[]>>();
                for (int s = i + 1; s < i + 1 + triNum; s++)
                {
                    string line2 = line[s];
                    int o = 0;
                    int p = 0;
                    int q = 0;
                    int matIdx2 = 0;
                    string[] subline = line2.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (subline.Length == 1)
                        subline = subline[0].Split(new char[] { '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (subline[0] == "f")
                    {
                        matIdx2 = int.Parse(subline[2]);
                        o = int.Parse(subline[3]);
                        p = int.Parse(subline[4]);
                        q = int.Parse(subline[5]);
                        int[] ind = new int[3] { o, q, p };//反转yz
                        if (indic.ContainsKey(matIdx2))
                            indic[matIdx2].Add(s - i - 1, ind);
                        else
                        {
                            SortedDictionary<int, int[]> va = new SortedDictionary<int, int[]>();
                            va.Add(s - i - 1, ind);
                            indic.Add(matIdx2, va);
                        }
                    }
                }
                i += triNum;
                mesh.subMeshCount = materials.Length;
                for (int mm = 0; mm < materials.Length; mm++)
                {
                    if (indic.ContainsKey(mm))
                    {
                        int cnt = indic[mm].Count;
                        int[] va = new int[cnt * 3];
                        int next = 0;
                        foreach (var each in indic[mm])
                        {
                            va[next++] = each.Value[0];
                            va[next++] = each.Value[1];
                            va[next++] = each.Value[2];
                        }

                        mesh.SetIndices(va, MeshTopology.Triangles, mm);
                        mesh.SetTriangles(va, mm);
                    }
                }

            }
            else
            {
                Debug.LogError(lineobj[0]);
            }
        }
        return line.Length - 1;
    }

    public static Texture2D GetTexrtureFromPlugin(int roleIdx, string imgPath)
    {
        ModelItem Target = null;
        Target = DlcMng.GetPluginModel(roleIdx);
        for (int i = 0; i < Target.resPath.Count; i++)
        {
            int idx = Target.resPath[i].LastIndexOf("/");
            string name = Target.resPath[i].Substring(idx + 1);
            idx = name.LastIndexOf(".");
            name = name.Substring(0, idx);
            if (string.Equals(name, imgPath, StringComparison.OrdinalIgnoreCase))
                return GetTexrture2DFromPath(Target.resPath[i]);
        }
        return null;
    }

    public static Texture2D GetTexrture2DFromPath(string imgPath)
    {
        //读取文件
        //WWW www = new WWW("file://" + imgPath);
        //return www.texture;
        FileStream fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
        int byteLength = (int)fs.Length;
        byte[] imgBytes = new byte[byteLength];
        fs.Read(imgBytes, 0, byteLength);
        fs.Close();
        fs.Dispose();
        //转化为Texture2D
        Texture2D t2d = new Texture2D(0, 0, TextureFormat.ARGB32, false);

        t2d.LoadImage(imgBytes);
        t2d.Apply();
        return t2d;
    }

    //0-19内的材质是3个，20-27的是单个
    public Material[] Material(int roleIdx, EUnitCamp camp)
    {
        if (roleIdx >= CombatData.Ins.MaxModel)
        {
            Material[] ret = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                ret[i] = new Material(ShaderMng.Find("AlphaTexture"));
                ret[i].SetTexture("_MainTex", SkcFile.GetTexrtureFromPlugin(roleIdx, materials[i].Texture));            }
            return ret;
        }
        else
        {
            if (GamePool.Instance != null && GamePool.Instance.SkcMng != null)
                return GamePool.Instance.SkcMng.GetPlayerMat(roleIdx, camp);
            return null;
        }
    }
}

