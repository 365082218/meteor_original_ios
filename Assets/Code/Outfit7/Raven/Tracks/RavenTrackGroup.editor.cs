using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public sealed partial class RavenTrackGroup {
#if UNITY_EDITOR

        public const int c_TrackCount = 2;

        public int TrackIndex {
            set {
                if (m_TrackIndex == value) {
                    return;
                }
                Undo.RecordObject(this, "TrackIndexChange");
                m_TrackIndex = value;

                if (m_PropertyTrack != null) {
                    for (int i = 0; i < m_PropertyTrack.Events.Count; ++i) {
                        var evnt = m_PropertyTrack.Events[i];
                        evnt.SetTrackIndex(PropertyTrackIndex);
                    }
                }

                if (m_AudioTrack != null) {
                    for (int i = 0; i < m_AudioTrack.Events.Count; ++i) {
                        var evnt = m_AudioTrack.Events[i];
                        evnt.SetTrackIndex(AudioTrackIndex);
                    }
                }
            }
        }

        public int PropertyTrackIndex {
            get {
                return m_TrackIndex;
            }
        }

        public int AudioTrackIndex {
            get {
                return m_TrackIndex + 1;
            }
        }

#endif
    }
}