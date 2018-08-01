using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public abstract class RavenAnimationDataBaseView<T> {
        protected RavenAnimationPropertyBaseView<T> m_PropertyViewBase;
        protected RavenAnimationDataBase<T> m_AnimationDataBase;
        protected RavenAnimationPropertyBase<T> m_PropertyBase;

        public virtual void Initialize(RavenAnimationPropertyBaseView<T> propertyView, RavenAnimationDataComponentBase animationData) {
            m_PropertyViewBase = propertyView;
            m_AnimationDataBase = animationData as RavenAnimationDataBase<T>;
            m_PropertyBase = propertyView.PropertyBase;
        }

        public void DrawGui(Rect position) {
            Undo.RecordObject(m_AnimationDataBase, "DrawGui");
            OnDrawGui(position);
        }

        public void DrawExtendedGui(Rect position) {
            Undo.RecordObject(m_AnimationDataBase, "DrawExtendedGui");
            OnDrawExtendedGui(position);
        }

        public bool HandleInput(Vector2 mousePosition) {
            Undo.RecordObject(m_AnimationDataBase, "HandleInput");
            return OnHandleInput(mousePosition);
        }

        public void RecordStart() {
            if (m_AnimationDataBase == null) {
                return;
            }

            Undo.RecordObject(m_AnimationDataBase, "RecordStart");
            OnRecordStart();
        }

        public void RecordEnd() {
            if (m_AnimationDataBase == null) {
                return;
            }

            Undo.RecordObject(m_AnimationDataBase, "RecordEnd");
            OnRecordEnd();
        }

        protected abstract void OnDrawGui(Rect position);

        protected abstract void OnDrawExtendedGui(Rect position);

        protected abstract bool OnHandleInput(Vector2 mousePosition);

        protected virtual void OnRecordStart() {
        }

        protected virtual void OnRecordEnd() {
        }
    }
}