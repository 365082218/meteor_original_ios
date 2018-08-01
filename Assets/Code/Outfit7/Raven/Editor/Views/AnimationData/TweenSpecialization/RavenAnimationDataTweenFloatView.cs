using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenAnimationDataTweenFloatView : RavenAnimationDataTweenView<float> {

        protected override void DrawAnimationData(Rect position) {
            DrawHelper.DrawAnimationDataTweenCurves(new Vector4(m_AnimationDataTweenBase.GetStartValueEditor(m_Sequence, m_PropertyBase), 0f, 0f, 0f),
                new Vector4(m_AnimationDataTweenBase.GetEndValueEditor(m_Sequence, m_PropertyBase), 0f, 0f, 0f),
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