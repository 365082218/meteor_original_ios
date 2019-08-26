using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using ProtoBuf;
using System;
using protocol;
using Idevgame.Util;
using System.Net;
using UnityEngine.Networking;
using DG.Tweening;
using Ionic.Zlib;
using Idevgame.GameState.DialogState;

public class MainMenuState:CommonDialogState<MainMenu>
{
    public override string DialogName { get { return "MainMenu"; } }
    public MainMenuState(MainDialogStateManager dialogMgr):base(dialogMgr)
    {

    }
}

public class MainMenu : Dialog
{
    bool subMenuOpen = false;
    [SerializeField]
    Text Version;
    [SerializeField]
    Button SinglePlayerMode;
    [SerializeField]
    GameObject SubMenu;
    [SerializeField]
    Button SinglePlayer;
    [SerializeField]
    Button DlcLevel;
    [SerializeField]
    Button TeachingLevel;
    [SerializeField]
    Button CreateBattle;
    [SerializeField]
    Button MultiplePlayer;
    [SerializeField]
    Button PlayerSetting;
    [SerializeField]
    Button Quit;
    [SerializeField]
    Button UploadLog;
    [SerializeField]
    public AudioSource menu;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        //进入主界面，创建全局
        Main.Instance.EnterState(Main.Instance.GameOverlay);
        Init();
        Main.Instance.listener.enabled = true;
        menu.volume = GameData.Instance.gameStatus.MusicVolume;
        //每次进入主界面，触发一次更新APP信息的操作，如果
        Main.Instance.UpdateAppInfo();
    }

    void Init()
    {
        Version.text = AppInfo.Instance.MeteorVersion;
        SinglePlayerMode.onClick.AddListener(() =>
        {
            subMenuOpen = !subMenuOpen;
            SubMenu.SetActive(subMenuOpen);
        });
        //单机关卡-官方剧情
        SinglePlayer.onClick.AddListener(() =>
        {
            OnSinglePlayer();
        });
        DlcLevel.onClick.AddListener(() =>
        {
            OnDlcWnd();
        });
        //教学关卡-教导使用招式方式
        TeachingLevel.onClick.AddListener(() =>
        {
            OnTeachingLevel();
        });
        //创建房间-各种单机玩法
        CreateBattle.onClick.AddListener(() =>
        {
            OnCreateRoom();
        });
        //多人游戏-联机
        MultiplePlayer.onClick.AddListener(() =>
        {
            OnlineGame();
        });
        //设置面板
        PlayerSetting.onClick.AddListener(() =>
        {
            OnSetting();
        });
        Quit.onClick.AddListener(() =>
        {
            GameData.Instance.SaveState();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
        if (GameData.Instance.gameStatus.GodLike)
        {
            UploadLog.gameObject.SetActive(true);
        }
        UploadLog.onClick.AddListener(() => { FtpLog.UploadStart(); });
        GamePool.Instance.CloseDbg();
        TcpClientProxy.Exit();
    }

    void OnSinglePlayer()
    {
        //打开单机关卡面板
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.LevelDialogState, true);
    }

    void OnDlcWnd()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.DlcDialogState);
    }

    //教学关卡.
    void OnTeachingLevel()
    {
        U3D.LoadLevel(31, LevelMode.Teach, GameMode.SIDOU);
    }

    void OnCreateRoom()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.WorldTemplateDialogState);
    }

    void OnlineGame()
    {
        if (Global.Instance.Logined)
            Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.MainLobbyDialogState);
        else
        {
            //没有账号记录，
            if (!string.IsNullOrEmpty(GameData.Instance.gameStatus.Account))
            {
                //进入账号登录创建页面，仿TX启动框
                Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.LoginDialogState);//无账号密码，默认
            }
            else
            {
                if (GameData.Instance.gameStatus.AutoLogin && !string.IsNullOrEmpty(GameData.Instance.gameStatus.Password))
                {
                    //自动登录，一定有账户和密码
                    Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.LoginDialogState, true);
                }
                else
                    Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.LoginDialogState);
            }
        }
    }

    //关卡外的设置面板和关卡内的设置面板并非同一个页面.
    void OnSetting()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.SettingDialogState);
    }
}


