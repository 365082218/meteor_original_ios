using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class LoginDialogState : CommonDialogState<LoginDialog>
{
    public override string DialogName { get { return "LoginDialog"; } }
    public LoginDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
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
        if (Main.Ins.GameStateMgr.gameStatus.AutoLogin)
            Main.Ins.GameStateMgr.gameStatus.RememberPassword = true;
        AutoLogin.isOn = Main.Ins.GameStateMgr.gameStatus.AutoLogin;
        RememberPassword.isOn = Main.Ins.GameStateMgr.gameStatus.RememberPassword;

        AutoLogin.onValueChanged.AddListener((bool on) => { Main.Ins.GameStateMgr.gameStatus.AutoLogin = on; });
        RememberPassword.onValueChanged.AddListener((bool on) => { Main.Ins.GameStateMgr.gameStatus.RememberPassword = on; });


        if (!string.IsNullOrEmpty(Main.Ins.GameStateMgr.gameStatus.Account) && Main.Ins.GameStateMgr.gameStatus.AutoLogin)
            Account.text = Main.Ins.GameStateMgr.gameStatus.Account;
        if (!string.IsNullOrEmpty(Main.Ins.GameStateMgr.gameStatus.Password) && Main.Ins.GameStateMgr.gameStatus.RememberPassword)
            Password.text = Main.Ins.GameStateMgr.gameStatus.Password;

        if (Main.Ins.GameStateMgr.gameStatus.AutoLogin)
            AutoConnect();
    }

    void AutoConnect()
    {

    }
}