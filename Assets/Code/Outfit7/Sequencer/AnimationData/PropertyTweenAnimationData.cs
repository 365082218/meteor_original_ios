using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class PropertyTweenAnimationData : PropertyAnimationData {
        public bool UseCustomCurve = false;
        public AnimationCurve CustomCurve = new AnimationCurve(new Keyframe[]{ new Keyframe(0, 0), new Keyframe(1, 1) });
        public EaseManager.Ease EaseType;
        public float Amplitude = 1f;
        public float Period = 1f;
        public ParameterVectorField StartValue = new ParameterVectorField(Vector4.zero);
        public ParameterVectorField EndValue = new ParameterVectorField(Vector4.zero);

        public override void LiveInit(SequencerSequence sequence) {
            StartValue.LiveInit(sequence.Parameters);
            EndValue.LiveInit(sequence.Parameters);
        }

        public override void Init(BaseProperty property) {
            bool success;
            Vector4 val = property.Value(out success);
            if (success) {
                StartValue.Init(val);
                EndValue.Init(val);
            }
        }

        public override void SetStartingValues(Vector4 current) {
            StartValue.Value = current;
            EndValue.Value = current;
        }

        public override Vector4 Evaluate(float absoluteTime, float duration) {
            float tweenValue = 0;
            if (UseCustomCurve)
                tweenValue = CustomCurve.Evaluate(absoluteTime / duration);
            else
                tweenValue = EaseManager.Evaluate(EaseType, absoluteTime, duration, Amplitude, Period);
            return Vector4.LerpUnclamped(StartValue.Value, EndValue.Value, tweenValue);
        }

        public override Vector2 GetMinMax(BaseProperty property) {
            Vector4 minVector = Vector4.Min(StartValue.Value, EndValue.Value);
            float min = Mathf.Min(Mathf.Min(Mathf.Min(minVector.x, minVector.y), minVector.z), minVector.w);
            Vector4 maxVector = Vector4.Max(StartValue.Value, EndValue.Value);
            float max = Mathf.Max(Mathf.Max(Mathf.Max(maxVector.x, maxVector.y), maxVector.z), maxVector.w);

            min = min - Mathf.Abs(min) * 0.5f;
            max = max + Mathf.Abs(max) * 0.5f;
            return new Vector2(min, max);
        }
    }

}