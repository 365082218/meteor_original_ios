using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;

public class WsGlobal {
    public static List<GameObject> DebugLine = new List<GameObject>();
	public static List<int> removeNodeIndex = new List<int>();

    public static void RemoveLine(GameObject obj)
    {
        if (DebugLine.Contains(obj))
        {
            GameObject.Destroy(obj);
            DebugLine.Remove(obj);
        }
    }


#if !STRIP_DBG_SETTING
    public static GameObject AddDebugLine(Vector3 pos, Vector3 pos2, Color co, string strTag = "", float fLastTime = float.MaxValue, bool autoHide = false)
    {
        GameObject obj = GameObject.Instantiate(Resources.Load("DebugLineRender")) as GameObject;
        obj.transform.position = (pos + pos2) / 2;
        LineRenderer line = obj.GetComponent<LineRenderer>();
        line.startWidth = line.endWidth = 0.1f;
        line.startColor = line.endColor = co;
        line.numPositions = 2;
        line.SetPosition(0, pos);
        line.SetPosition(1, pos2);
        UITalkBubble text = obj.GetComponent<UITalkBubble>();
        if (text == null)
        {
            text = obj.AddComponent<UITalkBubble>();
            text.AutoHide = autoHide;
        }
        text.strText = strTag;
        text.showtime = fLastTime;
        text.SetDebugText();
        int nCount = DebugLine.Count;
        if (removeNodeIndex.Count != 0)
        {
            nCount = removeNodeIndex[0];
            removeNodeIndex.RemoveAt(0);
        }
        DebugLine.Add(obj);
        return obj;
    }
#endif

    //动态递归改变怪物，英雄所有节点的层
    public static void SetObjectLayer(GameObject objSelect, int layermask)
	{
		objSelect.layer = layermask;
		for (int i = 0; i < objSelect.transform.childCount; i++)
		{
			SetObjectLayer(objSelect.transform.GetChild(i).gameObject, layermask);
		}
	}

