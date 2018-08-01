using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveBool {

        protected sealed override void SyncStartingValues(bool values) {
            var curve = m_Curves[0];
            var key = curve.keys[0];
            key.value = values ? 1f : 0f;
            curve.RemoveKey(0);
            curve.AddKey(key);
        }

        public sealed override bool EvaluateAtTime(double time, double duration) {
            return m_Curves[0].Evaluate((float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType)) >= 1f ? true : false;
        }
    }
}