using UnityEditor;
using UnityEngine;

namespace Outfit7.UI {
    public class DialogUiMakerData : UiMakerData {

        protected override string TypeName { get { return "Dialog"; } }

        public string TemplateDialogControllerFilePath = "Code/Outfit7/UI/Editor/UIMaker/CodeTemplates/DialogControllerTemplate";
        public string TemplateDialogStateFilePath = "Code/Outfit7/UI/Editor/UIMaker/CodeTemplates/DialogControllerTemplate";
        public string DialogControllerFolderPath = "Code/Controller/Dialog";
        public string DialogStateFolderPath = "Code/DialogState/State";
        public string DialogPrefabFolderPath = "Resources/UI/Dialogs";

        private string CurrentTemplateDialogControllerFilePath = string.Empty;
        private string CurrentTemplateDialogStateFilePath = string.Empty;
        private string CurrentDialogControllerFolderPath = string.Empty;
        private string CurrentDialogStateFolderPath = string.Empty;
        private string CurrentDialogPrefabFolderPath = string.Empty;

        public override bool ChangesMade { 
            get { 
                return 
                    TemplateDialogControllerFilePath != CurrentTemplateDialogControllerFilePath ||
                    TemplateDialogStateFilePath != CurrentTemplateDialogStateFilePath ||
                    DialogControllerFolderPath != CurrentDialogControllerFolderPath ||
                    DialogStateFolderPath != CurrentDialogStateFolderPath ||
                    DialogPrefabFolderPath != CurrentDialogPrefabFolderPath;
            }
        }

        public override void OnInit() {
            OnRevertChangedData();
        }

        public override void OnGui() {
            FoldOut = EditorGUILayout.Foldout(FoldOut, TypeName);
            if (FoldOut) {
                EditorGUI.indentLevel++;
                CurrentTemplateDialogControllerFilePath = EditorGUILayout.TextField("Controller template file path", CurrentTemplateDialogControllerFilePath);
                CurrentTemplateDialogStateFilePath = EditorGUILayout.TextField("State template file path", CurrentTemplateDialogStateFilePath);
                EditorGUILayout.Separator();
                CurrentDialogControllerFolderPath = EditorGUILayout.TextField("Controller folder path", CurrentDialogControllerFolderPath);
                CurrentDialogStateFolderPath = EditorGUILayout.TextField("State folder path", CurrentDialogStateFolderPath);
                CurrentDialogPrefabFolderPath = EditorGUILayout.TextField("Prefab folder path", CurrentDialogPrefabFolderPath);

                OnBottomButtonGui();
                EditorGUI.indentLevel--;
            }
        }

        public override void OnApplyChangedData() {
            TemplateDialogControllerFilePath = CurrentTemplateDialogControllerFilePath;
            TemplateDialogStateFilePath = CurrentTemplateDialogStateFilePath;
            DialogControllerFolderPath = CurrentDialogControllerFolderPath;
            DialogStateFolderPath = CurrentDialogStateFolderPath;
            DialogPrefabFolderPath = CurrentDialogPrefabFolderPath;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public override void OnRevertChangedData() {
            CurrentTemplateDialogControllerFilePath = TemplateDialogControllerFilePath;
            CurrentTemplateDialogStateFilePath = TemplateDialogStateFilePath;
            CurrentDialogControllerFolderPath = DialogControllerFolderPath;
            CurrentDialogStateFolderPath = DialogStateFolderPath;
            CurrentDialogPrefabFolderPath = DialogPrefabFolderPath;
            AssetDatabase.SaveAssets();
        }
    }
}
