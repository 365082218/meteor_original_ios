using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(ParticleAndAnimation))]
public class ParticleAndAnimationInspector : Editor 
{
	ParticleAndAnimation pa;
	
	void OnEnable()
	{
		pa = target as ParticleAndAnimation;
	}
	
	public override void OnInspectorGUI ()
	{
//		base.OnInspectorGUI ();
		
		if(GUILayout.Button("PlayLoop"))
			pa.PlayLoop();
		if(GUILayout.Button("PlayOnce"))
			pa.PlayOnce();
	}
}
