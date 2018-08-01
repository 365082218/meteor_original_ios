using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Outfit7.Sequencer {
    public class ButtonInteractableProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 1;
        }

        public override void OnApply(Component component, Vector4 value) {
            Button button = component as Button;
            if (button == null)
                return;
            button.interactable = value.x >= 1f;
        }

        public override Vector4 OnValue(Component component, out bool success) {
            Button button = component as Button;
            if (button == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return new Vector4(button.interactable ? 1 : 0, 0, 0, 0);
        }
    }
}