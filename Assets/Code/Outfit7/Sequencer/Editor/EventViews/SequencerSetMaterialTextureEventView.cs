using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Set Material Texture")]
    [SequencerCurveTrackAttribute("Set/Material Texture")]
    public class SequencerSetMaterialTextureEventView : SequencerTriggerEventView {
        private SequencerSetMaterialTextureEvent Event = null;
        private List<Texture> SaveTextures = new List<Texture>();

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerSetMaterialTextureEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Tex";
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
            Event.TextureSamplerName = EditorGUI.TextField(GetRectPosition(60f, 60f, 15f), Event.TextureSamplerName);

        }

        public override void OnRecordingStart() {
            SaveTextures.Clear();
            for (int i = 0; i < Event.Objects.Count; i++) {
                Renderer r = Event.Objects[i].Components[0] as Renderer;
                if (r != null)
                    SaveTextures.Add(r.sharedMaterial.GetTexture(Event.TextureSamplerName));
            }
        }

        public override void OnRecordingStop() {
            for (int i = 0; i < Event.Objects.Count; i++) {
                Renderer r = Event.Objects[i].Components[0] as Renderer;
                if (r == null)
                    continue;
                if (i >= SaveTextures.Count)
                    return;
                r.sharedMaterial.SetTexture(Event.TextureSamplerName, SaveTextures[i]);
            }
        }
    }
}

