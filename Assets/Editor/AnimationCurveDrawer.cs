using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AnimationCurve))]
public class AnimationCurveDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        AnimationCurveGUI.OnGUI(position, property, label);
    }
}
