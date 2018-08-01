using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("play")]
    [SequencerQuickSearchDisplayAttribute("Play Sequence")]
    [SequencerNormalTrackAttribute("Sequence/Play")]
    public class SequencerPlayEventView : SequencerTriggerEventView {
        private SequencerPlayEvent Event = null;
        private Rect GoToRect;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerPlayEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Play";
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            GUI.Label(GetRectPosition(10f, 40f, 20f), GetName());

            Event.BookmarkName = EditorGUI.TextField(GetRectPosition(30f, 60f, 20f), Event.BookmarkName);
            if (Event.BookmarkName == "") {
                Event.FromStart = EditorGUI.ToggleLeft(GetRectPosition(50f, 40f, 40f), "Reset", Event.FromStart);
            }
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
