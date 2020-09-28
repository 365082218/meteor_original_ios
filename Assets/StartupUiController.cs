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
        Main.Ins.ShowFps(Main.Ins.GameStateMgr.gameStatus.ShowFPS);
        StartCoroutine(LoadData());
        Main.Ins.SoundManager.SetMusicVolume(Main.Ins.GameStateMgr.gameStatus.MusicVolume);
        Main.Ins.SoundManager.SetSoundVolume(Main.Ins.GameStateMgr.gameStatus.SoundVolume);
    }
	
	void Update () {
		
	}

    IEnumerator LoadData()
    {
        Debug.Log("loaddata");
        InfoPanel.gameObject.SetActive(true);
        int toProgress = 0;
        int displayProgress = 0;
        toProgress = 100;
        for (int i = 0; i < 20; i++) {
            Main.Ins.AmbLoader.LoadCharacterAmb(i);
        }
        ActionManager.Clear();
        ActionManager.LoadAll();
        Main.Ins.AmbLoader.LoadCharacterAmb();
        Main.Ins.ActionInterrupt.Init();
        Main.Ins.MenuResLoader.Init();
        yield return Main.Ins.SFXLoader.Init();
        while (displayProgress < toProgress)
        {
            displayProgress += 1;
            percent.text = string.Format(StringUtils.Startup, displayProgress);
            LoadingBar.SetProgress((float)displayProgress / 100.0f);
            yield return 0;
        }
        Application.targetFrameRate = Main.Ins.GameStateMgr.gameStatus.TargetFrame;
#if UNITY_EDITOR
        Application.targetFrameRate = 240;
#endif
        Log.Write(string.Format("fps:{0}", Application.targetFrameRate));
        if (!Main.Ins.GameStateMgr.gameStatus.SkipVideo)
        {
            string movie = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, "Mmv/start.mv");
            U3D.PlayMovie(movie);
        }
        Main.Ins.DlcMng.Init();
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainMenuState);
        Main.Ins.SplashScreenHidden = true;
        Main.Ins.JoyStick.enabled = Main.Ins.GameStateMgr.gameStatus.UseJoyDevice;
    }
}
