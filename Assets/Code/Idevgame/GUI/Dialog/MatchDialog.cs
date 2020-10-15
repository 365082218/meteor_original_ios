using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchDialogState : CommonDialogState<MatchDialog>
{
    public override string DialogName { get { return "MatchWnd"; } }
    public MatchDialogState(MainDialogMgr stateMgr) : base(stateMgr)
    {

    }
}

public class MatchDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    Text TimesUsed;
    GameObject btnLeave;
    System.Timers.Timer EnterQueueTimeOut;
    System.Timers.Timer LeaveQueueTimeOut;
    void Init()
    {
        EnterQueueTimeOut = new System.Timers.Timer(5000);
        EnterQueueTimeOut.Elapsed += new System.Timers.ElapsedEventHandler(OnEnterTimeOut);
        EnterQueueTimeOut.AutoReset = false;

        LeaveQueueTimeOut = new System.Timers.Timer(5000);
        LeaveQueueTimeOut.Elapsed += new System.Timers.ElapsedEventHandler(OnLeaveTimeOut);
        LeaveQueueTimeOut.AutoReset = false;

        //排队预计时间-从排队包里取
        Control("TimesLeft").GetComponent<Text>().text = "03:00";
        TimesUsed = Control("TimesUsed").GetComponent<Text>();
        TimesUsed.text = "00:00";
        btnLeave = Control("Leave");
        btnLeave.GetComponent<Button>().onClick.AddListener(() =>
        {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainMenuState);
        });

        Control("Enter").GetComponent<Button>().onClick.AddListener(() =>
        {
            //Main.Ins.EnterState(Main.Ins.LoadingEx);
            ////Common.EnterQueue();
            //EnterQueueTimeOut.Start();
        });
        Control("Quit").GetComponent<Button>().onClick.AddListener(() =>
        {
            quit = true;
            TimesUsed.text = "00:00";
            tick = 0;
            //Main.Ins.EnterState(Main.Ins.LoadingEx);
            //LeaveQueueTimeOut.Start();
            //Common.LeaveQueue();
        });
    }

    public void OnLeaveTimeOut(object sender, System.Timers.ElapsedEventArgs e)
    {
        //Main.Ins.ExitState(Main.Ins.LoadingEx);
        LeaveQueueTimeOut.Stop();
    }

    public void OnEnterTimeOut(object sender, System.Timers.ElapsedEventArgs e)
    {
        //Main.Ins.ExitState(Main.Ins.LoadingEx);
        EnterQueueTimeOut.Stop();
    }

    public void OnEnterQueue()
    {
        //Main.Ins.ExitState(Main.Ins.LoadingEx);
        EnterQueueTimeOut.Stop();
        btnLeave.SetActive(false);
    }

    public void OnLeaveQueue()
    {
        //Main.Ins.ExitState(Main.Ins.LoadingEx);
        EnterQueueTimeOut.Stop();
        btnLeave.SetActive(true);
    }

    float tick = 0;
    float TotalSeconds = 0;
    bool quit = false;//等待退出
    //bool queue = false;//排队中
    void Update()
    {
        tick += FrameReplay.deltaTime;
        if (quit)
        {
            if (tick >= 5.0)
                OnLeaveQueue();
            return;
        }
        float left = tick;
        if (left < 0)
            left = 0;
        if (left <= TotalSeconds)
            return;
        TotalSeconds = left;
        float minute = left / 60;
        float seconds = left % 60;
        string t = "";
        t = string.Format("{0:D2}:{1:D2}", minute, seconds);
        TimesUsed.text = t;
    }
}
