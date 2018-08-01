using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace Outfit7.Logic {
    [CustomEditor(typeof(MessageEventBehaviour), true)]
    public class MessageEventBehaviourEditor : BucketUpdateBehaviourEditor {

        string[] MessageEventNames;
        List<int> TargetMessageEvents;
        bool[] MessageEventToggles;

        public void OnEnable() {
            MessageEventNames = MessageEventPreferences.MessageEventNames;
            MessageEventToggles = new bool[MessageEventNames.Length];
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            MessageEventBehaviour myTarget = (MessageEventBehaviour) target;

            TargetMessageEvents = myTarget.EditorGetMessageEvents();
            for (int a = 0; a < MessageEventNames.Length; a++) {
                string msgEventId = MessageEventNames[a];
                MessageEventToggles[a] = TargetMessageEvents.Contains(msgEventId.GetHashCode());
            }

            myTarget.EditorShowMessageEventsGroup = EditorGUILayout.Foldout(myTarget.EditorShowMessageEventsGroup, "Message Events");
            if (myTarget.EditorShowMessageEventsGroup) {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginVertical();

                myTarget.AutoRegister = EditorGUILayout.ToggleLeft("Auto Register on Enable/Disable", myTarget.AutoRegister);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Message Events");
                EditorGUI.indentLevel++;

                for (int a = 0; a < MessageEventNames.Length; a++) {
                    MessageEventToggles[a] = EditorGUILayout.ToggleLeft(MessageEventNames[a], MessageEventToggles[a]);
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck()) {

                    TargetMessageEvents.Clear();
                    for (int a = 0; a < MessageEventNames.Length; a++) {
                        string msgEventId = MessageEventNames[a];
                        if (MessageEventToggles[a]) {
                            TargetMessageEvents.Add(msgEventId.GetHashCode());
                        }
                    }

                    EditorUtility.SetDirty(myTarget);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}