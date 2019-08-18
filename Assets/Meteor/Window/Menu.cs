using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Menu : MonoBehaviour {
    public AudioSource menu;
    public static Menu Instance;
    public static bool gridUpdate = false;
    Coroutine Update;
    private void Awake()
    {
        Menu.Instance = this;
    }
    private void OnDestroy()
    {
        Menu.Instance = null;
    }
    // Use this for initialization
    void Start () {
        //if (!MainMenu.Exist)
        //    MainWnd.Instance.Open();
        //音频侦听器移动
        Startup.ins.listener.enabled = true;
        menu.volume = GameData.Instance.gameStatus.MusicVolume;
        if (!gridUpdate)
        {
            gridUpdate = true;
            StartCoroutine(UpdateAppInfo());
        }
    }

    IEnumerator UpdateAppInfo()
    {
        UnityWebRequest vFile = new UnityWebRequest();
        vFile.url = string.Format(Main.strSFile, Main.strHost, Main.port, Main.strProjectUrl, Main.strNewVersionName);
        vFile.timeout = 10;
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        vFile.downloadHandler = dH;
        yield return vFile.Send();
        if (vFile.isError || vFile.responseCode != 200)
        {
            Debug.LogError(string.Format("update version file:{0} error:{1} or responseCode:{2}", vFile.url, vFile.error, vFile.responseCode));
            vFile.Dispose();
            Update = null;
            yield break;
        }
        Debug.Log("download:" + vFile.url);
        LitJson.JsonData js = LitJson.JsonMapper.ToObject(dH.text);
        GameConfig.Instance.LoadGrid(js);
    }
}
