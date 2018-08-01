using UnityEngine;
using System.Collections.Generic;

namespace Outfit7.UI {

    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasRenderer))]
    public class CanvasMeshRenderer : UnityEngine.EventSystems.UIBehaviour {

        [SerializeField] private Mesh Mesh = null;
        [SerializeField] private Material MaterialPrefab = null;
        [SerializeField] private Texture Texture = null;
#if UNITY_5_1
        [SerializeField] private Color Color = Color.white;
#endif
        [SerializeField] private Material MaterialInstance = null;
        [SerializeField] private float Scale = 1000f;
        [SerializeField] private bool SnapRectangle = true;

        private CanvasRenderer CanvasRenderer;
#if UNITY_5_1
        private List<UIVertex> UIVertices = new List<UIVertex>();
#endif


        public Material Material { 
            get { 
                MaterialCheck();
                return MaterialInstance; 
            }
        }

        public Mesh GetMesh() {
            return Mesh;
        }

        public Texture GetTexture() {
            return Texture;
        }

        public void SetMesh(Mesh m) {
            Mesh = m;
            Refresh();
        }

        public void SetMaterial(Material mat) {
            if (MaterialPrefab != mat) {
                MaterialInstance = Instantiate(mat);
            }

            CanvasRenderer.SetMaterial(MaterialInstance, Texture);
        }

        public void SetTexture(Texture tex) {
            Texture = tex;
            CanvasRenderer.SetMaterial(MaterialInstance, tex);
        }

        public void SetMaterialAndTexture(Material mat, Texture tex) {
            if (MaterialPrefab != mat) {
                MaterialInstance = Instantiate(mat);
            }
            Texture = tex;
            CanvasRenderer.SetMaterial(MaterialInstance, tex);
        }

        public void SetColor(Color color) {
#if UNITY_5_1
            Color = color;
#endif
            if (CanvasRenderer == null) {
                CanvasRenderer = GetComponent<CanvasRenderer>();
            }
            CanvasRenderer.SetColor(color);
        }

        public void Refresh() {
            if (CanvasRenderer == null) {
                CanvasRenderer = GetComponent<CanvasRenderer>();
            }

            if (!IsActive()) {
                CanvasRenderer.Clear();
                return;
            }

            MaterialCheck();

            if (Mesh == null) {
                return;
            }

#if !UNITY_5_1
            Mesh mesh = Instantiate(Mesh) as Mesh;
            Vector3[] vertices = mesh.vertices;

            RectTransform rectTransform = transform as RectTransform;

            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] *= Scale;
            }

            mesh.vertices = vertices;


            if (rectTransform != null && SnapRectangle) {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs((Mesh.bounds.max.x) * Scale * 2));
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs((Mesh.bounds.max.y) * Scale * 2));
            }

            mesh.RecalculateBounds();
            CanvasRenderer.Clear();
            CanvasRenderer.SetMesh(mesh);
#else
            Vector3[] vertices = Mesh.vertices;
            Vector2[] uvs = Mesh.uv;
            int[] triangles = Mesh.triangles;

            UIVertices.Clear();

            RectTransform rectTransform = transform as RectTransform;

            for (int i = 0; i < triangles.Length; i++) {
                UIVertex vert = new UIVertex();

                vert.position = vertices[triangles[i]] * Scale;
                vert.uv0 = uvs[triangles[i]];
                vert.color = Color;

                UIVertices.Add(vert);
                if (i % 3 == 0) {
                    UIVertices.Add(vert);
                }
            }

            if (rectTransform != null && SnapRectangle) {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs((Mesh.bounds.max.x) * Scale * 2));
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs((Mesh.bounds.max.y) * Scale * 2));
            }

            CanvasRenderer.Clear();
            CanvasRenderer.SetVertices(UIVertices);
#endif
            if (MaterialInstance != null) {
                CanvasRenderer.SetMaterial(MaterialInstance, Texture);
            }
        }

        private void MaterialCheck() {
            if (MaterialInstance == null) {
                if (MaterialPrefab == null) {
                    MaterialInstance = null;
                } else {
                    MaterialInstance = Instantiate(MaterialPrefab);
                }
            }
        }

        // UIBehaviour
        protected override void Awake() {
            base.Awake();

            MaterialInstance = null;
            Refresh();
        }

        protected override void OnEnable() {
            base.OnEnable();

            Refresh();
        }

        protected override void OnDisable() {
            base.OnDisable();

            CanvasRenderer.Clear();
        }

#if UNITY_EDITOR
        private void Update() {
            if (!Application.isPlaying) {
                Refresh();
            }
        }
#endif
    }
}