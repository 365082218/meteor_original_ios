using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Play Animation State")]
    [SequencerCurveTrackAttribute("Animation/Play Animation State")]
    public class PlayAnimatorStateEventView : SequencerContinuousEventView {
        private PlayAnimatorStateEvent Event = null;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as PlayAnimatorStateEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "AnimState";
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            Event.StateName = EditorGUI.TextField(new Rect(EventRect.x, EventRect.y + 20f, EventRect.width, 15f), "State Name", Event.StateName);
            Event.SpeedParamName = EditorGUI.TextField(new Rect(EventRect.x, EventRect.y + 35f, EventRect.width, 15f), "Speed Param Name", Event.SpeedParamName);
        }

        protected override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            
            /*if (EventRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                Event.EaseType = EaseManager.EaseHotkeys(Event.EaseType);
            }*/
            return base.OnHandleInput(timelineData, sequenceView, timelineTrackRect, highiestEventTrackIndex, actor);
        }
    }
}

