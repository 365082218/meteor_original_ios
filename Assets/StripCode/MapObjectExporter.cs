using Excel2Json;
using Idevgame.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
//using UnityEditor;
using UnityEngine;

public class MapObjectExporter : MonoBehaviour {

    const string SavePath = "Assets/MeteorObj/";//+sn/xxx.obj xxx.mat
    const string ItemSavePath = "Assets/MeteorObj/Items/";
    public bool saveSceneModel;
    Dictionary<string, LevelData> levAll = new Dictionary<string, LevelData>();
    List<string> ModelInScene = new List<string>();
    //List<string> ModelInModel = new List<string>();
    private void Awake()
    {
        return;//这段代码开开会生成所有地图的模型，并保存到项目（SavePath 、ItemSavePath ）内
        //Level[] allLevel = LevelMng.Instance.GetAllItem();
        //for (int i = 0; i < allLevel.Length; i++)
        //{
        //    if (!levAll.ContainsKey(allLevel[i].sceneItems))
        //    {
        //        levAll.Add(allLevel[i].sceneItems, allLevel[i]);
        //        LoadSceneAndExportModel(allLevel[i].sceneItems);//导出场景元素.
        //    }
        //}

        //for (int i = 0; i < ModelInScene.Count; i++)
        //{
        //    LoadItemAndExportModel(null, ModelInScene[i]);
        //}
    }

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //由于Obj格式子对象根据对象名排序，又由于子对象顺序与动画有关系
    //所以子物件把名称改为顺序，再导出Obj
    public void LoadItemAndExportModel(Transform parent, string despath)
    {
        string fullSavePath = ItemSavePath + despath + "/";
        if (!System.IO.Directory.Exists(fullSavePath))
            System.IO.Directory.CreateDirectory(fullSavePath);
        DesFile des = DesLoader.Ins.Load(despath);
        GMBFile gmb = GMBLoader.Ins.Load(despath);
        if (des == null || gmb == null)
            return;
        string shaders = "Custom/MeteorVertexL";
        Material[] mat = new Material[gmb.TexturesCount];
        for (int x = 0; x < gmb.TexturesCount; x++)
        {
            string materialName = string.Format("{0:D2}_{1:D2}", despath, x);
            mat[x] = new Material(Resources.Load<Shader>("Mobile-Diffuse"));
            string tex = gmb.TexturesNames[x];
            int del = tex.LastIndexOf('.');
            if (del != -1)
                tex = tex.Substring(0, del);
            Texture texture = Resources.Load<Texture>(tex);
            //这里的贴图可能是一个ifl文件，这种先不考虑，手动修改
            if (texture == null)
                Debug.LogError("texture miss:" + tex);
            mat[x].SetTexture("_MainTex", texture);
            mat[x].name = materialName;
            //UnityEditor.AssetDatabase.CreateAsset(mat[x], fullSavePath + mat[x].name + ".mat");
            //UnityEditor.AssetDatabase.Refresh();
        }

        bool saveMesh = false;
        if (parent == null)
        {
            saveMesh = true;
            GameObject root = new GameObject(despath);
            root.Identity(transform);
            parent = root.transform;
        }
        while (true)
        {
            for (int i = 0; i < Mathf.Min(des.SceneItems.Count, gmb.SceneObjectsCount); i++)
            {
                GameObject objMesh = new GameObject();
                objMesh.name = i.ToString();
                objMesh.transform.localRotation = Quaternion.identity;
                objMesh.transform.localPosition = Vector3.zero;
                objMesh.transform.localScale = Vector3.one;
                int j = i;
                Mesh w = new Mesh();
                //前者子网格编号，后者 索引缓冲区
                Dictionary<int, List<int>> tr = new Dictionary<int, List<int>>();
                List<Vector3> ve = new List<Vector3>();
                List<Vector2> uv = new List<Vector2>();
                List<Vector3> nor = new List<Vector3>();
                List<Color> col = new List<Color>();
                for (int k = 0; k < gmb.mesh[j].faces.Count; k++)
                {
                    int key = gmb.mesh[j].faces[k].material;
                    if (tr.ContainsKey(key))
                    {
                        tr[key].Add(gmb.mesh[j].faces[k].triangle[0]);
                        tr[key].Add(gmb.mesh[j].faces[k].triangle[1]);
                        tr[key].Add(gmb.mesh[j].faces[k].triangle[2]);
                    }
                    else
                    {
                        tr.Add(key, new List<int>());
                        tr[key].Add(gmb.mesh[j].faces[k].triangle[0]);
                        tr[key].Add(gmb.mesh[j].faces[k].triangle[1]);
                        tr[key].Add(gmb.mesh[j].faces[k].triangle[2]);
                    }

                }
                for (int k = 0; k < gmb.mesh[j].vertices.Count; k++)
                {
                    Vector3 vecLocalToWorld = des.SceneItems[i].pos;
                    Quaternion quatToWorld = des.SceneItems[i].quat;
                    Vector3 vecWorldToLocal = new Vector3(-vecLocalToWorld.x, -vecLocalToWorld.y, -vecLocalToWorld.z);
                    Vector3 vec = gmb.mesh[j].vertices[k].pos + vecWorldToLocal;
                    vec = Quaternion.Inverse(quatToWorld) * vec;
                    ve.Add(vec);
                    uv.Add(gmb.mesh[j].vertices[k].uv);
                    col.Add(gmb.mesh[j].vertices[k].color);
                    nor.Add(gmb.mesh[j].vertices[k].normal);
                }
                w.SetVertices(ve);
                w.uv = uv.ToArray();
                w.subMeshCount = tr.Count;
                int ss = 0;
                //查看这个网格使用了哪些材质，然后把这几个材质取出来
                List<Material> targetMat = new List<Material>();
                foreach (var each in tr)
                {
                    w.SetIndices(each.Value.ToArray(), MeshTopology.Triangles, ss++);
                    if (each.Key >= 0 && each.Key < gmb.shader.Length)
                    {
                        int materialIndex = gmb.shader[each.Key].TextureArg0;
                        if (materialIndex >= 0 && materialIndex < mat.Length)
                            targetMat.Add(mat[materialIndex]);
                        else
                        {
                            //即使没有贴图，也存在材质
                            string materialName = string.Format("{0:D2}_{1:D2}", despath, materialIndex);
                            Material defaults = Resources.Load<Material>(materialName);
                            if (defaults == null)
                            {
                                defaults = new Material(Shader.Find("Custom/MeteorVertexL"));
                                defaults.name = materialName;
                                //UnityEditor.AssetDatabase.CreateAsset(defaults, fullSavePath + "/Resources/" + defaults.name + ".mat");
                                //UnityEditor.AssetDatabase.Refresh();
                            }
                            targetMat.Add(defaults);
                        }
                    }
                }
                MeshRenderer mr = objMesh.AddComponent<MeshRenderer>();
                MeshFilter mf = objMesh.AddComponent<MeshFilter>();
                mf.sharedMesh = w;
                mf.sharedMesh.colors = col.ToArray();
                mf.sharedMesh.normals = nor.ToArray();
                mf.sharedMesh.RecalculateBounds();
                mf.sharedMesh.RecalculateNormals();
                mr.materials = targetMat.ToArray();
                string vis = "";
                if (des.SceneItems[i].ContainsKey("visible", out vis))
                {
                    if (vis == "0")
                    {
                        mr.enabled = false;
                        mr.gameObject.AddComponent<MeshCollider>();
                    }
                }
                string block = "";
                if (des.SceneItems[i].ContainsKey("blockplayer", out block))
                {
                    if (block == "no")
                    {
                    }
                    else
                        mr.gameObject.AddComponent<MeshCollider>();
                }
                else
                {
                    mr.gameObject.AddComponent<MeshCollider>();
                }

                string v = "";
                if (des.SceneItems[i].ContainsKey("model", out v))
                {
                    if (!string.IsNullOrEmpty(v))
                        LoadItemAndExportModel(objMesh.transform, v);  
                }
                objMesh.transform.SetParent(parent);
                objMesh.transform.localRotation = des.SceneItems[i].quat;
                objMesh.transform.localPosition = des.SceneItems[i].pos;
                //MeshFilter mfSave = objMesh.GetComponent<MeshFilter>();
                //if (mfSave != null)
                //{
                    //MeshToFile(mfSave, fullSavePath, des.SceneItems[i].name);
                    //UnityEditor.AssetDatabase.CreateAsset(objMesh.GetComponent<MeshFilter>().sharedMesh, fullSavePath + "/" + des.SceneItems[i].name + ".asset");
                    //UnityEditor.AssetDatabase.Refresh();
                //}
                //yield return 0;
            }

            if (saveMesh)
            {
                MeshFilter[] mfs = parent.GetComponentsInChildren<MeshFilter>();
                if (mfs.Length != 0)
                    MeshesToFile(mfs, fullSavePath, despath, null);
            }
            
            return;
        }
    }

