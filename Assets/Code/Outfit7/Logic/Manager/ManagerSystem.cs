using System;
using System.Collections;
using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Logic {

    [Serializable, ExecuteInEditMode]
    [RequireComponent(typeof(BucketUpdateSystem))]
    public class ManagerSystem : MonoBehaviour {

        public enum ExecuteOrderType {
            Create,
            Initialize,
            Terminate,
            PreUpdate,
            BucketUpdate,
            PostUpdate,
            Pause,
            Resume,
            Last,
        };

        [Flags]
        public enum ManagerEntryFlags {
            Enabled = 1,
            EnabledInEditor = 2,
            Visible = 4,
        };

        [Serializable]
        public class ManagerEntry {
            public MonoBehaviour ManagerBehaviour;
            public ManagerEntryFlags EntryFlags;

            [NonSerialized] public IManager Manager;

            public ManagerEntry(MonoBehaviour behaviour) {
                ManagerBehaviour = behaviour;
                Manager = behaviour as IManager;
                EntryFlags = ManagerEntryFlags.Enabled;
            }

            public bool IsEnabled {
                get {
#if UNITY_EDITOR
                    if (!UnityEngine.Application.isPlaying && (EntryFlags & ManagerEntryFlags.EnabledInEditor) == 0)
                        return false;
#endif
                    return (EntryFlags & ManagerEntryFlags.Enabled) != 0;
                }
            }
        }

        [Serializable]
        public class ExecuteOrderEntry {
            public int InstanceIndex;
            public bool IsEnabled;

            public ExecuteOrderEntry(int index) {
                InstanceIndex = index;
                IsEnabled = true;
            }
        }

        [Serializable]
        public class ExecuteOrder {
            public ExecuteOrderEntry[] Entries = new ExecuteOrderEntry[0];
        }

        // Delegates definitions
        private delegate void ExecuteOrderDelegate(IManager manager);

        // Private properties
        private ExecuteOrderDelegate[] ExecuteOrderDelegates = null;
        // Internal update properties
        private float DeltaTime = 0.0f;
        private int BucketIndex = 0;
#if UNITY_EDITOR
        private float LastEditorTime = -1.0f;
#endif

        // Internal (DON'T USE!)
        public ExecuteOrder[] ExecuteOrders = new ExecuteOrder[(int) ExecuteOrderType.Last];
        public ManagerEntry[] ManagerEntries = new ManagerEntry[0];

        // Public properties
        public BucketUpdateSystem BucketUpdateSystem;

        public bool IsInitialized { get; private set; }

        public ExecuteOrderType ActiveExecuteOrderType { get; private set; }

        public ExecuteOrderType LastExecuteOrderType { get; private set; }

        // Private methods
        private void SetOrderDelegate(ExecuteOrderType orderType, ExecuteOrderDelegate updateOrderDelegate) {
            if (ExecuteOrderDelegates == null) {
                ExecuteOrderDelegates = new ExecuteOrderDelegate[(int) ExecuteOrderType.Last];
            }
            ExecuteOrderDelegates[(int) orderType] = updateOrderDelegate;
        }

        private void SetActiveExecuteOrderType(ExecuteOrderType orderType) {
            ActiveExecuteOrderType = orderType;
            if (orderType != ExecuteOrderType.Last) {
                LastExecuteOrderType = orderType;
            }
        }

        private void ExecuteUpdateOrder(ExecuteOrderType orderType, bool setActive = true) {
            if (setActive) {
                SetActiveExecuteOrderType(orderType);
            }
            ExecuteOrder order = ExecuteOrders[(int) orderType];
            ExecuteOrderDelegate updateOrderDelegate = ExecuteOrderDelegates[(int) orderType];
            // Update!
            for (int i = 0; i < order.Entries.Length; ++i) {
                ExecuteOrderEntry entry = order.Entries[i];
                if (!entry.IsEnabled) {
                    continue;
                }
                ManagerEntry instance = ManagerEntries[entry.InstanceIndex];
                if (!instance.IsEnabled) {
                    continue;
                }
                updateOrderDelegate(instance.Manager);
            }
            if (setActive) {
                SetActiveExecuteOrderType(ExecuteOrderType.Last);
            }
        }

#if UNITY_EDITOR
        private void EditorUpdate() {
            if (LastEditorTime < 0) {
                LastEditorTime = (float) UnityEditor.EditorApplication.timeSinceStartup;
            }
            float newTime = (float) UnityEditor.EditorApplication.timeSinceStartup;
            // Set internal
            DeltaTime = newTime - LastEditorTime;
            LastEditorTime = newTime;
            // PreUpdate
            ExecuteUpdateOrder(ExecuteOrderType.PreUpdate);
            ExecuteUpdateOrder(ExecuteOrderType.PostUpdate);

        }
#endif

        private void ValidateArrays() {
            if (ExecuteOrderDelegates == null || ExecuteOrderDelegates.Length != (int) ExecuteOrderType.Last) {
                ExecuteOrderDelegates = new ExecuteOrderDelegate[(int) ExecuteOrderType.Last];
            }
            // Check validity
            for (int i = 0; i < ExecuteOrders.Length; ++i) {
                if (ExecuteOrders[i] == null) {
                    ExecuteOrders[i] = new ExecuteOrder();
                }
            }
        }

        // MonoBehaviour
        private void OnEnable() {
            // Don't destroy
            if (Application.isPlaying) {
                DontDestroyOnLoad(gameObject);
            }
            // Set Log
            O7Log.SetMainThread();
            // Initialize
            ValidateArrays();
            BucketUpdateSystem.Initialize();
            // Initialize managers
            for (int i = 0; i < ManagerEntries.Length; ++i) {
                ManagerEntry managerEntry = ManagerEntries[i];
                managerEntry.Manager = managerEntry.ManagerBehaviour as IManager;
                managerEntry.Manager.SetInstance(managerEntry.ManagerBehaviour);
            }
            // Initialize delegates
            SetOrderDelegate(ExecuteOrderType.Create, (IManager Manager) => {
                Manager.OnCreate();
            });
            SetOrderDelegate(ExecuteOrderType.Initialize, (IManager Manager) => {
                Manager.OnInitialize();
            });
            SetOrderDelegate(ExecuteOrderType.Terminate, (IManager Manager) => {
                Manager.OnTerminate();
            });
            SetOrderDelegate(ExecuteOrderType.PreUpdate, (IManager Manager) => {
                Manager.OnPreUpdate(DeltaTime);
            });
            SetOrderDelegate(ExecuteOrderType.BucketUpdate, (IManager Manager) => {
                Manager.OnBucketUpdate(BucketIndex, DeltaTime);
            });
            SetOrderDelegate(ExecuteOrderType.PostUpdate, (IManager Manager) => {
                Manager.OnPostUpdate(DeltaTime);
            });
            SetOrderDelegate(ExecuteOrderType.Pause, (IManager Manager) => {
                Manager.OnPause();
            });
            SetOrderDelegate(ExecuteOrderType.Resume, (IManager Manager) => {
                Manager.OnResume();
            });
            // Create
            ExecuteUpdateOrder(ExecuteOrderType.Create);
        }

        private void Start() {
            // Initialize
            ExecuteUpdateOrder(ExecuteOrderType.Initialize);
            IsInitialized = true;
#if UNITY_EDITOR
            LastEditorTime = -1.0f;
            // Register update callback
            if (UnityEditor.EditorApplication.isPlaying == false) {
                UnityEditor.EditorApplication.update -= EditorUpdate;
                UnityEditor.EditorApplication.update += EditorUpdate;
            }
#endif
        }

        private void OnDisable() {
#if UNITY_EDITOR
            // Register update callback
            if (UnityEditor.EditorApplication.isPlaying == false) {
                UnityEditor.EditorApplication.update -= EditorUpdate;
            }
#endif
            // Terminate
            ExecuteUpdateOrder(ExecuteOrderType.Terminate);
            IsInitialized = false;
        }

        private void Update() {
            DeltaTime = Time.deltaTime;
            // PreUpdate
            ExecuteUpdateOrder(ExecuteOrderType.PreUpdate, false);
            SetActiveExecuteOrderType(ExecuteOrderType.PreUpdate);
            // BucketUpdate
            if (BucketUpdateSystem != null) {
                BucketUpdateSystem.PreUpdateSystem(DeltaTime);
            }
            SetActiveExecuteOrderType(ExecuteOrderType.Last);
        }

        private void LateUpdate() {
            SetActiveExecuteOrderType(ExecuteOrderType.PostUpdate);
            // PostUpdate
            ExecuteUpdateOrder(ExecuteOrderType.PostUpdate, false);
            // BucketUpdate
            if (BucketUpdateSystem != null) {
                BucketUpdateSystem.PostUpdateSystem(DeltaTime);
            }
            SetActiveExecuteOrderType(ExecuteOrderType.Last);

        }

        private void OnApplicationPause(bool paused) {
            if (!IsInitialized) {
                return;
            }
            // pause/resume
            if (BucketUpdateSystem.Instance != null) {
                if (paused) {
                    SetActiveExecuteOrderType(ExecuteOrderType.Pause);
                    ExecuteUpdateOrder(ExecuteOrderType.Pause, false);
                    BucketUpdateSystem.Instance.OnPauseSystem(DeltaTime);
                    SetActiveExecuteOrderType(ExecuteOrderType.Last);
                } else {
                    SetActiveExecuteOrderType(ExecuteOrderType.Resume);
                    ExecuteUpdateOrder(ExecuteOrderType.Resume, false);
                    BucketUpdateSystem.Instance.OnResumeSystem(DeltaTime);
                    SetActiveExecuteOrderType(ExecuteOrderType.Last);
                }
            }
        }

        // Public
        ManagerSystem() {
            ActiveExecuteOrderType = ExecuteOrderType.Last;
            LastExecuteOrderType = ExecuteOrderType.Last;
        }

        public void BucketUpdate(int index) {
            ExecuteUpdateOrder(ExecuteOrderType.BucketUpdate, false);
        }
    }

}