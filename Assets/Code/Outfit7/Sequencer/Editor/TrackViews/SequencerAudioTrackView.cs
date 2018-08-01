using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using System;
using System.Reflection;
using System.Linq;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class SequencerAudioTrackAttribute : SequencerTrackAttribute {
        public SequencerAudioTrackAttribute(string path) : base(path) {
            this.path = path;
        }
    }

    [SequencerMainTrackGroupAttribute]
    public class SequencerAudioTrackView : SequencerTrackView {
        private SequencerAudioTrack Track = null;

        public override string GetName() {
            return "Audio Track";
        }

        protected override void GetTypesEnumerator() {
            Assembly assembly = typeof(SequencerEventView).Assembly;
            TypesEnumerator = assembly.GetTypes().
                Where(t => typeof(SequencerEventView).IsAssignableFrom(t)).
                Where(t => Attribute.IsDefined(t, typeof(SequencerAudioTrackAttribute)));
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            if (!foldoutData.Enabled)
                return;
        }

        public override void OnInit(object track) {
            Track = (SequencerAudioTrack) track;
            //temporary until i figure this out

            base.OnInit(track);
            Track.FoldoutData.Height = 90f;
        }
    }
}