using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveRect {
        private const int c_ValueCount = 4;

        protected sealed override void SyncStartingValues(Rect values) {
            var vectorValues = values.ToVector4();
            for (int i = 0; i < c_ValueCount; ++i) {
                var curve = m_Curves[i];
                var key = curve.keys[0];
                key.value = vectorValues[i];
                curve.RemoveKey(0);
                curve.AddKey(key);
            }
        }

        public sealed override Rect EvaluateAtTime(double time, double duration) {
            var normalizedTime = (float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType);
            var v = new Vector4();
            for (int i = 0; i < c_ValueCount; ++i) {
                v[i] = m_Curves[m_UniformCurves ? 0 : i].Evaluate(normalizedTime);
            }
            return v.ToRect();
        }
    }
}