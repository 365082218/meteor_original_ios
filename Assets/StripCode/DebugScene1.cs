
using DG.Tweening;
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
    [SerializeField]
    private Transform[] cameraPosition;
    void Awake()
    {
        AudioListener[] au = FindObjectsOfType<AudioListener>();
        if (au == null || au.Length == 0)
            gameObject.AddComponent<AudioListener>();
        Main.Ins.SFXLoader.InitSync();
        InfiniteScrollRect rect = ScrollView.GetScrollRect();
        Main.Ins.SoundManager.SetSoundVolume(100);
        rect.SetFullScrollView(false);
        rect.SetModifiedScale(true);
        for (int i = 0; i < Main.Ins.SFXLoader.Eff.Length; i++)
        {
            ScrollView.Add(new SfxCellData(i));
        }

        ScrollView.RefreshView();
    }
    // Use this for initialization
    SFXEffectPlay sfxDebugTarget = null;
    //bool inited = false;
    float t = 0;
	void Update () {
        t += Time.deltaTime * DebugUtil.speedScale;
        if (sfxDebugTarget != null && sfxDebugTarget.OnSfxFrame == null)
            sfxDebugTarget.OnSfxFrame += this.OnSfxFrame;
        while (t >= FrameReplay.deltaTime)
        {
            FrameReplay.Instance.NetUpdate();
            FrameReplay.Instance.NetLateUpdate();
            t -= FrameReplay.deltaTime;
        }
	}

    public void PlayEffect()
    {
        if (sfxDebugTarget != null)
        {
            sfxDebugTarget.OnPlayAbort();
            sfxDebugTarget = null;
        }
        
        int sfx = (ScrollView.CurrentData as SfxCellData).Sfx;
        sfxDebugTarget = Main.Ins.SFXLoader.PlayEffect(sfx, Player.charLoader);
        if (sfxDebugTarget != null)
            sfxDebugTarget.setAsDebug();
    }

    public void Far()
    {
        Tweener t = Camera.main.transform.DOMove(Camera.main.transform.position + Camera.main.transform.forward * -20, 0.5f);
        t.OnComplete(() => { Camera.main.transform.DOLookAt(Vector3.zero + new Vector3(0, 30, 0), 0.5f); });
    }

    public void Near()
    {
        Tweener t = Camera.main.transform.DOMove(Camera.main.transform.position + Camera.main.transform.forward * +20, 0.5f);
        t.OnComplete(() => { Camera.main.transform.DOLookAt(Vector3.zero + new Vector3(0, 30, 0), 0.5f); });
    }

    public void SpeedFast()
    {
        DebugUtil.SpeedFast();
    }

    public void SpeedSlow()
    {
        DebugUtil.SpeedSlow();
    }

    public void SpeedFast1()
    {
        DebugUtil.SpeedFast1();
    }

    public void SpeedSlow1()
    {
        DebugUtil.SpeedSlow1();
    }

    public void ChangeCamera(int i)
    {
        if (cameraPosition.Length > i && i >= 0)
        {
            Main.Ins.MainCamera.transform.position = cameraPosition[i].position;
            Main.Ins.MainCamera.transform.rotation = cameraPosition[i].rotation;
        }
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
            SfxName.text = sfxDebugTarget.file;
        SfxInfo.text = string.Format("时间:{0:f3}", time);
    }
}
