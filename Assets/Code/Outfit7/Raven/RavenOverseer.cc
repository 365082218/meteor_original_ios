#ifdef STARLITE
#include "RavenOverseer.h"

#include <Utility/RavenLog.h>

using namespace Starlite::Raven::Internal;

namespace Starlite {
    namespace Raven {
        const String RavenOverseer::Tag = "RavenOverseer";
        int RavenOverseer::s_MaximumSequenceWarmupFramesPerFrame = 1000;

        Queue<List<Ptr<RavenEventTriggerPoint>>*> RavenOverseer::s_TriggerPointListPool;
        Queue<List<Ptr<RavenPropertyComponent>>*> RavenOverseer::s_PropertyComponentListPool;
        Queue<Ptr<RavenEventTriggerPoint>> RavenOverseer::s_TriggerPointPool;
        Queue<Ptr<RavenSequence::IgnoredEventEntry>> RavenOverseer::s_IgnoredEventEntryPool;
        HashMap<String, UInt64> RavenOverseer::s_SequencePlayCountTable = HashMap<String, UInt64>(1024);
        int RavenOverseer::s_ActiveSequences = 0;
        int RavenOverseer::s_AllocatedFramesThisFrame = 0;
        UInt64 RavenOverseer::s_CurrentFrame = -1;
        int RavenOverseer::s_UsedIgnoredEventEntryCount = 0;
        int RavenOverseer::s_UsedPropertyComponentListCount = 0;
        int RavenOverseer::s_UsedTriggerPointCount = 0;
        int RavenOverseer::s_UsedTriggerPointListCount = 0;
        UInt64 RavenOverseer::s_MaximumSequencePlayCount = 0;

        RavenOverseer::constructor RavenOverseer::constructor_;

        void RavenOverseer::SetMaximumSequenceWarmupFramesPerFrame(int frames) {
            DebugAssert(frames > 0, "SetMaximumSequenceWarmupFramesPerFrame frames <= 0 (%d)", frames);
            s_MaximumSequenceWarmupFramesPerFrame = frames;
        }

        int RavenOverseer::GetAllocatedWarmupFrames(const RavenSequence* sequence, int maxFrames) {
            UInt64 frameCount = pTime->GetFrameCount();
            if (s_CurrentFrame != frameCount) {
                s_CurrentFrame = frameCount;
                s_AllocatedFramesThisFrame = 0;
            }

            if (s_AllocatedFramesThisFrame >= s_MaximumSequenceWarmupFramesPerFrame) {
                return 0;
            }

            UInt64 playCount = 0;
            auto it = s_SequencePlayCountTable.Find(sequence->GetSequenceName());
            if (it != s_SequencePlayCountTable.end()) {
                playCount = it->value;
            }

            double percentageOfMax = s_MaximumSequencePlayCount == 0 ? 1.0 : (playCount / (double)s_MaximumSequencePlayCount);
            double multiplier = 1 / (1.5 - percentageOfMax);
            int allocatedFrames = Math::Min((int)Math::Ceil(Math::Max(s_MaximumSequenceWarmupFramesPerFrame / s_ActiveSequences, 1) * multiplier), maxFrames);
            s_AllocatedFramesThisFrame += allocatedFrames;

#ifdef RAVEN_DEBUG
            pRavenLog->InfoT(Tag.GetCString(), "Allocated %d frames for %s. PlayCount: %llu MaximumPlayCount: %llu FramesLeft: %d", allocatedFrames, sequence->GetSequenceName(), playCount,
                             s_MaximumSequencePlayCount, MaximumSequenceWarmupFramesPerFrame - s_AllocatedFramesThisFrame);
#endif

            return allocatedFrames;
        }

        List<Ptr<RavenEventTriggerPoint>>* RavenOverseer::PopTriggerPointList() {
            const int addSize = 2000;

            if (s_TriggerPointListPool.Count() == 0) {
                pRavenLog->InfoT(Tag.GetCString(), "TrigerPointList pool empty. Adding %d elements. Total used %d.", addSize, s_UsedTriggerPointListCount);

                for (int i = 0; i < addSize; ++i) {
                    s_TriggerPointListPool.Enqueue(new List<Ptr<RavenEventTriggerPoint>>(8));
                }
            }

            List<Ptr<RavenEventTriggerPoint>>* list;
            Assert(s_TriggerPointListPool.Dequeue(&list), "Dequeue failed!");
            ++s_UsedTriggerPointListCount;

            return list;
        }

        List<Ptr<RavenPropertyComponent>>* RavenOverseer::PopPropertyComponentList() {
            const int addSize = 20;

            if (s_PropertyComponentListPool.Count() == 0) {
                pRavenLog->InfoT(Tag.GetCString(), "PropertyComponentList pool empty. Adding %d elements. Total used %d.", addSize, s_UsedPropertyComponentListCount);

                for (int i = 0; i < addSize; ++i) {
                    s_PropertyComponentListPool.Enqueue(new List<Ptr<RavenPropertyComponent>>(8));
                }
            }

            List<Ptr<RavenPropertyComponent>>* list;
            Assert(s_PropertyComponentListPool.Dequeue(&list), "Dequeue failed!");
            ++s_UsedPropertyComponentListCount;

            return list;
        }

