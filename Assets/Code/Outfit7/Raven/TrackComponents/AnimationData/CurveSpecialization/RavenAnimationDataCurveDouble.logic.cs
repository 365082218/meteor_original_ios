using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveDouble {

        protected sealed override void SyncStartingValues(double values) {
            var curve = m_Curves[0];
            var key = curve.keys[0];
            key.value = (float)values;
            curve.RemoveKey(0);
            curve.AddKey(key);
        }

        public sealed override double EvaluateAtTime(double time, double duration) {
            return (double)m_Curves[0].Evaluate((float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType));
        }
    }
}