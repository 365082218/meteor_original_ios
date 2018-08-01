using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Outfit7.UI {

    // version >= 5.2.2 BaseMeshEffect - > ModifyMesh(VertexHelper vh)
    // version == 5.2.1 BaseMeshEffect - > ModifyMesh(Mesh mesh)
    // version == 5.1 BaseVertexEffect - > ModifyMesh(List<UIVertex> verts)
    // TODO: remove this legacy support once we migrate to 5.2.2 or more

    [AddComponentMenu("UI/Effects/Gradient")]

#if UNITY_5_1
    public class Gradient : BaseVertexEffect {
#else
    public class Gradient : BaseMeshEffect {
#endif

        public enum GradientMode {
            Global,
            Local
        }

        public enum GradientDirection {
            Vertical,
            Horizontal,
            DiagonalLeftToRight,
            DiagonalRightToLeft
            //Free
        }
        //enum color mode Additive, Multiply, Overwrite

        public GradientMode Mode = GradientMode.Global;
        public GradientDirection Direction = GradientDirection.Vertical;
        public bool OverwriteGraphicColor = false;
        public Color vertex1 = Color.white;
        public Color vertex2 = Color.black;

#if UNITY_5_1
    public override void ModifyVertices(List<UIVertex> vertexList) {
        if (!IsActive() || vertexList.Count == 0) {
            return;
        }
        int count = vertexList.Count;
        UIVertex uiVertex = vertexList[0];
        if (Mode == GradientMode.Global) {
            if (Direction == GradientDirection.DiagonalLeftToRight || Direction == GradientDirection.DiagonalRightToLeft) {


#if UNITY_EDITOR
                Debug.LogError("Diagonal dir is not supported in Global mode");
#endif
                Direction = GradientDirection.Vertical;
            }

            float topY = float.MaxValue;
            float bottomY = float.MinValue;

            if (Direction == GradientDirection.Vertical) {
                for (int i = 0; i < vertexList.Count; i++) {
                    uiVertex = vertexList[i];
                    topY = Mathf.Min(topY, uiVertex.position.y);
                    bottomY = Mathf.Max(bottomY, uiVertex.position.y);
                }
            } else {
                for (int i = 0; i < vertexList.Count; i++) {
                    uiVertex = vertexList[i];
                    topY = Mathf.Min(topY, uiVertex.position.x);
                    bottomY = Mathf.Max(bottomY, uiVertex.position.x);
                }
            }

            float uiElementHeight = bottomY - topY;

            for (int i = 0; i < count; i++) {
                uiVertex = vertexList[i];
                if (OverwriteGraphicColor) {
                    uiVertex.color = Color.white;
                }

                uiVertex.color *= Color.Lerp(vertex2, vertex1, ((Direction == GradientDirection.Vertical ? uiVertex.position.y : uiVertex.position.x) - topY) / uiElementHeight);
                vertexList[i] = uiVertex;
            }
        } else {
            for (int i = 0; i < count; i++) {
                uiVertex = vertexList[i];
                if (OverwriteGraphicColor) {
                    uiVertex.color = Color.white;
                }
                switch (Direction) {
                    case GradientDirection.Vertical:
                        uiVertex.color *= (i % 4 == 0 || (i - 1) % 4 == 0) ? vertex1 : vertex2;
                        break;
                    case GradientDirection.Horizontal:
                        uiVertex.color *= (i % 4 == 0 || (i - 3) % 4 == 0) ? vertex1 : vertex2;
                        break;
                    case GradientDirection.DiagonalLeftToRight:
                        uiVertex.color *= (i % 4 == 0) ? vertex1 : ((i - 2) % 4 == 0 ? vertex2 : Color.Lerp(vertex2, vertex1, 0.5f));
                        break;
                    case GradientDirection.DiagonalRightToLeft:
                        uiVertex.color *= ((i - 1) % 4 == 0) ? vertex1 : ((i - 3) % 4 == 0 ? vertex2 : Color.Lerp(vertex2, vertex1, 0.5f));
                        break;

                }
                vertexList[i] = uiVertex;
            }
        }
    }
#elif UNITY_5_2_1
        public override void ModifyMesh(Mesh mesh) {
            if (!enabled)
                return;
            List<Color> colors = new List<Color>(mesh.colors);
            int count = colors.Count;
            for (int i = 0; i < count; i++) {
                if (OverwriteGraphicColor) {
                    colors[i] = Color.white;
                }
            }

            if (Mode == GradientMode.Global) {
                if (Direction == GradientDirection.DiagonalLeftToRight || Direction == GradientDirection.DiagonalRightToLeft) {
#if UNITY_EDITOR
                    Debug.LogError("Diagonal dir is not supported in Global mode");
#endif
                    Direction = GradientDirection.Vertical;
                }

                float topY = float.MaxValue;
                float bottomY = float.MinValue;

                if (Direction == GradientDirection.Vertical) {
                    for (int i = 0; i < count; i++) {
                        topY = Mathf.Min(topY, mesh.vertices[i].y);
                        bottomY = Mathf.Max(bottomY, mesh.vertices[i].y);
                    }
                } else {
                    for (int i = 0; i < count; i++) {
                        topY = Mathf.Min(topY, mesh.vertices[i].x);
                        bottomY = Mathf.Max(bottomY, mesh.vertices[i].x);
                    }
                }

                float uiElementHeight = bottomY - topY;
                for (int i = 0; i < count; i++) {
                    colors[i] *= Color.Lerp(vertex2, vertex1, ((Direction == GradientDirection.Vertical ? mesh.vertices[i].y : mesh.vertices[i].x) - topY) / uiElementHeight);
                }
            } else {
                for (int i = 0; i < count; i++) {
                    switch (Direction) {
                        case GradientDirection.Vertical:
                            colors[i] *= (i % 4 == 0 || (i - 1) % 4 == 0) ? vertex1 : vertex2;
                            break;
                        case GradientDirection.Horizontal:
                            colors[i] *= (i % 4 == 0 || (i - 3) % 4 == 0) ? vertex1 : vertex2;
                            break;
                        case GradientDirection.DiagonalLeftToRight:
                            colors[i] *= (i % 4 == 0) ? vertex1 : ((i - 2) % 4 == 0 ? vertex2 : Color.Lerp(vertex2, vertex1, 0.5f));
                            break;
                        case GradientDirection.DiagonalRightToLeft:
                            colors[i] *= ((i - 1) % 4 == 0) ? vertex1 : ((i - 3) % 4 == 0 ? vertex2 : Color.Lerp(vertex2, vertex1, 0.5f));
                            break;

                    }
                }
            }
            mesh.SetColors(colors);
        }
#else
        public override void ModifyMesh(VertexHelper vh) {
        if (!IsActive() || vh.currentVertCount == 0) {
            return;
        }
        int count = vh.currentVertCount;
        UIVertex uiVertex = new UIVertex();
        if (Mode == GradientMode.Global) {
            if (Direction == GradientDirection.DiagonalLeftToRight || Direction == GradientDirection.DiagonalRightToLeft) {
#if UNITY_EDITOR
                Debug.LogError("Diagonal dir is not supported in Global mode");
#endif
                Direction = GradientDirection.Vertical;
            }
            UIVertex vertexBottom = new UIVertex();
            UIVertex vertexTop = new UIVertex();
            vh.PopulateUIVertex(ref vertexBottom, count - 1);
            vh.PopulateUIVertex(ref vertexTop, 0);

            float bottomY = Direction == GradientDirection.Vertical ? vertexBottom.position.y : vertexBottom.position.x;
            float topY = Direction == GradientDirection.Vertical ? vertexTop.position.y : vertexTop.position.x;

            float uiElementHeight = topY - bottomY;

            for (int i = 0; i < count; i++) {
                vh.PopulateUIVertex(ref uiVertex, i);
                if (OverwriteGraphicColor) {
                    uiVertex.color = Color.white;
                }

                uiVertex.color *= Color.Lerp(vertex2, vertex1, ((Direction == GradientDirection.Vertical ? uiVertex.position.y : uiVertex.position.x) - bottomY) / uiElementHeight);
                vh.SetUIVertex(uiVertex, i);
            }
        } else {
            for (int i = 0; i < count; i++) {
                vh.PopulateUIVertex(ref uiVertex, i);
                if (OverwriteGraphicColor) {
                    uiVertex.color = Color.white;
                }
                switch (Direction) {
                    case GradientDirection.Vertical:
                        uiVertex.color *= (i % 4 == 0 || (i - 1) % 4 == 0) ? vertex1 : vertex2;
                        break;
                    case GradientDirection.Horizontal:
                        uiVertex.color *= (i % 4 == 0 || (i - 3) % 4 == 0) ? vertex1 : vertex2;
                        break;
                    case GradientDirection.DiagonalLeftToRight:
                        uiVertex.color *= (i % 4 == 0) ? vertex1 : ((i - 2) % 4 == 0 ? vertex2 : Color.Lerp(vertex2, vertex1, 0.5f));
                        break;
                    case GradientDirection.DiagonalRightToLeft:
                        uiVertex.color *= ((i - 1) % 4 == 0) ? vertex1 : ((i - 3) % 4 == 0 ? vertex2 : Color.Lerp(vertex2, vertex1, 0.5f));
                        break;

                }
                vh.SetUIVertex(uiVertex, i);
            }
        }
    }
#endif
    }
}