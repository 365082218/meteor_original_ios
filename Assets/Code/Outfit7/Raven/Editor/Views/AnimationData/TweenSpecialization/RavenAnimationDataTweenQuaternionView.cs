using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenAnimationDataTweenQuaternionView : RavenAnimationDataTweenView<Quaternion> {

        protected override void DrawAnimationData(Rect position) {
            DrawHelper.DrawAnimationDataTweenCurves(m_AnimationDataTweenBase.GetStartValueEditor(m_Sequence, m_PropertyBase).ToVector4(),
                m_AnimationDataTweenBase.GetEndValueEditor(m_Sequence, m_PropertyBase).ToVector4(),
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