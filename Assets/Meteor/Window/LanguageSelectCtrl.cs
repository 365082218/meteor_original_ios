using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class LanguageSelectCtrl : MonoBehaviour {

    public Text selectChinese;
    public Text selectEnglish;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void UpdateUI()
    {
        selectChinese.gameObject.SetActive(false);
        selectEnglish.gameObject.SetActive(false);
        if (Startup.ins.Lang == (int)LanguageType.Ch)
        {
            selectChinese.gameObject.SetActive(true);
        }
        else
        {
            selectEnglish.gameObject.SetActive(true);
        }
    }

    public void SelectLanguage(int lang)
    {
        if (Startup.ins.Lang == lang)
            return;
        U3D.ChangeLang(lang);
        UpdateUI();
        CancelSelect();
        //SceneMng.OnReEntry();
        //把系统界面隐藏起来.否则下一次这个界面无法打开了.
    }

    public void CancelSelect()
    {
        WsWindow.Close(WsWindow.LanguageSelect);
    }
}
