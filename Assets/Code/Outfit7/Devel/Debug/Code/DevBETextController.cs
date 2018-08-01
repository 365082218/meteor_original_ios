using UnityEngine;

namespace Outfit7.Devel.O7Debug {
    public class DevBETextController : MonoBehaviour {

#pragma warning disable 0414
        [SerializeField] private UnityEngine.UI.Text Text = null;
#pragma warning restore 0414

        public static bool ShowDevBEText = false;

#if UNITY_EDITOR || DEVEL_BUILD || PROD_BUILD
        private void LateUpdate() {
            if (Text.enabled != ShowDevBEText) {
                Text.enabled = ShowDevBEText;
            }
        }
#endif
    }
}
