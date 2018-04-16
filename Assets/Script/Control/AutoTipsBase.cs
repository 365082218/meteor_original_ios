using UnityEngine;
using System.Collections;

public class AutoTipsBase{
	public Transform parent;
	public string tips;
	public Vector3 pos;
	public Vector3 scale;
	public string color;

	public void SetDefaultValue(){
		parent = null;
		tips = "";
		pos = Vector3.zero;
		scale = Vector3.one;
		color = "[ffffff]";
	}
}
