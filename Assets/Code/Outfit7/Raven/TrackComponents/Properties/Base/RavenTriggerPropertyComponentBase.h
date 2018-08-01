#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/Base/Interfaces/RavenPropertyComponent.h>

namespace Starlite {
    namespace Raven {
        class RavenTriggerPropertyComponentBase : public RavenPropertyComponent {
            SCLASS_ABSTRACT(RavenTriggerPropertyComponentBase);

        public:
            RavenTriggerPropertyComponentBase();
            bool CheckForDependencies() const final;
            bool IsValid() const;
            int GetParameterIndex() const override;
            const RavenPropertyComponent* GetChildPropertyComponent() const override;
            String GetComponentType() const;
            String GetFullFunctionName() const;
            String GetFunctionName() const;
            UInt64 GetTargetHash() const;
            void Initialize(Ptr<RavenSequence> sequence) override;
            bool OnBuild(int pass) override;
            void SetComponentType(String value);
            void SetFunctionName(String value);
            void SetParameterIndexEditor(int parameterIndex);
            void SetTargetHash(UInt64 value);

        protected:
            SPROPERTY(Access : "private");
            int m_ParameterIndex = -1;

            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Internal\")"], Access : "private");
            String m_ComponentType;

            SPROPERTY(Access : "private");
            String m_FunctionName;

            SPROPERTY(Access : "private");
            UInt64 m_TargetHash;

            String m_StrippedFunctionName;
            Ptr<ReflectionFunction> m_Function;
        };
    } // namespace Raven
} // namespace Starlite

#endif