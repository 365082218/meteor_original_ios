using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class GraphicEnableProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 1;
        }

        public override DisplayMode GetDisplayMode() {
            return DisplayMode.CURVE;
        }

        public override void OnApply(Component component, Vector4 value) {
            Graphic image = component as Graphic;
            if (image == null)
                return;
            image.enabled = value.x >= 1;
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorUtility.SetDirty(image);
            }
            #endif
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Graphic image = component as Graphic;
            if (image == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return new Vector4(image.enabled ? 1 : 0, 0, 0, 0);
        }
    }
}