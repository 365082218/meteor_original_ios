using UnityEditor;

namespace Outfit7.UI {
    public class ScrollViewUiMakerData : UiMakerData {

        protected override string TypeName { get { return "ScrollView"; } }

        public string TemplateCellDataFilePath = "Code/Outfit7/UI/Editor/UIMaker/CodeTemplates/CellDataTemplate";
        public string TemplateCellControllerFilePath = "Code/Outfit7/UI/Editor/UIMaker/CodeTemplates/CellControllerTemplate";
        public string CellDataFolderPath = "Test";
        public string CellControllerFolderPath = "Test";
        public string CellPrefabFolderPath = "Test/Cells";

        private string CurrentTemplateCellDataFilePath = string.Empty;
        private string CurrentTemplateCellControllerFilePath = string.Empty;
        private string CurrentCellDataFolderPath = string.Empty;
        private string CurrentCellControllerFolderPath = string.Empty;
        private string CurrentCurrentCellPrefabFolderPath = string.Empty;

        public override bool ChangesMade { 
            get { 
                return 
                    TemplateCellDataFilePath != CurrentTemplateCellDataFilePath ||
                    TemplateCellControllerFilePath != CurrentTemplateCellControllerFilePath ||
                    CellDataFolderPath != CurrentCellDataFolderPath ||
                    CellControllerFolderPath != CurrentCellControllerFolderPath ||
                    CellPrefabFolderPath != CurrentCurrentCellPrefabFolderPath;
            }
        }

        public override void OnInit() {
            OnRevertChangedData();
        }

        public override void OnGui() {
            FoldOut = EditorGUILayout.Foldout(FoldOut, TypeName);
            if (FoldOut) {
                EditorGUI.indentLevel++;
                CurrentTemplateCellDataFilePath = EditorGUILayout.TextField("Cell data template path", CurrentTemplateCellDataFilePath);
                CurrentTemplateCellControllerFilePath = EditorGUILayout.TextField("Cell controller template path", CurrentTemplateCellControllerFilePath);
                EditorGUILayout.Separator();
                CurrentCellDataFolderPath = EditorGUILayout.TextField("Cell data folder path", CurrentCellDataFolderPath);
                CurrentCellControllerFolderPath = EditorGUILayout.TextField("Cell controller folder path", CurrentCellControllerFolderPath);
                EditorGUILayout.Separator();
                CurrentCurrentCellPrefabFolderPath = EditorGUILayout.TextField("Cell prefab folder path", CurrentCurrentCellPrefabFolderPath);
                OnBottomButtonGui();
                EditorGUI.indentLevel--;
            }
        }

        public override void OnApplyChangedData() {
            TemplateCellDataFilePath = CurrentTemplateCellDataFilePath;
            TemplateCellControllerFilePath = CurrentTemplateCellControllerFilePath;
            CellDataFolderPath = CurrentCellDataFolderPath;
            CellControllerFolderPath = CurrentCellControllerFolderPath;
            CellPrefabFolderPath = CurrentCurrentCellPrefabFolderPath;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public override void OnRevertChangedData() {
            CurrentTemplateCellDataFilePath = TemplateCellDataFilePath;
            CurrentTemplateCellControllerFilePath = TemplateCellControllerFilePath;
            CurrentCellDataFolderPath = CellDataFolderPath;
            CurrentCellControllerFolderPath = CellControllerFolderPath;
            CurrentCurrentCellPrefabFolderPath = CellPrefabFolderPath;
            AssetDatabase.SaveAssets();
        }
    }
}
