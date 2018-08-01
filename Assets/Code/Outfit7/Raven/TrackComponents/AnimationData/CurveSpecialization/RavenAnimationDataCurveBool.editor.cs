using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveBool {
#if UNITY_EDITOR

        protected sealed override void SetStartingValues(bool values) {
            m_Curves = new AnimationCurve[1];
            m_Curves[0] = new AnimationCurve();
            m_Curves[0].AddKey(new Keyframe(0, values ? 1f : 0f));
            m_Curves[0].AddKey(new Keyframe(1, values ? 1f : 0f));
        }

#endif
    }
}