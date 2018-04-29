using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

public class Global
{
    public const string AppVersion = "0.4.3";
    public const int Version = 20180411;
    public static string MeteorVersion = "9.07";
    //运行帧速率设置 60 = 12 30 = 6 120 = 24
#if UNITY_IOS || UNITY_ANDROID
    public static int waitForNextInput = 9;//2个输入中间最大间隔6帧超过即断开.
    public static int targetFrame = 30;
#elif UNITY_EDITOR
    public static int waitForNextInput = 12;//2个输入中间最大间隔24帧超过即断开.
    public static int targetFrame = 120;
#endif
    public static string[] model = new string[] { "孟星魂", "冷燕", "铁胡子", "夜猫子", "破空", "冯浩", "孙剑", "萧安", "子興", "王强", "无名", "叶翔", "小何", "凤凤", "石群", "屠城", "范旋", "高老大", "孙玉伯", "律香川"};//"骷髅哥", "骷髅哥2", "火焰骷髅王", "僵尸2"
    public static MeteorInput GMeteorInput = null;
	public static Level GLevelItem = null;
    public static System.Random Rand = new System.Random((int)DateTime.Now.ToFileTime());
	static bool mPauseAll ; 
    public static bool PauseAll
	{
		get{return mPauseAll;}
		set
		{
			mPauseAll = value;
		}
	}
	public static bool PauseMosterAI = false;
	public static bool PauseProtection = false;
    public const float ClimbLimit = 1.5f;//爬墙上限
    public const float JumpTimeLimit = 0.25f;//最少要跳跃这么久之后才能攀爬
    public const int LEVELSTART = 1;//初始关卡ID
    public const int LEVELMAX = 99;//角色级别上限
    public const int ANGRYMAX = 100;

	public static float timeScale
	{
		get {return Time.timeScale;}

		set
		{
			Time.timeScale = value;
		}
	}

    public static DateTime JSLongToDataTime(long longTime)
    {
        long tempLong = new DateTime(1970, 1, 1).Ticks +
            longTime * 10000 + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours * 3600 * (long)10000000;
        return new DateTime(tempLong);
    }

    //旋转的圈圈百分比，类似网络下载时候的提示.
    public static void ShowLoadingStart()
    {
        if (EmptyForLoadingWnd.Exist)
            EmptyForLoadingWnd.Instance.Show();
        else
            EmptyForLoadingWnd.Instance.Open();
    }

    public static void ShowLoadingEnd()
    {
        if (EmptyForLoadingWnd.Exist)
            EmptyForLoadingWnd.Instance.Close();
    }

    //不区分大小写字母.
	public static GameObject ldaControlX (string name, GameObject parent) {
        if (parent.name == name)
            return parent;
		for (int i=0; i < parent.transform.childCount; i++) {
			GameObject childObj = parent.transform.GetChild(i).gameObject;
			if(name.ToLower() == childObj.name.ToLower()){
				return childObj;
			}
			GameObject childchildObj = ldaControlX (name, childObj);
			if(childchildObj != null)
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

    //new Color(148f/255f, 210f/255f, 241f/255f), new Color(153f/255f,51f/255f,250f/255f) };
    public static string GetMonsterColor(int quality)
    {
        if (quality < 1 || quality > 4)
            return monsterPrefix[0];
        return monsterPrefix[quality - 1];
    }
    
    public static string[] monsterPrefix = { "<color=#ffffffff>", "<color=#804000ff>", "<color=#94D2F1ff>", "<color=#9933faff>"};
    public static string[] colorPrefix = { "<color=#ffffffff>", "<color=#1eff00ff>", "<color=#0081ffff>", "<color=#c600ffff>", "<color=#ff8000ff>", "<color=#e5cc80ff>" };
    public static string colorSuffix = "</color>";
}
