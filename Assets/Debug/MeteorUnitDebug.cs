using UnityEngine;
using System.Collections;
using CoClass;
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
        for (int i = 0; i < 20; i++)
            AmbLoader.Ins.LoadCharacterAmb(i);
        AmbLoader.Ins.LoadCharacterAmb();
        ActionInterrupt.Instance.Init();
        MenuResLoader.Instance.Init();
        Attr = new MonsterEx();
        Attr.InitPlayer(null);
        IgnoreGravity = true;
        if (charLoader != null)
            return;
        UnitId %= 20;
        
        Init(UnitId < 0 ? 0:UnitId, LayerMask.NameToLayer("3DUIPlayer"));
    }

    public void GetOut()
    {
        U3D.GoBack();
    }

    // Update is called once per frame
    public void Update()
    {
        charLoader.LockUpdate();
        //ProcessGravity();
    }

    //public override void ProcessGravity()
    //{
    //    if (ImpluseVec.y > 0)
    //    {
    //        float s = ImpluseVec.y * Time.deltaTime - 0.5f * gScale * Time.deltaTime * Time.deltaTime;
    //        Move(new Vector3(ImpluseVec.x * Time.deltaTime, s, ImpluseVec.z * Time.deltaTime) + charLoader.moveDelta);
    //        ImpluseVec.y = ImpluseVec.y - gScale * Time.deltaTime;
    //        //???
    //        //Move(new Vector3(ImpluseVec.x * Time.deltaTime, 0, ImpluseVec.z * Time.deltaTime) + charLoader.moveDelta);
    //    }
    //    else
    //    {
    //        if (IgnoreGravity)
    //        {
    //            //浮空状态，某些大招会在空中停留.注意其他轴如果有速度，那么应该算
    //            Move(new Vector3(ImpluseVec.x * Time.deltaTime, 0, ImpluseVec.z * Time.deltaTime) + charLoader.moveDelta);
    //        }
    //        else
    //        {
    //            //处理跳跃的下半程
    //            float s = ImpluseVec.y * Time.deltaTime - 0.5f * gScale * Time.deltaTime * Time.deltaTime;
    //            Vector3 v;
    //            v.x = ImpluseVec.x * Time.deltaTime;
    //            v.y = IsOnGround() ? 0 : s;
    //            v.z = ImpluseVec.z * Time.deltaTime;
    //            v += charLoader.moveDelta;
    //            Move(v);
    //            float v2 = Vector3.Magnitude(v);
    //            if (Mathf.Abs(v2) >= 5.0f)
    //            {
    //                UnityEngine.Debug.DebugBreak();
    //            }
    //            //Move(new Vector3(0, s, 0) + transform.right * ImpluseVec.x * Time.deltaTime - transform.forward * ImpluseVec.z * Time.deltaTime);
    //            //Debug.Log("move s:" + v2);

    //            //只判断控制器，有时候在空中也会为真，但是还是要把速度与加速度计算
    //        }
    //    }
    //}
    //计算重力作用下的运动方向以及位移

    //public CharacterController charController;
    //Rigidbody rig;

    public void Init(int modelIdx, int layer)
    {
        tag = "meteorUnit";
        UnitId = modelIdx;
        IgnoreGravity = true;
        if (Attr == null && MeteorManager.Instance != null && MeteorManager.Instance.LocalPlayer != null)
            Attr = MeteorManager.Instance.LocalPlayer.Attr;
        charLoader = GetComponent<CharacterLoader>();
        if (charLoader == null)
            charLoader = new CharacterLoader();
        if (posMng == null)
            posMng = new PoseStatus();

        charLoader.LoadCharactor(UnitId, transform);
        posMng.Init(this);
        WeaponR = Global.ldaControlX("d_wpnR", charLoader.rootBone.gameObject).transform;
        WeaponL = Global.ldaControlX("d_wpnL", charLoader.rootBone.gameObject).transform;
        ROOTNull = Global.ldaControlX("b", gameObject).transform;
        RootdBase = charLoader.rootBone;
        CameraTarget = RootdBase;
        weaponLoader = gameObject.GetComponent<WeaponLoader>();
        if (weaponLoader == null)
            weaponLoader = gameObject.AddComponent<WeaponLoader>();
        weaponLoader.Init(this);
        posMng.ChangeAction();
        if (MeteorManager.Instance != null)
        {
            if (MeteorManager.Instance.LocalPlayer != null)
                MeteorManager.Instance.LocalPlayer.OnEquipChanged += EquipChanged;
            //weaponLoader.EquipWeapon(MeteorManager.Instance.LocalPlayer.weaponLoader.GetCurrentWeapon());
        }
        WsGlobal.SetObjectLayer(gameObject, layer);

        //charController = gameObject.AddComponent<CharacterController>();
        //charController.center = new Vector3(0, 17.8f, 0);
        //charController.height = 36.0f;
        //charController.radius = 8.0f;
        //charController.stepOffset = 7.8f;
        //mPos = transform.position;
        //for (int i = 0; i < 32; i++)
        //{
        //    if (LayerMask.LayerToName(i) == "LocalPlayer")
        //        continue;
        //    if (LayerMask.LayerToName(i) == "Monster")
        //        continue;
        //    if (LayerMask.LayerToName(i) == "Trigger")
        //        continue;
        //    if (!Physics.GetIgnoreLayerCollision(gameObject.layer, i))
        //        mCacheLayerMask |= (1 << i);
        //}
    }

    public void Jump()
    {
        posMng.ChangeAction(CommonAction.Jump);
    }

    void EquipChanged(InventoryItem it)
    {
        weaponLoader.EquipWeapon(it);
        WsGlobal.SetObjectLayer(gameObject, LayerMask.NameToLayer("3DUIPlayer"));
    }
}
