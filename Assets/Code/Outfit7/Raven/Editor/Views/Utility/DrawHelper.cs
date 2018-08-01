#if !STARLITE_EDITOR
using Outfit7.Logic.Util;
#else
using Starlite;
#endif

using System;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public static class DrawHelper {
        private const int c_CurveQuality = 25;

        private static GUIContent s_EmptyContent = new GUIContent();

        public static void DrawToggleBoxes<T>(bool[] values, RavenAnimationPropertyBase<T> property, Rect position) {
            const float toggleSize = 16f;

            if (property.PropertyType != ERavenAnimationPropertyType.Set || values == null) {
                return;
            }

            var toggleSizeWidth = position.width / values.Length;
            if (toggleSizeWidth > toggleSize) {
                toggleSizeWidth = toggleSize;
            }
            var rect = new Rect(position.x + position.width / 2f - toggleSizeWidth * values.Length / 2f, position.y + position.height - toggleSize, toggleSizeWidth, toggleSize);
            for (int i = 0; i < values.Length; ++i) {
                values[i] = EditorGUI.Toggle(rect, values[i]);
                rect.x += toggleSizeWidth;
            }
        }

        public static void DrawAnimationDataTweenCurves<T>(Vector4 startValue, Vector4 endValue, Vector2 minMax, int repeatCount, bool[] flags, RavenAnimationDataTween<T> tweenData, RavenAnimationPropertyBase<T> property, Color[] colors, Rect position) {
            float left = position.x;
            float right = left + position.width;
            float top = position.y;
            float bottom = top + position.height;

            if (flags == null) {
                flags = new bool[] { true };
            }

            for (int i = 0; i < repeatCount; ++i) {
                for (int j = 0; j < flags.Length; j++) {
                    if (flags[j] || property.PropertyType != ERavenAnimationPropertyType.Set) {
                        DrawTweenCurve(left, right, top, bottom, startValue, endValue, minMax, tweenData, colors, j, i);
                    }
                }
            }
        }

        public static void DrawTweenCurve<T>(float left, float right, float top, float bottom, Vector4 startValue, Vector4 endValue, Vector2 minMax, RavenAnimationDataTween<T> tweenData, Color[] colors, int valueIndex, int repeatIndex) {
            Vector2 startPoint = new Vector2(Mathf.Lerp(left, right, 0), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, startValue[valueIndex])));
            var interval = 1.0f / c_CurveQuality;
            for (int s = 0; s <= c_CurveQuality; s++) {
                var position = interval * s;
                var inputPosition = tweenData.GetTimeForRepeatableMirrorEditor(position, 1, tweenData.RepeatCount, tweenData.Mirror);

                double tweenValue;
                if (tweenData.UseCustomEaseCurve) {
                    tweenValue = tweenData.CustomCurve.Evaluate((float)inputPosition);
                } else {
                    tweenValue = RavenEaseUtility.Evaluate(tweenData.EaseType, inputPosition, 1, tweenData.EaseAmplitude, tweenData.EasePeriod);
                }

                var value = RavenValueInterpolatorFloat.Default.Interpolate(startValue[valueIndex], endValue[valueIndex], tweenValue);
                var endPoint = new Vector2(Mathf.Lerp(left, right, position), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, value)));
                GUIUtil.DrawFatLine(startPoint, endPoint, 5f, colors[valueIndex]);
                startPoint = endPoint;
            }
        }

        public static void DrawProperty(SerializedProperty property, Rect position, Type argumentType, Type objectType, bool isObjectLink = false) {
            const float singleLineHeight = 16f;

            if (property == null) {
                return;
            }

            // We are manually calling things here to avoid having custom property drawers applied
            switch (property.propertyType) {
                case SerializedPropertyType.Quaternion:
                    EditorGUI.BeginChangeCheck();
                    var v4 = property.quaternionValue.ToVector4();
                    v4 = EditorGUI.Vector4Field(position, "", v4);
                    if (EditorGUI.EndChangeCheck()) {
                        property.quaternionValue = v4.ToQuaternion();
                    }
                    break;
                case SerializedPropertyType.Vector2:
                    EditorGUI.BeginChangeCheck();
                    var vector2Value = EditorGUI.Vector2Field(position, "", property.vector2Value);
                    if (EditorGUI.EndChangeCheck()) {
                        property.vector2Value = vector2Value;
                    }
                    break;
                case SerializedPropertyType.Vector3:
                    EditorGUI.BeginChangeCheck();
                    var vector3Value = EditorGUI.Vector3Field(position, "", property.vector3Value);
                    if (EditorGUI.EndChangeCheck()) {
                        property.vector3Value = vector3Value;
                    }
                    break;
                case SerializedPropertyType.Vector4:
                    EditorGUI.BeginChangeCheck();
                    var vector4Value = EditorGUI.Vector4Field(position, "", property.vector4Value);
                    if (EditorGUI.EndChangeCheck()) {
                        property.vector4Value = vector4Value;
                    }
                    break;
                case SerializedPropertyType.Boolean:
                    EditorGUI.BeginChangeCheck();
                    var boolValue = EditorGUI.Toggle(position, property.boolValue);
                    if (EditorGUI.EndChangeCheck()) {
                        property.boolValue = boolValue;
                    }
                    break;
                case SerializedPropertyType.String:
                    position.height = singleLineHeight;
                    EditorGUI.BeginChangeCheck();
                    var stringValue = EditorGUI.TextField(position, s_EmptyContent, property.stringValue);
                    if (EditorGUI.EndChangeCheck()) {
                        property.stringValue = stringValue;
                    }
                    break;
                case SerializedPropertyType.Rect:
                    position.height = singleLineHeight * 2;
                    EditorGUI.BeginChangeCheck();
                    var rectValue = EditorGUI.RectField(position, s_EmptyContent, property.rectValue);
                    if (EditorGUI.EndChangeCheck()) {
                        property.rectValue = rectValue;
                    }
                    break;
                case SerializedPropertyType.Color:
                    position.height = singleLineHeight;
                    EditorGUI.BeginChangeCheck();
                    var colorValue = EditorGUI.ColorField(position, s_EmptyContent, property.colorValue);
                    if (EditorGUI.EndChangeCheck()) {
                        property.colorValue = colorValue;
                    }
                    break;
                case SerializedPropertyType.AnimationCurve:
                    position.height = singleLineHeight;
                    EditorGUI.BeginChangeCheck();
                    var animationCurveValue = EditorGUI.CurveField(position, s_EmptyContent, property.animationCurveValue);
                    if (EditorGUI.EndChangeCheck()) {
                        property.animationCurveValue = animationCurveValue;
                    }
                    break;
                case SerializedPropertyType.Bounds:
                    position.height = singleLineHeight * 2;
                    EditorGUI.BeginChangeCheck();
                    var boundsValue = EditorGUI.BoundsField(position, s_EmptyContent, property.boundsValue);
                    if (EditorGUI.EndChangeCheck()) {
                        property.boundsValue = boundsValue;
                    }
                    break;
                case SerializedPropertyType.Float:
                    position.height = singleLineHeight;
                    EditorGUI.BeginChangeCheck();
                    var floatValue = EditorGUI.FloatField(position, s_EmptyContent, property.floatValue);
                    if (EditorGUI.EndChangeCheck()) {
                        property.floatValue = floatValue;
                    }
                    break;
                case SerializedPropertyType.Integer:
                    position.height = singleLineHeight;
                    EditorGUI.BeginChangeCheck();
                    var intValue = EditorGUI.IntField(position, s_EmptyContent, property.intValue);
                    if (EditorGUI.EndChangeCheck()) {
                        property.intValue = intValue;
                    }
                    break;
                case SerializedPropertyType.Enum:
                    EditorGUI.BeginChangeCheck();
                    var num = EditorGUI.Popup(position, "", (!property.hasMultipleDifferentValues ? property.enumValueIndex : -1), property.enumDisplayNames);
                    if (EditorGUI.EndChangeCheck()) {
                        property.enumValueIndex = num;
                    }
                    break;
                case SerializedPropertyType.ObjectReference:
                    position.height = singleLineHeight;
                    Type objType = argumentType;
                    if (isObjectLink || argumentType == typeof(GameObject) || argumentType == typeof(Component) || argumentType.IsSubclassOf(typeof(Component))) {
                        objType = objectType;
                    }
                    EditorGUI.BeginChangeCheck();
                    var objRef = EditorGUI.ObjectField(position, s_EmptyContent, property.objectReferenceValue, objType, true);
                    if (EditorGUI.EndChangeCheck()) {
                        property.objectReferenceValue = objRef;
                    }
                    break;
                default:
                    EditorGUI.PropertyField(position, property, s_EmptyContent, true);
                    break;
            }
        }
    }
}