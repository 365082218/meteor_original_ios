#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/Base/Interfaces/RavenPropertyComponent.h>
#include <TrackComponents/AnimationData/Base/RavenAnimationDataComponentBase.h>
#include <TrackComponents/Properties/Base/RavenTriggerPropertyComponentBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationPropertyComponentBase : public RavenPropertyComponent {
            SCLASS_ABSTRACT(RavenAnimationPropertyComponentBase);

        public:
            RavenAnimationPropertyComponentBase();
            bool IsTriggerPropertyValid() const;
            bool IsValid() const;
            const RavenPropertyComponent* GetChildPropertyComponent() const override;
            String GetComponentType() const;
            String GetMemberName();
            const String& GetMemberName() const;
            UInt64 GetTargetHash() const;
            virtual bool IsCustom() const = 0;
            virtual Ref<RavenAnimationDataComponentBase>& GetAnimationData() = 0;
            virtual Ref<RavenTriggerPropertyComponentBase>& GetTriggerProperty() = 0;
            virtual void EvaluateAtTime(double time, double duration) = 0;
            virtual void OnExit() = 0;
            virtual void SetAnimationData(Ref<RavenAnimationDataComponentBase>& value) = 0;
            virtual void SetTriggerProperty(Ref<RavenTriggerPropertyComponentBase>& value) = 0;
            void DestroyEditor(Ptr<RavenSequence> sequence) override;
            void Initialize(Ptr<RavenSequence> sequence) override;
            void SetComponentType(String value);
            void SetMemberName(String value);
            void SetTargetHash(UInt64 value);

        protected:
            virtual bool IsCustomValid() const = 0;

        protected:
            SPROPERTY(Access : "protected");
            Ref<RavenTriggerPropertyComponentBase> m_TriggerProperty;

            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Internal\")"], Access : "protected");
            Ref<RavenAnimationDataComponentBase> m_AnimationData;

        private:
            SPROPERTY(Access : "private");
            String m_ComponentType;

            SPROPERTY(Access : "private");
            String m_MemberName;

            SPROPERTY(Access : "private");
            UInt64 m_TargetHash;
        };
    } // namespace Raven
} // namespace Starlite
#endif