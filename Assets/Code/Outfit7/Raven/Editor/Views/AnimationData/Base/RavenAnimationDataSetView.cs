using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenAnimationDataSetView<T> : RavenAnimationDataBaseView<T> {
        protected const int c_CurveQuality = 25;

        protected RavenAnimationDataSet<T> m_AnimationDataSetBase;
        protected SerializedObject m_AnimationDataSetSerializedObject;
        protected RavenParameterFieldView<T> m_StartValueView;
        protected RavenSequence m_Sequence;

        protected readonly Color[] m_Colors = new Color[] { Color.red, Color.green, Color.blue, Color.white };

        public override void Initialize(RavenAnimationPropertyBaseView<T> propertyView, RavenAnimationDataComponentBase animationData) {
            base.Initialize(propertyView, animationData);
            m_AnimationDataSetBase = animationData as RavenAnimationDataSet<T>;
            m_AnimationDataSetSerializedObject = new SerializedObject(m_AnimationDataSetBase);
            m_Sequence = RavenSequenceEditor.Instance.Sequence;

            m_StartValueView = new RavenParameterFieldView<T>(m_AnimationDataSetSerializedObject,
                m_AnimationDataSetSerializedObject.FindProperty("m_StartValueStart"),
                m_AnimationDataSetSerializedObject.FindProperty("m_StartValueEnd"),
                m_AnimationDataSetSerializedObject.FindProperty("m_StartValueType"),
                m_AnimationDataSetSerializedObject.FindProperty("m_StartParameterIndex"),
                m_AnimationDataSetSerializedObject.FindProperty("m_StartValueIsObjectLink"),
                m_AnimationDataSetSerializedObject.FindProperty("m_StartValueObjectLink"),
                m_PropertyBase.TargetComponent.GetType());
        }

        protected virtual void DrawAnimationData(Rect position) {
        }

        protected override void OnDrawGui(Rect position) {
        }

        protected override void OnDrawExtendedGui(Rect position) {
            float width = Mathf.Min(350f, Mathf.Max(position.width, 80f));
            const float offset = 20f;

            DrawAnimationData(position);
            m_StartValueView.DrawGui(new Rect(position.x, position.y + offset, width, position.height - offset));
        }

        protected override bool OnHandleInput(Vector2 mousePosition) {
            return false;
        }
    }
}