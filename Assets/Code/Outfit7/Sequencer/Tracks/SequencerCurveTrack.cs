using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.Internal;

namespace Outfit7.Sequencer {
    public class SequencerCurveTrack : SequencerTrack {

        public List<BaseProperty> Properties = new List<BaseProperty>();

        public override void OnInit() {
            
        }

        public override void Evaluate(SequencerSequence sequence, BinarySortList<ActionPoint> actionPoints, float prevTime, float currentTime) {
            ResetUsedProperties();
            base.Evaluate(sequence, actionPoints, prevTime, currentTime);
        }

        public override void LiveInit(SequencerSequence sequence) {
            for (int i = 0; i < Properties.Count; i++) {
                Properties[i].LiveInit(sequence);
            }
            base.LiveInit(sequence);
        }

        private void ResetUsedProperties() {
            for (int i = 0; i < Properties.Count; i++) {
                Properties[i].UsedThisFrame = false;
            }
        }
    }
}
