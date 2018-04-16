using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
public class FightWndEx: Window<FightWndEx>
{
    public override string PrefabName { get { return "FightWndEx"; } }
    protected override bool OnOpen()
    {
        ObjectInit();
        Init();
        return true;
    }

    protected override bool OnShow()
    {
        int size = UIMoveControl.mUIMoveControlList.Count;
        for (int i = 0; i < size; i++)
        {
            UIMoveControl.mUIMoveControlList[i].ShowAction();
        }

        return base.OnShow();
    }

    public override void Show()
    {
        base.Show();
    }

    void ObjectInit()
    {
        
    }

    //Text timeLabel;
    //Image weaponType;

    Image hpBar;
    Image mpBar;
    Image angryBar;
    Text hpLabel;
    Text mpLabel;
    //Text NameLabel;
    Text LvLabel;

    Image hpWarning;
    Image angryWarning;
    
    //Image exp;
    //Dropdown dr;
    //Dropdown effectDr;
    //Button currentPos;

    //Text fightText;
    void Init()
    {
        //fightText = Global.ldaControlX("FightText", WndObject).GetComponent<Text>();
        Global.ldaControlX("Attack", WndObject).GetComponent<GameButton>().OnPress.AddListener(OnAttackPress);
        Global.ldaControlX("Attack", WndObject).GetComponent<GameButton>().OnRelease.AddListener(OnAttackRelease);
        Global.ldaControlX("Defence", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnDefencePress);
        Global.ldaControlX("Defence", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnDefenceRelease);
        Global.ldaControlX("Jump", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnJumpPress);
        Global.ldaControlX("Jump", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnJumpRelease);
        Global.ldaControlX("ChangeWeapon", WndObject).GetComponentInChildren<Button>().onClick.AddListener(OnChangeWeapon);
        Global.ldaControlX("Unlock", WndObject).GetComponentInChildren<Button>().onClick.AddListener(OnUnlock);
        //Global.ldaControlX("System", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(() => { U3D.OpenSystem(); });
        //weaponType = ldaControl("WeaponType", WndObject).gameObject.GetComponent<Image>();
        hpWarning = ldaControl("HPFlashWarning", WndObject).gameObject.GetComponent<Image>();
        angryWarning = ldaControl("AngryWarning", WndObject).gameObject.GetComponent<Image>();
        hpBar = ldaControl("HPBar", WndObject).gameObject.GetComponent<Image>();
        mpBar = ldaControl("MPBar", WndObject).gameObject.GetComponent<Image>();
        angryBar = ldaControl("AngryBar", WndObject).gameObject.GetComponent<Image>();
        hpLabel = ldaControl("HPLabel", WndObject).gameObject.GetComponent<Text>();
        mpLabel = ldaControl("MPLabel", WndObject).gameObject.GetComponent<Text>();
        //NameLabel = ldaControl("NameLabel", WndObject).gameObject.GetComponent<Text>();
        LvLabel = ldaControl("LVLabel", WndObject).gameObject.GetComponent<Text>();
        //exp = ldaControl("Exp", WndObject).gameObject.GetComponent<Image>();
        if (MeteorManager.Instance.LocalPlayer != null)
        {
            //int SubType = MeteorManager.Instance.LocalPlayer.GetWeaponType();
            //string weaponIcon = "";
            //switch ((EquipWeaponType)SubType)
            //{
            //    case EquipWeaponType.Knife: weaponIcon = "FW05"; break;
            //    case EquipWeaponType.Sword: weaponIcon = "FW06"; break;
            //    case EquipWeaponType.Blade: weaponIcon = "FW08"; break;
            //    case EquipWeaponType.Lance: weaponIcon = "FW07"; break;
            //    case EquipWeaponType.Gun: weaponIcon = "FW03"; break;
            //}
            //weaponType.overrideSprite = Resources.Load<Sprite>(weaponIcon);
            angryBar.fillAmount = 0.0f;
            angryWarning.enabled = false;
            hpWarning.enabled = false;
            UpdatePlayerInfo();
        }
    }

    //int currentPosIdx;
    //public void UpdatePoseStatus(int idx, int frame = 0)
    //{
    //    if (currentPos != null)
    //    {
    //        currentPosIdx = idx;
    //        currentPos.GetComponentInChildren<Text>().text = "Pose " + idx + " Frames " + frame;
    //    }
    //}
    IEnumerator ShowHPWarning()
    {
        while (MeteorManager.Instance.LocalPlayer.Attr.TotalHp * 0.1f > MeteorManager.Instance.LocalPlayer.Attr.hpCur)
        {
            hpWarning.enabled = !hpWarning.enabled;
            yield return new WaitForSeconds(0.5f);//半秒切换一次状态
        }
        hpWarning.enabled = false;
    }

