using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using protocol;
using System;
using UnityEngine.SceneManagement;

public class NetWorkBattle : MonoBehaviour {

    //�ڷ�������.
    Dictionary<int, protocol.Player_> playerInfo = new Dictionary<int, Player_>();
    Dictionary<int, MeteorUnit> player = new Dictionary<int, MeteorUnit>();
    SceneInfo scene;
    public int RoomId = -1;//�����ڷ������ı��
    public int LevelId = -1;//���䳡���ؿ����
    public int PlayerId = -1;//�����ڷ������Ľ�ɫ���.
    public int heroIdx;//ѡ���ģ�ͱ��.
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
        return "���������";
    }
    //�ȴ�������֡ͬ��.
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
            //������ս��������.
            GameBattleEx.Instance.Pause();
            SoundManager.Instance.StopAll();
            BuffMng.Instance.Clear();
            MeteorManager.Instance.Clear();
            FightWnd.Instance.Close();
            player.Clear();
            U3D.GoBack();
            bSync = false;
            FrameIndex = ServerFrameIndex = 0;
            U3D.InsertSystemMsg("��������Ͽ�����.");
        }
        RoomId = -1;
        RoomName = "";
    }

    //ѡ����˽�ɫ����������������������뷿������.
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

    //ͬ��ȫ��������.
    public void OnSyncInput()
    {
        ServerFrameIndex++;
    }

    //ͬ��ȫ������
    public void OnSyncFrame()
    {

    }


    public void Load(List<SceneItem_> sceneItems, List<Player_> players)
    {
        Level lev = LevelMng.Instance.GetItem(LevelId);
        Global.GLevelItem = lev;
        Global.GLevelMode = LevelMode.MultiplyPlayer;
        Global.GGameMode = GameMode.MENGZHU;//���г������߶������أ��ⲿ�������������Ҫͬ��.
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
        //���س�����������
        SceneMng.OnEnterNetLevel(sceneItems, lev.ID);//ԭ�湦�ܲ����������浵����.

        //������������
        for (int i = 0; i < players.Count; i++)
            U3D.InitNetPlayer(players[i]);

        //����Ƶ�����Ƶ���ɫ
        Startup.ins.listener.enabled = false;
        Startup.ins.playerListener = MeteorManager.Instance.LocalPlayer.gameObject.AddComponent<AudioListener>();

        //�Ƚű����ú������״̬�󣬸���״̬�����Ƿ������ܻ��У������е�.
        GameBattleEx.Instance.Init(lev, script);

        //�ȴ���һ�����
        GameObject camera = GameObject.Instantiate(Resources.Load("CameraEx")) as GameObject;
        camera.name = "CameraEx";

        //��ɫ������������Ž�ɫ.
        CameraFollow followCamera = GameObject.Find("CameraEx").GetComponent<CameraFollow>();
        followCamera.Init();
        GameBattleEx.Instance.m_CameraControl = followCamera;
        //�������Ϻ�
        FightWnd.Instance.Open();
        if (!string.IsNullOrEmpty(lev.BgmName))
            SoundManager.Instance.PlayMusic(lev.BgmName);

        //�������ǵ����н�ɫ,��ʼ���,ѡ����Ӫ, ����ս��
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
            return string.Format("{0} ѡ�����, ����ս��", unit.name);
        if (unit.Camp == EUnitCamp.EUC_FRIEND)
            return string.Format("{0} ѡ������,����ս��", unit.name);
        return string.Format("{0} ����ս��", unit.name);
    }
}
