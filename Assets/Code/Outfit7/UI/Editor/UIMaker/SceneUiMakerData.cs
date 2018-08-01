using UnityEditor;

namespace Outfit7.UI {
    public class SceneUiMakerData : UiMakerData {

        protected override string TypeName { get { return "Scene"; } }

        public string ScenesFolder = "Scenes";
        public string TemplateUiControllerFilePath = "Code/Outfit7/UI/Editor/UIMaker/CodeTemplates/UiControllerTemplate";
        public string TemplateControllerFilePath = "Code/Outfit7/UI/Editor/UIMaker/CodeTemplates/ControllerTemplate";
        public string TemplateStateFilePath = "Code/Outfit7/UI/Editor/UIMaker/CodeTemplates/StateTemplate";
        public string UiControllerFolderPath = "Code/Controller/UI";
        public string LogicControllerFolderPath = "Code/Controller/Logic";
        public string StateFolderPath = "Code/GameState/State";
        public string LogicControllerNamespace = "Controller.Logic";
        public string UiControllerNamespace = "Controller.Ui";
        public string NamespaceProject = "ProjectName";
        public string GameStateManagerName = "GameStateManager";
        public string StateParentName = "StateControllerSupport";

        private string CurrentScenesFolder = string.Empty;
        private string CurrentTemplateUiControllerFilePath = string.Empty;
        private string CurrentTemplateControllerFilePath = string.Empty;
        private string CurrentTemplateStateFilePath = string.Empty;
        private string CurrentUiControllerFolderPath = string.Empty;
        private string CurrentLogicControllerFolderPath = string.Empty;
        private string CurrentStateFolderPath = string.Empty;
        private string CurrentLogicControllerNamespace = string.Empty;
        private string CurrentUiControllerNamespace = string.Empty;
        private string CurrentNamespaceProject = string.Empty;
        private string CurrentGameStateManagerName = string.Empty;
        private string CurrentStateParentName = string.Empty;

        public override bool ChangesMade { get { 
                return 
                    ScenesFolder != CurrentScenesFolder ||
                    TemplateUiControllerFilePath != CurrentTemplateUiControllerFilePath ||
                    TemplateControllerFilePath != CurrentTemplateControllerFilePath ||
                    TemplateStateFilePath != CurrentTemplateStateFilePath ||
                    UiControllerFolderPath != CurrentUiControllerFolderPath ||
                    LogicControllerFolderPath !=CurrentLogicControllerFolderPath ||
                    StateFolderPath != CurrentStateFolderPath ||
                    LogicControllerNamespace != CurrentLogicControllerNamespace ||
                    UiControllerNamespace != CurrentUiControllerNamespace ||
                    NamespaceProject != CurrentNamespaceProject ||
                    GameStateManagerName != CurrentGameStateManagerName ||
                    StateParentName != CurrentStateParentName;
            }
        }

        public override void OnInit() {
            OnRevertChangedData();
        }

        public override void OnGui() {
            FoldOut = EditorGUILayout.Foldout(FoldOut, TypeName);
            if (FoldOut) {
                EditorGUI.indentLevel++;
                CurrentScenesFolder = EditorGUILayout.TextField("Scenes folder", CurrentScenesFolder);
                EditorGUILayout.Separator();
                CurrentNamespaceProject = EditorGUILayout.TextField("Project Name", CurrentNamespaceProject);
                EditorGUILayout.Separator();
                CurrentTemplateUiControllerFilePath = EditorGUILayout.TextField("UI controller template file path", CurrentTemplateUiControllerFilePath);
                CurrentTemplateControllerFilePath = EditorGUILayout.TextField("Logic controller template file path", CurrentTemplateControllerFilePath);
                CurrentTemplateStateFilePath = EditorGUILayout.TextField("State template file path", CurrentTemplateStateFilePath);
                EditorGUILayout.Separator();
                CurrentUiControllerFolderPath = EditorGUILayout.TextField("UI controller folder path", CurrentUiControllerFolderPath);
                CurrentLogicControllerFolderPath = EditorGUILayout.TextField("Logic controller folder path", CurrentLogicControllerFolderPath);
                CurrentStateFolderPath = EditorGUILayout.TextField("State folder path", CurrentStateFolderPath);
                EditorGUILayout.Separator();
                CurrentUiControllerNamespace = EditorGUILayout.TextField("UI controller namespace", CurrentUiControllerNamespace);
                CurrentLogicControllerNamespace = EditorGUILayout.TextField("Logic controller namespace", CurrentLogicControllerNamespace);
                CurrentGameStateManagerName = EditorGUILayout.TextField("Game state manager name", CurrentGameStateManagerName);
                CurrentStateParentName = EditorGUILayout.TextField("State parent name", CurrentStateParentName);

                OnBottomButtonGui();
                EditorGUI.indentLevel--;
            }
        }

        public override void OnApplyChangedData() {
            ScenesFolder = CurrentScenesFolder;
            TemplateUiControllerFilePath = CurrentTemplateUiControllerFilePath;
            TemplateControllerFilePath = CurrentTemplateControllerFilePath;
            TemplateStateFilePath = CurrentTemplateStateFilePath;
            UiControllerFolderPath = CurrentUiControllerFolderPath;
            LogicControllerFolderPath = CurrentLogicControllerFolderPath;
            StateFolderPath = CurrentStateFolderPath;
            LogicControllerNamespace = CurrentLogicControllerNamespace;
            UiControllerNamespace = CurrentUiControllerNamespace;
            NamespaceProject = CurrentNamespaceProject;
            GameStateManagerName = CurrentGameStateManagerName;
            StateParentName = CurrentStateParentName;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public override void OnRevertChangedData() {
            CurrentScenesFolder = ScenesFolder;
            CurrentTemplateUiControllerFilePath = TemplateUiControllerFilePath;
            CurrentTemplateControllerFilePath = TemplateControllerFilePath;
            CurrentTemplateStateFilePath = TemplateStateFilePath;
            CurrentUiControllerFolderPath = UiControllerFolderPath;
            CurrentLogicControllerFolderPath = LogicControllerFolderPath;
            CurrentStateFolderPath = StateFolderPath;
            CurrentLogicControllerNamespace = LogicControllerNamespace;
            CurrentUiControllerNamespace = UiControllerNamespace;
            CurrentNamespaceProject = NamespaceProject;
            CurrentGameStateManagerName = GameStateManagerName;
            CurrentStateParentName = StateParentName;
            AssetDatabase.SaveAssets();
        }
    }
}
