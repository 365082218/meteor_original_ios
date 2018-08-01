using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveInt {

        protected sealed override void SyncStartingValues(int values) {
            var curve = m_Curves[0];
            var key = curve.keys[0];
            key.value = values;
            curve.RemoveKey(0);
            curve.AddKey(key);
        }

        public sealed override int EvaluateAtTime(double time, double duration) {
            return (int)m_Curves[0].Evaluate((float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType));
        }
    }
}