//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UITextures.
/// </summary>

[CustomEditor(typeof(UITexture))]
public class UITextureInspector : UIWidgetInspector
{
	UITexture mTex;

	override protected bool DrawProperties ()
	{
		mTex = mWidget as UITexture;

		if (!mTex.hasDynamicMaterial && (mTex.material != null || mTex.mainTexture == null))
		{
			Material mat = EditorGUILayout.ObjectField("Material", mTex.material, typeof(Material), false) as Material;

			if (mTex.material != mat)
			{
				NGUIEditorTools.RegisterUndo("Material Selection", mTex);
				mTex.material = mat;
			}
		}

		if (mTex.material == null || mTex.hasDynamicMaterial)
		{
			Shader shader = EditorGUILayout.ObjectField("Shader", mTex.shader, typeof(Shader), false) as Shader;

			if (mTex.shader != shader)
			{
				NGUIEditorTools.RegisterUndo("Shader Selection", mTex);
				mTex.shader = shader;
			}

			Texture tex = EditorGUILayout.ObjectField("Texture", mTex.mainTexture, typeof(Texture), false) as Texture;

			if (mTex.mainTexture != tex)
			{
				NGUIEditorTools.RegisterUndo("Texture Selection", mTex);
				mTex.mainTexture = tex;
			}
		}

		if (mTex.mainTexture != null)
		{
			Rect rect = EditorGUILayout.RectField("UV Rectangle", mTex.uvRect);

			if (rect != mTex.uvRect)
			{
				NGUIEditorTools.RegisterUndo("UV Rectangle Change", mTex);
				mTex.uvRect = rect;
			}
		}
		return (mWidget.material != null);
	}
}