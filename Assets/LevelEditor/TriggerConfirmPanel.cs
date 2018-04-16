using UnityEngine;
using System.Collections;

public class TriggerConfirmPanel : WsWindowEx
{
	public static Vector3 LocalPos;
	public static Vector3 LocalScale;
	public static Vector3 LocalEuler;
	UIEventListener.BoolDelegate onConfirm;
	GameObject objParam;
	UIInput xPos;
	UIInput yPos;
	UIInput zPos;
	UIInput xScale;
	UIInput yScale;
	UIInput zScale;
	UIInput xRotation;
	UIInput yRotation;
	UIInput zRotation;

	public override string strPrefab
	{
		get
		{
			return "TriggerConfirmPanel";
		}
	}
	
	public override void UIInit()
	{
		onConfirm = null;
		objParam = null;
		xPos = Global.ldaControlX("XPos", Panel).GetComponent<UIInput>();
		yPos = Global.ldaControlX("YPos", Panel).GetComponent<UIInput>();
		zPos = Global.ldaControlX("ZPos", Panel).GetComponent<UIInput>();

		xScale = Global.ldaControlX("XScale", Panel).GetComponent<UIInput>();
		yScale = Global.ldaControlX("YScale", Panel).GetComponent<UIInput>();
		zScale = Global.ldaControlX("ZScale", Panel).GetComponent<UIInput>();

		xRotation = Global.ldaControlX("XRotation", Panel).GetComponent<UIInput>();
		yRotation = Global.ldaControlX("YRotation", Panel).GetComponent<UIInput>();
		zRotation = Global.ldaControlX("ZRotation", Panel).GetComponent<UIInput>();

		Global.ldaControlX("Confirm", Panel).GetComponent<UIEventListener>().onClick = OnConfirm;
		Global.ldaControlX("Cancel", Panel).GetComponent<UIEventListener>().onClick = OnCancel;

		xPos.text = "0.0";
		yPos.text = "0.0";
		zPos.text = "0.0";

		xScale.text = "1.0";
		yScale.text = "1.0";
		zScale.text = "1.0";

		xRotation.text = "0";
		yRotation.text = "0";
		zRotation.text = "0";
	}

	public void SetConfirm(UIEventListener.BoolDelegate call, GameObject objSelect)
	{
		onConfirm = call;
		objParam = objSelect;
		Global.ldaControlX("CreateTarget", Panel).GetComponent<UILabel>().text = objParam.name;
	}

	public void OnConfirm(GameObject objClick)
	{
		if (onConfirm != null)
		{
			try
			{
				LocalPos.x = System.Convert.ToSingle(xPos.text);
				LocalPos.y = System.Convert.ToSingle(yPos.text);
				LocalPos.z = System.Convert.ToSingle(zPos.text);
				LocalScale.x = System.Convert.ToSingle(xScale.text);
				LocalScale.y = System.Convert.ToSingle(yScale.text);
				LocalScale.z = System.Convert.ToSingle(zScale.text);
				LocalEuler.x = System.Convert.ToSingle(xRotation.text);
				LocalEuler.y = System.Convert.ToSingle(yRotation.text);
				LocalEuler.z = System.Convert.ToSingle(zRotation.text);
			}
			catch (System.Exception exp)
			{
				Debug.LogError(exp.Message + "|" + exp.StackTrace);
				return;
			}
			onConfirm(objParam, false);
			onConfirm = null;
			objParam = null;
			Close();
		}
	}

	public void OnCancel(GameObject objClick)
	{
		onConfirm = null;
		objParam = null;
		Close();
	}

	public void SetLocalPos(Vector3 vecLocalPos)
	{
		if (vecLocalPos != null)
		{
			xPos.text = vecLocalPos.x.ToString();
			yPos.text = vecLocalPos.y.ToString();
			zPos.text = vecLocalPos.z.ToString();
		}

		//copy prefab attrib
		GameObject objPrefab = Resources.Load(objParam.name) as GameObject;
		xScale.text = objPrefab.transform.localScale.x.ToString();
		yScale.text = objPrefab.transform.localScale.y.ToString();
		zScale.text = objPrefab.transform.localScale.z.ToString();
	}
}
