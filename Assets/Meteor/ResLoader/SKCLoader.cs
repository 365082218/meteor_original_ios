using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SkcLoader : Singleton<SkcLoader> {
    static Dictionary<string, SkcFile> SkcFile = new Dictionary<string, global::SkcFile>();
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

    public SkcFile Load(int characterIdx)
    {
        string BoneCnt = "";
        //if (Startup.ins != null && GameData.gameStatus != null)
        //{
        //    switch (GameData.gameStatus.Quality)
        //    {
        //        case 0:
        //            BoneCnt = ""; break;
        //        //case 1:
        //        //    BoneCnt = "_800"; break;
        //        //case 2:
        //        //    BoneCnt = "_300"; break;
                
        //    }
        //}
        return Load("p" + characterIdx + BoneCnt + ".skc");
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

public class SkcFile
{
    int StaticSkins = 0;
    int DynmaicSkins = 0;
    public string Skin;
    public MaterialUnit[] materials;
    public Mesh mesh;
    ParseError errorno = ParseError.None;
    public bool error { get { return errorno != ParseError.None; } }
    public void Load(string file)
    {
        TextAsset assets = Resources.Load<TextAsset>(file);
        if (assets == null)
        {
            errorno = ParseError.Miss;
            return;
        }

        try
        {
            string[] line = assets.text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
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

    int ReadEach(string[] line, int start, int idx)
    {
        int end = start;
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
                        int boneCtrlNum = int.Parse(subline[8]);
                        switch (boneCtrlNum)
                        {
                            case 1:
                                weight.boneIndex0 = (int)float.Parse(subline[9]);
                                weight.weight0 = float.Parse(subline[10]);
                                break;
                            case 2:
                                weight.boneIndex0 = (int)float.Parse(subline[9]);
                                weight.weight0 = float.Parse(subline[10]);
                                weight.boneIndex1 = (int)float.Parse(subline[11]);
                                weight.weight1 = float.Parse(subline[12]);
                                break;
                            case 3:
                                weight.boneIndex0 = (int)float.Parse(subline[9]);
                                weight.weight0 = float.Parse(subline[10]);
                                weight.boneIndex1 = (int)float.Parse(subline[11]);
                                weight.weight1 = float.Parse(subline[12]);
                                weight.boneIndex2 = (int)float.Parse(subline[13]);
                                weight.weight2 = float.Parse(subline[14]);
                                break;
                            case 4:
                                weight.boneIndex0 = (int)float.Parse(subline[9]);
                                weight.weight0 = float.Parse(subline[10]);
                                weight.boneIndex1 = (int)float.Parse(subline[11]);
                                weight.weight1 = float.Parse(subline[12]);
                                weight.boneIndex2 = (int)float.Parse(subline[13]);
                                weight.weight2 = float.Parse(subline[14]);
                                weight.boneIndex3 = (int)float.Parse(subline[15]);
                                weight.weight3 = float.Parse(subline[16]);
                                break;
                        }
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
                Dictionary<int, Dictionary<int, int[]>> indic = new Dictionary<int, Dictionary<int, int[]>>();
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
                            Dictionary<int, int[]> va = new Dictionary<int, int[]>();
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

    //0-19内的材质是3个，20-27的是单个
    public Material[] Material(int roleIdx, EUnitCamp camp)
    {
        //return new UnityEngine.Material[0];
        //if (SkcMatMng.Instance != null)
        //    return SkcMatMng.Instance.GetPlayerMat(roleIdx, camp);
        //使用预先设置好的材质球，降低DC和Batch
        Material[] ret = new Material[materials.Length];
        string strTexture = "";
        string strIndex = "";
        //if (camp == EUnitCamp.EUC_KILLALL)
        //    strIndex = "01";
        //if (camp == EUnitCamp.EUC_FRIEND)
            strIndex = "01";//非联机模式,只有1皮肤
        //else
            //strIndex = "01";
        //else if (camp == EUnitCamp.EUC_ENEMY)
        //    strIndex = "03";

        for (int i = 0; i < materials.Length; i++)
        {
            //if (materials[i].TwoSide)
            //    ret[i] = new Material(Shader.Find("Shader Forge/DoubleSideTexture"));
            //else
            ret[i] = new Material(ShaderUtil.Find("AlphaTexture"));
            //根据阵营决定贴图序号
            if (roleIdx > 19)
                ret[i].SetTexture("_MainTex", Resources.Load<Texture>(materials[i].Texture));
            else
            {
                string[] str = materials[i].Texture.Split('b');
                if (str.Length == 2)
                    strTexture = str[0] + "b" + strIndex;
                else
                    strTexture = materials[i].Texture;
                ret[i].SetTexture("_MainTex", Resources.Load<Texture>(strTexture));
            }
            ret[i].SetColor("_Color", materials[i].Diffuse);
        }
        return ret;
    }
}

