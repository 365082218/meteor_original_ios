using UnityEditor;

namespace Outfit7.UI {
    public class ButtonUiMakerData : UiMakerData {

        protected override string TypeName { get { return "Button"; } }

        public string DialogActionButtonNamespace = "Outfit7.UI";
        public string GameActionButtonNamespace = "Outfit7.UI";

        private string CurrentDialogActionButtonNamespace = string.Empty;
        private string CurrentGameActionButtonNamespace = string.Empty;

        public override bool ChangesMade { get { return 
                DialogActionButtonNamespace != CurrentDialogActionButtonNamespace ||
            GameActionButtonNamespace != CurrentGameActionButtonNamespace; } }

        public override void OnInit() {
            OnRevertChangedData();
        }

        public override void OnGui() {
            FoldOut = EditorGUILayout.Foldout(FoldOut, TypeName);
            if (FoldOut) {
                EditorGUI.indentLevel++;
                CurrentDialogActionButtonNamespace = EditorGUILayout.TextField("DialogAction button namespace", CurrentDialogActionButtonNamespace);
                EditorGUILayout.Separator();
                CurrentGameActionButtonNamespace = EditorGUILayout.TextField("GameAction button namespace", CurrentGameActionButtonNamespace);
                EditorGUILayout.Separator();

                OnBottomButtonGui();
                EditorGUI.indentLevel--;
            }
        }

        public override void OnApplyChangedData() {
            DialogActionButtonNamespace = CurrentDialogActionButtonNamespace;
            GameActionButtonNamespace = CurrentGameActionButtonNamespace;

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public override void OnRevertChangedData() {
            CurrentDialogActionButtonNamespace = DialogActionButtonNamespace;
            CurrentGameActionButtonNamespace = GameActionButtonNamespace;

            AssetDatabase.SaveAssets();
        }
    }
}
