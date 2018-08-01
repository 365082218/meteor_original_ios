using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenTriggerPropertyComponentBaseView : RavenPropertyBaseNonGenericView {
        protected RavenEventView m_EventView;
        protected RavenTriggerPropertyComponentBase m_PropertyBase;
        protected SerializedObject m_SerializedObject;
        protected List<SerializedProperty> m_SerializedProperties = new List<SerializedProperty>();

        private static Type s_UnityObjectType = typeof(UnityEngine.Object);

        public override void Initialize(RavenEventView eventView, RavenPropertyComponent property) {
            m_EventView = eventView;
            m_PropertyBase = property as RavenTriggerPropertyComponentBase;

            m_SerializedObject = new SerializedObject(m_PropertyBase);
            for (int i = 0; i < RavenUtility.c_MaxFunctionParameters; ++i) {
                var serializedProperty = m_SerializedObject.FindProperty("m_Value" + i);
                if (serializedProperty == null) {
                    break;
                }
                m_SerializedProperties.Add(serializedProperty);
            }
        }

        public sealed override void DrawGui(Rect position) {
            if (m_PropertyBase == null) {
                return;
            }

            Undo.RecordObject(m_PropertyBase, "DrawGui");
            OnDrawGui(position);
        }

        public sealed override void DrawExtendedGui(Rect position) {
            if (m_PropertyBase == null) {
                return;
            }

            Undo.RecordObject(m_PropertyBase, "DrawExtendedGui");
            OnDrawExtendedGui(position);
        }

        public sealed override bool HandleInput(Vector2 mousePosition) {
            if (m_PropertyBase == null) {
                return false;
            }

            Undo.RecordObject(m_PropertyBase, "HandleInput");
            return OnHandleInput(mousePosition);
        }

        public sealed override void RecordStart() {
            if (m_PropertyBase == null) {
                return;
            }

            Undo.RecordObject(m_PropertyBase, "RecordStart");
            OnRecordStart();
        }

        public sealed override void RecordEnd() {
            if (m_PropertyBase == null) {
                return;
            }

            Undo.RecordObject(m_PropertyBase, "RecordEnd");
            OnRecordEnd();
        }

        protected virtual void OnDrawGui(Rect position) {

        }

        protected virtual void OnDrawExtendedGui(Rect position) {
            if (!RavenPreferences.TimelineShowFunctionParameters) {
                return;
            }
            var nMaxProperties = RavenPreferences.TimelineShowFunctionParametersCount;

            if (m_SerializedProperties.Count > nMaxProperties) {
                return;
            }

            var newPosition = new Rect(position);
            var usedHeight = newPosition.height * 0.5f;
            newPosition.y += usedHeight;
            newPosition.height = 16f;
            newPosition.width = Math.Min(Math.Max(position.width, 80f), 350f);

            m_SerializedObject.Update();
            var propertyCount = Math.Min(nMaxProperties, m_SerializedProperties.Count);
            for (int i = 0; i < propertyCount; ++i) {
                DrawHelper.DrawProperty(m_SerializedProperties[i], m_SerializedProperties[i].propertyType == SerializedPropertyType.Boolean ? new Rect(newPosition.position, new Vector2(16f, 16)) : newPosition, s_UnityObjectType, s_UnityObjectType);
                newPosition.y += 16f;
            }
            m_SerializedObject.ApplyModifiedProperties();
        }

        protected virtual bool OnHandleInput(Vector2 mousePosition) {
            return false;
        }

        protected virtual void OnRecordStart() {

        }

        protected virtual void OnRecordEnd() {

        }
    }
}