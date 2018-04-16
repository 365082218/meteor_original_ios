using UnityEngine;
using System.Collections;

public class SkillSlider : MonoBehaviour {
	
	UISlider slider;
	float FullCheckTime;
	float LittleCheckTime;//小步长时间
	MeteorUnit mplayer;
	bool mlock= false;
	ldaFTimer mFTimer;
	float LastSliderValue;
	GameObject showtimex;
	GameObject thumbx;
	UISlider showtimexx;
	
	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {

		if (FullCheckTime > LittleCheckTime) {
			LittleCheckTime += Time.deltaTime/FullCheckTime;
			mlock = true;
			slider.sliderValue = Mathf.Lerp (0, 1, LittleCheckTime);
		}

//		//Debug.Log ("thumbx "+thumbx.transform.position);
		if (showtimex != null && thumbx != null) {
			showtimex.transform.localPosition = thumbx.transform.localPosition;
			//保留小数位后2位
			float ftime = LittleCheckTime;
			ftime *= 1000;
			ftime = Mathf.Round(ftime);
			ftime /= 100;
			showtimex.GetComponent<UILabel>().text = ftime.ToString();
		}
	}
	
	public void Init(int DirectionType, int showtime, MeteorUnit player, ldaFTimer ftimer)
	{
		showtimex = Global.ldaControlX("ShowTime", this.transform.gameObject);
		thumbx = Global.ldaControlX("Thumb", this.transform.gameObject);

		if (DirectionType == 1) {
			this.transform.Rotate (0, 0, 0);
		}
		else if (DirectionType == 2){
			this.transform.Rotate (0,0,90);
			showtimex.transform.Rotate (0,0,270);
		}
		else if (DirectionType == 3){
			this.transform.Rotate (0,0,180);
			showtimex.transform.Rotate (0,0,180);
		}
		else if (DirectionType == 4){
			this.transform.Rotate (0,0,270);
			showtimex.transform.Rotate (0,0,90);
		}

		showtime += 5000;
		//DirectionType显示方向
		//显示时间showtime 1000 1秒
		slider = this.GetComponent<UISlider> ();
		
		slider.sliderValue = 0;
		
		slider.eventReceiver = this.gameObject;
		slider.functionName = "OnSliderChange";
		
		FullCheckTime = (float)showtime / 1000;
		//CheckTime = 0;
		LittleCheckTime = 0;

		mplayer = player;

		this.gameObject.SetActive (true);

		mFTimer = ftimer;


	}
	
	void OnDestroy() {
	}
	
	void OnDisable () {
	}
	
	void OnSliderChange(){
		
		if (slider.sliderValue > 0.9) {

			//DestroyImmediate(this.gameObject);
			this.gameObject.SetActive(false);

			//释放下一个连击技能
		//	if(FightMainWnd.Exist)
		//		FightMainWnd.Instance.ldaFSkillTimerOnTime(mplayer);

			//mplayer.ldaPlaySkill (1);


			if(slider.sliderValue - LastSliderValue > 0.2)//人工
			{
				//NewSkillItem sitem = mplayer.ldaPlaySkill (1);
				//if(sitem==null)
				//	return;
			
				////mFTimer.Init ((int)sitem.CD*1000, mplayer);
				//mFTimer.Init (2000, mplayer);
				return;
			}

			//mplayer.ldaPlaySkill (0);
		}

		LastSliderValue = slider.sliderValue;
	}
}