public class BattleResultWnd : Dialog
{
    //public override string PrefabName
    //{
    //    get
    //    {
    //        return "BattleResultWnd";
    //    }
    //}

    //protected override bool OnOpen()
    //{
    //    Init();
    //    return base.OnOpen();
    //}

    //protected override bool OnClose()
    //{
    //    return base.OnClose();
    //}

    //GameObject BattleResultAll;
    //GameObject BattleResult;
    //GameObject BattleTitle;
    //Transform MeteorResult;
    //Transform ButterflyResult;
    //public void SetResult(int result)
    //{
    //    if (result == 1)
    //    {
    //        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
    //        {
    //            if (MeteorManager.Instance.UnitInfos[i].Robot != null)
    //                MeteorManager.Instance.UnitInfos[i].Robot.StopMove();
    //            MeteorManager.Instance.UnitInfos[i].controller.Input.ResetVector();
    //            MeteorManager.Instance.UnitInfos[i].OnGameResult(result);
    //        }
    //    }

    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {
    //        U3D.InsertSystemMsg("回合结束");
    //    }
    //    else
    //    {
    //        string mat = "";
    //        Text txt;
    //        switch (result)
    //        {
    //            case -1:
    //            case 0:
    //                mat = "BattleLose";
    //                txt = Control("ButterflyWin").GetComponent<Text>();
    //                U3D.InsertSystemMsg("蝴蝶阵营 获胜");
    //                txt.text = "1";
    //                break;
    //            case 1:
    //            case 2:
    //                mat = "BattleWin";
    //                txt = Control("MeteorWin").GetComponent<Text>();
    //                U3D.InsertSystemMsg("流星阵营 获胜");
    //                txt.text = "1";
    //                break;
    //            case 3:
    //                mat = "BattleNone";
    //                U3D.InsertSystemMsg("和局");
    //                break;

    //        }
    //        BattleResult.GetComponent<Image>().material = Resources.Load<Material>(mat);
    //        BattleResult.SetActive(true);
    //        BattleTitle.SetActive(true);
    //    }
    //    Control("Close").SetActive(true);
    //    Control("Close").GetComponent<Button>().onClick.AddListener(() =>
    //    {
    //        if (SettingWnd.Exist)
    //            SettingWnd.Instance.Close();
    //        GameData.Instance.SaveState();
    //        GameBattleEx.Instance.Pause();
    //        Main.Instance.StopAllCoroutines();
    //        SoundManager.Instance.StopAll();
    //        BuffMng.Instance.Clear();
    //        MeteorManager.Instance.Clear();
    //        Close();
    //        if (FightWnd.Exist)
    //            FightWnd.Instance.Close();
    //        if (GameOverlayWnd.Exist)
    //            GameOverlayWnd.Instance.ClearSystemMsg();
    //        //离开副本
    //        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
    //            UdpClientProxy.LeaveLevel();
    //        else
    //        {
    //            FrameReplay.Instance.OnDisconnected();
    //            Main.Instance.PlayEndMovie(result == 1);
    //        }
    //    });
    //}

    //public void Init()
    //{
    //    GamePool.Instance.CloseDbg();
    //    MeteorResult = Control("MeteorResult").transform;
    //    ButterflyResult = Control("ButterflyResult").transform;
    //    BattleResult = Global.ldaControlX("BattleResult", WndObject);
    //    BattleTitle = Global.ldaControlX("BattleTitle", WndObject);
    //    Control("Close").SetActive(false);
    //    BattleResultAll = Global.ldaControlX("AllResult", WndObject);
    //    Control("CampImage", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Title", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Result", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("CampImage1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Title1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("Result1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
    //    Control("CampImageAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);
    //    Control("TitleAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);
    //    Control("ResultAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);

    //    for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
    //    {
    //        if (GameBattleEx.Instance.BattleResult.ContainsKey(MeteorManager.Instance.UnitInfos[i].InstanceId))
    //        {
    //            InsertPlayerResult(MeteorManager.Instance.UnitInfos[i].InstanceId, GameBattleEx.Instance.BattleResult[MeteorManager.Instance.UnitInfos[i].InstanceId]);
    //            GameBattleEx.Instance.BattleResult.Remove(MeteorManager.Instance.UnitInfos[i].InstanceId);
    //        }
    //        else
    //            InsertPlayerResult(MeteorManager.Instance.UnitInfos[i].InstanceId, MeteorManager.Instance.UnitInfos[i].InstanceId, 0, 0, MeteorManager.Instance.UnitInfos[i].Camp);
    //    }

    //    foreach (var each in GameBattleEx.Instance.BattleResult)
    //        InsertPlayerResult(each.Key, each.Value);
    //    GameBattleEx.Instance.BattleResult.Clear();
    //}

    //void InsertPlayerResult(int instanceId, int id, int killed, int dead, EUnitCamp camp)
    //{
    //    GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {
    //        obj.transform.SetParent(BattleResultAll.transform);
    //    }
    //    else
    //        obj.transform.SetParent(camp == EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
    //    //obj.transform.SetParent(camp ==  EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
    //    obj.layer = MeteorResult.gameObject.layer;
    //    obj.transform.localRotation = Quaternion.identity;
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;

    //    Text Idx = ldaControl("Idx", obj).GetComponent<Text>();
    //    Text Name = ldaControl("Name", obj).GetComponent<Text>();
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {

    //    }
    //    else
    //    {
    //        Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
    //        Camp.text = U3D.GetCampStr(camp);
    //    }
    //    //Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
    //    Text Killed = ldaControl("Killed", obj).GetComponent<Text>();
    //    Text Dead = ldaControl("Dead", obj).GetComponent<Text>();
    //    Idx.text = (id + 1).ToString();
    //    Name.text = U3D.GetUnit(instanceId).Name;
    //    //Camp.text = result.camp == 1 ""
    //    Killed.text = killed.ToString();
    //    Dead.text = dead.ToString();
    //    MeteorUnit u = U3D.GetUnit(id);
    //    if (u != null)
    //    {
    //        if (u.Dead)
    //        {
    //            Idx.color = Color.red;
    //            Name.color = Color.red;
    //            Killed.color = Color.red;
    //            Dead.color = Color.red;
    //        }
    //    }
    //    else
    //    {
    //        //得不到信息了。说明该NPC被移除掉了
    //        Idx.color = Color.red;
    //        Name.color = Color.red;
    //        Killed.color = Color.red;
    //        Dead.color = Color.red;
    //    }
    //}

    //void InsertPlayerResult(int instanceId, BattleResultItem result)
    //{
    //    GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {
    //        obj.transform.SetParent(BattleResultAll.transform);
    //    }
    //    else
    //        obj.transform.SetParent(result.camp == (int)EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
    //    obj.layer = MeteorResult.gameObject.layer;
    //    obj.transform.localRotation = Quaternion.identity;
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.localPosition = Vector3.zero;

    //    Text Idx = ldaControl("Idx", obj).GetComponent<Text>();
    //    Text Name = ldaControl("Name", obj).GetComponent<Text>();
    //    Text Killed = ldaControl("Killed", obj).GetComponent<Text>();
    //    Text Dead = ldaControl("Dead", obj).GetComponent<Text>();
    //    Idx.text = (result.id + 1).ToString();
    //    Name.text = U3D.GetUnit(instanceId).Name;
    //    if (Global.Instance.GGameMode == GameMode.MENGZHU)
    //    {

    //    }
    //    else
    //    {
    //        Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
    //        Camp.text = U3D.GetCampStr((EUnitCamp)result.camp);
    //    }
    //    Killed.text = result.killCount.ToString();
    //    Dead.text = result.deadCount.ToString();
    //    MeteorUnit u = U3D.GetUnit(result.id);
    //    if (u != null)
    //    {
    //        if (u.Dead)
    //        {
    //            Idx.color = Color.red;
    //            Name.color = Color.red;
    //            Killed.color = Color.red;
    //            Dead.color = Color.red;
    //        }
    //    }
    //}
}