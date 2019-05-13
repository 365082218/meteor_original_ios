using UnityEngine;

using System.IO;
using ProtoBuf;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

public class Startup : MonoBehaviour {
    public static Startup ins;
    public Font TextFont;
    public GameObject fpsCanvas;
    public AudioSource Music;
    public AudioSource Sound;
    public AudioListener listener;
    public AudioListener playerListener;

    void Awake()
    {
        Debug.LogError(Application.persistentDataPath);
        ins = this;
        DontDestroyOnLoad(gameObject);
        Log.WriteError(string.Format("GameStart AppVersion:{0}", AppInfo.Instance.AppVersion()));
        Application.targetFrameRate = 60;
    }

    // Use this for initialization
    void Start () {
        Random.InitState((int)System.DateTime.UtcNow.Ticks);
    }
    
    public void ShowFps(bool active)
    {
        fpsCanvas.SetActive(active);
    }

    // Update is called once per frame
    void Update () {
        if (GameBattleEx.Instance != null && !GameBattleEx.Instance.BattleFinished() && Global.Instance.GLevelMode <= LevelMode.CreateWorld)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (EscWnd.Exist)
                {
                    EscWnd.Instance.Close();
#if UNITY_STANDALONE
                    Cursor.lockState = CursorLockMode.Locked;
#endif
                }
                else
                    EscWnd.Instance.Open();
            }
        }
        if (Global.Instance.GLevelItem == null)
            DlcMng.Instance.Update();
    }

    public void PlayEndMovie(bool play)
    {
        if (!string.IsNullOrEmpty(Global.Instance.GLevelItem.sceneItems) && play && Global.Instance.GLevelMode == LevelMode.SinglePlayerTask && Global.Instance.Chapter == null)
        {
            string num = Global.Instance.GLevelItem.sceneItems.Substring(2);
            int number = 0;
            if (int.TryParse(num, out number))
            {
                if (Global.Instance.GLevelItem.ID >= 0 && Global.Instance.GLevelItem.ID <= 9)
                {
                    string movie = string.Format(Main.strSFile, Main.strHost, Main.port, Main.strProjectUrl, "mmv/" + "v" + number + ".mv");
                    U3D.PlayMovie(movie);
                }
            }
        }

        GotoMenu();
    }

    void GotoMenu()
    {
        MeteorManager.Instance.Clear();
        FightWnd.Instance.Close();
        if (Global.Instance.GLevelMode == LevelMode.Teach || Global.Instance.GLevelMode == LevelMode.CreateWorld)
            U3D.GoBack(() => { MainWnd.Instance.Open(); });
        else if (Global.Instance.Chapter != null)
        {
            U3D.GoBack(() => { DlcLevelSelect.Instance.Open(); });
        }
        else
            U3D.GoBack(() => { MainMenu.Instance.Open(); });
    }

    public void OnApplicationQuit()
    {
        TcpClientProxy.Exit();
        Log.Uninit();
        FtpLog.Uninit();
        GlobalUpdate.Instance.SaveCache();
    }

	public void OnGameStart()
	{
        MainWnd.Instance.Open();
    }
}
