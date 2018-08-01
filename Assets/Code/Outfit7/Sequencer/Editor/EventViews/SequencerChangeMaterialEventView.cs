using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Change Material")]
    [SequencerCurveTrackAttribute("Renderer/Change Material")]
    public class SequencerChangeMaterialEventView : SequencerContinuousEventView {
        private SequencerChangeMaterialEvent Event = null;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerChangeMaterialEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Change Material";
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            Event.SelfInstantiate = EditorGUI.Toggle(new Rect(EventRect.x, EventRect.y + 25, EventRect.width, 20f), "Self Instantiate?", Event.SelfInstantiate);
            if (!Event.SelfInstantiate)
                Event.NewMaterial = (Material) EditorGUI.ObjectField(new Rect(EventRect.x, EventRect.y + 45f, EventRect.width, 20f), new GUIContent("Material:"), Event.NewMaterial, typeof(Material), true);
            Event.ChangeBack = EditorGUI.Toggle(new Rect(EventRect.x, EventRect.y + 65f, EventRect.width, 20f), "Change back on end?", Event.ChangeBack);
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
            if (Event.OldMaterial == null)
                return;
            for (int i = 0; i < Event.Objects.Count; i++) {
                if (Event.Objects[i].Components.Count == 0)
                    continue;
                Renderer r = Event.Objects[i].Components[0] as Renderer;
                r.material = Event.OldMaterial;
                Event.OldMaterial = null;
            }
        }
    }
}
