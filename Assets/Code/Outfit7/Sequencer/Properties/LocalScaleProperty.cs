using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Sequencer {
    public class LocalScaleProperty : BaseProperty {
        public bool IsUniform = false;

        public override int GetNumberOfValuesUsed() {
            return IsUniform ? 1 : 3;
        }

        public override void OnApply(Component component, Vector4 value) {
            Transform transform = component as Transform;
            if (transform == null)
                return;

            if (UsedThisFrame) {
                transform.localScale = transform.localScale + (Vector3) value;
            } else {
                if (IsUniform)
                    transform.localScale = value.x * Vector3.one;
                else
                    transform.localScale = ApplyPartial(transform.localScale, value);
            }
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Transform transform = component as Transform;
            if (transform == null) {
                success = false;
                return Vector4.one;
            }
            success = true;
            return (Vector4) transform.localScale;
        }
    }
}