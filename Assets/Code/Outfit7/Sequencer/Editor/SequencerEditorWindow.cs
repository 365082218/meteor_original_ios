using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Outfit7.Sequencer {

    public class SequencerWindow : EditorWindow {
        static public SequencerEditor SequencerEditor { get; private set; }

        [MenuItem("Outfit7/Sequencer %&S", false, 1)]
        public static void ShowWindow() {
            SequencerEditor = new SequencerEditor();
            EditorWindow.GetWindow<SequencerWindow>("Sequencer");
            SequencerEditor.SetCurrentlySelectedSequence();
        }

        [MenuItem("GameObject/Sequence", false, -1)]
        public static void AddSequence() {
            GameObject seq = new GameObject("seq_NoName");
            Undo.AddComponent<SequencerSequence>(seq);
            if (Selection.activeGameObject != null) {
                seq.transform.parent = Selection.activeGameObject.transform;
            }
            Selection.activeGameObject = seq;
        }

        void OnEnable() {
            Undo.undoRedoPerformed += RemakeView;
        }

        void OnDisable() {
            Undo.undoRedoPerformed -= RemakeView;
        }

        public void RemakeView() {
            SequencerEditor.RemakeView();
        }

        void OnFocus() {
            //if (AnimationStateMachineEditor == null) {
            //    return;
            //}
            //AnimationStateMachineEditor.OnWindowFocus();            
        }

        void OnLostFocus() {
            //if (AnimationStateMachineEditor == null) {
            //    return;
            //}
            //AnimationStateMachineEditor.OnWindowLostFocus();
        }

        void OnSelectionChange() {
        }

        void OnGUI() {
            if (SequencerEditor == null) {
                return;
            }
            
            if (SequencerEditor.OnWindowGUI(position)) {
                Repaint();
            }
        }
    }
}