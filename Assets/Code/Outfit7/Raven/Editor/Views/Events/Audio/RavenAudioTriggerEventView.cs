using System.Collections.Generic;
using Outfit7.Audio;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenAudioTriggerEventView : RavenTriggerEventView {
        private RavenAudioTriggerEvent m_AudioTriggerEvent = null;

        private static GUIStyle s_Node = (GUIStyle)"flow node 2";
        private static GUIStyle s_NodeSelected = (GUIStyle)"flow node 2 on";

        public override string Name {
            get {
                if (m_AudioTriggerEvent.AudioEventData != null) {
                    return m_AudioTriggerEvent.AudioEventData.name;
                }

                return "One Shot";
            }
        }

        protected override GUIStyle NodeStyle {
            get {
                return s_Node;
            }
        }

        protected override GUIStyle NodeSelectedStyle {
            get {
                return s_NodeSelected;
            }
        }

        public override void Initialize(RavenEvent evnt, RavenTrackView parent) {
            base.Initialize(evnt, parent);
            m_AudioTriggerEvent = evnt as RavenAudioTriggerEvent;
        }

        protected override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);

            var rects = new Rect[] { GetHorizontalRect(0.5f, 0.1f, 120f, 16f, 80f), GetHorizontalRect(0.5f, 0.32f, 40f, 16f, 40f), GetHorizontalRect(0.5f, 0.54f, 40f, 16f, 40f) };
            JustifyRectsLeft(rects);

            m_AudioTriggerEvent.AudioEventData = EditorGUI.ObjectField(rects[0], m_AudioTriggerEvent.AudioEventData, typeof(AudioEventData), true) as AudioEventData;
            m_AudioTriggerEvent.Volume = EditorGUI.FloatField(rects[1], m_AudioTriggerEvent.Volume);
            m_AudioTriggerEvent.Pitch = EditorGUI.FloatField(rects[2], m_AudioTriggerEvent.Pitch);
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu, RavenTrackView parent) {
        }

        protected override void OnUpdateWhileRecording(double currentTime) {
        }

        protected override void OnRecordingStart() {
        }

        protected override void OnRecordingStop() {
        }
    }
}