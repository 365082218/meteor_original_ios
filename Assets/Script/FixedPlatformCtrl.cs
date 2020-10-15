//using UnityEngine;
//using System.Collections;
//using System;
//using System.Collections.Generic;

//public class FixedPlatformCtrl :NetBehaviour {
//	public int Trigger = 0;
//    [SerializeField] bool AllowShake = true;
//    [HideInInspector] private AnimationCurve curve = null;
//    [SerializeField] private FMCPlayer fmcPlayer = null;
//    [SerializeField] private TextAsset model = null;
//    [SerializeField] private TextAsset ani = null;
//    [Header("Pose")]
//    [SerializeField] private bool InitializePose = false;
//    [SerializeField] private bool LoopPose = false;
//    [SerializeField] private int StartPose;
//    protected new void Awake()
//    {
//        base.Awake();
//        GMBLoader.Ins.Load(model);
//        int index = model.name.IndexOf(".");
//        Utility.ShowMeteorObject(model.name.Substring(0, index), transform);
//        if (fmcPlayer != null)
//        {
//            fmcPlayer.Init(ani);
//            //fmcPlayer.ChangePose(0, 0);
//            if (InitializePose)
//                fmcPlayer.ChangePose(StartPose, LoopPose ? 1 : 0);
//        }
//    }

//    public void Start()
//    {
//        hScale = 50;
//        if (AllowShake)
//        {
//            Keyframe[] ks = new Keyframe[2];
//            ks[0] = new Keyframe(0, 0);
//            ks[1] = new Keyframe(10, 2);
//            curve = new AnimationCurve(ks);
//            curve.postWrapMode = WrapMode.PingPong;
//            curve.preWrapMode = WrapMode.PingPong;
//        }
//        initializeY = transform.position.y;
//    }

//    float initializeY;
//    public float hScale;

//    // Update is called once per frame
//    void Update () {
	
//	}

//    public override void NetUpdate()
//    {
//        if (AllowShake)
//        {
//            float y = curve.Evaluate(FrameReplay.Instance.globalTime);
//            transform.position = new Vector3(transform.position.x, initializeY + hScale * y, transform.position.z);
//        }
//    }

//    public void OnTriggerEnter(Collider other)
//    {
//        MeteorUnit u = other.gameObject.GetComponent<MeteorUnit>();
//        if (u != null)
//        {
//            //Debug.LogError("OnTrigger:" + Trigger);
//            if (Main.Ins.GameBattleEx != null)
//                Main.Ins.GameBattleEx.OnSceneEvent(SceneEvent.EventEnter, u.InstanceId);
//            //任意角色进来，都会触发掉落动画，之后其他角色进来无法再触发.
//            if (fmcPlayer != null)
//                this.enabled = false;
//            //u.transform.SetParent(transform);
//            //u._ignoreGravityEx = true; 
//        }
//    }

//    public void OnTriggerExit(Collider other)
//    {
//        //MeteorUnit u = other.gameObject.GetComponent<MeteorUnit>();
//        //if (u != null)
//        //{
//        //    u._ignoreGravityEx = false;
//        //    u.transform.SetParent(null);
//        //}
//    }
//}
