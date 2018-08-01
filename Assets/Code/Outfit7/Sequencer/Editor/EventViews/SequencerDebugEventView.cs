using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Debug")]
    [SequencerNormalTrackAttribute("Util/Debug")]
    public class SequencerDebugEventView : SequencerContinuousEventView {

        public override string GetName() {
            return "Debug";
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }


        public override void OnRefresh(object actor) {
            base.OnRefresh(actor);
        }

        protected override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            return base.OnHandleInput(timelineData, sequenceView, timelineTrackRect, highiestEventTrackIndex, actor);
        }

        public override void OnRecordingStart() {
        }

        public override void OnRecordingStop() {
        }
    }
}
