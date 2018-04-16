using UnityEngine;
using System.Collections;

public class ldaFTimer : MonoBehaviour {

    //携带一些东西给返回的函数
    MeteorUnit mPlayer;

	float CheckTime; 
	bool StartCheck;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (StartCheck) {
			CheckTime -= Time.deltaTime;
			if(CheckTime<0){
				StartCheck = false;
				//if(FightMainWnd.Exist)
				//	FightMainWnd.Instance.ldaFSkillTimerOnTime(mPlayer);
			}
		}
	}
	
	void OnDisable() {
		StartCheck = false;
	}
	
	public void Init(int time, MeteorUnit player) {
		mPlayer = player;
		CheckTime = time / 1000;
		StartCheck = true;
	}
}
