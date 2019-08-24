using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestCurve))]
public class TestCurveEditor : Editor
{
    SerializedProperty curveProp;
    SerializedProperty curveProp2;
    private AnimationCurve m_TestAnimCurve = new AnimationCurve();
    private AnimationCurve m_TestAnimCurve2 = new AnimationCurve();

    void OnEnable()
    {
        curveProp = serializedObject.FindProperty("curve");
        curveProp2 = serializedObject.FindProperty("curve2");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(curveProp);
        EditorGUILayout.PropertyField(curveProp2, new GUIContent("Curve2 Object"));

        m_TestAnimCurve = AnimationCurveGUI.CurveField("扩展曲线", m_TestAnimCurve);
        m_TestAnimCurve2 = EditorGUILayout.CurveField("默认曲线", m_TestAnimCurve2);

        //EditorGUILayout.TextField("abc");

        serializedObject.ApplyModifiedProperties();
    }
}
