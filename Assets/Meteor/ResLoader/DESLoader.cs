using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class DesLoader
{
    Dictionary<string, DesFile> DesFile = new Dictionary<string, global::DesFile>();
    public DesFile Load(string file)
    {
        string file_no_ext = file;
        file += ".des";
        if (DesFile.ContainsKey(file))
            return DesFile[file];

        DesFile f = null;
        if (Main.Ins.CombatData.Chapter != null) {
            string path = Main.Ins.CombatData.Chapter.GetResPath(FileExt.Des, file_no_ext);
            if (!string.IsNullOrEmpty(path)) {
                f = new DesFile();
                f.Load(path);
                DesFile[file] = f;
                return f;
            }
        }

        f = new DesFile();
        f.Load(file);
        DesFile[file] = f;
        return f;
    }

    public void Clear() { DesFile.Clear(); }
}

public class DesItem
{
    public string name;
    public FixVector3 pos = new FixVector3((Fix64)0, (Fix64)0, (Fix64)0);
    public FixQuaternion quat = new FixQuaternion((Fix64)0, (Fix64)0, (Fix64)0, (Fix64)0);
    public bool useTextAnimation; //是否使用uv动画
    public FixVector2 textAnimation = new FixVector2(0, 0);//uv参数
    public List<string> custom = new List<string>();
    public bool ContainsKey(string key, out string value)
    {
        for (int i = 0; i < custom.Count; i++)
        {
            string []kv = custom[i].Split(new char[] { '='}, System.StringSplitOptions.RemoveEmptyEntries);
            if (kv.Length == 2)
            {
                string k = kv[0].Trim(new char[] { ' '});
                string v = kv[1].Trim(new char[] { ' ' });
                if (k == key)
                {
                    string[] varray = v.Split(new char[] { '\"' }, System.StringSplitOptions.RemoveEmptyEntries);
                    value = varray[0].Trim(new char[] { ' '});
                    return true;
                }
            }
        }
        value = "";
        return false;
    }
    public void ReadObjAttr(StreamReader asset)
    {
        bool readLeftToken = false;
        int leftTokenStack = 0;
        while (!asset.EndOfStream)
        {
            string obj = asset.ReadLine().Trim();
            string[] keyValue = obj.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (keyValue.Length == 0)
                continue;
            if (keyValue[0] == "#")
                continue;
            if (keyValue[0] == "{")
            {
                readLeftToken = true;
                leftTokenStack++;
                continue;
            }
            //Z UP TO Y UP x轴z轴取反
            if (keyValue[0] == "Position:" && readLeftToken && leftTokenStack == 1)
            {
                pos.x = (Fix64)float.Parse(keyValue[1]);
                pos.z = (Fix64)float.Parse(keyValue[2]);
                pos.y = (Fix64)float.Parse(keyValue[3]);
            }
            if (keyValue[0] == "Quaternion:" && readLeftToken && leftTokenStack == 1)
            {
                quat.w = (Fix64)float.Parse(keyValue[1]);
                quat.x = -(Fix64)float.Parse(keyValue[2]);
                quat.y = -(Fix64)float.Parse(keyValue[4]);
                quat.z = -(Fix64)float.Parse(keyValue[3]);
            }
            if (keyValue[0] == "TextureAnimation:" && readLeftToken && leftTokenStack == 1)
            {
                useTextAnimation = (int.Parse(keyValue[1]) == 1);
                textAnimation.x = (Fix64)float.Parse(keyValue[2]);
                textAnimation.y = (Fix64)float.Parse(keyValue[3]);
            }
            if (keyValue[0] == "Custom:" && readLeftToken && leftTokenStack == 1)
            {
                while (!asset.EndOfStream)
                {
                    obj = asset.ReadLine().Trim();
                    if (obj == "{")
                    {
                        leftTokenStack++;
                        continue;
                    }
                    if (obj == "}")
                    {
                        leftTokenStack--;
                        if (leftTokenStack == 1)
                            break;
                        continue;
                    }
                    if (obj == "#" || string.IsNullOrEmpty(obj))
                        continue;
                    custom.Add(obj);
                }
            }
            if (keyValue[0] == "}")
            {
                leftTokenStack--;
                if (leftTokenStack == 0)
                    break;
            }
        }
    }
}
public class DesFile
{
    public List<DesItem> SceneItems = new List<DesItem>();
    public int DummyCount;
    public int ObjectCount;
    public void Load(string strFile)
    {
        SceneItems.Clear();
        if (!string.IsNullOrEmpty(strFile))
        {
            TextAsset assetDes = Resources.Load<TextAsset>(strFile);
            byte[] body = null;
            if (assetDes == null){
                if (File.Exists(strFile))
                    body = File.ReadAllBytes(strFile);
            }
            else {
                body = assetDes.bytes;
            }
            if (body == null)
                return;
            MemoryStream ms = new MemoryStream(body);
            StreamReader asset = new StreamReader(ms);
            while (!asset.EndOfStream)
            {
                string obj = asset.ReadLine();
                string[] keyValue = obj.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length == 0)
                {
                    Debug.LogError("err：" + obj);
                    continue;
                }
                if (keyValue[0] == "#")
                    continue;
                if (keyValue[0] == "SceneObjects" && keyValue[2] == "DummeyObjects")
                {
                    ObjectCount = int.Parse(keyValue[1]);
                    DummyCount = int.Parse(keyValue[3]);
                    continue;
                }

                if (keyValue[0] == "Object")
                {
                    DesItem attr = new DesItem();
                    attr.name = keyValue[1];
                    attr.ReadObjAttr(asset);
                    SceneItems.Add(attr);
                }
            }
            //Debug.LogFormat("共有{0}个对象{1}个虚拟物", ObjectCount, DummyCount);
            if (SceneItems.Count != ObjectCount + DummyCount)
            {
                Debug.Log(string.Format("物品数量不对, 场景物件数量{0}-对象{1}-拷贝对象{2}", SceneItems.Count, ObjectCount, DummyCount));
                return;
            }
        }
    }
}
