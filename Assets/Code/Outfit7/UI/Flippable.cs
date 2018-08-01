using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Outfit7.UI {

    // version >= 5.2.2 BaseMeshEffect - > ModifyMesh(VertexHelper vh)
    // version == 5.2.1 BaseMeshEffect - > ModifyMesh(Mesh mesh)
    // version == 5.1 BaseVertexEffect - > ModifyMesh(List<UIVertex> verts)
    // TODO: remove this legacy support once we migrate to 5.2.2 or more

    [RequireComponent(typeof(RectTransform)), RequireComponent(typeof(Graphic)), DisallowMultipleComponent, AddComponentMenu("UI/Flippable")]

#if UNITY_5_1
    public class Flippable : BaseVertexEffect {
#else
    public class Flippable : BaseMeshEffect {
#endif
        [SerializeField]
        private bool m_Horizontal = false;
        [SerializeField]
        private bool m_Veritical = false;

        public bool horizontal {
            get { return m_Horizontal; }
            set {
                m_Horizontal = value;
                graphic.SetVerticesDirty();
            }
        }

        public bool vertical {
            get { return m_Veritical; }
            set {
                m_Veritical = value;
                graphic.SetVerticesDirty();
            }
        }

#if UNITY_EDITOR
#if UNITY_5
        protected override void OnValidate() {
            base.OnValidate();
            graphic.SetVerticesDirty();
        }
#else
                protected void OnValidate() {
                    graphic.SetVerticesDirty();
                }
#endif
#endif


#if UNITY_5_1
        public override void ModifyVertices(List<UIVertex> verts) {
            RectTransform rt = transform as RectTransform;
            for (int i = 0; i < verts.Count; ++i) {
                UIVertex v = verts[i];
                // Modify positions
                v.position = new Vector3(
                    (m_Horizontal ? (v.position.x + (rt.rect.center.x - v.position.x) * 2) : v.position.x),
                    (m_Veritical ? (v.position.y + (rt.rect.center.y - v.position.y) * 2) : v.position.y),
                    v.position.z
                );
                // Apply
                verts[i] = v;
            }
        }
#elif UNITY_5_2_1
        public override void ModifyMesh(Mesh mesh) {
            RectTransform rt = transform as RectTransform;
            List<Vector3> verts = new List<Vector3>(mesh.vertices);
            for (int i = 0; i < verts.Count; ++i) {
                Vector3 v = verts[i];
                // Modify positions
                v.x = m_Horizontal ? (v.x + (rt.rect.center.x - v.x) * 2) : v.x;
                v.y = m_Veritical ? (v.y + (rt.rect.center.y - v.y) * 2) : v.y;
                verts[i] = v;
            }
            mesh.SetVertices(verts);
        }
#else
        public override void ModifyMesh(VertexHelper vh) {
            RectTransform rt = transform as RectTransform;
            int count = vh.currentVertCount;
            UIVertex uiVertex = new UIVertex();
            for (int i = 0; i < count; ++i) {
                vh.PopulateUIVertex(ref uiVertex, i);
                // Modify positions
                uiVertex.position = new Vector3(
                    (m_Horizontal ? (uiVertex.position.x + (rt.rect.center.x - uiVertex.position.x) * 2) : uiVertex.position.x),
                    (m_Veritical ? (uiVertex.position.y + (rt.rect.center.y - uiVertex.position.y) * 2) : uiVertex.position.y),
                    uiVertex.position.z
                );
                // Apply
                vh.SetUIVertex(uiVertex, i);
            }
        }
#endif
    }
}