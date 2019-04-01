using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : Singleton<GameConfig> {
    public string newVersion;
    public string apkUrl;
    public void LoadGrid(LitJson.JsonData json)
    {
        if (json != null && json["newVersion"] != null)
        {
            newVersion = json["newVersion"]["v"].ToString();
            apkUrl = json["newVersion"]["url"].ToString();
        }
    }
}
