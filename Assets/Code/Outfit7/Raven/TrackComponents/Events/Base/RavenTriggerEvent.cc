#ifdef STARLITE
#include "RavenTriggerEvent.h"
#include "RavenTriggerEvent.cs"

#include <Utility/RavenLog.h>
#include <RavenSequence.h>

using namespace Starlite::Raven::Internal;

namespace Starlite {
    namespace Raven {
        RavenTriggerEvent::RavenTriggerEvent() {
        }

        ERavenEventType RavenTriggerEvent::GetEventType() const {
            return ERavenEventType::Trigger;
        }

        int RavenTriggerEvent::GetEndFrame() const {
            return m_StartFrame;
        }

        int RavenTriggerEvent::GetLastFrame() const {
            return m_StartFrame;
        }

        const Ref<SceneObject>& RavenTriggerEvent::GetTarget() const {
            return m_Target;
        }

        void RavenTriggerEvent::InitializeEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target, int startFrame, int lastFrame, int trackIndex, int subTrackIndex) {
            RavenEvent::InitializeEditor(sequence, target, startFrame, lastFrame, trackIndex, subTrackIndex);
            m_Target = target;
        }

        void RavenTriggerEvent::OnEnter(int frame) {
#ifdef RAVEN_DEBUG
            pRavenLog->InfoT(RavenSequence::Tag.GetCString(), "%s OnEnter %d", this, frame);
#endif

            m_ConditionsMet = ConditionsMet();

            if (m_ConditionsMet) {
                OnEnterCallback(frame);
            }
        }

        void RavenTriggerEvent::SetEndFrame(int frame) {
            SetStartFrame(frame);
        }

        void RavenTriggerEvent::SetLastFrame(int frame) {
            SetStartFrame(frame);
        }

        void RavenTriggerEvent::SetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) {
            m_Target = target;
            OnSetTargetEditor(sequence, target);
        }
    } // namespace Raven
} // namespace Starlite

#endif