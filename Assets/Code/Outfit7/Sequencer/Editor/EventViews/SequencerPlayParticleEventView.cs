using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Play Particle System")]
    [SequencerNormalTrackAttribute("Effects/Play Particle System")]
    public class SequencerPlayParticleEventView : SequencerTriggerEventView {
        private SequencerPlayParticleEvent Event = null;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerPlayParticleEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Particle";
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            Event.ParticleSystem = (ParticleSystem) EditorGUI.ObjectField(GetRectPosition(40f, 80f, 15f), Event.ParticleSystem, typeof(ParticleSystem), true);
            //Event.ParameterFieldView.DrawParameterField(new Rect(EventRect.center.x - 20f, EventRect.yMin + 40f, 40f, 40f), Event.GoToTime, parameters);
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }
    }
}
