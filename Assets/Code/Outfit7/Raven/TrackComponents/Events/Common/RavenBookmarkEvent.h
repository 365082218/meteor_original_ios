#ifdef STARLITE
#pragma once

#include <TrackComponents/Events/Base/RavenEvent.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenBookmarkEvent : public RavenEvent {
            SCLASS_SEALED(RavenBookmarkEvent);

            SENUM() enum class ERavenBookmarkType { Ignore = 0, Pause, Stop, Loop };

        public:
            RavenBookmarkEvent();
            bool IsBarrier() const final;
            ERavenEventType GetEventType() const final;
            int GetEndFrame() const final;
            int GetLastFrame() const final;
            RavenBookmarkEvent::ERavenBookmarkType GetBookmarkType() const;
            const RavenAnimationDataComponentBase* GetAnimationDataEditorOnly() final;
            RavenPropertyComponent* GetPropertyComponent() const override;
            const String& GetBookmarkName() const;
            void Initialize(Ptr<RavenSequence> sequence) final;
            void OnEnter(int frame) final;
            void RecalculateFpsChange(double durationFactor) override;
            void SetBookmarkName(String value);
            void SetBookmarkType(RavenBookmarkEvent::ERavenBookmarkType value);
            void SetEndFrame(int frame) final;
            void SetLastFrame(int frame) final;
            void SetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) final;
            static int Comparer(const RavenBookmarkEvent* event1, const RavenBookmarkEvent* event2);

        private:
            SPROPERTY(Access : "private");
            RavenBookmarkEvent::ERavenBookmarkType m_BookmarkType = ERavenBookmarkType::Ignore;

            SPROPERTY(Access : "private");
            String m_BookmarkName = "Le Bookmark";

            bool m_ConditionsMet;
            Ref<RavenSequence> m_Sequence;
        };
    } // namespace Raven
} // namespace Starlite
#endif