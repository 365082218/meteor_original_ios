using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenAnimationDataTweenVector3TransformPositionView : RavenAnimationDataTweenVector3View {

        private Quaternion m_RecordSavedRotationValue;

        public override void Initialize(RavenAnimationPropertyBaseView<Vector3> propertyView, RavenAnimationDataComponentBase animationData) {
            base.Initialize(propertyView, animationData);

            m_StartValueView.SetSecondaryProperties(m_AnimationDataTweenSerializedObject.FindProperty("m_StartTangentStart"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_StartTangentEnd"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_StartTangentValueType"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_StartTangentParameterIndex"));

            m_EndValueView.SetSecondaryProperties(m_AnimationDataTweenSerializedObject.FindProperty("m_EndTangentStart"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_EndTangentEnd"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_EndTangentValueType"),
                m_AnimationDataTweenSerializedObject.FindProperty("m_EndTangentParameterIndex"));
        }

        protected override void OnDrawExtendedGui(Rect position) {
            base.OnDrawExtendedGui(position);

            const float toggleSize = 16f;
            const int nValues = 3;

            var drawGui = position.width > 100f;

            if (drawGui) {
                var toggleSizeWidth = position.width / nValues;
                if (toggleSizeWidth > toggleSize) {
                    toggleSizeWidth = toggleSize;
                }

                var tangentToggleRect = new Rect(position.x + position.width / 2f - toggleSizeWidth * nValues / 2f, position.y + position.height - toggleSize * 2f, toggleSizeWidth, toggleSize);
                tangentToggleRect.x += toggleSizeWidth;

                var showTangents = m_StartValueView.ShowSecondary;
                showTangents = EditorGUI.Toggle(tangentToggleRect, "", showTangents);
                m_StartValueView.ShowSecondary = m_EndValueView.ShowSecondary = showTangents;

                DrawCameras(position);
            }
        }

        private void DrawCameras(Rect position) {
        }

        protected override void OnRecordStart() {
            base.OnRecordStart();
            m_RecordSavedRotationValue = (m_PropertyViewBase.PropertyBase.TargetComponent as Transform).rotation;
        }

        protected override void OnRecordEnd() {
            base.OnRecordEnd();
            (m_PropertyViewBase.PropertyBase.TargetComponent as Transform).rotation = m_RecordSavedRotationValue;
        }
    }
}