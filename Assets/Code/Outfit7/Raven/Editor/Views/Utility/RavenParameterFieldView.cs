using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenParameterFieldView<T> {
        private SerializedObject m_Object;

        // values
        private SerializedProperty m_FieldStartProperty;

        private SerializedProperty m_FieldEndProperty;
        private SerializedProperty m_FieldTypeProperty;
        private SerializedProperty m_ValueParameterProperty;
        private SerializedProperty m_IsObjectLinkProperty;
        private SerializedProperty m_ObjectLinkProperty;

        // flip values
        private SerializedProperty m_StartSecondaryProperty;

        private SerializedProperty m_EndSecondaryProperty;
        private SerializedProperty m_SecondaryTypeProperty;
        private SerializedProperty m_SecondaryParameterProperty;

        private Type m_Type;
        private Type m_ObjectType;
        private List<RavenParameter> m_CachedParameters;
        private string[] m_Choices;

        private static readonly string[] s_ConstValues = Enum.GetNames(typeof(ERavenValueType));

        public bool ShowSecondary {
            get;
            set;
        }

        public RavenParameterFieldView(SerializedObject obj, SerializedProperty fieldStartProperty,
            SerializedProperty fieldEndProperty, SerializedProperty fieldTypeProperty,
            SerializedProperty parameterIndexProperty, SerializedProperty isObjectLinkProperty,
            SerializedProperty objectLinkProperty, Type objectType) {
            m_Object = obj;
            m_FieldStartProperty = fieldStartProperty;
            m_FieldEndProperty = fieldEndProperty;
            m_FieldTypeProperty = fieldTypeProperty;
            m_ValueParameterProperty = parameterIndexProperty;
            m_IsObjectLinkProperty = isObjectLinkProperty;
            m_ObjectLinkProperty = objectLinkProperty;

            m_Type = typeof(T);
            m_ObjectType = objectType;
        }

        public void SetSecondaryProperties(SerializedProperty startTangentProperty,
            SerializedProperty endTangentProperty, SerializedProperty tangentTypeProperty,
            SerializedProperty tangentParameterProperty) {
            m_StartSecondaryProperty = startTangentProperty;
            m_EndSecondaryProperty = endTangentProperty;
            m_SecondaryTypeProperty = tangentTypeProperty;
            m_SecondaryParameterProperty = tangentParameterProperty;
        }

        public virtual void DrawGui(Rect position) {
            const float toggleSize = 16f;

            var height = 16f;

            var popupRect = new Rect(position);
            popupRect.height = height;

            m_Object.Update();

            if (!ShowSecondary) {
                popupRect.x += toggleSize;
                popupRect.width -= toggleSize;

                ValidateParameters(x => x.CanBeAssignedTo(m_Type));
                DrawHelper.DrawProperty(m_IsObjectLinkProperty, new Rect(position.x, position.y, toggleSize, toggleSize), m_Type, m_ObjectType);
                if (m_IsObjectLinkProperty.boolValue) {
                    m_ValueParameterProperty.intValue = -1;
                    DrawHelper.DrawProperty(m_ObjectLinkProperty, popupRect, m_Type, m_ObjectType, true);
                } else {
                    var selectedPopupIndex = GetSelectedPopupIndex(m_ValueParameterProperty, m_FieldTypeProperty);
                    int idx = EditorGUI.Popup(popupRect, selectedPopupIndex, m_Choices);
                    if (idx != selectedPopupIndex) {
                        ProcessSelectedPopupIndex(idx, m_ValueParameterProperty, m_FieldTypeProperty);
                    }
                    var valueType = (ERavenValueType)idx;

                    if (valueType == ERavenValueType.Constant || valueType == ERavenValueType.Range) {
                        position.y += height;
                        position.height -= height;
                        DrawHelper.DrawProperty(m_FieldStartProperty, position, m_Type, m_ObjectType);

                        if (valueType == ERavenValueType.Range) {
                            position.y += height;
                            position.height -= height;
                            DrawHelper.DrawProperty(m_FieldEndProperty, position, m_Type, m_ObjectType);
                        }
                    }
                }
            } else {
                ValidateParameters(x => x.m_ParameterType == ERavenParameterType.Vector4);
                var selectedPopupIndex = GetSelectedPopupIndex(m_SecondaryParameterProperty, m_SecondaryTypeProperty);
                int idx = EditorGUI.Popup(popupRect, selectedPopupIndex, m_Choices);
                if (idx != selectedPopupIndex) {
                    ProcessSelectedPopupIndex(idx, m_SecondaryParameterProperty, m_SecondaryTypeProperty);
                }
                var valueType = (ERavenValueType)idx;

                if (valueType == ERavenValueType.Constant || valueType == ERavenValueType.Range) {
                    position.y += height;
                    position.height -= height;
                    DrawHelper.DrawProperty(m_StartSecondaryProperty, position, m_Type, m_ObjectType);

                    if (valueType == ERavenValueType.Range) {
                        position.y += height;
                        position.height -= height;
                        DrawHelper.DrawProperty(m_EndSecondaryProperty, position, m_Type, m_ObjectType);
                    }
                }
            }
            m_Object.ApplyModifiedProperties();
        }

        private void ValidateParameters(Func<RavenParameter, bool> predicate) {
            var sequence = RavenSequenceEditor.Instance.Sequence;
            var parameters = sequence.Parameters;
            if (m_Choices == null || m_CachedParameters.Count != parameters.Count) {
                RebuildChoices(predicate);
                return;
            }

            for (int i = 0; i < parameters.Count; ++i) {
                if (m_CachedParameters[i] != parameters[i] ||
                    m_CachedParameters[i].m_ParameterType != parameters[i].m_ParameterType ||
                    m_CachedParameters[i].m_Name != parameters[i].m_Name ||
                    m_CachedParameters[i].m_ParameterIndex != parameters[i].m_ParameterIndex) {
                    RebuildChoices(predicate);
                    return;
                }
            }
        }

        private void RebuildChoices(Func<RavenParameter, bool> predicate) {
            var sequence = RavenSequenceEditor.Instance.Sequence;
            m_CachedParameters = new List<RavenParameter>();
            for (int i = 0; i < sequence.Parameters.Count; ++i) {
                m_CachedParameters.Add(sequence.Parameters[i].ShallowCopy());
            }
            var choices = new List<string>(s_ConstValues);
            var parameters = m_CachedParameters.Where(predicate).Select(x => x.m_Name);
            if (parameters.Any()) {
                // separator
                choices.Add(string.Empty);
            }
            choices.AddRange(parameters);
            m_Choices = choices.ToArray();
        }

        private int GetSelectedPopupIndex(SerializedProperty parameterProperty, SerializedProperty valueTypeProperty) {
            var savedIndex = parameterProperty.intValue;
            if (savedIndex >= 0) {
                if (savedIndex >= m_CachedParameters.Count) {
                    parameterProperty.intValue = -1;
                    return -1;
                } else {
                    var idx = Array.IndexOf(m_Choices, m_CachedParameters[savedIndex].m_Name);
                    if (idx == -1) {
                        parameterProperty.intValue = -1;
                    }
                    return idx;
                }
            }
            return valueTypeProperty.enumValueIndex;
        }

        private void ProcessSelectedPopupIndex(int idx, SerializedProperty parameterProperty, SerializedProperty valueTypeProperty) {
            if (idx < s_ConstValues.Length) {
                valueTypeProperty.enumValueIndex = idx;
                parameterProperty.intValue = -1;
            } else {
                parameterProperty.intValue = m_CachedParameters.FindIndex(x => x.m_Name == m_Choices[idx]);
            }
        }
    }
}