using Ionic.Zlib;
using ShortcutExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

using UnityEngine;
using UnityEngine.UI;

public class Utility
{
    public static byte[] ConvertBytesZlib(byte[] data, CompressionMode compressionMode)
    {
        CompressionMode mode = compressionMode;
        if (mode != CompressionMode.Compress)
        {
            if (mode != CompressionMode.Decompress)
            {
                throw new NotImplementedException();
            }
            return ZlibStream.UncompressBuffer(data);
        }
        return ZlibStream.CompressBuffer(data);
    }

    public static void Zero(GameObject target)
    {
        target.transform.localPosition = Vector3.zero;
        target.transform.localRotation = Quaternion.identity;
        target.transform.localScale = Vector3.one;
    }

    public static void Swap<T>(ref T a, ref T b) {
        T c = a;
        a = b;
        b = c;
    }

    public static Quaternion Scale(Quaternion q, float s) {
        Quaternion r = new Quaternion(q.x * s, q.y * s, q.z * s, q.w * s);
        return r;
    }

    public static Quaternion Divide(Quaternion q, float s) {
        Quaternion r = new Quaternion(q.x / s, q.y / s, q.z / s, q.w / s);
        return r;
    }

    public static Quaternion Add(Quaternion q, Quaternion v) {
        q.w += v.w;
        q.x += v.x;
        q.y += v.y;
        q.z += v.z;
        return q;
    }

    public static void Clamp(ref Vector3 vec, float speedMin, float speedMax) {
        vec.x = Mathf.Clamp(vec.x, speedMin, speedMax);
        vec.y = Mathf.Clamp(vec.y, speedMin, speedMax);
        vec.z = Mathf.Clamp(vec.z, speedMin, speedMax);
    }
    public static Quaternion NormalizeSafe(Quaternion q)
    {
	    float mag = Magnitude(q);
	    if (mag < Vector3.kEpsilon)
		    return Quaternion.identity;	
	    else
		    return Divide(q, mag);
    }

    public static float Magnitude(Quaternion q)
    {
	    return SqrtImpl(SqrMagnitude(q));
    }

    public static float SqrtImpl(float f) {
        return Mathf.Sqrt(f);
    }

    public static float SqrMagnitude(Quaternion q)
    {
	    return Quaternion.Dot(q, q);
    }

    public static int GetDirection(float v) {
        if (v == 0) return 0;
        else return v > 0 ? 1 : -1;
    }

    public static float PingPong(float t, float begin, float end) {
        return PingPong(t - begin, end - begin) + begin;
    }

    public static float PingPong(float t, float length) {
        t = Repeat(t, length * 2.0F);
        t = length - Mathf.Abs(t - length);
        return t;
    }

    public static float Repeat(float t, float begin, float end) {
        return Repeat(t - begin, end - begin) + begin;
    }

    // Returns float remainder for t / length
    public static float Repeat(float t, float length) {
        if (length <= 0)
            return 0;
        return t - Mathf.Floor(t / length) * length;
    }

    public static void HermiteInterpolatePose(BoneStatus prev, BoneStatus next, ref BoneStatus calcOut, float percent) {
        if (prev == null && next != null) {
            next.Clone(ref calcOut);
            return;
        } else if (prev != null && next == null) {
            prev.Clone(ref calcOut);
            return;
        }
        calcOut.BonePos = Vector3.Lerp(prev.BonePos, next.BonePos, percent);
        for (int i = 0; i < calcOut.DummyQuat.Count; i++) {
            calcOut.DummyQuat[i] = Quaternion.Slerp(prev.DummyQuat[i], next.DummyQuat[i], percent);
        }
        for (int i = 0; i < calcOut.DummyPos.Count; i++) {
            calcOut.DummyPos[i] = Vector3.Lerp(prev.DummyPos[i], next.DummyPos[i], percent);
        }
        for (int i = 0; i < calcOut.BoneQuat.Count; i++) {
            calcOut.BoneQuat[i] = Quaternion.Slerp(prev.BoneQuat[i], next.BoneQuat[i], percent);
        }
    }

    public static bool CompareApproximately(float f0, float f1, float epsilon = 0.000001F) {
        float dist = (f0 - f1);
        dist = Mathf.Abs(dist);
        return dist < epsilon;
    }

    public static double RepeatD(double t, double begin, double end) {
        return RepeatD(t - begin, end - begin) + begin;
    }

