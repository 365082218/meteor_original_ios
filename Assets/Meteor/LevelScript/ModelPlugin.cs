using LitJson;
using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ProtoContract]
public class Dependence
{
    [ProtoMember(1)]
    public List<int> scene;
    [ProtoMember(2)]
    public List<int> model;
    [ProtoMember(3)]
    public List<int> weapon;
}

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class NpcTemplate
{
    public string npcTemplate;
    public string filePath;
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
    public string Path;//zip路径-非本地路径
    [ProtoMember(4)]
    public Dependence Res;//依赖资源，包括Model,地图(不存在基础场景内的)
    [ProtoMember(5)]
    public string[] resPath;//解压出的所有资源.
    [ProtoMember(6)]
    public string Desc;
    [ProtoMember(7)]
    public string localPath;
    [ProtoMember(8)]
    public int level = 1;//该资料片通过的最远关卡.

    public string LocalPath
    {
        get
        {
            if (!string.IsNullOrEmpty(localPath))
                return localPath;
            localPath = Application.persistentDataPath + "/Plugins/" + Path;
            return localPath;
        }
    }//本地存储路径，由
    public string Dll
    {
        get
        {
            return U3D.GetDefaultFile(Path, 1, true, true);
        }
    }
    public string webPreview
    {
        get
        {
            return U3D.GetDefaultFile(Path, 0, false, false);
        }
    }

    public string Preview
    {
        get
        {
            return U3D.GetDefaultFile(Path, 0, true, false);
        }
    }

    //是否已安装(dll能否正常加载,并产出具体得LevelScriptBase)
    public List<LevelDatas.LevelDatas> LoadAll()
    {
        //level.txt
        if (mLevel == null)
            mLevel = new DlcLevelMng(U3D.GetDefaultFile(Path, 2, true, true));
        return mLevel.GetAllLevel();
    }

    DlcLevelMng mLevel;
    public LevelDatas.LevelDatas GetItem(int id)
    {
        if (mLevel == null)
            mLevel = new DlcLevelMng(U3D.GetDefaultFile(Path, 2, true, true));
        return mLevel.GetLevel(id);
    }

    [ProtoMember(9)]
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

    public void CleanRes()
    {
        if (resPath != null)
        {
            for (int j = 0; j < resPath.Length; j++)
            {
                if (System.IO.File.Exists(resPath[j]))
                {
                    System.IO.File.Delete(resPath[j]);
                }
            }
            resPath = null;
        }
        if (System.IO.File.Exists(LocalPath))
            System.IO.File.Delete(LocalPath);
        Installed = false;
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
    [ProtoMember(5)]
    public string localDir;
    public string LocalPath
    {
        get
        {
            if (!string.IsNullOrEmpty(localPath))
                return localPath;
            localPath = Application.persistentDataPath + "/Plugins/" + Path;
            localDir = localPath.Substring(0, localPath.Length - ".zip".Length);
            return localPath; 
        }
    }//本地存储路径，由
    [ProtoMember(5)]
    public string[] resPath;//压缩包内含有的所有资源
    [ProtoMember(6)]
    public string Desc;//描述
    //内存/存档中取
    [ProtoMember(7)]
    public bool Installed;//是否已安装,解压了zip包.
    [ProtoMember(8)]
    public bool useFemalePos;//使用女性角色动作
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
    public string webPreview
    {
        get
        {
            return U3D.GetDefaultFile(Path, 0, false, false);
        }
    }

    //预览图都在zip文件同一级，压缩包内的文件在解压出的文件夹下
    public string Preview
    {
        get
        {
            return U3D.GetDefaultFile(Path, 0, true, false);
        }
    }

    public void CleanRes()
    {
        if (resPath != null)
        {
            for (int j = 0; j < resPath.Length; j++)
            {
                if (System.IO.File.Exists(resPath[j]))
                {
                    System.IO.File.Delete(resPath[j]);
                }
            }
        }
        if (System.IO.File.Exists(LocalPath))
            System.IO.File.Delete(LocalPath);
        if (System.IO.Directory.Exists(localDir))
            System.IO.Directory.Delete(localDir);
        Installed = false;
    }
}