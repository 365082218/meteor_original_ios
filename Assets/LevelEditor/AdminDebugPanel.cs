using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class AdminDebugPanel : WsWindowEx {

	public override string strPrefab
	{
		get
		{
			return "AdminDebugPanel";
		}
	}
	
	public override float GetZAxis()
	{
		return -10;
	}

	Transform LevelRoot;
	Text LevelDesLabel;
    Text LevelName;
	int nIndex = -1;
    static string mLevelName;
	static Level[] LevelInfo;
	static Dictionary<string, int> LevelDict = new Dictionary<string, int>();
	public override void UIInit()
	{
		LevelRoot = Global.ldaControlX("LevelRoot", Panel).transform;
		LevelDesLabel = Global.ldaControlX("LevelDesLabel", Panel).GetComponentInChildren<Text>();
		LevelName = Global.ldaControlX("LevelName", Panel).GetComponent<Text>();
		if (LevelInfo == null)
			LevelInfo = LevelMng.Instance.GetAllItem();
		for (int i = 0; i < LevelInfo.Length; i++)
		{
			string strKey = LevelInfo[i].ID.ToString() + "_" + LevelInfo[i].Name;
			AddGridItem(strKey, ShowLevelInfo, LevelRoot);
			if (LevelDict.ContainsKey(strKey))
				LevelDict[strKey] = i;
			else
				LevelDict.Add(strKey, i);
		}
		Global.ldaControlX("EnterLevel", Panel).GetComponent<Button>().onClick.AddListener(()=> { EnterLevel(); });
		Global.ldaControlX("Close", Panel).GetComponent<Button>().onClick.AddListener(Close);
        mLevelName = string.IsNullOrEmpty(mLevelName) ? (LevelInfo[0].ID.ToString() + "_" + LevelInfo[0].Name) : mLevelName;
        ShowLevelInfo(mLevelName);
	}

	void AddGridItem(string strTag, UnityEngine.Events.UnityAction<GameObject> call, Transform parent)
	{
		GameObject objPrefab = Resources.Load("LevelSelectItem", typeof(GameObject)) as GameObject;
		GameObject obj = GameObject.Instantiate(objPrefab) as GameObject;
		obj.transform.SetParent(parent);
		obj.name = strTag;
		obj.GetComponent<Button>().onClick.AddListener(()=> { call(obj); });
		obj.GetComponentInChildren<Text>().text = strTag;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = Vector3.one;
	}

	void ShowLevelInfo(GameObject Level)
	{
        ShowLevelInfo(Level.name);
	}
    void ShowLevelInfo(string levelName)
    {
        if (LevelDict.ContainsKey(levelName))
        {
            nIndex = LevelDict[levelName];
            Level sel = LevelInfo[LevelDict[levelName]];
            mLevelName = levelName;
            if (sel != null)
            {
                if (LevelDesLabel != null)
                {
                    LevelDesLabel.text = "";
                    //if (!string.IsNullOrEmpty(sel.Desc))
                    //    LevelDesLabel.text = sel.Desc;
                }
                if (LevelName != null)
                    LevelName.text = sel.Name;
            }
        }
    }

	public void EnterLevel()
	{
		if (LevelInfo.Length > nIndex && nIndex >= 0)
		{
			if (LevelInfo[nIndex] != null)
				EnterLevel(LevelInfo[nIndex].ID);
		}
	}

    private void EnterLevel(int levelId)
    {
        Close();
        GameBattleEx.Instance.Pause();
        U3D.LoadLevel(levelId);
    }
}
