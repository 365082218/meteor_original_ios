using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

public class Global
{
    private static Global _Instance;
    public static Global Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new Global();
            return _Instance;
        }
    }
    public bool Logined = false;
    public ServerInfo Server;//当前选择的服务器.
    public List<ServerInfo> Servers = new List<ServerInfo>();
    public float FPS = 1.0f / 30.0f;//动画设计帧率
    public float gGravity = 980;
    public const float angularVelocity = 540.0f;
    public const float RebornDelay = 15.0f;//复活队友的CD间隔
    public const float RebornRange = 125.0f;//复活队友的距离最大限制
    public const float RefreshFollowPathDelay = 5.0f;//如果跟随一个动态的目标，那么每5秒刷新一次位置
    public bool useShadowInterpolate = true;//是否使用影子跟随插值
    public bool PluginUpdated = false;//是否已成功更新过资料片配置文件
    public int MaxPlayer;
    public int RoundTime;
    public int MainWeapon;
    public int SubWeapon;
    public int PlayerLife;
    public int PlayerModel;
    public int ComboProbability = 5;//连击率
    public int SpecialWeaponProbability = 98;//100-98=2几率切换到远程武器，每次Think都有2%几率
    public float AimDegree = 30.0f;//夹角超过30度，需要重新瞄准
    public MeteorInput GMeteorInput = null;
	public Level GLevelItem = null;//普通关卡
    public LevelMode GLevelMode;//建立房间时选择的类型，从主界面进，都是Normal
    public GameMode GGameMode;//游戏玩法类型
    public Vector3[] GLevelSpawn;
    public Vector3[] GCampASpawn;
    public Vector3[] GCampBSpawn;
    public int CampASpawnIndex;
    public int CampBSpawnIndex;
    public int SpawnIndex;
    public LevelScriptBase GScript;
    public Type GScriptType;
    public System.Random Rand = new System.Random((int)DateTime.Now.ToFileTime());
    bool mPauseAll ;
    public Vector3 BodyHeight = new Vector3(0, 28, 0);
    public Chapter Chapter;
    public bool PauseAll
    {
        get { return mPauseAll; }
        set { mPauseAll = value; }
    }

    public const float ClimbLimit = 1.5f;//爬墙持续提供向上的力
    public const float JumpTimeLimit = 0.15f;//最少要跳跃这么久之后才能攀爬
    public const int LEVELSTART = 1;//初始关卡ID
    public int LEVELMAX = 29;//最大关卡29
    public const int ANGRYMAX = 100;
    public const int ANGRYBURST = 60;
    public const float AttackRangeMinD = 1225;//最小约35码
    public const float AttackRange = 8100.0f;//90 * 90换近战武器
    public const float FollowDistanceEnd = 3600.0f;//结束跟随60
    public const float FollowDistanceStart = 6400.0f;//开始跟随80
    public const int BreakChange = 3;//3%爆气几率
    public const int MaxModel = 20;//内置角色模型20个
    public void Init()
    {
        LEVELMAX = U3D.GetMaxLevel();
    }

    public void OnServiceChanged(int i, ServerInfo Info)
    {
        if (Servers == null)
            return;
        if (i == -1)
        {
            if (Servers.Contains(Info))
            {
                Servers.Remove(Info);
                if (Server == Info)
                    Server = Servers[0];
            }
        }
        else if (i == 1)
        {
            if (!Servers.Contains(Info))
                Servers.Add(Info);
        }
    }

    private List<Level> AllLevel;
    public void ClearLevel()
    {
        AllLevel = null;
    }

    public Level[] GetAllLevel()
    {
        if (AllLevel != null)
            return AllLevel.ToArray();
        if (AllLevel == null)
            AllLevel = new List<Level>();
        Level[] baseLevel = LevelMng.Instance.GetAllItem();
        for (int i = 0; i < baseLevel.Length; i++)
        {
            AllLevel.Add(baseLevel[i]);
        }

        for (int i = 0; i < GameData.Instance.gameStatus.pluginChapter.Count; i++)
        {
            baseLevel = DlcMng.Instance.GetDlcLevel(GameData.Instance.gameStatus.pluginChapter[i].ChapterId);
            for (int j = 0; j < baseLevel.Length; j++)
            {
                AllLevel.Add(baseLevel[j]);
            }
        }
        return AllLevel.ToArray();
    }

    public Level GetGlobalLevel(int mix)
    {
        int c = (mix / 1000) * 1000;
        int l = mix % 1000;
        return GetLevel(c, l);
    }

    public Level GetLevel(int chapterId, int id)
    {
        if (chapterId == 0)
        {
            Level lev = LevelMng.Instance.GetItem(id);
            if (lev != null)
                return lev;
        }

        Level[] l = DlcMng.Instance.GetDlcLevel(chapterId);
        for (int i = 0; i < l.Length; i++)
        {
            if (l[i].Id == id)
                return l[i];
        }
        Debug.LogError(string.Format("无法找到指定的剧本{0}关卡{1}", chapterId, id));
        return null;
    }

    public string GetCharacterName(int id)
    {
        if (id >= Global.MaxModel)
        {
            return DlcMng.GetPluginModel(id).Name;
        }
        return ModelMng.Instance.GetItem(id).Name;
    }

    public static DateTime JSLongToDataTime(long longTime)
    {
        long tempLong = new DateTime(1970, 1, 1).Ticks + longTime * 10000 + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours * 3600 * (long)10000000;
        return new DateTime(tempLong);
    }

    //旋转的圈圈百分比，类似网络下载时候的提示.
    public static void ShowLoadingStart()
    {
        //if (FirstLoadingController.Exist)
        //    FirstLoadingController.Instance.Show();
        //else
        //    FirstLoadingController.Instance.Open();
    }

    public static void ShowLoadingEnd()
    {
        //if (FirstLoadingController.Exist)
        //    FirstLoadingController.Instance.Close();
    }

    //必须不区分大小写字母.一些关卡原件大小写未明确.
	public static GameObject ldaControlX (string name, GameObject parent) {
        if (parent.name == name)
            return parent;
		for (int i=0; i < parent.transform.childCount; i++) {
			GameObject childObj = parent.transform.GetChild(i).gameObject;
			if(name.ToLower() == childObj.name.ToLower()){
				return childObj;
			}
			GameObject childchildObj = ldaControlX (name, childObj);
			if (childchildObj != null)
				return childchildObj;
		}
		return null;
	}

    public static Color Gray
    {
        get { return new Color(0, 0, 0); }
    }

    public static GameObject Control(string name, GameObject parent)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == name)
                return child.gameObject;
        }
        return null;
    }

    //通过物品品质获取背包物品品质框图片.
    public static string getBackgroundByQuality(int quality)
    {
        switch (quality)
        {
            case 1://蓝.
                return "icon_dk_l";
            case 2://紫.
                return "icon_dk_z";
            case 3://橙.
                return "icon_dk_c";
            case 4://红.
                return "icon_dk_h";
            case 5://金.
                return "icon_dk_hu";
            default:
                return "icon_dikuang_1235";
        }
    }

    //通过物品品质获取品质对应label框颜色值.
    public static Color getColorByQuality(int quality)
    {
        switch (quality)
        {
            case 1://蓝.
                return new Color(84f / 255f, 143f / 255f, 250f / 255f, 1f);
            case 2://紫.
                return new Color(162f / 255f, 90f / 255f, 235f / 255f, 1f);
            case 3://橙.
                return new Color(237f / 255f, 164f / 255f, 78f / 255f, 1f);
            case 4://红.
                return new Color(250f / 255f, 77f / 255f, 106f / 255f, 1f);
            case 5://金.
                return new Color(250f / 255f, 244f / 255f, 77f / 255f, 1f);
            default:
                return Color.gray;
        }
    }

    public static string GetQualityHexadecimalColor(int quality)
    {
        //hexadecimal color
        List<string> strcolorlist = new List<string>();
        strcolorlist.Add("548FFA");
        strcolorlist.Add("A25AEB");
        strcolorlist.Add("EDA44E");
        strcolorlist.Add("FA4D6A");
        strcolorlist.Add("FAF44D");

        return strcolorlist[Mathf.Max(Mathf.Min(5, quality) - 1, 0)];
    }

    public static string GetHeroQualityHexadecimalColor(int quality)
    {
        //hexadecimal color
        List<string> strcolorlist = new List<string>();
        strcolorlist.Add("548FFA");
        strcolorlist.Add("A25AEB");
        strcolorlist.Add("A25AEB");
        strcolorlist.Add("EDA44E");
        strcolorlist.Add("EDA44E");
        strcolorlist.Add("EDA44E");
        strcolorlist.Add("FA4D6A");
        strcolorlist.Add("FA4D6A");
        strcolorlist.Add("FA4D6A");
        strcolorlist.Add("FA4D6A");
        strcolorlist.Add("FAF44D");
        strcolorlist.Add("FAF44D");
        strcolorlist.Add("FAF44D");
        strcolorlist.Add("FAF44D");

        return strcolorlist[Mathf.Max(Mathf.Min(14, quality) - 1, 0)];
    }

    public static string GetHeroQualitySuffix(int quality)
    {
        List<string> herosuffix = new List<string>();
        herosuffix.Add("");
        herosuffix.Add("");
        herosuffix.Add("+1");
        herosuffix.Add("");
        herosuffix.Add("+1");
        herosuffix.Add("+2");
        herosuffix.Add("");
        herosuffix.Add("+1");
        herosuffix.Add("+2");
        herosuffix.Add("+3");
        herosuffix.Add("");
        herosuffix.Add("+1");
        herosuffix.Add("+2");
        herosuffix.Add("+3");

        return herosuffix[Mathf.Min(quality, 14) - 1];
    }

    public static string GetHeroheadQualityBgSprite(int quality)
    {
        List<string> heroheadbg = new List<string>();
        heroheadbg.Add("icon_touxiang_lan");
        heroheadbg.Add("icon_touxiang_zi");
        heroheadbg.Add("icon_touxiang_zi");
        heroheadbg.Add("icon_touxiang_cheng");
        heroheadbg.Add("icon_touxiang_cheng");
        heroheadbg.Add("icon_touxiang_cheng");
        heroheadbg.Add("icon_touxiang_hong");
        heroheadbg.Add("icon_touxiang_hong");
        heroheadbg.Add("icon_touxiang_hong");
        heroheadbg.Add("icon_touxiang_hong");
        heroheadbg.Add("icon_touxiang_jin");
        heroheadbg.Add("icon_touxiang_jin");
        heroheadbg.Add("icon_touxiang_jin");
        heroheadbg.Add("icon_touxiang_jin");

        return heroheadbg[Mathf.Min(quality, 14) - 1];
    }

    public static string GetHeroheadQualityBgSpriteEx(int quality)
    {
        List<string> heroheadbg = new List<string>();
        heroheadbg.Add("icon_touxiang_lan");
        heroheadbg.Add("icon_touxiang_zi");
        heroheadbg.Add("icon_touxiang_zi1");
        heroheadbg.Add("icon_touxiang_cheng");
        heroheadbg.Add("icon_touxiang_cheng1");
        heroheadbg.Add("icon_touxiang_cheng2");
        heroheadbg.Add("icon_touxiang_hong");
        heroheadbg.Add("icon_touxiang_hong1");
        heroheadbg.Add("icon_touxiang_hong2");
        heroheadbg.Add("icon_touxiang_hong3");
        heroheadbg.Add("icon_touxiang_jin");
        heroheadbg.Add("icon_touxiang_jin1");
        heroheadbg.Add("icon_touxiang_jin2");
        heroheadbg.Add("icon_touxiang_jin3");

        return heroheadbg[Mathf.Min(quality, 14) - 1];
    }

    public static string GetEquipmentbgByQuality(int quality)
    {
        List<string> equipbglist = new List<string>();
        equipbglist.Add("icon_dk_l");
        equipbglist.Add("icon_dk_z");
        equipbglist.Add("icon_dk_z");
        equipbglist.Add("icon_dk_c");
        equipbglist.Add("icon_dk_c");
        equipbglist.Add("icon_dk_c");
        equipbglist.Add("icon_dk_h");
        equipbglist.Add("icon_dk_h");
        equipbglist.Add("icon_dk_h");
        equipbglist.Add("icon_dk_h");
        equipbglist.Add("icon_dk_hu");
        equipbglist.Add("icon_dk_hu");
        equipbglist.Add("icon_dk_hu");
        equipbglist.Add("icon_dk_hu");

        return equipbglist[Mathf.Min(quality, 14) - 1];
    }

    public static string GetGameCommonNumberSpriteName(int number)
    {
        string spritename = "";
        if (0 > number || 10 < number) return spritename;
        string[] strNumberArray = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        return strNumberArray[number];
    }

    //游戏中使用的物品总类型
    public enum CommonItemIconType
    {
        CommonItemIconType_None,
        CommonItemIconType_Wupin,
        CommonItemIconType_Equipment,
        CommonItemIconType_Diamond,
        CommonItemIconType_Gold,
        CommonItemIconType_Xuebanshi,
        CommonItemIconType_Exp,
        CommonItemIconType_Life,
        CommonItemIconType_Jingjibi,
        CommonItemIconType_Heibi
    }

    public static string GetCommonItemIcon(CommonItemIconType type, int itemid = -1)
    {
        string icon = "";
        if (CommonItemIconType.CommonItemIconType_Wupin == type)
        {
            //if (null != ib) icon = ib.icon;
        }
        else if (CommonItemIconType.CommonItemIconType_Equipment == type)
        {
            //Equip equipbase = EquipManager.Instance.GetItem(itemid);
            //if (null != equipbase) icon = equipbase.icon;
        }
        else if (CommonItemIconType.CommonItemIconType_Diamond == type) icon = "icon_jinbi2";
        else if (CommonItemIconType.CommonItemIconType_Gold == type) icon = "icon_jinbi";
        else if (CommonItemIconType.CommonItemIconType_Xuebanshi == type) icon = "icon_zhanhun";
        else if (CommonItemIconType.CommonItemIconType_Exp == type) icon = "exp";
        else if (CommonItemIconType.CommonItemIconType_Life == type) icon = "icon_jinbi1";
        else if (CommonItemIconType.CommonItemIconType_Jingjibi == type) icon = "icon_jinbi1";
        else if (CommonItemIconType.CommonItemIconType_Heibi == type) icon = "icon_jinbi1";
        else icon = "icon_jinbi1";

        return icon;
    }

    public static string GetCommonItemIconBg(CommonItemIconType type, int itemquality = -1)
    {
        string iconbg = "";
        if (CommonItemIconType.CommonItemIconType_Wupin == type || CommonItemIconType.CommonItemIconType_Equipment == type) iconbg = GetEquipmentbgByQuality(itemquality);
        else if (CommonItemIconType.CommonItemIconType_Equipment == type
            || CommonItemIconType.CommonItemIconType_Diamond == type
            || CommonItemIconType.CommonItemIconType_Gold == type
            || CommonItemIconType.CommonItemIconType_Xuebanshi == type
            || CommonItemIconType.CommonItemIconType_Exp == type
            || CommonItemIconType.CommonItemIconType_Life == type
            || CommonItemIconType.CommonItemIconType_Jingjibi == type
            || CommonItemIconType.CommonItemIconType_Heibi == type)
        {
            iconbg = "bg_zb_di213";
        }
        else iconbg = "bg_zb_di213";

        return iconbg;
    }

    public static string GetGameCommonIcon(int type)
    {
        switch (type)
        {
            case 1:
                return "icon_jinbi2";
            case 2:
                return "icon_jinbi";
            case 3:
                return "icon_jinbi2";
            case 4:
                return "exp";
            case 5:
                return "icon_jinbi1";
            case 6:
                return "icon_jinbi";
            case 7:
                return "icon_jinbi";
            default:
                return "icon_jinbi";
        }
    }

    public static string GetMonsterColor(int quality)
    {
        if (quality < 1 || quality > 4)
            return monsterPrefix[0];
        return monsterPrefix[quality - 1];
    }

    public static bool IsNetworkAvailable { get { return Application.internetReachability != NetworkReachability.NotReachable; } }
    public static string[] monsterPrefix = { "<color=#ffffffff>", "<color=#804000ff>", "<color=#94D2F1ff>", "<color=#9933faff>"};
    public static string[] colorPrefix = { "<color=#ffffffff>", "<color=#1eff00ff>", "<color=#0081ffff>", "<color=#c600ffff>", "<color=#ff8000ff>", "<color=#e5cc80ff>" };
    public static string colorSuffix = "</color>";
}
