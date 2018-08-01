#ifdef STARLITE
#pragma once

#include <RavenSequence.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenOverseer {
            friend class constructor;

        public:
            RavenOverseer() = delete;
            static void SetMaximumSequenceWarmupFramesPerFrame(int frames);
            static int GetAllocatedWarmupFrames(const RavenSequence* sequence, int maxFrames);
            static List<Ptr<RavenEventTriggerPoint>>* PopTriggerPointList();
            static List<Ptr<RavenPropertyComponent>>* PopPropertyComponentList();
            static Ptr<RavenEventTriggerPoint> PopTriggerPoint(int frame, ERavenEventTriggerPointType type, Ptr<RavenEvent> sequencerEvent);
            static Ptr<RavenSequence::IgnoredEventEntry> PopIgnoredEventEntry(Ptr<RavenEvent> evnt, int ignoreCount);
            static void PushIgnoredEventEntry(Ptr<RavenSequence::IgnoredEventEntry>& ignoredEventEntry);
            static void PushPropertyComponentList(List<Ptr<RavenPropertyComponent>>* list);
            static void PushTriggerPoint(Ptr<RavenEventTriggerPoint>& point);
            static void PushTriggerPointList(List<Ptr<RavenEventTriggerPoint>>* list);
            static void RegisterSequence(Ptr<RavenSequence> sequence);
            static void RegisterSequencePlay(Ptr<RavenSequence> sequence, bool forcefulWarmup);
            static void UnregisterSequence(Ptr<RavenSequence> sequence);

        private:
            static void InitRavenEventTriggerPointListPool();
            static void InitRavenEventTriggerPointPool();
            static void InitRavenIgnoredEventEntryPool();
            static void InitRavenPropertyComponentListPool();

        private:
            static const String Tag;

            static int s_MaximumSequenceWarmupFramesPerFrame;
            static Queue<List<Ptr<RavenEventTriggerPoint>>*> s_TriggerPointListPool;
            static Queue<List<Ptr<RavenPropertyComponent>>*> s_PropertyComponentListPool;
            static Queue<Ptr<RavenEventTriggerPoint>> s_TriggerPointPool;
            static Queue<Ptr<RavenSequence::IgnoredEventEntry>> s_IgnoredEventEntryPool;
            static HashMap<String, UInt64> s_SequencePlayCountTable;
            static int s_ActiveSequences;
            static int s_AllocatedFramesThisFrame;
            static UInt64 s_CurrentFrame;
            static int s_UsedIgnoredEventEntryCount;
            static int s_UsedPropertyComponentListCount;
            static int s_UsedTriggerPointCount;
            static int s_UsedTriggerPointListCount;
            static UInt64 s_MaximumSequencePlayCount;

            // static stuff
        private:
            struct constructor {
                constructor() {
                    InitRavenEventTriggerPointPool();
                    InitRavenEventTriggerPointListPool();
                    InitRavenIgnoredEventEntryPool();
                    InitRavenPropertyComponentListPool();
                }
            };

            static constructor constructor_;
        };
    } // namespace Raven
} // namespace Starlite
#endif