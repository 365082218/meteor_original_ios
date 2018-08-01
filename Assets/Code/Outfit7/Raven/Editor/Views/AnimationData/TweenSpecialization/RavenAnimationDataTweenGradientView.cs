using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenAnimationDataTweenGradientView : RavenAnimationDataTweenView<Gradient> {
        private RavenAnimationDataTweenGradient m_AnimationDataTween;
        private Texture2D m_ColorTexture;
        private Color m_TexturePreviousStartValue;
        private Color m_TexturePreviousEndValue;
        private ERavenEaseType m_PreviousEaseType;
        private int m_PreviousRepeatCount;
        private bool m_PreviousMirror;

        public override void Initialize(RavenAnimationPropertyBaseView<Gradient> propertyView, RavenAnimationDataComponentBase animationData) {
            base.Initialize(propertyView, animationData);
            m_AnimationDataTween = m_AnimationDataTweenBase as RavenAnimationDataTweenGradient;
            CreateTexture();
            m_TexturePreviousStartValue = Vector4.one * -1f;
            m_TexturePreviousEndValue = Vector4.one;
        }

        protected override void DrawAnimationData(Rect position) {
            RecalculateTexture(m_AnimationDataTween);

            EditorGUI.DrawTextureTransparent(position, m_ColorTexture);
        }

        public void RecalculateTexture(RavenAnimationDataTweenGradient tweenData) {
            var startValue = tweenData.GetStartValueEditor(m_Sequence, m_PropertyBase);
            var endValue = tweenData.GetEndValueEditor(m_Sequence, m_PropertyBase);
            if (!CreateTexture() && 
                Vector4.SqrMagnitude(m_TexturePreviousStartValue - startValue.Evaluate(0.5f)) < 0.001f &&
                Vector4.SqrMagnitude(m_TexturePreviousEndValue - endValue.Evaluate(0.5f)) < 0.001f &&
                m_PreviousEaseType == tweenData.EaseType &&
                m_PreviousRepeatCount == tweenData.RepeatCount &&
                m_PreviousMirror == tweenData.Mirror) {
                return;
            }

            var textureWidth = m_ColorTexture.width;

            m_TexturePreviousStartValue = startValue.Evaluate(0.5f);
            m_TexturePreviousEndValue = endValue.Evaluate(0.5f);
            m_PreviousEaseType = tweenData.EaseType;
            m_PreviousRepeatCount = tweenData.RepeatCount;
            m_PreviousMirror = tweenData.Mirror;

            Color[] pixels = new Color[textureWidth];
            for (int s = 0; s < textureWidth; s++) {
                var position = (double)s / (textureWidth - 1);
                var inputPosition = tweenData.GetTimeForRepeatableMirrorEditor(position, 1, tweenData.RepeatCount, tweenData.Mirror);
                double tweenValue;
                if (tweenData.UseCustomEaseCurve) {
                    tweenValue = tweenData.CustomCurve.Evaluate((float)inputPosition);
                } else {
                    tweenValue = RavenEaseUtility.Evaluate(tweenData.EaseType, inputPosition, 1, tweenData.EaseAmplitude, tweenData.EasePeriod);
                }

                pixels[s] = RavenValueInterpolatorColor.Default.Interpolate(startValue.Evaluate((float)tweenValue), endValue.Evaluate((float)tweenValue), tweenValue);
            }
            m_ColorTexture.SetPixels(pixels);
            m_ColorTexture.Apply();
        }

        private bool CreateTexture() {
            if (m_ColorTexture == null) {
                m_ColorTexture = new Texture2D(128, 1);
                return true;
            }

            return false;
        }
    }
}