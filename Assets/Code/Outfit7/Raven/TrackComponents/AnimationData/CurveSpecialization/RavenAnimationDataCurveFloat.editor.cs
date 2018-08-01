using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveFloat {
#if UNITY_EDITOR

        protected sealed override void SetStartingValues(float values) {
            m_Curves = new AnimationCurve[1];
            m_Curves[0] = new AnimationCurve();
            m_Curves[0].AddKey(new Keyframe(0, values));
            m_Curves[0].AddKey(new Keyframe(1, values));
        }

#endif
    }
}