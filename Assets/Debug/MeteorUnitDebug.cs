using UnityEngine;
using System.Collections;
using CoClass;
using System.Collections.Generic;
public class MeteorUnitDebug : MeteorUnit
{
    public Transform CameraTarget;
    //Vector3 mPosition;
    //int mCacheLayerMask;

    void Awake()
    {
    }

    public override bool IsDebugUnit() { return true; }

    void Start()
    {
        Attr = new MonsterEx();
        Attr.InitPlayer(null);
        IgnoreGravity = true;
        if (charLoader != null)
            return;
        UnitId %= 20;
        
        Init(UnitId < 0 ? 0:UnitId);
    }

    public void GetOut()
    {
        U3D.GoBack();
    }

    // Update is called once per frame
    public void Update()
    {
        charLoader.CharacterUpdate();
        ProcessGravity();
    }

    //计算重力作用下的运动方向以及位移
    
    //public CharacterController charController;
    //Rigidbody rig;
    public void Init(int modelIdx)
    {
        tag = "meteorUnit";
        UnitId = modelIdx; 
        charLoader = GetComponent<CharacterLoader>();
        if (charLoader == null)
            charLoader = gameObject.AddComponent<CharacterLoader>();
        //posMng = GetComponent<PoseStatus>();
        if (posMng == null)
            posMng = new PoseStatus();

        charLoader.LoadCharactor(UnitId);
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
        WsGlobal.SetObjectLayer(gameObject, gameObject.layer);
        charController = gameObject.AddComponent<CharacterController>();
        charController.center = new Vector3(0, 17.8f, 0);
        charController.height = 36.0f;
        charController.radius = 8.0f;
        charController.stepOffset = 7.8f;
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
}
