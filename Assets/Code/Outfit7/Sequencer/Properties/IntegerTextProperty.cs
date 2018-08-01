using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class IntegerTextProperty : BaseProperty {

        public string Prefix = "";
        public string Suffix = "";

        public override int GetNumberOfValuesUsed() {
            return 1;
        }

        public override void OnApply(Component component, Vector4 value) {
            UnityEngine.UI.Text text = component as UnityEngine.UI.Text;
            if (text == null)
                return;

            text.text = Prefix + ((int) (value.x + 0.5f)) + Suffix;
            
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorUtility.SetDirty(text);
            }
            #endif
        }

        public override Vector4 OnValue(Component component, out bool success) {
            UnityEngine.UI.Text text = component as UnityEngine.UI.Text;
            if (text == null) {
                success = false;
                return Vector4.zero;
            }
            success = false;
            return Vector4.zero;
        }
    }
}