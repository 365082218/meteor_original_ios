#ifdef STARLITE
#include "RavenBookmarkEvent.h"
#include "RavenBookmarkEvent.cs"

#include <Utility/RavenLog.h>
#include <RavenSequence.h>

using namespace Starlite::Raven::Internal;

namespace Starlite {
    namespace Raven {
        RavenBookmarkEvent::RavenBookmarkEvent() {
        }

        bool RavenBookmarkEvent::IsBarrier() const {
            return false;
        }

        ERavenEventType RavenBookmarkEvent::GetEventType() const {
            return ERavenEventType::Bookmark;
        }

        int RavenBookmarkEvent::GetEndFrame() const {
            return m_StartFrame;
        }

        int RavenBookmarkEvent::GetLastFrame() const {
            return m_StartFrame;
        }

        RavenBookmarkEvent::ERavenBookmarkType RavenBookmarkEvent::GetBookmarkType() const {
            return m_BookmarkType;
        }

        const RavenAnimationDataComponentBase* RavenBookmarkEvent::GetAnimationDataEditorOnly() {
            return nullptr;
        }

        RavenPropertyComponent* RavenBookmarkEvent::GetPropertyComponent() const {
            return nullptr;
        }

        const String& RavenBookmarkEvent::GetBookmarkName() const {
            return m_BookmarkName;
        }

        void RavenBookmarkEvent::Initialize(Ptr<RavenSequence> sequence) {
            RavenEvent::Initialize(sequence);
            m_Sequence = sequence;
        }

        void RavenBookmarkEvent::OnEnter(int frame) {
#if RAVEN_DEBUG
            pRavenLog->InfoT(RavenSequence::Tag.GetCString(), "%s OnEnter %d", this, frame);
#endif
            m_ConditionsMet = ConditionsMet();

            if (m_ConditionsMet) {
                switch (m_BookmarkType) {
                case ERavenBookmarkType::Pause:
                    m_Sequence->Pause();
                    break;

                case ERavenBookmarkType::Stop:
                    m_Sequence->Stop();
                    break;

                case ERavenBookmarkType::Loop:
                    String prevBookmark = m_Sequence->FindFirstBookmarkBefore(m_StartFrame);
                    if (prevBookmark != String()) {
                        m_Sequence->GoToBookmark(prevBookmark);
                    } else {
                        m_Sequence->JumpToFrame(0);
                    }
                    break;
                }
            }
        }

        void RavenBookmarkEvent::RecalculateFpsChange(double durationFactor) {
            m_StartFrame = (int)(m_StartFrame * durationFactor);
        }

        void RavenBookmarkEvent::SetBookmarkName(String value) {
            m_BookmarkName = value;
        }

        void RavenBookmarkEvent::SetBookmarkType(RavenBookmarkEvent::ERavenBookmarkType value) {
            m_BookmarkType = value;
        }

        void RavenBookmarkEvent::SetEndFrame(int frame) {
            SetStartFrame(frame);
        }

        void RavenBookmarkEvent::SetLastFrame(int frame) {
            SetStartFrame(frame);
        }

        void RavenBookmarkEvent::SetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) {
        }

        int RavenBookmarkEvent::Comparer(const RavenBookmarkEvent* event1, const RavenBookmarkEvent* event2) {
            return RavenEvent::Comparer(event1, event2);
        }
    } // namespace Raven
} // namespace Starlite

#endif