using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using protocol;
using System;
using UnityEngine.SceneManagement;

public class NetWorkBattle : MonoBehaviour {

    //在房间的玩家.
    Dictionary<int, protocol.Player_> playerInfo = new Dictionary<int, Player_>();
    Dictionary<int, MeteorUnit> player = new Dictionary<int, MeteorUnit>();
    SceneInfo scene;
    public int RoomId = -1;//房间在服务器的编号
    public int LevelId = -1;//房间场景关卡编号
    public int PlayerId = -1;//主角在服务器的角色编号.
    public int heroIdx;//选择的模型编号.
    public int weaponIdx;
    string RoomName;
    bool bSync;
    int FrameIndex;
    int ServerFrameIndex;
    static NetWorkBattle _Ins;
    public static NetWorkBattle Ins { get { return _Ins; } }
    // Use this for initialization
    private void Awake()
    {
        if (_Ins == null)
            _Ins = this;    
    }

    void Start () {
        RoomId = -1;
        RoomName = "";
        bSync = false;
	}
	
    public string GetPlayerName(int id)
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].InstanceId == id)
                return MeteorManager.Instance.UnitInfos[i].name;
        }
        return "不明身份者";
    }
    //等待服务器帧同步.
    void Update () {
        if (bSync)
        {
            while (FrameIndex < ServerFrameIndex)
            {
                //foreach (var each in player)
                //    each.Value.GameFrame(FrameIndex);
                //GameBattleEx.Instance.GameFrame(FrameIndex);
                FrameIndex++;
            }
        }
	}

    public void OnDisconnect()
    {
        if (GameBattleEx.Instance && RoomId != -1)
        {
            //在联机战斗场景中.
            GameBattleEx.Instance.Pause();
            SoundManager.Instance.StopAll();
            BuffMng.Instance.Clear();
            MeteorManager.Instance.Clear();
            FightWnd.Instance.Close();
            player.Clear();
            U3D.GoBack();
            bSync = false;
            FrameIndex = ServerFrameIndex = 0;
            U3D.InsertSystemMsg("与服务器断开链接.");
        }
        RoomId = -1;
        RoomName = "";
    }

    //选择好了角色和武器，向服务器发出进入房间请求.
    public void EnterLevel()
    {
        ClientProxy.EnterLevel(heroIdx, weaponIdx);
    }

    public void OnEnterLevelSucessed(SceneInfo scene_)
    {
        scene = scene_;
        FrameIndex = 0;
    }

    public void OnEnterRoomSuccessed(int roomId, int levelid, int playerid)
    {
        RoomId = roomId;
        LevelId = levelid;
        PlayerId = playerid;
    }

    public void OnNetWorkBattleStart()
    {
        bSync = true;
    }

    //同步全部的输入.
    public void OnSyncInput()
    {
        ServerFrameIndex++;
    }

    //同步全部数据
    public void OnSyncFrame()
    {

    }


    public void Load(List<SceneItem_> sceneItems, List<Player_> players)
    {
        Level lev = LevelMng.Instance.GetItem(LevelId);
        Global.GLevelItem = lev;
        Global.GLevelMode = LevelMode.MultiplyPlayer;
        Global.GGameMode = GameMode.MENGZHU;//所有场景道具都不加载，这部分物件的属性需要同步.
        LoadingWnd.Instance.Open();
        StartCoroutine(LoadAsync(lev, sceneItems, players));
    }

    AsyncOperation mAsync;
    IEnumerator LoadAsync(Level lev, List<SceneItem_> sceneItems, List<Player_> players)
    {
        ResMng.LoadScene(lev.Scene);
        mAsync = SceneManager.LoadSceneAsync(lev.Scene);
        mAsync.allowSceneActivation = false;
        while (mAsync.progress < 0.9f)
        {
            yield return 0;
        }
        mAsync.allowSceneActivation = true;
        while (!mAsync.isDone)
            yield return 0;
        yield return 0;
        OnLoadFinishedEx(lev, sceneItems, players);
        ProtoHandler.loading = false;
        yield return 0;
    }

    void SyncRoom(int room)
    {

    }

    LevelScriptBase GetLevelScript(string sn)
    {
        Type type = Type.GetType(string.Format("LevelScript_{0}", sn));
        if (type == null)
            return null;
        Global.GScriptType = type;
        return System.Activator.CreateInstance(type) as LevelScriptBase;
    }

    void OnLoadFinishedEx(Level lev, List<SceneItem_> sceneItems, List<Player_> players)
    {
        SoundManager.Instance.Enable(true);
        LevelScriptBase script = GetLevelScript(lev.LevelScript);
        if (script == null)
        {
            UnityEngine.Debug.LogError(string.Format("level script is null levId:{0}, levScript:{1}", lev.FuBenID, lev.LevelScript));
            return;
        }

        Global.GScript = script;
        SceneMng.OnLoad();//
        //加载场景配置数据
        SceneMng.OnEnterNetLevel(sceneItems, lev.ID);//原版功能不加载其他存档数据.

        //设置主角属性
        for (int i = 0; i < players.Count; i++)
            U3D.InitNetPlayer(players[i]);

        //把音频侦听移到角色
        Startup.ins.listener.enabled = false;
        Startup.ins.playerListener = MeteorManager.Instance.LocalPlayer.gameObject.AddComponent<AudioListener>();

        //等脚本设置好物件的状态后，根据状态决定是否生成受击盒，攻击盒等.
        GameBattleEx.Instance.Init(lev, script);

        //先创建一个相机
        GameObject camera = GameObject.Instantiate(Resources.Load("CameraEx")) as GameObject;
        camera.name = "CameraEx";

        //角色摄像机跟随者着角色.
        CameraFollow followCamera = GameObject.Find("CameraEx").GetComponent<CameraFollow>();
        followCamera.Init();
        GameBattleEx.Instance.m_CameraControl = followCamera;
        //摄像机完毕后
        FightWnd.Instance.Open();
        if (!string.IsNullOrEmpty(lev.BgmName))
            SoundManager.Instance.PlayMusic(lev.BgmName);

        //除了主角的所有角色,开始输出,选择阵营, 进入战场
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i] == MeteorManager.Instance.LocalPlayer)
                continue;
            MeteorUnit unitLog = MeteorManager.Instance.UnitInfos[i];
            U3D.InsertSystemMsg(GetCampStr(unitLog));
        }
    }

    public static string GetCampStr(MeteorUnit unit)
    {
        if (unit.Camp == EUnitCamp.EUC_ENEMY)
            return string.Format("{0} 选择蝴蝶, 进入战场", unit.name);
        if (unit.Camp == EUnitCamp.EUC_FRIEND)
            return string.Format("{0} 选择流星,进入战场", unit.name);
        return string.Format("{0} 进入战场", unit.name);
    }
}
