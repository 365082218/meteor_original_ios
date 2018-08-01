using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Graphics;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class SequencerRectTargetTweenEvent : SequencerContinuousEvent {
        public EaseManager.Ease EaseType;
        public ParameterComponentField FromComponentField = new ParameterComponentField(null);
        public ParameterComponentField ToComponentField = new ParameterComponentField(null);

        public ParameterVectorField FromPositionField = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField ToPositionField = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField FromRotationField = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField ToRotationField = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField FromScaleField = new ParameterVectorField(Vector4.one);
        public ParameterVectorField ToScaleField = new ParameterVectorField(Vector4.one);
        public ParameterVectorField FromSizeField = new ParameterVectorField(Vector4.one);
        public ParameterVectorField ToSizeField = new ParameterVectorField(Vector4.one);
        public Camera FromCamera;
        public Camera ToCamera;
        public float ToCameraDepth = 10f;
        public bool AffectPosition = true;
        public bool AffectRotation = true;
        public bool AffectScale = true;
        public bool AffectSize = true;
        public ParameterBoolField UseBezierCurve = new ParameterBoolField(false);
        public ParameterVectorField TangentStart = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField TangentEnd = new ParameterVectorField(Vector4.zero);
        private RectTransform FromTransform;
        private RectTransform ToTransform;

        public override void OnInit() {
            ComponentType = typeof(UnityEngine.RectTransform);
        }

        protected override void OnLiveInit(SequencerSequence sequence) {
            FromComponentField.LiveInit(sequence.Parameters);
            ToComponentField.LiveInit(sequence.Parameters);
            FromPositionField.LiveInit(sequence.Parameters);
            ToPositionField.LiveInit(sequence.Parameters);
            FromRotationField.LiveInit(sequence.Parameters);
            ToRotationField.LiveInit(sequence.Parameters);
            FromScaleField.LiveInit(sequence.Parameters);
            ToScaleField.LiveInit(sequence.Parameters);
            FromSizeField.LiveInit(sequence.Parameters);
            ToSizeField.LiveInit(sequence.Parameters);
            UseBezierCurve.LiveInit(sequence.Parameters);
            TangentStart.LiveInit(sequence.Parameters);
            TangentEnd.LiveInit(sequence.Parameters);
        }

        public override void OnEnter(List<Component> components, float absoluteTime, float normalizedTime) {
            FromComponentField.Init(null);
            FromTransform = FromComponentField.Value as RectTransform;
            ToComponentField.Init(null);
            ToTransform = ToComponentField.Value as RectTransform;

            if (FromComponentField.FieldType == ParameterFieldType.SOLID && components.Count > 0) {
                FromPositionField.FieldType = ParameterFieldType.SOLID;
                FromRotationField.FieldType = ParameterFieldType.SOLID;
                FromScaleField.FieldType = ParameterFieldType.SOLID;
                FromSizeField.FieldType = ParameterFieldType.SOLID;
            }

            if (FromComponentField.FieldType == ParameterFieldType.CURRENT && components.Count > 0) {
                RectTransform fromTransform = components[0] as RectTransform;
                FromPositionField.FieldType = ParameterFieldType.CURRENT;
                FromRotationField.FieldType = ParameterFieldType.CURRENT;
                FromScaleField.FieldType = ParameterFieldType.CURRENT;
                FromSizeField.FieldType = ParameterFieldType.CURRENT;

                FromPositionField.Init((Vector4) fromTransform.position);
                FromRotationField.Init(new Vector4(fromTransform.rotation.x, fromTransform.rotation.y, fromTransform.rotation.z, fromTransform.rotation.w));
                FromScaleField.Init((Vector4) fromTransform.localScale);
                FromSizeField.Init((Vector4) fromTransform.sizeDelta);
            } else {
                FromPositionField.Init(null);
                FromRotationField.Init(null);
                FromScaleField.Init(null);
                FromSizeField.Init(null);
            }
            ToPositionField.Init(null);
            ToRotationField.Init(null);
            ToScaleField.Init(null);
            ToSizeField.Init(null);
            UseBezierCurve.Init(null);
            TangentStart.Init(null);
            TangentEnd.Init(null);
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

                if (rectTransform == null)
                    return;
                Vector3 toPosition;

                float tweenValue = EaseManager.Evaluate(EaseType, absoluteTime, Duration, 1, 1);
                if (ToTransform != null) {
                    toPosition = ToTransform.position;
                } else {
                    toPosition = ToPositionField.Value;
                }
                if (FromCamera != null && ToCamera != null) {
                    Vector3 screenPoint = ToCamera.WorldToScreenPoint(toPosition);
                    screenPoint.z = ToCameraDepth;
                    toPosition = FromCamera.ScreenToWorldPoint(screenPoint);
                }

                Vector3 fromPosition;
                Quaternion fromRotation;
                Quaternion toRotation;
                Vector3 fromScale;
                Vector3 toScale;
                Vector2 fromSize;
                Vector2 toSize;

                if (FromTransform != null) {
                    fromPosition = FromTransform.position;
                    fromRotation = FromTransform.rotation;
                    fromScale = FromTransform.localScale;
                    fromSize = FromTransform.sizeDelta;
                } else {
                    fromPosition = FromPositionField.Value;
                    fromRotation = Quaternion.Euler(FromRotationField.Value);
                    fromScale = FromScaleField.Value;
                    fromSize = FromSizeField.Value;
                }
                if (ToTransform != null) {
                    toRotation = ToTransform.rotation;
                    toScale = ToTransform.localScale;
                    toSize = ToTransform.sizeDelta;
                } else {
                    toRotation = Quaternion.Euler(ToRotationField.Value);
                    toScale = ToScaleField.Value;
                    toSize = ToSizeField.Value;
                }

                if (AffectPosition) {
                    if (UseBezierCurve.Value)
                        rectTransform.position = Interpolation.BezierCubicCurve(fromPosition, fromPosition + (Vector3) TangentStart.Value, toPosition + (Vector3) TangentEnd.Value, toPosition, tweenValue);
                    else
                        rectTransform.position = Vector3.LerpUnclamped(fromPosition, toPosition, tweenValue);
                }
                if (AffectRotation)
                    rectTransform.rotation = Quaternion.LerpUnclamped(fromRotation, toRotation, tweenValue);
                if (AffectScale)
                    rectTransform.localScale = Vector3.LerpUnclamped(fromScale, toScale, tweenValue);
                if (AffectSize) {
                    rectTransform.sizeDelta = Vector2.LerpUnclamped(fromSize, toSize, tweenValue);
                }

            }
        }

        public override void OnExit(List<Component> components) {
        }
    }
}