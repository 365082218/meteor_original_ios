using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;

namespace Outfit7.Logic {

    [CustomEditor(typeof(ManagerSystem))]
    public class ManagerSystemEditor : UnityEditor.Editor {
        private ManagerSystem ManagerSystem;
        private MonoScript LinkedMonoScript;
        private string LinkStatus = string.Empty;
        private GUIStyle LinkStatusStyle = null;
        private GUIStyle ManagerLabelStyle = null;
        private ManagerSystem.ExecuteOrderType ManagerOrderType = ManagerSystem.ExecuteOrderType.Create;
        private Vector2 RegisteredManagersScrollViewPos = new Vector2();
        private Dictionary<ManagerSystem.ExecuteOrderType, ReorderableList> OrderLists = new Dictionary<ManagerSystem.ExecuteOrderType, ReorderableList>();
        private string[] UpdateOrderTypeEnumNames = null;

        // Helper functions
        private void RefreshManagers() {
            OrderLists.Clear();
        }

        private IManager FindManagerByType(Type type) {
            for (int i = 0; i < ManagerSystem.ManagerEntries.Length; ++i) {
                ManagerSystem.ManagerEntry managerEntry = ManagerSystem.ManagerEntries[i];
                if (managerEntry.ManagerBehaviour.GetType() == type) {
                    return managerEntry.ManagerBehaviour as IManager;
                }
            }
            return null;
        }

        private void AddManager(Type type) {
            MonoBehaviour managerBehaviour = ManagerSystem.gameObject.AddComponent(type) as MonoBehaviour;
            managerBehaviour.hideFlags |= HideFlags.HideInInspector;
            ArrayUtility.Add(ref ManagerSystem.ManagerEntries, new ManagerSystem.ManagerEntry(managerBehaviour));
            int managerEntryIndex = ManagerSystem.ManagerEntries.Length - 1;
            // Add it to all updates
            for (int i = 0; i < ManagerSystem.ExecuteOrders.Length; ++i) {
                ArrayUtility.Add(ref ManagerSystem.ExecuteOrders[i].Entries, new ManagerSystem.ExecuteOrderEntry(managerEntryIndex));
            }
            RefreshManagers();
        }

        private void RemoveManager(int index) {
            // Find and remove indices, move the rest
            for (int i = 0; i < ManagerSystem.ExecuteOrders.Length; ++i) {
                ManagerSystem.ExecuteOrder updateOrder = ManagerSystem.ExecuteOrders[i];
                for (int j = 0; j < updateOrder.Entries.Length; ++j) {
                    if (updateOrder.Entries[j].InstanceIndex == index) {
                        ArrayUtility.RemoveAt(ref updateOrder.Entries, j);
                        --j;
                    } else if (updateOrder.Entries[j].InstanceIndex > index) {
                        updateOrder.Entries[j].InstanceIndex--;
                    }
                }
            }
            // Destroy component (check incase the script was deleted beforehand)
            if (ManagerSystem.ManagerEntries[index].ManagerBehaviour != null) {
                GameObject.DestroyImmediate(ManagerSystem.ManagerEntries[index].ManagerBehaviour);
            }
            // Remove from list
            ArrayUtility.RemoveAt(ref ManagerSystem.ManagerEntries, index);
            RefreshManagers();
        }

        private void CleanupManagers() {
            for (int i = 0; i < ManagerSystem.ManagerEntries.Length; ++i) {
                // This one was deleted
                if (ManagerSystem.ManagerEntries[i] == null) {
                    RemoveManager(i);
                    --i;
                }
            }
        }

        private void BeginEdit() {
            EditorGUI.BeginChangeCheck();
        }

        private bool EndEdit(bool force = false) {
            if (Application.isPlaying) {
                return false;
            }
            if (force || EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(ManagerSystem);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                return true;
            }
            return false;
        }

