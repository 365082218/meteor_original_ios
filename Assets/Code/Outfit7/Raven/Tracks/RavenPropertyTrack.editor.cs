#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public sealed partial class RavenPropertyTrack {
#if UNITY_EDITOR

        public override ERavenTrackType TrackType {
            get {
                return ERavenTrackType.PropertyTrack;
            }
        }

        public override void InitializeEditor(RavenSequence sequence) {
            Undo.RecordObject(this, "InitializeEditor");
            sequence.TrackGroupsChanged();
        }

#endif
    }
}