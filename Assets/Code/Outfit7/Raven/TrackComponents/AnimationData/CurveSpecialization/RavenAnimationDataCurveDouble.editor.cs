using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveDouble {
#if UNITY_EDITOR

        protected sealed override void SetStartingValues(double values) {
            m_Curves = new AnimationCurve[1];
            m_Curves[0] = new AnimationCurve();
            m_Curves[0].AddKey(new Keyframe(0, (float)values));
            m_Curves[0].AddKey(new Keyframe(1, (float)values));
        }

#endif
    }
}