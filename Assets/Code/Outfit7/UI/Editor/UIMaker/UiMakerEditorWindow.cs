using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

namespace Outfit7.UI {
    public class UiMakerEditorWindow : EditorWindow {

        //        private static GUIContent[] SelectionContent = {
        //            new GUIContent("Existing prefabs", "Existing prefabs"),
        //            new GUIContent("New GameObject", "New GameObject"),
        //        };

        public static bool Compiling = true;

        public static bool Shown = false;

        public static bool ShouldClose = false;

        public static UiMakerEditorWindow Instance;

        private int GameObjectTypeIdx = 0;

        private Material Material;

        private List<UiMakerComponent> UiMakerComponents = new List<UiMakerComponent>();


        [MenuItem("Outfit7/UI/UI Maker", false, 0)]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow<UiMakerEditorWindow>("UI Maker");
            Shown = true;
        }

        public static void ShowUIMaker<T>() where T : UiMakerComponent {
            ShouldClose = !Shown;
            ShowWindow();
            Instance.SelectUiMakerComponent<T>();
        }

        public static void CloseWindow() {
            Shown = false;
            ShouldClose = false;
            Instance.Close();
        }

        public static T GetComponent<T>() where T : UiMakerComponent {
            return Instance.GetUiMakerComponent<T>();
        }

        public static Type[] GetSubTypes(Type aBaseClass) {
            List<Type> result = new List<Type>();
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly assembly in assemblies) {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types) {
                    if (type.IsSubclassOf(aBaseClass)) {
                        result.Add(type);
                    }
                }
            }
            return result.ToArray();
        }

        private static Type[] GetUiMakerSubTypes(Type aBaseClass) {
            List<Type> result = new List<Type>();
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            // first image and text
            result.Add(typeof(ProjectUiMakerComponent));
            result.Add(typeof(ImageUiMakerComponent));
            result.Add(typeof(TextUiMakerComponent));
            result.Add(typeof(PanelUiMakerComponent));
            foreach (System.Reflection.Assembly assembly in assemblies) {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types) {
                    if (type.IsSubclassOf(aBaseClass) &&
                        type != typeof(ProjectUiMakerComponent) &&
                        type != typeof(ImageUiMakerComponent) &&
                        type != typeof(TextUiMakerComponent) &&
                        type != typeof(PanelUiMakerComponent)) {
                        result.Add(type);
                    }
                }
            }
            return result.ToArray();
        }

        public void SelectUiMakerComponent<T>() where T : UiMakerComponent {
            for (int i = 0; i < UiMakerComponents.Count; i++) {
                if (UiMakerComponents[i] is T) {
                    GameObjectTypeIdx = i;
                }
            }
        }

        public T GetUiMakerComponent<T>() where T : UiMakerComponent {
            for (int i = 0; i < UiMakerComponents.Count; i++) {
                T uiMakerComponent = UiMakerComponents[i] as T;
                if (uiMakerComponent != null) {
                    return uiMakerComponent;
                }
            }

            return null;
        }

        private void OnSelectionChange() {
            UiMakerComponents[GameObjectTypeIdx].OnSelectionChanged();
            Repaint();
        }

        private void OnEnable() {
            Instance = this;

            Type[] uiMakerTypes = GetUiMakerSubTypes(typeof(UiMakerComponent));

            for (int i = 0; i < uiMakerTypes.Length; i++) {
                UiMakerComponent component = Activator.CreateInstance(uiMakerTypes[i]) as UiMakerComponent;
                UiMakerComponents.Add(component);
                component.Init();
                component.ReInitAll += ReInitAll;
            }
        }

        private void ReInitAll() {
            for (int i = 0; i < UiMakerComponents.Count; i++) {
                UiMakerComponents[i].Init();
            }
        }

        private void OnGUI() {
            List<string> components = new List<string>(UiMakerComponents.Count);
            for (int i = 0; i < UiMakerComponents.Count; i++) {
                components.Add(UiMakerComponents[i].TypeName);
            }
            GameObjectTypeIdx = GUILayout.SelectionGrid(GameObjectTypeIdx, components.ToArray(), 5, EditorStyles.miniButton, GUILayout.MinHeight(80f));

            EditorGUILayout.Space();

            if (GameObjectTypeIdx < UiMakerComponents.Count) {
                UiMakerComponents[GameObjectTypeIdx].OnGui();
            }
        }

        private void Update() {
            if (Compiling != EditorApplication.isCompiling) {
                Compiling = EditorApplication.isCompiling;
                Repaint();
                for (int i = 0; i < UiMakerComponents.Count; i++) {
                    UiMakerComponents[i].OnCompiled();
                }
            }
        }
    }
}
