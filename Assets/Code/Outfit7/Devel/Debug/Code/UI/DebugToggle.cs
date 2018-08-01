using UnityEngine.UI;
using UnityEngine.Events;

namespace Outfit7.Devel.O7Debug.UI {
    public class DebugToggle : Toggle {

        private int Id = -1;
        private UnityAction<int, bool> Action = null;

        public void AddAction(UnityAction<int, bool> action, int id) {
            Id = id;
            Action = action;
            onValueChanged.AddListener(ValueChanged);
        }

        private void ValueChanged(bool value) {
            Action.Invoke(Id, value);
        }

        public void Clear() {
            Id = -1;
            Action = null;
            onValueChanged.RemoveAllListeners();
        }



    }
}
