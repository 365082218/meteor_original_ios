using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class LangItem : MonoBehaviour {

    public static Dictionary<LangItem, Text> LangItems = new Dictionary<LangItem, Text>();
    public StringIden LangIdx;
    // Use this for initialization
    void Awake(){
        LangItems.Add(this, GetComponent<Text>());
    }
    
    void OnDestroy()
    {
        if (LangItems.ContainsKey(this))
            LangItems.Remove(this);
    }

    void Start () {
        GetComponent<Text>().ChangeLang();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public string GetLangStr()
    {
        LangBase langIt = GameData.Instance.langMng.GetRowByIdx((int)LangIdx) as LangBase;
        if (GameData.Instance.gameStatus.Language == (int)LanguageType.Ch && langIt != null)
            return langIt.Ch;
        if (GameData.Instance.gameStatus.Language == (int)LanguageType.En && langIt != null)
            return langIt.En;
        return null;
    }

    public static void ChangeLang()
    {
        foreach (var each in LangItems)
            each.Value.ChangeLang();
    }

    public static string GetLangString(StringIden strIden)
    {
        LangBase langIt = GameData.Instance.langMng.GetRowByIdx((int)strIden) as LangBase;
        if (U3D.Lang == (int)LanguageType.Ch && langIt != null)
            return langIt.Ch;
        if (U3D.Lang == (int)LanguageType.En && langIt != null)
            return langIt.En;
        return "";

    }
}
