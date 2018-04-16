using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
public class FightWnd: Window<FightWnd>
{
    public override string PrefabName { get { return "FightWnd"; } }
    protected override bool OnOpen()
    {
        ObjectInit();
        Init();
        return true;
    }
    protected override bool FullStretch()
    {
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

    Text timeLabel;
    //Image weaponType;

    Image hpBar;
    Image mpBar;
    Image angryBar;
    Text hpLabel;
    Text mpLabel;
    //Text NameLabel;
    Text LvLabel;
    //UIButtonExtended floatButton;//浮动按钮，同时只有一个.负责触发到传送-平台时，开启对应的功能与否
    Image hpWarning;
    Image angryWarning;
    
    Image exp;
    Dropdown dr;
    Dropdown effectDr;
    Button currentPos;
    GameObject debugPanel;

    //当前攻击对象控件
    GameObject targetInfo;
    Image targetHp;
    Text targetName;
    //Text targetTitleInfo;
    Text targetHpInfo;
    //Text targetBuffInfo;

    void OnDebugAI()
    {
        if (DebugWnd.Exist)
            DebugWnd.Instance.Close();
        else
            DebugWnd.Instance.Open();
    }

    void OnPlayerInfo()
    {
        if (PlayerWnd.Exist)
            PlayerWnd.Instance.Close();
        else
            PlayerWnd.Instance.Open();
    }

    void OnChangeDebug()
    {
        debugPanel.SetActive(!debugPanel.activeSelf);
    }
    AutoMsgCtrl ctrl;
    Transform LevelTalkRoot;
    Animation actionStatusBarCtrl;
    GameObject FloatOpen;
    GameObject BuffRoot;
    void Init()
    {
        debugPanel = Control("Debug");
        LevelTalkRoot = Global.ldaControlX("LevelTalk", WndObject).transform;
        ctrl = LevelTalkRoot.GetComponent<AutoMsgCtrl>();
        ctrl.SetConfig(3.0f, 2f);
        FloatOpen = Control("FloatOpen");
        BuffRoot = Control("BuffPanel");
        FloatOpen.GetComponent<Button>().onClick.AddListener(OnChangeActionBarStatus);
        actionStatusBarCtrl = Control("Slots").GetComponent<Animation>();
        Global.ldaControlX("Attack", WndObject).GetComponent<GameButton>().OnPress.AddListener(OnAttackPress);
        Global.ldaControlX("Attack", WndObject).GetComponent<GameButton>().OnRelease.AddListener(OnAttackRelease);
        Global.ldaControlX("Defence", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnDefencePress);
        Global.ldaControlX("Defence", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnDefenceRelease);
        Global.ldaControlX("Jump", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnJumpPress);
        Global.ldaControlX("Jump", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnJumpRelease);
        Global.ldaControlX("ChangeWeapon", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnChangeWeaponPress);
        Global.ldaControlX("ChangeWeapon", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnChangeWeaponRelease);
        Global.ldaControlX("BreakOut", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnBreakOut);
        Global.ldaControlX("ChangeDebug", WndObject).GetComponentInChildren<Button>().onClick.AddListener(OnChangeDebug);
        Global.ldaControlX("AddAI", WndObject).GetComponentInChildren<Button>().onClick.AddListener(OnDebugAI);
        Global.ldaControlX("HeroHead", WndObject).GetComponent<GameButton>().OnPress.AddListener(OnPlayerInfo);
        //Global.ldaControlX("Inventory", WndObject).GetComponentInChildren<Button>().onClick.AddListener(delegate () { U3D.OpenInVentory(); });
        Global.ldaControlX("System", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenSystem(); });
        Global.ldaControlX("Unlock", WndObject).GetComponentInChildren<Button>().onClick.AddListener(OnClickChangeLock);
        //floatButton = Global.ldaControlX("FloatButton", WndObject).GetComponent<UIButtonExtended>();
        //floatButton.gameObject.SetActive(false);
        dr = Global.ldaControlX("Dropdown", WndObject).GetComponent<Dropdown>();
        effectDr = Global.ldaControlX("EffectList", WndObject).GetComponent<Dropdown>();
        Button effectPlay = Global.ldaControlX("PlayEffect", WndObject).GetComponent<Button>();

        //targetInfo
        targetInfo = Global.ldaControlX("TargetInfo", WndObject);
        targetHp = Global.ldaControlX("TargetHp", targetInfo).GetComponent<Image>();
        targetName = Global.ldaControlX("TargetName", targetInfo).GetComponent<Text>();
        //targetTitleInfo = Global.ldaControlX("TargetTitleInfo", targetInfo).GetComponent<Text>();
        targetHpInfo = Global.ldaControlX("TargetHpInfo", targetInfo).GetComponent<Text>();
        //targetBuffInfo = Global.ldaControlX("TargetBuffInfo", targetInfo).GetComponent<Text>();
        //
        targetInfo.SetActive(false);

        Button replay = Global.ldaControlX("Replay", WndObject).GetComponent<Button>();
        currentPos = Global.ldaControlX("CurrentPose", WndObject).GetComponent<Button>();
        var skillList = Global.ldaControlX("SkillList", WndObject).GetComponent<Dropdown>();
        skillList.options.Add(new Dropdown.OptionData("匕首大绝-259"));
        skillList.options.Add(new Dropdown.OptionData("镖大绝-203"));
        skillList.options.Add(new Dropdown.OptionData("火枪大绝-216"));
        skillList.options.Add(new Dropdown.OptionData("双刺大绝-244"));
        skillList.options.Add(new Dropdown.OptionData("枪大绝-293"));
        skillList.options.Add(new Dropdown.OptionData("匕首大绝-259"));
        skillList.options.Add(new Dropdown.OptionData("刀大绝-310"));
        skillList.options.Add(new Dropdown.OptionData("爆气-367"));
        skillList.options.Add(new Dropdown.OptionData("剑大绝-368"));
        skillList.options.Add(new Dropdown.OptionData("拳套大绝-421"));
        skillList.options.Add(new Dropdown.OptionData("乾坤刀大绝-451"));
        skillList.options.Add(new Dropdown.OptionData("忍天地同寿-468"));
        skillList.options.Add(new Dropdown.OptionData("忍刀隐身-535"));
        skillList.options.Add(new Dropdown.OptionData("锤大绝-325"));
        skillList.options.Add(new Dropdown.OptionData("忍刀大绝-474"));
        skillList.onValueChanged.AddListener((int idx) => {
        string s = skillList.options[skillList.value].text; var v = s.Split('-'); MeteorManager.Instance.LocalPlayer.posMng.ChangeAction(int.Parse(v[1])); });
        timeLabel = Global.ldaControlX("GameTime", WndObject).GetComponent<Text>();
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
            int SubType = MeteorManager.Instance.LocalPlayer.GetWeaponType();
            if (PoseStatus.ActionList.ContainsKey(MeteorManager.Instance.LocalPlayer.UnitId))
            {
                foreach (var each in PoseStatus.ActionList[MeteorManager.Instance.LocalPlayer.UnitId])
                    dr.options.Add(new Dropdown.OptionData("动作 " + each.Idx));
            }
            foreach (var each in SFXLoader.Instance.Effect)
            {
                effectDr.options.Add(new Dropdown.OptionData(each.Key));
            }
            dr.onValueChanged.AddListener((int idx)=> { MeteorManager.Instance.LocalPlayer.posMng.ChangeActionSingle(idx); });
            effectDr.onValueChanged.AddListener((int idx) => { SFXLoader.Instance.PlayEffect(effectDr.options[idx].text, MeteorManager.Instance.LocalPlayer.posMng.AnimalCtrlEx); });
            replay.onClick.AddListener(() =>{ if (dr.value != -1)MeteorManager.Instance.LocalPlayer.posMng.ChangeActionSingle(dr.value); });
            currentPos.onClick.AddListener(() => { if (currentPosIdx != -1) MeteorManager.Instance.LocalPlayer.posMng.ChangeActionSingle(currentPosIdx); });
            effectPlay.onClick.AddListener(() => { if (effectDr.value != -1) SFXLoader.Instance.PlayEffect(effectDr.options[effectDr.value].text, MeteorManager.Instance.LocalPlayer.posMng.AnimalCtrlEx); });
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

        debugPanel.SetActive(false);
        Global.ldaControlX("AddAI", WndObject).SetActive(GameData.gameStatus.EnableFunc);
        Global.ldaControlX("ChangeDebug", WndObject).SetActive(GameData.gameStatus.EnableDebug);
#if UNITY_Mobile
        if (NGUIJoystick.instance != null)
            NGUIJoystick.instance.SetAnchor(GameData.gameStatus.JoyAnchor);
#elif BUILD_PC
        Control("JoyArrow").SetActive(false);
        Control("Unlock").SetActive(false);
        Control("ClickPanel").SetActive(false);
        Control("FloatOpen").SetActive(false);
        Control("ActionFloat").SetActive(false);
#endif
    }

