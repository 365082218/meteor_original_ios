using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class ShadowDistanceProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 2;
        }

        public override DisplayMode GetDisplayMode() {
            return DisplayMode.CURVE;
        }

        public override void OnApply(Component component, Vector4 value) {
            Shadow image = component as Shadow;
            if (image == null)
                return;
            image.effectDistance = ApplyPartial(image.effectDistance, value);
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorUtility.SetDirty(image);
            }
            #endif
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Shadow image = component as Shadow;
            if (image == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return new Vector4(image.effectDistance.x, image.effectDistance.y, 0, 0);
        }
    }
}