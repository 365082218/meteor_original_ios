#ifdef STARLITE
#include "RavenEventTriggerPoint.h"

namespace Starlite {
    namespace Raven {
        RavenEventTriggerPoint::RavenEventTriggerPoint()
            : m_IsPooled(true) {
        }

        bool RavenEventTriggerPoint::IsPooled() const {
            return m_IsPooled;
        }

        ERavenEventTriggerPointType RavenEventTriggerPoint::GetType() const {
            return m_Type;
        }

        int RavenEventTriggerPoint::GetFrame() const {
            return m_Frame;
        }

        Ptr<RavenEvent>& RavenEventTriggerPoint::GetRavenEvent() {
            return m_RavenEvent;
        }

        void RavenEventTriggerPoint::Initialize(int frame, ERavenEventTriggerPointType type, Ptr<RavenEvent>& sequencerEvent) {
            m_Frame = frame;
            m_Type = type;
            m_RavenEvent = sequencerEvent;
        }

        void RavenEventTriggerPoint::Reset() {
            m_Frame = 0;
            m_Type = ERavenEventTriggerPointType::Start;
            m_RavenEvent = nullptr;
        }

        void RavenEventTriggerPoint::SetIsPooled(bool value) {
            m_IsPooled = value;
        }

        int RavenEventTriggerPoint::Comparer(const RavenEventTriggerPoint* triggerPoint1, const RavenEventTriggerPoint* triggerPoint2) {
            DebugAssert(triggerPoint1->m_IsPooled == false, "%s (F: %d) is pooled when comparing!", triggerPoint1->m_RavenEvent->ToString().GetCString(), triggerPoint1->m_Frame);
            DebugAssert(triggerPoint2->m_IsPooled == false, "%s (F: %d) is pooled when comparing!", triggerPoint2->m_RavenEvent->ToString().GetCString(), triggerPoint2->m_Frame);

            int frameCompare = triggerPoint1->m_Frame - triggerPoint2->m_Frame;
            if (frameCompare != 0) {
                return frameCompare;
            }

            // end events have to be executed first at the beginning of the frame
            if (triggerPoint1->m_Type == ERavenEventTriggerPointType::End && triggerPoint2->m_Type < ERavenEventTriggerPointType::End) {
                return -1;
            }

            if (triggerPoint2->m_Type == ERavenEventTriggerPointType::End && triggerPoint1->m_Type < ERavenEventTriggerPointType::End) {
                return 1;
            }

            // barriers have to be executed after end events/bookmarks
            if (triggerPoint1->m_Type == ERavenEventTriggerPointType::Barrier && triggerPoint2->m_Type < ERavenEventTriggerPointType::Bookmark) {
                return -1;
            }

            if (triggerPoint2->m_Type == ERavenEventTriggerPointType::Barrier && triggerPoint1->m_Type < ERavenEventTriggerPointType::Bookmark) {
                return 1;
            }

            // bookmarks have track index of -1 so they'll be right after

            int trackIndexCompare = triggerPoint1->m_RavenEvent->GetTrackIndex() - triggerPoint2->m_RavenEvent->GetTrackIndex();
            if (trackIndexCompare != 0) {
                return trackIndexCompare;
            }

            return triggerPoint1->m_RavenEvent->GetSubTrackIndex() - triggerPoint2->m_RavenEvent->GetSubTrackIndex();
        }
    } // namespace Raven
} // namespace Starlite
#endif