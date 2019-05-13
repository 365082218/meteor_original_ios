
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    void Awake()
    {
        InfiniteScrollRect rect = ScrollView.GetScrollRect();

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
	
	// Update is called once per frame
	void Update () {
        if (Player.charLoader != null && Player.charLoader.OnAnimationFrame == null)
            Player.charLoader.OnAnimationFrame += this.OnAnimation;
	}

    public void PlayAnimation()
    {
        int act = (ScrollView.CurrentData as AnimationCellData).animation;
        if (PoseStatus.ActionExist(Player.UnitId, act))
            Player.posMng.ChangeAction(act);
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
        Player.SpeedFast();
    }

    public void SpeedSlow()
    {
        Player.SpeedSlow();
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
}
