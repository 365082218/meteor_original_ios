using UnityEngine;
using System.Collections;

public class FinishLevelControl : MonoBehaviour {
	
	public delegate void OnTimeReady();
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
//	
//	public void Show(float duration)
//	{
//		Invoke("ShowBase",duration);	
//	}
//	
//	private void ShowBase()
//	{
//		LevelFinishWnd.Instance.ShowEvaBase();
//	}
//	
//	public void LevelUpPromptFinish(float deltaTime)
//	{
//		Invoke("MoveAndAlpha",deltaTime);
//	}
//	private void MoveAndAlpha()
//	{
//		LevelFinishWnd.Instance.PromptMoveOut();
//	}
	
	public void SkillDelayTime(float deltaTime)
	{
		CancelInvoke("HideSegPrompt");
		Invoke("HideSegPrompt",deltaTime);
	}
	private void HideSegPrompt()
	{  
		SkillInput.mSegmentGrid.gameObject.SetActive(false);
		GameObject skillObj = SkillInput.LastSkillObj;
		skillObj.GetComponent<SkillInput>().ControlBtnBeat(skillObj,false);
	}
	
	public void WaitForTime(float time,OnTimeReady onTimeReady)
	{
		StartCoroutine(DelayTimeReady(time,onTimeReady));
	}
	
	private IEnumerator DelayTimeReady(float time,OnTimeReady onTimeReady)
	{
//		Debug.Log("Before wait Time:" + Time.time);
//		Debug.Log("Duration:"+time);
		yield return new WaitForSeconds(time);
//		Debug.Log("After Wait Time:" + Time.time);
		if(onTimeReady != null)
			onTimeReady();
	}
}
