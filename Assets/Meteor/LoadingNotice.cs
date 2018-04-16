using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
public class LoadingNotice: Window<LoadingNotice>
{
    public override string PrefabName { get { return "LoadingNotice"; } }
    protected override bool OnOpen()
    {
        WinStyle = WindowStyle.WS_Ext;
        Init();
        return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }

    public void Init()
    {
        Notice = Control("Notice").GetComponent<Text>();
        Accept = Control("Accept").GetComponent<UIButtonExtended>();
    }
    Text Notice;
    UIButtonExtended Accept;
    public void SetNotice(string text, UnityAction onaccept)
    {
        Notice.text = text;
        Accept.onClick.AddListener(onaccept);
    }
}