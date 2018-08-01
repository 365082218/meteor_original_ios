using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenAnimationDataTweenView<T> : RavenAnimationDataBaseView<T> {
        protected const int c_CurveQuality = 25;

        protected RavenAnimationDataTween<T> m_AnimationDataTweenBase;
        protected SerializedObject m_AnimationDataTweenSerializedObject;
        protected RavenParameterFieldView<T> m_StartValueView;
        protected RavenParameterFieldView<T> m_EndValueView;
        protected RavenSequence m_Sequence;

        protected readonly Color[] m_Colors = new Color[] { Color.red, Color.green, Color.blue, Color.white };

        public override void Initialize(RavenAnimationPropertyBaseView<T> propertyView, RavenAnimationDataComponentBase animationData) {
            base.Initialize(propertyView, animationData);
            m_AnimationDataTweenBase = animationData as RavenAnimationDataTween<T>;
            m_AnimationDataTweenSerializedObject = new SerializedObject(m_AnimationDataTweenBase);
            m_Sequence = RavenSequenceEditor.Instance.Sequence;

            m_StartValueView = new RavenParameterFieldView<T>(m_AnimationDataTweenSerializedObject,
                m_AnimationDataTweenSerializedObject.FindProperty("m_StartValueStart"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_StartValueEnd"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_StartValueType"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_StartParameterIndex"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_StartValueIsObjectLink"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_StartValueObjectLink"),
                m_PropertyBase.TargetComponent.GetType());

            m_EndValueView = new RavenParameterFieldView<T>(m_AnimationDataTweenSerializedObject,
                m_AnimationDataTweenSerializedObject.FindProperty("m_EndValueStart"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_EndValueEnd"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_EndValueType"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_EndParameterIndex"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_EndValueIsObjectLink"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_EndValueObjectLink"),
                m_PropertyBase.TargetComponent.GetType());
        }

        protected virtual void DrawAnimationData(Rect position) {
        }

        protected override void OnDrawGui(Rect position) {
        }

        protected override void OnDrawExtendedGui(Rect position) {
            float width = Mathf.Min(700f, position.width) / 2f;
            const float offset = 20f;

            bool drawGui = position.width > 100f;

            DrawAnimationData(position);
            if (drawGui) {
                m_StartValueView.DrawGui(new Rect(position.x, position.y + offset, width, position.height - offset));
                m_EndValueView.DrawGui(new Rect(position.x + position.width - width, position.y + offset, width, position.height - offset));

                float easeWidth = Mathf.Min(110, position.width - 15f) / 3f;
                m_AnimationDataTweenBase.UseCustomEaseCurve = EditorGUI.Toggle(new Rect(position.x, position.y, 15f, 10f), m_AnimationDataTweenBase.UseCustomEaseCurve);
                if (m_AnimationDataTweenBase.UseCustomEaseCurve) {
                    m_AnimationDataTweenBase.CustomCurve = EditorGUI.CurveField(new Rect(position.x + 15f, position.y, easeWidth, 15f), "", m_AnimationDataTweenBase.CustomCurve);
                } else {
                    m_AnimationDataTweenBase.EaseType = (ERavenEaseType)EditorGUI.EnumPopup(new Rect(position.x + 15f, position.y, easeWidth, 10f), m_AnimationDataTweenBase.EaseType);
                    m_AnimationDataTweenBase.EaseAmplitude = EditorGUI.DoubleField(new Rect(position.x + 15f + easeWidth, position.y, easeWidth, 20f), m_AnimationDataTweenBase.EaseAmplitude);
                    m_AnimationDataTweenBase.EasePeriod = EditorGUI.DoubleField(new Rect(position.x + 15f + 2 * easeWidth, position.y, easeWidth, 20f), m_AnimationDataTweenBase.EasePeriod);
                }
            }
        }

        protected override bool OnHandleInput(Vector2 mousePosition) {
            return false;
        }
    }
}