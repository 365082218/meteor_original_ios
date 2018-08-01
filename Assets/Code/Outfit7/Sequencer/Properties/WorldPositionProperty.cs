using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Sequencer {
    public class WorldPositionProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 3;
        }

        public override void OnApply(Component component, Vector4 value) {
            Transform transform = component as Transform;
            if (transform == null)
                return;
            transform.position = ApplyPartial(transform.position, value);
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Transform transform = component as Transform;
            if (transform == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return (Vector4) transform.position;
        }
    }
}