    int currentPosIdx;
    public void UpdatePoseStatus(int idx, int frame = 0)
    {
        if (currentPos != null)
        {
            currentPosIdx = idx;
            currentPos.GetComponentInChildren<Text>().text = "Pose " + idx + " Frames " + frame;
        }
    }
    IEnumerator ShowHPWarning()
    {
        while (true)
        {
            if (MeteorManager.Instance.LocalPlayer.Dead)
                break;
            if (MeteorManager.Instance.LocalPlayer.Attr.HpMax * 0.1f > MeteorManager.Instance.LocalPlayer.Attr.hpCur)
                hpWarning.enabled = !hpWarning.enabled;
            yield return new WaitForSeconds(0.5f);//半秒切换一次状态
        }
        hpWarning.enabled = false;
    }

    IEnumerator ShowAngryWarning()
    {
        while (true)
        {
            if (MeteorManager.Instance.LocalPlayer.Dead)
                break;
            if (MeteorManager.Instance.LocalPlayer.AngryValue == Global.ANGRYMAX)
                angryWarning.enabled = !angryWarning.enabled;
            yield return new WaitForSeconds(0.5f);//半秒切换一次状态
        }
        angryWarning.enabled = false;
    }

    bool actionBarStatus = false;
    void OnChangeActionBarStatus()
    {
        FloatOpen.GetComponent<Button>().interactable = false;
        GameBattleEx.Instance.StartCoroutine(actionStatusBarCtrl.PlayAnimation(actionBarStatus ? "HideActionBar" : "ShowActionBar", false, ()=> {
            FloatOpen.GetComponent<Button>().interactable = true;
            actionBarStatus = !actionBarStatus;
            FloatOpen.transform.rotation = Quaternion.Euler(0, 0, actionBarStatus ? 90 : 270);
        }));
        
    }

