#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace Outfit7.Util {

    public partial class AssetReference {
        private const string Resources = "/Resources/";
        private const string StreamingAssets = "/StreamingAssets/";

        [NonSerialized]
        private UnityEngine.Object EditorObjectInternal = null;
        [NonSerialized]
        private string EditorAssetGUIDCached;

        public UnityEngine.Object EditorObject {
            get {
                if (EditorAssetGUIDCached != EditorAssetGUID) {
                    EditorAssetGUIDCached = EditorAssetGUID;
                    return (EditorObjectInternal = GetObjectFromGUID(EditorAssetGUID, null));
                }

                return EditorObjectInternal;
            }
            set {
                EditorObjectInternal = value;
            }
        }

        public static string GetPrefabPath(string path) {
            // Remove
            int index = path.LastIndexOf(Resources);
            if (index != -1) {
                path = path.Remove(0, index + Resources.Length);
            }
            index = path.LastIndexOf(StreamingAssets);
            if (index != -1) {
                path = path.Remove(0, index + StreamingAssets.Length);
            }
            index = path.LastIndexOf('.');
            if (index > 0) {
                path = path.Remove(index);
            }
            return path;
        }

        public static UnityEngine.Object GetObjectFromGUID(string editorAssetGUID, Type type) {
            if (string.IsNullOrEmpty(editorAssetGUID)) {
                return null;
            } else {
                string path = AssetDatabase.GUIDToAssetPath(editorAssetGUID);
                if (type != null && (type.IsSubclassOf(typeof(MonoBehaviour)) || type == typeof(MonoBehaviour))) {
                    GameObject gameObject = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                    return gameObject.GetComponent(type);
                } else {
                    return AssetDatabase.LoadMainAssetAtPath(path);
                }
            }
        }
    }
}
#endif
