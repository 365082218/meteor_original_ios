using UnityEngine.EventSystems;
using UnityEngine;

namespace Outfit7.UI {
    [AddComponentMenu("Event/Touch Event System")]
    public class TouchEventSystem : EventSystem {

        private static TouchEventSystem CurrentTouchEventSystem;

        protected override void Awake() {
            base.Awake();

            CurrentTouchEventSystem = this;
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            CurrentTouchEventSystem = null;
        }

        public static TouchEventSystem Current { 
            get { return EventSystem.current != null ? EventSystem.current as TouchEventSystem : CurrentTouchEventSystem; } 
            set { EventSystem.current = value; }
        }

        public static void DeactivateModule() {
            CurrentTouchEventSystem.enabled = false;
        }

        public static void ActivateModule() {
            CurrentTouchEventSystem.enabled = true;
        }

        public static void ClearPointerData() {
            CurrentTouchEventSystem.enabled = false;
            CurrentTouchEventSystem.enabled = true;
            Current = CurrentTouchEventSystem;
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            sendNavigationEvents = false;
            #if UNITY_5_1_4 || UNITY_5_2_1
            TouchInputModule touchInputModule = GetComponent<TouchInputModule>();
            if (touchInputModule == null)
                touchInputModule = gameObject.AddComponent<TouchInputModule>();
            #else
            StandaloneInputModule touchInputModule = GetComponent<StandaloneInputModule>();
            if (touchInputModule == null)
                touchInputModule = gameObject.AddComponent<StandaloneInputModule>();
            #endif

            #if UNITY_5_1_4
            touchInputModule.allowActivationOnStandalone = true;
            #else
            touchInputModule.forceModuleActive = true;
            #endif
        }
#endif

    }
}

