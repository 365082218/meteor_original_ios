using UnityEngine;

namespace Outfit7.Sequencer {
    public class CanvasInteractableProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 1;
        }

        public override void OnApply(Component component, Vector4 value) {
            CanvasGroup canvasGroup = component as CanvasGroup;
            if (canvasGroup == null)
                return;
            canvasGroup.interactable = value.x >= 1;
        }

        public override Vector4 OnValue(Component component, out bool success) {
            CanvasGroup canvasGroup = component as CanvasGroup;
            if (canvasGroup == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            return new Vector4(canvasGroup.interactable ? 1 : 0, 0, 0, 0);
        }
    }
}