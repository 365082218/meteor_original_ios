using UnityEngine;
using UnityEditor;

namespace Outfit7.UI {
    public class ProgressBarUiMakerComponent : UiMakerComponent {

        public override string TypeName { get { return "ProgressBar"; } }

        public override bool IsSimpleType { get { return false; } }

        public override string NamePrefix { get { return "prb"; } }

        public override void  Init() {
            
        }

        public override void OnGui() {
            base.OnGui();

            EditorGUILayout.HelpBox("WIP", MessageType.Warning);

            SetCommonCreatePanel();
        }

        protected override RectTransform OnCreateExecute() {
            RectTransform rectTransform = base.OnCreateExecute();

            return rectTransform;
        }
    }
}
