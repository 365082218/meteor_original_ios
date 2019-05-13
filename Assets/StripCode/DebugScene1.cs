
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//该场景负责调试特效相关
public class DebugScene1 : MonoBehaviour {
    [SerializeField]
    private InfiniteScrollViewController ScrollView = null;
    [SerializeField]
    private MeteorUnitDebug Player = null;
    [SerializeField]
    private UnityEngine.UI.Text SfxName;
    [SerializeField]
    private UnityEngine.UI.Text SfxInfo;
    void Awake()
    {
        InfiniteScrollRect rect = ScrollView.GetScrollRect();

        rect.SetFullScrollView(false);
        rect.SetModifiedScale(true);
        for (int i = 0; i <= 1103 ; i++)
        {
            ScrollView.Add(new SfxCellData(i));
        }

        ScrollView.RefreshView();
    }
    // Use this for initialization
    SFXEffectPlay sfxDebugTarget = null;
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        if (sfxDebugTarget != null && sfxDebugTarget.OnSfxFrame == null)
            sfxDebugTarget.OnSfxFrame += this.OnSfxFrame;
	}

    public void PlayEffect()
    {
        int sfx = (ScrollView.CurrentData as SfxCellData).Sfx;
        SFXLoader.Instance.PlayEffect(sfx, Player.gameObject, false, false);
    }

    public void SpeedFast()
    {
        if (sfxDebugTarget != null)
            sfxDebugTarget.SpeedFast();
        Player.SpeedFast();
    }

    public void SpeedSlow()
    {
        if (sfxDebugTarget != null)
            sfxDebugTarget.SpeedSlow();
        Player.SpeedSlow();
    }


    public void ReturnToMain()
    {
        U3D.GoBack();
    }

    public void NextCharacter()
    {
        Player.Init(U3D.GetNextUnitId(Player.UnitId), LayerMask.NameToLayer("LocalPlayer"), true);
    }

    public void PrevCharacter()
    {
        Player.Init(U3D.GetPrevUnitId(Player.UnitId), LayerMask.NameToLayer("LocalPlayer"), true);
    }

    public void NextWeapon()
    {
        int nextWeapon = U3D.GetWeaponBySort(Player.weaponLoader.GetCurrentWeapon().Idx, 1);
        Player.weaponLoader.UnEquipWeapon();
        Player.weaponLoader.EquipWeapon(nextWeapon, false);
    }

    public void PrevWeapon()
    {
        int prevWeapon = U3D.GetWeaponBySort(Player.weaponLoader.GetCurrentWeapon().Idx, -1);
        Player.weaponLoader.UnEquipWeapon();
        Player.weaponLoader.EquipWeapon(prevWeapon, false);
    }

    public void OnSfxFrame(float time)
    {
        if (SfxName.text != sfxDebugTarget.name)
            SfxName.text = SfxName.name;
        SfxInfo.text = string.Format("时间:{3:f3}", time);
    }
}
