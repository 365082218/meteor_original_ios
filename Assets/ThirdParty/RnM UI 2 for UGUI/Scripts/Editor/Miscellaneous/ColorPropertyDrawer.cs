using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Color))]
public class ColorPropertyDrawer : PropertyDrawer
{
	private const float textFieldWidth = 86f;
	private const float spacing = 5f;

	public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
	{
		label = EditorGUI.BeginProperty(pos, label, prop);

		// Draw label
		pos = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);
		
		// Don't make child fields be indented
		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		Color32 color = prop.colorValue;
		Color32 color2 = EditorGUI.ColorField(new Rect(pos.x, pos.y, (pos.width - textFieldWidth - spacing), pos.height), prop.colorValue);

		if (!color2.Equals(color))
		{
			prop.colorValue = color = color2;
		}

		string colorString = EditorGUI.TextField(new Rect((pos.x + (pos.width - textFieldWidth) + spacing), pos.y, (textFieldWidth - spacing - 5f), pos.height), CommonColorBuffer.ColorToString(color));
		try
		{
			color2 = CommonColorBuffer.StringToColor(colorString);
			if (!color2.Equals(color))
			{
				prop.colorValue = color = color2;
			}
		}
		catch
		{
		}

		// Set indent back to what it was
		EditorGUI.indentLevel = indent;

		EditorGUI.EndProperty();
	}
}
