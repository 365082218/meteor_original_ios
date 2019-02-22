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
    public SceneInfo scene;
    public int RoomId = -1;//房间在服务器的编号
    public int LevelId = -1;//房间场景关卡编号
    public int PlayerId = -1;//主角在服务器的角色编号.
    public int heroIdx;//选择的模型编号.
    public int weaponIdx;
    string RoomName;
    public bool bSync;
    int FrameIndex;
    int ServerFrameIndex;
    public int gameTime;
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
        TurnStarted = false;//在进入战场后，对客户端来说才需要
    }
	
    public void OnPlayerUpdate(int t)
    {
        //Debug.Log("server frame:" + t);
        tick = 0;
        gameTime = t;
        waitSend = true;
    }

    public MeteorUnit GetNetPlayer(int id)
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].InstanceId == id)
                return MeteorManager.Instance.UnitInfos[i];
        }
        return null;
    }

    public string GetNetPlayerName(int id)
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].InstanceId == id)
                return MeteorManager.Instance.UnitInfos[i].name;
        }
        return "不明身份者";
    }
    //等待服务器帧同步.
    //KeyFrame frame = new KeyFrame();
    uint frameIndex = 0;
    uint turn = 0;
    uint tick = 0;
    public bool TurnStarted = false;
    public bool waitReborn = false;
    public bool waitSend = true;
    void Update () {
        if (bSync && RoomId != -1 && TurnStarted && MeteorManager.Instance.LocalPlayer != null && !waitReborn)
        {
            if (waitSend)
            {
                frameIndex++;
                tick++;
                if (frameIndex % 5 == 0)
                {
                    //frame.frameIndex = turn;
                    turn++;
                    waitSend = false;
                    //SyncAttribute(frame.Players[0]);
                    //Common.SyncFrame(frame);

                    if (MeteorManager.Instance.LocalPlayer.Dead)
                    {
                        //Debug.LogError("waitreborn hp:" + frame.Players[0].hp);

                        waitReborn = true;
                    }
                }


                //36=3秒个turn内没收到服务器回复的同步信息，算作断开连接.
                if (tick >= 360)
                {
                    bSync = false;
                    ReconnectWnd.Instance.Open();
                    if (GameBattleEx.Instance != null)
                        GameBattleEx.Instance.NetPause();
                }
                
            }
            if (Global.useShadowInterpolate)
                SyncInterpolate();
        }
	}

    public void SyncInterpolate()
    {
        if (NetWorkBattle.Ins.TurnStarted && MeteorManager.Instance.LocalPlayer != null)
        {
            //在战场更新中,更新其他角色信息，自己的只上传.
            //Debug.Log("SyncInterpolate:" + Time.frameCount);
            for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
            {
                MeteorUnit unit = MeteorManager.Instance.UnitInfos[i];
                if (unit != null && unit != MeteorManager.Instance.LocalPlayer)
                {
                    //玩家同步所有属性
                    if (unit.ShadowSynced)
                        continue;
                    //float next = Mathf.Clamp01(unit.ShadowDelta + 0.5f);
                    //Debug.Log("同步角色位置:" + Time.frameCount);
                    unit.ShadowDelta += 5 * Time.deltaTime;
                    unit.transform.position = Vector3.Lerp(unit.transform.position, unit.ShadowPosition, unit.ShadowDelta);
                    unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, unit.ShadowRotation, unit.ShadowDelta);
                    if (unit.ShadowDelta >= 1.0f)
                        unit.ShadowSynced = true;
                }
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
            if (FightWnd.Exist)
                FightWnd.Instance.Close();
            bSync = false;
            RoomId = -1;
            RoomName = "";
            TurnStarted = false;
            FrameIndex = ServerFrameIndex = 0;
            U3D.InsertSystemMsg("与服务器断开链接.");
            if (!MainWnd.Exist)
                U3D.GoBack();            
        }
        bSync = false;
        RoomId = -1;
        waitReborn = false;
        RoomName = "";
        TurnStarted = false;
    }

    //选择好了角色和武器，向服务器发出进入房间请求.
    public void EnterLevel()
    {
        ClientProxy.EnterLevel(heroIdx, weaponIdx);
    }

    public void SyncAttribute(Player_ p)
    {
        p.angry = MeteorManager.Instance.LocalPlayer.AngryValue;
        p.aniSource = MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx;
        p.Camp = (int)EUnitCamp.EUC_KILLALL;
        p.frame = MeteorManager.Instance.LocalPlayer.charLoader.GetCurrentFrameIndex();
        p.hp = MeteorManager.Instance.LocalPlayer.Attr.hpCur;
        p.hpMax = MeteorManager.Instance.LocalPlayer.Attr.HpMax;
        p.id = (uint)MeteorManager.Instance.LocalPlayer.InstanceId;
        p.model = MeteorManager.Instance.LocalPlayer.Attr.Model;
        p.name = "";// MeteorManager.Instance.LocalPlayer.name;
        p.pos = new Vector3_();
        p.pos.x = Mathf.FloorToInt(MeteorManager.Instance.LocalPlayer.mPos.x * 1000);
        p.pos.y = Mathf.FloorToInt(MeteorManager.Instance.LocalPlayer.mPos.y * 1000);
        p.pos.z = Mathf.FloorToInt(MeteorManager.Instance.LocalPlayer.mPos.z * 1000);
        p.rotation = new Quaternion_();
        p.rotation.w = Mathf.FloorToInt(MeteorManager.Instance.LocalPlayer.transform.rotation.w * 1000);
        p.rotation.x = Mathf.FloorToInt(MeteorManager.Instance.LocalPlayer.transform.rotation.x * 1000);
        p.rotation.y = Mathf.FloorToInt(MeteorManager.Instance.LocalPlayer.transform.rotation.y * 1000);
        p.rotation.z = Mathf.FloorToInt(MeteorManager.Instance.LocalPlayer.transform.rotation.z * 1000);
        p.SpawnPoint = MeteorManager.Instance.LocalPlayer.Attr.SpawnPoint;
        p.weapon = (uint)MeteorManager.Instance.LocalPlayer.Attr.Weapon;
        p.weapon1 = p.weapon;
        p.weapon2 = (uint)MeteorManager.Instance.LocalPlayer.Attr.Weapon2;
        p.weapon_pos = (uint)MeteorManager.Instance.LocalPlayer.weaponLoader.GetCurrentWeapon().WeaponPos;
    }

    public void ApplyAttribute(MeteorUnit unit, Player_ p)
    {
        //Debug.Log("sync:" + Time.frameCount);
        unit.AngryValue = p.angry;
        //MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx = ;
        //p.Camp = (int)EUnitCamp.EUC_KILLALL;
        unit.charLoader.ChangeFrame(p.aniSource, p.frame);
        unit.Attr.hpCur = p.hp;
        //p.hpMax = MeteorManager.Instance.LocalPlayer.Attr.HpMax;
        //p.id = (uint)MeteorManager.Instance.LocalPlayer.InstanceId;
        //p.model = MeteorManager.Instance.LocalPlayer.Attr.Model;
        //p.name = MeteorManager.Instance.LocalPlayer.name;

        if (unit.ShadowDelta < 0.95f && unit.ShadowUpdate && Global.useShadowInterpolate)
        {
            //unit.ShadowDeltaRatio++;
            //Debug.Log("调整插值比:" + unit.ShadowDeltaRatio);
            unit.transform.position = unit.ShadowPosition;
            unit.transform.rotation = unit.ShadowRotation;
        }

        unit.ShadowPosition.x = p.pos.x / 1000.0f;
        unit.ShadowPosition.y = p.pos.y / 1000.0f;
        unit.ShadowPosition.z = p.pos.z / 1000.0f;

        unit.ShadowRotation.w = p.rotation.w / 1000.0f;
        unit.ShadowRotation.x = p.rotation.x / 1000.0f;
        unit.ShadowRotation.y = p.rotation.y / 1000.0f;
        unit.ShadowRotation.z = p.rotation.z / 1000.0f;

        unit.ShadowDelta = 0.0f;
        unit.ShadowSynced = false;
        unit.ShadowUpdate = true;
        if (!Global.useShadowInterpolate)
        {
            unit.transform.position = unit.ShadowPosition;
            unit.transform.rotation = unit.ShadowRotation;
        }
        //p.SpawnPoint = MeteorManager.Instance.LocalPlayer.Attr.SpawnPoint;
        if (unit.Attr.Weapon != (int)p.weapon)
            unit.SyncWeapon((int)p.weapon, (int)p.weapon2);
        if (unit.weaponLoader.GetCurrentWeapon().WeaponPos != p.weapon_pos)
            unit.weaponLoader.ChangeWeaponPos((int)p.weapon_pos);
    }

    public void OnEnterLevelSucessed()
    {
        FrameIndex = 0;
        //frame.Players.Clear();
        Player_ p = new Player_();
        SyncAttribute(p);
        //frame.Players.Add(p);
        bSync = true;
        TurnStarted = true;//进入战场后才开始同步角色数据
    }

    public void OnEnterRoomSuccessed(int roomId, int levelid, int playerid)
    {
        RoomId = roomId;
        LevelId = levelid;
        PlayerId = playerid;
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
        NetWorkBattle.Ins.OnEnterLevelSucessed();
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

        U3D.InsertSystemMsg("新回合开始计时");
        if (FightWnd.Exist)
            FightWnd.Instance.OnBattleStart();
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
