using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class Languages : ITableItem
{
    public string ID;
    public string Text;

    public int Key() { return 0; }
};

public class LanguagesManager : TableManager<Languages, LanguagesManager>
{
    public override string TableName() { return "Languages"; }
	Dictionary<string, int> mStringIdMap = new Dictionary<string, int>();

	public LanguagesManager()
	{
		Languages[] languages = GetAllItem();
		for (int i = 0; i < languages.Length; i++)
		{
			mStringIdMap[languages[i].ID] = i;
//			Debug.Log(languages[i].ID);
		}
			
	}

	public override void ReLoad()
	{
		base.ReLoad();
		Languages[] languages = GetAllItem();
		for (int i = 0; i < languages.Length; i++)
		{
			mStringIdMap[languages[i].ID] = i;
			//			Debug.Log(languages[i].ID);
		}
	}

	public  string GetItem(string key)
	{
		int ID;
		Languages[] languages = GetAllItem();
//		Debug.Log("languages:"+languages.Length);
		if (mStringIdMap.TryGetValue(key, out ID))
			return languages[ID].Text;

		return key;
	}

}