    public static double RepeatD(double t, double length) {
        if (length <= 0)
            return 0;
        return (double)(t - Mathf.Floor((float)t / (float)length) * length);
    }

    public static string getFileHash(string filePath) {
        try {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            int len = (int)fs.Length;
            byte[] data = new byte[len];
            fs.Read(data, 0, len);
            fs.Close();
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(data);
            string fileMD5 = "";
            foreach (byte b in result) {
                fileMD5 += Convert.ToString(b, 16);
            }
            return fileMD5;
        } catch (FileNotFoundException e) {
            Log.WriteError(e.StackTrace);
            return "";
        }
    }

    static int index = 0;
    public static int Range(int min, int max) {
        return UnityEngine.Random.Range(min, max);
    }

    public static float Range(float min, float max) {
        return UnityEngine.Random.Range(min, max);
    }

    public static void SavePng(string path, byte[] content) {
        try {
            System.IO.File.WriteAllBytes(path, content);
        }
        catch (Exception exp){
            Debug.LogError(exp.Message);
        }
    }

    public static bool SameCrc(string file, string md5) {
        return getFileHash(file) == md5;
    }

    public static bool LoadPreview(Image preview, string localPath) {
        if (System.IO.File.Exists(localPath)) {
            byte[] array = System.IO.File.ReadAllBytes(localPath);
            Texture2D tex = new Texture2D(0, 0, TextureFormat.ARGB32, false);
            tex.LoadImage(array);
            preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            return true;
        }
        return false;
    }

    //把图片放到整个画布，不要留黑边，可以超越屏幕
    public static void Expand(Image image, float width, float height) {
        float full_height = 0;
        float full_width = 0;
        //找到比例小的属性，把比例小的属性拉满到对应的轴
        if (width > height) {
            float height_aspect = UIHelper.resolution.y / height;
            full_height = UIHelper.resolution.y; 
            full_width = width * height_aspect;
        } else {
            float width_aspect = UIHelper.resolution.x / width;
            full_width = UIHelper.resolution.x;
            full_height = height * width_aspect;
        }
        RectTransform rect = image.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(full_width, full_height);
    }

    public static byte[] CaptureScreen(Camera targetCamera) {
        if (targetCamera.targetTexture == null) {
            targetCamera.targetTexture = new RenderTexture((int)UIHelper.CanvasWidth / 5, (int)UIHelper.CanvasHeight / 5, 16);
        }
        int width = targetCamera.targetTexture.width;
        int height = targetCamera.targetTexture.height;
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = null;
        RenderTexture.active = targetCamera.targetTexture;
        targetCamera.Render();
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        byte[] screenShot = texture.EncodeToPNG();
        RenderTexture.active = null;
        targetCamera.targetTexture = null;
        return screenShot;
    }

    public static void SetObjectLayer(GameObject objSelect, int layermask) {
        objSelect.layer = layermask;
        for (int i = 0; i < objSelect.transform.childCount; i++) {
            SetObjectLayer(objSelect.transform.GetChild(i).gameObject, layermask);
        }
    }

