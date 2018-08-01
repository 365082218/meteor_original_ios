

//GUIStyle Node = (GUIStyle) "flow node 0";
//foldout = EditorGUI.Foldout(new Rect(0, 0, 100, 15), foldout, "Track");
//EditorGUI.LabelField(sizable, "Event", Node);
//Rect resizeRect = new Rect(sizable.x + sizable.width - 5, sizable.y, 10, sizable.height);
//EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);
//
//if (UnityEngine.Event.current.type == EventType.mouseDown && resizeRect.Contains(UnityEngine.Event.current.mousePosition)) {
//    resize = true;
//}
//if (resize) {
//    float mouseY = UnityEngine.Event.current.mousePosition.x;
//    sizable.Set(sizable.x, sizable.y, mouseY - sizable.x, sizable.height);
//}
//if (UnityEngine.Event.current.type == EventType.MouseUp)
//    resize = false;

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace Outfit7.Sequencer {
    public class TimelineData {
        public Rect Rect;
        public float LenghtOfASecond = 0;
        public float Offset = 0.1f;
        public float Scale = 1f;
        //200px = 1s

        public float GetTimeAtMousePosition(Vector2 mousePos) {
            return ((mousePos.x - Rect.x) / LenghtOfASecond) - Offset;
        }
    }

    public class SequencerEditor : UnityEditor.EditorWindow {

        public SequencerSequence Sequence = null;
        public SequencerSequenceView SequenceView = null;

        public static float LeftSpliWidth = 200f;
        private float StartHeight = 0;
        private Rect WindowSize = new Rect();

        private void DefineLayout() {
            GUIStyle Title = (GUIStyle) "OL Titlemid";
            GUIStyle RightPartBG = (GUIStyle) "AnimationCurveEditorBackground";
            EditorGUI.LabelField(new Rect(LeftSpliWidth, 0, WindowSize.width - LeftSpliWidth, WindowSize.height), "", RightPartBG);
            EditorGUI.LabelField(new Rect(0, 0, WindowSize.width, 30), "", Title);
        }

        private void DrawSequence(EditorWindow window) {
            SequenceView.DrawGui(WindowSize, StartHeight, LeftSpliWidth, window);
        }

        private bool HandleInput() {
            bool InputHandled = SequenceView.HandleInput();
            if ((EventType.KeyDown == UnityEngine.Event.current.type || EventType.keyUp == UnityEngine.Event.current.type || EventType.mouseDown == UnityEngine.Event.current.type || EventType.mouseUp == UnityEngine.Event.current.type)) {
                UnityEngine.Event.current.Use();
            }
            return InputHandled;
        }

        public void PrePaint() {
            SequenceView.PrePaint();
        }

        public void PostPaint() {
            SequenceView.PostPaint();
        }

        public bool SetCurrentlySelectedSequence() {
            if (Selection.activeGameObject == null)
                return true;
            Sequence = Selection.activeGameObject.GetComponent<SequencerSequence>();
            if (Sequence == null)
                return true;
            else {
                SequenceView = new SequencerSequenceView(Sequence);
                return false;
            }
        }

        public bool OnWindowGUI(Rect windowRect) {
            WindowSize = windowRect;
            // Styles
            //InitializeStyles();
            // Draw state machine

            DefineLayout();

            if (Sequence == null)
                return true;

            PrePaint();

            BeginEdit();
            DrawSequence(this);
            EndEdit(HandleInput());
            // Repaint
            Repaint();
            PostPaint();
            return true;
        }

        public void RemakeView() {
            SequenceView = new SequencerSequenceView(Sequence);
        }

        private void BeginEdit() {
            EditorGUI.BeginChangeCheck();
        }

        private bool EndEdit(bool force = false) {
            if (Application.isPlaying) {
                return false;
            }
            if (force || EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(Sequence.gameObject);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                return true;
            }
            return false;
        }

        private void OnDestroy() {
            Debug.LogError("WHY IS THIS NOT CALLED!");
        }
    }

}
