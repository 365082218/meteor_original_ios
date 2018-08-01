using UnityEditor;
using UnityEngine;

namespace Outfit7.Logic.Input {

    public class InputTagSetter : MonoBehaviour {

        [MenuItem("GameObject/Change Tag Recursive/INPUT_PASSIVE", false, -1)]
        static void SetInputPassive() {
            for (int a = 0; a < Selection.gameObjects.Length; a++) {
                SetTagRecursive(Selection.gameObjects[a], TouchManager.InputPassiveTag);
            }
        }

        [MenuItem("GameObject/Change Tag Recursive/INPUT_ACTIVE", false, -1)]
        static void SetInputActive() {
            for (int a = 0; a < Selection.gameObjects.Length; a++) {
                SetTagRecursive(Selection.gameObjects[a], TouchManager.InputActiveTag);
            }
        }

        static void SetTagRecursive(GameObject parent, string tag) {
            parent.tag = tag;
            for (int a = 0; a < parent.transform.childCount; a++) {
                GameObject child = parent.transform.GetChild(a).gameObject;
                child.tag = tag;
                SetTagRecursive(child, tag);
            }
        }
    }
}
