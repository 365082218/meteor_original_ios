using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Outfit7.Logic.Internal;

namespace Outfit7.Sequencer {
    [Serializable]
    public class SequencerFoldoutData {
        public bool Enabled = true;
        public float Height = 60f;
    }

    public class SequencerTrack : MonoBehaviour {

        [SerializeField]
        public List<SequencerEvent> Events = new List<SequencerEvent>();
        public string Name = "Track";

        public SequencerFoldoutData FoldoutData = new SequencerFoldoutData();

        public void Init() {
        }

        public virtual void OnInit() {
        }

        public virtual void LiveInit(SequencerSequence sequence) {
            for (int i = 0; i < Events.Count; i++) {
                Events[i].LiveInit(sequence);
            }
        }

        public void Preplay() {
            for (int i = 0; i < Events.Count; i++) {
                Events[i].DoPreplay();
            }
        }

        public virtual void Evaluate(SequencerSequence sequence, BinarySortList<ActionPoint> actionPoints, float prevTime, float currentTime) {

            for (int i = 0; i < Events.Count; i++) {
                var ev = Events[i];
                if (sequence.IgnoredEvents.Contains(ev)) {
                    continue;
                }
                ev.GetActionPoints(ref actionPoints, prevTime, currentTime);
            }

            /*int counter = 0;
            for (int layer = 0; layer < 10; layer++) {
                counter = 0;
                for (int i = 0; i < Events.Count; i++) {
                    if (Events[i].UiTrackIndex == layer) {
                        Events[i].Evaluate(deltaTime, currentTime);
                        if (sequence.ForceSplitUpdate)
                            return;
                        counter++;
                    }
                }
                if (counter == 0)
                    break;
            }*/
        }
    }
}
