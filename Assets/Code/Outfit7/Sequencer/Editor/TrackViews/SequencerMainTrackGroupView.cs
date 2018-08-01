using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using System.Reflection;
using System;
using System.Linq;

namespace Outfit7.Sequencer {
    public class SequencerMainTrackGroupAttribute : System.Attribute {
    }

    public class SequencerMainTrackGroupView : SequencerTrackGroupView {
        //private SequencerMainTrackGroup TrackGroup = null;

        public override string GetName() {
            return "MainGroup";
        }

        public override void OnInit(object track) {
            //TrackGroup = (SequencerMainTrackGroup) track;
            base.OnInit(track);
        }

        protected override void GetTypesEnumerator() {
            Assembly assembly = typeof(SequencerTrackView).Assembly;
            TypesEnumerator = assembly.GetTypes().Where(t => typeof(SequencerTrackView).IsAssignableFrom(t)).
                Where(t => Attribute.IsDefined(t, typeof(SequencerMainTrackGroupAttribute)));
        }

        public override void RefreshAllTracks() {

            foreach (SequencerTrackView trackView in TrackViews) {
                trackView.RefreshAllEvents(new List<GameObject>(0));
            }
        }
    }
}