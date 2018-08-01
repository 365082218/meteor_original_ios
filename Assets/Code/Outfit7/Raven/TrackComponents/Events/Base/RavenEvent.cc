#ifdef STARLITE
#include "RavenEvent.h"
#include "RavenEvent.cs"

#include <Utility/RavenLog.h>
#include <RavenSequence.h>

using namespace Starlite::Raven::Internal;

namespace Starlite {
    namespace Raven {
        RavenEvent::RavenEvent() {
        }

        bool RavenEvent::ConditionsMet() const {
            for (auto& condition : m_Conditions) {
                if (!condition->IsTrue()) {
                    return false;
                }
            }
            return true;
        }

        int RavenEvent::GetDurationInFrames() const {
            return GetEndFrame() - m_StartFrame;
        }

        int RavenEvent::GetStartFrame() const {
            return m_StartFrame;
        }

        int RavenEvent::GetSubTrackIndex() const {
            return m_SubTrackIndex;
        }

        int RavenEvent::GetTrackIndex() const {
            return m_TrackIndex;
        }

        const List<Ref<RavenCondition>>& RavenEvent::GetConditions() const {
            return m_Conditions;
        }

        bool RavenEvent::IsValid() const {
            return (m_Flags & RavenEventFlags::Enabled) != 0 && (m_Flags & RavenEventFlags::TrackEnabled) != 0;
        }

        void RavenEvent::DestroyEditor(Ptr<RavenSequence> sequence) {
            // TODO: destroy it
        }

        void RavenEvent::Initialize(Ptr<RavenSequence> sequence) {
#ifdef RAVEN_DEBUG
            pRavenLog->InfoT(RavenSequence::Tag.GetCString(), "%s Initialize", this);
#endif

            for (auto& condition : m_Conditions) {
                condition->m_Parameter = sequence->GetParameterAtIndex(condition->m_ParameterIndex);
                condition->m_ValueParameter = sequence->GetParameterAtIndex(condition->m_ValueIndex);
            }
        }

        void RavenEvent::InitializeEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target, int startFrame, int lastFrame, int trackIndex, int subTrackIndex) {
            m_StartFrame = startFrame;
            m_TrackIndex = trackIndex;
            m_SubTrackIndex = subTrackIndex;
        }

        void RavenEvent::OffsetEvent(int nFrames) {
            m_StartFrame += nFrames;
        }

        void RavenEvent::RecalculateFpsChange(double durationFactor) {
            m_StartFrame = (int)(m_StartFrame * durationFactor);
        }

        void RavenEvent::SetStartFrame(int frame) {
            if (frame < 0) {
                return;
            }
            m_StartFrame = frame;
        }

        void RavenEvent::SetSubTrackIndex(int subTrackIndex) {
            if (m_SubTrackIndex == subTrackIndex) {
                return;
            }
            m_SubTrackIndex = subTrackIndex;
        }

        void RavenEvent::SetTrackIndex(int trackIndex) {
            if (m_TrackIndex == trackIndex) {
                return;
            }
            m_TrackIndex = trackIndex;
        }

        int RavenEvent::Comparer(const RavenEvent* event1, const RavenEvent* event2) {
            return event1->m_StartFrame - event2->m_StartFrame;
        }
    } // namespace Raven
} // namespace Starlite

#endif