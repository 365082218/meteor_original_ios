using UnityEngine;
using System.Collections;

using System.Collections.Generic;
public class MeteorUnitDebug : MeteorUnit
{
    public Transform CameraTarget;
    new void Awake()
    {
        base.Awake();
    }

    public override bool IsDebugUnit() { return true; }

    void Start()
    {
        //for (int i = 0; i < 20; i++)
        //    AmbLoader.Ins.LoadCharacterAmb(i);
        //AmbLoader.Ins.LoadCharacterAmb();
        //ActionInterrupt.Instance.Init();
        //MenuResLoader.Instance.Init();
        Attr = new MonsterEx();
        Attr.InitPlayer(null);
        if (charLoader != null)
            return;
        UnitId %= 20;
        //Debug.LogError("start");
        Init(UnitId < 0 ? 0:UnitId, LayerMask.NameToLayer("LocalPlayer"));
    }

    public void GetOut()
    {
        U3D.GoBack();
    }

    public void Init(int modelIdx, int layer, bool updateModel = false)
    {
        tag = "meteorUnit";
        UnitId = modelIdx;
        IgnoreGravity = true;

        if (updateModel)
        {
            //把伤害盒子去掉，把受击盒子去掉
            hitList.Clear();

            if (charLoader != null)
            {
                GameObject.Destroy(charLoader.rootBone.parent.gameObject);
                GameObject.Destroy(charLoader.Skin.gameObject);
                charLoader = null;
            }
        }

        if (Attr == null && Main.Ins.MeteorManager != null && Main.Ins.LocalPlayer != null)
            Attr = Main.Ins.LocalPlayer.Attr;
        if (charLoader == null)
            charLoader = new CharacterLoader();
        if (posMng == null)
            posMng = new PoseStatus();
        if (updateModel)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
        charLoader.LoadCharactor(UnitId, transform);
        posMng.Init(this);
        WeaponR = NodeHelper.Find("d_wpnR", charLoader.rootBone.gameObject).transform;
        WeaponL = NodeHelper.Find("d_wpnL", charLoader.rootBone.gameObject).transform;
        ROOTNull = NodeHelper.Find("b", gameObject).transform;
        RootdBase = charLoader.rootBone;
        CameraTarget = RootdBase;
        weaponLoader = gameObject.GetComponent<WeaponLoader>();
        if (updateModel)
        {
            Destroy(weaponLoader);
            weaponLoader = null;
        }
        if (weaponLoader == null)
            weaponLoader = gameObject.AddComponent<WeaponLoader>();
        weaponLoader.Init(this);
        charController = gameObject.GetComponent<CharacterController>();
        if (charController == null)
            charController = gameObject.AddComponent<CharacterController>();
        charController.center = new Vector3(0, 16, 0);
        charController.height = 32;
        charController.radius = 9.0f;//不这么大碰不到寻路点.
        charController.stepOffset = 7.6f;
        posMng.ChangeAction();
        WsGlobal.SetObjectLayer(gameObject, layer);
        InventoryItem itWeapon = Main.Ins.GameStateMgr.MakeEquip(1);
        weaponLoader.EquipWeapon(itWeapon);
        this.name = Main.Ins.CombatData.GetCharacterName(UnitId);
    }

    public void Jump()
    {
        posMng.ChangeAction(CommonAction.Jump);
    }

    void EquipChanged(InventoryItem it)
    {
        weaponLoader.EquipWeapon(it);
        WsGlobal.SetObjectLayer(gameObject, gameObject.layer);
    }
}
