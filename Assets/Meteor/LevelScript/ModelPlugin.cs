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

//管理外置的模型等下载安装使用等.
public class ModelPlugin:Singleton<ModelPlugin>
{
    HttpClient client;//下载器
    List<DownloadContext> context = new List<DownloadContext>();
    public void Initialize()
    {

    }

    //安装外置的角色包.
    
    public void SetupCharacter(int characterIdx)
    {

    }
}
