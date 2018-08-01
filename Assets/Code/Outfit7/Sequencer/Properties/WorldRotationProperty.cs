using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Sequencer {
    public class WorldRotationProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 3;
        }

        public override void OnApply(Component component, Vector4 value) {
            Transform transform = component as Transform;
            if (transform == null)
                return;
            transform.eulerAngles = ApplyPartial(transform.eulerAngles, value);
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Transform transform = component as Transform;
            if (transform == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return (Vector4) transform.eulerAngles;
        }
    }
}