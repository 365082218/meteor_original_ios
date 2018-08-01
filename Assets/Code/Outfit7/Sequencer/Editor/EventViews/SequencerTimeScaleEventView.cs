using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("time")]
    [SequencerQuickSearchDisplayAttribute("scale")]
    [SequencerNormalTrackAttribute("Sequence/Time Scale")]
    public class SequencerTimeScaleEventView : SequencerTriggerEventView {
        private SequencerTimeScaleEvent Event = null;
        private Rect GoToRect;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerTimeScaleEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "TScale";
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            GUI.Label(GetRectPosition(20f, 40f, 20f), GetName());

            ParameterFieldView.DrawParameterField(new Rect(EventRect.center.x - 20f, EventRect.yMin + 40f, 40f, 40f), Event.TimeScale, parameters);
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }


        public override void OnRefresh(object actor) {
            base.OnRefresh(actor);
            if (Event.AffectingSequence == null)
                Event.AffectingSequence = Event.GetComponent<SequencerSequence>();
        }

        protected override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            return base.OnHandleInput(timelineData, sequenceView, timelineTrackRect, highiestEventTrackIndex, actor);
        }
    }
}
