using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO: remove this legacy support once we migrate to 5.2.2 or more

[AddComponentMenu("UI/TouchRectTransform", 55)]
public class TouchRectTransform : UnityEngine.UI.MaskableGraphic, ICanvasRaycastFilter {

#if UNITY_5_1
    protected override void OnFillVBO(List<UIVertex> vbo) {
        // leave vbo empty
    }
#elif UNITY_5_2_1
    protected override void OnPopulateMesh(Mesh m) {
        m.Clear();
    }
#else
    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();
    }
#endif

    public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
        return isActiveAndEnabled;
    }

#if UNITY_EDITOR
    public void OnDrawGizmosSelected() {
        if (!canvas || canvas.renderMode != RenderMode.WorldSpace) {
            return;
        }
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(rectTransform.rect.center, rectTransform.rect.size);
    }
#endif
}