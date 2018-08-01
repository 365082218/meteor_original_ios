using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenBookmarkTrackView : RavenTrackView {

        public override string Name {
            get {
                return "Bookmark Track";
            }
        }

        public override int TrackIndex {
            get {
                return -1;
            }
        }

        public override void Initialize(RavenTrack track, RavenTrackGroupView parent) {
            base.Initialize(track, parent);
            track.FoldoutHeight = 40f;
        }

        public override void OnTargetChanged(GameObject target) {
        }

        protected override void DrawTrackContextMenu() {
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
            if (m_Track.Events.Find(x => x.StartFrame == m_AddEventFrame) == null) {
                menu.AddItem(new GUIContent("Bookmark"), false, AddEvent<RavenBookmarkEvent>, null);
            }
        }

        protected override void OnDrawTrackContextMenu(GenericMenu menu) {
        }

        private void AddBookmarkEvent(RavenBookmarkEvent bookmark) {
        }
    }
}