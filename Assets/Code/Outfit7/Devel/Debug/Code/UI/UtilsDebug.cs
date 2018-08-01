using System;
using System.Reflection;
using Outfit7.Devel.O7Debug.UI.Exceptions;

namespace Outfit7.Devel.O7Debug {

    public class UtilsDebug {

        /// <summary>
        /// Gets float value from a field using reflection.
        /// </summary>
        public static float GetFloat(FieldInfo fieldInfo, object field, out bool isInt) {
            isInt = false;
            object valueO = fieldInfo.GetValue(field);

            float value;

            if (fieldInfo.FieldType == typeof(float)) {
                value = (float) valueO;
            } else if (fieldInfo.FieldType == typeof(int)) {
                isInt = true;
                value = Convert.ToSingle(valueO);
            } else {
                throw new UnsupportedSliderValueType();
            }

            return value;
        }

        /// <summary>
        /// Sets float value to a field using reflection.
        /// </summary>
        public static void SetFloat(FieldInfo fieldInfo, object field, float value, out bool isInt) {
            isInt = false;

            if (fieldInfo.FieldType == typeof(int)) {
                isInt = true;
                fieldInfo.SetValue(field, (int) value);
            } else if (fieldInfo.FieldType == typeof(float)) {
                fieldInfo.SetValue(field, (float) value);
            } else {
                throw new UnsupportedSliderValueType();
            }
        }
    }
}
