using UnityEngine;
using System.Collections;

//正在检查是否有更新
public class ConnectWnd : Window<ConnectWnd> {
    public override string PrefabName { get { return "ConnectWnd"; } }
}

public class ReconnectWnd:Window<ReconnectWnd>
{
    public override string PrefabName
    {
        get { return "ReConnectWnd"; }
    }
}

//显示稍等-转圈
public class LoadingEX : Window<LoadingEX>
{
    public override string PrefabName { get { return "LoadingEX"; } }
}
