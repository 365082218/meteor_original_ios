
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

//该场景负责调试角色得动作，使用固定相机看向角色，验证角色得动作是否和3d max内一致
public class DebugScene0 : MonoBehaviour {
    [SerializeField]
    private InfiniteScrollViewController ScrollView = null;
    [SerializeField]
    private MeteorUnitDebug Player = null;
    [SerializeField]
    private UnityEngine.UI.Text PlayerName;
    [SerializeField]
    private UnityEngine.UI.Text PlayerInfo;
    [SerializeField]
    private Transform[] cameraPosition;
    [SerializeField]
    private Animator playerCtrl;
    void Awake()
    {
        AudioListener[] au = FindObjectsOfType<AudioListener>();
        if (au == null || au.Length == 0)
            gameObject.AddComponent<AudioListener>();
        InfiniteScrollRect rect = ScrollView.GetScrollRect();
        Main.Ins.SoundManager.SetSoundVolume(100);
        rect.SetFullScrollView(false);
        rect.SetModifiedScale(true);
        for (int i = 0; i <= 600 ; i++)
        {
            ScrollView.Add(new AnimationCellData(i));
        }

        ScrollView.RefreshView();
    }
    // Use this for initialization
    void Start () {
        
    }
	
    float t = 0;
    void Update()
    {
        FrameReplay.deltaTime = Time.deltaTime * DebugUtil.speedScale;
        if (Player.charLoader != null && Player.charLoader.OnAnimationFrame == null)
        {
            Player.charLoader.OnAnimationFrame += this.OnAnimation;
            Player.posMng.OnDebugActionFinished += this.OnAnimationFinished;
        }
        FrameReplay.Instance.NetUpdate();//锁定帧
        FrameReplay.Instance.NetLateUpdate();
    }

    public void PlayAnimation()
    {
        int act = (ScrollView.CurrentData as AnimationCellData).animation;
        if (PoseStatus.ActionExist(Player.UnitId, act))
        {
            if (act == CommonAction.Jump)
                Player.Jump(false, 1);
            else
                Player.posMng.ChangeAction(act, 0);
        }
        else
        {
            Debug.LogError("角色不存在动作:" + act);
        }
    }

    public void ReturnToMain()
    {
        U3D.GoBack();
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
            Camera.main.transform.position = cameraPosition[i].position;
            Camera.main.transform.rotation = cameraPosition[i].rotation;
        }
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

    public void OnAnimation(int source, int action, int frame, float lerp)
    {
        if (PlayerName.text != Player.name)
            PlayerName.text = Player.name;
        PlayerInfo.text = string.Format("动作源:{0} 动作:{1:D3} 帧:{2:D5} 插值:{3:f3}", source, action, frame, lerp);
    }

    void OnAnimationFinished()
    {
        //Player.transform.position = Vector3.zero;
    }
}
