using System.Collections.Generic;

namespace Starlite.Raven {

    [System.Serializable]
    public class RavenEventTriggerPoint {

        public class RavenEventTriggerPointComparer : IComparer<RavenEventTriggerPoint> {

            public int Compare(RavenEventTriggerPoint x, RavenEventTriggerPoint y) {
#if RAVEN_DEBUG
                RavenAssert.IsTrue(x.IsPooled == false, "{0} (F: {1}) is pooled when comparing!", x.m_RavenEvent, x.Frame);
                RavenAssert.IsTrue(y.IsPooled == false, "{0} (F: {1}) is pooled when comparing!", y.m_RavenEvent, y.Frame);
#endif
                var frameCompare = x.m_Frame.CompareTo(y.m_Frame);
                if (frameCompare != 0) {
                    return frameCompare;
                }

                // end events have to be executed first at the beginning of the frame
                if (x.m_Type == ERavenEventTriggerPointType.End && y.m_Type < ERavenEventTriggerPointType.End) {
                    return -1;
                }
                if (y.m_Type == ERavenEventTriggerPointType.End && x.m_Type < ERavenEventTriggerPointType.End) {
                    return 1;
                }

                // barriers have to be executed after end events/bookmarks
                if (x.m_Type == ERavenEventTriggerPointType.Barrier && y.m_Type < ERavenEventTriggerPointType.Bookmark) {
                    return -1;
                }
                if (y.m_Type == ERavenEventTriggerPointType.Barrier && x.m_Type < ERavenEventTriggerPointType.Bookmark) {
                    return 1;
                }

                // bookmarks have track index of -1 so they'll be right after

                var trackIndexCompare = x.m_RavenEvent.TrackIndex.CompareTo(y.m_RavenEvent.TrackIndex);
                if (trackIndexCompare != 0) {
                    return trackIndexCompare;
                }

                return x.m_RavenEvent.SubTrackIndex.CompareTo(y.m_RavenEvent.SubTrackIndex);
            }
        }

        public static readonly RavenEventTriggerPointComparer Comparer = new RavenEventTriggerPointComparer();

        private int m_Frame = 0;
        private ERavenEventTriggerPointType m_Type = ERavenEventTriggerPointType.Start;
        private RavenEvent m_RavenEvent;

        public int Frame {
            get {
                return m_Frame;
            }
        }

        public ERavenEventTriggerPointType Type {
            get {
                return m_Type;
            }
        }

        public RavenEvent RavenEvent {
            get {
                return m_RavenEvent;
            }
        }

        public bool IsPooled {
            get;
            set;
        }

        public RavenEventTriggerPoint() {
            IsPooled = true;
        }

        public void Initialize(int frame, ERavenEventTriggerPointType type, RavenEvent sequencerEvent) {
            this.m_Frame = frame;
            this.m_Type = type;
            this.m_RavenEvent = sequencerEvent;
        }

        public void Reset() {
            m_Frame = 0;
            m_Type = ERavenEventTriggerPointType.Start;
            m_RavenEvent = null;
        }
    }
}