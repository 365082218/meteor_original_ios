using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class OutlineColorProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 4;
        }

        public override DisplayMode GetDisplayMode() {
            return DisplayMode.COLOR;
        }

        public override void OnApply(Component component, Vector4 value) {
            Outline image = component as Outline;
            if (image == null)
                return;
            image.effectColor = ApplyPartial(image.effectColor, value);
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorUtility.SetDirty(image);
            }
            #endif
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Outline image = component as Outline;
            if (image == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return new Vector4(image.effectColor.r, image.effectColor.g, image.effectColor.b, image.effectColor.a);
        }
    }
}