using System;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenAnimationPropertyBaseView<T> : RavenPropertyBaseNonGenericView {
        protected RavenEventView m_EventView;
        protected RavenPropertyEvent m_PropertyEvent;
        protected RavenAnimationPropertyBase<T> m_PropertyBase;
        protected RavenAnimationDataBaseView<T> m_AnimationDataBaseView;

        protected T m_RecordSavedValue;

        public RavenAnimationPropertyBase<T> PropertyBase {
            get {
                return m_PropertyBase;
            }
        }

        public override void Initialize(RavenEventView eventView, RavenPropertyComponent property) {
            m_EventView = eventView;
            m_PropertyEvent = m_EventView.Event as RavenPropertyEvent;
            m_PropertyBase = property as RavenAnimationPropertyBase<T>;
            m_AnimationDataBaseView = InitializeView(m_PropertyBase.AnimationData);

            // validate
            m_PropertyEvent.SetTriggerTarget(RavenSequenceEditor.Instance.Sequence, m_PropertyEvent.TriggerTarget);
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
            DrawHelper.DrawToggleBoxes(m_PropertyBase.ApplyValues, m_PropertyBase, position);
            DrawFunctionCallStuff(position);
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
            if (m_AnimationDataBaseView != null) {
                m_AnimationDataBaseView.RecordStart();
            }
        }

        public sealed override void RecordEnd() {
            if (m_PropertyBase == null) {
                return;
            }

            Undo.RecordObject(m_PropertyBase, "RecordEnd");
            OnRecordEnd();
            if (m_AnimationDataBaseView != null) {
                m_AnimationDataBaseView.RecordEnd();
            }
        }

        protected virtual void OnDrawGui(Rect position) {
            if (m_AnimationDataBaseView != null) {
                m_AnimationDataBaseView.DrawGui(position);
            }
        }

        protected virtual void OnDrawExtendedGui(Rect position) {
            if (m_AnimationDataBaseView != null) {
                m_AnimationDataBaseView.DrawExtendedGui(position);
            }
        }

        protected virtual bool OnHandleInput(Vector2 mousePosition) {
            if (m_AnimationDataBaseView != null) {
                return m_AnimationDataBaseView.HandleInput(mousePosition);
            }
            return false;
        }

        protected virtual void OnRecordStart() {
            m_RecordSavedValue = m_PropertyBase.GetSyncedValueForRecordingEditor();
        }

        protected virtual void OnRecordEnd() {
            m_PropertyBase.SetSyncedValueForRecordingEditor(m_RecordSavedValue);
        }

        private RavenAnimationDataBaseView<T> InitializeView(RavenAnimationDataComponentBase animationData) {
            if (animationData == null) {
                return null;
            }

            var viewType = RavenEditorUtility.GetGenericViewType(animationData.GetType(), typeof(T));
            var instance = Activator.CreateInstance(viewType) as RavenAnimationDataBaseView<T>;
            instance.Initialize(this, animationData);
            return instance;
        }

        private void DrawFunctionCallStuff(Rect position) {
            const float kTargetWidth = 100f;
            const float kFuncNameWidth = 110f;
            const float kOverrideParamWidth = 85f;
            const float kOtherStuffWidth = 125f;
            const float kMinWidth = 8f;

            var hasTriggerProperty = m_PropertyEvent.Property != null && m_PropertyEvent.Property.TriggerProperty != null;

            var widthFunction = m_PropertyEvent.TriggerTarget == null ? 0f : Mathf.Max(Mathf.Min(kFuncNameWidth, position.width - kOtherStuffWidth), 0f);
            var widthOverride = !hasTriggerProperty ? 0f : Mathf.Max(Mathf.Min(kOverrideParamWidth, position.width - kOtherStuffWidth - widthFunction), 0f);
            var widthTarget = Mathf.Max(Mathf.Min(kTargetWidth, position.width - kOtherStuffWidth - widthFunction - widthOverride), 0f);

            if (widthTarget < kTargetWidth && hasTriggerProperty && m_PropertyEvent.Property.TriggerProperty.ParameterIndex == -1) {
                var sub = Mathf.Min(kTargetWidth - widthTarget, widthOverride);
                widthOverride -= sub;
                widthTarget += sub;
            }

            GameObject target = null;
            if (widthTarget > kMinWidth) {
                target = EditorGUI.ObjectField(new Rect(position.xMax - widthTarget - widthFunction - widthOverride, position.y, widthTarget, 16f), m_PropertyEvent.TriggerTarget, typeof(GameObject), true) as GameObject;
                if (target != m_PropertyEvent.TriggerTarget) {
                    m_PropertyEvent.SetTriggerTarget(RavenSequenceEditor.Instance.Sequence, target);
                    RavenFunctionCallEditor.Refresh();
                }
            }

            if (widthOverride > kMinWidth) {
                var parameterRect = new Rect(position.xMax - widthFunction - widthOverride, position.y, widthOverride, 16f);
                RavenParameterEditor.DrawOverrideTargetsParameterFieldForTriggerProperty(m_PropertyEvent.Property.TriggerProperty, parameterRect, RavenSequenceEditor.Instance.Sequence.Parameters);
            }

            target = m_PropertyEvent.TriggerTarget;
            if (target != null && widthFunction > kMinWidth) {
                string text = "None";
                if (m_PropertyEvent.Property.TriggerProperty != null) {
                    text = RavenUtility.GetFunctionNameFromPackedFunctionName(m_PropertyEvent.Property.TriggerProperty.FunctionName);
                }
                if (GUI.Button(new Rect(position.xMax - widthFunction, position.y, widthFunction, 16f), "#" + text)) {
                    RavenFunctionCallEditor.OpenEditor(RavenSequenceEditor.Instance.Sequence, target, m_PropertyEvent, m_PropertyEvent.Property.TriggerProperty, true, m_PropertyEvent.Property.TypeConstraints);
                }
            }
        }
    }
}