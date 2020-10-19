using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartupUiController : Dialog {
    [SerializeField]
    Text percent;
    [SerializeField]
    GameObject InfoPanel;
    [SerializeField]
    UILoadingBar LoadingBar;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
#if UNITY_ANDROID
        //AndroidWrapper.Init();
#elif UNITY_IOS
        //IosWrapper.Init();
#endif
        Main.Ins.ShowFps(GameStateMgr.Ins.gameStatus.ShowFPS);
        StartCoroutine(LoadData());
        SoundManager.Ins.SetMusicVolume(GameStateMgr.Ins.gameStatus.MusicVolume);
        SoundManager.Ins.SetSoundVolume(GameStateMgr.Ins.gameStatus.SoundVolume);
    }
	
	void Update () {
		
	}

    IEnumerator LoadData()
    {
        //Debug.Log("loaddata");
        InfoPanel.gameObject.SetActive(true);
        int toProgress = 0;
        int displayProgress = 0;
        toProgress = 100;
        for (int i = 0; i < 20; i++) {
            AmbLoader.Ins.LoadCharacterAmb(i);
        }
        ActionManager.Clear();
        ActionManager.LoadAll();
        AmbLoader.Ins.LoadCharacterAmb();
        ActionInterrupt.Ins.Clear();
        ActionInterrupt.Ins.Init();
        MenuResLoader.Ins.Init();
        yield return SFXLoader.Ins.Init();
        while (displayProgress < toProgress)
        {
            displayProgress += 1;
            percent.text = string.Format(StringUtils.Startup, displayProgress);
            LoadingBar.SetProgress((float)displayProgress / 100.0f);
            yield return 0;
        }
        Application.targetFrameRate = GameStateMgr.Ins.gameStatus.TargetFrame;
        DlcMng.Ins.Init();
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainMenuState);
        Main.Ins.SplashScreenHidden = true;
        Main.Ins.JoyStick.enabled = GameStateMgr.Ins.gameStatus.UseGamePad;
    }
}
