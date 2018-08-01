using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Sequencer {
    public class AnchoredPositionProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 2;
        }

        public override void OnApply(Component component, Vector4 value) {
            RectTransform transform = component as RectTransform;
            if (transform == null)
                return;
            transform.anchoredPosition = ApplyPartial(transform.anchoredPosition, value);
        }

        public override Vector4 OnValue(Component component, out bool success) {
            RectTransform transform = component as RectTransform;
            if (transform == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return (Vector2) transform.anchoredPosition;
        }
    }
}