    public void LoadSceneAndExportModel(string despath, string overwritesavepath = "")
    {
        string fullSavePath = SavePath + despath + "/";
        if (!string.IsNullOrEmpty(overwritesavepath))
            fullSavePath = overwritesavepath + despath;
        DesFile des = DesLoader.Ins.Load(despath);
        GMBFile gmb = GMBLoader.Ins.Load(despath);
        if (des == null || gmb == null)
            return;

        if (!System.IO.Directory.Exists(SavePath))
            System.IO.Directory.CreateDirectory(SavePath);

        if (!System.IO.Directory.Exists(fullSavePath))
            System.IO.Directory.CreateDirectory(fullSavePath);

        string shaders = "Custom/MeteorVertexL";
        Material[] mat = new Material[gmb.TexturesCount];
        if (saveSceneModel)
        {
            for (int x = 0; x < gmb.TexturesCount; x++)
            {
                string materialName = string.Format("{0:D2}_{1:D2}", despath, x);
                //mat[x] = Resources.Load<Material>(materialName);
                mat[x] = new Material(Resources.Load<Shader>("Mobile-Diffuse"));
                string tex = gmb.TexturesNames[x];
                int del = tex.LastIndexOf('.');
                if (del != -1)
                    tex = tex.Substring(0, del);
                Texture texture = Resources.Load<Texture>(tex);
                //这里的贴图可能是一个ifl文件，这种先不考虑，手动修改
                if (texture == null)
                    Debug.LogError("texture miss:" + tex);
                mat[x].SetTexture("_MainTex", texture);
                mat[x].name = materialName;
                //UnityEditor.AssetDatabase.CreateAsset(mat[x], fullSavePath + mat[x].name + ".mat");
                //UnityEditor.AssetDatabase.Refresh();
            }
        }
        while (true)
        {
            if (saveSceneModel)
            {
                for (int i = 0; i < Mathf.Min(des.SceneItems.Count, gmb.SceneObjectsCount); i++)
                {
                    GameObject objMesh = new GameObject();
                    objMesh.name = des.SceneItems[i].name;
                    objMesh.transform.localRotation = Quaternion.identity;
                    objMesh.transform.localPosition = Vector3.zero;
                    objMesh.transform.localScale = Vector3.one;
                    for (int j = 0; j < gmb.SceneObjectsCount; j++)
                    {
                        if (gmb.mesh[j].name == objMesh.name && j == i)
                        {
                            Mesh w = new Mesh();
                            //前者子网格编号，后者 索引缓冲区
                            Dictionary<int, List<int>> tr = new Dictionary<int, List<int>>();
                            List<Vector3> ve = new List<Vector3>();
                            List<Vector2> uv = new List<Vector2>();
                            List<Vector3> nor = new List<Vector3>();
                            List<Color> col = new List<Color>();
                            for (int k = 0; k < gmb.mesh[j].faces.Count; k++)
                            {
                                int key = gmb.mesh[j].faces[k].material;
                                if (tr.ContainsKey(key))
                                {
                                    tr[key].Add(gmb.mesh[j].faces[k].triangle[0]);
                                    tr[key].Add(gmb.mesh[j].faces[k].triangle[1]);
                                    tr[key].Add(gmb.mesh[j].faces[k].triangle[2]);
                                }
                                else
                                {
                                    tr.Add(key, new List<int>());
                                    tr[key].Add(gmb.mesh[j].faces[k].triangle[0]);
                                    tr[key].Add(gmb.mesh[j].faces[k].triangle[1]);
                                    tr[key].Add(gmb.mesh[j].faces[k].triangle[2]);
                                }

                            }
                            for (int k = 0; k < gmb.mesh[j].vertices.Count; k++)
                            {
                                Vector3 vecLocalToWorld = des.SceneItems[i].pos;
                                Quaternion quatToWorld = des.SceneItems[i].quat;
                                Vector3 vecWorldToLocal = new Vector3(-vecLocalToWorld.x, -vecLocalToWorld.y, -vecLocalToWorld.z);
                                Vector3 vec = gmb.mesh[j].vertices[k].pos + vecWorldToLocal;
                                vec = Quaternion.Inverse(quatToWorld) * vec;
                                ve.Add(vec);
                                uv.Add(gmb.mesh[j].vertices[k].uv);
                                col.Add(gmb.mesh[j].vertices[k].color);
                                nor.Add(gmb.mesh[j].vertices[k].normal);
                            }
                            w.SetVertices(ve);
                            w.uv = uv.ToArray();
                            w.subMeshCount = tr.Count;
                            int ss = 0;
                            //查看这个网格使用了哪些材质，然后把这几个材质取出来
                            List<Material> targetMat = new List<Material>();
                            foreach (var each in tr)
                            {
                                w.SetIndices(each.Value.ToArray(), MeshTopology.Triangles, ss++);
                                if (each.Key >= 0 && each.Key < gmb.shader.Length)
                                {
                                    int materialIndex = gmb.shader[each.Key].TextureArg0;
                                    if (materialIndex >= 0 && materialIndex < mat.Length)
                                        targetMat.Add(mat[materialIndex]);
                                    else
                                    {
                                        //即使没有贴图，也存在材质
                                        string materialName = string.Format("{0:D2}_{1:D2}", despath, materialIndex);
                                        Material defaults = Resources.Load<Material>(materialName);
                                        if (defaults == null)
                                        {
                                            defaults = new Material(Shader.Find("Custom/MeteorVertexL"));
                                            defaults.name = materialName;
                                            //UnityEditor.AssetDatabase.CreateAsset(defaults, fullSavePath + "/Resources/" + defaults.name + ".mat");
                                            //UnityEditor.AssetDatabase.Refresh();
                                        }
                                        targetMat.Add(defaults);
                                    }
                                }
                            }
                            MeshRenderer mr = objMesh.AddComponent<MeshRenderer>();
                            MeshFilter mf = objMesh.AddComponent<MeshFilter>();
                            mf.sharedMesh = w;
                            mf.sharedMesh.colors = col.ToArray();
                            mf.sharedMesh.normals = nor.ToArray();
                            mf.sharedMesh.RecalculateBounds();
                            mf.sharedMesh.RecalculateNormals();
                            mr.materials = targetMat.ToArray();
                            string vis = "";
                            if (des.SceneItems[i].ContainsKey("visible", out vis))
                            {
                                if (vis == "0")
                                {
                                    mr.enabled = false;
                                    mr.gameObject.AddComponent<MeshCollider>();
                                }
                            }
                            string block = "";
                            if (des.SceneItems[i].ContainsKey("blockplayer", out block))
                            {
                                if (block == "no")
                                {
                                }
                                else
                                    mr.gameObject.AddComponent<MeshCollider>();
                            }
                            else
                            {
                                mr.gameObject.AddComponent<MeshCollider>();
                            }
                            break;
                        }
                    }

                    objMesh.transform.SetParent(transform);
                    objMesh.transform.localRotation = des.SceneItems[i].quat;
                    objMesh.transform.localPosition = des.SceneItems[i].pos;
                    MeshFilter mfSave = objMesh.GetComponent<MeshFilter>();
                    if (mfSave != null)
                    {
                        MeshToFile(mfSave, fullSavePath, des.SceneItems[i].name);
                        //UnityEditor.AssetDatabase.CreateAsset(objMesh.GetComponent<MeshFilter>().sharedMesh, fullSavePath + "/" + des.SceneItems[i].name + ".asset");
                        //UnityEditor.AssetDatabase.Refresh();
                    }
                    //yield return 0;
                }
            }

            if (string.IsNullOrEmpty(overwritesavepath))
            {
                for (int i = des.ObjectCount; i < des.SceneItems.Count; i++)
                {
                    string v = "";
                    if (des.SceneItems[i].ContainsKey("model", out v))
                    {
                        if (!string.IsNullOrEmpty(v) && !ModelInScene.Contains(v))
                            ModelInScene.Add(v);
                    }
                }
            }
            return;
            //yield break;
        }
    }

