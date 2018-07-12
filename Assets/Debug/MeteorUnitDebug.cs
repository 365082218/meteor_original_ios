using UnityEngine;
using System.Collections;
using CoClass;
using System.Collections.Generic;
public class MeteorUnitDebug : MonoBehaviour
{
    public int UnitId;
    public Transform WeaponL;//右手骨骼
    public Transform WeaponR;
    public Transform ROOTNull;
    public Transform RootdBase;
    public Transform CameraTarget;
    public EUnitCamp Camp = EUnitCamp.EUC_FRIEND;
    public PoseStatusDebug posMng;
    CharacterLoaderDebug charLoader;
    public WeaponLoaderEx weaponLoader;
    //Vector3 mPosition;
    //int mCacheLayerMask;

    void Awake()
    {
    }


    void Start()
    {
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
    void Update()
    {
    }

    //计算重力作用下的运动方向以及位移
    
    //public CharacterController charController;
    //Rigidbody rig;
    public void Init(int modelIdx)
    {
        tag = "meteorUnit";
        UnitId = modelIdx; 
        charLoader = GetComponent<CharacterLoaderDebug>();
        if (charLoader == null)
            charLoader = gameObject.AddComponent<CharacterLoaderDebug>();
        posMng = GetComponent<PoseStatusDebug>();
        if (posMng == null)
            posMng = gameObject.AddComponent<PoseStatusDebug>();

        charLoader.LoadCharactor(UnitId);
        posMng.Init();
        WeaponR = Global.ldaControlX("d_wpnR", charLoader.rootBone.gameObject).transform;
        WeaponL = Global.ldaControlX("d_wpnL", charLoader.rootBone.gameObject).transform;
        ROOTNull = Global.ldaControlX("b", gameObject).transform;
        RootdBase = charLoader.rootBone;
        CameraTarget = RootdBase;
        weaponLoader = gameObject.GetComponent<WeaponLoaderEx>();
        if (weaponLoader == null)
            weaponLoader = gameObject.AddComponent<WeaponLoaderEx>();
        weaponLoader.Init();
        posMng.ChangeAction();
        WsGlobal.SetObjectLayer(gameObject, gameObject.layer);
        //charController = gameObject.AddComponent<CharacterController>();
        //charController.center = new Vector3(0, 18.0f, 0);
        //charController.height = 36.0f;
        //charController.radius = 8.0f;
        //charController.stepOffset = 8.0f;
        //mPosition = transform.position;
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
}
