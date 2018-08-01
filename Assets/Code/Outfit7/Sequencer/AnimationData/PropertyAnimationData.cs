using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Outfit7.Sequencer {
    public class PropertyAnimationData : MonoBehaviour {
        public bool Enabled = true;

        public virtual void Init(BaseProperty property) {
        }

        public virtual void LiveInit(SequencerSequence sequence) {
        }

        public virtual void SetStartingValues(Vector4 current) {
            
        }

        public bool Process(BaseProperty Property, float absoluteTime, float duration, float multiplier, float offset, bool remap, float value0, float value1, float repeat, bool bounce) {
            if (Property == null)
                return false;
            if (Enabled && Property.Enabled) {
                float normalizedTime = absoluteTime / duration;
                int loop = bounce ? (int) (normalizedTime * repeat) : 0;

                //hack mode enabled, trying to cover all cases for exit event to work, rethink
                if (bounce && ((int) (repeat - 1)) % 2 == 1)
                    loop = Mathf.Min(loop, (int) (repeat - 1));
                normalizedTime = (normalizedTime % (1 / repeat)) * repeat;
                if (normalizedTime == 0 && absoluteTime != 0)
                    normalizedTime = 1;
                if (loop % 2 == 1)
                    normalizedTime = 1 - normalizedTime;
                //end hacks

                absoluteTime = normalizedTime * duration;

                Vector4 eval = Evaluate(absoluteTime, duration);
                if (remap) {
                    eval.x = eval.x * (value1 - value0) + value0;
                    eval.y = eval.y * (value1 - value0) + value0;
                    eval.z = eval.z * (value1 - value0) + value0;
                    eval.w = eval.w * (value1 - value0) + value0;
                }
                Property.Apply(eval * multiplier + (Vector4.one * offset));
            }
            return true;
        }

        public virtual Vector4 Evaluate(float absoluteTime, float duration) {
            
            return Vector4.one;
        }

        public virtual Vector2 GetMinMax(BaseProperty property) {
            return new Vector2(0, 1);
        }
    }
}