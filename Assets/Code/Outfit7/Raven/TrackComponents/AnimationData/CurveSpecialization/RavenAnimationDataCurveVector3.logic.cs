using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveVector3 {
        private const int c_ValueCount = 3;

        protected sealed override void SyncStartingValues(Vector3 values) {
            for (int i = 0; i < c_ValueCount; ++i) {
                var curve = m_Curves[i];
                var key = curve.keys[0];
                key.value = values[i];
                curve.RemoveKey(0);
                curve.AddKey(key);
            }
        }

        public sealed override Vector3 EvaluateAtTime(double time, double duration) {
            var normalizedTime = (float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType);
            var v = new Vector3();
            for (int i = 0; i < c_ValueCount; ++i) {
                v[i] = m_Curves[m_UniformCurves ? 0 : i].Evaluate(normalizedTime);
            }
            return v;
        }
    }
}