    IEnumerator ShowAngryWarning()
    {
        while (MeteorManager.Instance.LocalPlayer.AngryValue == Global.ANGRYMAX)
        {
            angryWarning.enabled = !angryWarning.enabled;
            yield return new WaitForSeconds(0.5f);//半秒切换一次状态
        }
        angryWarning.enabled = false;
    }

    void OnAttackPress()
    {
        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyDown(EKeyList.KL_Attack, false);//也可看作普攻
    }

    void OnUnlock()
    {
        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
        GameBattleEx.Instance.Unlock();
    }

    void OnChangeWeapon()
    {
        if (!MeteorManager.Instance.LocalPlayer.posMng.CanChangeWeapon)
            return;

        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyDown(EKeyList.KL_ChangeWeapon, false);
    }

    void OnAttackRelease()
    {
        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyUp(EKeyList.KL_Attack);
    }

    void OnDefencePress()
    {
        if (!MeteorManager.Instance.LocalPlayer.posMng.CanDefence)
            return;

        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyDown(EKeyList.KL_Defence, true);//不要被键盘状态同步，否则按下马上就抬起，那么防御姿势就消失了
        
    }

    void OnDefenceRelease()
    {
        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyUp(EKeyList.KL_Defence);
    }

    void OnJumpPress()
    {
        //if (!MeteorManager.Instance.LocalPlayer.posMng.CanJump)
        //    return;

        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyDown(EKeyList.KL_Jump, false);//
    }

    void OnJumpRelease()
    {
        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyUp(EKeyList.KL_Jump);
    }

