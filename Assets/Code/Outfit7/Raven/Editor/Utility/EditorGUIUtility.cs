using System;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Internal;

namespace Starlite.Raven {

    public static class EditorGUICustom {
        private static string s_AllowedCharactersForFloat = "inftynaeINFTYNAE0123456789.,-*/+%^()";
        private static GUIContent s_Text = new GUIContent();
        private static MethodInfo s_DelayedTextFieldInternalMethodInfo;

        static EditorGUICustom() {
            s_DelayedTextFieldInternalMethodInfo = typeof(EditorGUI).GetMethod("DelayedTextFieldInternal", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(Rect), typeof(string), typeof(string), typeof(GUIStyle) }, null);
        }

        public static double DelayedDoubleField(Rect position, double value) {
            return EditorGUICustom.DelayedDoubleField(position, value, EditorStyles.numberField);
        }

        /// <summary>
        ///   <para>Make a delayed text field for entering doubles.</para>
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the double field.</param>
        /// <param name="label">Optional label to display in front of the double field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="style">Optional GUIStyle.</param>
        /// <returns>
        ///   <para>The value entered by the user. Note that the return value will not change until the user has pressed enter or focus is moved away from the double field.</para>
        /// </returns>
        public static double DelayedDoubleField(Rect position, double value, [DefaultValue("EditorStyles.numberField")] GUIStyle style) {
            return EditorGUICustom.DelayedDoubleFieldInternal(position, null, value, style);
        }

        [ExcludeFromDocs]
        public static double DelayedDoubleField(Rect position, string label, double value) {
            return EditorGUICustom.DelayedDoubleField(position, label, value, EditorStyles.numberField);
        }

        /// <summary>
        ///   <para>Make a delayed text field for entering doubles.</para>
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the double field.</param>
        /// <param name="label">Optional label to display in front of the double field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="style">Optional GUIStyle.</param>
        /// <returns>
        ///   <para>The value entered by the user. Note that the return value will not change until the user has pressed enter or focus is moved away from the double field.</para>
        /// </returns>
        public static double DelayedDoubleField(Rect position, string label, double value, [DefaultValue("EditorStyles.numberField")] GUIStyle style) {
            double num = EditorGUICustom.DelayedDoubleField(position, EditorGUICustom.TempContent(label), value, style);
            return num;
        }

        [ExcludeFromDocs]
        public static double DelayedDoubleField(Rect position, GUIContent label, double value) {
            return EditorGUICustom.DelayedDoubleField(position, label, value, EditorStyles.numberField);
        }

        /// <summary>
        ///   <para>Make a delayed text field for entering doubles.</para>
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the double field.</param>
        /// <param name="label">Optional label to display in front of the double field.</param>
        /// <param name="value">The value to edit.</param>
        /// <param name="style">Optional GUIStyle.</param>
        /// <returns>
        ///   <para>The value entered by the user. Note that the return value will not change until the user has pressed enter or focus is moved away from the double field.</para>
        /// </returns>
        public static double DelayedDoubleField(Rect position, GUIContent label, double value, [DefaultValue("EditorStyles.numberField")] GUIStyle style) {
            return EditorGUICustom.DelayedDoubleFieldInternal(position, label, value, style);
        }

        private static double DelayedDoubleFieldInternal(Rect position, GUIContent label, double value, GUIStyle style) {
            double num = value;
            double num1 = num;
            if (label != null) {
                position = EditorGUI.PrefixLabel(position, label);
            }
            EditorGUI.BeginChangeCheck();
            string str = (string)s_DelayedTextFieldInternalMethodInfo.Invoke(null, new object[] { position, num.ToString(), EditorGUICustom.s_AllowedCharactersForFloat, style });
            if (EditorGUI.EndChangeCheck()) {
                if (double.TryParse(str, NumberStyles.Float, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, out num1) && num1 != num) {
                    value = num1;
                    GUI.changed = true;
                }
            }
            return num1;
        }

        private static GUIContent TempContent(string t) {
            EditorGUICustom.s_Text.text = t;
            return EditorGUICustom.s_Text;
        }
    }
}