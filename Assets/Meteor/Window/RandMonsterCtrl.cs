using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class RandMonsterCtrl : MonoBehaviour {
    public Text nameText;
    public Image progressBar;
    public Image progressMpBar;
    public Text hpText;
    public Text mpText;
    public Image hp;
    List<MonsterEx> group = null;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Attach(List<MonsterEx> monGroup)
    {
        group = monGroup;
        if (group.Count != 0)
        {
            nameText.text = group[0].Name;
            progressBar.fillAmount = 1.0f;
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
        hp.DOFillAmount(target, 0.5f);
        hpText.DOText(hpNow.ToString() + "/" + hpMax.ToString(), 0.5f);
        if (mpText != null)
            mpText.DOText(mpNow.ToString() + "/" + mpMax.ToString(), 0.5f);
        if (progressMpBar != null)
            progressMpBar.DOFillAmount((float)mpNow / (float)mpMax, 0.5f);
    }

    public void OnClick()
    {
        //弹出一个对话框，有3个按钮，一个查看敌方信息，一个战斗，一个离开
        //if (group[0].RandBattleTalk != null && group[0].RandBattleTalk.Count != 0)
        {

            //string[] talk = group[0].RandBattleTalk.Split(new char[] { '#' });
            //if (talk.Length > 1)
            //    U3D.Dialogue(group[0].Name, talk[Random.Range(0, talk.Length - 1)]);
            //U3D.AddDiaMenu("敌方信息", OnViewGroupInfo);
            //U3D.AddDiaMenu("战斗", OnBattle);
            //U3D.AddDiaMenu("离开", OnLeave);
        }
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
