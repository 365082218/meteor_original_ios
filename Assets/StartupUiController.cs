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
        Main.Instance.ShowFps(Main.Instance.GameStateMgr.gameStatus.ShowFPS);
        StartCoroutine(LoadData());
        Main.Instance.CombatData.Init();//加载全局表.
        Main.Instance.SoundManager.SetMusicVolume(Main.Instance.GameStateMgr.gameStatus.MusicVolume);
        Main.Instance.SoundManager.SetSoundVolume(Main.Instance.GameStateMgr.gameStatus.SoundVolume);
    }
	
	void Update () {
		
	}

    IEnumerator LoadData()
    {
        InfoPanel.gameObject.SetActive(true);
        int toProgress = 0;
        int displayProgress = 0;
        yield return Main.Instance.SFXLoader.Init();
        //在读取character.act后再初始化输入模块。
        Main.Instance.ActionInterrupt.Lines.Clear();
        Main.Instance.ActionInterrupt.Whole.Clear();
        Main.Instance.ActionInterrupt.Root = null;
        Main.Instance.ActionInterrupt.Init();
        Main.Instance.MenuResLoader.Init();

        //加载默认角色，这个角色有用的
        //AmbLoader.Ins.LoadCharacterAmb(0);
        
        for (int i = 0; i < 20; i++)
        {
            AmbLoader.Ins.LoadCharacterAmb(i);
            displayProgress++;
            percent.text = string.Format(StringUtils.Startup, displayProgress);
            LoadingBar.SetProgress((float)displayProgress / 100.0f);
            yield return 0;
        }

        AmbLoader.Ins.LoadCharacterAmb();
        AmbLoader.Ins.LoadCharacterAmbEx();
        toProgress = 100;
        
        while (displayProgress < toProgress)
        {
            displayProgress += 1;
            percent.text = string.Format(StringUtils.Startup, displayProgress);
            LoadingBar.SetProgress((float)displayProgress / 100.0f);
            yield return 0;
        }
        PoseStatus.Clear();
        Application.targetFrameRate = Main.Instance.GameStateMgr.gameStatus.TargetFrame;
#if UNITY_EDITOR
        Application.targetFrameRate = 120;
#endif
        Log.Write(string.Format("fps:{0}", Application.targetFrameRate));
        if (!Main.Instance.GameStateMgr.gameStatus.SkipVideo)
        {
            string movie = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, "mmv/start.mv");
            U3D.PlayMovie(movie);
        }
        Main.Instance.DlcMng.Init();
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.MainMenuState);
        Main.Instance.SplashScreenHidden = true;
    }
}
