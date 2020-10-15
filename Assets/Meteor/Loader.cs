using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEngine;
using ProtoBuf;
using System.Xml;
using protocol;

//加载关卡内的虚拟物品，类似原来流星蝴蝶剑的箱子/酒坛，或者之类的
public class Loader : MonoBehaviour {
    //public List<MapObject> mapObjectList = new List<MapObject>();
    //创建1-16 USER teamA teamB的位置
    public void CreateSpawnPoint() {
        for (int i = 1; i <= 16; i++) {
            string s = string.Format("D_user{0:d2}", i);
            GameObject obj = new GameObject(s);
        }

        for (int i = 1; i <= 8; i++) {
            string s = string.Format("D_teamA{0:d2}", i);
            GameObject obj = new GameObject(s);
        }

        for (int i = 1; i <= 8; i++) {
            string s = string.Format("D_teamB{0:d2}", i);
            GameObject obj = new GameObject(s);
        }
    }

    public string defFile;
    public void LoadSceneObjsFromDes(string des)
    {
        //mapObjectList.Clear();
        DesLoader.Ins.Clear();
        DesFile desDat = DesLoader.Ins.Load(des);
        int j = 0;
        for (int i = desDat.ObjectCount; i < desDat.SceneItems.Count; i++)
        {
            //不加载爆出物品。待打碎物品后，随机爆出物品
            //不加载属性灯.
            //不加载固有物件.
            if (desDat.SceneItems[i].name.StartsWith("D_user") || 
                desDat.SceneItems[i].name.StartsWith("D_team") || 
                desDat.SceneItems[i].name.StartsWith("D_User"))
            {
                j++;
                continue;
            }
            //环境特效.一直存在的特效.
            string effect;
            if (desDat.SceneItems[i].ContainsKey("effect", out effect))
            {
                j++;
                continue;
            }

            if (desDat.SceneItems[i].name.StartsWith("D_wp") ||
                desDat.SceneItems[i].name.StartsWith("D_it") ||
                desDat.SceneItems[i].name.StartsWith("D_It"))
            {
                j++;
                continue;
            }

            string type;
            bool active = false;
            if (desDat.SceneItems[i].ContainsKey("ticket", out type))
            {
                //剧情模式出现
                string[] subtype = type.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int t = 0; t < subtype.Length; t++)
                    if (int.Parse(subtype[t]) == 6)
                    {
                        active = true;
                        break;
                    }

            }
            else
                active = true;

            if (!active)
            {
                j++;
                continue;
            }
            GameObject obj = new GameObject();
            obj.name = desDat.SceneItems[i].name;
            string @model = "";
            if (desDat.SceneItems[i].ContainsKey("model", out model))
            {
                if (!string.IsNullOrEmpty(model))
                {
                    Utility.ShowMeteorObject(model, obj.transform);
                }
            }
            string Iden;
            if (desDat.SceneItems[i].ContainsKey("name", out Iden))
            {
                //隐藏属性
                //names.Add(Iden);//类似teamB12 蝴蝶阵营 12出生点 machine->固定不旋转
            }
            if (desDat.SceneItems[i].ContainsKey("billboard", out Iden))
            {
                //billBoard = (Iden == "1");
            }

            obj.transform.position = desDat.SceneItems[i].pos;
            obj.transform.rotation = desDat.SceneItems[i].quat;
            obj.transform.SetParent(transform);
            j++;
        }
    }

    //重新加载关卡配置.
	//public void LoadSceneObjs(int level)
	//{
 //       Items.Clear();
 //       mapObjectList.Clear();
 //       Level lev = LevelMng.Instance.GetItem(level);
 //       DesFile des = DesLoader.Instance.Load(lev.goodList);
 //       TextAsset objlst = Resources.Load<TextAsset>(lev.goodList + ".lst");
 //       List<RealObject> objectList = new List<RealObject>();
 //       if (objlst != null)
 //           objectList = Serializer.Deserialize<List<RealObject>>(new MemoryStream(objlst.bytes));
 //       int j = 0;
 //       for (int i = des.ObjectCount; i < des.SceneItems.Count; i++)
 //       {
 //           //一些特殊物件不需要加脚本，只相当于环境，也不用保存,只设置位置.
 //           if (des.SceneItems[i].name.StartsWith("D_user") || des.SceneItems[i].name.StartsWith("D_team"))
 //           {
 //               j++;
 //               continue;
 //           }
 //           //环境特效.一直存在的特效.
 //           string effect;
 //           if (des.SceneItems[i].ContainsKey("effect", out effect))
 //           {
 //               j++;
 //               continue;
 //           }

 //           //不加载爆出物品。待打碎物品后，随机爆出物品
 //           //不加载属性灯.
 //           if (des.SceneItems[i].name.StartsWith("D_wp") ||
 //               des.SceneItems[i].name.StartsWith("D_it") ||
 //               des.SceneItems[i].name.StartsWith("D_It"))
 //           {
 //               j++;
 //               continue;
 //           }
 //           //是否在允许加载列表里.
 //           if (objectList != null)
 //           {
 //               bool find = false;
 //               for (int x = 0; x < objectList.Count; x++)
 //               {
 //                   if (objectList[x].name == des.SceneItems[i].name)
 //                   {
 //                       find = true;
 //                       break;
 //                   }
 //               }
 //               if (!find)
 //               {
 //                   j++;
 //                   continue;
 //               }
 //           }
 //           GameObject obj = new GameObject();
 //           //可破坏物件，可拾取物件，等都放在此处.
 //           MapUnitCtrl it = obj.AddComponent<MapUnitCtrl>();
 //           it.InitDesItem(level, j);
 //           if (it.gameObject.activeSelf)
 //           {
 //               Items.Add(it);
 //               mapObjectList.Add(it.Item);
 //               obj.transform.SetParent(transform);
 //           }
 //           else
 //               DestroyObject(obj);
 //           j++;
 //       }
 //   }

    public int LevelId;
    public static Loader Instance;
    List<string> blacklist = new List<string>();
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        //外传有些场景模型不存在
        if (blacklist.Count == 0) {
            for (int i = 0; i < 10; i++) {
                blacklist.Add(i.ToString());
            }
        }
        //单独场景测试时，直接加载
        //if (Main.Instance == null)
        //    LoadSceneObjs(LevelId);
    }
    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    //从指定数据恢复一个场景元件
    //public void RestoreTrigger(MapObject desObject)
    //{
    //    GameObject obj = new GameObject();
    //    MapUnitCtrl it = obj.AddComponent<MapUnitCtrl>();
    //    it.RestoreTrigger(desObject);
    //    obj.transform.SetParent(transform);
    //}

    //静态固有物件。类似声音，位置点
    public void LoadFixedScene(string sceneItems)
    {
        DesFile des = DesLoader.Ins.Load(sceneItems);
        for (int i = des.ObjectCount; i < des.SceneItems.Count; i++)
        {
            //一些特殊物件不需要加脚本，只相当于环境，也不用保存,只设置位置.
            if (des.SceneItems[i].name.StartsWith("D_user") || 
                des.SceneItems[i].name.StartsWith("D_team") ||
                des.SceneItems[i].name.StartsWith("D_User"))
            {
                GameObject obj = new GameObject();
                obj.transform.position = des.SceneItems[i].pos;
                obj.transform.rotation = des.SceneItems[i].quat;
                obj.name = des.SceneItems[i].name;
                obj.transform.SetParent(transform);
                continue;
            }
        }
    }

    public void LoadDynamicTrigger(string sceneItems)
    {
        //return;
        //mapObjectList.Clear();
        DesFile des = DesLoader.Ins.Load(sceneItems);
        for (int i = des.ObjectCount; i < des.SceneItems.Count; i++)
        {
            //一些特殊物件不需要加脚本，只相当于环境，也不用保存,只设置位置.
            if (des.SceneItems[i].name.StartsWith("D_user") ||
                des.SceneItems[i].name.StartsWith("D_team") ||
                des.SceneItems[i].name.StartsWith("D_User"))
                continue;
            string type;
            bool active = false;
            if (des.SceneItems[i].ContainsKey("ticket", out type))
            {
                string[] subtype = type.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int t = 0; t < subtype.Length; t++)
                {
                    if (int.Parse(subtype[t]) == (int)CombatData.Ins.GGameMode)
                    {
                        active = true;
                        break;
                    }
                }
            }
            else
                active = true;
            if (!active)
                continue;
            GameObject obj = new GameObject();
            obj.name = des.SceneItems[i].name;
            string model;
            if (des.SceneItems[i].ContainsKey("model", out model))
            {
                if (!string.IsNullOrEmpty(model))
                {
                    //先检查这个模型是否存在，1：安装包内带有资源 2：外传资源里带有资源,
                    //若都不存在，则忽略这个物件
                    if (blacklist.IndexOf(model) != -1)
                        continue;
                    SceneItemAgent target = obj.AddComponent<SceneItemAgent>();
                    target.tag = "SceneItemAgent";
                    target.Load(model);
                    target.LoadCustom(des.SceneItems[i].name, des.SceneItems[i].custom);//自定义的一些属性，name=damage100
                    target.ApplyPost();
                    if (target.root != null)
                        target.root.gameObject.SetActive(active);
                }
            }
            //环境特效.一直存在的特效.和特效挂载点
            string effect;
            if (des.SceneItems[i].ContainsKey("effect", out effect))
                SFXLoader.Ins.PlayEffect(string.Format("{0}.ef", effect), obj);
            obj.transform.position = des.SceneItems[i].pos;
            obj.transform.rotation = des.SceneItems[i].quat;
            obj.transform.SetParent(transform);
        }
    }
}
