using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BncLoader:Singleton<BncLoader>
{
    SortedDictionary<string, BncFile> BncFile = new SortedDictionary<string, BncFile>();
    SortedDictionary<int, BncFile> GlobalBncFile = new SortedDictionary<int, BncFile>();
    public void Clear() {
        BncFile.Clear();
        GlobalBncFile.Clear();
    }
    BncFile Load(string file)
    {
        if (BncFile.ContainsKey(file))
            return BncFile[file];
        BncFile f = new global::BncFile();
        f.Load(file);
        if (f.error)
            return null;
        BncFile[file] = f;
        return f;
    }

    public BncFile LoadPluginBone(int modelIdx, string content)
    {
        if (GlobalBncFile.ContainsKey(modelIdx))
            return GlobalBncFile[modelIdx];
        BncFile f = new global::BncFile();
        f.LoadBnc(content);
        if (f.error)
            return null;
        GlobalBncFile.Add(modelIdx, f);
        return f;
    }

    public BncFile Load(int characterIdx)
    {
        if (CombatData.Ins.Chapter != null) {
            SortedDictionary<int, string> models = CombatData.Ins.GScript.GetModel();
            if (models != null && models.ContainsKey(characterIdx)) {
                string path = CombatData.Ins.Chapter.GetResPath(FileExt.Bnc, models[characterIdx]);
                if (!string.IsNullOrEmpty(path)) {
                    return Load(path);
                }
            }
        }

        if (characterIdx >= 20) {
            //如果这个插件拥有骨骼文件，加载骨骼文件，否则加载默认的0号主角骨骼.
            ModelItem m = DlcMng.GetPluginModel(characterIdx);
            if (m != null && m.Installed) {
                for (int i = 0; i < m.resPath.Count; i++) {
                    if (m.resPath[i].ToLower().EndsWith(".bnc")) {
                        string text = System.IO.File.ReadAllText(m.resPath[i]);
                        return LoadPluginBone(characterIdx, text);
                    }
                }
            }
            if (m != null) {
                if (m.useFemalePos)
                    return Load("p1.bnc");
            }
            return Load("p0.bnc");
        }
        return Load("p" + characterIdx + ".bnc");
    }
}
public class node
{
    public node parent;
    public string parentname;
    public string name;
    public int childcnt;
    public List<node> child = new List<node>();
    public Vector3 pivot;
    public Quaternion quat;
    public Transform bone;
    public bool type;//0 bone 1 dummy
    //帮儿子找爸爸.
    public static void AddChild(node son, List<node> search, string parentname)
    {
        bool find = false;
        for (int i = 0; i < search.Count; i++)
        {
            if (search[i].name == parentname)
            {
                if (search[i].child.Count != search[i].childcnt)
                    search[i].child.Add(son);
                if (son.bone == null)
                {
                    son.bone = new GameObject().transform;
                    son.bone.name = son.name;
                }

                son.bone.SetParent(search[i].bone);
                son.bone.localScale = Vector3.one;
                son.bone.localPosition = Vector3.zero;
                son.bone.localRotation = son.quat;
                son.bone.localPosition = son.pivot;
                find = true;
            }
        }
        if (!find)//没找到说明是d00 b00这样的父.
        {
            if (son.type)//是dummy
            {
                char dorb = son.parentname[0];
                int idx = int.Parse(son.parentname.Substring(1));
                node parent = GetNode(dorb == 'd', search, idx);
                if (parent.child.Count != parent.childcnt)
                    parent.child.Add(son);
                if (son.bone == null)
                {
                    son.bone = new GameObject().transform;
                    son.bone.name = son.name;
                }
                son.bone.SetParent(parent.bone);
                son.bone.localScale = Vector3.one;
                son.bone.localPosition = Vector3.zero;
                son.bone.localRotation = son.quat;
                son.bone.localPosition = son.pivot;
            }
        }
    }

    public static node GetNode(bool dummy, List<node> search, int index)
    {
        int idx = 0;
        for (int i = 0; i < search.Count; i++)
        {
            if (idx == index && search[i].type == dummy)
                return search[i];
            if (search[i].type == dummy)
                idx++;
        }
        return null;
    }
}

public class BncFile
{
    public node rootBone = null;
    List<node> allNode = new List<node>();
    ParseError errorno = ParseError.None;
    public bool error { get { return errorno != ParseError.None; } }
    public void LoadBnc(string content)
    {
        Parse(content);
    }

    public void Load(string file)
    {
        string text = null;
        TextAsset assets = Resources.Load<TextAsset>(file);
        if (assets == null)
        {
            if (!File.Exists(file)) {
                errorno = ParseError.Miss;
                return;
            }
            text = File.ReadAllText(file);
        } else {
            text = assets.text;
        }
        Parse(text);
    }

