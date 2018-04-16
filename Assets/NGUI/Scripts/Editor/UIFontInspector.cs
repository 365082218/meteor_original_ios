//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

// Dynamic font support contributed by the NGUI community members:
// Unisip, zh4ox, Mudwiz, Nicki, DarkMagicCK.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to view and edit UIFonts.
/// </summary>

[CustomEditor(typeof(UIFont))]
public class UIFontInspector : Editor
{
	enum View
	{
		Nothing,
		Atlas,
		Font,
	}

	enum FontType
	{
		Normal,
		Reference,
        Dynamic   //Lindean
	}

	static View mView = View.Font;
	static bool mUseShader = false;

	UIFont mFont;
	FontType mType = FontType.Normal;
	UIFont mReplacement = null;
	string mSymbolSequence = "";
	string mSymbolSprite = "";
	BMSymbol mSelectedSymbol = null;

	public override bool HasPreviewGUI () { return mView != View.Nothing; }

	void OnSelectFont (MonoBehaviour obj)
	{
		// Undo doesn't work correctly in this case... so I won't bother.
		//NGUIEditorTools.RegisterUndo("Font Change");
		//NGUIEditorTools.RegisterUndo("Font Change", mFont);

		mFont.replacement = obj as UIFont;
		mReplacement = mFont.replacement;
		UnityEditor.EditorUtility.SetDirty(mFont);
		if (mReplacement == null) mType = FontType.Normal;
	}

	void OnSelectAtlas (MonoBehaviour obj)
	{
		if (mFont != null)
		{
			NGUIEditorTools.RegisterUndo("Font Atlas", mFont);
			mFont.atlas = obj as UIAtlas;
			MarkAsChanged();
		}
	}

	void MarkAsChanged ()
	{
		List<UILabel> labels = NGUIEditorTools.FindInScene<UILabel>();

		foreach (UILabel lbl in labels)
		{
			if (UIFont.CheckIfRelated(lbl.font, mFont))
			{
				lbl.font = null;
				lbl.font = mFont;
			}
		}
	}

