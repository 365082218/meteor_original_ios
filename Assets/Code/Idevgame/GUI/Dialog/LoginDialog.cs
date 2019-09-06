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
        if (GameData.Instance.gameStatus.AutoLogin)
            GameData.Instance.gameStatus.RememberPassword = true;
        AutoLogin.isOn = GameData.Instance.gameStatus.AutoLogin;
        RememberPassword.isOn = GameData.Instance.gameStatus.RememberPassword;

        AutoLogin.onValueChanged.AddListener((bool on) => { GameData.Instance.gameStatus.AutoLogin = on; });
        RememberPassword.onValueChanged.AddListener((bool on) => { GameData.Instance.gameStatus.RememberPassword = on; });


        if (!string.IsNullOrEmpty(GameData.Instance.gameStatus.Account) && GameData.Instance.gameStatus.AutoLogin)
            Account.text = GameData.Instance.gameStatus.Account;
        if (!string.IsNullOrEmpty(GameData.Instance.gameStatus.Password) && GameData.Instance.gameStatus.RememberPassword)
            Password.text = GameData.Instance.gameStatus.Password;

        if (GameData.Instance.gameStatus.AutoLogin)
            AutoConnect();
    }

    void AutoConnect()
    {

    }
}