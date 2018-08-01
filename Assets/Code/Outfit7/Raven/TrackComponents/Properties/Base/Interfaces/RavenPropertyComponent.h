#ifdef STARLITE
#pragma once

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenSequence;

        class RavenPropertyComponent : public SceneObjectComponent {
            SCLASS_ABSTRACT(RavenPropertyComponent);

        public:
            RavenPropertyComponent();
            const Ref<Object>& GetTargetComponent() const;
            const Ref<SceneObject>& GetTarget() const;
            virtual bool CheckForDependencies() const = 0;
            virtual int GetParameterIndex() const = 0;
            virtual const RavenPropertyComponent* GetChildPropertyComponent() const = 0;
            virtual void DestroyEditor(Ptr<RavenSequence> sequence);
            virtual void Initialize(Ptr<RavenSequence> sequence);
            virtual void OnEnter() = 0;
            void SetTarget(Ref<SceneObject>& value);
            void SetTargetComponent(Ref<Object>& value);
            void SetTargets(const List<Ref<SceneObject>>& gameObjects);

        protected:
            bool HasOverridenTargetComponents();
            virtual void OnSetTargets(const List<Ref<SceneObject>>& gameObjects);

        protected:
            SPROPERTY(Access : "protected");
            Ref<Object> m_TargetComponent;

            SPROPERTY(Access : "protected");
            Ref<SceneObject> m_Target;

            List<Ref<Object>> m_OverridenTargetComponents;

        private:
            bool m_HasOverridenTargetComponents = false;
        };
    } // namespace Raven
} // namespace Starlite
#endif