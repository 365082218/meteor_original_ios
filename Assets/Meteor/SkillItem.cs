using UnityEngine;
using System;
using System.Collections.Generic;

public enum ESkillType
{

    Super = 4,
}

//add by Lindean 20141031
public class SkillItem
{
	/**技能ID*/
	public int SkillId;
	//技能配置表Skill.txt读上来的属性
	SkillBase mNewSkillBase;
	public SkillBase SkillBase { get { return mNewSkillBase; } }
	/**技能输入*/
	public SkillInput SkillInput = null;
	///**Buff触发方式,EBuffMode*/
	//public string BuffModes { get { return mNewSkillBase.buffModes; } }
	///**Skill Buff触发概率0-9999(SkillInput.cs里)*/
	//public string BuffChances { get { return mNewSkillBase.buffChances; } }
	///**作用对象,跟符文有关系,是作用在自己还是目标上 EBuffTarget */
	//public string BuffTargets { get { return mNewSkillBase.buffTargets; } }
	///**BuffID*/
	//public string BuffIDs { get { return mNewSkillBase.buffIDs; } }

	//public string Name{get {return mNewSkillBase.name;}}

	//public int EffectType{get{return mNewSkillBase.skillEffectType;}}

//	string[] mNewBuffIDs= null;
//	public string[] NewBuffIDs
//	{
//		get
//		{
//			if(mNewBuffIDs==null)
//			{
//				if(string.IsNullOrEmpty(mNewSkillBase.NewBuffIDs))
//				{
//					string[] tem= {};
//					mNewBuffIDs = tem;
//				}
//				else
//				{
//					mNewBuffIDs = mNewSkillBase.NewBuffIDs.Split('|');
//				}
//			}
//			return mNewBuffIDs;
//		}
//	}

    /** 技能伤害类型 ESkillHurtType */
    public ESkillHurtType SkillHurtType { get { return (ESkillHurtType)(int)mNewSkillBase.hurtType; } }
	
	/**技能顺序*/
	public ESkillType SkillType { get { return (ESkillType)(int)mNewSkillBase.skillType; } }

	/**释放技能需要消耗的能量*/
	public int Energy1 { get { return mNewSkillBase.energy; } }
	/*技能释放的动作(动作编辑器)*/
	public string Action1 { get { return mNewSkillBase.action; } }
	/**技能按钮图标*/
	public string BtnIcon1 { get { return mNewSkillBase.icon; } }
	/**技能冷却时间*/
	public float CD { get { return mNewSkillBase.cdTime * 0.001f ; } }

	public SmartInt hurtPercent{ get {return mNewSkillBase.hurtPercent;}}
	public SmartInt hurtNum{ get {return (SmartInt)(mNewSkillBase.initHurt + mNewSkillBase.upHurt * level);}}
    /** 技能攻击距离 */
    public float AttackRange;//{ get { return mAttackRange; } }
    /** 技能攻击距离的平方 */
    public float AttackRangeSqrLen; //{ get { return (float)mNewSkillBase.attackRange * 0.01f; } }
	/**技能等级*/ 
	public int level=1;
	/**初始固伤值*/
	public int initHurt{get{return mNewSkillBase.initHurt;}}
	/**每级固伤成长*/
	public float upHurt{get{return mNewSkillBase.upHurt;}}

    /**技能计时用的*/
    float SkillCDTime = 0;
    public bool IsSkillReady()
    {
        if (SkillCDTime <= 0)
            return true;
        else
            return false;
    }
    
    public float GetSillCurCD()
    {
        return SkillCDTime;
    }

	public void SetSillCurCD(float cd)
	{
		SkillCDTime = cd;
	}

    public void ResetSkillCDTime()
    {
        SkillCDTime = this.CD;
    }

    public void UpdateSkillCDTime(float deltaTime)
    {
        if (SkillCDTime > 0)
            SkillCDTime -= deltaTime;
    }


	public SkillItem(int skillId)
	{
		InitSkill(skillId);
	}


	public SkillItem(int skillId, MeteorUnit player)
	{
		InitSkill(skillId);
	}

	private void InitSkill(int skillId)
	{
		SkillId = skillId;
		mNewSkillBase = SkillBaseManager.Instance.GetItem(skillId);
		if (mNewSkillBase == null)
			Debug.LogError("Skill not found in [NewSkillBase] table: " + mNewSkillBase);
		
		//ResetSkillCDTime();
		
		//攻击距离
		//        AttackRange = (float)mNewSkillBase.attackRange * 0.001f;
		AttackRange = (float )mNewSkillBase.attackDistance * 0.01f;
		AttackRangeSqrLen = AttackRange * AttackRange;
	}

	public int damage = 0;
	public int time;



	bool isLock=false;
	public bool Lock
    {
        get 
        {
            ////调技能，锁定技能
            //if (SkillId == 10092)
            //    return false;
            //else
            //    return true;

            return isLock;
        } 
        set
        {
            isLock  =value;
        }
    }



}
