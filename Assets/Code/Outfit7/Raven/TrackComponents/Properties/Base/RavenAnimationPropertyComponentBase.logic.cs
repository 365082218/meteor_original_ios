using System;
using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenAnimationPropertyComponentBase {

        public sealed override RavenPropertyComponent ChildPropertyComponent {
            get {
                return m_TriggerProperty;
            }
        }

        public ulong TargetHash {
            get {
                return m_TargetHash;
            }
#if UNITY_EDITOR
            set {
                m_TargetHash = value;
            }
#endif
        }

        public abstract bool IsCustom {
            get;
        }

        public bool IsValid() {
            var propertyValid = m_AnimationData != null && (IsCustom ? IsCustomValid() : m_TargetHash != RavenUtility.s_InvalidHash);
            return propertyValid && IsTriggerPropertyValid();
        }

        public bool IsTriggerPropertyValid() {
            return m_TriggerProperty == null || m_TriggerProperty.TargetHash != RavenUtility.s_InvalidHash;
        }

        public override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);
            if (m_TriggerProperty != null) {
                m_TriggerProperty.Initialize(sequence);
            }
            if (m_AnimationData != null) {
                m_AnimationData.Initialize(sequence, this);
            }
        }

        public abstract void EvaluateAtTime(double time, double duration);

        public abstract void OnExit();

        public abstract Type GetPropertyType();

        protected abstract bool IsCustomValid();
    }
}