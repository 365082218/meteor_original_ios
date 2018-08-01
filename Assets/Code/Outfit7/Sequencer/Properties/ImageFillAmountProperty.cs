using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class ImageFillAmountProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 1;
        }

        public override void OnApply(Component component, Vector4 value) {
            Image image = component as Image;
            if (image == null)
                return;
            image.fillAmount = value.x;
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorUtility.SetDirty(image);
            }
            #endif
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Image image = component as Image;
            if (image == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return new Vector4(image.fillAmount, 0, 0, 0);
        }
    }
}