    //适用于非武器类型的任意模型的显示，包括地图都可以用此方法读，且支持IFL材质，创建虚拟体,只匹配序号，不匹配名称
    public static void ShowMeteorObject(string file, Transform parent, bool gen = false)
    {
        if (file.EndsWith(".des"))
            file = file.Substring(0, file.Length - 4);
        DesFile fIns = Main.Ins.DesLoader.Load(file);
        GMBFile gmb = Main.Ins.GMBLoader.Load(file);
        GMCFile fModel = Main.Ins.GMCLoader.Load(file);
        //保存材质球
        if (Application.isEditor)
        {
            if (!System.IO.Directory.Exists("Assets/Materials/Weapons/resources/"))
                System.IO.Directory.CreateDirectory("Assets/Materials/Weapons/resources/");
        }

        //看des文件是否为空
        //优先GMB
        if (gmb != null)
        {
            //还未处理IFL序列图动态加载。
            Material[] mat = new Material[gmb.TexturesCount];
            for (int i = 0; i < mat.Length; i++)
                mat[i] = null;
            Dictionary<int, string> iflMat = new Dictionary<int, string>();//IFL材质特殊处理,IFL材质均为mobile/particle/additive
            for (int x = 0; x < gmb.TexturesCount; x++)
            {
                string tex = gmb.TexturesNames[x];
                if (tex.ToLower().EndsWith(".ifl"))
                {
                    if (!iflMat.ContainsKey(x))
                        iflMat.Add(x, tex);
                    continue;
                }

                string iden = string.Format("{0}_{1:D2}", file, x);
                mat[x] = Resources.Load<Material>(iden);
                if (mat[x] == null)
                {
                    mat[x] = new Material(ShaderMng.Find("AlphaTexture"));
                    int del = tex.LastIndexOf('.');
                    if (del != -1)
                        tex = tex.Substring(0, del);
                    Texture texture = Resources.Load<Texture>(tex);
                    if (texture == null)
                        Debug.LogError("texture miss on load gmb:" + file + " texture name:" + tex);
                    mat[x].SetTexture("_MainTex", texture);
                    mat[x].name = iden;
                    if (gen)
                    {
                        //UnityEditor.AssetDatabase.CreateAsset(mat[x], "Assets/Materials/Weapons/resources/" + mat[x].name + ".mat");
                        //UnityEditor.AssetDatabase.Refresh();
                    }
                }
            }

            for (int i = 0; i < fIns.SceneItems.Count; i++)
            {
                GameObject objMesh = new GameObject();
                bool addIflComponent = false;
                int iflParam = -1;
                int iflMatIndex = 0;
                objMesh.name = fIns.SceneItems[i].name;
                objMesh.transform.localRotation = Quaternion.identity;
                objMesh.transform.localPosition = Vector3.zero;
                objMesh.transform.localScale = Vector3.one;
                objMesh.layer = parent.gameObject.layer;
                //bool realObject = false;//是不是正常物体，虚拟体无需设置材质球之类
                if (i < gmb.SceneObjectsCount)
                {
                //for (int j = 0; j < gmb.SceneObjectsCount; j++)
                //{
                //    if (gmb.mesh[j].name == objMesh.name)
                //    {
                        //realObject = true;
                        Mesh w = new Mesh();
                        //前者子网格编号，后者 索引缓冲区
                        Dictionary<int, List<int>> tr = new Dictionary<int, List<int>>();
                        List<Vector3> ve = new List<Vector3>();
                        List<Vector2> uv = new List<Vector2>();
                        List<Vector3> nor = new List<Vector3>();
                        List<Color> col = new List<Color>();
                        for (int k = 0; k < gmb.mesh[i].faces.Count; k++)
                        {
                            int key = gmb.mesh[i].faces[k].material;
                            if (tr.ContainsKey(key))
                            {
                                tr[key].Add(gmb.mesh[i].faces[k].triangle[0]);
                                tr[key].Add(gmb.mesh[i].faces[k].triangle[1]);
                                tr[key].Add(gmb.mesh[i].faces[k].triangle[2]);
                            }
                            else
                            {
                                tr.Add(key, new List<int>());
                                tr[key].Add(gmb.mesh[i].faces[k].triangle[0]);
                                tr[key].Add(gmb.mesh[i].faces[k].triangle[1]);
                                tr[key].Add(gmb.mesh[i].faces[k].triangle[2]);
                            }

                        }
                        for (int k = 0; k < gmb.mesh[i].vertices.Count; k++)
                        {
                            //ve.Add(gmb.mesh[i].vertices[k].pos);
                            Vector3 vec = gmb.mesh[i].vertices[k].pos - (Vector3)fIns.SceneItems[i].pos;
                            vec = Quaternion.Inverse(fIns.SceneItems[i].quat) * vec;
                            //ve.Add(fModel.mesh[i].vertices[k].pos);
                            ve.Add(vec);
                            uv.Add(gmb.mesh[i].vertices[k].uv);
                            col.Add(gmb.mesh[i].vertices[k].color);
                            nor.Add(gmb.mesh[i].vertices[k].normal);
                        }
                        w.SetVertices(ve);
                        w.uv = uv.ToArray();
                        w.subMeshCount = tr.Count;
                        int ss = 0;
                        List<Material> targetMat = new List<Material>();
                        foreach (var each in tr)
                        {
                            w.SetIndices(each.Value.ToArray(), MeshTopology.Triangles, ss++);
                            if (each.Key >= 0 && each.Key < gmb.shader.Length)
                            {
                                int materialIndex = gmb.shader[each.Key].TextureArg0;
                                if (materialIndex >= 0 && materialIndex < mat.Length)
                                {
                                    if (mat[materialIndex] == null)
                                    {
                                        targetMat.Add(new Material(Shader.Find("Unlit/Transparent")));
                                        addIflComponent = true;
                                        iflParam = materialIndex;
                                        iflMatIndex = targetMat.Count - 1;
                                    }
                                    else
                                    {
                                        targetMat.Add(mat[materialIndex]);
                                    }
                                }
                                else
                                {
                                    Material defaults = Resources.Load<Material>(string.Format("{0}_{1:D2}", file, materialIndex));
                                    if (defaults == null)
                                        defaults = new Material(Shader.Find("Unlit/Texture"));
                                    defaults.name = string.Format("{0}_{1:D2}", file, materialIndex);
                                    if (gen)
                                    {
                                        //UnityEditor.AssetDatabase.CreateAsset(defaults, "Assets/Materials/Weapons/resources/" + defaults.name + ".mat");
                                        //UnityEditor.AssetDatabase.Refresh();
                                    }
                                    targetMat.Add(defaults);
                                }
                            }
                        }
                        MeshRenderer mr = objMesh.AddComponent<MeshRenderer>();
                        MeshFilter mf = objMesh.AddComponent<MeshFilter>();
                        mf.mesh = w;
                        mf.mesh.colors = col.ToArray();
                        mf.mesh.normals = nor.ToArray();
                        mf.mesh.RecalculateBounds();
                        mf.mesh.RecalculateNormals();

                        mr.materials = targetMat.ToArray();
                        string vis = "";
                        if (fIns.SceneItems[i].ContainsKey("visible", out vis))
                        {
                            if (vis == "0")
                            {
                                mr.enabled = false;
                                //BoxCollider co = mr.gameObject.AddComponent<BoxCollider>();
                            }
                        }
                        string block = "";
                        if (fIns.SceneItems[i].ContainsKey("blockplayer", out block))
                        {
                            if (block == "no")
                            {
                                MeshCollider co = mr.gameObject.GetComponent<MeshCollider>();
                                if (co == null)
                                    co = mr.gameObject.AddComponent<MeshCollider>();
                                if (co != null)
                                {
                                    co.enabled = false;
                                    co.convex = false;
                                    co.isTrigger = false;
                                }
                            }
                        }
                        else
                        {
                            Collider coexist = mr.gameObject.GetComponent<Collider>();
                            if (coexist == null)
                            {
                                MeshCollider co = mr.gameObject.AddComponent<MeshCollider>();
                                co.isTrigger = false;
                            }
                        }
                    }
                //}

                objMesh.transform.SetParent(parent);

                if (addIflComponent && iflMat.ContainsKey(iflParam))
                {
                    IFLLoader iflL = objMesh.AddComponent<IFLLoader>();
                    iflL.fileNameReadOnly = iflMat[iflParam];
                    iflL.IFLFile = Resources.Load<TextAsset>(iflMat[iflParam]);
                    iflL.matIndex = iflMatIndex;
                    iflL.useSharedMaterial = false;
                    iflL.LoadIFL();
                }

                //如果是板凳桌子，加上双面网格，避免一些BUG
                if (parent.name.ToLower().Contains("chair") || parent.name.ToLower().Contains("desk"))
                    objMesh.AddComponent<DoubleSideMeshCollider>();
                objMesh.transform.localRotation = fIns.SceneItems[i].quat;
                objMesh.transform.localScale = Vector3.one;
                objMesh.transform.localPosition = fIns.SceneItems[i].pos;
            }
        }
        else if (fModel != null)
        {
            //Debug.LogError("error !!!!!!!!!!!!!!!!!!!!!!!!! not support gmc file any more");
            
            Material[] mat = new Material[fModel.TexturesCount];
            for (int i = 0; i < mat.Length; i++)
                mat[i] = null;
            Dictionary<int, string> iflMat = new Dictionary<int, string>();//IFL材质特殊处理,IFL材质均为mobile/particle/additive

            for (int x = 0; x < fModel.TexturesCount; x++)
            {
                string tex = fModel.TexturesNames[x];
                if (tex.ToLower().EndsWith(".ifl"))
                {
                    if (!iflMat.ContainsKey(x))
                        iflMat.Add(x, tex);
                    continue;
                }

                mat[x] = Resources.Load<Material>(string.Format("{0}_{1:D2}", file, x));
                if (mat[x] == null)
                {
                    mat[x] = new Material(ShaderMng.Find("AlphaTexture"));
                    int del = tex.LastIndexOf('.');
                    if (del != -1)
                        tex = tex.Substring(0, del);
                    Texture texture = Resources.Load<Texture>(tex);
                    if (texture == null)
                        Debug.LogError("texture miss on load gmc:" + file + " texture name:" + tex);
                    mat[x].SetTexture("_MainTex", texture);
                    mat[x].name = string.Format("{0}_{1:D2}", file, x);
                    if (gen)
                    {
                        //UnityEditor.AssetDatabase.CreateAsset(mat[x], "Assets/Materials/Weapons/resources/" + mat[x].name + ".mat");
                        //UnityEditor.AssetDatabase.Refresh();
                    }
                }
                //mat[x] = Resources.Load<Material>(string.Format("{0}_{1:D2}", file, x));
                //if (mat[x] == null)
                //{
                //    mat[x] = new Material(ShaderUtil.Find("UnlitAlphaTexture"));
                //    string tex = fModel.TexturesNames[x];
                //    int del = tex.LastIndexOf('.');
                //    if (del != -1)
                //        tex = tex.Substring(0, del);
                //    Texture texture = Resources.Load<Texture>(tex);
                //    if (texture == null)
                //        Debug.LogError("texture miss on load gmc:" + file + " texture name:" + tex);
                //    mat[x].SetTexture("_MainTex", texture);
                //    mat[x].name = string.Format("{0}_{1:D2}", file, x);
                //    //AssetDatabase.CreateAsset(mat[x], "Assets/Materials/" + file + "/resources/" + mat[x].name + ".mat");
                //    //AssetDatabase.Refresh();
                //}
            }
            for (int i = 0; i < fIns.SceneItems.Count; i++)
            {
                GameObject objMesh = new GameObject();
                bool addIflComponent = false;
                int iflParam = -1;
                int iflMatIndex = 0;
                objMesh.name = fIns.SceneItems[i].name;
                objMesh.transform.localRotation = Quaternion.identity;
                objMesh.transform.localPosition = Vector3.zero;
                objMesh.transform.localScale = Vector3.one;
                objMesh.layer = parent.gameObject.layer;
                //bool realObject = false;//是不是正常物体，虚拟体无需设置材质球之类
                if (i < fModel.SceneObjectsCount)
                {
                //for (int j = 0; j < fModel.SceneObjectsCount; j++)
                //{
                    //if (fModel.mesh[j].name == objMesh.name)
                    {
                        //realObject = true;
                        Mesh w = new Mesh();
                        //前者子网格编号，后者 索引缓冲区
                        Dictionary<int, List<int>> tr = new Dictionary<int, List<int>>();
                        List<Vector3> ve = new List<Vector3>();
                        List<Vector2> uv = new List<Vector2>();
                        List<Vector3> nor = new List<Vector3>();
                        List<Color> col = new List<Color>();
                        for (int k = 0; k < fModel.mesh[i].faces.Count; k++)
                        {
                            int key = fModel.mesh[i].faces[k].material;
                            if (tr.ContainsKey(key))
                            {
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[0]);
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[1]);
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[2]);
                            }
                            else
                            {
                                tr.Add(key, new List<int>());
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[0]);
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[1]);
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[2]);
                            }

                        }
                        for (int k = 0; k < fModel.mesh[i].vertices.Count; k++)
                        {
                            Vector3 vec = fModel.mesh[i].vertices[k].pos - (Vector3)fIns.SceneItems[i].pos;
                            vec = Quaternion.Inverse(fIns.SceneItems[i].quat) * vec;
                            //ve.Add(fModel.mesh[i].vertices[k].pos);
                            ve.Add(vec);
                            uv.Add(fModel.mesh[i].vertices[k].uv);
                            col.Add(fModel.mesh[i].vertices[k].color);
                            nor.Add(fModel.mesh[i].vertices[k].normal);
                        }
                        w.SetVertices(ve);
                        w.uv = uv.ToArray();
                        w.subMeshCount = tr.Count;
                        int ss = 0;
                        List<Material> targetMat = new List<Material>();
                        foreach (var each in tr)
                        {
                            w.SetIndices(each.Value.ToArray(), MeshTopology.Triangles, ss++);
                            if (each.Key >= 0 && each.Key < fModel.shader.Length)
                            {
                                int materialIndex = fModel.shader[each.Key].TextureArg0;
                                if (materialIndex >= 0 && materialIndex < mat.Length)
                                {
                                    if (mat[materialIndex] == null)
                                    {
                                        targetMat.Add(new Material(Shader.Find("Unlit/Transparent")));
                                        addIflComponent = true;
                                        iflParam = materialIndex;
                                        iflMatIndex = targetMat.Count - 1;
                                    }
                                    else
                                    {
                                        targetMat.Add(mat[materialIndex]);
                                    }
                                }
                                else
                                {
                                    //即使没有贴图，也存在材质
                                    Material defaults = Resources.Load<Material>(string.Format("{0}_{1:D2}", file, materialIndex));
                                    if (defaults == null)
                                        defaults = new Material(Shader.Find("Unlit/Texture"));
                                    defaults.name = string.Format("{0}_{1:D2}", file, materialIndex);
                                    if (gen)
                                    {
                                        //UnityEditor.AssetDatabase.CreateAsset(defaults, "Assets/Materials/Weapons/resources/" + defaults.name + ".mat");
                                        //UnityEditor.AssetDatabase.Refresh();
                                    }
                                    targetMat.Add(defaults);
                                }
                            }
                        }
                        MeshRenderer mr = objMesh.AddComponent<MeshRenderer>();
                        MeshFilter mf = objMesh.AddComponent<MeshFilter>();
                        mf.mesh = w;
                        mf.mesh.colors = col.ToArray();
                        mf.mesh.normals = nor.ToArray();
                        mf.mesh.RecalculateBounds();
                        mf.mesh.RecalculateNormals();

                        mr.materials = targetMat.ToArray();
                        string vis = "";
                        if (fIns.SceneItems[i].ContainsKey("visible", out vis))
                        {
                            if (vis == "0")
                            {
                                mr.enabled = false;
                                //BoxCollider co = mr.gameObject.AddComponent<BoxCollider>();
                            }
                        }
                        string block = "";
                        if (fIns.SceneItems[i].ContainsKey("blockplayer", out block))
                        {
                            if (block == "no")
                            {
                                Collider co = mr.gameObject.GetComponent<MeshCollider>();
                                if (co == null)
                                    co = mr.gameObject.AddComponent<MeshCollider>();
                                if (co != null)
                                {
                                    MeshCollider mec = co as MeshCollider;
                                    if (mec != null)
                                    {
                                        mec.enabled = false;
                                        mec.convex = false;//unity bug
                                        mec.isTrigger = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Collider coexist = mr.gameObject.GetComponent<Collider>();
                            if (coexist == null)
                            {
                                MeshCollider co = mr.gameObject.AddComponent<MeshCollider>();
                                co.isTrigger = false;
                            }
                        }
                    }
                }

                objMesh.transform.SetParent(parent);
                if (addIflComponent && iflMat.ContainsKey(iflParam))
                {
                    IFLLoader iflL = objMesh.AddComponent<IFLLoader>();
                    iflL.fileNameReadOnly = iflMat[iflParam];
                    iflL.IFLFile = Resources.Load<TextAsset>(iflMat[iflParam]);
                    iflL.matIndex = iflMatIndex;
                    iflL.useSharedMaterial = false;
                    iflL.LoadIFL();
                }
                //如果是板凳桌子，加上双面网格，避免一些BUG
                if (parent.name.ToLower().Contains("chair") || parent.name.ToLower().Contains("desk"))
                    objMesh.AddComponent<DoubleSideMeshCollider>();
                objMesh.transform.localRotation = fIns.SceneItems[i].quat;
                objMesh.transform.localScale = Vector3.one;
                objMesh.transform.localPosition = fIns.SceneItems[i].pos;
            }
        }
        else
        {
            //一个预设
            GameObject prefab = Resources.Load(file) as GameObject;
            if (prefab != null)
            {
                GameObject obj = GameObject.Instantiate(prefab, parent);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
                BoxCollider co = obj.GetComponentInChildren<BoxCollider>();
                co.isTrigger = true;
                int k = obj.transform.childCount;
                while (obj.transform.childCount != 0)
                {
                    Transform tri = obj.transform.GetChild(0);
                    tri.SetParent(parent);
                }
                GameObject.Destroy(obj);
                WsGlobal.SetObjectLayer(parent.gameObject, parent.gameObject.layer);
            }
        }
    }
}
