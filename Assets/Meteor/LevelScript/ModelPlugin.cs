using LitJson;
using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ProtoContract]
public class Dependence
{
    public List<int> scene;
    public List<int> model;
    public List<int> weapon;
}

//外接DLC剧本，对应plugins.json里的dlc其中的一个dll，一个dll又包含数10个关卡.
[ProtoContract]
public class Chapter
{
    [ProtoMember(1)]
    public string Name;
    [ProtoMember(2)]
    public int ChapterId;
    [ProtoMember(3)]
    public string Path;//zip路径
    [ProtoMember(4)]
    public Dependence Res;//依赖资源，包括Model,地图(不存在基础场景内的)
    [ProtoMember(5)]
    public string[] resPath;//解压出的所有资源.
    public string Preview
    {
        get
        {
            return GetDefaultFile(0);
        }
    }

    public string GetDefaultFile(int type)
    {
        string suffix = "";
        switch (type)
        {
            case 0:suffix = ".png";
                break;
            case 1:suffix = ".dll";
                break;
            case 2:suffix = ".txt";
                break;
        }
        return Application.persistentDataPath + "/" + Path.Substring(0, Path.Length - 4) + suffix;//.zip => .png
    }

    //是否已安装(dll能否正常加载,并产出具体得LevelScriptBase)
    public Level[] LoadAll()
    {
        //level.txt
        DlcLevelMng l = new DlcLevelMng(GetDefaultFile(2));
        return l.GetAllItem();
    }

    public Level GetItem(int id)
    {
        DlcLevelMng l = new DlcLevelMng(GetDefaultFile(2));
        return l.GetItem(id);
    }

    [ProtoMember(8)]
    public bool Installed;//是否已安装,解压了zip包.

    public void Check()
    {
        for (int j = 0; j < resPath.Length; j++)
        {
            if (!System.IO.File.Exists(resPath[j]))
            {
                Installed = false;
                break;
            }
        }
    }
    
}

//外接得模型
[ProtoContract]
public class ModelItem
{
    //json里取
    [ProtoMember(1)]
    public string Name;
    [ProtoMember(2)]
    public int ModelId;
    [ProtoMember(3)]
    public string Path;//资源路径,安装
    //zip路径
    [ProtoMember(4)]
    public string localPath;
    public string LocalPath
    {
        get
        {
            if (!string.IsNullOrEmpty(localPath))
                return localPath;
            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/Plugins/Model/"))
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Plugins/Model/");
            localPath = Application.persistentDataPath + "/Plugins/" + Path;
            return localPath; 
        }
    }//本地存储路径，由
    [ProtoMember(5)]
    public string[] resPath;//压缩包内含有的所有资源
    [ProtoMember(6)]
    public string IcoPath;//预览图,Icon/名称.jpg
    [ProtoMember(7)]
    public string Desc;//描述
    //内存/存档中取
    [ProtoMember(8)]
    public bool Installed;//是否已安装,解压了zip包.

    public void Check()
    {
        for (int j = 0; j < resPath.Length; j++)
        {
            if (!System.IO.File.Exists(resPath[j]))
            {
                Installed = false;
                break;
            }
        }
    }
}

//管理外置的模型等下载安装使用等.
public class ModelPlugin:Singleton<ModelPlugin>
{
    public List<ModelItem> Models = new List<ModelItem>();
    public void ClearModel()
    {
        Models.Clear();
    }

    //加入从json里获取到得一项资源
    public void AddModel(ModelItem Info)
    {
        Debug.Log("增加外部角色:" + Info.Name);
        Models.Add(Info);
    }
}
