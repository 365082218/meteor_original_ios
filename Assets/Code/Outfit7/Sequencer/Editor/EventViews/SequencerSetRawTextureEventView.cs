using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using UnityEngine.UI;


namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Set UI Texture")]
    [SequencerCurveTrackAttribute("UI/Set Raw Texture")]
    public class SequencerSetRawTextureEventView : SequencerTriggerEventView {
        private SequencerSetRawTextureEvent Event = null;
        private List<Texture> SaveTextures = new List<Texture>();

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerSetRawTextureEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Raw";
        }

        protected override void OnDestroy() {
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            GUI.Label(GetRectPosition(20f, 40f, 20f), GetName());
            Event.NewTexture = (Texture) EditorGUI.ObjectField(GetRectPosition(40f, 60f, 15f), Event.NewTexture, typeof(Texture), false);

        }

        public override void OnRecordingStart() {
            SaveTextures.Clear();
            for (int i = 0; i < Event.Objects.Count; i++) {
                RawImage r = Event.Objects[i].Components[0] as RawImage;
                if (r != null)
                    SaveTextures.Add(r.texture);
            }
        }

        public override void OnRecordingStop() {
            for (int i = 0; i < Event.Objects.Count; i++) {
                RawImage r = Event.Objects[i].Components[0] as RawImage;
                if (r == null)
                    continue;
                if (i >= SaveTextures.Count)
                    return;
                r.texture = SaveTextures[i];
            }
        }
    }
}

