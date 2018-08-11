using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

//任意怪物控制器，地形组合+载入Npc攻击
public class RandMonsterCtrl : MonoBehaviour {
    List<MonsterEx> group = null;
    public List<HitBox> hitGroup;
    public List<FixedPlatformCtrl> platformGroup;
    public int Hp;//血值和相关属性.
    public int Pose;//当死亡时，所有子物体播放动画
    // Use this for initialization
    private void Awake()
    {
        
    }

    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    int CalcDamage(MeteorUnit attacker, AttackDes attack = null)
    {
        //(((武器攻击力 + buff攻击力) x 招式攻击力） / 100) - （敌方武器防御力 + 敌方buff防御力） / 10
        //你的攻击力，和我的防御力之间的计算
        //attacker.damage.PoseIdx;
        int DefTmp = 0;
        AttackDes atk = attacker.CurrentDamage;
        if (atk == null)
            atk = attack;
        int WeaponDamage = attacker.CalcDamage();
        int PoseDamage = MenuResLoader.Instance.FindOpt(atk.PoseIdx, 3).second[0].flag[6];
        int BuffDamage = attacker.Attr.CalcBuffDamage();
        int realDamage = Mathf.FloorToInt((((WeaponDamage + BuffDamage) * PoseDamage) / 100.0f - (DefTmp)));
        return realDamage;
    }

    public void OnDamage(MeteorUnit attacker, AttackDes attck = null)
    {
        
    }

    public void OnDamagedByUnit(MeteorUnit u, AttackDes des)
    {
        int realDamage = CalcDamage(u, des);
        Hp -= realDamage;
        if (Hp <= 0)
        {
            for (int i = 0; i < hitGroup.Count; i++)
            {
                hitGroup[i].enabled = false;
            }

            for (int i = 0; i < platformGroup.Count; i++)
                platformGroup[i].GetComponent<FMCPlayer>().ChangePose(Pose, 0);

            GameObject.Destroy(gameObject, 5);
        }
    }

    public void Attach(List<MonsterEx> monGroup)
    {
        group = monGroup;
        if (group.Count != 0)
        {
            UpdateUI();
        }
        else
        {

        }
    }

    public void UpdateUI()
    {
        int hpNow = 0;
        int hpMax = 0;
        int mpNow = 0;
        int mpMax = 0;
        for (int i = 0; i < group.Count; i++)
        {
            hpNow += group[i].hpCur;
            hpMax += group[i].HpMax;
            mpNow += group[i].mpCur;
            mpMax += group[i].MpMax;
        }
        float target = (float)hpNow / (float)hpMax;
    }

    public void OnClick()
    {

    }

    public void OnViewGroupInfo()
    {
        //弹出信息界面显示怪物组信息
    }

    public void OnBattle()
    {
        //开始战斗
        //SceneMng.StartRandomBattle(group);
    }

    public void OnLeave()
    {
        //U3D.DiaClose();
    }
}
