using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Starlite.Raven {

    [CustomEditor(typeof(RavenSequence))]
    public sealed class RavenSequenceComponentEditor : Starlite.SceneObjectComponentEditor {
        private ReorderableList m_ParametersList = null;
        private ReorderableList m_BookmarkList = null;

        private void OnEnable() {
            RavenSequence myTarget = (RavenSequence)target;
            m_ParametersList = RavenParameterEditor.InitReorderableList(myTarget.Parameters);
            m_BookmarkList = new ReorderableList(myTarget.SortedBookmarks, typeof(RavenBookmarkEvent), false, true, false, false) {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    RavenBookmarkEvent evnt = myTarget.SortedBookmarks[index];
                    if (GUI.Button(new Rect(rect.x, rect.y, 25, 15), "->")) {
                        var window = RavenSequenceEditor.ShowWindow();
                        if (window.SequenceView != null) {
                            window.SequenceView.FocusTimeline(evnt.StartFrame);
                        }
                    }
                    EditorGUI.LabelField(new Rect(rect.x + 25, rect.y, rect.width - 120, rect.height), evnt.BookmarkName);
                    EditorGUI.LabelField(new Rect(rect.x + 25 + rect.width - 120, rect.y, 120, rect.height), (evnt.StartFrame * myTarget.FrameDuration).ToString());
                },
                drawHeaderCallback = (Rect rect) => {
                    EditorGUI.LabelField(rect, "Bookmarks");
                }
            };
        }

        public sealed override void OnInspectorGUI() {
            var sequence = target as RavenSequence;
            Undo.RecordObject(sequence, "Raven Inspector");

            DrawInspectorGUI();
            m_BookmarkList.DoLayoutList();
            m_ParametersList.DoLayoutList();

            if (sequence.Name != target.name) {
                sequence.Name = target.name;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.DoubleField("Current Time", sequence.CurrentTime);
            EditorGUI.EndDisabledGroup();
            sequence.Fps = EditorGUILayout.DelayedIntField("Fps", sequence.Fps);
            var duration = EditorGUICustom.DelayedDoubleField(EditorGUILayout.GetControlRect(false, 16f, EditorStyles.numberField), "Duration", sequence.Duration);
            if (duration != sequence.Duration) {
                sequence.SetDuration(duration);
            }
            sequence.TimeScale = EditorGUILayout.DoubleField("Time Scale", sequence.TimeScale);
            sequence.PlayOnAwake = EditorGUILayout.Toggle("Play On Awake", sequence.PlayOnAwake);
            sequence.PlayOnEnable = EditorGUILayout.Toggle("Play On Enable", sequence.PlayOnEnable);
            sequence.Loop = EditorGUILayout.Toggle("Loop", sequence.Loop);

            RavenSequence.s_DebugMode = EditorGUILayout.Toggle("Debug Mode", RavenSequence.s_DebugMode);

            if (RavenSequence.s_DebugMode) {
                sequence.ShowAllComponents();
            }

            if (GUILayout.Button("Play")) {
                for (int i = 0; i < targets.Length; ++i) {
                    (targets[i] as RavenSequence).Play();
                }
            }
            if (GUILayout.Button("Play Instant")) {
                for (int i = 0; i < targets.Length; ++i) {
                    (targets[i] as RavenSequence).Play(true, true);
                }
            }
            if (GUILayout.Button("Resume")) {
                for (int i = 0; i < targets.Length; ++i) {
                    (targets[i] as RavenSequence).Play(false);
                }
            }
            if (GUILayout.Button("Clean Orphans")) {
                for (int i = 0; i < targets.Length; ++i) {
                    var t = (targets[i] as RavenSequence);
                    t.ClearOrphans();
                }
            }
            if (GUILayout.Button("Show Sequence")) {
                RavenSequenceEditor.ShowWindow();
            }
        }
    }
}