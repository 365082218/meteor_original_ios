using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenShaderGlobalPropertyBaseView<T> : RavenAnimationPropertyBaseView<T> {
        protected RavenShaderGlobalPropertyBase<T> m_ShaderProperty;

        public override void Initialize(RavenEventView eventView, RavenPropertyComponent property) {
            base.Initialize(eventView, property);
            m_ShaderProperty = property as RavenShaderGlobalPropertyBase<T>;
        }

        protected override void OnDrawExtendedGui(Rect position) {
            base.OnDrawExtendedGui(position);

            const float kLineSize = 16f;
            const float kStringSize = 100f;
            const float kHeightOffset = kLineSize;

            var y = position.yMax - kLineSize - kHeightOffset;

            m_ShaderProperty.TargetShaderProperty = EditorGUI.TextField(new Rect(position.x + 3f, y, Mathf.Min(kStringSize, position.width), kLineSize), m_ShaderProperty.TargetShaderProperty);
        }
    }
}