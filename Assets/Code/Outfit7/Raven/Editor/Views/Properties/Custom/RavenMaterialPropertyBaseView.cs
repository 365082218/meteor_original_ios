using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenMaterialPropertyBaseView<T> : RavenAnimationPropertyBaseView<T> {
        protected RavenMaterialPropertyBase<T> m_MaterialProperty;

        public override void Initialize(RavenEventView eventView, RavenPropertyComponent property) {
            base.Initialize(eventView, property);
            m_MaterialProperty = property as RavenMaterialPropertyBase<T>;
        }

        protected override void OnDrawExtendedGui(Rect position) {
            base.OnDrawExtendedGui(position);

            const float kLineSize = 16f;
            const float kStringSize = 100f;
            const float kIndexSize = 32f;
            const float kSpacing = 3f;
            const float kHeightOffset = kLineSize;

            var y = position.yMax - kLineSize - kHeightOffset;

            var totalWidth = 0f;
            var rect = new Rect(position.x, y, Mathf.Min(kLineSize, position.width), kLineSize);
            totalWidth += rect.width;
            m_MaterialProperty.UseSharedMaterial = EditorGUI.Toggle(rect, m_MaterialProperty.UseSharedMaterial);

            var newWidth = Mathf.Min(kIndexSize, position.width - totalWidth);
            rect.x += rect.width;
            rect.width = newWidth;
            totalWidth += rect.width;
            m_MaterialProperty.TargetMaterialIndex = EditorGUI.IntField(rect, m_MaterialProperty.TargetMaterialIndex);

            newWidth = Mathf.Min(kStringSize, position.width - totalWidth - kSpacing);
            rect.x += rect.width + kSpacing;
            rect.width = newWidth;
            m_MaterialProperty.TargetMaterialProperty = EditorGUI.TextField(rect, m_MaterialProperty.TargetMaterialProperty);
        }
    }
}