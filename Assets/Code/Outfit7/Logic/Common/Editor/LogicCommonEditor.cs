using UnityEngine;
using UnityEditor;
using System.Collections;
using Outfit7.Util;

namespace Outfit7.Logic {

    public enum AddRemoveMoveAction {
        None,
        Add,
        Remove,
        MoveUp,
        MoveDown,
    }

    public class LogicEditorCommon : UnityEditor.Editor {

        private static InvokeEventEditor EventEditorInstance = null;

        private static bool AddRemoveEvents(ref InvokeEvent[] events, int index, bool canAdd, bool canRemove) {
            AddRemoveMoveAction addRemove = LogicEditorCommon.AddRemoveGUI(canAdd, canRemove);
            if (addRemove == AddRemoveMoveAction.Add) {
                ArrayUtility.Add(ref events, new InvokeEvent());
                return true;
            } else if (addRemove == AddRemoveMoveAction.Remove) {
                ArrayUtility.RemoveAt(ref events, index);
                return true;
            }
            return false;
        }

        public static AddRemoveMoveAction AddRemoveGUI(bool canAdd, bool canRemove) {
            if (canAdd && GUILayout.Button("+", GUILayout.Width(20))) {
                return AddRemoveMoveAction.Add;
            }
            if (canRemove && GUILayout.Button("-", GUILayout.Width(20))) {
                return AddRemoveMoveAction.Remove;
            }
            return AddRemoveMoveAction.None;
        }

        public static AddRemoveMoveAction AddRemoveAndMoveGUI(bool canAdd, bool canRemove, bool canMoveUp, bool canMoveDown) {
            if (canAdd && GUILayout.Button("+", GUILayout.Width(20))) {
                return AddRemoveMoveAction.Add;
            }
            if (canRemove && GUILayout.Button("-", GUILayout.Width(20))) {
                return AddRemoveMoveAction.Remove;
            }
            if (canMoveUp && GUILayout.Button("U", GUILayout.Width(20))) {
                return AddRemoveMoveAction.MoveUp;
            }
            if (canMoveDown && GUILayout.Button("D", GUILayout.Width(20))) {
                return AddRemoveMoveAction.MoveDown;
            }
            return AddRemoveMoveAction.None;
        }

        public static void OpenEventEditor(GameObject ActiveGameObject, MonoBehaviour[] userDefinedCallbackMonoBehaviours, InvokeEvent e) {
            CloseEventEditor();
            EventEditorInstance = EditorWindow.GetWindow<InvokeEventEditor>();
            Assert.NotNull(EventEditorInstance, "EventEditor must not fail!");
            EventEditorInstance.Initialize(ActiveGameObject, userDefinedCallbackMonoBehaviours, e);
            EventEditorInstance.ShowPopup();
        }

        public static void CloseEventEditor() {
            if (EventEditorInstance != null) {
                EventEditorInstance.Close();
                EventEditorInstance = null;
            }
        }

        public static bool EditEvents(GameObject ActiveGameObject, MonoBehaviour[] userDefinedCallbackMonoBehaviours, ref InvokeEvent[] events) {
            bool changed = false;
            // Must not be null!
            if (events == null) {
                events = new InvokeEvent[0];
            }
            EditorGUI.indentLevel++;
            for (int i = 0; i < events.Length; i++) {
                InvokeEvent e = events[i];
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(string.Format("#{0} {1}", i, e.Method), GUILayout.MaxWidth(300))) {
                    OpenEventEditor(ActiveGameObject, userDefinedCallbackMonoBehaviours, e);
                    changed = true;
                }
                if (AddRemoveEvents(ref events, i, i == events.Length - 1, true)) {
                    changed = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (AddRemoveEvents(ref events, 0, events.Length == 0, false)) {
                changed = true;
            }
            EditorGUI.indentLevel--;
            return changed;
        }
    }

}