using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Stop Sequence")]
    [SequencerNormalTrackAttribute("Sequence/Stop")]
    public class SequencerStopSequenceEventView : SequencerTriggerEventView {
        private SequencerStopSequenceEvent Event = null;
        //private Color LineColor = new Color(0.2f, 0.6f, .2f);

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerStopSequenceEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Stop!";
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            GUI.Label(GetRectPosition(20f, 40f, 20f), GetName());
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }


        public override void OnRefresh(object actor) {
            base.OnRefresh(actor);
            if (Event.AffectingSequence == null)
                Event.AffectingSequence = Event.GetComponent<SequencerSequence>();
        }
    }
}
