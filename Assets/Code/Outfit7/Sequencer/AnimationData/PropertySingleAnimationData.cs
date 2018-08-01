using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class PropertySingleAnimationData : PropertyAnimationData {
        public ParameterVectorField Value = new ParameterVectorField(Vector4.zero);

        public override void LiveInit(SequencerSequence sequence) {
            Value.LiveInit(sequence.Parameters);
        }

        public override void Init(BaseProperty property) {
            bool success;
            Vector4 val = property.Value(out success);
            if (success) {
                Value.Init(val);
            }
        }

        public override void SetStartingValues(Vector4 current) {
            Value.Value = current;
        }

        public override Vector4 Evaluate(float absoluteTime, float duration) {
            return Value.Value;
        }

        public override Vector2 GetMinMax(BaseProperty property) {
            return Vector2.zero;
        }
    }

}