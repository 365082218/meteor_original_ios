#if !UNITY_5_3_OR_NEWER
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Outfit7.UI {

    // version >= 5.2.2 BaseMeshEffect - > ModifyMesh(VertexHelper vh)
    // version == 5.2.1 BaseMeshEffect - > ModifyMesh(Mesh mesh)
    // version == 5.1 BaseVertexEffect - > ModifyMesh(List<UIVertex> verts)
    // TODO: Fix this file for Unity 5.3

    [AddComponentMenu("UI/Effects/Glow", 40)]

#if UNITY_5_1
    public class Glow : BaseVertexEffect {
#else
    public class Glow : BaseMeshEffect {
#endif

        [SerializeField] private Color m_EffectColor = new Color(0.996f, 0.9915f, 1f, 0.61f);
        [SerializeField] private float m_EffectSize = 0.8f;
        [SerializeField] private bool m_UseGraphicAlpha = true;
        private UnityEngine.UI.Text Text;

#if UNITY_EDITOR
        protected override void OnEnable() {
            base.OnEnable();
            Text = GetTextComponent();
        }
#endif

        public Color effectColor {
            get { return m_EffectColor; }
            set {
                m_EffectColor = value;
                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        protected override void Awake() {
            base.Awake();
            Text = GetTextComponent();
        }

        private UnityEngine.UI.Text GetTextComponent() {
            return GetComponent<UnityEngine.UI.Text>();
        }


#if UNITY_5_1
        protected void ApplyGlow(List<UIVertex> verts, Color32 color, int start, int end) {
            if (!enabled || color.a < 0.001f)
                return;

            UIVertex vt = new UIVertex();
            //            Vector2 lastPosition = new Vector2();
            float effectSize = m_EffectSize;
            var neededCpacity = verts.Count;
            if (verts.Capacity < neededCpacity)
                verts.Capacity = neededCpacity;

            for (int i = start; i < end; ++i) {
                int quadIndex = i % 4;
                vt = verts[i];
                if (quadIndex == 0) {
                    if (Vector2.Distance(vt.position, verts[i + 1].position) < Text.fontSize * 0.10f) {
                        i += 3;
                        continue;
                    }
                }
                verts.Add(vt);

                Vector3 v = new Vector3(0, 0, 0);
                Vector3 uv = new Vector2(0, 0);
                if (quadIndex == 0) {
                    v = new Vector3(vt.position.x - effectSize, vt.position.y + effectSize, vt.position.z);
                    uv = new Vector2(1, 1);
                } else if (quadIndex == 1) {
                    v = new Vector3(vt.position.x + effectSize, vt.position.y + effectSize, vt.position.z);
                    uv = new Vector2(2, 1);
                } else if (quadIndex == 2) {
                    v = new Vector3(vt.position.x + effectSize, vt.position.y - effectSize, vt.position.z);
                    uv = new Vector2(2, 0);
                } else if (quadIndex == 3) {
                    v = new Vector3(vt.position.x - effectSize, vt.position.y - effectSize, vt.position.z);
                    uv = new Vector2(1, 0);
                }
                vt.position = v;
                vt.uv0 = uv;
                if (m_UseGraphicAlpha) {
                    Color32 newColor = color;
                    newColor.a = (byte) ((newColor.a * verts[i].color.a) / 255);
                    vt.color = newColor;
                } else {
                    vt.color = color;
                }
                verts[i] = vt;
            }
        }

        public override void ModifyVertices(List<UIVertex> verts) {
            ApplyGlow(verts, effectColor, 0, verts.Count);
        }

#elif UNITY_5_2_1
        private void ApplyColor(ref Color32 vertexColor, Color32 color) {
            Color32 outColor = Color.white;
            if (m_UseGraphicAlpha) {
                Color32 newColor = color;
                newColor.a = (byte) ((newColor.a * vertexColor.a) / 255);
                outColor = newColor;
            } else {
                outColor = color;
            }
            vertexColor = outColor;
        }

        protected void ApplyGlow(Vector3[] vertices, Vector2[] uvs, int[] triangles, Color32[] colors, Color32 color, int originalVertexCount) {
            float effectSize = m_EffectSize;
            int vertexCount = (originalVertexCount / 4) * 4;
            int oldVertexIndex = originalVertexCount;

            for (int i = 0; i < vertexCount; i += 4, oldVertexIndex += 4) {
                Vector3 ov0 = vertices[oldVertexIndex + 0];
                Vector3 ov1 = vertices[oldVertexIndex + 1];
                if (Vector2.Distance(ov0, ov1) < Text.fontSize * 0.10f) {
                    continue;
                }
                Vector3 ov2 = vertices[oldVertexIndex + 2];
                Vector3 ov3 = vertices[oldVertexIndex + 3];

                Color32 c0 = colors[oldVertexIndex + 0];
                Color32 c1 = colors[oldVertexIndex + 1];
                Color32 c2 = colors[oldVertexIndex + 2];
                Color32 c3 = colors[oldVertexIndex + 3];

                Vector3 v0 = new Vector3(ov0.x - effectSize, ov0.y + effectSize, ov0.z);
                Vector2 uv0 = new Vector2(1, 1);

                Vector3 v1 = new Vector3(ov1.x + effectSize, ov1.y + effectSize, ov1.z);
                Vector2 uv1 = new Vector2(2, 1);

                Vector3 v2 = new Vector3(ov2.x + effectSize, ov2.y - effectSize, ov2.z);
                Vector2 uv2 = new Vector2(2, 0);

                Vector3 v3 = new Vector3(ov3.x - effectSize, ov3.y - effectSize, ov3.z);
                Vector2 uv3 = new Vector2(1, 0);

                ApplyColor(ref c0, color);
                ApplyColor(ref c1, color);
                ApplyColor(ref c2, color);
                ApplyColor(ref c3, color);

                vertices[i + 0] = v0;
                vertices[i + 1] = v1;
                vertices[i + 2] = v2;
                vertices[i + 3] = v3;

                uvs[i + 0] = uv0;
                uvs[i + 1] = uv1;
                uvs[i + 2] = uv2;
                uvs[i + 3] = uv3;

                colors[i + 0] = c0;
                colors[i + 1] = c1;
                colors[i + 2] = c2;
                colors[i + 3] = c3;
            }

        }

        public override void ModifyMesh(Mesh mesh) {
            if (!enabled || effectColor.a < 0.001f)
                return;

            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = mesh.uv;
            int[] triangles = mesh.triangles;
            Color32[] colors = mesh.colors32;

            Vector3[] newVertices = new Vector3[vertices.Length * 2];
            Vector2[] newUvs = new Vector2[uvs.Length * 2];
            int[] newTriangles = new int[triangles.Length * 2];
            Color32[] newColors = new Color32[colors.Length * 2];

            for (int i = 0; i < vertices.Length; i++) {
                newVertices[i + vertices.Length] = vertices[i];
                newUvs[i + vertices.Length] = uvs[i];
                newColors[i + vertices.Length] = colors[i];
            }

            for (int i = 0; i < triangles.Length; i++) {
                newTriangles[i + triangles.Length] = triangles[i] + vertices.Length;
            }
            for (int i = 0; i < triangles.Length; i++) {
                newTriangles[i] = triangles[i];
            }

            ApplyGlow(newVertices, newUvs, newTriangles, newColors, effectColor, vertices.Length);

            mesh.vertices = newVertices;
            mesh.uv = newUvs;
            mesh.colors32 = newColors;
            mesh.triangles = newTriangles;
            mesh.RecalculateBounds();
        }
#else
        public override void ModifyMesh(VertexHelper vh) {
            throw new NotImplementedException();
        }
#endif
    }
}
#endif