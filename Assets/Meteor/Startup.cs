using UnityEngine;
using CoClass;
using System.IO;
using ProtoBuf;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class Startup : MonoBehaviour {
    public static Startup ins;
    public Font TextFont;
    public GameObject fpsCanvas;
    public GameState state { get { return GameData.Instance.gameStatus; } }
    //public InputField cheat;
    public AudioSource Music;
    public AudioSource Sound;
    public AudioListener listener;
    public AudioListener playerListener;
    //public int ServerIdx;
    //public string GameServerIP;
    //public string ServerName;
    //public int ServerPort;
    float lasttime;

    void Awake()
    {
        ins = this;
        DontDestroyOnLoad(gameObject);
        Log.Write("GameStart");
    }

    // Use this for initialization
    void Start () {
        Random.InitState(System.Guid.NewGuid().GetHashCode());
#if !STRIP_LOGS
        fpsCanvas.SetActive(true);
#endif
    }

    // Update is called once per frame
    void Update () {
    }

#region (notification)
    //void CleanNotification()
    //{
    //    UnityEngine.iOS.LocalNotification l = new UnityEngine.iOS.LocalNotification();
    //    l.applicationIconBadgeNumber = -1;
    //    UnityEngine.iOS.NotificationServices.PresentLocalNotificationNow(l);
    //    UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
    //    UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
    //}

    //public static void NotificationMessage(string message, int hour, bool isRepeatDay)
    //{
    //    int year = System.DateTime.Now.Year;
    //    int month = System.DateTime.Now.Month;
    //    int day = System.DateTime.Now.Day;
    //    System.DateTime newDate = new System.DateTime(year, month, day, hour, 0, 0);
    //    NotificationMessage(message, newDate, isRepeatDay);
    //}
    ////本地推送 你可以传入一个固定的推送时间
    //public static void NotificationMessage(string message, System.DateTime newDate, bool isRepeatDay)
    //{
    //    //推送时间需要大于当前时间
    //    if (newDate > System.DateTime.Now)
    //    {
    //        LocalNotification localNotification = new LocalNotification();
    //        localNotification.fireDate = newDate;
    //        localNotification.alertBody = message;
    //        localNotification.applicationIconBadgeNumber = 1;
    //        localNotification.hasAction = true;
    //        if (isRepeatDay)
    //        {
    //            //是否每天定期循环
    //            localNotification.repeatCalendar = CalendarIdentifier.ChineseCalendar;
    //            localNotification.repeatInterval = CalendarUnit.Day;
    //        }
    //        localNotification.soundName = LocalNotification.defaultSoundName;
    //        NotificationServices.ScheduleLocalNotification(localNotification);
    //    }
    //}
#endregion
    void OnApplicationPause(bool paused)
    {
        //程序进入后台时
        if (paused)
        {
            lasttime = Time.time;
            //10秒后发送
            //NotificationMessage("雨松MOMO : 10秒后发送", System.DateTime.Now.AddSeconds(10), false);
            //每天中午12点推送
            //NotificationMessage("雨松MOMO : 每天中午12点推送", 12, true);
        }
        else
        {
            //程序从后台进入前台时
            //CleanNotification();
            if (Time.time - lasttime >= 1000 * 60)
            {
            }    
        }
    }

    public void OnApplicationQuit()
    {
        ClientProxy.Exit();
        Log.Uninit();
        FtpLog.Uninit();
        GameData.Instance.SaveState();
        GlobalUpdate.Instance.SaveCache();
    }

    //游戏接口
    public void GameStart()
	{
        LangItem.ChangeLang();
        OnGameStart();
        return;
    }

    /*
    public void OnConnect(CBase result)
    {
        GameObject loading = U3D.ShowLoading();
        ConnResult connRet = result as ConnResult;
        if (connRet != null)
        {
            if (!connRet.success)
                U3D.btn.gameObject.SetActive(true);
            else
                OnConnectSuccess();
        }
    }
    */

    /*
    public void OnDisConnect()
    {
        if (!GameData.anotherLogined)
        {
            U3D.PopupTip("服务器断开连接, 正在尝试重新连接");
            CancelInvoke("HeartBeat");
            U3D.CloseLoading();
            U3D.ShowLoading();
        }
    }
    */
    /*
    public static void OnSynServerList(RBase rsp)
    {
        ResponGetServerList Rsp = rsp as ResponGetServerList;
        if (Rsp != null && Rsp.lst != null)
            GameData.server = Rsp.lst;
        if (GameData.server != null && GameData.server.Count != 0)
        {
            ins.ServerIdx = GameData.server[0].Idx;
            ins.GameServerIP = GameData.server[0].ServerIP;
            ins.ServerName = GameData.server[0].ServerName;
            ins.ServerPort = GameData.server[0].ServerPort;
            LoginCtrl ctrl = WsWindow.Open<LoginCtrl>(WsWindow.RegAndLogin);
            U3D.CloseLoading();
        }
        else
            ins.OnConnectSuccess();
    }
    */

    /*
    public void OnConnectSuccess()
    {
        //其他设备把此设备挤出去了.
        if (GameData.anotherLogined)
            return;
        //已经登录过的，尝试重连.
        if (GameData.logined)
        {
            ClientProxy.OnClickLogin(GameData.account.lastAccount, GameData.account.lastPassword, 
            delegate (RBase rsp)
            {
                Common.OnAuthRsp(rsp, delegate (RBase rsp2) {
                    //如果重连，那么发过来的信息，结构体不是UserInfo，而是一个可以恢复玩家 线路，角色，和状态的一个包。
                    //这个重连等能进游戏再回过头来做
                    OnSyncRoleStatus(rsp2);
                });
            });
        }
        else
            ClientProxy.UpdateGameServer(OnSynServerList);
    }

    void HeartBeat()
    {
        cnt--;
        if (cnt <= 0)
        {
            U3D.ShowLoading();
            return;
        }
        Common.SendHeartBeat(OnHeartBeat);
    }

    int cnt = 4;
    void OnHeartBeat(RBase rq)
    {
        U3D.PopupTip("心跳成功");
        cnt = 4;
    }

    public void OnSyncRoleStatus(RBase rsp)
    {
        //需要线路信息1，角色编号信息2，2者有了，又登录了的情况下。是可以恢复角色状态的.若恢复失败，则告知需要重新登录.
        U3D.PopupTip("状态重连 还没做，流程是对的。先做状态同步");
    }

    public void OnLoginSuccess()
    {
        //InvokeRepeating("HeartBeat", 15.0f, 15.0f);
        U3D.CloseLoading();
        WsWindow.Close(WsWindow.RegAndLogin);
        ShowServerInfo();
    }

    void ShowServerInfo()
    {
        WsWindow.Open(WsWindow.ServerInfo);
    }

    public void ShowRoleInfo()
    {
        WsWindow.Close(WsWindow.ServerInfo);
        GameObject panel = WsWindow.Open(WsWindow.RoleInfo);
    }
    */
    public void EntryNextTitle()
	{
        WsWindow.Open(WsWindow.UIPage);
	}

	public void OnGameStart()
	{
        MainWnd.Instance.Open();
    }

    public void OnGameOver()
    {

    }

    bool sceneState = false;
    bool battleState = false;
    bool itemViewerState = false;
    bool ventoryState = false;


    public void OnInventoryClose()
    {
        GameData.Instance.Resume();
    }

    public void SelectTarget()
    {
        WsWindow.Close(WsWindow.SelectTarget);
    }
}
