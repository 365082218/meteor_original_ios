using UnityEditor;
using UnityEngine;

namespace Outfit7.UI {
    public class PanelUiMakerComponent : UiMakerComponent {

        public override string TypeName { get { return "Panel"; } }

        public override bool IsSimpleType { get { return true; } }

        public override string NamePrefix { get { return "pnl"; } }

        private bool IsTouchRectTransform = false;

        public override void Init() {
            
        }

        public override void OnGui() {
            base.OnGui();

            OnNewGameObjectGui();
        }

        protected override RectTransform OnCreateExecute() {
            RectTransform rectTransform = base.OnCreateExecute();

            if (IsTouchRectTransform) {
                rectTransform.gameObject.AddComponent<TouchRectTransform>();
            }

            SetToStretch(rectTransform);

            return rectTransform;
        }

        private void OnNewGameObjectGui() {
            IsTouchRectTransform = EditorGUILayout.Toggle("Add TouchRectTransform", IsTouchRectTransform);

            SetCommonCreatePanel();
        }
    }
}
