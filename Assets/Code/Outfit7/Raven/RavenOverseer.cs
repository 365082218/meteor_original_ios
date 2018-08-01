using Starlite.Raven.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Starlite.Raven {

    public static class RavenOverseer {
        private const string Tag = "RavenOverseer";

        private static int s_MaximumSequenceWarmupFramesPerFrame = 1000;

        private static Queue<RavenEventTriggerPoint> s_TriggerPointPool;
        private static int s_UsedTriggerPointCount = 0;

        private static Queue<List<RavenEventTriggerPoint>> s_TriggerPointListPool;
        private static int s_UsedTriggerPointListCount = 0;

        private static Queue<RavenSequence.IgnoredEventEntry> s_IgnoredEventEntryPool;
        private static int s_UsedIgnoredEventEntryCount;

        private static Queue<List<RavenPropertyComponent>> s_PropertyComponentListPool;
        private static int s_UsedPropertyComponentListCount = 0;

        private static readonly Dictionary<string, ulong> s_SequencePlayCountTable;
        private static ulong s_MaximumSequencePlayCount;
        private static int s_ActiveSequences;
        private static int s_AllocatedFramesThisFrame;
        private static int s_CurrentFrame;

        static RavenOverseer() {
            s_CurrentFrame = -1;
            s_SequencePlayCountTable = new Dictionary<string, ulong>(1024);
            InitRavenEventTriggerPointPool();
            InitRavenEventTriggerPointListPool();
            InitRavenIgnoredEventEntryPool();
            InitRavenPropertyComponentListPool();
        }

        #region Utility

        public static void SetMaximumSequenceWarmupFramesPerFrame(int frames) {
            if (frames <= 0) {
                return;
            }
            s_MaximumSequenceWarmupFramesPerFrame = frames;
        }

        #endregion Utility

        #region SequenceWarmup

        public static void RegisterSequence(RavenSequence sequence) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            ++s_ActiveSequences;
        }

        public static void UnregisterSequence(RavenSequence sequence) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif
            --s_ActiveSequences;
        }

        public static void RegisterSequencePlay(RavenSequence sequence, bool forcefulWarmup) {
            var sequenceName = sequence.Name;
            ulong addPlayCount = 1;

            if (forcefulWarmup) {
                addPlayCount = 10;
            }

            ulong playCount = 0;
            if (!s_SequencePlayCountTable.TryGetValue(sequenceName, out playCount)) {
                playCount = addPlayCount;
            } else {
                playCount += addPlayCount;
            }

            s_SequencePlayCountTable[sequenceName] = playCount;

            if (playCount > s_MaximumSequencePlayCount) {
                s_MaximumSequencePlayCount = playCount;
            }
        }

        public static int GetAllocatedWarmupFrames(RavenSequence sequence, int maxFrames) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return maxFrames;
            }
#endif

            if (s_CurrentFrame != Time.frameCount) {
                s_CurrentFrame = Time.frameCount;
                s_AllocatedFramesThisFrame = 0;
            }

            if (s_AllocatedFramesThisFrame >= s_MaximumSequenceWarmupFramesPerFrame) {
                return 0;
            }

            ulong playCount;
            if (!s_SequencePlayCountTable.TryGetValue(sequence.Name, out playCount)) {
                playCount = 0;
            }

            var percentageOfMax = s_MaximumSequencePlayCount == 0 ? 1.0 : (playCount / (double)s_MaximumSequencePlayCount);
            var multiplier = 1 / (1.5 - percentageOfMax);
            var allocatedFrames = Math.Min((int)Math.Ceiling(Math.Max(s_MaximumSequenceWarmupFramesPerFrame / s_ActiveSequences, 1) * multiplier), maxFrames);
            s_AllocatedFramesThisFrame += allocatedFrames;
#if RAVEN_DEBUG
            RavenLog.DebugT(Tag, "Allocated {0} frames for {1}. PlayCount: {2} MaximumPlayCount: {3} FramesLeft: {4}", allocatedFrames, sequence.Name, playCount, s_MaximumSequencePlayCount, MaximumSequenceWarmupFramesPerFrame - s_AllocatedFramesThisFrame);
