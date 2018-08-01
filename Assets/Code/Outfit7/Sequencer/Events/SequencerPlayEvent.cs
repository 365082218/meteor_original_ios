using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    public class SequencerPlayEvent : SequencerTriggerEvent {
        //public Parameter GoToParam;
        //public float GoToTime = 0f;
        public bool FromStart = true;
        public bool Instant = false;
        public string BookmarkName = "";
        public SequencerSequence AffectingSequence;

        public override bool IgnoreObjects() {
            return true;
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (AffectingSequence == null)
                return;
            #if UNITY_EDITOR
            if (!Application.isPlaying && AffectingSequence.gameObject != gameObject)
                return;
            #endif
            if (BookmarkName != "") {
                AffectingSequence.Play(BookmarkName, Instant);
            } else {
                AffectingSequence.Play(FromStart, Instant);
            }
        }
    }
}   