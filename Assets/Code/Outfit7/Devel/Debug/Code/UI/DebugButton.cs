using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Outfit7.Devel.O7Debug.UI {
    public class DebugButton : Button {

        private int Id = -1;
        private UnityAction<int> Action = null;

        public void AddAction(UnityAction<int> action, int id) {
            Id = id;
            Action = action;
        }

        public void Clear() {
            Id = -1;
            Action = null;
            onClick.RemoveAllListeners();
        }

        public override void OnPointerClick(PointerEventData eventData) {
            if (Action != null) {
                Action(Id);
                return;
            }
            base.OnPointerClick(eventData);

        }
    }
}
