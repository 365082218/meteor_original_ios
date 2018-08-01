using UnityEngine;
using Outfit7.Text.Localization;

namespace Outfit7.UI {
    [ExecuteInEditMode]
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    [AddComponentMenu("UI/Localizer", 52)]

    public class Localizer : UnityEngine.EventSystems.UIBehaviour {
        [Tooltip("Safety variable, if the text is changing from code (using Localize(string) and Localize(string,args)) this checkbox should be set to true")]
        public bool Dynamic;
        public bool AllCaps;
        public bool AsianBold;
        [SerializeField] private string Key;
        [SerializeField] private UnityEngine.UI.Text Text = null;
        private bool TextAlreadyLocalizedOnEnable;

        public UnityEngine.UI.Text GetText() {
            SetTextComponent();
            return Text;
        }

        public void Localize(string newKey) {
            LocalizeSafe(newKey, null);
        }

        public void Localize(string newKey, params object[] args) {
            LocalizeSafe(newKey, args);
        }

        public void LocalizeStatic() {
            SetTextComponent();

            if (Dynamic) {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    Debug.LogWarning("Dynamic flag is enabled!");
                }
#endif
                return;
            }

            if (string.IsNullOrEmpty(Key)) {
                Key = Text.text;
            } else {
                LocalizeUnsafe(Key, null);
            }

            TextAlreadyLocalizedOnEnable = true;
        }

#if UNITY_EDITOR
        public void LocalizeEditor(string locaValue) {
            SetTextComponent();
            if (string.IsNullOrEmpty(locaValue)) {
                Text.text = Key + " (" + L10n.Language + ")";
            } else {
                Text.text = ProcessText(locaValue);
            }

            if (!Application.isPlaying) {
                UnityEditor.EditorUtility.SetDirty(Text);
            }
        }

        public void SetKeyEditor(string key) {
            Key = key;
        }
#endif

        private void SetTextComponent() {
            if (Text == null) {
                Text = GetComponent<UnityEngine.UI.Text>();
            }
        }

        private string ProcessText(string val) {
            if (AllCaps)
                val = val.ToUpper();

            if (AsianBold && L10n.IsEasternLanguage)
                val = string.Format("<b>{0}</b>", val);

            return val;
        }

        private void LocalizeUnsafe(string newKey, params object[] args) {
            string val = L10n.GetText(newKey, args);

            SetTextComponent();

            Text.text = ProcessText(val);

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEditor.EditorUtility.SetDirty(Text);
            }
#endif
        }

        private void LocalizeSafe(string newKey, params object[] args) {
            Outfit7.Util.Assert.IsTrue(Dynamic, "If this text is meant to change from code enable Dynamic flag on Localizer script, GameObject: {0}", name);
            LocalizeUnsafe(newKey, args);
        }

        protected override void OnEnable() {
            base.OnEnable();

#if UNITY_EDITOR
            SetTextComponent();

            if (!Application.isPlaying)
                return;
#endif

            LocalizeStatic();
        }

        protected override void Start() {
            base.Start();

#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (TextAlreadyLocalizedOnEnable)
                return;

            LocalizeStatic();
        }
    }
}