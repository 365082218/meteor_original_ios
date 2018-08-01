using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveColor {
        private const int c_ValueCount = 4;

        protected sealed override void SyncStartingValues(Color values) {
            for (int i = 0; i < c_ValueCount; ++i) {
                var curve = m_Curves[i];
                var key = curve.keys[0];
                key.value = values[i];
                curve.RemoveKey(0);
                curve.AddKey(key);
            }
        }

        public sealed override Color EvaluateAtTime(double time, double duration) {
            var normalizedTime = (float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType);
            var color = new Color();
            for (int i = 0; i < c_ValueCount; ++i) {
                color[i] = m_Curves[m_UniformCurves ? 0 : i].Evaluate(normalizedTime);
            }
            return color;
        }
    }
}