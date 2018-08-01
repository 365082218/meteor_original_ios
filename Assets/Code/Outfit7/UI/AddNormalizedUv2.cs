using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Outfit7.UI {

    // version >= 5.2.2 BaseMeshEffect - > ModifyMesh(VertexHelper vh)
    // version == 5.2.1 BaseMeshEffect - > ModifyMesh(Mesh mesh)
    // version == 5.1 BaseVertexEffect - > ModifyMesh(List<UIVertex> verts)
    // TODO: remove this legacy support once we migrate to 5.2.2 or more

#if UNITY_5_1
    public class AddNormalizedUv2 : BaseVertexEffect {
#else
    public class AddNormalizedUv2 : BaseMeshEffect {
#endif

#if UNITY_5_1
        public override void ModifyVertices(List<UIVertex> verts) {
            Rect rect = graphic.rectTransform.rect;
            float minY = rect.min.y;
            float maxY = rect.max.y;
            float minX = rect.min.x;
            float maxX = rect.max.x;
            for (int i = 0; i < verts.Count; i++) {
                UIVertex uiVertex = verts[i];
                uiVertex.uv1 = new Vector2(Mathf.InverseLerp(minX, maxX, uiVertex.position.x), Mathf.InverseLerp(minY, maxY, uiVertex.position.y));
                verts[i] = uiVertex;
            }
        }
#elif UNITY_5_2_1
        public override void ModifyMesh(Mesh mesh) {
            Bounds bounds = mesh.bounds;
            float minY = bounds.min.y;
            float maxY = bounds.max.y;
            float minX = bounds.min.x;
            float maxX = bounds.max.x;
            Vector3[] vertices = mesh.vertices;
            Vector2[] uv2 = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) {
                uv2[i] = new Vector2(Mathf.InverseLerp(minX, maxX, vertices[i].x), Mathf.InverseLerp(minY, maxY, vertices[i].y));
            }
            mesh.uv2 = uv2;
        }
#else
        public override void ModifyMesh(VertexHelper vh) {
            Rect rect = graphic.rectTransform.rect;
            float minY = rect.min.y;
            float maxY = rect.max.y;
            float minX = rect.min.x;
            float maxX = rect.max.x;
            int count = vh.currentVertCount;
            UIVertex uiVertex = new UIVertex();
            for (int i = 0; i < count; i++) {
                vh.PopulateUIVertex(ref uiVertex, i);
                uiVertex.uv1 = new Vector2(Mathf.InverseLerp(minX, maxX, uiVertex.position.x), Mathf.InverseLerp(minY, maxY, uiVertex.position.y));
                vh.SetUIVertex(uiVertex, i);
            }
        }
#endif
    }
}