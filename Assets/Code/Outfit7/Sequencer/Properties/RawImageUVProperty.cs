using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class RawImageUVProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 4;
        }

        public override void OnApply(Component component, Vector4 value) {
            RawImage image = component as RawImage;
            if (image == null)
                return;
            image.uvRect = ApplyPartial(image.uvRect, value);
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorUtility.SetDirty(image);
            }
            #endif
        }

        public override Vector4 OnValue(Component component, out bool success) {
            RawImage image = component as RawImage;
            if (image == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return new Vector4(image.uvRect.x, image.uvRect.y, image.uvRect.width, image.uvRect.height);
        }
    }
}