using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class LanguagesMgr:Singleton<LanguagesMgr>
{
    static string LANGUAGE_CFG_PATH = "language";
    static string language = "chinese";
    Dictionary<string, string> textTable = new Dictionary<string, string>();
    public LanguagesMgr()
    {
        TextAsset lang = ResMng.LoadTextAsset(LANGUAGE_CFG_PATH);
        JsonData json = LitJson.JsonMapper.ToObject(lang.text);
        for (int i = 0; i < json.Count; i++)
        {
            JsonData data = json[i];
            textTable.Add(data["key"].ToString(), data["chinese"].ToString());
        }
        Resources.UnloadAsset(lang);
    }

    public static string GetText(string key, params object [] args){

        if (LanguagesMgr.Instance == null || LanguagesMgr.Instance.textTable == null) {
            Debug.LogError("LanguageCfg.Instance.texthash == null");
            return "";
        }

        if (!LanguagesMgr.Instance.textTable.ContainsKey(key)) {
            Debug.LogError("GetText() ERROR,key not exist,key=" + key);
            return key;
        }
        if (args.Length > 0) {
            return string.Format(LanguagesMgr.Instance.textTable[key], args);
        } else {
            return LanguagesMgr.Instance.textTable[key];
        }
    }

}