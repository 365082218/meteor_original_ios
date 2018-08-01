#ifdef STARLITE
#pragma once

#include <Enums/RavenEventType.h>
#include <TrackComponents/RavenComponent.h>
#include <TrackComponents/Components/RavenCondition.h>
#include <TrackComponents/AnimationData/Base/RavenAnimationDataComponentBase.h>
#include <TrackComponents/Properties/Base/Interfaces/RavenPropertyComponent.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenSequence;

        namespace Internal {
            SENUM();
            enum class RavenEventFlags { Enabled = 1 << 0, TrackEnabled = 1 << 1, Default = Enabled | TrackEnabled };
            SENUM_FLAGS(RavenEventFlags);
        } // namespace Internal

        class RavenEvent : public RavenComponent {
            SCLASS_ABSTRACT(RavenEvent);

        public:
            RavenEvent();
            int GetDurationInFrames() const;
            int GetStartFrame() const;
            int GetSubTrackIndex() const;
            int GetTrackIndex() const;
            const List<Ref<RavenCondition>>& GetConditions() const;
            virtual bool IsBarrier() const = 0;
            virtual bool IsValid() const;
            virtual ERavenEventType GetEventType() const = 0;
            virtual int GetEndFrame() const = 0;
            virtual int GetLastFrame() const = 0;
            virtual const RavenAnimationDataComponentBase* GetAnimationDataEditorOnly() = 0;
            virtual const RavenPropertyComponent* GetPropertyComponent() const = 0;
            virtual void DestroyEditor(Ptr<RavenSequence> sequence);
            virtual void Initialize(Ptr<RavenSequence> sequence);
            virtual void InitializeEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target, int startFrame, int lastFrame, int trackIndex, int subTrackIndex);
            virtual void OffsetEvent(int nFrames);
            virtual void OnEnter(int frame) = 0;
            virtual void RecalculateFpsChange(double durationFactor);
            virtual void SetEndFrame(int frame) = 0;
            virtual void SetLastFrame(int frame) = 0;
            virtual void SetStartFrame(int frame);
            virtual void SetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) = 0;
            void SetSubTrackIndex(int subTrackIndex);
            void SetTrackIndex(int trackIndex);
            static int Comparer(const RavenEvent* event1, const RavenEvent* event2);

        protected:
            bool ConditionsMet() const;

        protected:
            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Internal\")"], Access : "protected");
            int m_StartFrame = 0;

            SPROPERTY(Access : "protected");
            int m_SubTrackIndex = 0;

            SPROPERTY(Access : "protected");
            int m_TrackIndex = 0;

            SPROPERTY(Access : "protected");
            List<Ref<RavenCondition>> m_Conditions;

        private:
            SPROPERTY(CustomAttributes : ["UnityEngine.HideInInspector"], Access : "private");
            Internal::RavenEventFlags m_Flags = Internal::RavenEventFlags::Default;
        };
    } // namespace Raven
} // namespace Starlite
#endif