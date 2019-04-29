using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//退出二次确认框
public class EscConfirmWnd : Window<EscConfirmWnd>
{
    public override string PrefabName { get { return "EscConfirmWnd"; } }
	
    protected override bool OnOpen()
    {
		Init();
		return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }
	
	void Init()
	{
        Control("Leave").GetComponent<Button>().onClick.AddListener(OnLeave);
        Control("Continue").GetComponent<Button>().onClick.AddListener(Close);
	}

    void OnLeave()
    {
        if (SettingWnd.Exist)
            SettingWnd.Instance.Close();
        GameData.Instance.SaveState();
        GameBattleEx.Instance.Pause();
        Startup.ins.StopAllCoroutines();
        SoundManager.Instance.StopAll();
        BuffMng.Instance.Clear();
        MeteorManager.Instance.Clear();
        Close();
        if (FightWnd.Exist)
            FightWnd.Instance.Close();
        if (GameOverlayWnd.Exist)
            GameOverlayWnd.Instance.ClearSystemMsg();
        //离开副本
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
            ClientProxy.LeaveLevel();
        else
        {
            FrameReplay.Instance.OnDisconnected();
            U3D.GoBack();
        }
    }

}
