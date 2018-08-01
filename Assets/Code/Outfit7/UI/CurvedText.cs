using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Outfit7.UI {

    // version >= 5.2.2 BaseMeshEffect - > ModifyMesh(VertexHelper vh)
    // version == 5.2.1 BaseMeshEffect - > ModifyMesh(Mesh mesh)
    // version == 5.1 BaseVertexEffect - > ModifyMesh(List<UIVertex> verts)
    // TODO: remove this legacy support once we migrate to 5.2.2 or more

    [RequireComponent(typeof(UnityEngine.UI.Text), typeof(RectTransform))]

#if UNITY_5_1
    public class CurvedText : BaseVertexEffect {
#else
    public class CurvedText : BaseMeshEffect {
#endif
        public AnimationCurve AnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [Range(-5, 5)]
        public float CurveMultiplier = 1f;
        private RectTransform RectTransform;

        #if UNITY_EDITOR
        protected void Update() {
            if (AnimationCurve[0].time != 0) {
                Keyframe tmpRect = AnimationCurve[0];
                tmpRect.time = 0;
                AnimationCurve.MoveKey(0, tmpRect);
            }
            if (AnimationCurve[AnimationCurve.length - 1].time != RectTransform.rect.width) {
                OnRectTransformDimensionsChange();
            }
        }
        #endif
        private void SetRectTransform() {
            if (RectTransform == null) {
                RectTransform = GetComponent<RectTransform>();
            }
        }

        protected override void Awake() {
            base.Awake();
            OnRectTransformDimensionsChange();
        }

        protected override void OnEnable() {
            base.OnEnable();
            OnRectTransformDimensionsChange();
        }

#if UNITY_5_1
        public override void ModifyVertices(List<UIVertex> verts) {
            if (!IsActive())
                return;
		
            // TODO: don't screw the texture, just rotate around its center
            for (int index = 0; index < verts.Count; index++) {
                UIVertex uiVertex = verts[index];
                uiVertex.position.y += AnimationCurve.Evaluate(RectTransform.rect.width * RectTransform.pivot.x + uiVertex.position.x) * CurveMultiplier;
                verts[index] = uiVertex;
            }
        }
#elif UNITY_5_2_1
        public override void ModifyMesh(Mesh mesh) {
            List<Vector3> vertices = new List<Vector3>(mesh.vertices);
            for (int a = 0; a < vertices.Count; a++) {
                Vector3 v = vertices[a];
                v.y += AnimationCurve.Evaluate(RectTransform.rect.width * RectTransform.pivot.x + v.x) * CurveMultiplier;
                vertices[a] = v;
            }
            mesh.SetVertices(vertices);
        }
#else
        public override void ModifyMesh(VertexHelper vh) {
            // TODO: don't screw the texture, just rotate around its center
            int count = vh.currentVertCount;
            UIVertex uiVertex = new UIVertex();
            for (int i = 0; i < count; i++) {
                vh.PopulateUIVertex(ref uiVertex, i);
                uiVertex.position.y += AnimationCurve.Evaluate(RectTransform.rect.width * RectTransform.pivot.x + uiVertex.position.x) * CurveMultiplier;
                vh.SetUIVertex(uiVertex, i);
            }
        }
#endif
        protected override void OnRectTransformDimensionsChange() {
            SetRectTransform();
            if (AnimationCurve == null || AnimationCurve.length == 0)
                return;
            Keyframe keyFrame = AnimationCurve[AnimationCurve.length - 1];
            keyFrame.time = RectTransform.rect.width;
            AnimationCurve.MoveKey(AnimationCurve.length - 1, keyFrame);
        }
    }
}