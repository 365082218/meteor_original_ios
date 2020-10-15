using LitJson;
using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Excel2Json;

public enum FileExt {
    Jpeg = 0,
    Dll = 1,
    Txt = 2,
    Json = 3,
    Des = 4,//.des
    WayPoint = 5,//.wp
    Skc,
    Bnc,
    Amb,
    Pos,
    Png,
    Fmc,//刚体动画.fmc
    Gmc,//模型.gmc
    Gmb,//模型.gmb
    Sfx,//新特效.ef
    MP3,//新音乐
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
    public List<ReferenceItem> Res;//包含的资源，包括(角色骨架bnc,角色皮肤skc,角色动画描述pose,角色动画amb,场景描述des,场景路点wp等文件
    [ProtoMember(5)]
    public List<string> resPath = new List<string>();//解压出的所有资源.
    [ProtoMember(6)]
    public string Desc;
    [ProtoMember(7)]
    public string localPath;
    [ProtoMember(8)]
    public int level = 1;//该资料片通过的最远关卡.
    [ProtoMember(11)]
    public List<string> resCrc = new List<string>();//md5.

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
            return U3D.GetDefaultFile(Path, FileExt.Dll, true, true);
        }
    }
    public string webPreview
    {
        get
        {
            return U3D.GetDefaultFile(Path, FileExt.Jpeg, false, false);
        }
    }

    public string Preview
    {
        get
        {
            return U3D.GetDefaultFile(Path, FileExt.Jpeg, true, false);
        }
    }

    public byte[] GetResBytes(FileExt type, string iden) {
        if (Res != null) {
            for (int i = 0; i < Res.Count; i++) {
                if (Res[i].Name == iden && Res[i].Type == type)
                    return System.IO.File.ReadAllBytes(Res[i].Path);
            }
        }
        return null;
    }

    public string GetResPath(FileExt type, string iden) {
        if (Res != null) {
            for (int i = 0; i < Res.Count; i++) {
                if (Res[i].Name.ToLower() == iden.ToLower() && Res[i].Type == type)
                    return Res[i].Path;
            }
        }
        return null;
    }

    //是否已安装(dll能否正常加载,并产出具体得LevelScriptBase)
    public List<LevelData> LoadAll()
    {
        //level.json
        if (mLevel == null)
            mLevel = new DlcLevelMng(U3D.GetDefaultFile(Path, FileExt.Json, true, true));
        return mLevel.GetAllLevel();
    }

    DlcLevelMng mLevel;
    public LevelData GetLevel(int id)
    {
        if (mLevel == null)
            mLevel = new DlcLevelMng(U3D.GetDefaultFile(Path, FileExt.Json, true, true));
        return mLevel.GetLevel(id);
    }

    //public byte[] GetDes(int id) {
    //    for (int i = 0; i < )
    //}

    [ProtoMember(9)]
    public bool Installed;//是否已安装,解压了zip包.
    [ProtoMember(10)]
    public string version;//版本号，为第一次开发此插件时，客户端解析该格式对应的客户端版本
    public void Check()
    {
        if (resPath == null || resCrc == null) {
            Installed = false;
            return;
        }
        for (int j = 0; j < resPath.Count; j++)
        {
            if (!System.IO.File.Exists(resPath[j]) || !Utility.SameCrc(resPath[j], resCrc[j]))
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
            for (int j = 0; j < resPath.Count; j++)
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

//剧本有自己的数据库.
[ProtoContract]
public class ReferenceItem {
    [ProtoMember(1)]
    public string Name;//sn01.wp=>wn01
    [ProtoMember(2)]
    public string Path;//路径
    [ProtoMember(3)]
    public FileExt Type;//路径
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
    [ProtoMember(4)]
    public string localPath;//zip路径
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
    }//本地存储路径
    [ProtoMember(10)]
    public List<string> resPath = new List<string>();//压缩包内含有的所有资源
    [ProtoMember(6)]
    public string Desc;//描述
    //内存/存档中取
    [ProtoMember(7)]
    public bool Installed;//是否已安装,解压了zip包.
    [ProtoMember(8)]
    public bool useFemalePos;//使用女性角色动作
    [ProtoMember(9)]
    public List<string> resCrc = new List<string>();//资源的MD5.
    public void Check()
    {
        Installed = true;
        if (resCrc == null || resPath == null || resPath.Count == 0 || resCrc.Count == 0) {
            Installed = false;
            return;
        }
        for (int j = 0; j < resPath.Count; j++)
        {
            if (!System.IO.File.Exists(resPath[j]) || !Utility.SameCrc(resPath[j], resCrc[j]))
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
            for (int j = 0; j < resPath.Count; j++)
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