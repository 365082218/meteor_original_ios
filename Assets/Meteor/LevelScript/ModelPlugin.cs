using LitJson;
using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//下载上下文
public class DownloadContext
{
    public int type;//资源种类
    public int idx;//资源唯一编号
    public JsonData JsSource;//详细信息 
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
