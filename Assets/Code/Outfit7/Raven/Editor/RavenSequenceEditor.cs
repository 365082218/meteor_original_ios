using System;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenSequenceEditor : EditorWindow {
        private const float c_LeftSplitWidth = 200f;
        private const float c_StartHeight = 0;

        [SerializeField]
        private RavenSequence m_Sequence = null;

        [SerializeField]
        private TimelineData m_TimeLineData = new TimelineData();

        [SerializeField]
        private Vector2 m_ScrollPosition = Vector2.zero;

        private bool m_JustEnabled = false;

        private static RavenSequence s_CachedSequence = null;

        public RavenSequence Sequence {
            get {
                return m_Sequence;
            }
        }

        public RavenSequenceView SequenceView { get; private set; }

        public static RavenSequenceEditor Instance { get; private set; }

        [MenuItem("GameObject/Raven Sequence", false, -2)]
        public static void AddSequence() {
            var seq = new GameObject("seq_NoName");
            Undo.AddComponent<RavenSequence>(seq);
            if (Selection.activeGameObject != null) {
                seq.transform.parent = Selection.activeGameObject.transform;
            }
            Selection.activeGameObject = seq;
        }

        [MenuItem("Raven/Editor %&S", false, -1)]
        public static RavenSequenceEditor ShowWindow() {
            var sequence = FindSequence();
            if (sequence == null) {
                return null;
            }
            s_CachedSequence = sequence;
            var editor = GetWindow<RavenSequenceEditor>("Raven Sequence Editor", true, typeof(EditorWindow));
            if (editor.m_Sequence != sequence) {
                editor.m_Sequence = sequence;
                editor.RemakeView();
            }
            editor.autoRepaintOnSceneChange = false;
            editor.Show();
            return editor;
        }

        public void RemakeView() {
            if (m_Sequence != null) {
                m_Sequence.FlagDirty();
            }
            SequenceView = new RavenSequenceView();
            SequenceView.Initialize(m_Sequence, m_TimeLineData);
        }

        private void OnEnable() {
            Undo.undoRedoPerformed += RemakeView;
            m_JustEnabled = true;
            Instance = this;
            if (s_CachedSequence != null) {
                m_Sequence = s_CachedSequence;
                s_CachedSequence = null;
            } else if (m_Sequence == null) {
                m_Sequence = FindSequence();
            }
        }

        private void OnDisable() {
            Undo.undoRedoPerformed -= RemakeView;
            m_JustEnabled = false;
        }

        private void OnGUI() {
            GUI.skin = RavenPreferences.Skin;

            if (m_JustEnabled || SequenceView == null || SequenceView.Sequence == null) {
                if (m_Sequence == null) {
                    m_Sequence = FindSequence();
                    return;
                }
                m_JustEnabled = false;
                SequenceView = new RavenSequenceView();
                SequenceView.Initialize(m_Sequence, m_TimeLineData);
            }

            SequenceView.PrePaint();
            BeginEdit();
            SequenceView.DrawGUI(position, ref m_ScrollPosition, c_StartHeight, c_LeftSplitWidth);
            var inputHandled = HandleInput();
            EndEdit(inputHandled);
            Repaint();
            SequenceView.PostPaint();
        }

        private bool HandleInput() {
            var inputHandled = SequenceView.HandleInput();
            if (EventType.KeyDown == UnityEngine.Event.current.type ||
                EventType.KeyUp == UnityEngine.Event.current.type ||
                EventType.MouseDown == UnityEngine.Event.current.type ||
                EventType.MouseUp == UnityEngine.Event.current.type) {
                UnityEngine.Event.current.Use();
            }
            return inputHandled;
        }

        private void BeginEdit() {
            EditorGUI.BeginChangeCheck();
        }

        private bool EndEdit(bool force = false) {
            if (Application.isPlaying) {
                return false;
            }
            if (!force && !EditorGUI.EndChangeCheck()) {
                return false;
            }
            // This is used to force redraw of inspector
            EditorUtility.SetDirty(m_Sequence.gameObject);
            return true;
        }

        private static RavenSequence FindSequence() {
            return Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponent<RavenSequence>();
        }
    }

    [Serializable]
    public class TimelineData {
        public const double c_PixelsPerSecond = 200;

        public Rect m_Rect;
        public double m_LenghtOfASecond = 0;
        public double m_Offset = 0.1f;
        public float m_Scale = 1f;

        public double GetTimeAtMousePosition(Vector2 mousePos) {
            return ((mousePos.x - m_Rect.x) / m_LenghtOfASecond) - m_Offset;
        }

        public float GetPositionAtFrame(int frame) {
            var sequence = RavenSequenceEditor.Instance.Sequence;
            if (sequence == null) {
                return m_Rect.x;
            }

            return m_Rect.x + (float)(m_LenghtOfASecond * (sequence.GetTimeForFrame(frame) + m_Offset));
        }

        public float GetWidthForFrames(int nFrames) {
            var sequence = RavenSequenceEditor.Instance.Sequence;
            if (sequence == null) {
                return m_Rect.x;
            }

            return (float)(m_LenghtOfASecond * sequence.GetTimeForFrame(nFrames));
        }

        public int GetFrameAtMousePosition(Vector2 mousePos) {
            var sequence = RavenSequenceEditor.Instance.Sequence;
            if (sequence == null) {
                return 0;
            }

            return (int)sequence.GetFrameForTime(GetTimeAtMousePosition(mousePos));
        }

        public void SetScaleToFitFrames(int nFrames) {
            var sequence = RavenSequenceEditor.Instance.Sequence;
            if (sequence == null) {
                return;
            }

            var duration = sequence.GetTimeForFrame(nFrames);
            m_Scale = (float)(c_PixelsPerSecond / duration) * 0.012f;
            m_LenghtOfASecond = m_Scale * c_PixelsPerSecond;
        }

        public void ResetScale() {
            m_Scale = 1;
            m_LenghtOfASecond = c_PixelsPerSecond;
        }
    }
}