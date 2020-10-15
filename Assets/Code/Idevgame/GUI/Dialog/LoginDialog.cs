using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class LoginDialogState : CommonDialogState<LoginDialog>
{
    public override string DialogName { get { return "LoginDialog"; } }
    public LoginDialogState(MainDialogMgr stateMgr) : base(stateMgr)
    {

    }
}

public class LoginDialog : Dialog
{
    InputField Account;
    InputField Password;
    UIButtonExtended Login;
    UIButtonExtended Register;
    UIButtonExtended QuickRegister;
    Toggle AutoLogin;
    Toggle RememberPassword;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    public void Init()
    {
        Account = Control("Account").GetComponent<InputField>();
        Password = Control("Password").GetComponent<InputField>();
        Login = Control("Confirm").GetComponent<UIButtonExtended>();
        Register = Control("Register").GetComponent<UIButtonExtended>();
        QuickRegister = Control("QuickGame").GetComponent<UIButtonExtended>();

        AutoLogin = Control("AutoLogin").GetComponent<Toggle>();
        RememberPassword = Control("RemPsw").GetComponent<Toggle>();
        if (GameStateMgr.Ins.gameStatus.AutoLogin)
            GameStateMgr.Ins.gameStatus.RememberPassword = true;
        AutoLogin.isOn = GameStateMgr.Ins.gameStatus.AutoLogin;
        RememberPassword.isOn = GameStateMgr.Ins.gameStatus.RememberPassword;

        AutoLogin.onValueChanged.AddListener((bool on) => { GameStateMgr.Ins.gameStatus.AutoLogin = on; });
        RememberPassword.onValueChanged.AddListener((bool on) => { GameStateMgr.Ins.gameStatus.RememberPassword = on; });


        if (!string.IsNullOrEmpty(GameStateMgr.Ins.gameStatus.Account) && GameStateMgr.Ins.gameStatus.AutoLogin)
            Account.text = GameStateMgr.Ins.gameStatus.Account;
        if (!string.IsNullOrEmpty(GameStateMgr.Ins.gameStatus.Password) && GameStateMgr.Ins.gameStatus.RememberPassword)
            Password.text = GameStateMgr.Ins.gameStatus.Password;

        if (GameStateMgr.Ins.gameStatus.AutoLogin)
            AutoConnect();
    }

    void AutoConnect()
    {

    }
}