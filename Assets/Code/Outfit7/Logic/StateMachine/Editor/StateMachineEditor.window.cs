using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Outfit7.Logic {

    public class StateMachineEditorWindow : EditorWindow {

        public static StateMachineEditor StateMachineEditor { private get; set; }

        private void SetTitleName(string name) {
            titleContent = new GUIContent(name);
        }

        private void OnEnable() {
        }

        private void OnDisable() {
        }

        private void OnFocus() {
            if (StateMachineEditor == null) {
                return;
            }
            StateMachineEditor.OnWindowFocus();
            SetTitleName(StateMachineEditor.TitleName);
        }

        private void OnLostFocus() {
            if (StateMachineEditor == null) {
                return;
            }
            StateMachineEditor.OnWindowLostFocus();
        }

        private void OnSelectionChange() {
            if (StateMachineEditor == null) {
                SetTitleName("State Machine");
            }
        }

        private void OnGUI() {
            if (StateMachineEditor == null) {
                return;
            }
            if (StateMachineEditor.OnWindowGUI(position)) {
                Repaint();
            }
        }
    }
}