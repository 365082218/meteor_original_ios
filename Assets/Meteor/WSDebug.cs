using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
//using UnityEditor;
using System.Collections.Generic;

//负责一些辅助功能，加载原流星的gmc转换为level和解析原流星的引用图片等等
public class WSDebug : MonoBehaviour {
    public int MoveSpeed;
    public float GameSpeed = 1.0f;
    public int level;
    public GameObject canvas;
    public static WSDebug Ins = null;
    void Awake() { if (Ins == null) Ins = this; }
    void OnDestroy() { Ins = null; }
    public void SyncAttr()
    {
        if (MeteorManager.Instance.LocalPlayer != null && MeteorManager.Instance.LocalPlayer.Attr != null)
            MoveSpeed = MeteorManager.Instance.LocalPlayer.Attr.Speed;
    }

    public void Apply()
    {
        if (MeteorManager.Instance.LocalPlayer != null && MeteorManager.Instance.LocalPlayer.Attr != null)
            MeteorManager.Instance.LocalPlayer.Attr.Speed = MoveSpeed;
        GameSpeed %= 4.0f;
        Time.timeScale = GameSpeed;
    }
    public void CalcMeshCount()
    {
        int vertexCnt = 0;
        int triangleCnt = 0;
        int submeshCnt = 0;
        MeshFilter[] f = FindObjectsOfType<MeshFilter>();
        
        for (int i = 0; i < f.Length; i++)
        {
            bool repeat = false;
            for (int j = 0; j < i; j++)
            {
                if (f[i] == f[j])
                {
                    repeat = true;
                    break;
                }
                if (f[i].sharedMesh == f[j].sharedMesh)
                {
                    repeat = true;
                    break;
                }
            }
            if (repeat)
                continue;
            vertexCnt += f[i].sharedMesh.vertexCount;
            triangleCnt += f[i].sharedMesh.triangles.Length / 3;
            submeshCnt += f[i].sharedMesh.subMeshCount;
        }

        Debug.Log("顶点数量:" + vertexCnt + " 面数:" + triangleCnt + " 子网格数:" + submeshCnt);
    }

    int missCnt = 0;
    public void ProcessNextLevel()
    {
        System.Collections.Generic.List<string> ls = new System.Collections.Generic.List<string>();
        Level lev = LevelMng.Instance.GetItem(level);
        if (lev == null || ls.Contains(lev.Scene))
        {
            missCnt++;
            if (missCnt >= 5)
                return;
            level++;
            ProcessNextLevel();
            return;
        }
        else
        {
            missCnt = 0;
            Scene sc = SceneManager.GetSceneByName(lev.Scene);
            if (sc == null)
            {
                level++;
                ProcessNextLevel();
                return;
            }
            SceneManager.LoadScene(lev.Scene, LoadSceneMode.Single);
            ls.Add(lev.Scene);


            //ProcessTexture();
            return;
            level++;
            ProcessNextLevel();
        }
    }

