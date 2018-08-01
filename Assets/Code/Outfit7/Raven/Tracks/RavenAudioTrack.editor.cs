#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public sealed partial class RavenAudioTrack {
#if UNITY_EDITOR

        public override ERavenTrackType TrackType {
            get {
                return ERavenTrackType.AudioTrack;
            }
        }

        public override void InitializeEditor(RavenSequence sequence) {
            Undo.RecordObject(this, "InitializeEditor");
            sequence.TrackGroupsChanged();
        }

#endif
    }
}