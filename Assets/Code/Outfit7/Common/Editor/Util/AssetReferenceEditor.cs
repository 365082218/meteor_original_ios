
using System;
using UnityEditor;
using UnityEngine;

namespace Outfit7.Util {

    [CustomPropertyDrawer(typeof(AssetReference), true)]
    [CustomPropertyDrawer(typeof(AssetReferenceDescriptionAttribute), true)]
    public sealed class AssetReferenceDrawer : PropertyDrawer {

        public SerializedProperty PrefabPath;
        public SerializedProperty EditorAssetGUID;
        public UnityEngine.Object CachedObject;
        public Type Type;

        private void Init(SerializedProperty property) {
            AssetReferenceDescriptionAttribute descAtt = attribute as AssetReferenceDescriptionAttribute;
            if (descAtt != null) {
                Type = descAtt.Type;
            } else {
                Type = typeof(UnityEngine.Object);
            }
            PrefabPath = property.FindPropertyRelative("PrefabPath");
            EditorAssetGUID = property.FindPropertyRelative("EditorAssetGUID");
            AssetReferenceEditor.Refresh(this, Type);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Init(property);
            EditorGUI.BeginProperty(position, label, property);

            var data = EditorGUI.ObjectField(position, label, CachedObject, Type, false);
            if (data != CachedObject) {
                CachedObject = data;
                if (data != null) {
                    string path = AssetDatabase.GetAssetPath(data);
                    EditorAssetGUID.stringValue = AssetDatabase.AssetPathToGUID(path);
                    PrefabPath.stringValue = AssetReference.GetPrefabPath(path);
                    if (PrefabPath.stringValue.Contains("Assets/")) {
                        AssetReferenceEditor.Reset(this);
                        EditorUtility.DisplayDialog("Error", string.Format("{0} is not a valid path!", path), "Ok");
                    }
                } else {
                    AssetReferenceEditor.Reset(this);
                }
            }

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            Init(property);
            return base.GetPropertyHeight(property, label);
        }

        public void SetCachedObject(UnityEngine.Object obj) {
            CachedObject = obj;
        }
    }

    public static class AssetReferenceEditor {

        private const string Resources = "/Resources/";
        private const string StreamingAssets = "/StreamingAssets/";

        public static void Reset(AssetReference assetReference) {
            ResetInternal(x => assetReference.PrefabPath = x, x => assetReference.EditorObject = x, x => assetReference.EditorAssetGUID = x);
        }

        public static void Reset(AssetReferenceDrawer assetReferenceDrawer) {
            ResetInternal(x => assetReferenceDrawer.PrefabPath.stringValue = x, assetReferenceDrawer.SetCachedObject, x => assetReferenceDrawer.EditorAssetGUID.stringValue = x);
        }

        private static void ResetInternal(Action<string> prefabPathSetter, Action<UnityEngine.Object> editorObjectSetter, Action<string> editorAssetGUIDSetter) {
            prefabPathSetter(string.Empty);
            editorAssetGUIDSetter(string.Empty);
            editorObjectSetter(null);
        }

        public static bool SetFromObject(UnityEngine.Object obj, AssetReference assetReference, Type type) {
            string path = AssetDatabase.GetAssetPath(obj);
            assetReference.EditorAssetGUID = AssetDatabase.AssetPathToGUID(path);
            assetReference.PrefabPath = AssetReference.GetPrefabPath(path);
            Refresh(assetReference, type);
            return assetReference.EditorObject != null;
        }

        public static bool Field(string name, AssetReference assetReference, Type type) {
            return FieldInternal(null, name, assetReference, type);
        }

        public static bool Field(Rect rect, string name, AssetReference assetReference, Type type) {
            return FieldInternal(rect, name, assetReference, type);
        }

        public static void Refresh(AssetReference assetReference, Type type) {
            if (assetReference == null) {
                return;
            }
            RefreshInternal(x => assetReference.PrefabPath = x, x => assetReference.EditorObject = x, assetReference.EditorAssetGUID, type);
        }

        public static void Refresh(AssetReferenceDrawer assetReferenceDrawer, Type type) {
            if (assetReferenceDrawer == null) {
                return;
            }
            RefreshInternal(x => assetReferenceDrawer.PrefabPath.stringValue = x, assetReferenceDrawer.SetCachedObject, assetReferenceDrawer.EditorAssetGUID.stringValue, type);
        }

        private static void RefreshInternal(Action<string> prefabPathSetter, Action<UnityEngine.Object> editorObjectSetter, string editorAssetGUID, Type type) {
            if (string.IsNullOrEmpty(editorAssetGUID)) {
                prefabPathSetter(string.Empty);
                editorObjectSetter(null);
            } else {
                editorObjectSetter(AssetReference.GetObjectFromGUID(editorAssetGUID, type));
            }
        }

        private static bool FieldInternal(Rect? rect, string name, AssetReference assetReference, Type type) {
            if (assetReference == null) {
                return false;
            }
            // Refresh reference if needed
            if (assetReference.EditorObject == null && !string.IsNullOrEmpty(assetReference.EditorAssetGUID)) {
                Refresh(assetReference, type);
            }
            // Update reference
            UnityEngine.Object data;
            if (!string.IsNullOrEmpty(name)) {
                if (rect != null) {
                    data = EditorGUI.ObjectField(rect.Value, name, assetReference.EditorObject, type, false);
                } else {
                    data = EditorGUILayout.ObjectField(name, assetReference.EditorObject, type, false);
                }
            } else {
                if (rect != null) {
                    data = EditorGUI.ObjectField(rect.Value, assetReference.EditorObject, type, false);
                } else {
                    data = EditorGUILayout.ObjectField(assetReference.EditorObject, type, false);
                }
            }
            if (data != assetReference.EditorObject) {
                if (data != null) {
                    assetReference.EditorObject = data;
                    string path = AssetDatabase.GetAssetPath(data);
                    assetReference.EditorAssetGUID = AssetDatabase.AssetPathToGUID(path);
                    assetReference.PrefabPath = AssetReference.GetPrefabPath(path);
                    if (assetReference.PrefabPath.Contains("Assets/")) {
                        Reset(assetReference);
                        EditorUtility.DisplayDialog("Error", string.Format("{0} is not a valid path!", path), "Ok");
                    }
                } else {
                    Reset(assetReference);
                }
                return true;
            }
            return false;
        }

    }
}
