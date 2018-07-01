using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
public class LoadingNotice: Window<LoadingNotice>
{
    public override string PrefabName { get { return "LoadingNotice"; } }
    protected override bool OnOpen()
    {
        WinStyle = WindowStyle.WS_Normal;
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
        Accept = Control("Accept").GetComponent<Button>();
        Cancel = Control("Cancel").GetComponent<Button>();
        LoadingBar = Control("LoadingProgressBar").GetComponent<UILoadingBar>();
        SpeedText = Control("Speed").GetComponent<Text>();
    }
    Text Notice;
    Button Accept;
    Button Cancel;
    UILoadingBar LoadingBar;
    Text SpeedText;
    public void SetNotice(string text, UnityAction onaccept, UnityAction oncancel)
    {
        Notice.text = text;
        Accept.onClick.AddListener(onaccept);
        Cancel.onClick.AddListener(oncancel);
    }

    public void UpdateProgress(float percent, string speedstr)
    {
        if (LoadingBar != null)
            LoadingBar.SetProgress(percent);
        if (SpeedText != null)
            SpeedText.text = speedstr;
    }

    public void DisableAcceptBtn()
    {
        Accept.interactable = false;
    }
}