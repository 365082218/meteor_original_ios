using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.Internal;

namespace Outfit7.Sequencer {

    public enum SequencerTrackGroupMode {
        EXTENDED,
        OPTIMIZED,
        COLLAPSED
    }

    public class SequencerTrackGroup : MonoBehaviour {
        public bool Foldout = true;
        public SequencerTrackGroupMode TrackGroupMode;
        public List<SequencerTrack> Tracks = new List<SequencerTrack>();

        public virtual void Evaluate(SequencerSequence sequence, BinarySortList<ActionPoint> actionPoints, float prevTime, float currentTime) {
            for (int i = 0; i < Tracks.Count; i++) {
                Tracks[i].Evaluate(sequence, actionPoints, prevTime, currentTime);
                if (sequence.ForceSplitUpdate)
                    return;
            }
        }

        public void LiveInit(SequencerSequence sequence) {
            for (int i = 0; i < Tracks.Count; i++) {
                Tracks[i].LiveInit(sequence);
            }
        }

        public void Preplay() {
            for (int i = 0; i < Tracks.Count; i++) {
                Tracks[i].Preplay();
            }
        }
    }
}