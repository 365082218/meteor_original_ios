using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
#if !UNITY_5_1 && !UNITY_5_2
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace Outfit7.UI {
    public class SceneUiMakerComponent : UiMakerComponent {

        public override string TypeName { get { return "Scene"; } }

        public override bool IsSimpleType { get { return true; } }

        public override string NamePrefix { get { return "sce"; } }

        private SceneUiMakerData Data;

        public override void Init() {
            Data = ProjectUiMakerComponent.GetData<SceneUiMakerData>();
        }

        public override void OnGui() {
            base.OnGui();

            if (GUILayout.Button("Create scene", GUILayout.Height(33f))) {
                OnCreateExecute();
            }
            if (GUILayout.Button("Create scene scripts", GUILayout.Height(33f))) {
                OnSceneScriptsCreateExecute();
            }
            if (GUILayout.Button("Create scene object", GUILayout.Height(33f))) {
                OnSceneObjectsCreateExecute();
            }
        }

        protected override RectTransform OnCreateExecute() {
            if (string.IsNullOrEmpty(Name)) {
                throw new UnityException("Name is not set!");
            }

            #if UNITY_5_1 || UNITY_5_2
            if (EditorApplication.isSceneDirty) {
                string sceneName = GetCurrentSceneName(true);
                switch (EditorUtility.DisplayDialogComplex("Scene Has Been Modified", 
                    "Do you want to save the changes you made in the scene " + sceneName + "?\n\nYour changes will be lost if you don't save them.", 
                    "Save", 
                    "Don't Save",
                    "Cancel")) {
                    case 0:
                        EditorApplication.SaveScene();
                        CreateScene(SetPascalCase(Name));
                        break;
                    case 1:
                        CreateScene(SetPascalCase(Name));
                        break;
                    case 2:
                        return null;
                }
            } else {
                CreateScene(SetPascalCase(Name));
            }
            #else
            if (SceneManager.GetActiveScene().isDirty) {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            } else {
                CreateScene(SetPascalCase(Name));
            }
            #endif

            return null;
        }

        private void CreateScene(string name) {
            #if UNITY_5_1 || UNITY_5_2
            EditorApplication.NewEmptyScene();

            string scenePath = string.Format("{0}/{1}/{2}.unity", Application.dataPath, Data.ScenesFolder, name);

            EditorApplication.SaveScene(scenePath);
            AssetDatabase.Refresh();
            EditorApplication.OpenScene(scenePath);
            #else
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            string scenePath = string.Format("{0}/{1}/{2}.unity", Application.dataPath, Data.ScenesFolder, name);

            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(scenePath);
            #endif
        }

        private void OnSceneScriptsCreateExecute() {
            // TODO: create scene object scripts (LogicController, UiController, state)
            // TODO: modify project level/scene/state scripts to accept the three
            #if UNITY_5_1 || UNITY_5_2
            string sceneName = GetCurrentSceneName(false);
            #else
            string sceneName = SceneManager.GetActiveScene().name;
            #endif

            string projectNamespace = Data.NamespaceProject;
            string gameStateManager = Data.GameStateManagerName;

            string uiControllerName = sceneName + "UiController";
            CreateFromTemplate(Data.UiControllerFolderPath, Data.TemplateUiControllerFilePath, uiControllerName, "txt", "cs", new Dictionary<string, string> {
                { "UiControllerTemplate", uiControllerName },
                { "NamespaceTemplate", projectNamespace }
            });

            string logicControllerName = sceneName + "Controller";
            CreateFromTemplate(Data.LogicControllerFolderPath, Data.TemplateControllerFilePath, logicControllerName, "txt", "cs", new Dictionary<string, string> {
                { "UiControllerTemplate", uiControllerName },
                { "ControllerTemplate", logicControllerName },
                { "NamespaceTemplate", projectNamespace }
            });

            string stateName = sceneName + "State";
            CreateFromTemplate(Data.StateFolderPath, Data.TemplateStateFilePath, stateName, "txt", "cs", new Dictionary<string, string> {
                { "StateTemplate", stateName },
                { "UiControllerTemplate", uiControllerName },
                { "ControllerTemplate", logicControllerName },
                { "NamespaceTemplate", projectNamespace },
                { "GameStateManagerTemplate", gameStateManager }
            });

            AssetDatabase.Refresh();
        }

        private void OnSceneObjectsCreateExecute() {
            #if UNITY_5_1 || UNITY_5_2
            string sceneName = GetCurrentSceneName(false);

            GameObject sceneObject = new GameObject(sceneName);
            string uiControllerName = sceneName + "UiController";
            string logicControllerName = sceneName + "Controller";
            sceneObject.AddComponent(GetTypeInNamespace(Data.LogicControllerNamespace, logicControllerName));
            sceneObject.AddComponent(GetTypeInNamespace(Data.UiControllerNamespace, uiControllerName));
            sceneObject.transform.SetAsLastSibling();
            #else
            string sceneName = SceneManager.GetActiveScene().name;

            GameObject sceneObject = new GameObject(sceneName);
            GameObject uiObject = new GameObject("UI");
            GameObject threeDObject = new GameObject("3D");
            uiObject.transform.SetParent(sceneObject.transform, false);
            threeDObject.transform.SetParent(sceneObject.transform, false);

            string uiControllerName = sceneName + "UiController";
            string logicControllerName = sceneName + "Controller";
            string namespacePrefix = string.Format("Outfit7.{0}.", Data.NamespaceProject);
            sceneObject.AddComponent(GetTypeInNamespace(namespacePrefix + Data.LogicControllerNamespace, logicControllerName));
            uiObject.AddComponent(GetTypeInNamespace(namespacePrefix + Data.UiControllerNamespace, uiControllerName));
            #endif
        }

        #if UNITY_5_1 || UNITY_5_2
        private string GetCurrentSceneName(bool suffix) {
            string[] nameSplit = EditorApplication.currentScene.Split('/');
            string sceneName = nameSplit[nameSplit.Length - 1];

            if (!suffix) {
                nameSplit = sceneName.Split('.');
                sceneName = nameSplit[0];
            }

            return sceneName;
        }
        #endif
    }
}
