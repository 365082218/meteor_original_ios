using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Sequencer {
    public class GlobalShaderValueProperty : BaseProperty {

        public ShaderValueProperty.ShaderValueType ValueType;
        public string ValueName = "";

        public override int GetNumberOfValuesUsed() {
            switch (ValueType) {
                case ShaderValueProperty.ShaderValueType.FLOAT:
                    return 1;

                case ShaderValueProperty.ShaderValueType.COLOR:
                case ShaderValueProperty.ShaderValueType.VECTOR:
                    return 4;
            }
            return 4;
        }

        public override DisplayMode GetDisplayMode() {
            if (ValueType == ShaderValueProperty.ShaderValueType.COLOR)
                return DisplayMode.COLOR;
            else
                return DisplayMode.CURVE;
        }

        public override void OnApply(Component component, Vector4 value) {
            switch (ValueType) {
                case ShaderValueProperty.ShaderValueType.FLOAT:
                    Shader.SetGlobalFloat(ValueName, value.x);
                    break;

                case ShaderValueProperty.ShaderValueType.COLOR:
                    Shader.SetGlobalColor(ValueName, new Color(value.x, value.y, value.z, value.w));
                    break;

                case ShaderValueProperty.ShaderValueType.VECTOR:
                    Shader.SetGlobalVector(ValueName, value);
                    break;
            }
        }

        public override Vector4 OnValue(Component component, out bool success) {   
            success = false;
            return Vector4.zero;
        }
    }
}