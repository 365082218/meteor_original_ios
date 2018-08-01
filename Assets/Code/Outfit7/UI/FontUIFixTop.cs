using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Outfit7.UI {

    // version >= 5.2.2 BaseMeshEffect - > ModifyMesh(VertexHelper vh)
    // version == 5.2.1 BaseMeshEffect - > ModifyMesh(Mesh mesh)
    // version == 5.1 BaseVertexEffect - > ModifyMesh(List<UIVertex> verts)
    // TODO: remove this legacy support once we migrate to 5.2.2 or more

#if UNITY_5_1
    public class FontUIFixTop : BaseVertexEffect {
#else
    public class FontUIFixTop : BaseMeshEffect {
#endif

#if UNITY_5_1
        public override void ModifyVertices(List<UIVertex> verts) {
            int count = verts.Count;
            UIVertex uiVertex = new UIVertex();
            UIVertex uiVertex2 = new UIVertex();
            float refHeight = 0;
            float height = 0;
            for (int i = 0; i < count / 4; i++) {
                uiVertex = verts[i * 4];
                uiVertex2 = verts[i * 4 + 2];
                if (i == 0) {
                    refHeight = uiVertex2.uv0.y - uiVertex.uv0.y;
                    height = refHeight;
                } else {
                    height = uiVertex2.uv0.y - uiVertex.uv0.y;
                }
                for (int j = 0; j < 4; j++) {
                    uiVertex = verts[i * 4 + j];
                    uiVertex.uv1 = new Vector2(0, j == 0 || j == 1 ? height / refHeight : 0);
                    verts[i * 4 + j] = uiVertex;
                }
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
            int count = vh.currentVertCount;
            UIVertex uiVertex = new UIVertex();
            UIVertex uiVertex2 = new UIVertex();
            float refHeight = 0;
            float height = 0;
            for (int i = 0; i < count / 4; i++) {
                vh.PopulateUIVertex(ref uiVertex, i * 4);
                vh.PopulateUIVertex(ref uiVertex2, i * 4 + 2);
                if (i == 0) {
                    refHeight = uiVertex2.uv0.y - uiVertex.uv0.y;
                    height = refHeight;
                } else {
                    height = uiVertex2.uv0.y - uiVertex.uv0.y;
                }
                for (int j = 0; j < 4; j++) {

                    vh.PopulateUIVertex(ref uiVertex, i * 4 + j);
                    uiVertex.uv1 = new Vector2(0, j == 0 || j == 1 ? height / refHeight : 0);
                    vh.SetUIVertex(uiVertex, i * 4 + j);
                }
            }
        }

#endif
    }
}