#ifdef STARLITE
#pragma once

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenSequence;
        class RavenAnimationPropertyComponentBase;

        class RavenAnimationDataComponentBase : public SceneObjectComponent {
            SCLASS_ABSTRACT(RavenAnimationDataComponentBase);

        public:
            RavenAnimationDataComponentBase();
            virtual void DestroyEditor(Ptr<RavenSequence> sequence);
            virtual void Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) = 0;
            void CopyValuesFrom(const RavenAnimationDataComponentBase* other);

        protected:
            virtual void CopyValuesCallback(const RavenAnimationDataComponentBase* other) = 0;
        };
    } // namespace Raven
} // namespace Starlite
#endif