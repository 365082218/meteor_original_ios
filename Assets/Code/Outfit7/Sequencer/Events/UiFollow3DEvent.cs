using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Graphics;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class UiFollow3DEvent : SequencerContinuousEvent {
        public ParameterVectorField PointField = new ParameterVectorField(Vector4.zero);
        public ParameterComponentField FromCamera = new ParameterComponentField(null);
        public ParameterComponentField ToCamera = new ParameterComponentField(null);
        public float ToCameraDepth = 10f;

        public override void OnInit() {
            ComponentType = typeof(UnityEngine.RectTransform);
        }

        protected override void OnLiveInit(SequencerSequence sequence) {
            PointField.LiveInit(sequence.Parameters);
            FromCamera.LiveInit(sequence.Parameters);
            ToCamera.LiveInit(sequence.Parameters);
        }

        public override void OnEnter(List<Component> components, float absoluteTime, float normalizedTime) {
            PointField.Init(null);
            FromCamera.Init(null);
            ToCamera.Init(null);
        }

        public override void OnPreplay() {
            for (int i = 0; i < Objects.Count; i++) {
                OnEnter(Objects[i].Components, 0, 0);
                OnProcess(Objects[i].Components, 0, 0);
            }
        }

        public override void OnProcess(List<Component> components, float absoluteTime, float normalizedTime) {
            for (int i = 0; i < components.Count; i++) {
                RectTransform rectTransform = components[i] as RectTransform;
                Camera FromCameraCast = FromCamera.Value as Camera;
                Camera ToCameraCast = ToCamera.Value as Camera;

                if (FromCameraCast == null)
                    return;
                if (ToCameraCast == null)
                    return;
                if (rectTransform == null)
                    return;

                Vector3 screenPoint = FromCameraCast.WorldToScreenPoint(PointField.Value);
                Vector3 outPoint = ToCameraCast.ScreenToWorldPoint(screenPoint);
                rectTransform.position = new Vector3(outPoint.x, outPoint.y, rectTransform.position.z);
            }
        }

        public override void OnExit(List<Component> components) {
        }
    }
}