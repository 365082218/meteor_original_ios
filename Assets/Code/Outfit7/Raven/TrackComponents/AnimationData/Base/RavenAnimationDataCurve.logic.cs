using System;
using UnityEngine;

namespace Starlite.Raven {

    [ContinuousAnimationData]
    public abstract partial class RavenAnimationDataCurve<T> {

        public override bool ShouldSyncStartingValues {
            get {
                return m_ValueType == ERavenValueType.Current;
            }
        }

        public override void Initialize(RavenSequence sequence, RavenAnimationPropertyComponentBase property) {
            base.Initialize(sequence, property);
        }

        protected override void OnEnterCallback() {
        }

        protected override void OnExitCallback() {
        }

        protected sealed override T GetValueFromParameterCallback(RavenParameter parameter) {
            throw new NotImplementedException();
        }

        protected double GetCurrentTime(double time, double duration) {
            if (m_RepeatCount > 1) {
                return GetTimeForRepeatableMirror(time, duration, m_RepeatCount, m_Mirror);
            }

            return time;
        }
    }
}