        Ptr<RavenEventTriggerPoint> RavenOverseer::PopTriggerPoint(int frame, ERavenEventTriggerPointType type, Ptr<RavenEvent> sequencerEvent) {
            const int addSize = 2000;

            if (s_TriggerPointPool.Count() == 0) {
                pRavenLog->InfoT(Tag.GetCString(), "TrigerPoint pool empty. Adding %d elements. Total used %d.", addSize, s_UsedTriggerPointCount);

                for (int i = 0; i < addSize; ++i) {
                    s_TriggerPointPool.Enqueue(new RavenEventTriggerPoint());
                }
            }

            Ptr<RavenEventTriggerPoint> point;
            Assert(s_TriggerPointPool.Dequeue(&point), "Dequeue failed!");
            DebugAssert(point->IsPooled() == true, "TriggerPoint was not pooled when taking it from the pool!");

            point->Initialize(frame, type, sequencerEvent);
            point->SetIsPooled(false);
            ++s_UsedTriggerPointCount;

            return point;
        }

        Ptr<RavenSequence::IgnoredEventEntry> RavenOverseer::PopIgnoredEventEntry(Ptr<RavenEvent> evnt, int ignoreCount) {
            const int addSize = 16;

            if (s_IgnoredEventEntryPool.Count() == 0) {
                pRavenLog->InfoT(Tag.GetCString(), "IgnoredEventEntry pool empty. Adding %d elements. Total used %d.", addSize, s_UsedIgnoredEventEntryCount);

                for (int i = 0; i < addSize; ++i) {
                    s_IgnoredEventEntryPool.Enqueue(new RavenSequence::IgnoredEventEntry());
                }
            }

            Ptr<RavenSequence::IgnoredEventEntry> ignoredEventEntry;
            Assert(s_IgnoredEventEntryPool.Dequeue(&ignoredEventEntry), "Dequeue failed!");
            ignoredEventEntry->Initialize(evnt, ignoreCount);
            ++s_UsedIgnoredEventEntryCount;

            return ignoredEventEntry;
        }

        void RavenOverseer::PushIgnoredEventEntry(Ptr<RavenSequence::IgnoredEventEntry>& ignoredEventEntry) {
            if (ignoredEventEntry == nullptr) {
                return;
            }

            ignoredEventEntry->Reset();
            --s_UsedIgnoredEventEntryCount;

            s_IgnoredEventEntryPool.Enqueue(ignoredEventEntry);
        }

        void RavenOverseer::PushPropertyComponentList(List<Ptr<RavenPropertyComponent>>* list) {
            list->Clear();
            --s_UsedPropertyComponentListCount;

            s_PropertyComponentListPool.Enqueue(list);
        }

        void RavenOverseer::PushTriggerPoint(Ptr<RavenEventTriggerPoint>& point) {
            if (point == nullptr) {
                return;
            }

            DebugAssert(point->IsPooled() == false, "TriggerPoint was already pooled when pushing it back to the pool!");

            point->Reset();
            point->SetIsPooled(true);
            --s_UsedTriggerPointCount;

            s_TriggerPointPool.Enqueue(point);
        }

        void RavenOverseer::PushTriggerPointList(List<Ptr<RavenEventTriggerPoint>>* list) {
            if (list == nullptr) {
                return;
            }

            for (auto& el : *list) {
                PushTriggerPoint(el);
            }

            list->Clear();
            --s_UsedTriggerPointListCount;

            s_TriggerPointListPool.Enqueue(list);
        }

        void RavenOverseer::RegisterSequence(Ptr<RavenSequence> sequence) {
            ++s_ActiveSequences;
        }

        void RavenOverseer::RegisterSequencePlay(Ptr<RavenSequence> sequence, bool forcefulWarmup) {
            String sequenceName = sequence->GetSequenceName();
            UInt64 addPlayCount = 1;

            if (forcefulWarmup) {
                addPlayCount = 10;
            }

            UInt64 playCount = 0;
            auto it = s_SequencePlayCountTable.Find(sequenceName);
            if (it != s_SequencePlayCountTable.end()) {
                playCount += addPlayCount;
            } else {
                playCount = addPlayCount;
            }

            s_SequencePlayCountTable[sequenceName] = playCount;

            if (playCount > s_MaximumSequencePlayCount) {
                s_MaximumSequencePlayCount = playCount;
            }
        }

        void RavenOverseer::UnregisterSequence(Ptr<RavenSequence> sequence) {
            --s_ActiveSequences;
        }

        void RavenOverseer::InitRavenEventTriggerPointListPool() {
            const int poolSize = 20000;

            s_TriggerPointListPool = Queue<List<Ptr<RavenEventTriggerPoint>>*>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                s_TriggerPointListPool.Enqueue(new List<Ptr<RavenEventTriggerPoint>>(8));
            }
        }

        void RavenOverseer::InitRavenEventTriggerPointPool() {
            const int poolSize = 20000;

            s_TriggerPointPool = Queue<Ptr<RavenEventTriggerPoint>>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                s_TriggerPointPool.Enqueue(new RavenEventTriggerPoint());
            }
        }

        void RavenOverseer::InitRavenIgnoredEventEntryPool() {
            const int poolSize = 20;

            s_IgnoredEventEntryPool = Queue<Ptr<RavenSequence::IgnoredEventEntry>>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                s_IgnoredEventEntryPool.Enqueue(new RavenSequence::IgnoredEventEntry());
            }
        }

        void RavenOverseer::InitRavenPropertyComponentListPool() {
            const int poolSize = 100;

            s_PropertyComponentListPool = Queue<List<Ptr<RavenPropertyComponent>>*>(poolSize);
            for (int i = 0; i < poolSize; ++i) {
                s_PropertyComponentListPool.Enqueue(new List<Ptr<RavenPropertyComponent>>(8));
            }
        }
    } // namespace Raven
} // namespace Starlite

#endif
