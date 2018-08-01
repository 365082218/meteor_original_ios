using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Outfit7.UI {
    public class ProjectUiMakerComponent : UiMakerComponent {

        protected string ProjectPath = string.Empty;

        protected string SelectedProjectPath = "Assets/Editor/UIMaker";

        protected static List<UiMakerData> UiMakerDataList = new List<UiMakerData>();

        public override string TypeName { get { return "Project"; } }

        public override bool IsSimpleType { get { return true; } }

        public override string NamePrefix { get { return "prj"; } }

        private Vector2 ScrollViewPosition = Vector2.zero;

        public static T GetData<T>() where T : UiMakerData {
            for (int i = 0; i < UiMakerDataList.Count; i++) {
                T data = UiMakerDataList[i] as T;
                if (data != null) {
                    return data;
                }
            }

            return null;
        }

        public override void Init() {
            UiMakerDataList.Clear();
            string path = ProjectPath;
            if (string.IsNullOrEmpty(path)) {
                path = SelectedProjectPath;
            }

            if (System.IO.Directory.Exists(path)) {
                Type[] uiMakerDataTypes = UiMakerEditorWindow.GetSubTypes(typeof(UiMakerData));

                for (int i = 0; i < uiMakerDataTypes.Length; i++) {
                    UiMakerData data = ScriptableObject.CreateInstance(uiMakerDataTypes[i]) as UiMakerData;
                    Debug.Log("Loading: " + uiMakerDataTypes[i]);
                    if (!System.IO.File.Exists(data.DataAssetPath(path))) {
                        AssetDatabase.CreateAsset(data, data.DataAssetPath(path));
                        AssetDatabase.SaveAssets();
                    } else {
                        data = AssetDatabase.LoadAssetAtPath(data.DataAssetPath(path), uiMakerDataTypes[i]) as UiMakerData;
                    }
                    UiMakerDataList.Add(data);
                    data.OnInit();
                    Debug.Log(uiMakerDataTypes[i] + " loaded!");
                }
            }
        }

        public override void OnGui() {
            EditorGUILayout.LabelField("Project folder path");
            SelectedProjectPath = EditorGUILayout.TextField(SelectedProjectPath);
            if (!SelectedProjectPath.Equals(ProjectPath)) {
                if (!System.IO.Directory.Exists(SelectedProjectPath)) {
                    if (GUILayout.Button("Create project folder")) {
                        ProjectPath = SelectedProjectPath;
                        System.IO.Directory.CreateDirectory(ProjectPath);
                        Init();
                    }
                } else {
                    if (GUILayout.Button("Select project folder")) {
                        ProjectPath = SelectedProjectPath;
                        if (ReInitAll != null) {
                            ReInitAll();
                        } else {
                            Init();
                        }
                    }
                }
            } else {
                EditorGUILayout.Separator();
                ScrollViewPosition = EditorGUILayout.BeginScrollView(ScrollViewPosition);
                for (int i = 0; i < UiMakerDataList.Count; i++) {
                    UiMakerDataList[i].OnGui();
                    EditorGUILayout.Separator();
                }
                EditorGUILayout.EndScrollView();
            }
        }

        protected override RectTransform OnCreateExecute() {
            return null;
        }

        public override void OnCompiled() {
            base.OnCompiled();

            if (System.IO.Directory.Exists(SelectedProjectPath)) {
                ProjectPath = SelectedProjectPath;
                if (ReInitAll != null) {
                    ReInitAll();
                } else {
                    Init();
                }
                UiMakerEditorWindow.Instance.Repaint();
            }
        }
    }
}
