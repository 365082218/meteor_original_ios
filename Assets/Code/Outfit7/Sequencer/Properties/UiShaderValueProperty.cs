using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Sequencer;
using UnityEngine.UI;

namespace Outfit7.Sequencer {
    public class UiShaderValueProperty : BaseProperty {
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
            Graphic renderer = component as Graphic;
            if (renderer == null)
                return;
            switch (ValueType) {
                case ShaderValueProperty.ShaderValueType.FLOAT:
                    renderer.material.SetFloat(ValueName, value.x);
                    break;

                case ShaderValueProperty.ShaderValueType.COLOR:
                    renderer.material.SetColor(ValueName, new Color(value.x, value.y, value.z, value.w));
                    break;

                case ShaderValueProperty.ShaderValueType.VECTOR:
                    renderer.material.SetVector(ValueName, value);
                    break;
            }
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Graphic renderer = component as Graphic;
            if (renderer == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            switch (ValueType) {
                case ShaderValueProperty.ShaderValueType.FLOAT:
                    return new Vector4(renderer.material.GetFloat(ValueName), 0, 0, 0);

                case ShaderValueProperty.ShaderValueType.COLOR:
                    Color c = renderer.material.GetColor(ValueName);
                    return new Vector4(c.r, c.g, c.b, c.a);

                case ShaderValueProperty.ShaderValueType.VECTOR:
                    return renderer.material.GetVector(ValueName);
            }
            success = false;
            return Vector4.zero;
        }
    }
}