    public static void ShowMeteorObject(string file, Transform parent, bool gen = false) {
        if (file.EndsWith(".des"))
            file = file.Substring(0, file.Length - 4);
        DesFile fIns = DesLoader.Ins.Load(file);
        GMBFile gmb = GMBLoader.Ins.Load(file);
        GMCFile fModel = GMCLoader.Ins.Load(file);
        //保存材质球
        if (Application.isEditor) {
            if (!System.IO.Directory.Exists("Assets/Materials/Weapons/resources/"))
                System.IO.Directory.CreateDirectory("Assets/Materials/Weapons/resources/");
        }

        //看des文件是否为空
        //优先GMB
        if (gmb != null) {
            //还未处理IFL序列图动态加载。
            Material[] mat = new Material[gmb.TexturesCount];
            for (int i = 0; i < mat.Length; i++)
                mat[i] = null;
            SortedDictionary<int, string> iflMat = new SortedDictionary<int, string>();//IFL材质特殊处理,IFL材质均为mobile/particle/additive
            for (int x = 0; x < gmb.TexturesCount; x++) {
                string tex = gmb.TexturesNames[x];
                if (tex.ToLower().EndsWith(".ifl")) {
                    if (!iflMat.ContainsKey(x))
                        iflMat.Add(x, tex);
                    continue;
                }

                string iden = string.Format("{0}_{1:D2}", file, x);
                mat[x] = Resources.Load<Material>(iden);
                if (mat[x] == null) {
                    mat[x] = new Material(ShaderMng.Find("AlphaTexture"));
                    int del = tex.LastIndexOf('.');
                    if (del != -1)
                        tex = tex.Substring(0, del);
                    Texture texture = Resources.Load<Texture>(tex);
                    if (texture == null)
                        Debug.LogError("texture miss on load gmb:" + file + " texture name:" + tex);
                    mat[x].SetTexture("_MainTex", texture);
                    mat[x].name = iden;
                    if (gen) {
                        //UnityEditor.AssetDatabase.CreateAsset(mat[x], "Assets/Materials/Weapons/resources/" + mat[x].name + ".mat");
                        //UnityEditor.AssetDatabase.Refresh();
                    }
                }
            }

            for (int i = 0; i < fIns.SceneItems.Count; i++) {
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
                if (i < gmb.SceneObjectsCount) {
                    //for (int j = 0; j < gmb.SceneObjectsCount; j++)
                    //{
                    //    if (gmb.mesh[j].name == objMesh.name)
                    //    {
                    //realObject = true;
                    Mesh w = new Mesh();
                    //前者子网格编号，后者 索引缓冲区
                    SortedDictionary<int, List<int>> tr = new SortedDictionary<int, List<int>>();
                    List<Vector3> ve = new List<Vector3>();
                    List<Vector2> uv = new List<Vector2>();
                    List<Vector3> nor = new List<Vector3>();
                    List<Color> col = new List<Color>();
                    for (int k = 0; k < gmb.mesh[i].faces.Count; k++) {
                        int key = gmb.mesh[i].faces[k].material;
                        if (tr.ContainsKey(key)) {
                            tr[key].Add(gmb.mesh[i].faces[k].triangle[0]);
                            tr[key].Add(gmb.mesh[i].faces[k].triangle[1]);
                            tr[key].Add(gmb.mesh[i].faces[k].triangle[2]);
                        } else {
                            tr.Add(key, new List<int>());
                            tr[key].Add(gmb.mesh[i].faces[k].triangle[0]);
                            tr[key].Add(gmb.mesh[i].faces[k].triangle[1]);
                            tr[key].Add(gmb.mesh[i].faces[k].triangle[2]);
                        }

                    }
                    for (int k = 0; k < gmb.mesh[i].vertices.Count; k++) {
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
                    foreach (var each in tr) {
                        w.SetIndices(each.Value.ToArray(), MeshTopology.Triangles, ss++);
                        if (each.Key >= 0 && each.Key < gmb.shader.Length) {
                            int materialIndex = gmb.shader[each.Key].TextureArg0;
                            if (materialIndex >= 0 && materialIndex < mat.Length) {
                                if (mat[materialIndex] == null) {
                                    targetMat.Add(new Material(Shader.Find("Unlit/Transparent")));
                                    addIflComponent = true;
                                    iflParam = materialIndex;
                                    iflMatIndex = targetMat.Count - 1;
                                } else {
                                    targetMat.Add(mat[materialIndex]);
                                }
                            } else {
                                Material defaults = Resources.Load<Material>(string.Format("{0}_{1:D2}", file, materialIndex));
                                if (defaults == null)
                                    defaults = new Material(Shader.Find("Unlit/Texture"));
                                defaults.name = string.Format("{0}_{1:D2}", file, materialIndex);
                                if (gen) {
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
                    if (fIns.SceneItems[i].ContainsKey("visible", out vis)) {
                        if (vis == "0") {
                            mr.enabled = false;
                            //BoxCollider co = mr.gameObject.AddComponent<BoxCollider>();
                        }
                    }
                    string block = "";
                    if (fIns.SceneItems[i].ContainsKey("blockplayer", out block)) {
                        if (block == "no") {
                            MeshCollider co = mr.gameObject.GetComponent<MeshCollider>();
                            if (co == null)
                                co = mr.gameObject.AddComponent<MeshCollider>();
                            if (co != null) {
                                co.enabled = false;
                                co.convex = false;
                                co.isTrigger = false;
                            }
                        }
                    } else {
                        Collider coexist = mr.gameObject.GetComponent<Collider>();
                        if (coexist == null) {
                            MeshCollider co = mr.gameObject.AddComponent<MeshCollider>();
                            co.isTrigger = false;
                        }
                    }
                }
                //}

                objMesh.transform.SetParent(parent);

                if (addIflComponent && iflMat.ContainsKey(iflParam)) {
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
        } else if (fModel != null) {
            //Debug.LogError("error !!!!!!!!!!!!!!!!!!!!!!!!! not support gmc file any more");

            Material[] mat = new Material[fModel.TexturesCount];
            for (int i = 0; i < mat.Length; i++)
                mat[i] = null;
            SortedDictionary<int, string> iflMat = new SortedDictionary<int, string>();//IFL材质特殊处理,IFL材质均为mobile/particle/additive

            for (int x = 0; x < fModel.TexturesCount; x++) {
                string tex = fModel.TexturesNames[x];
                if (tex.ToLower().EndsWith(".ifl")) {
                    if (!iflMat.ContainsKey(x))
                        iflMat.Add(x, tex);
                    continue;
                }

                mat[x] = Resources.Load<Material>(string.Format("{0}_{1:D2}", file, x));
                if (mat[x] == null) {
                    mat[x] = new Material(ShaderMng.Find("AlphaTexture"));
                    int del = tex.LastIndexOf('.');
                    if (del != -1)
                        tex = tex.Substring(0, del);
                    Texture texture = Resources.Load<Texture>(tex);
                    if (texture == null)
                        Debug.LogError("texture miss on load gmc:" + file + " texture name:" + tex);
                    mat[x].SetTexture("_MainTex", texture);
                    mat[x].name = string.Format("{0}_{1:D2}", file, x);
                    if (gen) {
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
            for (int i = 0; i < fIns.SceneItems.Count; i++) {
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
                if (i < fModel.SceneObjectsCount) {
                    //for (int j = 0; j < fModel.SceneObjectsCount; j++)
                    //{
                    //if (fModel.mesh[j].name == objMesh.name)
                    {
                        //realObject = true;
                        Mesh w = new Mesh();
                        //前者子网格编号，后者 索引缓冲区
                        SortedDictionary<int, List<int>> tr = new SortedDictionary<int, List<int>>();
                        List<Vector3> ve = new List<Vector3>();
                        List<Vector2> uv = new List<Vector2>();
                        List<Vector3> nor = new List<Vector3>();
                        List<Color> col = new List<Color>();
                        for (int k = 0; k < fModel.mesh[i].faces.Count; k++) {
                            int key = fModel.mesh[i].faces[k].material;
                            if (tr.ContainsKey(key)) {
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[0]);
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[1]);
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[2]);
                            } else {
                                tr.Add(key, new List<int>());
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[0]);
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[1]);
                                tr[key].Add(fModel.mesh[i].faces[k].triangle[2]);
                            }

                        }
                        for (int k = 0; k < fModel.mesh[i].vertices.Count; k++) {
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
                        foreach (var each in tr) {
                            w.SetIndices(each.Value.ToArray(), MeshTopology.Triangles, ss++);
                            if (each.Key >= 0 && each.Key < fModel.shader.Length) {
                                int materialIndex = fModel.shader[each.Key].TextureArg0;
                                if (materialIndex >= 0 && materialIndex < mat.Length) {
                                    if (mat[materialIndex] == null) {
                                        targetMat.Add(new Material(Shader.Find("Unlit/Transparent")));
                                        addIflComponent = true;
                                        iflParam = materialIndex;
                                        iflMatIndex = targetMat.Count - 1;
                                    } else {
                                        targetMat.Add(mat[materialIndex]);
                                    }
                                } else {
                                    //即使没有贴图，也存在材质
                                    Material defaults = Resources.Load<Material>(string.Format("{0}_{1:D2}", file, materialIndex));
                                    if (defaults == null)
                                        defaults = new Material(Shader.Find("Unlit/Texture"));
                                    defaults.name = string.Format("{0}_{1:D2}", file, materialIndex);
                                    if (gen) {
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
                        if (fIns.SceneItems[i].ContainsKey("visible", out vis)) {
                            if (vis == "0") {
                                mr.enabled = false;
                                //BoxCollider co = mr.gameObject.AddComponent<BoxCollider>();
                            }
                        }
                        string block = "";
                        if (fIns.SceneItems[i].ContainsKey("blockplayer", out block)) {
                            if (block == "no") {
                                Collider co = mr.gameObject.GetComponent<MeshCollider>();
                                if (co == null)
                                    co = mr.gameObject.AddComponent<MeshCollider>();
                                if (co != null) {
                                    MeshCollider mec = co as MeshCollider;
                                    if (mec != null) {
                                        mec.enabled = false;
                                        mec.convex = false;//unity bug
                                        mec.isTrigger = false;
                                    }
                                }
                            }
                        } else {
                            Collider coexist = mr.gameObject.GetComponent<Collider>();
                            if (coexist == null) {
                                MeshCollider co = mr.gameObject.AddComponent<MeshCollider>();
                                co.isTrigger = false;
                            }
                        }
                    }
                }

                objMesh.transform.SetParent(parent);
                if (addIflComponent && iflMat.ContainsKey(iflParam)) {
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
        } else {
            //一个预设
            GameObject prefab = Resources.Load(file) as GameObject;
            if (prefab != null) {
                GameObject obj = GameObject.Instantiate(prefab, parent);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
                BoxCollider co = obj.GetComponentInChildren<BoxCollider>();
                co.isTrigger = true;
                int k = obj.transform.childCount;
                while (obj.transform.childCount != 0) {
                    Transform tri = obj.transform.GetChild(0);
                    tri.SetParent(parent);
                }
                GameObject.Destroy(obj);
                Utility.SetObjectLayer(parent.gameObject, parent.gameObject.layer);
            }
        }
    }

    //得到小时：分钟：秒
    public static string GetDuration(float duration) {
        int hour = Mathf.FloorToInt(duration / (60 * 60));
        int minute = Mathf.FloorToInt((duration - (hour * 60 * 60)) / 60);
        int second = Mathf.FloorToInt(duration - (hour * 60 * 60) - minute * 60);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, minute, second);
    }

    public static bool CameraCanLookTarget(MeteorUnit Target, Vector3 pos, out Vector3 hit) {
        RaycastHit wallHit;
        Vector3 targetPos = Target.mSkeletonPivot;
        if (Physics.Linecast(targetPos, pos, out wallHit, LayerManager.AllSceneMask)) {
            hit = wallHit.point + Vector3.Normalize(targetPos - wallHit.point);
            return false;
        }
        hit = pos;
        return true;
    }

    //得到某个角色的面向向量与某个位置的夹角,不考虑Y轴 
    public static float GetAngleBetween(MeteorUnit Player, Vector3 vec) {
        vec.y = 0;
        //同位置，无法计算夹角.
        if (vec.x == Player.transform.position.x && vec.z == Player.transform.position.z)
            return 0;

        Vector3 vec1 = -1 * Player.transform.forward;
        Vector3 vec2 = (vec - Player.mPos2d).normalized;
        vec2.y = 0;
        float radian = Vector3.Dot(vec1, vec2);
        float degree = Mathf.Acos(Mathf.Clamp(radian, -1.0f, 1.0f)) * Mathf.Rad2Deg;
        return degree;
    }

    //令相机Z朝向目标，Y控制朝向，X控制俯仰
    //Z轴为定轴，不能斜着
    public static void LookAt(Transform camera, Vector3 position) {

    }

    //计算夹角，不考虑Y轴
    public static float GetAngleBetween(Vector3 first, Vector3 second) {
        if (first.x == second.x && first.z == second.z)
            return 0;
        first.y = 0;
        second.y = 0;
        float s = Vector3.Dot(first, second);
        return s;//大于0，同方向，小于0 反方向
    }
    //public static void ClearCollision() {
    //    OBBs.Clear();
    //    OBBCache.Clear();
    //    CheckTimes = 0;
    //}
    ////清理掉碰撞缓存,这段时间内只要计算一次，就不再计算初始值
    //public static int CheckTimes = 0;
    //public static void ClearCollisionCache() {
    //    if (CheckTimes >= 5000) {
    //        ClearCollision();
    //        return;
    //    }
    //    for (int i = 0; i < OBBs.Count; i++) {
    //        OBBs[i].ClearCache();
    //    }
    //    CheckTimes += 1;
    //}

    //static BiMap<OBB3D, Collider> OBBCache = new BiMap<OBB3D, Collider>();
    //static List<OBB3D> OBBs = new List<OBB3D>();
    //public static void OnDrawGizmos() {
    //    for (int i = 0; i < OBBs.Count; i++) {
    //        OBBs[i].OnDrawGizmos();
    //    }
    //}

    //转换为OBB通过OBB做相交判定,效率太差，得用系统的，此方式已废弃
    //改为使用引擎的碰撞系统
    //public static bool Intersects(Collider a, Collider b) {
    //    bool ret = false;
    //    OBB3D collisionA = null, collisionB = null;
    //    if (OBBCache.ContainsValue(a))
    //        collisionA = OBBCache.GetKey(a);
    //    if (OBBCache.ContainsValue(b))
    //        collisionB = OBBCache.GetKey(b);
    //    if (U3D.showBox) {
    //        BoundsGizmos.Instance.AddCollider(a);
    //        BoundsGizmos.Instance.AddCollider(b);
    //    }
    //    if (a is BoxCollider && b is BoxCollider) {
    //        if (collisionA == null) {
    //            BoxCollider ca = a as BoxCollider;
    //            collisionA = new OBB3D(ca.transform, ca.center, ca.size);
    //            OBBCache.Add(collisionA, a);
    //            OBBs.Add(collisionA);
    //        }
    //        if (collisionB == null) {
    //            BoxCollider cb = b as BoxCollider;
    //            collisionB = new OBB3D(cb.transform, cb.center, cb.size);
    //            OBBCache.Add(collisionB, b);
    //            OBBs.Add(collisionB);
    //        }
    //        ret = collisionA.Intersects(collisionB);
    //    } else if (a is BoxCollider && b is MeshCollider) {
    //        if (collisionA == null) {
    //            BoxCollider ca = a as BoxCollider;
    //            collisionA = new OBB3D(ca.transform, ca.center, ca.size);
    //            OBBCache.Add(collisionA, a);
    //            OBBs.Add(collisionA);
    //        }
    //        if (collisionB == null) {
    //            MeshCollider cb = b as MeshCollider;
    //            collisionB = new OBB3D(b.transform, cb.sharedMesh.bounds.center, cb.sharedMesh.bounds.size);
    //            OBBCache.Add(collisionB, b);
    //            OBBs.Add(collisionB);
    //        }
    //        ret = collisionA.Intersects(collisionB);
    //    } else if (a is MeshCollider && b is BoxCollider) {
    //        if (collisionB == null) {
    //            BoxCollider cb = b as BoxCollider;
    //            collisionB = new OBB3D(b.transform, cb.center, cb.size);
    //            OBBCache.Add(collisionB, b);
    //            OBBs.Add(collisionB);
    //        }
    //        if (collisionA == null) {
    //            MeshCollider ca = a as MeshCollider;
    //            collisionA = new OBB3D(a.transform, ca.sharedMesh.bounds.center, ca.sharedMesh.bounds.size);
    //            OBBCache.Add(collisionA, a);
    //            OBBs.Add(collisionA);
    //        }
    //        ret = collisionA.Intersects(collisionB);
    //    } else if (a is MeshCollider && b is MeshCollider) {
    //        if (collisionB == null) {
    //            MeshCollider cb = b as MeshCollider;
    //            collisionB = new OBB3D(b.transform, cb.sharedMesh.bounds.center, cb.sharedMesh.bounds.size);
    //            OBBCache.Add(collisionB, b);
    //            OBBs.Add(collisionB);
    //        }
    //        if (collisionA == null) {
    //            MeshCollider ca = a as MeshCollider;
    //            collisionA = new OBB3D(a.transform, ca.sharedMesh.bounds.center, ca.sharedMesh.bounds.size);
    //            OBBCache.Add(collisionA, a);
    //            OBBs.Add(collisionA);
    //        }
    //        ret = collisionA.Intersects(collisionB);
    //    } else {
    //        Debug.LogError("unknown type overlap");
    //    }
    //    return ret;
    //}
}

public class StringUtils
{
    //读取表就OK了，后续改.
    public static SortedDictionary<string, string> language = new SortedDictionary<string, string>();
    public const string Install = "安裝";
    public const string Cancel = "取消";
    public const string InstallFailed = "安裝失敗";
    public const string Uninstall = "卸载";
    public const string ModelName = "「{0}-{1}」";//id-name
    public const string DlcName = "「{0}-{1}」";//id-name
    public const string UninstallModel = "卸载「模型-{0}」";
    public const string UninstallChapter = "卸载「剧本-{0}」";
    public const string Startup = "流星正在启动{0}%";
    public const string SetPlayerModel = "已设置主角使用「模型-{0}」";
    public const string DefaultPlayer = "孟星魂";
    public const string Unzip = "解压中";
    public const string Error = "出错";
}