using UnityEngine;
using System.Collections;
using UnityEditor;
using ProtoBuf;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Idevgame.Util;


public class BuildLevelBytes
{
    [ProtoContract]
    public class SceneObjAttr
    {
        [ProtoMember(1)]
        public string name;
        [ProtoMember(2)]
        public MyVector pos = new MyVector(0, 0, 0);
        [ProtoMember(3)]
        public MyQuaternion quat = new MyQuaternion(0, 0, 0, 0);
        [ProtoMember(4)]
        public bool useTextAnimation; //是否使用uv动画
        [ProtoMember(5)]
        public MyVector2 textAnimation = new MyVector2(0, 0);//uv参数
        [ProtoMember(6)]
        public List<string> custom = new List<string>();
        public void ReadObjAttr(StreamReader asset)
        {
            bool readLeftToken = false;
            int leftTokenStack = 0;
            while (!asset.EndOfStream)
            {
                string obj = asset.ReadLine();
                string[] keyValue = obj.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
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
                    pos.x = -float.Parse(keyValue[1]);
                    pos.z = -float.Parse(keyValue[2]);
                    pos.y = float.Parse(keyValue[3]);
                }
                if (keyValue[0] == "Quaternion:" && readLeftToken && leftTokenStack == 1)
                {
                    quat.w = float.Parse(keyValue[1]);
                    quat.x = float.Parse(keyValue[2]);
                    quat.z = float.Parse(keyValue[3]);
                    quat.y = float.Parse(keyValue[4]);
                }
                if (keyValue[0] == "TextureAnimation:" && readLeftToken && leftTokenStack == 1)
                {
                    useTextAnimation = (int.Parse(keyValue[1]) == 1);
                    textAnimation.x = float.Parse(keyValue[2]);
                    textAnimation.y = float.Parse(keyValue[3]);
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

    //加载原流星场景物件des文件
    //[MenuItem("Meteor/SceneMgr/LoadSceneDes")]
    //public static void LoadDesFile()
    //{
    //    string strFile = EditorUtility.OpenFilePanel("选择流星关卡描述文件(*.des.bytes)", "D:/Meteor/", "bytes");
    //    if (string.IsNullOrEmpty(strFile))
    //        return;

    //    FileInfo fi = new FileInfo(strFile);
    //    GameObject root = GameObject.Find("SceneObjs");
    //    if (root == null)
    //        root = new GameObject("SceneObjs");
    //    List<SceneObjAttr> objs = new List<SceneObjAttr>();
    //    int objCount = 0;
    //    int DummyCount = 0;
    //    if (!string.IsNullOrEmpty(strFile))
    //    {
    //        StreamReader asset = File.OpenText(strFile);
    //        while (!asset.EndOfStream)
    //        {
    //            string obj = asset.ReadLine();
    //            string[] keyValue = obj.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
    //            if (keyValue[0] == "#")
    //                continue;
    //            if (keyValue[0] == "SceneObjects" && keyValue[2] == "DummeyObjects")
    //            {
    //                objCount = int.Parse(keyValue[1]);
    //                DummyCount = int.Parse(keyValue[3]);
    //                continue;
    //            }

    //            if (keyValue[0] == "Object")
    //            {
    //                SceneObjAttr attr = new SceneObjAttr();
    //                attr.name = keyValue[1];
    //                attr.ReadObjAttr(asset);
    //                objs.Add(attr);
    //            }
    //        }
    //        Debug.LogFormat("共有{0}个对象{1}个虚拟物", objCount, DummyCount);
    //        if (objs.Count != objCount + DummyCount)
    //        {
    //            Debug.Log("物品数量不对");
    //            return;
    //        }
    //        for (int i = 0; i < objs.Count; i++)
    //        {
    //            GameObject obj = new GameObject(objs[i].name);
    //            obj.transform.parent = root.transform;
    //            obj.transform.rotation = objs[i].quat;
    //            obj.transform.position = objs[i].pos;
    //        }
    //        string file = "Assets/Resources/" + fi.Name.Substring(0, fi.Name.Length - 4) + ".bytes";
    //        if (File.Exists(file))
    //        {
    //            if (!EditorUtility.DisplayDialog("提示", "要覆盖:\n" + file + "吗", "是", "否"))
    //                return;
    //        }
    //        File.Delete(file);
    //        FileStream fs = File.Open(file, FileMode.CreateNew, FileAccess.ReadWrite);
    //        Serializer.Serialize<List<SceneObjAttr>>(fs, objs);
    //        fs.Close();
    //    }
    //}


    [MenuItem("Meteor/SceneMgr/SaveColliderFile")]
    public static void SaveColliderFile()
    {
        if (Selection.activeObject as GameObject == null)
            return;
        if (Selection.activeObject != null)
        {
            string strFile = EditorUtility.SaveFilePanelInProject("保存角色受击碰撞列表", "boxdef", "bytes", "保存选择对象上的碰撞盒列表");
            BoxCollider[] co = (Selection.activeObject as GameObject).GetComponentsInChildren<BoxCollider>();
            List<BoxColliderDef> save = new List<BoxColliderDef>();
            for (int i = 0; i < co.Length; i++)
            {
                BoxColliderDef def = new BoxColliderDef();
                def.center = co[i].center;
                def.size = co[i].size;
                def.name = co[i].name;
                save.Add(def);
            }
            if (File.Exists(strFile))
            {
                if (!EditorUtility.DisplayDialog("提示", "要覆盖:\n" + strFile + "吗", "是", "否"))
                    return;
            }
            File.Delete(strFile);
            FileStream fs = File.Open(strFile, FileMode.CreateNew, FileAccess.ReadWrite);
            Serializer.Serialize<List<BoxColliderDef>>(fs, save);
            fs.Close();
            AssetDatabase.Refresh();
        }
    }
}
