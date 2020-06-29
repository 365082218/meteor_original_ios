using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

//添加域名和端口的服务器
public class HostEditDialogState : CommonDialogState<HostEditDialog>
{
    public override string DialogName { get { return "HostEditDialog"; } }
    public HostEditDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
}

public class HostEditDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    InputField serverName;
    InputField serverHost;
    InputField serverIP;
    InputField serverPort;
    void Init()
    {
        serverName = Control("ServerName").GetComponent<InputField>();
        serverHost = Control("ServerHost").GetComponent<InputField>();
        serverIP = Control("ServerIP").GetComponent<InputField>();
        serverPort = Control("ServerPort").GetComponent<InputField>();
        serverPort.onEndEdit.AddListener((string editport) =>
        {
            int p = 0;
            if (!int.TryParse(editport, out p))
            {
                U3D.PopupTip("端口必须是小于65535的正整数");
                serverPort.text = "";
                return;
            }
            if (p >= 65535 || p < 0)
            {
                U3D.PopupTip("端口必须是小于65535的正整数");
                serverPort.text = "";
                return;
            }
        });
        serverIP.onEndEdit.AddListener((string value) =>
        {
            IPAddress address;
            if (!IPAddress.TryParse(value, out address))
            {
                U3D.PopupTip("请输入正确的IP地址");
                serverIP.text = "";
                return;
            }
        });
        Control("Yes").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(serverHost.text) && string.IsNullOrEmpty(serverIP.text))
            {
                U3D.PopupTip("域名和IP地址必须正确填写其中一项");
                return;
            }
            int port = 0;
            if (string.IsNullOrEmpty(serverPort.text) || !int.TryParse(serverPort.text, out port))
            {
                U3D.PopupTip("端口填写不正确");
                return;
            }

            //如果域名不为空
            if (!string.IsNullOrEmpty(serverHost.text))
            {
                ServerInfo info = new ServerInfo();
                info.type = 0;
                info.ServerPort = port;
                info.ServerName = string.IsNullOrEmpty(serverName.text) ? serverHost.text : serverName.text;
                info.ServerHost = serverHost.text;
                Main.Ins.GameStateMgr.gameStatus.ServerList.Add(info);
                if (ServerListDialogState.Exist)
                    ServerListDialogState.Instance.OnRefresh(ServerListDialog.ADD, info);
                Main.Ins.CombatData.OnServiceChanged(1, info);
                OnBackPress();
            }
            else if (!string.IsNullOrEmpty(serverIP.text))
            {
                ServerInfo info = new ServerInfo();
                info.type = 1;
                info.ServerPort = port;
                info.ServerName = string.IsNullOrEmpty(serverName.text) ? serverHost.text : serverName.text;
                info.ServerIP = serverIP.text;
                Main.Ins.GameStateMgr.gameStatus.ServerList.Add(info);
                if (ServerListDialogState.Exist)
                    ServerListDialogState.Instance.OnRefresh(ServerListDialog.ADD, info);
                Main.Ins.CombatData.OnServiceChanged(1, info);
                OnBackPress();
            }
        });
        Control("Cancel").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnBackPress();
        });
    }
}