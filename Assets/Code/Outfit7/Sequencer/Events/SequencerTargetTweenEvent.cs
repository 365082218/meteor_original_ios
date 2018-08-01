using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Graphics;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class SequencerTargetTweenEvent : SequencerContinuousEvent {
        public EaseManager.Ease EaseType;
        public ParameterComponentField FromComponentField = new ParameterComponentField(null);
        public ParameterComponentField ToComponentField = new ParameterComponentField(null);

        public ParameterVectorField FromPositionField = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField ToPositionField = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField FromRotationField = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField ToRotationField = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField FromScaleField = new ParameterVectorField(Vector4.one);
        public ParameterVectorField ToScaleField = new ParameterVectorField(Vector4.one);

        public ParameterComponentField FromCamera = new ParameterComponentField(null);
        public ParameterComponentField ToCamera = new ParameterComponentField(null);
        public float ToCameraDepth = 10f;
        public bool AffectPosition = true;
        public bool AffectRotation = true;
        public bool AffectScale = true;
        public bool UseCustomCurve = false;
        public bool FromPointCasting = false;
        public bool RotateWithTangent = false;
        public AnimationCurve CustomCurve = new AnimationCurve(new Keyframe[]{ new Keyframe(0, 0), new Keyframe(1, 1) });
        public ParameterBoolField UseBezierCurve = new ParameterBoolField(false);
        public ParameterVectorField TangentStart = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField TangentEnd = new ParameterVectorField(Vector4.zero);
        private Transform FromTransform;
        private Transform ToTransform;


        public override void OnInit() {
            ComponentType = typeof(UnityEngine.Transform);
        }

        protected override void OnLiveInit(SequencerSequence sequence) {
            FromComponentField.LiveInit(sequence.Parameters);
            ToComponentField.LiveInit(sequence.Parameters);
            FromPositionField.LiveInit(sequence.Parameters);
            ToPositionField.LiveInit(sequence.Parameters);
            FromRotationField.LiveInit(sequence.Parameters);
            ToRotationField.LiveInit(sequence.Parameters);
            FromScaleField.LiveInit(sequence.Parameters);
            ToRotationField.LiveInit(sequence.Parameters);
            UseBezierCurve.LiveInit(sequence.Parameters);
            TangentStart.LiveInit(sequence.Parameters);
            TangentEnd.LiveInit(sequence.Parameters);
            FromCamera.LiveInit(sequence.Parameters);
            ToCamera.LiveInit(sequence.Parameters);
        }

        public override void OnEnter(List<Component> components, float absoluteTime, float normalizedTime) {
            FromCamera.Init(null);
            ToCamera.Init(null);
            FromComponentField.Init(null);
            FromTransform = FromComponentField.Value as Transform;
            ToComponentField.Init(null);
            ToTransform = ToComponentField.Value as Transform;
            if (FromComponentField.FieldType == ParameterFieldType.CURRENT && components.Count > 0) {
                Transform fromTransform = components[0] as Transform;
                FromPositionField.Init((Vector4) fromTransform.position);
                FromRotationField.Init(new Vector4(fromTransform.rotation.x, fromTransform.rotation.y, fromTransform.rotation.z, fromTransform.rotation.w));
                FromScaleField.Init((Vector4) fromTransform.localScale);
            } else {
                FromPositionField.Init(null);
                FromRotationField.Init(null);
                FromScaleField.Init(null);
            }
            if (ToComponentField.FieldType == ParameterFieldType.CURRENT && components.Count > 0) {
                Transform fromTransform = components[0] as Transform;
                ToPositionField.Init((Vector4) fromTransform.position);
                ToRotationField.Init(new Vector4(fromTransform.rotation.x, fromTransform.rotation.y, fromTransform.rotation.z, fromTransform.rotation.w));
                ToRotationField.Init((Vector4) fromTransform.localScale);
            } else {
                ToPositionField.Init(null);
                ToRotationField.Init(null);
                ToRotationField.Init(null);
            }
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
                Transform transform = components[i] as Transform;
                if (transform == null)
                    return;
                Vector3 toPosition;
                Vector3 fromPosition;
                float tweenValue = 0;
                if (UseCustomCurve) {
                    tweenValue = CustomCurve.Evaluate(absoluteTime / Duration);
                } else {
                    tweenValue = EaseManager.Evaluate(EaseType, absoluteTime, Duration, 1, 1);
                }
                if (ToTransform != null) {
                    toPosition = ToTransform.position;
                } else {
                    toPosition = ToPositionField.Value;
                }

                if (FromTransform != null) {
                    fromPosition = FromTransform.position;
                } else {
                    fromPosition = FromPositionField.Value;
                }

                Camera FromCameraCast = FromCamera.Value as Camera;
                Camera ToCameraCast = ToCamera.Value as Camera; 
                if (FromCameraCast != null && ToCameraCast != null) {
                    if (FromPointCasting) {

                        Vector3 screenPoint = FromCameraCast.WorldToScreenPoint(fromPosition);
                        screenPoint.z = ToCameraDepth;
                        fromPosition = ToCameraCast.ScreenToWorldPoint(screenPoint);
                    } else {
                        Vector3 screenPoint = ToCameraCast.WorldToScreenPoint(toPosition);
                        screenPoint.z = ToCameraDepth;
                        toPosition = FromCameraCast.ScreenToWorldPoint(screenPoint);
                    }
                }

                Quaternion fromRotation;
                Quaternion toRotation;
                Vector3 fromScale;
                Vector3 toScale;

                if (FromTransform != null) {
                    fromRotation = FromTransform.rotation;
                    fromScale = FromTransform.localScale;
                } else {
                    fromRotation = Quaternion.Euler(FromRotationField.Value);
                    fromScale = FromScaleField.Value;
                }
                if (ToTransform != null) {
                    toRotation = ToTransform.rotation;
                    toScale = ToTransform.localScale;
                } else {
                    toRotation = Quaternion.Euler(ToRotationField.Value);
                    toScale = ToScaleField.Value;
                }

                if (AffectPosition) {
                    if (UseBezierCurve.Value)
                        transform.position = Interpolation.BezierCubicCurve(fromPosition, fromPosition + (Vector3) TangentStart.Value, toPosition + (Vector3) TangentEnd.Value, toPosition, tweenValue);
                    else
                        transform.position = Vector3.LerpUnclamped(fromPosition, toPosition, tweenValue);
                }
                if (AffectRotation)
                    transform.rotation = Quaternion.LerpUnclamped(fromRotation, toRotation, tweenValue);
                if (AffectScale)
                    transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, tweenValue);

                if (RotateWithTangent) {
                    transform.LookAt(transform.localPosition + Interpolation.BezierCubicTangent(fromPosition, fromPosition + (Vector3) TangentStart.Value, toPosition + (Vector3) TangentEnd.Value, toPosition, tweenValue));
                }

            }
        }

        public override void OnExit(List<Component> components) {
        }
    }
}