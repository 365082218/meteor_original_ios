using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum PlatformType
{

    SN25WayNode,
    SN25NpcPlatform,
}

public class FixedPlatformCtrl : MonoBehaviour {
	public int Trigger = 0;
    [SerializeField] PlatformType Type;
    [SerializeField] bool AllowShake = true;
    [HideInInspector] private AnimationCurve curve = null;
    public FMCPlayer fmcPlayer;
    private void Awake()
    {
        fmcPlayer = GetComponent<FMCPlayer>();
        if (fmcPlayer != null && Type == PlatformType.SN25WayNode)
        {
            fmcPlayer.Init("sn09f01");
            fmcPlayer.ChangePose(0, 0);
        }
    }

    public void Start()
    {
        hScale = 50;
        if (AllowShake)
        {
            Keyframe[] ks = new Keyframe[2];
            ks[0] = new Keyframe(0, 0);
            ks[1] = new Keyframe(4, 2);
            curve = new AnimationCurve(ks);
            curve.postWrapMode = WrapMode.PingPong;
            curve.preWrapMode = WrapMode.PingPong;
        }
        initializeY = transform.position.y;
    }

    float initializeY;
    public float hScale;

    // Update is called once per frame
    void Update () {
	
	}

    private void LateUpdate()
    {
        if (AllowShake)
        {
            float y = curve.Evaluate(Time.time);
            transform.position = new Vector3(transform.position.x, initializeY + hScale * y, transform.position.z);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        MeteorUnit u = other.gameObject.GetComponent<MeteorUnit>();
        if (u != null)
        {
            Debug.LogError("OnTrigger:" + Trigger);
            if (GameBattleEx.Instance != null)
                GameBattleEx.Instance.OnSceneEvent(SceneEvent.EventEnter, u.InstanceId, gameObject);
            this.enabled = false;
        }
    }
}