    public void UpdateUIButton()
    {
        Global.ldaControlX("AddAI", WndObject).SetActive(GameData.gameStatus.EnableFunc);
        Global.ldaControlX("ChangeDebug", WndObject).SetActive(GameData.gameStatus.EnableDebug);
    }

    void OnAttackPress()
    {
        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyDown(EKeyList.KL_Attack, false);//也可看作普攻
    }

    public void OnChangeLock(bool locked)
    {
        Global.ldaControlX("Unlock", WndObject).GetComponentInChildren<Image>().overrideSprite = locked ? Resources.Load<Sprite>("CameraU") : Resources.Load<Sprite>("CameraL");
    }

    void OnClickChangeLock()
    {
        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
        if (GameBattleEx.Instance.bLocked)
            GameBattleEx.Instance.Unlock();
        else
            GameBattleEx.Instance.Lock();
    }

    void OnChangeWeaponPress()
    {
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;

        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
            Global.GMeteorInput.OnKeyDown(EKeyList.KL_ChangeWeapon);
    }

    void OnChangeWeaponRelease()
    {
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;

        if (Global.GMeteorInput == null || Global.timeScale == 0 || Global.PauseAll) return;
            Global.GMeteorInput.OnKeyUp(EKeyList.KL_ChangeWeapon);
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
        if (MeteorManager.Instance.LocalPlayer.AngryValue >= 60 || Startup.ins.state.EnableInfiniteAngry)
        {
            Global.GMeteorInput.OnKeyDown(EKeyList.KL_BreakOut, false);
            //Debug.Log("OnKeyDown");
        }
    }

