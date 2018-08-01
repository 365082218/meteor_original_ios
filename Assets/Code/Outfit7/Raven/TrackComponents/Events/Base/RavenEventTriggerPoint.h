#ifdef STARLITE
#pragma once

#include <Enums/RavenEventTriggerPointType.h>
#include <TrackComponents/Events/Base/RavenEvent.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenEventTriggerPoint : public MemoryObject<> {
        public:
            RavenEventTriggerPoint();

            bool IsPooled() const;
            ERavenEventTriggerPointType GetType() const;
            int GetFrame() const;
            Ptr<RavenEvent>& GetRavenEvent();
            void Initialize(int frame, ERavenEventTriggerPointType type, Ptr<RavenEvent>& sequencerEvent);
            void Reset();
            void SetIsPooled(bool value);

        public:
            static int Comparer(const RavenEventTriggerPoint* triggerPoint1, const RavenEventTriggerPoint* triggerPoint2);

        private:
            bool m_IsPooled;
            ERavenEventTriggerPointType m_Type;
            int m_Frame;
            Ptr<RavenEvent> m_RavenEvent;
        };
    } // namespace Raven
} // namespace Starlite
#endif