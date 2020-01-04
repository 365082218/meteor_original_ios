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
        Main.Instance.ShowFps(GameData.Instance.gameStatus.ShowFPS);
        StartCoroutine(LoadData());
        Global.Instance.Init();//加载全局表.
        SoundManager.Instance.SetMusicVolume(GameData.Instance.gameStatus.MusicVolume);
        SoundManager.Instance.SetSoundVolume(GameData.Instance.gameStatus.SoundVolume);
    }
	
	void Update () {
		
	}

    IEnumerator LoadData()
    {
        InfoPanel.gameObject.SetActive(true);
        int toProgress = 0;
        int displayProgress = 0;
        yield return SFXLoader.Instance.Init();
        //在读取character.act后再初始化输入模块。
        ActionInterrupt.Instance.Lines.Clear();
        ActionInterrupt.Instance.Whole.Clear();
        ActionInterrupt.Instance.Root = null;
        ActionInterrupt.Instance.Init();
        MenuResLoader.Instance.Init();

        //加载默认角色，这个角色有用的
        //AmbLoader.Ins.LoadCharacterAmb(0);
        
        for (int i = 0; i < 20; i++)
        {
            AmbLoader.Ins.LoadCharacterAmb(i);
            displayProgress++;
            percent.text = LanguagesMgr.GetText("Startup.Start", displayProgress);
            LoadingBar.SetProgress((float)displayProgress / 100.0f);
            yield return 0;
        }

        AmbLoader.Ins.LoadCharacterAmb();
        AmbLoader.Ins.LoadCharacterAmbEx();
        toProgress = 100;
        
        while (displayProgress < toProgress)
        {
            displayProgress += 1;
            percent.text = LanguagesMgr.GetText("Startup.Start", displayProgress);
            LoadingBar.SetProgress((float)displayProgress / 100.0f);
            yield return 0;
        }
        PoseStatus.Clear();
        Application.targetFrameRate = GameData.Instance.gameStatus.TargetFrame;
#if UNITY_EDITOR
        Application.targetFrameRate = 120;
#endif
        Log.Write(string.Format("fps:{0}", Application.targetFrameRate));
        if (!GameData.Instance.gameStatus.SkipVideo)
        {
            string movie = string.Format(Main.strSFile, Main.strHost, Main.port, Main.strProjectUrl, "mmv/start.mv");
            U3D.PlayMovie(movie);
        }
        DlcMng.Instance.Init();
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.MainMenuState);
        Main.Instance.SplashScreenHidden = true;
    }
}