    //按爆气.
    public void OnBreakOut()
    {
        Debug.Log("OnBreakOut");
        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll)
        {
            //Debug.Log("Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll");
            return;
        }
        if (MeteorManager.Instance.LocalPlayer.AngryValue >= 60)
        {
            Global.GMeteorInput.OnKeyDown(EKeyList.KL_BreakOut, false);
            //Debug.Log("OnKeyDown");
        }
    }

    int lastAngry = 0;
    public void UpdateAngryBar()
    {
        if (MeteorManager.Instance.LocalPlayer != null)
        {
            angryBar.fillAmount = (float)MeteorManager.Instance.LocalPlayer.AngryValue /(float)Global.ANGRYMAX;
            if (MeteorManager.Instance.LocalPlayer.AngryValue == Global.ANGRYMAX && lastAngry != Global.ANGRYMAX)
            {
                if (angryWarningE != null)
                    GameBattleEx.Instance.StopCoroutine(angryWarningE);
                angryWarningE = GameBattleEx.Instance.StartCoroutine(ShowAngryWarning());
                lastAngry = Global.ANGRYMAX;
            }
            else
                lastAngry = MeteorManager.Instance.LocalPlayer.AngryValue;
        }
    }

    int lastHp = 0;
    public void UpdateHpBar()
    {
        if (MeteorManager.Instance.LocalPlayer != null)
        {
            hpBar.fillAmount = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.TotalHp;
            if ((float)MeteorManager.Instance.LocalPlayer.Attr.TotalHp * 0.1f >= (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur && (float)MeteorManager.Instance.LocalPlayer.Attr.TotalHp * 0.1f < lastHp)
            {
                GameBattleEx.Instance.StartCoroutine(ShowHPWarning());
                lastHp = MeteorManager.Instance.LocalPlayer.Attr.hpCur;
            }
            else
                lastHp = MeteorManager.Instance.LocalPlayer.Attr.hpCur;
        }
    }

    //public void UpdateTime(string label)
    //{
    //    timeLabel.text = label;
    //}

    public bool DefenceLongPress = false;

    protected override bool OnClose()
    {
        return base.OnClose();
    }

    private void OnClickPause(GameObject go)
    {
        int size = UIMoveControl.mUIMoveControlList.Count;
        for (int i = 0; i < size; i++)
        {
            UIMoveControl.mUIMoveControlList[i].SetEndPos();
        }
        if (Exist)
            WndObject.SetActive(false);
        if (!PauseWnd.Exist)
            PauseWnd.Instance.Open();
    }

    //弹出提示.
    public void ShowFightMessage(string text)
    {
        U3D.PopupTip(text);
    }

    
    //public void OnPlayerDead(MeteorUnit player)
    //{
    //    if (player == MeteorManager.Instance.LocalPlayer)
    //    {
    //        MeteorManager.Instance.OnUnitDead(player);
    //        OnBattleFailed();
    //    }
    //    else
    //    {
    //        MeteorManager.Instance.OnUnitDead(player);
    //        if (player.Camp == EUnitCamp.EUC_ENEMY)
    //        {
    //            //无论谁杀死，都要爆东西
    //            DropMng.Instance.Drop(player);

    //            //如果是被任意流星角色的伤害，特效，技能，等那么主角得到经验，如果是被场景环境杀死，
    //            //爆钱和东西
    //            //检查剧情
    //            //检查任务
    //            //检查过场对白，是否包含角色对话
    //        }
    //    }
    //}

    public void OnBattleFailed()
    {
        U3D.PopupTip("任务失败");
    }

    public void UpdateMonsterInfo(MeteorUnit mon)
    {
        
    }

    IEnumerator UpdateHPMP()
    {
        float targetValueHp = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.TotalHp;
        float targetValueMp = (float)MeteorManager.Instance.LocalPlayer.Attr.mpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.MpMax;
        float tick = 0.0f;
        while (true)
        {
            hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, targetValueHp, tick);
            mpBar.fillAmount = Mathf.Lerp(mpBar.fillAmount, targetValueMp, tick);
            tick += Time.deltaTime;
            if (tick >= 1.0f)
                yield break;
            yield return new WaitForEndOfFrame();
        }
    }

    Coroutine updateValue;
    Coroutine hpWarningE;
    Coroutine angryWarningE;
    public void UpdatePlayerInfo()
    {
        if (MeteorManager.Instance.LocalPlayer.Attr != null)
        {
            if (updateValue != null)
                GameBattleEx.Instance.StopCoroutine(updateValue);
            updateValue = Startup.ins.StartCoroutine(UpdateHPMP());
            hpLabel.text = MeteorManager.Instance.LocalPlayer.Attr.hpCur + "/" + MeteorManager.Instance.LocalPlayer.Attr.HpMax;
            //int level = MeteorManager.Instance.LocalPlayer.Attr.Level;
            //uint curlevelExp = (GameData.expMng.GetRowByIdx(level) as ExpBaseEx).Exp;

            //uint lastlevelExp = (GameData.expMng.GetRowByIdx(Mathf.Max(level - 1, 1)) as ExpBaseEx).Exp;
            //if (lastlevelExp == curlevelExp)
            //    exp.fillAmount = 0.0f;
            //else
            //    exp.fillAmount = (float)(GameData.MainRole.baseInfo.Exp - lastlevelExp) / (float)(curlevelExp - lastlevelExp);

            //hpBar.fillAmount = 
            float hpPercent = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.TotalHp;
            if (hpPercent <= 0.1f)
            {
                if (hpWarningE != null)
                    GameBattleEx.Instance.StopCoroutine(hpWarningE);
                if (!MeteorManager.Instance.LocalPlayer.Attr.Dead)
                    hpWarningE = GameBattleEx.Instance.StartCoroutine(ShowHPWarning());
            }
            //mpLabel.text = PlayerEx.Instance.Heros[0].mpCur + "/" + PlayerEx.Instance.Heros[0].MpMax;
            //mpBar.fillAmount = (float)PlayerEx.Instance.Heros[0].mpCur / (float)PlayerEx.Instance.Heros[0].MpMax;

            //NameLabel.text = PlayerEx.Instance.Heros[0].Name;
            //LvLabel.text = "lv:" + level;
        }
    }

    //调试输入内容
    //Dictionary<EKeyList, string> keystring = new Dictionary<EKeyList, string>();
    //bool init = false;
    //public void UpdateInputInfo()
    //{
        //if (!init)
        //{
        //    keystring.Add(EKeyList.KL_Jump, "K");
        //    keystring.Add(EKeyList.KL_Attack, "J");
        //    keystring.Add(EKeyList.KL_KeyD, "→");
        //    keystring.Add(EKeyList.KL_KeyA, "←");
        //    keystring.Add(EKeyList.KL_KeyW, "↑");
        //    keystring.Add(EKeyList.KL_KeyS, "↓");
        //    init = true;
        //}
        //string s = "";
        //for (int i = Global.GMeteorInput.Record.Count - 1; i >= 0 ; i--)
        //{
        //    if (keystring.ContainsKey(Global.GMeteorInput.Record[i].key))
        //        s += keystring[Global.GMeteorInput.Record[i].key] + "\n";
        //}
        //if (fightText != null)
        //    fightText.text = s;
    //}
}