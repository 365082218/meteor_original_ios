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

	void ReLoadLanguage(){LanguagesManager.Instance.ReLoad();}
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

	override protected bool DrawProperties ()
	{
		mLabel = mWidget as UILabel;
		ComponentSelector.Draw<UIFont>(mLabel.font, OnSelectFont);




		if (mLabel.font != null)
		{
			GUI.skin.textArea.wordWrap = true;
			string text = string.IsNullOrEmpty(mLabel.text) ? "" : mLabel.text;
			text = EditorGUILayout.TextArea(mLabel.text, GUI.skin.textArea, GUILayout.Height(100f));
			if (!text.Equals(mLabel.text)) { RegisterUndo(); mLabel.text = text; }
			GUILayout.Label("Language Key");
			string language = string.IsNullOrEmpty(mLabel.language) ? "" : mLabel.language;
			language = EditorGUILayout.TextArea(mLabel.language, GUI.skin.textArea, GUILayout.Height(30f));
			if (!language.Equals(mLabel.language)) { RegisterUndo(); mLabel.language = language; }

			GUILayout.BeginHorizontal();
			int len = EditorGUILayout.IntField("Max Width", mLabel.lineWidth, GUILayout.Width(120f));
			GUILayout.Label("pixels");
			GUILayout.EndHorizontal();
			if (len != mLabel.lineWidth) { RegisterUndo(); mLabel.lineWidth = len; }

			int count = EditorGUILayout.IntField("Max Lines", mLabel.maxLineCount, GUILayout.Width(100f));
			if (count != mLabel.maxLineCount) { RegisterUndo(); mLabel.maxLineCount = count; }

			GUILayout.BeginHorizontal();
			bool shrinkToFit = EditorGUILayout.Toggle("Shrink to Fit", mLabel.shrinkToFit, GUILayout.Width(100f));
			GUILayout.Label("- adjust scale if doesn't fit");
			GUILayout.EndHorizontal();
			if (shrinkToFit != mLabel.shrinkToFit) { RegisterUndo(); mLabel.shrinkToFit = shrinkToFit; }

			GUILayout.BeginHorizontal();
			bool password = EditorGUILayout.Toggle("Password", mLabel.password, GUILayout.Width(100f));
			GUILayout.Label("- hide characters");
			GUILayout.EndHorizontal();
			if (password != mLabel.password) { RegisterUndo(); mLabel.password = password; }

			GUILayout.BeginHorizontal();
			bool encoding = EditorGUILayout.Toggle("Encoding", mLabel.supportEncoding, GUILayout.Width(100f));
			GUILayout.Label("- use emoticons and colors");
			GUILayout.EndHorizontal();
			if (encoding != mLabel.supportEncoding) { RegisterUndo(); mLabel.supportEncoding = encoding; }

			GUILayout.BeginHorizontal();
			bool isReload=EditorGUILayout.Toggle("Reload Language", false, GUILayout.Width(130f));
			if(isReload){ReLoadLanguage();if(mLabel.language!="")mLabel.language=mLabel.language;}
			GUILayout.EndHorizontal();


			//GUILayout.EndHorizontal();

			if (encoding && mLabel.font.hasSymbols)
			{
				UIFont.SymbolStyle sym = (UIFont.SymbolStyle)EditorGUILayout.EnumPopup("Symbols", mLabel.symbolStyle, GUILayout.Width(170f));
				if (sym != mLabel.symbolStyle) { RegisterUndo(); mLabel.symbolStyle = sym; }
			}

			GUILayout.BeginHorizontal();
			{
				UILabel.Effect effect = (UILabel.Effect)EditorGUILayout.EnumPopup("Effect", mLabel.effectStyle, GUILayout.Width(170f));
				if (effect != mLabel.effectStyle) { RegisterUndo(); mLabel.effectStyle = effect; }

				if (effect != UILabel.Effect.None)
				{
					Color c = EditorGUILayout.ColorField(mLabel.effectColor);
					if (mLabel.effectColor != c) { RegisterUndo(); mLabel.effectColor = c; }
				}
			}
			GUILayout.EndHorizontal();

			if (mLabel.effectStyle != UILabel.Effect.None)
			{
				GUILayout.Label("Distance", GUILayout.Width(70f));
				//GUILayout.Space(-34f);
				//GUILayout.BeginHorizontal();
				//GUILayout.Space(70f);
				Vector2 offset = EditorGUILayout.Vector2Field("", mLabel.effectDistance);
				//GUILayout.Space(20f);

				if (offset != mLabel.effectDistance)
				{
					RegisterUndo();
					mLabel.effectDistance = offset;
				}
				//GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			GameColor.GColor  mLabelColor = (GameColor.GColor)EditorGUILayout.EnumPopup("LabelColor", mLabel.labelColor, GUILayout.Width(170f));
			if (mLabelColor != mLabel.labelColor) { mLabel.color =GameColor.GetColor(mLabelColor) ; RegisterUndo(); mLabel.labelColor = mLabelColor; }
			GUILayout.EndHorizontal();
			return true;
		}
		EditorGUILayout.Space();
		return false;
	}
}
