using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public sealed partial class RavenCallFunctionContinuousEvent {
#if UNITY_EDITOR

        public RavenTriggerPropertyComponentBase Property {
            get {
                return m_Property;
            }
            set {
                m_Property = value;
            }
        }

        public sealed override RavenAnimationDataComponentBase AnimationDataEditorOnly {
            get {
                return null;
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