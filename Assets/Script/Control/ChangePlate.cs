using UnityEngine;
using System.Collections;

public class ChangePlate : MonoBehaviour {
	
	public  delegate void OnChooseItem();
	public  OnChooseItem onChooseItem;
	// Use this for initialization
	//初速度,速度越小越快 
	private float mInitSpeed = 160;
	//public  int InitSpeed{ get{return this.mInitSpeed;} set{this.mInitSpeed = value;} }
	//加速度， 加速度越大，加速越快 
	private float mAccSpeed = 20;
	//public  int AccSpeed{ get{return this.mAccSpeed;} set{this.mAccSpeed = value; }}
	//减速度， 减速度越大，减速越快 
	private float mLAccSpeed= 40;
	//public  int LAccSpeed{ get{return this.mLAccSpeed;} set{this.mLAccSpeed = value; }}
	//最大速度  数值越小，整度越快 最快为1 
	private float mMaxSpeed = 50;
	//public  int MaxSpeed{ get{return this.mMaxSpeed;} set{this.mMaxSpeed = value; }}	 
	private float mMinSpeed = 500;
	private bool mIsLow = false; 
	private bool mIsHig = false; 	
	private int  mIsHigTme = 0; 
    private const int TotalHigTime = 30;
	//private int Result;   
    private bool mEndStop = false;
	public  bool EndStop{ get{return this.mEndStop;} }    
    //private uint mIntervalTime;     
    private bool mSpinStop = false; 
	void Start () {
	}
	
	public void Stop(){ 
        mSpinStop = true; 
    } 
	
	public void StartRun(int totalCount,int lstIndex)
	{
		SetObjValue();
		ResetInitSpeed(totalCount,lstIndex);
		Effect();
	}
	
	private void SetObjValue()
	{
		mInitSpeed = 160;
		mAccSpeed = 20;
		mLAccSpeed= 40;
		mMaxSpeed = 50;
		mMinSpeed = 500;
		mIsLow = false; 		
		mIsHig = false; 		
		mIsHigTme = 0; 		    
	    mEndStop = false;
		mSpinStop = false; 
	}
	
	private void ResetInitSpeed(int count,int result)
	{
		Debug.Log("Result:"+result);
		Debug.Log("totlCount"+count);
		if(result > 0){		
			int modeCount = (Mathf.CeilToInt(1.0f*(mMinSpeed-mMaxSpeed)/mLAccSpeed) + TotalHigTime)%count;
			Debug.Log("modeCount"+modeCount);
			int addCount;
			if(result - modeCount < 0)
				addCount = count + (result - modeCount);
			else
				addCount = result - modeCount;			
			mInitSpeed = mMaxSpeed + addCount * mAccSpeed;
			Debug.Log("mInitSpeed:"+mInitSpeed);
		}
	}

	private void Effect()
	{
		CancelInvoke("Effect");
		if(onChooseItem != null)
			onChooseItem();

		if(mSpinStop){ 
			return; 
		} 
		
		if(mIsLow)
		{ 
			mInitSpeed += mLAccSpeed;
			if(mInitSpeed >= 400){ 
				mInitSpeed = 400; 
				mEndStop = true; 			
			} 			
		}
		else{ 				
			if(mInitSpeed <= mMaxSpeed){ 					
				if(!mIsHig){ 					
					if(mIsHigTme > TotalHigTime)
					{ 				
						mIsHig = true; 
					} 						
					mIsHigTme++; 						
				}
				else
				{ 						
					mInitSpeed += mLAccSpeed; 						
					mIsLow = true; 					
				} 				
			}
			else
			{ 					
				mInitSpeed -= mAccSpeed; 				
			} 			
		}  
		Invoke("Effect",mInitSpeed/1000.0f);
	}
}

