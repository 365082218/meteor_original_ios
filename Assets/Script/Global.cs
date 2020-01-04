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
}