    int lastAngry = 0;
    public void UpdateAngryBar()
    {
        if (MeteorManager.Instance.LocalPlayer != null && !MeteorManager.Instance.LocalPlayer.Dead)
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

    public void UpdateTime(string label)
    {
        timeLabel.text = label;
    }

    public bool DefenceLongPress = false;

    public void PlayerMoveNotify(Transform target, EUnitCamp camp, bool isMainPlayer)
    {
        //在小地图这个地点绘制一个 圆点,如果是主角，绘制一个尖三角.
    }

    protected override bool OnClose()
    {
        buffList.Clear();
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

    //弹出剧情.
    public void InsertFightMessage(string text)
    {
        if (LevelTalkRoot != null && ctrl != null)
            ctrl.PushMessage(text);
    }

    public void OnBattleEnd()
    {
        targetInfo.SetActive(false);
        if (hideTargetInfo != null)
            GameBattleEx.Instance.StopCoroutine(hideTargetInfo);
        if (hpWarningE != null)
            GameBattleEx.Instance.StopCoroutine(hpWarningE);
        if (angryWarningE != null)
            GameBattleEx.Instance.StopCoroutine(angryWarningE);
        if (updateValue != null)
            GameBattleEx.Instance.StopCoroutine(updateValue);
        if (update != null)
            GameBattleEx.Instance.StopCoroutine(update);
        hpBar.fillAmount = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.HpMax;
    }

    //：生命值    112155/ 129373
    Coroutine hideTargetInfo;
    public void UpdateMonsterInfo(MeteorUnit mon)
    {
        if (!targetInfo.activeInHierarchy)
            targetInfo.SetActive(true);
        targetHp.fillAmount = (float)mon.Attr.hpCur / (float)mon.Attr.TotalHp;
        targetHpInfo.text = ":" + LangItem.GetLangString(StringIden.Life) + mon.Attr.hpCur + "/" + mon.Attr.TotalHp;
        //targetTitleInfo.text = mon.name;
        targetName.text = mon.name;
        //targetBuffInfo.text = "";
        //for (int i = 0; i < mon.allBuffs.Count; i++)
        //    targetBuffInfo.text += mon.allBuffs[i].Info.Name + "-";
        if (hideTargetInfo != null)
            GameBattleEx.Instance.StopCoroutine(hideTargetInfo);
        hideTargetInfo = GameBattleEx.Instance.StartCoroutine(HideTargetInfo());
    }

    IEnumerator HideTargetInfo()
    {
        yield return new WaitForSeconds(5.0f);
        targetInfo.SetActive(false);
        hideTargetInfo = null;
    }

    IEnumerator UpdateHPMP()
    {
        float targetValueHp = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.HpMax;
        //float targetValueMp = (float)PlayerEx.Instance.AngryValue / (float)100.0f;
        float tick = 0.0f;
        while (true)
        {
            if (!FightWnd.Exist)
                yield break;
            hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, targetValueHp, tick);
            //mpBar.fillAmount = Mathf.Lerp(mpBar.fillAmount, targetValueMp, tick);
            tick += Time.deltaTime;
            if (tick >= 1.0f)
                yield break;
            yield return 0;
        }
    }

    Coroutine updateValue;
    Coroutine hpWarningE;
    Coroutine angryWarningE;
    public void UpdatePlayerInfo()
    {
        if (MeteorManager.Instance.LocalPlayer != null && MeteorManager.Instance.LocalPlayer.Attr.hpCur >= 0)
        {
            if (updateValue != null)
                GameBattleEx.Instance.StopCoroutine(updateValue);
            updateValue = GameBattleEx.Instance.StartCoroutine(UpdateHPMP());
            hpLabel.text = MeteorManager.Instance.LocalPlayer.Attr.hpCur + "/" + MeteorManager.Instance.LocalPlayer.Attr.HpMax;
            //int level = MeteorManager.Instance.LocalPlayer.Attr.Level;
            //uint curlevelExp = (GameData.expMng.GetRowByIdx(level) as ExpBaseEx).Exp;

            //uint lastlevelExp = (GameData.expMng.GetRowByIdx(Mathf.Max(level - 1, 1)) as ExpBaseEx).Exp;
            //if (lastlevelExp == curlevelExp)
            //    exp.fillAmount = 0.0f;
            //else
            //    exp.fillAmount = (float)(GameData.MainRole.baseInfo.Exp - lastlevelExp) / (float)(curlevelExp - lastlevelExp);

            //hpBar.fillAmount = 
            float hpPercent = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.HpMax;
            if (hpPercent <= 0.1f)
            {
                if (hpWarningE != null)
                    GameBattleEx.Instance.StopCoroutine(hpWarningE);
                if (!MeteorManager.Instance.LocalPlayer.Dead)
                    hpWarningE = GameBattleEx.Instance.StartCoroutine(ShowHPWarning());
            }

            UpdateAngryBar();
            //mpLabel.text = PlayerEx.Instance.Heros[0].mpCur + "/" + PlayerEx.Instance.Heros[0].MpMax;
            //mpBar.fillAmount = (float)PlayerEx.Instance.Heros[0].mpCur / (float)PlayerEx.Instance.Heros[0].MpMax;

            //NameLabel.text = PlayerEx.Instance.Heros[0].Name;
            //LvLabel.text = LangItem.GetLangString(StringIden.Level) + ":" + level;
        }
    }

    //调试输入内容
    //Dictionary<EKeyList, string> keystring = new Dictionary<EKeyList, string>();
    //bool init = false;
    //Text fightText;
    //public void UpdateInputInfo()
    //{
    //    if (!init)
    //    {
    //        fightText = Global.ldaControlX("FightText", WndObject).GetComponent<Text>();
    //        keystring.Add(EKeyList.KL_Jump, "K");
    //        keystring.Add(EKeyList.KL_Attack, "J");
    //        keystring.Add(EKeyList.KL_KeyD, "→");
    //        keystring.Add(EKeyList.KL_KeyA, "←");
    //        keystring.Add(EKeyList.KL_KeyW, "↑");
    //        keystring.Add(EKeyList.KL_KeyS, "↓");
    //        init = true;
    //    }
    //    string s = "";
    //    for (int i = Global.GMeteorInput.Record.Count - 1; i >= 0; i--)
    //    {
    //        if (keystring.ContainsKey(Global.GMeteorInput.Record[i].key))
    //            s += keystring[Global.GMeteorInput.Record[i].key] + "\n";
    //    }
    //    if (fightText != null)
    //        fightText.text = s;
    //}
    Dictionary<Buff, GameObject> buffList = new Dictionary<Buff, GameObject>();
    Coroutine update;
    public void AddBuff(Buff buf)
    {
        if (update == null)
            update = GameBattleEx.Instance.StartCoroutine(UpdateBuff());
        if (buffList.ContainsKey(buf))
        {
            GameObject BuffName = Control("BuffName", buffList[buf]);
            BuffName.GetComponent<Text>().text = buf.Iden;
            GameObject BuffImg = Control("BuffImg", buffList[buf]);
            BuffImg.GetComponent<Image>().fillAmount = 1;
            GameObject BuffLength = Control("BuffLength", buffList[buf]);
            BuffLength.GetComponent<Text>().text = string.Format("{0:F2}", buf.last_time / 10);
        }
        else
        {
            GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("BuffItem"));// new GameObject(buf.Iden);
            obj.name = buf.Iden;
            obj.transform.SetParent(BuffRoot.transform);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.layer = BuffRoot.layer;
            buffList.Add(buf, obj);
            GameObject BuffName = Control("BuffName", buffList[buf]);
            BuffName.GetComponent<Text>().text = buf.Iden;
            GameObject BuffImg = Control("BuffImg", buffList[buf]);
            BuffImg.GetComponent<Image>().fillAmount = 1;
            GameObject BuffLength = Control("BuffLength", buffList[buf]);
            BuffLength.GetComponent<Text>().text = string.Format("{0:F1}", buf.last_time / 10);
        }
    }

    public IEnumerator UpdateBuff()
    {
        while (true)
        {
            try
            {
                foreach (var each in buffList)
                {
                    GameObject BuffImg = Control("BuffImg", each.Value);
                    BuffImg.GetComponent<Image>().fillAmount = each.Key.Units[MeteorManager.Instance.LocalPlayer].refresh_tick / (each.Key.last_time / 10);
                    GameObject BuffLength = Control("BuffLength", each.Value);
                    BuffLength.GetComponent<Text>().text = string.Format("{0:F1}", each.Key.Units[MeteorManager.Instance.LocalPlayer].refresh_tick);
                }
            }
            catch (Exception exp)
            {
                Debug.LogError(exp.Message + exp.StackTrace);
            }
            yield return 0;
        }
    }

    public void RemoveBuff(Buff buf)
    {
        if (buffList.ContainsKey(buf))
        {
            GameObject.Destroy(buffList[buf]);
            buffList.Remove(buf);
        }
    }
}