    //public System.Collections.Generic.List<string> lsTexture = new System.Collections.Generic.List<string>();
    public void ProcessTexture()
    {
        //Level lev = LevelMng.Instance.GetItem(level);
        //if (lev == null)
        //    return;
        //if (!System.IO.Directory.Exists("Assets/Meteor/" + lev.goodList  + "/resources/"))
        //{
        //    System.IO.Directory.CreateDirectory("Assets/Meteor/" + lev.goodList + "/resources/");
        //    //AssetDatabase.Refresh();
        //    //generateFile = true;
        //}

        //MeshRenderer[] mr = FindObjectsOfType<MeshRenderer>();
        //for (int i = 0; i < mr.Length; i++)
        //{
        //    for (int j = 0; j < mr[i].materials.Length; j++)
        //    {
        //        if (mr[i].materials[j] != null)
        //        {
        //            if (mr[i].materials[j].shader.name == "TextureMask")
        //            {
        //                //2张图
        //                Texture tex = mr[i].materials[j].mainTexture;
        //                Texture texMask = mr[i].materials[j].GetTexture("_MaskTex");
        //                if (tex != null)
        //                {
        //                    string path = AssetDatabase.GetAssetPath(tex);
        //                    string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
        //                    string filename = filenames[filenames.Length - 1];
        //                    string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
        //                    if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename))
        //                    {
        //                        AssetDatabase.MoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
        //                        AssetDatabase.Refresh();
        //                    }
        //                    else
        //                        Debug.LogError(error);
        //                }
        //                if (texMask != null)
        //                {
        //                    string path = AssetDatabase.GetAssetPath(texMask);
        //                    string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
        //                    string filename = filenames[filenames.Length - 1];
        //                    string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
        //                    if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename))
        //                    {
        //                        AssetDatabase.MoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
        //                        AssetDatabase.Refresh();
        //                    }
        //                    else
        //                        Debug.LogError(error);
        //                }
        //            }
        //            else
        //            {
        //                if (mr[i].materials[j].mainTexture != null)
        //                {
        //                    string path = AssetDatabase.GetAssetPath(mr[i].materials[j].mainTexture);
        //                    string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
        //                    string filename = filenames[filenames.Length - 1];
        //                    string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
        //                    if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename))
        //                    {
        //                        AssetDatabase.MoveAsset(path, "Assets/Meteor/" + lev.goodList + "/resources/" + filename);
        //                        AssetDatabase.Refresh();
        //                    }
        //                    else
        //                        Debug.LogError(error);
        //                }
        //            }
        //        }
        //    }
        //}

        //IFLLoader[] lo = FindObjectsOfType<IFLLoader>();
        //if (lo.Length != 0)
        //{
        //    for (int i = 0; i < lo.Length; i++)
        //    {
        //        if (lo[i].tex != null && lo[i].tex.Length != 0)
        //        {
        //            for (int j = 0; j < lo[i].tex.Length; j++)
        //            {
        //                string path = AssetDatabase.GetAssetPath(lo[i].tex[j]);
        //                string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
        //                string filename = filenames[filenames.Length - 1];
        //                string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/ifl"  + "/resources/" + filename);
        //                if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/ifl" + "/resources/" + filename))
        //                {
        //                    AssetDatabase.MoveAsset(path, "Assets/Meteor/ifl"  + "/resources/" + filename);
        //                    AssetDatabase.Refresh();
        //                }
        //                else
        //                    Debug.LogError(error);
        //            }
        //        }
        //    }
        //}
        //Debug.LogError("ProcessTexture done");
        //level++;
        //AssetDatabase.MoveAsset();
    }

    //有此脚本，即可在任何关卡开启游戏
    public void Start()
    {
        if (Ins == this)
        {
            for (int i = 0; i < 20; i++)
                AmbLoader.Ins.LoadCharacterAmb(i);
            AmbLoader.Ins.LoadCharacterAmb();
            ActionInterrupt.Instance.Init();
            MenuResLoader.Instance.Init();
            //level = 1;
            //DontDestroyOnLoad(gameObject);
            //DontDestroyOnLoad(canvas);
            //Dictionary<string, List<string>> kv = new Dictionary<string, List<string>>();
            //for (int i = 0; i < 45; i++)
            //{
            //    if (i == 41)
            //        continue;
            //    Level lev = LevelMng.Instance.GetItem(i);
            //    if (lev == null)
            //        continue;
            //    DesFile des = DesLoader.Instance.Load(lev.goodList);
            //    GMBFile gmb = GMBLoader.Instance.Load(lev.goodList);
            //    if (gmb == null)
            //        continue;
            //    //for (int j = 0; j < gmb.TexturesNames.Count; j++)
            //    //{
            //        if (!kv.ContainsKey(lev.goodList))
            //            kv.Add(lev.goodList, gmb.TexturesNames);
            //    //}
            //}

            ////System.IO.StreamWriter wr = System.IO.File.CreateText("Assets/debug.txt");
            //foreach (var each in kv)
            //{
            //    //wr.WriteLine(each.Key);
            //    for (int i = 0; i < each.Value.Count; i++)
            //    {
            //        string []file = each.Value[i].Split(new char[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries);
            //        Texture tex = Resources.Load<Texture>(file[0]);
            //        if (tex != null)
            //        {
            //            string path = AssetDatabase.GetAssetPath(tex);
            //            tex = null;
            //            string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
            //            string filename = filenames[filenames.Length - 1];
            //            string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + each.Key + "/resources/" + filename);
            //            if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/" + each.Key + "/resources/" + filename))
            //            {
            //                AssetDatabase.MoveAsset(path, "Assets/Meteor/" + each.Key + "/resources/" + filename);
            //                AssetDatabase.Refresh();
            //            }
            //            else
            //                Debug.LogError(error);
            //        }
            //    }
            //        //wr.WriteLine(each.Value[i]);
            //}
            //wr.Flush();
            //wr.Close();
            //ProcessItemTexture();
        }

    
    }

