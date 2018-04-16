using UnityEngine;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/GrabCameraTexture")]
public class GrabCameraTexture : MonoBehaviour{
	private Renderer rend;
	private static GrabCameraTexture instance;
	private float destroyTime=0;

	private RenderTexture rt;

	private int t=0;
	void Start(){

	
	}
	#if UNITY_ANDROID

	void OnPreRender () {
		if(rend!=null){
			rend.material.SetTexture("_MainTex",rt);
		}
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination) {
        if (rend == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
			Graphics.Blit(source,rt);
			Graphics.SetRenderTarget(rt);
			Graphics.Blit(rt, destination);
			Graphics.SetRenderTarget(null);
		//rend.enabled=!	rend.enabled;
//			rend.enabled=((t % 3)!=0);
//		t=(t +1)% 3;
		if(rend !=null && rend.enabled==false)rend.enabled=true;
		//  RenderTexture.ReleaseTemporary(rt);
	}
	void Update(){
		if((destroyTime>0&&destroyTime<Time.time)||rend==null || rend.gameObject==null || rend.gameObject.activeSelf==false){
			rend=null;
            rt = null;
            this.enabled = false;
			return;
		}
	}
	#endif
	public static void StartHeatEffect(Renderer r,float t=0){
		if(r==null || r.material==null){
			r=null;
			instance.enabled=false;
			return;
		}
		if(instance==null){
			Camera cur=Camera.main;
			if(cur==null)return;
			GrabCameraTexture inst=cur.gameObject.GetComponent<GrabCameraTexture>();
			if(inst==null)inst=cur.gameObject.AddComponent<GrabCameraTexture>();
			instance=inst;
		}
		r.enabled=false;
#if UNITY_ANDROID
        instance.rt = RenderTexture.GetTemporary(Screen.width, Screen.height);

#endif
		instance.rend=r;
		instance.t=1;
	    instance.destroyTime=t>0?t+Time.time:0;
		instance.enabled=true;
	}
}