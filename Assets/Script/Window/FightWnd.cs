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
    Image hpBar;
    Image angryBar;
    Text hpLabel;
    //Text userNick;
    Image hpWarning;
    Image angryWarning;
    
    Image exp;
    Dropdown dr;
    Dropdown effectDr;
    Button currentPos;

    //角色面板，暂时取消
    void OnPlayerInfo()
    {
        if (PlayerWnd.Exist)
            PlayerWnd.Instance.Close();
        else
            PlayerWnd.Instance.Open();
    }

    public void HideCameraBtn()
    {
        if (Unlock != null)
            Unlock.SetActive(false);
    }

    public void ShowCameraBtn()
    {
        if (Unlock != null)
            Unlock.SetActive(true);
    }

    AutoMsgCtrl ctrl;
    Transform LevelTalkRoot;
    Animation actionStatusBarCtrl;
    GameObject FloatOpen;
    GameObject BuffRoot;
    GameObject TargetBuffPanel;
    GameObject Unlock;
    Image LockSprite;
    void Init()
    {
        LevelTalkRoot = Global.ldaControlX("LevelTalk", WndObject).transform;
        ctrl = LevelTalkRoot.GetComponent<AutoMsgCtrl>();
        ctrl.SetConfig(1.5f, 1f);
        FloatOpen = Control("FloatOpen");
        BuffRoot = Control("BuffPanel");
        TargetBuffPanel = Control("TargetBuffPanel");
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
        Global.ldaControlX("WeaponSelect", WndObject).GetComponentInChildren<Button>().onClick.AddListener(()=> { U3D.OpenWeaponWnd(); });
        Global.ldaControlX("SceneName", WndObject).GetComponent<Button>().onClick.AddListener(()=> { OpenMiniMap(); });
        Global.ldaControlX("SceneName", WndObject).GetComponentInChildren<Text>().text = Global.GLevelItem.Name;
        Global.ldaControlX("MXH", WndObject).GetComponent<GameButton>().OnPress.AddListener(()=> { U3D.OpenSystemWnd(); });
        Global.ldaControlX("Crouch", WndObject).GetComponent<GameButton>().OnPress.AddListener(OnCrouchPress);
        Global.ldaControlX("Crouch", WndObject).GetComponent<GameButton>().OnRelease.AddListener(OnCrouchRelease);
        Global.ldaControlX("Drop", WndObject).GetComponent<Button>().onClick.AddListener(OnClickDrop);
        Unlock = Global.ldaControlX("Unlock", WndObject);
        Unlock.GetComponentInChildren<Button>().onClick.AddListener(OnClickChangeLock);
        LockSprite = Global.ldaControlX("LockSprite", Unlock).GetComponent<Image>();
        Global.ldaControlX("SfxMenu", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenSfxWnd(); });
        Global.ldaControlX("Robot", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenRobotWnd(); });
        timeLabel = Global.ldaControlX("GameTime", WndObject).GetComponent<Text>();
        hpWarning = ldaControl("HPFlashWarning", WndObject).gameObject.GetComponent<Image>();
        angryWarning = ldaControl("AngryWarning", WndObject).gameObject.GetComponent<Image>();
        hpBar = ldaControl("HPBar", WndObject).gameObject.GetComponent<Image>();
        angryBar = ldaControl("AngryBar", WndObject).gameObject.GetComponent<Image>();
        hpLabel = ldaControl("HPLabel", WndObject).gameObject.GetComponent<Text>();
        //userNick = ldaControl("UserNick", WndObject).gameObject.GetComponent<Text>();
        //if (Global.GLevelMode == LevelMode.MultiplyPlayer)
        //{
        //    userNick.text = MeteorManager.Instance.LocalPlayer.Name;
        //    userNick.gameObject.SetActive(true);
        //}
        if (MeteorManager.Instance.LocalPlayer != null)
        {
            angryBar.fillAmount = 0.0f;
            angryWarning.enabled = false;
            hpWarning.enabled = false;
            UpdatePlayerInfo();
        }
        UpdateUIButton();
        if (NGUIJoystick.instance != null)
            NGUIJoystick.instance.SetAnchor(GameData.Instance.gameStatus.JoyAnchor);
    }

    //int currentPosIdx;
    //public void UpdatePoseStatus(int idx, int frame = 0)
    //{
    //    if (currentPos != null)
    //    {
    //        currentPosIdx = idx;
    //        currentPos.GetComponentInChildren<Text>().text = "Pose " + idx + " Frames " + frame;
    //    }

    //    MeteorManager.Instance.LocalPlayer.Action = idx;
    //    MeteorManager.Instance.LocalPlayer.Frame = frame;
    //}

    IEnumerator ShowHPWarning()
    {
        while (true)
        {
            if (MeteorManager.Instance.LocalPlayer.Dead)
                break;
            if ((float)MeteorManager.Instance.LocalPlayer.Attr.HpMax * 0.3f > (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur)
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

    bool openMiniMap = false;
    void OpenMiniMap()
    {
        openMiniMap = !openMiniMap;
        Global.ldaControlX("MiniMapFrame", WndObject).SetActive(openMiniMap);
    }

    public void UpdateUIButton()
    {
        Global.ldaControlX("WeaponSelect", WndObject).SetActive(GameData.Instance.gameStatus.EnableWeaponChoose);
        Global.ldaControlX("SfxMenu", WndObject).SetActive(GameData.Instance.gameStatus.EnableDebugSFX);
        Global.ldaControlX("Robot", WndObject).SetActive(GameData.Instance.gameStatus.EnableDebugRobot);
        Global.ldaControlX("MiniMap", WndObject).SetActive(true);

        if (GameData.Instance.gameStatus.LevelDebug)
            Game.Instance.ShowDbg();
        else
            Game.Instance.CloseDbg();
        if (GameData.Instance.gameStatus.EnableLog)
            WSDebug.Ins.OpenLogView();
        else
            WSDebug.Ins.CloseLogView();
    }

    void OnAttackPress()
    {
        if (Global.GMeteorInput == null || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyDown(EKeyList.KL_Attack, false);//也可看作普攻
    }

    public void OnChangeLock(bool locked)
    {
        LockSprite.enabled = locked;
    }

    void OnClickChangeLock()
    {
        if (Global.GMeteorInput == null || Global.PauseAll) return;
        //远程武器禁止切换锁定状态
        int weaponEquiped = MeteorManager.Instance.LocalPlayer.GetWeaponType();
        if (weaponEquiped == (int)EquipWeaponType.Gun || weaponEquiped == (int)EquipWeaponType.Dart || weaponEquiped == (int)EquipWeaponType.Guillotines)
            return;

        if (GameBattleEx.Instance.bLocked)
            GameBattleEx.Instance.Unlock();
        else
            GameBattleEx.Instance.Lock();
    }

    void OnClickDrop()
    {
        MeteorManager.Instance.LocalPlayer.DropWeapon();
    }

    void OnCrouchPress()
    {
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;

        if (Global.GMeteorInput == null || Global.PauseAll) return;
            Global.GMeteorInput.OnKeyDown(EKeyList.KL_Crouch);
    }

    void OnCrouchRelease()
    {
        if (Global.GMeteorInput == null || Global.PauseAll) return;
            Global.GMeteorInput.OnKeyUp(EKeyList.KL_Crouch);
    }

    void OnChangeWeaponPress()
    {
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;

        if (Global.GMeteorInput == null || Global.PauseAll) return;
            Global.GMeteorInput.OnKeyDown(EKeyList.KL_ChangeWeapon);
    }

    void OnChangeWeaponRelease()
    {
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;

        if (Global.GMeteorInput == null || Global.PauseAll) return;
            Global.GMeteorInput.OnKeyUp(EKeyList.KL_ChangeWeapon);
    }

    void OnAttackRelease()
    {
        if (Global.GMeteorInput == null || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyUp(EKeyList.KL_Attack);
    }

    void OnDefencePress()
    {
        if (!MeteorManager.Instance.LocalPlayer.posMng.CanDefence)
            return;

        if (Global.GMeteorInput == null || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyDown(EKeyList.KL_Defence, true);//不要被键盘状态同步，否则按下马上就抬起，那么防御姿势就消失了
        
    }

    void OnDefenceRelease()
    {
        if (Global.GMeteorInput == null || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyUp(EKeyList.KL_Defence);
    }

    void OnJumpPress()
    {
        //if (!MeteorManager.Instance.LocalPlayer.posMng.CanJump)
        //    return;

        if (Global.GMeteorInput == null || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyDown(EKeyList.KL_Jump, false);//
    }

    void OnJumpRelease()
    {
        if (Global.GMeteorInput == null || Global.PauseAll) return;
        Global.GMeteorInput.OnKeyUp(EKeyList.KL_Jump);
    }

    //按爆气.
    public void OnBreakOut()
    {
        //Debug.Log("OnBreakOut");
        if (Global.GMeteorInput == null || Global.PauseAll)
            return;
        if (MeteorManager.Instance.LocalPlayer.AngryValue >= 60 || GameData.Instance.gameStatus.EnableInfiniteAngry)
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
                    MeteorManager.Instance.LocalPlayer.StopCoroutine(angryWarningE);
                angryWarningE = MeteorManager.Instance.LocalPlayer.StartCoroutine(ShowAngryWarning());
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
            if ((float)MeteorManager.Instance.LocalPlayer.Attr.TotalHp * 0.3f >= (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur && (float)MeteorManager.Instance.LocalPlayer.Attr.TotalHp * 0.3f < lastHp)
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
        //buffList.Clear();
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
        //targetInfo.SetActive(false);
        //if (hideTargetInfo != null)
        //{
        //    GameBattleEx.Instance.StopCoroutine(hideTargetInfo);
        //    hideTargetInfo = null;
        //}
        if (hpWarningE != null)
        {
            MeteorManager.Instance.LocalPlayer.StopCoroutine(hpWarningE);
            hpWarningE = null;
        }
        if (angryWarningE != null)
        {
            MeteorManager.Instance.LocalPlayer.StopCoroutine(angryWarningE);
            angryWarningE = null;
        }
        if (updateValue != null)
        {
            MeteorManager.Instance.LocalPlayer.StopCoroutine(updateValue);
            updateValue = null;
        }
        //if (updateBuff != null)
        //{
        //    GameBattleEx.Instance.StopCoroutine(updateBuff);
        //    updateBuff = null;
        //}
        //if (updateEnemyBuff != null)
        //{
        //    GameBattleEx.Instance.StopCoroutine(updateEnemyBuff);
        //    updateEnemyBuff = null;
        //}
        hpBar.fillAmount = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.HpMax;
    }

    //：生命值    112155/ 129373
    //Coroutine hideTargetInfo;
    //Dictionary<Buff, GameObject> enemyBuffList = new Dictionary<Buff, GameObject>();
    //MeteorUnit currentMonster;
    //public void UpdateMonsterInfo(MeteorUnit mon)
    //{
    //    if (!targetInfo.activeInHierarchy)
    //        targetInfo.SetActive(true);

    //    targetHp.fillAmount = (float)mon.Attr.hpCur / (float)mon.Attr.TotalHp;
    //    targetHpInfo.text = ((int)(mon.Attr.hpCur / 10.0f)).ToString() + "/" + ((int)(mon.Attr.TotalHp / 10.0f)).ToString();
    //    //targetTitleInfo.text = mon.name;
    //    targetName.text = mon.name;
    //    if (updateEnemyBuff != null)
    //    {
    //        GameBattleEx.Instance.StopCoroutine(updateEnemyBuff);
    //        updateEnemyBuff = null;
    //    }
    //    foreach (var each in enemyBuffList)
    //        GameObject.Destroy(each.Value);
    //    enemyBuffList.Clear();

    //    if (!mon.Dead)
    //    {
    //        foreach (var each in BuffMng.Instance.BufDict)
    //        {
    //            if (!enemyBuffList.ContainsKey(each.Value) && each.Value.Units.ContainsKey(mon))
    //            {
    //                GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("BuffItem"));// new GameObject(buf.Iden);
    //                obj.name = each.Value.Iden;
    //                obj.transform.SetParent(TargetBuffPanel.transform);
    //                obj.transform.localScale = Vector3.one;
    //                obj.transform.localPosition = Vector3.zero;
    //                obj.transform.localRotation = Quaternion.identity;
    //                obj.layer = TargetBuffPanel.layer;
    //                enemyBuffList.Add(each.Value, obj);

    //                GameObject BuffImg = Control("BuffImg", enemyBuffList[each.Value]);
    //                BuffImg.GetComponent<Image>().fillAmount = each.Value.Units[mon].refresh_tick / (each.Value.last_time / 10);
    //                GameObject BuffLength = Control("BuffLength", enemyBuffList[each.Value]);
    //                BuffLength.GetComponent<Text>().text = string.Format("{0:F1}", each.Value.Units[mon].refresh_tick);

    //                GameObject BuffName = Control("BuffName", enemyBuffList[each.Value]);
    //                BuffName.GetComponent<Text>().text = each.Value.Iden;
    //            }
    //        }
    //        if (updateEnemyBuff == null)
    //            updateEnemyBuff = GameBattleEx.Instance.StartCoroutine(UpdateEnemyBuff());
    //    }
    //    currentMonster = mon;
    //    if (hideTargetInfo != null)
    //        GameBattleEx.Instance.StopCoroutine(hideTargetInfo);
    //    hideTargetInfo = GameBattleEx.Instance.StartCoroutine(HideTargetInfo());
    //}

    //Coroutine updateEnemyBuff;
    //IEnumerator UpdateEnemyBuff()
    //{
    //    while (true)
    //    {
    //        try
    //        {
    //            foreach (var each in enemyBuffList)
    //            {
    //                if (currentMonster != null && each.Key.Units.ContainsKey(currentMonster))
    //                {
    //                    GameObject BuffImg = Control("BuffImg", each.Value);
    //                    BuffImg.GetComponent<Image>().fillAmount = each.Key.Units[currentMonster].refresh_tick / (each.Key.last_time / 10);
    //                    GameObject BuffLength = Control("BuffLength", each.Value);
    //                    BuffLength.GetComponent<Text>().text = string.Format("{0:F1}", each.Key.Units[currentMonster].refresh_tick);
    //                }
    //            }
    //        }
    //        catch (Exception exp)
    //        {
    //            Debug.LogError(exp.Message + exp.StackTrace);
    //        }
    //        yield return 0;
    //    }
    //}

    //IEnumerator HideTargetInfo()
    //{
    //    yield return new WaitForSeconds(5.0f);
    //    if (updateEnemyBuff != null)
    //    {
    //        GameBattleEx.Instance.StopCoroutine(updateEnemyBuff);
    //        updateEnemyBuff = null;
    //    }
    //    foreach (var each in enemyBuffList)
    //        GameObject.Destroy(each.Value);
    //    enemyBuffList.Clear();
    //    if (targetInfo != null)
    //        targetInfo.SetActive(false);
    //    hideTargetInfo = null;
    //}

    IEnumerator UpdateHPMP()
    {
        float targetValueHp = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.HpMax;
        float tick = 0.0f;
        while (true)
        {
            if (!FightWnd.Exist)
                yield break;
            hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, targetValueHp, tick);
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
                MeteorManager.Instance.LocalPlayer.StopCoroutine(updateValue);
            updateValue = MeteorManager.Instance.LocalPlayer.StartCoroutine(UpdateHPMP());
            hpLabel.text = ((int)(MeteorManager.Instance.LocalPlayer.Attr.hpCur / 10.0f)).ToString() + "/" + ((int)(MeteorManager.Instance.LocalPlayer.Attr.HpMax / 10.0f)).ToString();
            float hpPercent = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.HpMax;
            if (hpPercent <= 0.1f)
            {
                if (hpWarningE != null)
                    MeteorManager.Instance.LocalPlayer.StopCoroutine(hpWarningE);
                if (!MeteorManager.Instance.LocalPlayer.Dead)
                    hpWarningE = MeteorManager.Instance.LocalPlayer.StartCoroutine(ShowHPWarning());
            }

            UpdateAngryBar();
        }
    }

    //Dictionary<Buff, GameObject> buffList = new Dictionary<Buff, GameObject>();
    //Coroutine updateBuff;
    //public void AddBuff(Buff buf)
    //{
    //    if (updateBuff == null)
    //        updateBuff = GameBattleEx.Instance.StartCoroutine(UpdateBuff());
    //    if (buffList.ContainsKey(buf))
    //    {
    //        GameObject BuffName = Control("BuffName", buffList[buf]);
    //        BuffName.GetComponent<Text>().text = buf.Iden;
    //        GameObject BuffImg = Control("BuffImg", buffList[buf]);
    //        BuffImg.GetComponent<Image>().fillAmount = 1;
    //        GameObject BuffLength = Control("BuffLength", buffList[buf]);
    //        BuffLength.GetComponent<Text>().text = string.Format("{0:F2}", buf.last_time / 10);
    //    }
    //    else
    //    {
    //        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("BuffItem"));// new GameObject(buf.Iden);
    //        obj.name = buf.Iden;
    //        obj.transform.SetParent(BuffRoot.transform);
    //        obj.transform.localScale = Vector3.one;
    //        obj.transform.localPosition = Vector3.zero;
    //        obj.transform.localRotation = Quaternion.identity;
    //        obj.layer = BuffRoot.layer;
    //        buffList.Add(buf, obj);
    //        GameObject BuffName = Control("BuffName", buffList[buf]);
    //        BuffName.GetComponent<Text>().text = buf.Iden;
    //        GameObject BuffImg = Control("BuffImg", buffList[buf]);
    //        BuffImg.GetComponent<Image>().fillAmount = 1;
    //        GameObject BuffLength = Control("BuffLength", buffList[buf]);
    //        BuffLength.GetComponent<Text>().text = string.Format("{0:F1}", buf.last_time / 10);
    //    }
    //}

    //public IEnumerator UpdateBuff()
    //{
    //    while (true)
    //    {
    //        try
    //        {
    //            foreach (var each in buffList)
    //            {
    //                if (each.Key.Units.ContainsKey(MeteorManager.Instance.LocalPlayer))
    //                {
    //                    GameObject BuffImg = Control("BuffImg", each.Value);
    //                    BuffImg.GetComponent<Image>().fillAmount = each.Key.Units[MeteorManager.Instance.LocalPlayer].refresh_tick / (each.Key.last_time / 10);
    //                    GameObject BuffLength = Control("BuffLength", each.Value);
    //                    BuffLength.GetComponent<Text>().text = string.Format("{0:F1}", each.Key.Units[MeteorManager.Instance.LocalPlayer].refresh_tick);
    //                }
    //            }
    //        }
    //        catch (Exception exp)
    //        {
    //            Debug.LogError(exp.Message + exp.StackTrace);
    //        }
    //        yield return 0;
    //    }
    //}

    //public void RemoveBuff(Buff buf, MeteorUnit unit = null)
    //{
    //    if (unit == null)
    //    {
    //        if (buffList.ContainsKey(buf))
    //        {
    //            GameObject.Destroy(buffList[buf]);
    //            buffList.Remove(buf);
    //        }
    //    }
    //    else
    //    {
    //        if (enemyBuffList.ContainsKey(buf))
    //        {
    //            GameObject.Destroy(enemyBuffList[buf]);
    //            enemyBuffList.Remove(buf);
    //        }
    //    }
    //}
}