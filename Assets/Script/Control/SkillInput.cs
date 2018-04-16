using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillInput : MonoBehaviour
{
    UIFilledSprite mIcon;
    UISprite mBGSprite;
    GameObject mSkillPromptObj;
    bool mFirstCDDone = false;
    FinishLevelControl mFinishLevelControl;
	UILabel CDLabel;

    bool mEnabled = false;
    bool mSkillLinked = false;
    bool mSkillReady = false;
    bool mSelfBuff = false;
    bool mTargetBuff = false;
    float mCD = 0;
    float mSkillCD = 0;
    int mAcionCache = -1;
    int mInterruptIndex = -1;
    int mSkillStage = 1; 
	MeteorUnit mLocalPlayer;
    List<MeteorUnit> mBuffTargets = null;
	SkillItem mSkillItem;
	public SkillItem SkillItem {get {return mSkillItem;}}
	float mCurSkillmAnimTime = 0.0f;
	public float CurSkillmAnimTime {get {return mCurSkillmAnimTime;}}

	//save the normal skillitem add by zhj 20150121
	List<int> SkillItemIDList = new List<int>();
	//save the normal skillitem cd add by zhj 20150121
	public List<float> SkillItemCDList = new List<float>();

    static int LastSkillId = 0;
	static bool EnableSkillKey = false;
    public static GameObject LastSkillObj;
    public static Transform mSegmentGrid;


    void Awake()
    {
        mBGSprite = transform.Find("bg").GetComponent<UISprite>();
        mIcon = transform.Find("icon").GetComponent<UIFilledSprite>();
		CDLabel = transform.Find("CDLabel").GetComponent<UILabel>();
        if (FightWnd.Exist)
        {
            mSegmentGrid = FightWnd.Instance.WndObject.transform.Find("SkillSegmentGrid");
            mFinishLevelControl = mSegmentGrid.GetComponent<FinishLevelControl>();
            if (mFinishLevelControl == null)
                mFinishLevelControl = mSegmentGrid.gameObject.AddComponent<FinishLevelControl>();
        }
        //if (MeteorManager.Instance.LocalPlayer != null)
        //{
        //    MeteorManager.Instance.LocalPlayer.SkillList.Add(this);
        //}
    }


    //public void Init(SkillItem skillItem)//cancel by Lindean 20141031
	public void Init(SkillItem skillItem, MeteorUnit heroplayer)//add by Lindean 20141031
    {
        mSkillItem = skillItem;
        //mLocalPlayer = UnitManager.Instance.LocalPlayer as LocalPlayer;
		mLocalPlayer = heroplayer;

        ResetSkillInfo();
		if (mSkillItem != null) {
			int skillId = skillItem.SkillId;
			SkillBase skillBase = SkillBaseManager.Instance.GetItem(skillId);

			if(!mSkillItem.Lock)
				Show (true);
			else
				Show (false);
		}
        else
            Show(false);
    }

	public void SetSkillCD(int skillID, float skillCD)
	{

	}

    void ResetSkillInfo()
    {
//cancel by Lindean 20141031
//        if (mSkillItem == null)
//            return;
//
//        // setup the skill icon name.
//        mItemBase = ItemBaseManager.Instance.GetItem(mSkillItem.SkillBase.ID);
//        mIcon.spriteName = mItemBase.Icon;
//        mBGSprite.spriteName = mItemBase.Icon; ;
//
//        mSkillItem.SkillInput = this;
//        if (!Global.GuideMode)
//            mSkillItem.ApplyRune();
//
//        mIcon.enabled = false;
//        mIcon.fillAmount = 1.0f;
//
//        mSkillCD = mSkillItem.CD;
//        mCD = mSkillCD;
//
//        mBuffCheckFlag[mSkillItem.SkillAttrib.Mode] = true;
//        if (mSkillItem.RuneBase1 != null) mBuffCheckFlag[mSkillItem.RuneBase1.Mode] = true;
//        if (mSkillItem.RuneBase2 != null) mBuffCheckFlag[mSkillItem.RuneBase2.Mode] = true;
//        if (mSkillItem.RuneBase3 != null) mBuffCheckFlag[mSkillItem.RuneBase3.Mode] = true;
//        if (mSkillItem.RuneBase4 != null) mBuffCheckFlag[mSkillItem.RuneBase4.Mode] = true;

		if (mSkillItem == null)
			return;

		if(mIcon != null && mBGSprite != null){
			// setup the skill icon name.
			mIcon.spriteName = mSkillItem.BtnIcon1;
			mBGSprite.spriteName = mSkillItem.BtnIcon1;

			mIcon.enabled = false;
			mIcon.fillAmount = 1.0f;
		}
		
		mSkillItem.SkillInput = this;
		mSkillCD = mSkillItem.CD;

		//add by zhj 20150121
		int size = SkillItemIDList.Count;
		for (int i = 0; i < size; i++)
		{
			if (mSkillItem.SkillId == SkillItemIDList[i])
			{
				mCD = SkillItemCDList[i];
				return;
			}
		}
		SkillItemIDList.Add(mSkillItem.SkillId);
		SkillItemCDList.Add(mCD);
		mCD = mSkillCD;
    }

	public float GetCD()
	{
		int size = SkillItemIDList.Count;
		for (int i = 0; i < size; i++)
		{
			if (mSkillItem.SkillId == SkillItemIDList[i])
			{
				return mSkillCD -SkillItemCDList[i];

			}
		}
		return 0;
	}

	public void SetCD(float time)
	{
		int size = SkillItemIDList.Count;
		for (int i = 0; i < size; i++)
		{
			if (mSkillItem.SkillId == SkillItemIDList[i])
			{
				 SkillItemCDList[i] =mSkillCD-time;
				mCD = SkillItemCDList[i];
			}
		}
	}

    void Show(bool enabled)
    {
        mEnabled = enabled;
		if (mIcon != null && mBGSprite != null) 
		{
			GetComponent<Collider>().enabled = mEnabled;
			mIcon.enabled = mEnabled;
			mBGSprite.enabled = mEnabled;
            if (enabled)
                mIcon.color = new Color(0.5f,0.5f,0.5f,1);//Color.white;
            else
                mIcon.SetGray();
		}
    }
	
    public void PlaySkill()
    {
        // super skill cost soul, and the other cost ability.
        string skillAction = "";
   //     if (mSkillItem.SkillType == ESkillType.Super)
   //     {
			//int curAbility = mLocalPlayer.GetAttrib(EHDVO.CurAbility);

			//if (curAbility >= mSkillItem.Energy1 && !string.IsNullOrEmpty(mSkillItem.Action1))
			//{
			//	skillAction = mSkillItem.Action1;
			//	mLocalPlayer.AddAbility(-mSkillItem.Energy1);
			//	mSkillStage = 1;
			//}
			//else//add by Lindean
			//	return;//技能释放不成功
   //     }
   //     else
        {
            if (mSkillStage <= 1)
            {
				skillAction = mSkillItem.Action1;
				//add // by zhj 20150108
//				mLocalPlayer.AddAbility(-mSkillItem.Energy1); 
            }
        }

        if (mBuffTargets != null)
            mBuffTargets.Clear();

        // 处理加给自身的buff
        //ProcessBuff(mLocalPlayer, EBuffMode.PlaySkill);

        // 设置技能的段数。
       // mSkillItem.SkillStage = mSkillStage;

        // 执行动作。
        //mLocalPlayer.PlaySkill(mSkillItem, skillAction);

        // 动作系统需要的监测技能的按键
		//LastSkillId = mItemBase.ID;//cancel by Lindean 20141031
		LastSkillId = mSkillItem.SkillId;

        mFirstCDDone = true;

        if (FightWnd.Exist)
            ReleaseSkill();

        // reset the cd
        //mCD = -2.0f;
		mCD=0f;
		// set the cd for skillitemcd list; add by zhj  20150121
		int _size = SkillItemIDList.Count;
		for (int i = 0; i < _size; i++)
		{
			if (mSkillItem.SkillId == SkillItemIDList[i])
				SkillItemCDList[i] = mCD;
		}
		// set commom cd add by zhj 20150106
		//SuperSkillInput.mCommonCD = 2.0f;

		//mCurSkillmAnimTime = mLocalPlayer.CachedAnimationState.length;


		mSkillItem.ResetSkillCDTime();
//		if(mSkillItem!=null)
//		{
//			//skillitem.damage+=damage;
//			foreach(NewSkillItem tem in mLocalPlayer.SillItems)
//			{
//				if(tem.SkillId == mSkillItem.SkillId)
//				{
//					tem.time++;
//				}
//			}
//		}
//		if(skillitem!=null)
//		{
//			//skillitem.damage+=damage;
//			foreach(NewSkillItem item in wn.SillItems)
//			{
//				if(item.SkillId == skillitem.SkillId)
//				{
//					item.damage+=damage;
//				}
//			}
//		}
    }

	bool isCdDone = false;
    // Update is called once per frame
    void Update()
    {

        //if (NGUIJoystick_skill.instance != null && Camera.main!=null)
        //NGUIJoystick_skill.instance.SkillButtonUpdate(Camera.main.transform.eulerAngles,mLocalPlayer);

        if (!mEnabled)
            return;

        // update the skill cd.
		isCdDone = UpdateCD();
        // update the skill icon.
        mSkillReady = UpdateIcon();
        // update the skillprompt.
		UpdateSkillPrompt(isCdDone);

		//Buff 不能使用技能
		if (!mLocalPlayer.posMng.CanSkill)
			mSkillReady = false;
		
		//cdDone = false;//debug by Lindean 20141031
		//mSkillReady = true;//debug by Lindean 20141031

		if (!isCdDone)
            mBGSprite.color = Color.white;
        else
            mBGSprite.color = mSkillReady ? Color.white : new Color(0.3f, 0.3f, 0.3f);
    }

    void ReleaseSkill()
    {
        if (mSkillItem != null && mSkillItem.SkillType != ESkillType.Super)
        {
            //int curAbility = mLocalPlayer.GetAttrib(EPA.CurAbility);
            //int curAbility = mLocalPlayer.GetAttrib(EHDVO.CurAbility);
            //int skillSegCount = GetSegmentCount();
//            if (skillSegCount > 1 && CanReleaseCurSeg())
//            {
//                if (gameObject != LastSkillObj)
//                    ControlBtnBeat(LastSkillObj, false);
//                ControlBtnBeat(gameObject, mSkillStage < skillSegCount);
//                mSegmentGrid.gameObject.SetActive(true);
//                int num = mSkillStage;
//                num = num <= 1 ? 1 : num;
//
//                mFinishLevelControl.SkillDelayTime(1.0f);
//            }
//            else
//            {
                ControlBtnBeat(LastSkillObj, false);
                mSegmentGrid.gameObject.SetActive(false);
//            }
            LastSkillObj = gameObject;
        }

		mSegmentGrid.gameObject.SetActive(false);//add by Lindean 20141031 战斗中下部3个释放技能时的技能级别球不看了
    }

    public void ControlBtnBeat(GameObject targetObj, bool isBeat)
    {
        if (targetObj != null)
        {
            Animation animation = targetObj.GetComponent<Animation>();
            if (isBeat)
            {
                if (!animation.isPlaying)
                    animation.Play();
            }
            else
            {
                if (animation != null && animation.isPlaying)
                {
                    animation.Stop();
                    targetObj.transform.localScale = new Vector3(0.85f, 0.85f, 1.0f);
                }
            }
            string objName = targetObj.name + "Panel";
            targetObj.transform.parent.Find(objName).gameObject.SetActive(isBeat);
            transform.Find("icon").gameObject.SetActive(!isBeat);
        }
    }

//    bool CanReleaseCurSeg()
//    {
//	
//		return true;
//    }



    void UpdateSkillPrompt(bool isCDDone)
    {
        if (mSkillItem != null && mSkillItem.SkillType == ESkillType.Super)
        {
            if (mSkillReady && isCDDone)
            {
                if (mSkillPromptObj == null)
                    CreatePromptObj();
                if (!mSkillPromptObj.activeSelf)
                {
                    mSkillPromptObj.SetActive(true);
                }
            }
            else
            {
                if (mSkillPromptObj != null)
                {
                    GameObject.Destroy(mSkillPromptObj);
                }
            }
        }

        if (mSkillItem != null && mSkillItem.SkillType != ESkillType.Super)
        {
            if (isCDDone && mFirstCDDone && mSkillReady)
            {
                if (mSkillPromptObj == null)
                    CreatePromptObj();
                mFirstCDDone = false;
                float duration = mSkillPromptObj.GetComponentInChildren<ParticleSystem>().duration;
                GameObject.Destroy(mSkillPromptObj, duration);
            }
        }
    }

    void CreatePromptObj()
    {
		mSkillPromptObj = GameObject.Instantiate(Resources.Load("SuccessEffect")) as GameObject;
        //mSkillPromptObj = GameObject.Instantiate(Resources.Load("UI_SkillButton_03")) as GameObject;
        mSkillPromptObj.transform.parent = transform;
        mSkillPromptObj.transform.localPosition = new Vector3(0.0f, 0.0f, -2.0f);
        mSkillPromptObj.transform.localScale = Vector3.one;
        mSkillPromptObj.transform.localEulerAngles = Vector3.zero;
    }
	
    bool UpdateCD()
    {
		if (mCurSkillmAnimTime > 0)
		{
			mCurSkillmAnimTime -= Time.deltaTime;
		}

//		if (SuperSkillInput.mCommonCD > 0)
//		{
//			mIcon.enabled = true;	
////			return false;
//		}

		//to count the CD list add by zhj 20150121
		int _size = SkillItemCDList.Count;
		for (int i = 0; i < _size; i++)
			SkillItemCDList[i] += Time.deltaTime;

		if (mCD >= mSkillCD)
		{
			mIcon.enabled = false;
			CDLabel.gameObject.SetActive(false);
            return true;
        }

		CDLabel.gameObject.SetActive(true);
        mCD += Time.deltaTime;
        mIcon.fillAmount = Mathf.Max(1.0f - ((mCD + 2.0f) / (mSkillCD + 2.0f)), 0.0f);
		int CD = (int)(mSkillCD - mCD) + 1;
		CDLabel.text = CD.ToString();
        mIcon.enabled = true;
        return false;
    }

    bool UpdateIcon()
    {    
        // check the skill has action link
        //Data.Action activeAction = mLocalPlayer.ActionStatus.ActiveAction;
        //if (activeAction != null && activeAction.mActionCache != mAcionCache)
        //{
       //     mInterruptIndex = -1;
       //     mAcionCache = activeAction.mActionCache;
       //     for (int i = 0; i < activeAction.mActionInterrupts.Count; i++)
      //      {
       //         Data.ActionInterrupt interrupt = activeAction.mActionInterrupts[i];
      //          if (interrupt.SkillID != mSkillItem.SkillId)
       //             continue;

//                // the second stage of the skill.
//				if (interrupt.CheckSkillID == 2 && string.IsNullOrEmpty(mSkillItem.Action2))
//                    continue;
//
//                // the third stage of the skill.
//				if (interrupt.CheckSkillID == 3 && string.IsNullOrEmpty(mSkillItem.Action3))
//                    continue;

                // update the skill icon.
				//string skillIcon = mSkillItem.BtnIcon1;
				//if (interrupt.CheckSkillID == 2 && !string.IsNullOrEmpty(mSkillItem.BtnIcon1))
				//	skillIcon = mSkillItem.BtnIcon1;
				//else if (interrupt.CheckSkillID == 3 && !string.IsNullOrEmpty(mSkillItem.BtnIcon1))
				//	skillIcon = mSkillItem.BtnIcon1;
    //            mIcon.spriteName = skillIcon;
    //            mBGSprite.spriteName = skillIcon;
    //            mSkillStage = interrupt.CheckSkillID;
    //            mInterruptIndex = i;
    //            break;
    //        }
    //        mSkillLinked = (mInterruptIndex >= 0);
        //}

        //if this action does not contain any skill link.
   //     if (!mSkillLinked) return false;

   //     // check the interrupt is enabled.
   //     if (!mLocalPlayer.ActionStatus.GetInterruptEnabled(mInterruptIndex))
   //         return false;

   //     // check the skill has consume value.
   //     if (!Global.GuideMode)
   //     {
			////add by Lindean 20141031
			//int curAbility = mLocalPlayer.GetAttrib(EHDVO.CurAbility);
   //         if(MainScript.Instance.DebugSkillButtons)
			//    curAbility = 100000;//Lindean debug 
			//if (curAbility < mSkillItem.Energy1)
			//	return false;
   //     }

        return true;
    }

    public void OnHitTarget(MeteorUnit target)
    {
        ProcessBuff(target);
    }

    public void OnHit(MeteorUnit target)
    {
        ProcessBuff(target);
    }

    public void OnHurt(MeteorUnit target)
    {
        ProcessBuff(target);
    }

    void ProcessBuff(MeteorUnit target)
    {
        if (target.Dead) return;
        // 优化一下，后面的检测不侦测此类型的条件。
        //if (!mBuffCheckFlag[(int)mode]) return;//cancel by Lindean 20141031 莫名其妙

        // 检测是不是已经中过buff了。
        if (mBuffTargets == null) mBuffTargets = new List<MeteorUnit>();
        if (mBuffTargets.Contains(target)) return;
        mBuffTargets.Add(target);

        //if (mSkillItem.BuffMode == (int)mode)
		//	ProcessBuff(target, mSkillItem.BuffChance, mSkillItem.BuffTarget, mSkillItem.BuffID);

//		string[] tmpBuffIDs = mSkillItem.BuffIDs.Split ('|');
//		string[] tmpBuffTargets = mSkillItem.BuffTargets.Split ('|');
//		string[] tmpBuffChances = mSkillItem.BuffChances.Split ('|');
//		string[] tmpBuffModes = mSkillItem.BuffModes.Split ('|');
//		if (tmpBuffIDs.Length != tmpBuffTargets.Length || 
//			tmpBuffIDs.Length != tmpBuffChances.Length ||
//		    tmpBuffIDs.Length != tmpBuffModes.Length)
//		{
//			Debug.Log("Skill buff set error!");
//			return;//Skill Buff 配置数据不对
//		}
//		for(int i=0; i < tmpBuffIDs.Length; i++)
//		{
//            //if(int.Parse(tmpBuffModes[i]) == (int)mode)
//            //    ProcessBuff(target, int.Parse(tmpBuffChances[i]), 
//            //                int.Parse(tmpBuffTargets[i]), int.Parse(tmpBuffIDs[i]));
//            if (int.Parse(tmpBuffIDs[i]) == 0 ||
//                int.Parse(tmpBuffChances[i]) < Random.Range(0, 10001))
//                    return;
//
//            if (int.Parse(tmpBuffTargets[i]) == (int)EBuffTarget.Self)
//                mLocalPlayer.AddBuff(int.Parse(tmpBuffIDs[i]));
//		}
//		//add by Lindean
//		ProcessBuff(target, mSkillItem.BuffChance, 
//		            mSkillItem.BuffTarget, mSkillItem.BuffID);
    }


    void ProcessBuff(MeteorUnit target, int chance, int targetType, int id)
    {
        if (id == 0 || chance < Random.Range(0, 10001))
            return;

        //if (targetType == (int)EBuffTarget.Self)
        //    mLocalPlayer.AddBuff(id);
        //else
        //    target.AddBuff(id);
    }

	//DIY自动战斗函数，非原装GOD进口函数
	public bool OnAIPress(bool isPressed,bool isAI)
	{
		//return false;
		if (Global.PauseAll)
			return false;

		if (!mSkillReady)
			return false;
		
		// the skill stage need check the CD.
		if (mSkillStage <= 1 && mCD < mSkillCD)
			return false;
		OnPress(isPressed,isAI);
		return true;
	}
	void OnPress(bool isPressed)
	{


		if (!isCdDone)
			return;
//		OnPress(isPressed,false);
		if (isPressed)
		{
            //if (NGUIJoystick_skill.instance.isPress)
            //    return;

            //NGUIJoystick_skill.instance.CurSkillBtnName = this.gameObject.name;
//            Debug.Log("NGUIJoystick_skill.instance.CurSkillBtnName = " + NGUIJoystick_skill.instance.CurSkillBtnName);

            //半径  角度
            string[] attackRanges = mSkillItem.SkillBase.attackRange.Split(':');
            int Parse0 = int.Parse(attackRanges[0]);
            int Parse1 = int.Parse(attackRanges[1]);

            //Parse0 = 500;
            //Parse1 = 200;

            /** 技能范围类型 1=扇形； 2=方形； 3=线性+箭头； 4=圆形；*/
            /**
             *
                扇形： 半径：角度（360度为圆形）
                方形： 长：宽
                线性+箭头： 长：0
                圆形： 施法距离：伤害半径 
             * 
             * 
             */

            //1=扇形  半径：角度（360度为圆形）
            //if (mSkillItem.SkillBase.rangeType == 1)
            //{
            //    NGUIJoystick_skill.instance.InitSkillJoy(NGUIJoystick_skill.SkillRangeType.FanSkill, Parse0, Parse1, 0, mLocalPlayer, this.transform);
            //}

            ////2=方形： 长：宽
            //if (mSkillItem.SkillBase.rangeType == 2)
            //{
            //    NGUIJoystick_skill.instance.InitSkillJoy(NGUIJoystick_skill.SkillRangeType.RectangleSkill, Parse0, 0, Parse1, mLocalPlayer, this.transform);
            //}

            ////3=线性+箭头  长：0
            //if (mSkillItem.SkillBase.rangeType == 3)
            //{
            //    NGUIJoystick_skill.instance.InitSkillJoy(NGUIJoystick_skill.SkillRangeType.ArrowSkill, Parse0, 0, 0, mLocalPlayer, this.transform);
            //}

            ////4=圆形： 施法距离：伤害半径 
            //if (mSkillItem.SkillBase.rangeType == 4)
            //{
            //    NGUIJoystick_skill.instance.InitSkillJoy(NGUIJoystick_skill.SkillRangeType.RoundCtrlSkill, Parse0, 0, Parse1, mLocalPlayer, this.transform);
            //}

            //if (mSkillItem.NewSkillBase.rangeType == 4)
            //{
            //    NGUIJoystick_skill.instance.InitSkillJoy(NGUIJoystick_skill.SkillRangeType.RoundShowSkill, radius_, angle_, angle_, mLocalPlayer, this.transform);
            //}

            //NGUIJoystick_skill.instance.target.parent.gameObject.SetActive(true);
            //NGUIJoystick_skill.instance.RotationTarget.gameObject.SetActive(true);
            //NGUIJoystick_skill.instance.OnPress(true);
            //if (NGUIJoystick_skill.instance.skillType == NGUIJoystick_skill.SkillRangeType.RoundCtrlSkill)
            //{
            //    NGUIJoystick_skill.instance.RoundShowTarget.gameObject.SetActive(true);
            //    NGUIJoystick_skill.instance.RoundShowTarget.parent = mLocalPlayer.UUnitInfo.transform.parent;
            //    NGUIJoystick_skill.instance.RoundShowTarget.localPosition = mLocalPlayer.UUnitInfo.transform.localPosition;
            //    //Vector3 rstls  = NGUIJoystick_skill.instance.RoundShowTarget.localScale;
            //    //rstls = Vector3.one * radius_ * 0.01f;
            //    NGUIJoystick_skill.instance.RoundShowTarget.localScale = Vector3.one * radius_ * 0.01f; ;
            //    //NGUIJoystick_skill.instance.RoundShowTarget.localScale = Vector3.one;
            //    NGUIJoystick_skill.instance.RoundShowTarget.localEulerAngles = mLocalPlayer.UUnitInfo.transform.localEulerAngles;
            //}
            //if (NGUIJoystick_skill.instance.skillType == NGUIJoystick_skill.SkillRangeType.ArrowSkill)
            //{
            //    NGUIJoystick_skill.instance.ArrowShowTarget.gameObject.SetActive(true);
            //    NGUIJoystick_skill.instance.ArrowShowTarget.parent = mLocalPlayer.UUnitInfo.transform.parent;
            //    NGUIJoystick_skill.instance.ArrowShowTarget.localPosition = mLocalPlayer.UUnitInfo.transform.localPosition;
            //    //Vector3 rstls  = NGUIJoystick_skill.instance.RoundShowTarget.localScale;
            //    //rstls = Vector3.one * radius_ * 0.01f;
            //    NGUIJoystick_skill.instance.ArrowShowTarget.localScale = Vector3.one * radius_ * 0.01f; ;
            //    //NGUIJoystick_skill.instance.RoundShowTarget.localScale = Vector3.one;
            //    NGUIJoystick_skill.instance.ArrowShowTarget.localEulerAngles = mLocalPlayer.UUnitInfo.transform.localEulerAngles;
            //}

            ////NGUIJoystick_skill.instance.OnPress(true);
            //NGUIJoystick_skill.instance.target.parent.parent.parent = transform.parent;
            //NGUIJoystick_skill.instance.target.parent.parent.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -2);
            //NGUIJoystick_skill.instance.RotationTarget.parent = mLocalPlayer.UUnitInfo.transform.parent;
            //NGUIJoystick_skill.instance.RotationTarget.localPosition = mLocalPlayer.UUnitInfo.transform.localPosition;
            ////NGUIJoystick_skill.instance.RotationTarget.localScale = Vector3.one;

            //if (NGUIJoystick_skill.instance.skillType != NGUIJoystick_skill.SkillRangeType.RoundCtrlSkill)
            //{
            //    NGUIJoystick_skill.instance.RotationTarget.localScale = Vector3.one * radius_ * 0.01f;
            //}
            //else
            //    NGUIJoystick_skill.instance.RotationTarget.localScale = Vector3.one * angle_ * 0.01f;


            //NGUIJoystick_skill.instance.RotationTarget.localEulerAngles = mLocalPlayer.UUnitInfo.transform.localEulerAngles;
		}
		else
		{
			//Data.Action activeAction = mLocalPlayer.ActionStatus.ActiveAction;
		
			//if (mInterruptIndex < activeAction.mActionInterrupts.Count && mInterruptIndex >= 0)
			//	NGUIJoystick_skill.instance.mCurInterrupt = mLocalPlayer.ActionStatus.ActiveAction.ActionInterrupts[mInterruptIndex];
			//else
			//{
			//	for (int i = 0; i < activeAction.mActionInterrupts.Count; i++)
			//	{
			//		Data.ActionInterrupt interrupt = activeAction.mActionInterrupts[i];
			//		if (interrupt.SkillID == mSkillItem.SkillId)
			//			NGUIJoystick_skill.instance.mCurInterrupt = mLocalPlayer.ActionStatus.ActiveAction.ActionInterrupts[i];
			//	}
			//}
			//NGUIJoystick_skill.instance.OnPress(false);
			OnAIPress(true, false);
		}
	}


	public void OnDrag(Vector2 delta)
	{
		if(Camera.main==null)return;

        //NGUIJoystick_skill.instance.SkillButtonOnDrag(Camera.main.transform.eulerAngles, delta);
	}

		//战斗4个技能按钮输入函数
	void OnPress(bool isPressed,bool isAI)
	{
		if (Global.PauseAll)
			return;
		
		// 支持技能按键的检测。
		if (EnableSkillKey && mSkillItem != null && LastSkillId == mSkillItem.SkillId && Global.GMeteorInput != null)
		{
			//if (isPressed)
			//	Global.GMeteorInput.OnKeyDown(EKeyList.KL_SkillAttack,isAI);
			//else
			//	Global.GMeteorInput.OnKeyUp(EKeyList.KL_SkillAttack);
		}
		
		if (!isPressed || !mSkillReady)
			return;

		// the skill stage need check the CD.
		//if (mSkillStage <= 1 && mCD < mSkillCD)
		//if (mCD < mSkillCD)
		//{
		//	if (FightWnd.Exist) FightWnd.Instance.ShowFightMessage("技能CD中");
		//	return;
		//}

		//check the common cd
		//if ( SuperSkillInput.mCommonCD > 0) 
		//if(mLocalPlayer.ActionStatus.ActiveAction.ID.IndexOf('W')==0&&mLocalPlayer.ActionStatus.ActiveAction.ID.IndexOf("W0")!=0)
		//{
		//	//if (FightMainWnd.Exist) FightMainWnd.Instance.ShowFightMessage("公共CD中");
		//	return;		
		//}

		//// link skill now, it may enqueue to some time to play.
		//mLocalPlayer.LinkSkill(this, mInterruptIndex);
		
		// 动作系统需要的监测技能的按键
		if (EnableSkillKey && Global.GMeteorInput != null)
		{
			//if (isPressed)
			//	Global.GMeteorInput.OnKeyDown(EKeyList.KL_SkillAttack,isAI);
			//else
			//	Global.GMeteorInput.OnKeyUp(EKeyList.KL_SkillAttack);
		}
		//mLocalPlayer.AIAction.IsPlayingAction = true;
	}

	/** 返回true表示该技能解锁  false表示未解锁*/
	public static bool isSkillLock(int heroQuality,int heroLevel,string condition){
		string []arr = condition.Split('|');
		int conditionType = int.Parse (arr [0]);
		int conditionValue = int.Parse (arr [1]);
		if (conditionType == 1) {//升品质解锁.
			if(heroQuality >= conditionValue)
				return true;
			else
				return false;
		}
		if (conditionType == 2) {//升等级解锁.
			if(heroLevel >= conditionValue)
				return true;
			else
				return false;
		}
		return false;
	}
}