        public void OnEnable() {
            ManagerSystem = (ManagerSystem) target;
            // Cleanup
            CleanupManagers();
            // Add BucketUpdateSystem
            if (ManagerSystem.BucketUpdateSystem == null) {
                ManagerSystem.BucketUpdateSystem = ManagerSystem.gameObject.GetComponent<BucketUpdateSystem>();
                ManagerSystem.BucketUpdateSystem.ManagerSystem = ManagerSystem;
            }
            // Force execution order
            if (MonoImporter.GetExecutionOrder(MonoScript.FromMonoBehaviour(ManagerSystem)) != -100) {
                MonoImporter.SetExecutionOrder(MonoScript.FromMonoBehaviour(ManagerSystem), -100);
                MonoImporter.SetExecutionOrder(MonoScript.FromMonoBehaviour(ManagerSystem.BucketUpdateSystem), -99);
            }
            // Get ExecuteOrderType enum and remove last entry
            List<string> enumNames = new List<string>(Enum.GetNames(typeof(ManagerSystem.ExecuteOrderType)));
            enumNames.RemoveAt(enumNames.Count - 1);
            UpdateOrderTypeEnumNames = enumNames.ToArray();
        }

        public override void OnInspectorGUI() {
            float w = EditorGUIUtility.currentViewWidth;

            if (LinkStatusStyle == null) {
                LinkStatusStyle = new GUIStyle(EditorStyles.label);
            }
            if (ManagerLabelStyle == null) {
                ManagerLabelStyle = new GUIStyle(EditorStyles.label);
            }
            // Manager registration
            BeginEdit();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            LinkedMonoScript = (MonoScript) EditorGUILayout.ObjectField("Register Manager", (UnityEngine.Object) LinkedMonoScript, typeof(MonoScript), false);
            if (LinkedMonoScript != null) {
                Type managerType = LinkedMonoScript.GetClass();
                Type[] interfaces = managerType.GetInterfaces();
                if (interfaces.Contains(typeof(IManager)) == false) {
                    EditorUtility.DisplayDialog("Error", string.Format("Script {0} does not contain a class derived from Manager!", LinkedMonoScript.name), "OK");
                    LinkStatus = string.Format("Failed to link {0}", LinkedMonoScript.name);
                    LinkStatusStyle.normal.textColor = Color.red;
                } else if (FindManagerByType(managerType) != null) {
                    EditorUtility.DisplayDialog("Error", string.Format("Manager {0} already registered!", managerType.Name), "OK");
                    LinkStatus = string.Format("Manager {0} already registered!", managerType.Name);
                    LinkStatusStyle.normal.textColor = Color.red;
                    LinkStatusStyle.fontStyle = FontStyle.Bold;
                } else {
                    // Add manager
                    AddManager(managerType);
                    LinkStatus = string.Format("Manager {0} registered!", LinkedMonoScript.name);
                    LinkStatusStyle.normal.textColor = new Color(0.0f, 0.7f, 0.0f);
                    LinkStatusStyle.fontStyle = FontStyle.Bold;
                }
            }

            // Manager state
            LinkedMonoScript = null;
            EditorGUILayout.LabelField(LinkStatus, LinkStatusStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Registered Managers");
            RegisteredManagersScrollViewPos = EditorGUILayout.BeginScrollView(RegisteredManagersScrollViewPos, false, true, GUILayout.Height(300.0f));

            float w3 = w * 0.29f;

            for (int i = 0; i < ManagerSystem.ManagerEntries.Length; i++) {
                ManagerSystem.ManagerEntry instance = ManagerSystem.ManagerEntries[i];
                string managerName = instance.ManagerBehaviour.GetType().Name;
                //
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                string state = "?";
                if ((instance.EntryFlags & ManagerSystem.ManagerEntryFlags.Enabled) != 0) {
                    state = "E";
                    ManagerLabelStyle.normal.textColor = Color.green;
                } else if ((instance.EntryFlags & ManagerSystem.ManagerEntryFlags.Enabled) == 0) {
                    state = "D";
                    ManagerLabelStyle.normal.textColor = Color.grey;
                } else {
                    ManagerLabelStyle.normal.textColor = Color.yellow;
                }
                EditorGUILayout.LabelField(string.Format("[{0}] {1}", state, managerName), ManagerLabelStyle, GUILayout.Width(w3));
                //
                if ((instance.EntryFlags & ManagerSystem.ManagerEntryFlags.Enabled) == 0 && GUILayout.Button("Enable", GUILayout.Width(w3))) {
                    if (EditorUtility.DisplayDialog("Warning", string.Format("Are you sure you want to ENABLE {0}?", managerName), "Yes", "No") == true) {
                        instance.EntryFlags |= ManagerSystem.ManagerEntryFlags.Enabled;
                    }
                }
                if ((instance.EntryFlags & ManagerSystem.ManagerEntryFlags.Enabled) != 0 && GUILayout.Button("Disable", GUILayout.Width(w3))) {
                    if (EditorUtility.DisplayDialog("Warning", string.Format("Are you sure you want to DISABLE {0}?", managerName), "Yes", "No") == true) {
                        instance.EntryFlags &= ~ManagerSystem.ManagerEntryFlags.Enabled;
                    }
                }
                if (GUILayout.Button("Delete", GUILayout.Width(w3))) {
                    if (EditorUtility.DisplayDialog("Warning", string.Format("Are you sure you want to DELETE {0}?", managerName), "Yes", "No") == true) {
                        RemoveManager(i);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                bool enabledInEditor = (instance.EntryFlags & ManagerSystem.ManagerEntryFlags.EnabledInEditor) != 0;
                enabledInEditor = EditorGUILayout.Toggle("Execute In Editor", enabledInEditor);
                if (enabledInEditor) {
                    instance.EntryFlags |= ManagerSystem.ManagerEntryFlags.EnabledInEditor;
                } else {
                    instance.EntryFlags &= ~ManagerSystem.ManagerEntryFlags.EnabledInEditor;
                }
                bool visible = (instance.EntryFlags & ManagerSystem.ManagerEntryFlags.Visible) != 0;
                visible = EditorGUILayout.Toggle("Visible", visible);
                if (visible) {
                    instance.EntryFlags |= ManagerSystem.ManagerEntryFlags.Visible;
                    instance.ManagerBehaviour.hideFlags &= ~HideFlags.HideInInspector;
                } else {
                    instance.EntryFlags &= ~ManagerSystem.ManagerEntryFlags.Visible;
                    instance.ManagerBehaviour.hideFlags |= HideFlags.HideInInspector;
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            // Manager update orders
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            ManagerOrderType = (ManagerSystem.ExecuteOrderType) EditorGUILayout.Popup("Execute Order Type", (int) ManagerOrderType, UpdateOrderTypeEnumNames);
            EditorGUILayout.Space();
            ReorderableList list = EnableDisplayManagerOrder(ManagerOrderType);
            list.DoLayoutList();
            EditorGUILayout.EndVertical();
            EndEdit();
        }

        private ReorderableList EnableDisplayManagerOrder(ManagerSystem.ExecuteOrderType orderType) {
            ManagerSystem.ExecuteOrder updateOrder = ManagerSystem.ExecuteOrders[(int) orderType];
            ReorderableList list = null;
            if (OrderLists.TryGetValue(orderType, out list) == false) {
                list = new ReorderableList(updateOrder.Entries, typeof(ManagerSystem.ExecuteOrderEntry), true, true, false, false);
                list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    if (index >= updateOrder.Entries.Length) {
                        return;
                    }
                    ManagerSystem.ExecuteOrderEntry entry = updateOrder.Entries[index];
                    ManagerSystem.ManagerEntry instance = ManagerSystem.ManagerEntries[entry.InstanceIndex];
                    rect.y += 2;
                    float rect3 = rect.width * 0.333f;
                    GUI.Label(new Rect(rect.x, rect.y, rect3, EditorGUIUtility.singleLineHeight), instance.ManagerBehaviour.GetType().Name);
                    entry.IsEnabled = EditorGUI.Toggle(new Rect(rect.x + rect.width - 60, rect.y, 20, EditorGUIUtility.singleLineHeight), entry.IsEnabled);
                };
                list.drawHeaderCallback = (Rect rect) => {
                    EditorGUI.LabelField(rect, string.Format("{0} Order", orderType));
                };
                list.onChangedCallback = (ReorderableList reorderableList) => {
                    BeginEdit();
                    EndEdit(true);
                };
                OrderLists.Add(orderType, list);
            }
            return list;
        }
    }
}