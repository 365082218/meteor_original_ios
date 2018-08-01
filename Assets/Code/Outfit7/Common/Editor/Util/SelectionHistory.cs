using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;

namespace Outfit7.Util {

    public static class SelectionHistoryNavigator {
        //private const string Tag = "SelectionHistoryNavigator";
        private static List<Object> SelectionHistory = new List<Object>();
        private static int SelectionHistoryIndex;
        private static bool IgnoreSelectionChange;

        [InitializeOnLoadMethod]
        [DidReloadScripts]
        private static void Start() {
            SelectionHistoryIndex = -1;
            IgnoreSelectionChange = false;
            Selection.selectionChanged -= OnSelectionChange;
            Selection.selectionChanged += OnSelectionChange;
        }

        private static void OnSelectionChange() {
            if (IgnoreSelectionChange) {
                IgnoreSelectionChange = false;
                return;
            }
            //O7Log.DebugT(Tag, "OnSelectionChange: {0}; current history count: {1}; index: {2}", Selection.activeObject, SelectionHistory.Count, SelectionHistoryIndex);

            int remove = SelectionHistory.Count - SelectionHistoryIndex - 1;
            if (remove > 0) {
                //O7Log.DebugT(Tag, "OnSelectionChange: removing {0} items", remove);
                SelectionHistory.RemoveRange(SelectionHistoryIndex + 1, remove);
            }

            if (SelectionHistory.Count <= 0 || SelectionHistory[SelectionHistoryIndex] != Selection.activeObject) {
                SelectionHistory.Add(Selection.activeObject);
                SelectionHistoryIndex = SelectionHistory.Count - 1;
            }
        }

        [MenuItem("Outfit7/Selection/Prev &LEFT")]
        private static void SelectPrev() {
            if (SelectionHistory.Count > 0) {
                SelectionHistoryIndex--;
                if (SelectionHistoryIndex < 0) {
                    SelectionHistoryIndex = 0;
                }
                IgnoreSelectionChange = true;
                Selection.activeObject = SelectionHistory[SelectionHistoryIndex];
                //O7Log.DebugT(Tag, "SelectPrev: {0}; current history count: {1}; index: {2}", Selection.activeObject, SelectionHistory.Count, SelectionHistoryIndex);
                InternalEditorUtility.RepaintAllViews();
            }
        }

        [MenuItem("Outfit7/Selection/Next &RIGHT")]
        private static void SelectNext() {
            if (SelectionHistory.Count > 0) {
                SelectionHistoryIndex++;
                if (SelectionHistoryIndex >= SelectionHistory.Count) {
                    SelectionHistoryIndex = SelectionHistory.Count - 1;
                }
                IgnoreSelectionChange = true;
                Selection.activeObject = SelectionHistory[SelectionHistoryIndex];
                //O7Log.DebugT(Tag, "SelectNext: {0}; current history count: {1}; index: {2}", Selection.activeObject, SelectionHistory.Count, SelectionHistoryIndex);
                InternalEditorUtility.RepaintAllViews();
            }
        }
    }
}
