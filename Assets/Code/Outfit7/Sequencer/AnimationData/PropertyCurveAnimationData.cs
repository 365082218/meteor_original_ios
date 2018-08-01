using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Outfit7.Sequencer {
    public class PropertyCurveAnimationData : PropertyAnimationData {
        public EaseManager.Ease EaseType;
        public AnimationCurve[] Curve;

        public override void SetStartingValues(Vector4 current) {
            Curve = new AnimationCurve[4];
            for (int i = 0; i < 4; i++) {
                Curve[i] = new AnimationCurve();
                Curve[i].AddKey(new Keyframe(0, current[i]));
                Curve[i].AddKey(new Keyframe(1, current[i]));
            }
        }

        public override Vector4 Evaluate(float absoluteTime, float duration) {
            float normalizedTime = Mathf.Clamp01(EaseManager.Evaluate(EaseType, absoluteTime, duration, 1, 1));
            //float normalizedTime = Mathf.Clamp01(absoluteTime / duration);
            return new Vector4(
                Curve[0].Evaluate(normalizedTime),
                Curve[1].Evaluate(normalizedTime),
                Curve[2].Evaluate(normalizedTime),
                Curve[3].Evaluate(normalizedTime));
            //return Vector4.Lerp(StartValue, EndValue, normalizedTime);
        }

        public override Vector2 GetMinMax(BaseProperty property) {
            if (property == null)
                base.GetMinMax(property); 
            float min = Mathf.Infinity;
            float max = Mathf.NegativeInfinity;

            for (int c = 0; c < property.GetNumberOfValuesUsed(); c++) {
                for (int i = 0; i < Curve[c].keys.Length; i++) {
                    if (Curve[c].keys[i].value > max)
                        max = Curve[c].keys[i].value;
                    if (Curve[c].keys[i].value < min)
                        min = Curve[c].keys[i].value;
                }
            }
            return new Vector2(min, max);

        }
    }

}