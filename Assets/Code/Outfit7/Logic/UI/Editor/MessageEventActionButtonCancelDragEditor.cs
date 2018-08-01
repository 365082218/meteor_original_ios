using System.Collections;
using UnityEngine;
using Outfit7.Audio;
using Outfit7.Util;
using Outfit7.UI;
using UnityEditor;

namespace Outfit7.Logic {

    [CustomEditor(typeof(MessageEventActionButtonCancelDrag), true)]
    [CanEditMultipleObjects]
    public class MessageEventActionButtonCancelDragEditor : MessageEventActionButtonEditor {

        private SerializedProperty CancelClickWhenMovingFactorProperty;

        protected override void OnEnable() {
            base.OnEnable();
            CancelClickWhenMovingFactorProperty = serializedObject.FindProperty("CancelClickWhenMovingFactor");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(CancelClickWhenMovingFactorProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}