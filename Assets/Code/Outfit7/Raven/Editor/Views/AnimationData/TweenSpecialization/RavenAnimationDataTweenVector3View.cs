using UnityEngine;

namespace Starlite.Raven {

    public class RavenAnimationDataTweenVector3View : RavenAnimationDataTweenView<Vector3> {

        protected override void DrawAnimationData(Rect position) {
            DrawHelper.DrawAnimationDataTweenCurves(m_AnimationDataTweenBase.GetStartValueEditor(m_Sequence, m_PropertyBase),
                m_AnimationDataTweenBase.GetEndValueEditor(m_Sequence, m_PropertyBase),
                m_AnimationDataTweenBase.GetMinMax(m_Sequence, m_PropertyBase),
                m_AnimationDataTweenBase.RepeatCount,
                m_PropertyBase.ApplyValues,
                m_AnimationDataTweenBase,
                m_PropertyBase,
                m_Colors,
                position);
        }
    }
}