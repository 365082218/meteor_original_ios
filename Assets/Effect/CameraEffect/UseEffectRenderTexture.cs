using UnityEngine;
[AddComponentMenu("Image Effects/UseEffectRenderTexture")]
public class UseEffectRenderTexture : MonoBehaviour {
	private ParticleSystem mp;

	void Start(){
		ParticleSystem[] coms = GetComponentsInChildren<ParticleSystem>();
		foreach(ParticleSystem p in coms){
			if(p.GetComponent<Renderer>()!=null &&p.GetComponent<Renderer>().material != null && p.GetComponent<Renderer>().material.shader.name=="effect/DistortAndoridIos"){
				mp=p;
				OnEnable();
				break;
			}
		}
	}
	void OnEnable(){

			if(mp==null)return;

			#if UNITY_ANDROID
			mp.GetComponent<Renderer>().material.shader.maximumLOD=400;
		    //mp.renderer.enabled=false;
			GrabCameraTexture.StartHeatEffect(mp.GetComponent<Renderer>(),mp.loop?0:mp.duration);
			
			#else
			mp.GetComponent<Renderer>().material.shader.maximumLOD=100;
			#endif
	}
	#if UNITY_ANDROID
//	void	LateUpdate(){
//		OnEnable();
//	}
	void OnDestroy () {
		GrabCameraTexture.StartHeatEffect(null);
	}
	#endif
}