using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(UITabBar))]
public class UITabBarInspector : Editor {

	UITabBar mTabBar;


	override public void OnInspectorGUI ()
	{

//		if(mWidget==null) return false;

		mTabBar = target as UITabBar;
		ComponentSelector.Draw<UIAtlas>(mTabBar.atlas, OnSelectAtlas);
		ComponentSelector.Draw<UIFont>(mTabBar.font, OnSelectFont);


//		EditorGUILayout.Space();

		if(mTabBar.atlas!=null&&mTabBar.font!=null)
		{
			NGUIEditorTools.AdvancedSpriteField(mTabBar.atlas, mTabBar.spriteName, SelectSprite, false);

			GUILayout.Label("选中的样式");
			NGUIEditorTools.AdvancedSpriteField(mTabBar.atlas, mTabBar.selectSpriteName, SelectedSprite, false);
			GUI.skin.textArea.wordWrap = true;
			string texts = string.IsNullOrEmpty(mTabBar.texts) ? "" : mTabBar.texts;
			texts = EditorGUILayout.TextArea(mTabBar.texts, GUI.skin.textArea, GUILayout.Height(50.0f));
			if(texts!=mTabBar.texts)
			{
				mTabBar.texts =texts;
				OnSet();
			}
			GUILayout.Label("Language Keys");
			string languages = string.IsNullOrEmpty(mTabBar.languages) ? "" : mTabBar.languages;
			languages = EditorGUILayout.TextArea(mTabBar.languages, GUI.skin.textArea, GUILayout.Height(50.0f));
			if(languages!=mTabBar.languages)
			{
				mTabBar.languages=languages;
				OnSet();
			}

			GUILayout.BeginHorizontal();
			int w = EditorGUILayout.IntField("ItemWidth", mTabBar.w, GUILayout.Width(200f));
			if(w!=mTabBar.w)
			{
				mTabBar.w=w;
				OnSet();
			}

			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			int h = EditorGUILayout.IntField("ItemHeight", mTabBar.h, GUILayout.Width(200f));
			if(h!=mTabBar.h)
			{
				mTabBar.h=h;
				OnSet();
			}
			GUILayout.EndHorizontal();






			UITabBar.Direction direction = (UITabBar.Direction)EditorGUILayout.EnumPopup("Direction", mTabBar.direction ==null? UITabBar.Direction.Horizontal:mTabBar.direction);
			if (mTabBar.direction != direction)
			{
				mTabBar.direction = direction;
				OnSet();
//				EditorUtility.SetDirty(mTabBar);
			}


			GUILayout.BeginHorizontal();
			int fontSize = EditorGUILayout.IntField("SelectedFontSize", mTabBar.fontSize, GUILayout.Width(200f));
			if(fontSize!=mTabBar.fontSize)
			{
				mTabBar.fontSize=fontSize;
				OnSet();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			int normalSize = EditorGUILayout.IntField("NormalFontSize", mTabBar.normalSize, GUILayout.Width(200f));
			if(normalSize!=mTabBar.normalSize)
			{
				mTabBar.normalSize=normalSize;
				OnSet();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			float gap = EditorGUILayout.FloatField("Gap", mTabBar.gap, GUILayout.Width(200f));
			if(gap!=mTabBar.gap)
			{
				mTabBar.gap=gap;
				OnSet();
			}
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			GUILayout.Label("SelectedColor");
			Color c = EditorGUILayout.ColorField(mTabBar.SelectedColor);
			if(c!=mTabBar.SelectedColor)
			{
				mTabBar.SelectedColor =c;
				OnSet();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GameColor.GColor  SelectedGameColor = (GameColor.GColor)EditorGUILayout.EnumPopup("SelectedColor", mTabBar.SelectedGameColor, GUILayout.Width(200f));
			if (SelectedGameColor != mTabBar.SelectedGameColor) { mTabBar.SelectedColor =GameColor.GetColor(SelectedGameColor) ; OnSet(); mTabBar.SelectedGameColor = SelectedGameColor; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("NormalColor");
			c = EditorGUILayout.ColorField(mTabBar.NormalColor);
			if(c!=mTabBar.NormalColor)
			{
				mTabBar.NormalColor =c;
				OnSet();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GameColor.GColor  NormalGameColor = (GameColor.GColor)EditorGUILayout.EnumPopup("NormalColor", mTabBar.NormalGameColor, GUILayout.Width(200f));
			if (NormalGameColor != mTabBar.NormalGameColor) { mTabBar.NormalColor =GameColor.GetColor(NormalGameColor) ; OnSet(); mTabBar.NormalGameColor = NormalGameColor; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			Vector2 SelectedTextPos = EditorGUILayout.Vector2Field("SelectedTextPos",mTabBar.SelectedTextPos);
			if(SelectedTextPos!=mTabBar.SelectedTextPos)
			{
				mTabBar.SelectedTextPos = SelectedTextPos;
				OnSet();
			}
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			Vector2 NormalTextPos = EditorGUILayout.Vector2Field("NormalTextPos",mTabBar.NormalTextPos);
			if(NormalTextPos!=mTabBar.NormalTextPos)
			{
				mTabBar.NormalTextPos = NormalTextPos;
				OnSet();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			bool IsOutLine=EditorGUILayout.Toggle("IsOutline", mTabBar.IsOutline);
			if(IsOutLine!=mTabBar.IsOutline)
			{
				mTabBar.IsOutline = IsOutLine;
				OnSet();
			}
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			bool isOnSet=EditorGUILayout.Toggle("OnSet", false);
//			Debug.Log("isOnSet:"+isOnSet);
			if(isOnSet){OnSet();}
			GUILayout.EndHorizontal();
		}

	}

//	public override void OnInspectorGUI ()
//	{
////		UIPanel panel = target as UIPanel;
//	}


	void SelectSprite (string spriteName)
	{
		if (mTabBar != null && mTabBar.spriteName != spriteName)
		{
			NGUIEditorTools.RegisterUndo("Sprite Change", mTabBar);
			mTabBar.spriteName = spriteName;
//			mTabBar.MakePixelPerfect();
			EditorUtility.SetDirty(mTabBar.gameObject);
			OnSet();
		}
	}


	void SelectedSprite (string spriteName)
	{
		if (mTabBar != null && mTabBar.selectSpriteName != spriteName)
		{
			NGUIEditorTools.RegisterUndo("Sprite Change", mTabBar);
			mTabBar.selectSpriteName = spriteName;
			//			mTabBar.MakePixelPerfect();
			EditorUtility.SetDirty(mTabBar.gameObject);
			OnSet();
		}

	}
	void OnSelectFont (MonoBehaviour obj)
	{
		if (mTabBar != null)
		{
			NGUIEditorTools.RegisterUndo("Font Selection", mTabBar);
			bool resize = (mTabBar.font == null);
			mTabBar.font = obj as UIFont;
			OnSet();
//			if (resize) mTabBar.MakePixelPerfect();
		}
	}

	void OnSelectAtlas (MonoBehaviour obj)
	{
		if (mTabBar != null)
		{
			NGUIEditorTools.RegisterUndo("Atlas Selection", mTabBar);
			bool resize = (mTabBar.atlas == null);
			mTabBar.atlas = obj as UIAtlas;
			OnSet();
//			if (resize) mTabBar.MakePixelPerfect();
			EditorUtility.SetDirty(mTabBar.gameObject);
		}
	}

	void OnSet()
	{
		if(mTabBar!=null)
		{
			mTabBar.OnSet();
		}
	}

}
