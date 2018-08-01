using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Outfit7.Logic.SceneStateMachineInternal {
    public class InjectSceneLoadBehaviour {

        [PostProcessScene]
        public static void OnPostProcessScene() {
            var sceneLoadedBehaviours = Resources.FindObjectsOfTypeAll(typeof(SceneLoadedBehaviour)) as SceneLoadedBehaviour[];
            SceneLoadedBehaviour sceneLoadedBehaviour = null;
            if (Application.isPlaying) {
                // temp thing just to hack exec order
                if (sceneLoadedBehaviours.Length == 0) {
                    var go = new GameObject("LoadCallback");
                    sceneLoadedBehaviour = go.AddComponent<SceneLoadedBehaviour>();
                    go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                }
            } else {
                if (sceneLoadedBehaviours.Length == 0) {
                    var go = new GameObject("LoadCallback");
                    sceneLoadedBehaviour = go.AddComponent<SceneLoadedBehaviour>();
                    go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                } else {
                    sceneLoadedBehaviour = sceneLoadedBehaviours[0];
                }
            }

            if (sceneLoadedBehaviour != null) {
                var monoScript = MonoScript.FromMonoBehaviour(sceneLoadedBehaviour);
                if (MonoImporter.GetExecutionOrder(monoScript) != -10000) {
                    MonoImporter.SetExecutionOrder(monoScript, -10000);
                }
            }
        }
    }
}
