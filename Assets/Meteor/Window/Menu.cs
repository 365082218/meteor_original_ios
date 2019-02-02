using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
    public AudioSource menu;
    public static Menu Instance;
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
        if (!MainMenu.Exist)
            MainWnd.Instance.Open();
        //音频侦听器移动
        Startup.ins.listener.enabled = true;
        menu.volume = GameData.Instance.gameStatus.MusicVolume;
    }
}
