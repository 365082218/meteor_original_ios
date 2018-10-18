using UnityEngine;
using System.Collections;

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