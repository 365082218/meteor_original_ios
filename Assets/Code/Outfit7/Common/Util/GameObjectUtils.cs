//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using UnityEngine;

namespace Outfit7.Util {

    /// <summary>
    /// Unity's GameObject utilities.
    /// </summary>
    public static class GameObjectUtils {

        public static GameObject CreateEmpty(string name, GameObject parent) {
            GameObject go = new GameObject();
            go.name = name;
            if (parent != null) {
                go.transform.parent = parent.transform;
            }
            return go;
        }

        public static GameObject CreatePrimitive(string name, GameObject parent, PrimitiveType type) {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null) {
                go.transform.parent = parent.transform;
            }
            return go;
        }

        public static GameObject CreateFromResource(string resourcePath, string name, GameObject parent) {
            GameObject prefab = Resources.Load(resourcePath) as GameObject;
            Assert.NotNull(prefab, "No resource exists at the specified path: {0}", resourcePath);
            // Need to clone read-only prefab in order to be able to change it
            GameObject go = Clone(prefab, name);
            if (parent != null) {
                go.transform.parent = parent.transform;
            }
            return go;
        }

        public static GameObject Clone(GameObject original, string name) {
            GameObject go = GameObject.Instantiate(original) as GameObject;
            go.name = name;
            return go;
        }

        public static GameObject Clone(GameObject original, string name, Vector3 position, Quaternion quaternion) {
            GameObject go = GameObject.Instantiate(original, position, quaternion) as GameObject;
            go.name = name;
            return go;
        }
    }
}

