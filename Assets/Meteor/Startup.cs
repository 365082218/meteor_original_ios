using UnityEngine;
using CoClass;
using System.IO;
using ProtoBuf;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public enum LanguageType
{
    Ch,
    En,
}


[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class ClientVersion
{
    public int Version;
}

//整个游戏只有一份的开关状态.就是整个
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class GameState
{
    public int saveSlot;//默认使用的存档编号
    public int Language;//语言设置. 0 中文 1 英文
    public float MusicVolume;//设置背景音乐
    public float SoundVolume;//设置声音
    public string ClientId;//IOS GAMECENTER账号。
    public int Quality;//0默认最高,1中_800面,2低_300面
    public int Level;//当前最远通过的关卡
    public string NickName;
    public bool useJoystickOrKeyBoard;//是否使用外设摇杆
    public bool EnableDebug;//战斗UI调试面板是否显示按钮
    public bool EnableFunc;//战斗UI控制面板是否显示按钮
    public bool EnableInfiniteAngry;//无限气.
    public bool EnableItemName;//查看物品名称.
    public bool EnableGodMode;//一击必杀
    public MyVector2 JoyAnchor;//摇杆坐标.
    public MyVector2 AxisSensitivity;//轴视角转向灵敏度
    public string MeteorVersion;

}

public class Startup : MonoBehaviour {
    public static Startup ins;
    public Font TextFont;
    public GameState state { get { return GameData.gameStatus; } }
    public InputField cheat;
    public AudioSource Music;
    public AudioSource Sound;
    public AudioListener Listener;
    
    List<string> cheats = new List<string>();
    int cheatsel = -1;
    public bool debugMode;//是否观看开头过场,是否不限制金钱
    public bool GODMODE = true;//上帝模式，玩家对怪物一击必杀.
    public long frameIdx;
    //public int ServerIdx;
    //public string GameServerIP;
    //public string ServerName;
    //public int ServerPort;
    float lasttime;

    void Awake()
    {
        if (ins == null)
        {
            ins = this;
            DontDestroyOnLoad(this);
        }
    }

    public int Lang
    {
        get
        {
            if (state == null)
                return (int)LanguageType.En;
            else
                return state.Language;
        }
        set
        {
            if (state == null)
                return;
            else
                state.Language = value;
        }
    }
    // Use this for initialization
    void Start () {
        if (ins != this)
        {
            if (Music != null)
                DestroyImmediate(Music);
            if (Sound != null)
                DestroyImmediate(Sound);
            if (Listener != null)
                DestroyImmediate(Listener);
            Music = null;
            Sound = null;
            Listener = null;
            //UI上的元素重新赋值过去，从其他场景转回来就会出现新的一套UI
            ins.cheat = cheat;
            DestroyImmediate(gameObject);//U3D OCAgent等其他挂在这里的都会没有.
            return;
        }
        //Log.LogInfo("GameStart");
        Random.InitState(System.Guid.NewGuid().GetHashCode());
    }

    // Update is called once per frame
    void Update () {
        frameIdx++;
        ProtoHandler.Update();
    }

	//APP接口
	public void AppStart()
	{
        OCAgent.AppStart();
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
        GameData.SaveState();
        GameData.SaveCache();
    }

    //游戏接口
    public void GameStart()
	{
        //state = GameData.gameStatus;
        //所有的多语言文本，在读取到语言设置前初始化过的，都需要重新设置文本.
        LangItem.ChangeLang();
        //调试模式不显示过场文本.
        if (debugMode && WSDebug.Ins == null)
            gameObject.AddComponent<WSDebug>();
        //if (debugMode || state.watchedNovel)
        //{
            OnGameStart();
            return;
        //}
        //WsWindow.Open(WsWindow.StartGame);
        //state.watchedNovel = true;
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

    public void GameStartFromSlot()
    {
        ScriptMng.ins.Load(state.saveSlot);
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
        GameData.Resume();
    }

    public void SelectTarget()
    {
        WsWindow.Close(WsWindow.SelectTarget);
        //SelectTarget ctrl = WsWindow.Open<SelectTarget>(WsWindow.SelectTarget);
    }

    //测试代码
    public void PlayEffect()
    {
        StartCoroutine("PlayEf");
    }

    IEnumerator PlayEf()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            SFXLoader.Instance.PlayNextEffect();
        }
    }
}
