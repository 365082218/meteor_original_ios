using Outfit7.Logic;
using UnityEngine;

namespace Outfit7.Logic.SceneStateMachineInternal {
    public class SceneLoadedBehaviour : MonoBehaviour {
        public string ScenePath = string.Empty;

        private void Awake() {
            if (SceneStateManager.Instance != null) {
                SceneStateManager.Instance.RegisterSceneLoaded(string.IsNullOrEmpty(ScenePath) ? gameObject.scene.path : ScenePath);
            }
        }
    }
}
