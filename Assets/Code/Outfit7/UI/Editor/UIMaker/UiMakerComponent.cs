using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Outfit7.UI {
    public abstract class UiMakerComponent {

        public abstract string TypeName { get; }

        public abstract string NamePrefix { get; }

        public abstract bool IsSimpleType { get; }

        public abstract void Init();

        public Action ReInitAll;

        protected static bool IsStaticResource = true;

        protected string Name = string.Empty;

        protected string SetFirstCharacterToLowerCase(string str) {
            return char.ToLower(str[0]) + str.Substring(1);
        }

        protected string SetFirstCharacterToUpperCase(string str) {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        protected string SetCamelBack(string name) {
            string[] pieces = name.Split(' ');
            name = SetFirstCharacterToLowerCase(pieces[0]);
            for (int i = 1; i < pieces.Length; i++) {
                name += SetFirstCharacterToUpperCase(pieces[i]);
            }

            return name;
        }

        protected string SetPascalCase(string name) {
            string[] pieces = name.Split(' ');
            name = string.Empty;
            for (int i = 0; i < pieces.Length; i++) {
                name += SetFirstCharacterToUpperCase(pieces[i]);
            }

            return name;
        }

        protected string RemoveSuffix(string str, string suffix) {
            if (str.EndsWith(suffix)) {
                str = str.Remove(str.Length - suffix.Length);
            }
            return str;
        }

        protected virtual RectTransform OnCreateExecute() {
            if (string.IsNullOrEmpty(Name)) {
                throw new UnityException("Name is not set!");
            }

            string name = SetCamelBack(Name);

            if (UiMakerEditorWindow.ShouldClose) {
                UiMakerEditorWindow.CloseWindow();
            }

            return CreateUIElementRoot(string.Format("{0}_{1}", NamePrefix, name));
        }

        protected virtual void SetDefaults() {
            
        }

        public virtual void OnGui() {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", EditorStyles.label, GUILayout.Width(40f));
            Name = EditorGUILayout.TextField(Name);
            GUILayout.EndHorizontal();
        }

        protected void SetCommonCreatePanel() {
            SetCommonCreatePanel(string.Empty);
        }

        protected void SetCommonCreatePanel(string createName) {
            if (Selection.activeTransform == null) {
                EditorGUILayout.HelpBox("You have to select a parent object from the hierachy window", MessageType.Error);
            } else if (Selection.activeTransform.GetComponentInParent<Canvas>() == null) {
                EditorGUILayout.HelpBox("The object you have selected is not on a Canvas", MessageType.Error);
            } else {
                if (createName == string.Empty) {
                    createName = "Create";
                }
                if (GUILayout.Button(createName, GUILayout.Height(33f))) {
                    OnCreateExecute();
                }
                // TODO: smart implementation
//                IsStaticResource = EditorGUILayout.Toggle("Is static resource", IsStaticResource);
//                if (GUILayout.Button("Create and save prefab", GUILayout.Height(33f))) {
//                    
//                }
            }
        }

        protected virtual void SetToStretch(RectTransform rectTransform) {
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private RectTransform CreateUIElementRoot(string name) {
            GameObject parent = Selection.activeGameObject;
            GameObject child = new GameObject(name);
        
            Undo.RegisterCreatedObjectUndo(child, "Create " + name);
            Undo.SetTransformParent(child.transform, parent == null ? null : parent.transform, "Parent " + child.name);
            GameObjectUtility.SetParentAndAlign(child, parent);
        
            RectTransform rectTransform = child.AddComponent<RectTransform>();

//            SetPositionVisibleinSceneView(rectTransform);
            Selection.activeGameObject = child;
            return rectTransform;
        }

        //        private static void SetPositionVisibleinSceneView(RectTransform itemTransform) {
        //            // Find the best scene view
        //            SceneView sceneView = SceneView.lastActiveSceneView;
        //            if (sceneView == null && SceneView.sceneViews.Count > 0)
        //                sceneView = SceneView.sceneViews[0] as SceneView;
        //
        //            // Couldn't find a SceneView. Don't set position.
        //            if (sceneView == null || sceneView.camera == null)
        //                return;
        //
        //            Canvas parentCanvas = itemTransform.GetComponentInParent<Canvas>();
        //
        //            RectTransform canvasRTransform = parentCanvas == null ? null : parentCanvas.transform as RectTransform;
        //
        //            // Create world space Plane from canvas position.
        //            Vector2 localPlanePosition;
        //            Camera camera = sceneView.camera;
        //            Vector3 position = Vector3.zero;
        //            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition)) {
        //                // Adjust for canvas pivot
        //                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
        //                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;
        //
        //                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
        //                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);
        //
        //                // Adjust for anchoring
        //                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
        //                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;
        //
        //                Vector3 minLocalPosition;
        //                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
        //                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;
        //
        //                Vector3 maxLocalPosition;
        //                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
        //                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;
        //
        //                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
        //                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
        //            }
        //
        //            itemTransform.anchoredPosition = position;
        //            itemTransform.localRotation = Quaternion.identity;
        //            itemTransform.localScale = Vector3.one;
        //        }

        protected static void CreateCodeFile(string folderPath, string name, string suffix, string code) {
            string newFilePath = string.Format("{0}/{1}/{2}.{3}", Application.dataPath, folderPath, name, suffix);
            System.IO.File.WriteAllText(newFilePath, code);
        }

        protected static string ReadTemplateFile(string templateFilePath) {
            return System.IO.File.ReadAllText(string.Format("{0}/{1}", Application.dataPath, templateFilePath));
        }

        protected static void CreateFromTemplate(string folderPath, string templateFilePath, string scriptName, string readSuffix, string writeSuffix, Dictionary<string, string> replaceDictionary) {
            string newDataPath = string.Format("{0}/{1}/{2}.{3}", Application.dataPath, folderPath, scriptName, readSuffix);
            string templateFilePathWithSuffix = string.Format("{0}.{1}", templateFilePath, readSuffix);
            if (!System.IO.File.Exists(newDataPath)) {
                string code = ReadTemplateFile(templateFilePathWithSuffix);
                foreach (var item in replaceDictionary) {
                    code = code.Replace(item.Key, item.Value);
                }
                CreateCodeFile(folderPath, scriptName, writeSuffix, code);
            } else {
                AssetDatabase.Refresh();
                if (EditorUtility.DisplayDialog("Are you sure?", 
                        "The " + scriptName + " file already exists. Do you want to overwrite it?", 
                        "Yes", 
                        "No")) {
                    string code = ReadTemplateFile(templateFilePathWithSuffix);
                    foreach (var item in replaceDictionary) {
                        code = code.Replace(item.Key, item.Value);
                    }
                    CreateCodeFile(folderPath, scriptName, writeSuffix, code);
                }
            }
        }

        protected static Type GetTypeInNamespace(string namespaceString, string typeName) {
            Type type = null;
            string typeString = string.Format("{0}.{1}", namespaceString, typeName);
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly assembly in assemblies) {
                type = assembly.GetType(typeString, false);
                if (type != null) {
                    break;
                }
            }

            if (typeName == null) {
                throw new UnityException("Couldn't find " + typeString + " type!");
            }

            return type;
        }

        public static T[] GetAssetsWithFilter<T>(string filter) where T : UnityEngine.Object {
            string[] assetguids = AssetDatabase.FindAssets(filter);
            List<T> list = new List<T>();
            for (int i = 0; i < assetguids.Length; i++) {
                T asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetguids[i]), typeof(T)) as T;
                if (asset != null) {
                    list.Add(asset);
                }
            }

            return list.ToArray();
        }

        protected static GameObject CreatePrefab(string prefabName, string prefabLoadAssetAtPath, string prefabCreatePath, GameObject gameObject, Action<GameObject> onPreOverwrite, Action<GameObject> onPostReplacePrefab) {
            GameObject obj = null;
            GameObject prefabGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabLoadAssetAtPath);
            if (prefabGameObject != null) {
                if (EditorUtility.DisplayDialog("Are you sure?", 
                        "The " + prefabName + "prefab already exists. Do you want to overwrite it?", 
                        "Yes", 
                        "No")) {
                    if (onPreOverwrite != null) {
                        // prefabGameObject == if you want to do something on the previous gameObject
                        onPreOverwrite(prefabGameObject);
                    }
                    UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(prefabCreatePath);
                    obj = PrefabUtility.ReplacePrefab(gameObject, prefab, ReplacePrefabOptions.Default);
                    if (onPostReplacePrefab != null) {
                        // obj == the new prefab
                        onPostReplacePrefab(obj);
                    }
                }
            } else {
                UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(prefabCreatePath);
                obj = PrefabUtility.ReplacePrefab(gameObject, prefab, ReplacePrefabOptions.Default);
                if (onPostReplacePrefab != null) {
                    // obj == the new prefab
                    onPostReplacePrefab(obj);
                }
            }

            return obj;
        }

        protected static Canvas GetTopMostCanvasObject() {
            Canvas[] canvases = Selection.activeTransform.GetComponentsInParent<Canvas>();
            for (int i = 0; i < canvases.Length; i++) {
                if (canvases[i].isRootCanvas)
                    return canvases[i];
            }

            return null;
        }

        public virtual void OnSelectionChanged() {
        }

        public virtual void OnCompiled() {
        }
    }
}