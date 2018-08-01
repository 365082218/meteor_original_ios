using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Outfit7.Sequencer {
    public class CustomFieldProperty : BaseProperty {
        public enum CustomFieldPropertyType {
            FLOAT,
            BOOL,
            INT,
            COLOR,
            VECTOR4,
            UNKNOWN
        }

        FieldInfo FieldInfo;
        public string FieldName = "";
        public string ComponentName = "";
        public CustomFieldPropertyType FieldPropertyType;

        public override int GetNumberOfValuesUsed() {
            switch (FieldPropertyType) {
                case CustomFieldPropertyType.FLOAT:
                case CustomFieldPropertyType.BOOL:
                case CustomFieldPropertyType.INT:
                    return 1;
                case CustomFieldPropertyType.COLOR:
                case CustomFieldPropertyType.VECTOR4:
                case CustomFieldPropertyType.UNKNOWN:
                    return 4;
            }
            return 4;
        }

        private bool CheckAndSetFieldInfo(Component component) {
            if (component == null)
                return false;
            
            if (FieldInfo != null)
                return true;

            if (FieldInfo == null && FieldName == "")
                return false;

            FieldInfo = component.GetType().GetField(FieldName);
            Type fieldType = FieldInfo.FieldType;
            if (fieldType == typeof(float))
                FieldPropertyType = CustomFieldPropertyType.FLOAT;
            else if (fieldType == typeof(bool))
                FieldPropertyType = CustomFieldPropertyType.BOOL;
            else if (fieldType == typeof(int))
                FieldPropertyType = CustomFieldPropertyType.INT;
            else if (fieldType == typeof(Color))
                FieldPropertyType = CustomFieldPropertyType.COLOR;
            else if (fieldType == typeof(Vector4))
                FieldPropertyType = CustomFieldPropertyType.VECTOR4;
            else
                FieldPropertyType = CustomFieldPropertyType.UNKNOWN;
            return true;
        }

        public override void OnApply(Component component, Vector4 value) {
            if (!CheckAndSetFieldInfo(component))
                return;
            switch (FieldPropertyType) {
                case CustomFieldPropertyType.FLOAT:
                    FieldInfo.SetValue(component, value.x);
                    break;
                case CustomFieldPropertyType.BOOL:
                    FieldInfo.SetValue(component, value.x > 0.5f);
                    break;
                case CustomFieldPropertyType.INT:
                    FieldInfo.SetValue(component, (int) value.x);
                    break;
                case CustomFieldPropertyType.COLOR:
                    FieldInfo.SetValue(component, new Color(value.x, value.y, value.z, value.w));
                    break;
                case CustomFieldPropertyType.VECTOR4:
                    FieldInfo.SetValue(component, value);
                    break;
                case CustomFieldPropertyType.UNKNOWN:
                    break;
            }
        }

        public override Vector4 OnValue(Component component, out bool success) {
            if (!CheckAndSetFieldInfo(component)) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            switch (FieldPropertyType) {
                case CustomFieldPropertyType.FLOAT:
                    return new Vector4((float) FieldInfo.GetValue(component), 0, 0, 0);
                case CustomFieldPropertyType.BOOL:
                    bool value = (bool) FieldInfo.GetValue(component);
                    return new Vector4(value ? 1 : 0, 0, 0, 0);
                case CustomFieldPropertyType.INT:
                    return new Vector4((float) FieldInfo.GetValue(component), 0, 0, 0);
                case CustomFieldPropertyType.COLOR:
                    Color c = (Color) FieldInfo.GetValue(component);
                    return new Vector4(c.r, c.g, c.b, c.a);
                case CustomFieldPropertyType.VECTOR4:
                    return (Vector4) FieldInfo.GetValue(component);
                case CustomFieldPropertyType.UNKNOWN:
                    success = false;
                    return Vector4.zero;
            }
            success = false;
            return Vector4.zero;
        }
    }
}