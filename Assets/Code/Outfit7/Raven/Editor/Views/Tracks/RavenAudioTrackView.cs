using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenAudioTrackView : RavenTrackView {
        private RavenAudioTrack m_AudioTrack = null;

        public override string Name {
            get {
                return "Audio Track";
            }
        }

        public override int TrackIndex {
            get {
                return m_Parent.TrackGroup.AudioTrackIndex;
            }
        }

        public override void Initialize(RavenTrack track, RavenTrackGroupView parent) {
            base.Initialize(track, parent);
            m_AudioTrack = (RavenAudioTrack)track;
            m_AudioTrack.FoldoutHeight = 90f;
        }

        public override void OnTargetChanged(GameObject target) {
            for (int i = 0; i < m_AudioTrack.Events.Count; ++i) {
                var evnt = m_AudioTrack.Events[i];
                Undo.RecordObject(evnt, "Target Change");
                evnt.SetTargetEditor(RavenSequenceEditor.Instance.Sequence, target);
            }
        }

        protected override void OnDrawSidebarGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
        }

        protected override void OnDestroyTrack() {
        }

        protected override void OnRecordingStart() {
        }

        protected override void OnRecordingStop() {
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("One Shot Audio"), false, AddEvent<RavenAudioTriggerEvent>, null);
            if (RavenEditorCallbacks.e_GenerateCustomEventsForAudioTrackContextMenu != null) {
                RavenEditorCallbacks.e_GenerateCustomEventsForAudioTrackContextMenu(menu, this);
            }
        }

        protected override void OnDrawTrackContextMenu(GenericMenu menu) {
        }
    }
}