    public static void MeshToFile(MeshFilter mf, string folder, string filename)
    {
        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".obj"))
        {
            sw.Write("mtllib ./" + filename + ".mtl\n");
            sw.Write(MeshToString(mf, materialList));
        }

        MaterialsToFile(materialList, folder, filename);
    }

    private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string folder, string filename)
    {
        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".mtl"))
        {
            foreach (KeyValuePair<string, ObjMaterial> kvp in materialList)
            {
                sw.Write("\n");
                sw.Write("newmtl {0}\n", kvp.Key);
                sw.Write("Ka  0.6 0.6 0.6\n");
                sw.Write("Kd  0.6 0.6 0.6\n");
                sw.Write("Ks  0.9 0.9 0.9\n");
                sw.Write("d  1.0\n");
                sw.Write("Ns  0.0\n");
                sw.Write("illum 2\n");

                if (kvp.Value.textureName != null)
                {
                    string destinationFile = kvp.Value.textureName;
                    int stripIndex = destinationFile.LastIndexOf('/');//FIXME: Should be Path.PathSeparator;

                    if (stripIndex >= 0)
                        destinationFile = destinationFile.Substring(stripIndex + 1).Trim();


                    string relativeFile = destinationFile;

                    //destinationFile = folder + "/" + destinationFile;

                    //Debug.Log("Copying texture from " + kvp.Value.textureName + " to " + destinationFile);

                    //try
                    //{
                    //    //Copy the source file
                    //    File.Copy(kvp.Value.textureName, destinationFile);
                    //}
                    //catch
                    //{

                    //}


                    sw.Write("map_Kd {0}", relativeFile);
                }

                sw.Write("\n\n\n");
            }
        }
    }

    private static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList, WorldtoLocalParam param = null)
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 lv in m.vertices)
        {
            Vector3 vec = lv;
            //通过参数，可以还原原流星里的任意的 模型。
            //if (param != null)
            //{
            //    vec = vec + param.vec;
            //    vec = param.rotation * vec;
            //}
            //Vector3 wv = mf.transform.TransformPoint(lv);
            Vector3 wv = vec;
            //This is sort of ugly - inverting x-component since we're in
            //a different coordinate system than "everyone" is "used to".
            sb.Append(string.Format("v {0} {1} {2}\n", -wv.x, wv.y, wv.z));
        }
        sb.Append("\n");

        foreach (Vector3 lv in m.normals)
        {
            Vector3 wv = mf.transform.TransformDirection(lv);
            //Vector3 wv = lv;
            sb.Append(string.Format("vn {0} {1} {2}\n", -wv.x, wv.y, wv.z));
        }
        sb.Append("\n");

        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }

        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            //See if this material is already in the materiallist.
            try
            {
                if (material >= mats.Length)
                    continue;
                if (mats[material] == null)
                    continue;
                ObjMaterial objMaterial = new ObjMaterial();
                if (mats[material].mainTexture)
                {
                    //objMaterial.textureName = AssetDatabase.GetAssetPath(mats[material].mainTexture);
                }
                else
                    objMaterial.textureName = null;
                string[] s = null;
                string ss = null;
                if (!string.IsNullOrEmpty(objMaterial.textureName))
                {
                    s = objMaterial.textureName.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (s != null && s.Length != 0)
                        ss = s[s.Length - 1];
                }
                objMaterial.name = mats[material].name + "_" + ss;
                if (!materialList.ContainsKey(objMaterial.name))
                    materialList.Add(objMaterial.name, objMaterial);
                sb.Append("usemtl ").Append(objMaterial.name).Append("\n");
                sb.Append("usemap ").Append(objMaterial.name).Append("\n");
            }
            catch (ArgumentException)
            {
                //Already in the dictionary
            }


            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                //Because we inverted the x-component, we also needed to alter the triangle winding.
                sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                    triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
            }
        }

        vertexOffset += m.vertices.Length;
        normalOffset += m.normals.Length;
        uvOffset += m.uv.Length;

        return sb.ToString();
    }

    private static Dictionary<string, ObjMaterial> PrepareFileWrite()
    {
        Clear();

        return new Dictionary<string, ObjMaterial>();
    }

    private static int vertexOffset = 0;
    private static int normalOffset = 0;
    private static int uvOffset = 0;
    private static void Clear()
    {
        vertexOffset = 0;
        normalOffset = 0;
        uvOffset = 0;
    }

    private static void MeshesToFile(MeshFilter[] mf, string folder, string filename, WorldtoLocalParam param = null)
    {
        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".obj"))
        {
            sw.Write("mtllib ./" + filename + ".mtl\n");

            for (int i = mf.Length - 1; i >= 0; i--)
            {
                sw.Write(MeshToString(mf[i], materialList, param));
            }
        }

        MaterialsToFile(materialList, folder, filename);
    }
}

internal struct ObjMaterial
{
    public string name;
    public string textureName;
}

internal class WorldtoLocalParam
{
    public Vector3 vec;
    public Quaternion rotation;
}