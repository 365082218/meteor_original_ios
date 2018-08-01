#ifdef STARLITE
#include "RavenPropertyComponent.h"
#include "RavenPropertyComponent.cs"

#include <Utility/RavenLog.h>
#include <RavenSequence.h>

using namespace Starlite::Raven::Internal;

namespace Starlite {
    namespace Raven {
        RavenPropertyComponent::RavenPropertyComponent() {
        }

        bool RavenPropertyComponent::HasOverridenTargetComponents() {
            return m_HasOverridenTargetComponents;
        }

        const Ref<Object>& RavenPropertyComponent::GetTargetComponent() const {
            return m_TargetComponent;
        }

        const Ref<SceneObject>& RavenPropertyComponent::GetTarget() const {
            return m_Target;
        }

        void RavenPropertyComponent::DestroyEditor(Ptr<RavenSequence> sequence) {
        }

        void RavenPropertyComponent::Initialize(Ptr<RavenSequence> sequence) {
        }

        void RavenPropertyComponent::OnSetTargets(const List<Ref<SceneObject>>& gameObjects) {
        }

        void RavenPropertyComponent::SetTarget(Ref<SceneObject>& value) {
            m_Target = value;
        }

        void RavenPropertyComponent::SetTargetComponent(Ref<Object>& value) {
            m_TargetComponent = value;
        }

        void RavenPropertyComponent::SetTargets(const List<Ref<SceneObject>>& gameObjects) {
            // if it comes here, it will _always_ have override target components (even if null or empty)
            bool wasOverriden = m_HasOverridenTargetComponents;
            m_HasOverridenTargetComponents = true;

            bool sameTargets = wasOverriden && gameObjects.Count() == m_OverridenTargetComponents.Count();
            bool targetComponentIsGameObject = m_TargetComponent->IsDerivedFrom(SceneObject::TypeId);

            if (gameObjects.Count() == 0) {
                m_OverridenTargetComponents.Clear();
                return;
            }

            for (Size_T i = 0; i < gameObjects.Count(); ++i) {
                if (gameObjects[i] == nullptr) {
                    pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "Object %llu/%llu is null when setting actor list on %s!", i, gameObjects.Count(), this);
                    return;
                } else if (sameTargets) {
                    if (targetComponentIsGameObject) {
                        sameTargets &= m_OverridenTargetComponents[i] == gameObjects[i];
                    } else {
                        sameTargets &=
                        m_OverridenTargetComponents[i] != nullptr && reinterpret_cast<SceneObjectComponent*>(m_OverridenTargetComponents[i].GetObject())->GetSceneObject() == gameObjects[i];
                    }
                }
            }

            if (sameTargets) {
                return;
            }

            m_OverridenTargetComponents.Clear();
            if (m_OverridenTargetComponents.Size() < gameObjects.Count()) {
                m_OverridenTargetComponents.SetSize(gameObjects.Count());
            }
            for (Size_T i = 0; i < gameObjects.Count(); ++i) {
                auto& gameObject = gameObjects[i];
                if (targetComponentIsGameObject) {
                    m_OverridenTargetComponents.Add(gameObject);
                } else {
                    auto component = gameObject->FindComponent(m_TargetComponent->GetObjectTypeId());
                    Assert(component != nullptr, "Failed to find component of type %s on %s!", m_TargetComponent->GetObjectTypeName(), gameObject->ToString().GetCString());
                    m_OverridenTargetComponents.Add(component);
                }
            }

            OnSetTargets(gameObjects);
        }
    } // namespace Raven
} // namespace Starlite
#endif