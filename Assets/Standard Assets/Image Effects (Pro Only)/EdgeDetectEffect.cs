// Implements Edge Detection using a Roberts cross filter.

using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Edge Detection (Color)")]
public class EdgeDetectEffect : ImageEffectBase
{
	public float threshold = 0.2F;
	
	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		material.SetFloat ("_Treshold", threshold * threshold);
		Graphics.Blit (source, destination, material);
	}
}
