using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public sealed partial class RavenPropertyEvent {
#if UNITY_EDITOR

        public RavenAnimationPropertyComponentBase Property {
            get {
                return m_Property;
            }
            set {
                m_Property = value;
            }
        }

        public GameObject TriggerTarget {
            get {
                return m_TriggerTarget;
            }
        }

        public bool IsSetProperty {
            get {
                return m_IsSetProperty;
            }
            set {
                m_IsSetProperty = value;
            }
        }

        public override RavenAnimationDataComponentBase AnimationDataEditorOnly {
            get {
                return m_Property != null ? m_Property.AnimationData : null;
            }
        }

        public override void SetHideFlags(HideFlags hideFlags) {
            base.SetHideFlags(hideFlags);
            if (m_Property != null) {
                m_Property.SetHideFlags(hideFlags);
            }
        }

        public override void DestroyEditor(RavenSequence sequence) {
            base.DestroyEditor(sequence);
            if (m_Property != null) {
                m_Property.DestroyEditor(sequence);
            }
        }

        public void SetTriggerTarget(RavenSequence sequence, GameObject target) {
            Undo.RecordObject(this, "SetTriggerTarget");
            m_TriggerTarget = target;

            if (m_Property != null && m_Property.TriggerProperty != null) {
                Undo.RecordObject(m_Property.TriggerProperty, "SetTriggerTarget");
                m_Property.TriggerProperty.Target = target;
                m_Property.TriggerProperty.Initialize(sequence);
                if (!m_Property.TriggerProperty.ValidateProperty(sequence, target)) {
                    m_Property.TriggerProperty = null;
                }
            }
        }

        protected override void OnSetTargetEditor(RavenSequence sequence, GameObject target) {
            if (m_Property != null) {
                Undo.RecordObject(m_Property, "OnSetTarget");
                m_Property.Target = target;
                m_Property.Initialize(sequence);
                if (!m_Property.ValidateProperty(sequence, target)) {
                    m_Property = null;
                }
            }
        }

#endif
    }
}