using LitJson;
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
public class ModelItem
{
    //json里取
    public string Name;
    public int ModelId;
    public string Path;//资源路径,安装
    public string LocalPath
    {
        get
        {
            return Application.persistentDataPath + "Plugins/" + Path; 
        }
    }//本地存储路径，由
    public string IcoPath;//预览图,Icon/名称.jpg
    public string Desc;//描述
    //内存/存档中取
    public bool Installed;//是否已安装,解压了zip包.
    public bool Downloaded;//是否下载了zip包.
}

//管理外置的模型等下载安装使用等.
public class ModelPlugin:Singleton<ModelPlugin>
{
    public List<ModelItem> Models = new List<ModelItem>();
    //安装外置的角色包.
    public void SetupCharacter(int characterIdx)
    {
        for (int i = 0; i < Models.Count; i++)
        {
            if (Models[i].ModelId != characterIdx)
                continue;
            if (!Models[i].Downloaded)
            {
                Models[i].Downloaded = true;
                
            }
        }
    }

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
