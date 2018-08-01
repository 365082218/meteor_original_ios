using System;
using System.Collections.Generic;

namespace Starlite.Raven {

    public partial class RavenSequence {
        private int m_LastWarmupFrame = 0;
        private bool m_ForceWarmup = false;

        private readonly List<RavenEventTriggerPoint> m_TempTriggerPointSortListForSearch = new List<RavenEventTriggerPoint>(64);
        private readonly List<RavenEvent> m_CachedContinuousEventsForSearch = new List<RavenEvent>(64);

        private bool ShouldWarmup() {
            return m_LastWarmupFrame < m_TotalFrameCount;
        }

        private int GetAllocatedWarmupFrames() {
            var maxFrames = m_TotalFrameCount - m_LastWarmupFrame;
            return m_ForceWarmup ? maxFrames : RavenOverseer.GetAllocatedWarmupFrames(this, maxFrames);
        }

        private void UpdateWarmup() {
            if (!ShouldWarmup()) {
                return;
            }

            var warmupFrames = GetAllocatedWarmupFrames();
            int startIndex = BinarySearchIndexOfFirst(m_SortedEvents, m_LastWarmupFrame);
            for (int i = 0; i < warmupFrames; ++i) {
                var triggerPointList = GetTriggerPointsForFrame(m_LastWarmupFrame, ref startIndex);
                m_TriggerPoints.Add(triggerPointList);
                ++m_LastWarmupFrame;
            }

            if (m_LastWarmupFrame == m_TotalFrameCount) {
                List<RavenEventTriggerPoint> endTriggerPoints = null;
                // do we have some leftover events that weren't finished until the end?
                if (m_CachedContinuousEventsForSearch.Count > 0) {
                    endTriggerPoints = RavenOverseer.PopTriggerPointList();
                    for (int i = 0; i < m_CachedContinuousEventsForSearch.Count; ++i) {
                        m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer.PopTriggerPoint(m_LastWarmupFrame, ERavenEventTriggerPointType.End, m_CachedContinuousEventsForSearch[i]), RavenEventTriggerPoint.Comparer);
                    }
                    for (int i = 0; i < m_TempTriggerPointSortListForSearch.Count; ++i) {
                        endTriggerPoints.Add(m_TempTriggerPointSortListForSearch[i]);
                    }
                    m_TempTriggerPointSortListForSearch.Clear();
                    m_CachedContinuousEventsForSearch.Clear();
                }
                m_TriggerPoints.Add(endTriggerPoints);
            }
        }

        private List<RavenEventTriggerPoint> GetTriggerPointsForFrame(int frame, ref int startIndex) {
            var placedBarrier = false;
            if (startIndex >= 0) {
                while (startIndex < m_SortedEvents.Count && m_SortedEvents[startIndex].StartFrame == frame) {
                    var evnt = m_SortedEvents[startIndex];
                    if (evnt.IsValid()) {
                        switch (evnt.EventType) {
                            case ERavenEventType.Bookmark:
                                m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer.PopTriggerPoint(frame, ERavenEventTriggerPointType.Bookmark, evnt), RavenEventTriggerPoint.Comparer);
                                evnt.Initialize(this);
                                break;

                            case ERavenEventType.Trigger:
                                m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer.PopTriggerPoint(frame, ERavenEventTriggerPointType.Start, evnt), RavenEventTriggerPoint.Comparer);
                                evnt.Initialize(this);
                                break;

                            case ERavenEventType.Continuous:
                                m_CachedContinuousEventsForSearch.Add(evnt);
                                break;
                        }

                        // Add barrier if necessary
                        if (!placedBarrier && evnt.IsBarrier) {
                            placedBarrier = true;
                            m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer.PopTriggerPoint(frame, ERavenEventTriggerPointType.Barrier, evnt), RavenEventTriggerPoint.Comparer);
                        }
                    }
                    ++startIndex;
                }
            } else {
                startIndex = ~startIndex;
            }

            for (int i = 0; i < m_CachedContinuousEventsForSearch.Count; ++i) {
                var continuousEvent = m_CachedContinuousEventsForSearch[i] as RavenContinuousEvent;
                var type = ERavenEventTriggerPointType.Start;
                if (continuousEvent.ShouldEndEvent(frame)) {
                    type = ERavenEventTriggerPointType.End;
                    m_CachedContinuousEventsForSearch.RemoveAt(i--);
                } else if (continuousEvent.ShouldProcessEvent(frame)) {
                    type = ERavenEventTriggerPointType.Process;
                } else {
                    // start
                    continuousEvent.Initialize(this);
                }

                var index = m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer.PopTriggerPoint(frame, type, continuousEvent), RavenEventTriggerPoint.Comparer);
                // we have to inject process here because we might need to interpolate in the starting frame multiple times
                // but OnEnter callback doesn't allow that
                if (type == ERavenEventTriggerPointType.Start) {
                    m_TempTriggerPointSortListForSearch.Insert(index + 1, RavenOverseer.PopTriggerPoint(frame, ERavenEventTriggerPointType.Process, continuousEvent));
                }

                // continuous events can be barriers too
                if (!placedBarrier && continuousEvent.IsBarrier) {
                    placedBarrier = true;
                    m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer.PopTriggerPoint(frame, ERavenEventTriggerPointType.Barrier, continuousEvent), RavenEventTriggerPoint.Comparer);
                }
            }

            // fill the frame list with the resulting trigger points
            List<RavenEventTriggerPoint> triggerList = null;
            if (m_TempTriggerPointSortListForSearch.Count > 0) {
                triggerList = RavenOverseer.PopTriggerPointList();
                for (int i = 0; i < m_TempTriggerPointSortListForSearch.Count; ++i) {
                    triggerList.Add(m_TempTriggerPointSortListForSearch[i]);
                }
                m_TempTriggerPointSortListForSearch.Clear();
            }
            return triggerList;
        }

        private static int BinarySearchIndexOfFirst(List<RavenEvent> list, int frame) {
            if (list == null) {
                throw new ArgumentNullException("list");
            }

            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper) {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = frame.CompareTo(list[middle].StartFrame);
                if (comparisonResult == 0) {
                    var lowest = middle;
                    while (lowest > 0 && frame.CompareTo(list[--lowest].StartFrame) == 0) {
                        middle = lowest;
                    }
                    return middle;
                } else if (comparisonResult < 0) {
                    upper = middle - 1;
                } else {
                    lower = middle + 1;
                }
            }

            return ~lower;
        }
    }
}