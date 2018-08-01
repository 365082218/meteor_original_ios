using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Audio;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("Sound")]
    [SequencerQuickSearchDisplayAttribute("AudioFireAndForget")]
    [SequencerAudioTrackAttribute("Audio Fire And Forget Event")]
    public class SequencerAudioFireAndForgetEventView : SequencerTriggerEventView {
        private SequencerAudioFireAndForgetEvent Event = null;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerAudioFireAndForgetEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            if (Event.AudioEventData != null) {
                return Event.AudioEventData.name;
            } else {
                return "No Audio";
            }
        }

        protected override void OnDestroy() {
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            Event.AudioEventData = (AudioEventData) EditorGUI.ObjectField(new Rect(EventRect.x, EventRect.y + 20f, 150f, 15f), "", Event.AudioEventData, typeof(AudioEventData), false);
            EditorGUI.LabelField(new Rect(EventRect.x - 15f, EventRect.yMax - 30f, 50f, 15f), "Vol:");
            Event.Volume = EditorGUI.FloatField(new Rect(EventRect.x + 15f, EventRect.yMax - 30f, 20f, 15f), Event.Volume);
            EditorGUI.LabelField(new Rect(EventRect.x - 15f, EventRect.yMax - 15f, 50f, 15f), "Pit:");
            Event.Pitch = EditorGUI.FloatField(new Rect(EventRect.x + 15f, EventRect.yMax - 15f, 20f, 15f), Event.Pitch);
        }
    }
}

