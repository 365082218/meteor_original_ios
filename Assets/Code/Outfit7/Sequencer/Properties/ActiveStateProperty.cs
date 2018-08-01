using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Sequencer {
    public class ActiveStateProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 1;
        }

        public override void OnApply(Component component, Vector4 value) {
            Transform transform = component as Transform;
            if (transform == null)
                return;
            transform.gameObject.SetActive(value.x >= 1);
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Transform transform = component as Transform;
            if (transform == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return new Vector4(transform.gameObject.activeSelf ? 1 : 0, 0, 0, 0);
        }
    }
}