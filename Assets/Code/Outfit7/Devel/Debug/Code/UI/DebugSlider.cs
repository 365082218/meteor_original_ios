using UnityEngine.Events;
using UnityEngine.UI;

namespace Outfit7.Devel.O7Debug.UI {
    public class DebugSlider : Slider {

        public string SliderInfo = "";
        private int Id = -1;
        private UnityAction<int, float> Action = null;

        public void AddAction(UnityAction<int, float> action, int id, string info) {
            Id = id;
            Action = action;
            SliderInfo = info;
            onValueChanged.AddListener(ValueChanged);
        }

        private void ValueChanged(float value) {
            Action.Invoke(Id, value);
        }

        public void Clear() {
            Id = -1;
            Action = null;
            SliderInfo = "";
            onValueChanged.RemoveAllListeners();
        }
    }
}
