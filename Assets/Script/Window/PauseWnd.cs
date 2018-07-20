using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseWnd : Window<PauseWnd>
{
    public override string PrefabName { get { return "PauseWnd"; } }
	
	GameObject mPauseRoot;
	GameObject mCounter;
	bool mbMutex = false;
	float mTime = 3.0f;
	float mStartTime = 0.0f;
	private GameObject SoundBtn;

    protected override bool OnOpen()
    {
		Init();
		return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }
	
	void Init()
	{
		WndObject.AddComponent<ControlHelper>().ControlHelpUpdate = UpdateTimeCounter;
        UIEventListener.Get(Control("QuitLevel")).onClick = OnClickBacktoMainTown;
		UIEventListener.Get(Control("Continue")).onClick = OnClickBtnContinueGame;
		UIEventListener.Get(Control("SoundBtn")).onClick = OnSoundClick;
		mPauseRoot = Control ("PauseRoot");
		mCounter = Control("TimeCounter");
		mCounter.SetActive(false);

//		SoundBtn = Control("SoundBtn");
//		SoundBtn.SetActive(false);
		SeateToScene();
	}
		
	void OnSoundClick(GameObject go)
	{
	  BattleFinish();
      //BattleFaile();
	}

	private int requestTime = 3;//网络掉包请求次数.
	public void BattleFinish()
	{
		//		return;
		//M_FightStatus=EFightStatus.Fighting;
	}

    public void BattleFaile()
    {
        //		return;

    }


	void SeateToScene()
	{
        //GameBattleEx.Instance.LevelPause(true);
        CreateDropItemGrid();
    }

    void OnClickBacktoMainTown(GameObject go)
    {
		BackToMainTown();
		return;
    }
	
    void BackToMainTown()
    {
        if (FightWnd.Exist)
		    FightWnd.Instance.Close();
		
		if (PauseWnd.Exist)
		    PauseWnd.Instance.Close();
		//LevelHelper.LevelContinue(false);
		//LevelHelper.LevelReset();
	}
	
	void ResumeGame()
	{
        //LevelHelper.LevelContinue(false);
        Close();
	}
	
	void OnClickBtnContinueGame(GameObject go)
    {
		//ResumeGame();
		mbMutex = true;	
		mPauseRoot.SetActive(false);
		mCounter.SetActive(true);
		mStartTime = Time.realtimeSinceStartup;
	}
	
	void CreateDropItemGrid()
	{
		return;
  //      Dictionary<int, int> DicDropItem = GameLevel.Instance.DropInfo;
		//if( DicDropItem == null){
		//	return;
		//}
	
		//List<int> listKey = new List<int>(DicDropItem.Keys); 
		//int DropItemID = 0;
		//string DropItemName = "";
		//string IconName = "";
		//int DropItemCount = 0;
		//GameObject goDropItem ;
		//ItemBase itemBase;
		//for( int i=0;i<DicDropItem.Count;i++ ){
		//	DropItemID = listKey[i];
		//	itemBase =  ItemBaseManager.Instance.GetItem(DropItemID);
		//	DropItemName = itemBase.Name;
		//	IconName =  itemBase.Icon;
		//	DropItemCount = DicDropItem[ DropItemID ];

		//	goDropItem = Control("GridContentDropItem"+i.ToString(),Control("UIGridPanel"));
		//	Control("DropLabel",goDropItem).GetComponent<UILabel>().text = DropItemName;
		//	Control("DropTex",goDropItem).GetComponent<UISprite>().spriteName = IconName;
		//	Control("Count",Control("Label",goDropItem)).GetComponent<UILabel>().text = DropItemCount.ToString();
		//	UpdateItemQualityFrameIcon(Control("DropQuality",goDropItem),itemBase);
		//}
		
		///隐藏其他GridItem
//		for(int j = CommonData.DropItemMax; j>=DicDropItem.Count; j--){
//			goDropItem = Control("GridContentDropItem"+j.ToString(),Control("UIGridPanel"));
//			goDropItem.SetActive(false);
//		}
	}
	
	void UpdateTimeCounterUI(float dt)
	{
		string str = string.Format("{0:C}",dt).Substring(1,4);
		mCounter.GetComponent<UILabel>().text = str ;
	}
	
	void UpdateTimeCounter()
	{
		if( !mbMutex )
			return;
		
	//	Debug.Log("UpdateTimeCounter " + (Time.realtimeSinceStartup - mStartTime));
		float dtime = Time.realtimeSinceStartup - mStartTime;
		UpdateTimeCounterUI( mTime - dtime );
		if( mTime - dtime <= 0.0f )
		{
			mbMutex = false;
			ResumeGame();
		}
	}
}
