using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdGame
{
    public string Name;//游戏名称
    public string Url;//实际地址
    public string Preview;//预览图
    public string Desc;//描述
    public string Query;//连接到游戏得标识，用来互相给奖励.
}

public class GameNotice {
    public string newVersion;
    public string apkUrl;
    public string notice;
    public List<AdGame> moreGame = new List<AdGame>();
    public void LoadGrid(LitJson.JsonData json)
    {
        if (json != null)
        {
            if (json["newVersion"] != null)
            {
                newVersion = json["newVersion"]["v"].ToString();
                apkUrl = json["newVersion"]["url"].ToString();
            }

            if (json["moreGame"] != null)
            {
                for (int i = 0; i < json["moreGame"].Count; i++)
                {
                    AdGame g = new AdGame();
                    g.Name = json["moreGame"][i]["Name"].ToString();
                    g.Url = json["moreGame"][i]["Url"].ToString();
                    g.Preview = json["moreGame"][i]["Preview"].ToString();
                    g.Desc = json["moreGame"][i]["Desc"].ToString();
                    g.Query = json["moreGame"][i]["Query"].ToString();
                    moreGame.Add(g);
                }
            }

            if (json["Notice"] != null)
            {
                notice = json["Notice"].ToString();
            }
        }
    }

    public bool HaveNotice()
    {
        return !string.IsNullOrEmpty(notice);
    }
}
