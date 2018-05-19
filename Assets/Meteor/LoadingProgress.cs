using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingProgress : Window<LoadingProgress>
{
    public override string PrefabName { get { return "LoadingProgressBar"; } }
    public UILoadingBar loadbar;
    protected override bool OnOpen()
    {
        WinStyle = WindowStyle.WS_Ext;
        loadbar = WndObject.GetComponent<UILoadingBar>();
        return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }

    int min;
    int max;
    int cur;
    public void InitProgress()
    {
        //GameData.gameVersion.Total;
        loadbar.SetProgress(0);
        cur = 0;
        min = 0;
        //max = GameData.updateVersion.Total;
    }

    public void SetProgress(int current)
    {
        if (cur != current)
        {
            cur = current;
            loadbar.SetProgress((float)cur / (float)max);
        }
    }
}
