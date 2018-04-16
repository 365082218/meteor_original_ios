using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PetSkillInput : MonoBehaviour {
	float CommonCD = 0.0f;
	MeteorUnit mPlayer;
	public static bool isPetCanSkill = true;

	int LastSkillID = 0;
	float[] mSkillsCD = {0.0f, 0.0f, 0.0f, 0.0f};
	public float[] SkillsCD{set {mSkillsCD =value;} get{return mSkillsCD;}}
	int[] mSkillsID = {0, 0, 0, 0};
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (mCurSkillmAnimTime > 0)
		{
			mCurSkillmAnimTime -= Time.deltaTime;
			return;
		}

		for (int i = 0; i < 4; i++)
		{
			if (mSkillsCD[i] > 0)
				mSkillsCD[i] -= Time.deltaTime;
		}
	}

	public void initSkills(MeteorUnit player)
	{
		mPlayer = player;
		//int _size = mPlayer.SillItems.Count;
		//for (int i = 0; i < _size; i++)
		//{
		//	mSkillsID[i] = mPlayer.SillItems[i].SkillId;
		//}
	}

	public bool IsSkillReady(int skillid)
	{
		bool bFind = false;
		int nIndex = 0;
		foreach (var each in mSkillsID)
		{
			if (each == skillid)
			{
				bFind = true;
				break;
			}
			nIndex++;
		}

		if (bFind && nIndex < mSkillsCD.Length)
			return mSkillsCD[nIndex] <= 0;
		return false;
	}


	public void PlaySkill(int skillid)
	{
		//foreach(NewSkillItem item in mPlayer.SillItems)
		//{
		//	if (item.SkillId != skillid)
		//		continue;
		//	if (Global.PauseAll)
		//		return;
		//	if (mCurSkillmAnimTime > 0)
		//		return;

		//	int mInterruptIndex = -1;
		//	Pose activeAction = mPlayer.posMng.mActiveAction;
		//	for (int i = 0; i < activeAction.mActionInterrupts.Count; i++)
		//	{
		//		Data.ActionInterrupt interrupt = activeAction.mActionInterrupts[i];
		//		if (interrupt.SkillID != item.SkillId)
		//			continue;
		//		mInterruptIndex = i;
		//	}
		//	// link skill now, it may enqueue to some time to play.
		//	if (mInterruptIndex < 0)
		//		return;
		//	mSkillItem = item;
		//	CommonCD = 5.0f;
		//	mPlayer.LinkSkill(null, mInterruptIndex, this);
		//	item.ResetSkillCDTime();
		//	return;
		//}
	}

	public void PlaySkill()
	{
		//if (mPlayer.BuffManager.CanSkill)
		//{
		//	//sort by priority
		//	List<SkillJudge> CanUse = new List<SkillJudge>();
		//	Dictionary<int, Unit> TargetMap = new Dictionary<int, Unit>();
		//	Unit targetTmp = null;
		//	foreach (var each in mPlayer.SillItems)
		//	{
		//		bool FriendArea = false;
		//		if (each.Lock)
		//			continue;
		//		//Skill Distance
		//		//0 敌方1 我方2 敌方英雄3 我方英雄4 自身
		//		switch (each.NewSkillBase.targetObject)
		//		{
		//		case 0:
		//		case 2:
		//			targetTmp = mPlayer.AIFollowAction.mEnemy;
		//			break;
		//		case 1:
		//		case 3:
		//			targetTmp = mPlayer.AIFollowAction.mFriend;
		//			FriendArea = true;
		//			break;
		//		case 4:
		//			targetTmp = mPlayer as Unit;
		//			break;
		//		default:
		//			Debug.LogError("skill can not find target");
		//			break;
		//		}

		//		if (!IsSkillReady(each.SkillId))
		//			continue;
		//		if (targetTmp == null && !FriendArea)
		//			continue;
		//		if (!FriendArea)
		//			TargetMap.Add(each.SkillId, targetTmp);

		//		if (FriendArea)
		//		{
		//			List<Unit> friendList = new List<Unit>();
		//			for (int i = 0; i < FightMainWnd.mFightHeros.Count; i++)
		//			{
		//				int nInsertIndex = 0;
		//				float fInsertHP = (float)FightMainWnd.mFightHeros[i].GetAttrib(EHDVO.CurHP) / (float)FightMainWnd.mFightHeros[i].GetAttrib(EHDVO.MaxHP);
		//				for (int j = 0; j < friendList.Count; j++)
		//				{
		//					float fHP = (float)friendList[j].GetAttrib(EHDVO.CurHP) / (float)friendList[j].GetAttrib(EHDVO.MaxHP);
		//					if (fHP >= fInsertHP)
		//						break;
		//					else
		//						nInsertIndex++;
		//				}
		//				friendList.Insert(nInsertIndex, FightMainWnd.mFightHeros[i]);
		//			}

		//			for (int i = 0; i < friendList.Count; i++)
		//			{
		//				bool bBreak = false;
		//				foreach (var eachSkillJudge in each.mSkillJudge)
		//				{
		//					if (eachSkillJudge.Judge(mPlayer, friendList[i], Time.timeSinceLevelLoad, 0, mPlayer.GetBattleTime()))
		//					{
		//						bBreak = true;
		//						int nInsertIndex = 0;
		//						foreach (var eachItem in CanUse)
		//						{
		//							if (eachSkillJudge.usePriority < eachItem.usePriority)
		//								break;
		//							nInsertIndex++;
		//						}
		//						CanUse.Insert(nInsertIndex, eachSkillJudge);
		//						break;
		//					}
		//					else
		//					{
		//						if (each.SkillId == mPlayer.CacheSkillId && friendList[i] == mPlayer.CacheSkillTarget)
		//						{
		//							mPlayer.CacheSkillId = 0;
		//							mPlayer.CacheSkillTarget = null;
		//						}
		//					}
		//				}
		//				if (bBreak)
		//				{
		//					TargetMap.Add(each.SkillId, friendList[i]);
		//					break;
		//				}
		//			}
		//		}
		//		else
		//		{
		//			foreach (var eachSkillJudge in each.mSkillJudge)
		//			{
		//				if (eachSkillJudge.Judge(mPlayer, targetTmp, Time.timeSinceLevelLoad, 0, mPlayer.GetBattleTime()))
		//				{
		//					int nInsertIndex = 0;
		//					foreach (var eachItem in CanUse)
		//					{
		//						if (eachSkillJudge.usePriority < eachItem.usePriority)
		//							break;
		//						nInsertIndex++;
		//					}
		//					CanUse.Insert(nInsertIndex, eachSkillJudge);
		//					break;
		//				}
		//				else
		//				{
		//					if (each.SkillId == mPlayer.CacheSkillId && targetTmp == mPlayer.CacheSkillTarget)
		//					{
		//						mPlayer.CacheSkillId = 0;
		//						mPlayer.CacheSkillTarget = null;
		//					}
		//				}
		//			}
		//		}
		//	}
			
		//	if (CanUse.Count != 0)
		//	{
		//		if (CanUse[0].Owner.NewSkillBase.targetObject == 4)
		//		{
		//			CanUse[0].DoAction(mPlayer, mPlayer);
		//			mPlayer.OnKeyUpAttack();
		//		}
		//		else if (CanUse[0].Owner.NewSkillBase.attackDistance * 0.01f < Vector3.Distance(mPlayer.Position, TargetMap[CanUse[0].Owner.SkillId].Position))
		//		{
		//			mPlayer.CacheSkillId = CanUse[0].Owner.SkillId;
		//			mPlayer.CacheSkillTarget = TargetMap[CanUse[0].Owner.SkillId];
		//		}
		//		else
		//		{
		//			CanUse[0].DoAction(mPlayer, TargetMap[CanUse[0].Owner.SkillId]);
		//			mPlayer.OnKeyUpAttack();
		//			if (mPlayer.CacheSkillId == CanUse[0].Owner.SkillId)
		//			{
		//				mPlayer.CacheSkillId = 0;
		//				mPlayer.CacheSkillTarget = null;
		//			}
		//		}
		//		return;
		//	}
		//}
	}

	private SkillItem mSkillItem;
	private int mSkillStage = 0;
	List<MeteorUnit> mBuffTargets = null;
	float mCurSkillmAnimTime = 0.0f;

	public void PlayAction()
	{
		// super skill cost soul, and the other cost ability.
		string skillAction = "";
		if (mSkillItem == null)
			return;
		//if (mSkillItem.SkillType == ESkillType.Super)
		//{
		//	int curAbility = mPlayer.GetAttrib(EHDVO.CurAbility);
			
		//	if (curAbility >= mSkillItem.Energy1 && !string.IsNullOrEmpty(mSkillItem.Action1))
		//	{
		//		skillAction = mSkillItem.Action1;
		//		mPlayer.AddAbility(-mSkillItem.Energy1);
		//		mSkillStage = 1;
		//	}
		//	else//add by Lindean
		//		return;//技能释放不成功
		//}
		//else
		//{
		//	if (mSkillStage <= 1)
		//	{
		//		skillAction = mSkillItem.Action1;
		//		//add // by zhj 20150108
		//		//				mLocalPlayer.AddAbility(-mSkillItem.Energy1); 
		//	}
		//}
		
		//if (mBuffTargets != null)
		//	mBuffTargets.Clear();
		
		//// 处理加给自身的buff
		//ProcessBuff(mPlayer, EBuffMode.PlaySkill);
		
		//// 设置技能的段数。
		//// mSkillItem.SkillStage = mSkillStage;
		
		//// 执行动作。
		//mPlayer.PlaySkill(mSkillItem, skillAction);
		//if (MainScript.Instance.DebugNoCDTime)
		//	return;

		//int idx = 0;
		//for (idx = 0; idx  < 4; idx++)
		//{
		//	if (mSkillsID[idx] == mSkillItem.SkillId)
		//		break;
		//}

		//mSkillItem.ResetSkillCDTime();
		//mSkillsCD[idx] = mSkillItem.GetSillCurCD();
		//CommonCD = 2.0f;

		//mCurSkillmAnimTime = mPlayer.CachedAnimationState.length;
		

		//mPlayer.AIFollowAction.IsPlayingAction = true;
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
//		    tmpBuffIDs.Length != tmpBuffChances.Length ||
//		    tmpBuffIDs.Length != tmpBuffModes.Length)
//		{
//			Debug.Log("Skill buff set error!");
//			return;//Skill Buff 配置数据不对
//		}
//		for(int i=0; i < tmpBuffIDs.Length; i++)
//		{
//			//if(int.Parse(tmpBuffModes[i]) == (int)mode)
//			//    ProcessBuff(target, int.Parse(tmpBuffChances[i]), 
//			//                int.Parse(tmpBuffTargets[i]), int.Parse(tmpBuffIDs[i]));
//			if (int.Parse(tmpBuffIDs[i]) == 0 ||
//			    int.Parse(tmpBuffChances[i]) < Random.Range(0, 10001))
//				return;
//			
//			if (int.Parse(tmpBuffTargets[i]) == (int)EBuffTarget.Self)
//				mPlayer.AddBuff(int.Parse(tmpBuffIDs[i]));
//		}
		//		//add by Lindean
		//		ProcessBuff(target, mSkillItem.BuffChance, 
		//		            mSkillItem.BuffTarget, mSkillItem.BuffID);
	}
}