	override public void OnInspectorGUI ()
	{
		mFont = target as UIFont;
		EditorGUIUtility.LookLikeControls(80f);

		NGUIEditorTools.DrawSeparator();

        //Lindean
        if (mFont.dynamicFont != null)
        {
            mType = FontType.Dynamic;
        }

		if (mFont.replacement != null)
		{
			mType = FontType.Reference;
			mReplacement = mFont.replacement;
		}
		else if (mFont.dynamicFont_1 != null)
		{
			mType = FontType.Dynamic;
		}

		GUILayout.BeginHorizontal();
		FontType fontType = (FontType)EditorGUILayout.EnumPopup("Font Type", mType);
		GUILayout.Space(18f);
		GUILayout.EndHorizontal();

		if (mType != fontType)
		{
			if (fontType == FontType.Normal)
			{
				OnSelectFont(null);
			}
			else
			{
				mType = fontType;
			}

            //Lindean
			if (mType != FontType.Dynamic && mFont.dynamicFont_1 != null)
				mFont.dynamicFont_1 = null;
		}

        //Lindean
        if (mType == FontType.Dynamic)
        {
            //Lindean - Draw settings for dynamic font
            bool changed = false;
            Font f = EditorGUILayout.ObjectField("Font", mFont.dynamicFont, typeof(Font), false) as Font;
            if (f != mFont.dynamicFont)
            {
                mFont.dynamicFont = f;
                changed = true;
            }

            Material mat = EditorGUILayout.ObjectField("Material", mFont.dynamicFontMaterial_1, typeof(Material), false) as Material;
            if (mat != mFont.dynamicFontMaterial_1)
            {
                mFont.dynamicFontMaterial_1 = mat;
                changed = true;
            }
            if (mFont.dynamicFontMaterial_1 == null)
                GUILayout.Label("Warning: no coloring or clipping when using default font material");

            int i = EditorGUILayout.IntField("Size", mFont.dynamicFontSize);
            if (i != mFont.dynamicFontSize)
            {
                mFont.dynamicFontSize = i;
                changed = true;
            }

            FontStyle style = (FontStyle)EditorGUILayout.EnumPopup("Style", mFont.dynamicFontStyle);
            if (style != mFont.dynamicFontStyle)
            {
                mFont.dynamicFontStyle = style;
                changed = true;
            }

            if (changed)
            {
                //force access to material property as it refreshes the texture assignment
                Debug.Log("font changed...");
                Material fontMat = mFont.material;
                if (fontMat.mainTexture == null)
                    Debug.Log("font material texture issue...");
                UIFont.OnFontRebuilt(mFont);
            }

            NGUIEditorTools.DrawSeparator();

            // Font spacing
            GUILayout.BeginHorizontal();
            {
                EditorGUIUtility.LookLikeControls(0f);
                GUILayout.Label("Spacing", GUILayout.Width(60f));
                GUILayout.Label("X", GUILayout.Width(12f));
                int x = EditorGUILayout.IntField(mFont.horizontalSpacing);
                GUILayout.Label("Y", GUILayout.Width(12f));
                int y = EditorGUILayout.IntField(mFont.verticalSpacing);
                EditorGUIUtility.LookLikeControls(80f);

                if (mFont.horizontalSpacing != x || mFont.verticalSpacing != y)
                {
                    NGUIEditorTools.RegisterUndo("Font Spacing", mFont);
                    mFont.horizontalSpacing = x;
                    mFont.verticalSpacing = y;
                }
            }
            GUILayout.EndHorizontal();
        }
        //Lindean

		if (mType == FontType.Reference)
		{
			ComponentSelector.Draw<UIFont>(mFont.replacement, OnSelectFont);

			NGUIEditorTools.DrawSeparator();
			GUILayout.Label("You can have one font simply point to\n" +
				"another one. This is useful if you want to be\n" +
				"able to quickly replace the contents of one\n" +
				"font with another one, for example for\n" +
				"swapping an SD font with an HD one, or\n" +
				"replacing an English font with a Chinese\n" +
				"one. All the labels referencing this font\n" +
				"will update their references to the new one.");

			if (mReplacement != mFont && mFont.replacement != mReplacement)
			{
				NGUIEditorTools.RegisterUndo("Font Change", mFont);
				mFont.replacement = mReplacement;
				UnityEditor.EditorUtility.SetDirty(mFont);
			}
			return;
		}
		else if (mType == FontType.Dynamic)
		{
#if UNITY_3_5
			EditorGUILayout.HelpBox("Dynamic fonts require Unity 4.0 or higher.", MessageType.Error);
#else
			NGUIEditorTools.DrawSeparator();
			Font fnt = EditorGUILayout.ObjectField("TTF Font", mFont.dynamicFont_1, typeof(Font), false) as Font;
			
			if (fnt != mFont.dynamicFont_1)
			{
				NGUIEditorTools.RegisterUndo("Font change", mFont);
				mFont.dynamicFont_1 = fnt;
			}

			GUILayout.BeginHorizontal();
			int size = EditorGUILayout.IntField("Size", mFont.dynamicFontSize_1, GUILayout.Width(120f));
			FontStyle style = (FontStyle)EditorGUILayout.EnumPopup(mFont.dynamicFontStyle_1);
			GUILayout.Space(18f);
			GUILayout.EndHorizontal();

			if (size != mFont.dynamicFontSize_1)
			{
				NGUIEditorTools.RegisterUndo("Font change", mFont);
				mFont.dynamicFontSize_1 = size;
			}

			if (style != mFont.dynamicFontStyle_1)
			{
				NGUIEditorTools.RegisterUndo("Font change", mFont);
				mFont.dynamicFontStyle_1 = style;
			}

			Material mat = EditorGUILayout.ObjectField("Material", mFont.material, typeof(Material), false) as Material;

			if (mFont.material != mat)
			{
				NGUIEditorTools.RegisterUndo("Font Material", mFont);
				mFont.material = mat;
			}
#endif
		}
		else
		{
			NGUIEditorTools.DrawSeparator();

			ComponentSelector.Draw<UIAtlas>(mFont.atlas, OnSelectAtlas);

			if (mFont.atlas != null)
			{
				if (mFont.bmFont.isValid)
				{
					NGUIEditorTools.AdvancedSpriteField(mFont.atlas, mFont.spriteName, SelectSprite, false);
				}
				EditorGUILayout.Space();
			}
			else
			{
				// No atlas specified -- set the material and texture rectangle directly
				Material mat = EditorGUILayout.ObjectField("Material", mFont.material, typeof(Material), false) as Material;

				if (mFont.material != mat)
				{
					NGUIEditorTools.RegisterUndo("Font Material", mFont);
					mFont.material = mat;
				}
			}

			// For updating the font's data when importing from an external source, such as the texture packer
			bool resetWidthHeight = false;

			if (mFont.atlas != null || mFont.material != null)
			{
				TextAsset data = EditorGUILayout.ObjectField("Import Data", null, typeof(TextAsset), false) as TextAsset;

				if (data != null)
				{
					NGUIEditorTools.RegisterUndo("Import Font Data", mFont);
					BMFontReader.Load(mFont.bmFont, NGUITools.GetHierarchy(mFont.gameObject), data.bytes);
					mFont.MarkAsDirty();
					resetWidthHeight = true;
					Debug.Log("Imported " + mFont.bmFont.glyphCount + " characters");
				}
			}

			if (mFont.bmFont.isValid)
			{
				Color green = new Color(0.4f, 1f, 0f, 1f);
				Texture2D tex = mFont.texture;

				if (tex != null)
				{
					if (mFont.atlas == null)
					{
						// Pixels are easier to work with than UVs
						Rect pixels = NGUIMath.ConvertToPixels(mFont.uvRect, tex.width, tex.height, false);

						// Automatically set the width and height of the rectangle to be the original font texture's dimensions
						if (resetWidthHeight)
						{
							pixels.width = mFont.texWidth;
							pixels.height = mFont.texHeight;
						}

						// Font sprite rectangle
						GUI.backgroundColor = green;
						pixels = EditorGUILayout.RectField("Pixel Rect", pixels);
						GUI.backgroundColor = Color.white;

						// Create a button that can make the coordinates pixel-perfect on click
						GUILayout.BeginHorizontal();
						{
							Rect corrected = NGUIMath.MakePixelPerfect(pixels);

							if (corrected == pixels)
							{
								GUI.color = Color.grey;
								GUILayout.Button("Make Pixel-Perfect");
								GUI.color = Color.white;
							}
							else if (GUILayout.Button("Make Pixel-Perfect"))
							{
								pixels = corrected;
								GUI.changed = true;
							}
						}
						GUILayout.EndHorizontal();

						// Convert the pixel coordinates back to UV coordinates
						Rect uvRect = NGUIMath.ConvertToTexCoords(pixels, tex.width, tex.height);

						if (mFont.uvRect != uvRect)
						{
							NGUIEditorTools.RegisterUndo("Font Pixel Rect", mFont);
							mFont.uvRect = uvRect;
						}
						//NGUIEditorTools.DrawSeparator();
						EditorGUILayout.Space();
					}
				}
			}
		}

		// The font must be valid at this point for the rest of the options to show up
		if (mFont.isDynamic || mFont.bmFont.isValid)
		{
			// Font spacing
			GUILayout.BeginHorizontal();
			{
				EditorGUIUtility.LookLikeControls(0f);
				GUILayout.Label("Spacing", GUILayout.Width(60f));
				GUILayout.Label("X", GUILayout.Width(12f));
				int x = EditorGUILayout.IntField(mFont.horizontalSpacing);
				GUILayout.Label("Y", GUILayout.Width(12f));
				int y = EditorGUILayout.IntField(mFont.verticalSpacing);
				GUILayout.Space(18f);
				EditorGUIUtility.LookLikeControls(80f);

				if (mFont.horizontalSpacing != x || mFont.verticalSpacing != y)
				{
					NGUIEditorTools.RegisterUndo("Font Spacing", mFont);
					mFont.horizontalSpacing = x;
					mFont.verticalSpacing = y;
				}
			}
			GUILayout.EndHorizontal();

			if (mFont.atlas == null)
			{
				mView = View.Font;
				mUseShader = false;

				float pixelSize = EditorGUILayout.FloatField("Pixel Size", mFont.pixelSize, GUILayout.Width(120f));

				if (pixelSize != mFont.pixelSize)
				{
					NGUIEditorTools.RegisterUndo("Font Change", mFont);
					mFont.pixelSize = pixelSize;
				}
			}
			EditorGUILayout.Space();
		}

		// Preview option
		if (!mFont.isDynamic && mFont.atlas != null)
		{
			GUILayout.BeginHorizontal();
			{
				mView = (View)EditorGUILayout.EnumPopup("Preview", mView);
				GUILayout.Label("Shader", GUILayout.Width(45f));
				mUseShader = EditorGUILayout.Toggle(mUseShader, GUILayout.Width(20f));
			}
			GUILayout.EndHorizontal();
		}

		// Dynamic fonts don't support emoticons
		if (!mFont.isDynamic && mFont.bmFont.isValid)
		{
			if (mFont.atlas != null)
			{
				NGUIEditorTools.DrawHeader("Symbols and Emoticons");

				List<BMSymbol> symbols = mFont.symbols;

				for (int i = 0; i < symbols.Count; )
				{
					BMSymbol sym = symbols[i];

					GUILayout.BeginHorizontal();
					GUILayout.Label(sym.sequence, GUILayout.Width(40f));
					if (NGUIEditorTools.SimpleSpriteField(mFont.atlas, sym.spriteName, ChangeSymbolSprite))
						mSelectedSymbol = sym;

					if (GUILayout.Button("Edit", GUILayout.Width(40f)))
					{
						if (mFont.atlas != null)
						{
							EditorPrefs.SetString("NGUI Selected Sprite", sym.spriteName);
							NGUIEditorTools.Select(mFont.atlas.gameObject);
						}
					}

					GUI.backgroundColor = Color.red;

					if (GUILayout.Button("X", GUILayout.Width(22f)))
					{
						NGUIEditorTools.RegisterUndo("Remove symbol", mFont);
						mSymbolSequence = sym.sequence;
						mSymbolSprite = sym.spriteName;
						symbols.Remove(sym);
						mFont.MarkAsDirty();
					}
					GUI.backgroundColor = Color.white;
					GUILayout.EndHorizontal();
					GUILayout.Space(4f);
					++i;
				}

				if (symbols.Count > 0)
				{
					NGUIEditorTools.DrawSeparator();
				}

				GUILayout.BeginHorizontal();
				mSymbolSequence = EditorGUILayout.TextField(mSymbolSequence, GUILayout.Width(40f));
				NGUIEditorTools.SimpleSpriteField(mFont.atlas, mSymbolSprite, SelectSymbolSprite);

				bool isValid = !string.IsNullOrEmpty(mSymbolSequence) && !string.IsNullOrEmpty(mSymbolSprite);
				GUI.backgroundColor = isValid ? Color.green : Color.grey;

				if (GUILayout.Button("Add", GUILayout.Width(40f)) && isValid)
				{
					NGUIEditorTools.RegisterUndo("Add symbol", mFont);
					mFont.AddSymbol(mSymbolSequence, mSymbolSprite);
					mFont.MarkAsDirty();
					mSymbolSequence = "";
					mSymbolSprite = "";
				}
				GUI.backgroundColor = Color.white;
				GUILayout.EndHorizontal();

				if (symbols.Count == 0)
				{
					EditorGUILayout.HelpBox("Want to add an emoticon to your font? In the field above type ':)', choose a sprite, then hit the Add button.", MessageType.Info);
				}
				else GUILayout.Space(4f);
			}
		}
	}

