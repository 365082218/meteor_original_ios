using UnityEngine;
using System.Collections;

public class GoodsList : MonoBehaviour {

	// Use this for initialization
	public static int PageCount = 1;	
	
	GameObject mListPanel;
	GameObject mSlidePoint;
	GameObject mFocusPoint;
	SpringPanel mSpringPanel;
	
	UISprite mLeftArrow;
	UISprite mRightArrow;
	UIGrid mGrid;
	
	int  mCurrentIndex = 0;
	float mPointOffset = 35.0f;
	float mStartPosition;
	
	void Start () {		
	
	    mListPanel = transform.Find("panel_grid").gameObject;
		mSpringPanel = mListPanel.GetComponent<SpringPanel>();
		mSlidePoint = transform.Find("panel/slide_point").gameObject;
		mLeftArrow = transform.Find("panel/frame_root/left_arrow").GetComponent<UISprite>();
		mRightArrow = transform.Find("panel/frame_root/right_arrow").GetComponent<UISprite>();
		mGrid =transform.Find("panel_grid/grid").GetComponent<UIGrid>();
		InitSpringPanel();
		GetStartPos();	
		Debug.Log(mGrid.cellWidth);
	
	}
	
	// Update is called once per frame
	void Update () {	
		
	}
			   	
	void NextPage ()
	{
		if(mCurrentIndex < PageCount-1)
		{
			mCurrentIndex++;
			mSpringPanel.target = new Vector3(-mGrid.cellWidth * mCurrentIndex ,0 ,0);
			mSpringPanel.enabled = true;
			UpdateFocusPoint();
		}
		else
		{
			Debug.Log("you have in the last page");
		}
		
	}

	void FormerPage ()
	{
		if(mCurrentIndex > 0)
		{
			mCurrentIndex--;
			mSpringPanel.target = new Vector3(-mGrid.cellWidth * mCurrentIndex ,0 ,0);
			mSpringPanel.enabled = true;
			UpdateFocusPoint();
		}
		else
		{
			Debug.Log("you have in the first page");
		}
	}
	void InitSpringPanel()
	{
		if(mSpringPanel == null)
		{
			mSpringPanel = mListPanel.AddComponent<SpringPanel>();
		}
		mSpringPanel.target = Vector3.zero;
		mSpringPanel.enabled = false;
		mSpringPanel.strength =13;
		mSpringPanel.onFinished = null;
		
	}
	
	void GetStartPos()
	{
		if(PageCount % 2 ==1)
		{
			int gapCount = PageCount/2;
			mStartPosition = -gapCount * mPointOffset;
		}
		else
		{
			float gapCount = (float)(PageCount-1)/2.0f;
			Debug.Log(gapCount);
			mStartPosition = -gapCount * mPointOffset;
		}
		SetPointPos();
	}
	void SetPointPos()
	{
	    for(int i=0;i< PageCount;i++)
		{			
			GameObject point =  GameObject.Instantiate(Resources.Load("NormalPoint")) as GameObject;	
			point.transform.parent = mSlidePoint.transform;
			point.transform.localScale = new Vector3(18,18,0);
			//set the normalpoint's scale
			point.transform.localPosition = new Vector3(mStartPosition + mPointOffset*i,0,0);
			point.GetComponent<UISprite>().depth = 0;
		}	
		mFocusPoint = GameObject.Instantiate(Resources.Load("FocusPoint")) as GameObject;
		mFocusPoint.transform.parent = mSlidePoint.transform;
		mFocusPoint.transform.localScale = new Vector3(20,20,0);
		//set the fucospoint's scale,a little bigger than that of others
		mFocusPoint.GetComponent<UISprite>().depth = 1;
		UpdateFocusPoint();
	}	
	void UpdateFocusPoint()
	{			
		mFocusPoint.transform.localPosition = new Vector3(mStartPosition + mPointOffset * mCurrentIndex,0,0);
		if(PageCount == 1)
		{
			mLeftArrow.spriteName = "105.icon_01";
			mRightArrow.spriteName = "105.icon_01";
		}
		else if(mCurrentIndex == 0)
		{
		    mLeftArrow.spriteName = "105.icon_01";
			mRightArrow.spriteName = "105.icon_00";
		}
		else if(mCurrentIndex == PageCount-1)
		{
			mLeftArrow.spriteName = "105.icon_00";
			mRightArrow.spriteName = "105.icon_01";
		}
		else
		{
			mLeftArrow.spriteName = "105.icon_00";
			mRightArrow.spriteName = "105.icon_00";			
		}						
	}	
}
