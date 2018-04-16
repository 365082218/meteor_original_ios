using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AutoTipsManager {
	public static AutoTipsManager msInstance = null;
	public static AutoTipsManager Instance { get { return msInstance ?? (msInstance = new AutoTipsManager()); } }
	List<AutoTipsBase> baseList = new List<AutoTipsBase>();
	float time = 0.0f;
    public Vector3 mStartScale=new Vector3(0.01f,0.01f,0.01f);
	public void AddAutoTipsBase(AutoTipsBase atBase)
	{
		baseList.Add(atBase);
	}
	public void CheckAutoTips(float delateTime)
	{
		time -= delateTime;
        if (time > 0) return;
		else
		{
			if (baseList.Count > 0)
			{
				GameObject go = GameObject.Instantiate(Resources.Load("AutoTips")) as GameObject;
				AutoTipsControl control = go.GetComponent<AutoTipsControl>() as AutoTipsControl;
                control.StartAction(baseList[0].parent, baseList[0].tips, baseList[0].pos, mStartScale, baseList[0].color);
				baseList.RemoveAt(0);
				time = 1f;
			}
		}
	}

	public void ShowTip(GameObject parent, string tips)
	{
		AutoTipsBase atBase = new AutoTipsBase();
		atBase.SetDefaultValue();
        if (parent != null) atBase.parent = parent.transform;
        else atBase.parent = GameObject.Find("WindowsRoot").transform;

		atBase.tips = tips;
		AutoTipsManager.Instance.AddAutoTipsBase(atBase);
	}
}
