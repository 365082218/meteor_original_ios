using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Sequencer {
    public class ShaderValueProperty : BaseProperty {
        public enum ShaderValueType {
            FLOAT,
            COLOR,
            VECTOR
        }

        public ShaderValueType ValueType;
        public string ValueName = "";

        public override int GetNumberOfValuesUsed() {
            switch (ValueType) {
                case ShaderValueType.FLOAT:
                    return 1;

                case ShaderValueType.COLOR:
                case ShaderValueType.VECTOR:
                    return 4;
            }
            return 4;
        }

        public override DisplayMode GetDisplayMode() {
            if (ValueType == ShaderValueType.COLOR)
                return DisplayMode.COLOR;
            else
                return DisplayMode.CURVE;
        }

        public override void OnApply(Component component, Vector4 value) {
            Renderer renderer = component as Renderer;
            if (renderer == null)
                return;
            switch (ValueType) {
                case ShaderValueType.FLOAT:
                    renderer.sharedMaterial.SetFloat(ValueName, value.x);
                    break;

                case ShaderValueType.COLOR:
                    renderer.sharedMaterial.SetColor(ValueName, new Color(value.x, value.y, value.z, value.w));
                    break;

                case ShaderValueType.VECTOR:
                    renderer.sharedMaterial.SetVector(ValueName, ApplyPartial(renderer.sharedMaterial.GetVector(ValueName), value));
                    break;
            }
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Renderer renderer = component as Renderer;
            if (renderer == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            switch (ValueType) {
                case ShaderValueType.FLOAT:
                    return new Vector4(renderer.sharedMaterial.GetFloat(ValueName), 0, 0, 0);

                case ShaderValueType.COLOR:
                    Color c = renderer.sharedMaterial.GetColor(ValueName);
                    return new Vector4(c.r, c.g, c.b, c.a);

                case ShaderValueType.VECTOR:
                    return renderer.sharedMaterial.GetVector(ValueName);
            }
            success = false;
            return Vector4.zero;
        }
    }
}