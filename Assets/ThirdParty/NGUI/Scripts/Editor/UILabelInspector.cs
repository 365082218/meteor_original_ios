//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CustomEditor(typeof(UILabel))]
public class UILabelInspector : UIWidgetInspector
{
	UILabel mLabel;
	UILabel mLanguage;
	/// <summary>
	/// Register an Undo command with the Unity editor.
	/// </summary>

	void RegisterUndo () { NGUIEditorTools.RegisterUndo("Label Change", mLabel); }

	void ReLoadLanguage(){
        //LanguagesManager.Instance.ReLoad();
    }
	/// <summary>
	/// Font selection callback.
	/// </summary>


	void OnSelectFont (MonoBehaviour obj)
	{
		if (mLabel != null)
		{
			NGUIEditorTools.RegisterUndo("Font Selection", mLabel);
			bool resize = (mLabel.font == null);
			mLabel.font = obj as UIFont;
			if (resize) mLabel.MakePixelPerfect();
		}
	}
}
