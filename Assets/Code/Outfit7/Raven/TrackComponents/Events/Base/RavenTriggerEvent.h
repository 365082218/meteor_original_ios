#ifdef STARLITE
#pragma once

#include <TrackComponents/Events/Base/RavenEvent.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenTriggerEvent : public RavenEvent {
            SCLASS_ABSTRACT(RavenTriggerEvent);

        public:
            RavenTriggerEvent();
            ERavenEventType GetEventType() const final;
            int GetEndFrame() const final;
            int GetLastFrame() const final;
            const Ref<SceneObject>& GetTarget() const;
            void InitializeEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target, int startFrame, int lastFrame, int trackIndex, int subTrackIndex) override;
            void OnEnter(int frame) final;
            void SetEndFrame(int frame) final;
            void SetLastFrame(int frame) final;
            void SetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) final;

        protected:
            virtual void OnEnterCallback(int frame) = 0;
            virtual void OnSetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) = 0;

        protected:
            SPROPERTY(Access : "protected");
            Ref<SceneObject> m_Target;

        private:
            bool m_ConditionsMet;
        };
    } // namespace Raven
} // namespace Starlite
#endif