#endif

            return allocatedFrames;
        }

        #endregion SequenceWarmup

        #region Pools

        public static RavenEventTriggerPoint PopTriggerPoint(int frame, ERavenEventTriggerPointType type, RavenEvent sequencerEvent) {
            const int addSize = 2000;

            if (s_TriggerPointPool.Count == 0) {
                RavenLog.DebugT(Tag, "TrigerPoint pool empty. Adding {0} elements. Total used {1}.", addSize, s_UsedTriggerPointCount);

                for (int i = 0; i < addSize; ++i) {
                    s_TriggerPointPool.Enqueue(new RavenEventTriggerPoint());
                }
            }

            var point = s_TriggerPointPool.Dequeue();
#if RAVEN_DEBUG
            RavenAssert.IsTrue(point.IsPooled == true, "TriggerPoint was not pooled when taking it from the pool!");
#endif
            point.Initialize(frame, type, sequencerEvent);
            point.IsPooled = false;
            ++s_UsedTriggerPointCount;

            return point;
        }

        public static void PushTriggerPoint(RavenEventTriggerPoint point) {
            if (point == null) {
                return;
            }

#if RAVEN_DEBUG
            RavenAssert.IsTrue(point.IsPooled == false, "TriggerPoint was already pooled when pushing it back to the pool!");
#endif
            point.Reset();
            point.IsPooled = true;
            --s_UsedTriggerPointCount;

            s_TriggerPointPool.Enqueue(point);
        }

        private static void InitRavenEventTriggerPointPool() {
            const int poolSize = 20000;

            s_TriggerPointPool = new Queue<RavenEventTriggerPoint>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                s_TriggerPointPool.Enqueue(new RavenEventTriggerPoint());
            }
        }

        public static List<RavenEventTriggerPoint> PopTriggerPointList() {
            const int addSize = 2000;

            if (s_TriggerPointListPool.Count == 0) {
                RavenLog.DebugT(Tag, "TrigerPointList pool empty. Adding {0} elements. Total used {1}.", addSize, s_UsedTriggerPointListCount);

                for (int i = 0; i < addSize; ++i) {
                    s_TriggerPointListPool.Enqueue(new List<RavenEventTriggerPoint>(8));
                }
            }

            var list = s_TriggerPointListPool.Dequeue();
            ++s_UsedTriggerPointListCount;

            return list;
        }

        public static void PushTriggerPointList(List<RavenEventTriggerPoint> list) {
            if (list == null) {
                return;
            }

            for (int i = 0; i < list.Count; ++i) {
                PushTriggerPoint(list[i]);
            }

            list.Clear();
            --s_UsedTriggerPointListCount;

            s_TriggerPointListPool.Enqueue(list);
        }

        private static void InitRavenEventTriggerPointListPool() {
            const int poolSize = 20000;

            s_TriggerPointListPool = new Queue<List<RavenEventTriggerPoint>>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                s_TriggerPointListPool.Enqueue(new List<RavenEventTriggerPoint>(8));
            }
        }

        public static RavenSequence.IgnoredEventEntry PopIgnoredEventEntry(RavenEvent evnt, int ignoreCount) {
            const int addSize = 16;

            if (s_IgnoredEventEntryPool.Count == 0) {
                RavenLog.DebugT(Tag, "IgnoredEventEntry pool empty. Adding {0} elements. Total used {1}.", addSize, s_UsedIgnoredEventEntryCount);

                for (int i = 0; i < addSize; ++i) {
                    s_IgnoredEventEntryPool.Enqueue(new RavenSequence.IgnoredEventEntry());
                }
            }

            var ignoredEventEntry = s_IgnoredEventEntryPool.Dequeue();
            ignoredEventEntry.Initialize(evnt, ignoreCount);
            ++s_UsedIgnoredEventEntryCount;

            return ignoredEventEntry;
        }

        public static void PushIgnoredEventEntry(RavenSequence.IgnoredEventEntry ignoredEventEntry) {
            if (ignoredEventEntry == null) {
                return;
            }

            ignoredEventEntry.Reset();
            --s_UsedIgnoredEventEntryCount;

            s_IgnoredEventEntryPool.Enqueue(ignoredEventEntry);
        }

        private static void InitRavenIgnoredEventEntryPool() {
            const int poolSize = 20;

            s_IgnoredEventEntryPool = new Queue<RavenSequence.IgnoredEventEntry>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                s_IgnoredEventEntryPool.Enqueue(new RavenSequence.IgnoredEventEntry());
            }
        }

        public static List<RavenPropertyComponent> PopPropertyComponentList() {
            const int addSize = 20;

            if (s_PropertyComponentListPool.Count == 0) {
                RavenLog.DebugT(Tag, "PropertyComponentList pool empty. Adding {0} elements. Total used {1}.", addSize, s_UsedPropertyComponentListCount);

                for (int i = 0; i < addSize; ++i) {
                    s_PropertyComponentListPool.Enqueue(new List<RavenPropertyComponent>(8));
                }
            }

            var list = s_PropertyComponentListPool.Dequeue();
            ++s_UsedPropertyComponentListCount;

            return list;
        }

        public static void PushPropertyComponentList(List<RavenPropertyComponent> list) {
            if (list == null) {
                return;
            }

            list.Clear();
            --s_UsedPropertyComponentListCount;

            s_PropertyComponentListPool.Enqueue(list);
        }

        private static void InitRavenPropertyComponentListPool() {
            const int poolSize = 100;

            s_PropertyComponentListPool = new Queue<List<RavenPropertyComponent>>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                s_PropertyComponentListPool.Enqueue(new List<RavenPropertyComponent>(8));
            }
        }

        #endregion Pools
    }
}