    public void Parse(string text)
    {
        try
        {
            //int boneCnt = 0;
            node cur = null;
            string[] lineBone = text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lineBone.Length; i++)
            {
                string[] eachlineBone = lineBone[i].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (eachlineBone[0].StartsWith("#"))
                    continue;
                if (eachlineBone[0] == "Bones:" && eachlineBone[2] == "Dummey:")
                {
                    //boneCnt = int.Parse(eachlineBone[1]);
                    //dummyCnt = int.Parse(eachlineBone[3]);
                }
                else
                if (eachlineBone[0] == "bone")
                {
                    //GameObject obj = new GameObject(eachlineBone[1]);
                    //obj.transform.position = Vector3.zero;
                    //obj.transform.localScale = Vector3.one;
                    //obj.transform.rotation = Quaternion.identity;
                    cur = new node();
                    cur.name = eachlineBone[1];
                    //cur.bone = obj.transform;
                    cur.type = false;
                    allNode.Add(cur);
                }
                else if (eachlineBone[0] == "{")
                    continue;
                else if (eachlineBone[0] == "}")
                    continue;
                else if (eachlineBone[0] == "parent")
                {
                    cur.parentname = eachlineBone[1];
                    if (cur.parentname == "NULL")
                    {
                        rootBone = new node();
                        //GameObject obj = new GameObject("NULL");
                        //obj.transform.SetParent(transform);
                        //obj.transform.localPosition = Vector3.zero;
                        //obj.transform.localScale = Vector3.one;
                        rootBone.name = "NULL";
                        rootBone.parent = null;
                        rootBone.childcnt = 1;
                        //rootBone.bone = obj.transform;
                        rootBone.parentname = "";
                        rootBone.pivot = Vector3.zero;
                        rootBone.quat = Quaternion.identity;
                        allNode.Add(rootBone);
                    }
                }
                else if (eachlineBone[0] == "pivot")
                {
                    cur.pivot = new Vector3();
                    cur.pivot.x = float.Parse(eachlineBone[1]);
                    cur.pivot.z = float.Parse(eachlineBone[2]);
                    cur.pivot.y = float.Parse(eachlineBone[3]);
                }
                else if (eachlineBone[0] == "quaternion")
                {
                    cur.quat = new Quaternion();
                    float w = float.Parse(eachlineBone[1]);
                    float x = -float.Parse(eachlineBone[2]);
                    float y = -float.Parse(eachlineBone[4]);
                    float z = -float.Parse(eachlineBone[3]);
                    cur.quat.Set(x, y, z, w);
                }
                else if (eachlineBone[0] == "children")
                {
                    cur.childcnt = int.Parse(eachlineBone[1]);
                }
                else if (eachlineBone[0] == "Dummey")
                {
                    //GameObject obj = new GameObject(eachlineBone[1]);
                    //obj.transform.position = Vector3.zero;
                    //obj.transform.localScale = Vector3.one;
                    //obj.transform.rotation = Quaternion.identity;
                    cur = new node();
                    cur.name = eachlineBone[1];
                    cur.bone = null;
                    cur.type = true;
                    allNode.Add(cur);
                }

            }

        }
        catch
        {

        }
    }
    public BncFile GenerateBone(Transform parent, ref List<Transform> bones, ref List<Transform> dummy, ref List<Matrix4x4> bindPos, ref Transform root)
    {
        //这个不是根骨骼，但是是根骨骼的父级。他的坐标内的动画，里面的d_base就会移动，他类似于骨骼坐标轴原点.
        //但是跳的时候，他是不动的，只有d_base控制的骨架会跟着动
        rootBone.bone = new GameObject().transform;
        rootBone.bone.name = "NULL";
        rootBone.bone.SetParent(parent);
        rootBone.bone.localPosition = Vector3.zero;
        rootBone.bone.localRotation = Quaternion.identity;
        rootBone.bone.localScale = Vector3.one;
        rootBone.bone.gameObject.layer = LayerManager.Bone;
        while (true)
        {
            for (int i = 0; i < allNode.Count; i++)
            {
                if (allNode[i].bone == null)
                {
                    allNode[i].bone = new GameObject().transform;
                    allNode[i].bone.name = allNode[i].name;
                    allNode[i].bone.gameObject.layer = LayerManager.Bone;
                    //allNode[i].bone.gameObject.layer = layer;
                }
            }
            for (int i = 0; i < allNode.Count; i++)
            {
                if (allNode[i].parentname != "")
                {
                    node.AddChild(allNode[i], allNode, allNode[i].parentname);
                }
            }
            break;
        }

        for (int i = 0; i < allNode.Count; i++)
        {
            if (allNode[i].name == "NULL")
                continue;
            if (allNode[i].name.StartsWith("d_"))
            {
                dummy.Add(allNode[i].bone);
                continue;
            }
            bones.Add(allNode[i].bone);
            bindPos.Add(allNode[i].bone.worldToLocalMatrix);
        }

        root = dummy[0];
        for (int i = 0; i < allNode.Count; i++)
            allNode[i].bone = null;
        return this;
    }
}
