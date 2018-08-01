#if !STARLITE_EDITOR

using Outfit7.Logic;

namespace Starlite {

    public class SceneObjectComponent : BucketUpdateBehaviour {
        protected bool RegisterToManager;

        protected virtual bool ShouldRegisterToManager() {
            return false;
        }

        protected override void Awake() {
            RegisterToManager = ShouldRegisterToManager();
            if (RegisterToManager) {
                base.Awake();
            }
        }

        protected override void OnEnable() {
            if (RegisterToManager) {
                base.OnEnable();
            }
        }

        protected override void OnDisable() {
            if (RegisterToManager) {
                base.OnDisable();
            }
        }
    }
}

#endif