    //public System.Collections.Generic.List<string> ltemTexture = new System.Collections.Generic.List<string>();
    public void ProcessItemTexture()
    {
        //Level lev = LevelMng.Instance.GetItem(level);
        //if (lev == null)
        //    return;
        //if (!System.IO.Directory.Exists("Assets/Meteor/Item"  + "/resources/"))
        //{
        //    System.IO.Directory.CreateDirectory("Assets/Meteor/Item" + "/resources/");
        //    AssetDatabase.Refresh();
        //    //generateFile = true;
        //}

        //MeshRenderer[] mr = FindObjectsOfType<MeshRenderer>();
        //for (int i = 0; i < mr.Length; i++)
        //{
        //    for (int j = 0; j < mr[i].materials.Length; j++)
        //    {
        //        if (mr[i].materials[j] != null)
        //        {

        //            if (mr[i].materials[j].shader.name == "TextureMask")
        //            {
        //                //2张图
        //                Texture tex = mr[i].materials[j].mainTexture;
        //                Texture texMask = mr[i].materials[j].GetTexture("_MaskTex");
        //                if (tex != null)
        //                {
        //                    string path = AssetDatabase.GetAssetPath(tex);
        //                    string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
        //                    string filename = filenames[filenames.Length - 1];
        //                    string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
        //                    if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename))
        //                    {
        //                        AssetDatabase.MoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
        //                        AssetDatabase.Refresh();
        //                    }
        //                    else
        //                        Debug.LogError(error);
        //                }
        //                if (texMask != null)
        //                {
        //                    string path = AssetDatabase.GetAssetPath(texMask);
        //                    string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
        //                    string filename = filenames[filenames.Length - 1];
        //                    string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
        //                    if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename))
        //                    {
        //                        AssetDatabase.MoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
        //                        AssetDatabase.Refresh();
        //                    }
        //                    else
        //                        Debug.LogError(error);
        //                }
        //            }
        //            else
        //            {
        //                if (mr[i].materials[j].mainTexture != null)
        //                {
        //                    string path = AssetDatabase.GetAssetPath(mr[i].materials[j].mainTexture);
        //                    string[] filenames = path.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
        //                    string filename = filenames[filenames.Length - 1];
        //                    string error = AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
        //                    if ("" == AssetDatabase.ValidateMoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename))
        //                    {
        //                        AssetDatabase.MoveAsset(path, "Assets/Meteor/Item" + "/resources/" + filename);
        //                        AssetDatabase.Refresh();
        //                    }
        //                    else
        //                        Debug.LogError(error);
        //                }
        //            }
        //        }
        //    }
        //}
        //Debug.LogError("ProcessItemTexture done");
        //level++;
        //AssetDatabase.MoveAsset();
    }

    public void GenerateAll()
    {
        Level[] all = LevelMng.Instance.GetAllItem();
        for (int i = 0; i < all.Length; i++)
        {
            GameObject obj = new GameObject(all[i].goodList);
            obj.transform.position = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.rotation = Quaternion.identity;
            obj.layer = LayerMask.NameToLayer("Scene");
            obj.transform.SetParent(null);
            MapLoader load = obj.AddComponent<MapLoader>();
            load.levelId = all[i].ID;
            load.LoadMap(all[i].ID);
        }
    }
}
