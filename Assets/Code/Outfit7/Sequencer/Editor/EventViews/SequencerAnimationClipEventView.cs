using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Animation Clip")]
    [SequencerCurveTrackAttribute("Animation/Animation Clip")]
    public class SequencerAnimationClipEventView : SequencerContinuousEventView {
        private SequencerAnimationClipEvent Event = null;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerAnimationClipEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            if (Event.AnimationClip != null) {
                return Event.AnimationClip.name + " (AnimationClip)";
            } else {
                return "No Animation";
            }
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            Event.AnimationClip = (AnimationClip) EditorGUI.ObjectField(new Rect(EventRect.x, EventRect.y + 20f, EventRect.width, 15f), "", Event.AnimationClip, typeof(AnimationClip), false);
            Event.EaseType = (EaseManager.Ease) EditorGUI.EnumPopup(new Rect(EventRect.x, EventRect.y + 40f, EventRect.width, 15f), Event.EaseType);
        }

        protected override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            
            if (EventRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                Event.EaseType = EaseManager.EaseHotkeys(Event.EaseType);
            }
            return base.OnHandleInput(timelineData, sequenceView, timelineTrackRect, highiestEventTrackIndex, actor);
        }
    }
}