	/// <summary>
	/// "New Sprite" selection.
	/// </summary>

	void SelectSymbolSprite (string spriteName)
	{
		mSymbolSprite = spriteName;
		Repaint();
	}

	/// <summary>
	/// Existing sprite selection.
	/// </summary>

	void ChangeSymbolSprite (string spriteName)
	{
		if (mSelectedSymbol != null && mFont != null)
		{
			NGUIEditorTools.RegisterUndo("Change symbol", mFont);
			mSelectedSymbol.spriteName = spriteName;
			Repaint();
			mFont.MarkAsDirty();
		}
	}

	/// <summary>
	/// Draw the font preview window.
	/// </summary>

	public override void OnPreviewGUI (Rect rect, GUIStyle background)
	{
		mFont = target as UIFont;
		if (mFont == null) return;
		Texture2D tex = mFont.texture;

		if (mView != View.Nothing && tex != null)
		{
			Material m = (mUseShader ? mFont.material : null);

			if (mView == View.Font)
			{
				Rect outer = new Rect(mFont.uvRect);
				Rect uv = outer;

				outer = NGUIMath.ConvertToPixels(outer, tex.width, tex.height, true);

				NGUIEditorTools.DrawSprite(tex, rect, outer, outer, uv, Color.white, m);
			}
			else
			{
				Rect outer = new Rect(0f, 0f, 1f, 1f);
				Rect inner = new Rect(mFont.uvRect);
				Rect uv = outer;

				outer = NGUIMath.ConvertToPixels(outer, tex.width, tex.height, true);
				inner = NGUIMath.ConvertToPixels(inner, tex.width, tex.height, true);

				NGUIEditorTools.DrawSprite(tex, rect, outer, inner, uv, Color.white, m);
			}
		}
	}

	/// <summary>
	/// Sprite selection callback.
	/// </summary>

	void SelectSprite (string spriteName)
	{
		NGUIEditorTools.RegisterUndo("Font Sprite", mFont);
		mFont.spriteName = spriteName;
		Repaint();
	}
}
