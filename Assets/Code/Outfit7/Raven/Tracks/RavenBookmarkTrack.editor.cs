namespace Starlite.Raven {

    public sealed partial class RavenBookmarkTrack {
#if UNITY_EDITOR

        public override ERavenTrackType TrackType {
            get {
                return ERavenTrackType.BookmarkTrack;
            }
        }

        public override void InitializeEditor(RavenSequence sequence) {
        }

#endif
    }
}