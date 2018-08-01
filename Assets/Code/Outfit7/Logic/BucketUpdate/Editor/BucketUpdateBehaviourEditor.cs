using System.Collections;
using UnityEngine;
using UnityEditor;
using Outfit7.Util;

namespace Outfit7.Logic {

    [CustomEditor(typeof(BucketUpdateBehaviour), true), CanEditMultipleObjects]
    public class BucketUpdateBehaviourEditor : UnityEditor.Editor {

        private static bool FoldoutEnabled = false;

        protected void DrawInspectorGUI() {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            FoldoutEnabled = EditorGUILayout.Foldout(FoldoutEnabled, "Bucket Update");
            if (FoldoutEnabled) {
                EditorGUI.indentLevel++;
                SerializedProperty bucketIndex = serializedObject.FindProperty("bucketIndex");
                SerializedProperty updateAfter = serializedObject.FindProperty("updateAfterBehaviour");
                if (updateAfter.objectReferenceValue == null) {
                    bucketIndex.intValue = BucketUpdatePreferences.GetBucketIndex(EditorGUILayout.Popup("Bucket Index", BucketUpdatePreferences.GetValidBucketIndex(bucketIndex.intValue), BucketUpdatePreferences.BucketNames));
                }
                updateAfter.objectReferenceValue = EditorGUILayout.ObjectField("Update After Behaviour", updateAfter.objectReferenceValue, typeof(BucketUpdateBehaviour), true);
                EditorGUI.indentLevel--;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            OnExtendedGUI();
            DrawInspectorGUI();
        }

        public virtual void OnExtendedGUI() {

        }
    }

}