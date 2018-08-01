using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditorInternal;
using Outfit7.Logic.StateMachineInternal;
using System.Collections.Generic;
using Outfit7.Logic;
using UnityEditor.SceneManagement;

namespace Outfit7.Sequencer {
    [CustomEditor(typeof(SequencerSequence))]
    public class SequencerSequenceEditor : BucketUpdateBehaviourEditor {
        private SequencerSequence SequencerSequence;
        ReorderableList ParametersList = null;
        ReorderableList BookmarkList = null;

        public void OnEnable() {
            SequencerSequence myTarget = (SequencerSequence) target;
            ParametersList = StateMachineParameterEditor.InitReorderableList(myTarget.Parameters);
            BookmarkList = new ReorderableList(myTarget.Bookmarks, typeof(SequencerBookmarkEvent), false, true, false, false);

            BookmarkList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                SequencerBookmarkEvent evnt = myTarget.Bookmarks[index] as SequencerBookmarkEvent;
                if (GUI.Button(new Rect(rect.x, rect.y, 25, 15), "->")) {
                    SequencerWindow.ShowWindow();
                    if (SequencerWindow.SequencerEditor.SequenceView != null)
                        SequencerWindow.SequencerEditor.SequenceView.FocusTimeline(evnt.StartTime, true);
                }
                EditorGUI.LabelField(new Rect(rect.x + 25, rect.y, rect.width - 120, rect.height), evnt.BookmarkName);
                EditorGUI.LabelField(new Rect(rect.x + 25 + rect.width - 120, rect.y, 120, rect.height), evnt.StartTime.ToString());
            };
            BookmarkList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Bookmarks");
            };
        }

        public override void OnInspectorGUI() {
            DrawInspectorGUI();
            SequencerSequence myTarget = (SequencerSequence) target;

            EditorGUI.BeginChangeCheck();
            BookmarkList.DoLayoutList();
            ParametersList.DoLayoutList();
            myTarget.Duration = EditorGUILayout.FloatField("Duration", myTarget.Duration);
            myTarget.Loop = EditorGUILayout.Toggle("Looping", myTarget.Loop);
            myTarget.PlayOnAwake = EditorGUILayout.Toggle("PlayOnAwake", myTarget.PlayOnAwake);
            myTarget.TimeScale = EditorGUILayout.FloatField("Time Scale", myTarget.TimeScale);
            myTarget.SetCurrentTime(EditorGUILayout.FloatField("Current Time", myTarget.GetCurrentTime()));
            myTarget.Playing = EditorGUILayout.Toggle("Playing", myTarget.Playing);
            SequencerSequence.DebugMode = EditorGUILayout.Toggle("DebugMode", SequencerSequence.DebugMode);
            if (EditorGUI.EndChangeCheck()) {
                if (!Application.isPlaying) {
                    EditorUtility.SetDirty(myTarget);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            if (SequencerSequence.DebugMode)
                myTarget.UnHideAll();
            if (GUILayout.Button("Show Sequencer")) {
                SequencerWindow.ShowWindow();
            }
        }
    }
}