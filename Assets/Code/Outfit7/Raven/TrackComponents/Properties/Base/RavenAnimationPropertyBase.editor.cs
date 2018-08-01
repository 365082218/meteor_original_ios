using Starlite.Raven.Internal;
using System;

namespace Starlite.Raven {

    public abstract partial class RavenAnimationPropertyBase<T> {
#if UNITY_EDITOR

        public sealed override RavenAnimationDataComponentBase AnimationData {
            get {
                return m_AnimationData;
            }

            set {
                m_AnimationData = value;
                m_AnimationDataCast = m_AnimationData as RavenAnimationDataBase<T>;
            }
        }

        public sealed override RavenTriggerPropertyComponentBase TriggerProperty {
            get {
                return m_TriggerProperty;
            }

            set {
                m_TriggerProperty = value;
            }
        }

        public ERavenAnimationPropertyType PropertyType {
            get {
                return m_PropertyType;
            }
        }

        public virtual bool[] ApplyValues {
            get {
                return null;
            }
        }

        public override Type[] TypeConstraints {
            get {
                return new Type[] { typeof(T) };
            }
        }

        public T GetSyncedValueForRecordingEditor() {
            return GetValue(m_TargetComponent);
        }

        public void SetSyncedValueForRecordingEditor(T value) {
            SetValue(value, m_TargetComponent);
        }

        public override bool CheckForDependencies() {
            var success = true;
            success &= CheckForInterpolatorDependency();

            return success;
        }

        protected virtual bool CheckForInterpolatorDependency() {
            try {
                RavenInterpolatorOverseer.GetInterpolator<T>();
            } catch {
                RavenLog.Error("Interpolator for type {0} does not exist!", typeof(T).ToString());
                return false;
            }

            return true;
        }

#endif
    }
}