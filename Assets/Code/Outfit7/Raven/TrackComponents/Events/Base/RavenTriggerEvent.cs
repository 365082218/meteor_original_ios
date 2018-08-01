//{ "engine_register_objects":["Starlite__Raven__RavenTriggerEvent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenTriggerEvent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenTriggerEvent")]
        public abstract partial class RavenTriggerEvent : global::Starlite.Raven.RavenEvent {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::UnityEngine.GameObject m_Target;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenTriggerEvent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_ABSTRACT_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenTriggerEvent, RavenTriggerEvent, RavenTriggerEvent, Starlite::Raven::RavenEvent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTriggerEvent, m_Target, m_Target, kObject, kUnknown, Starlite::SceneObject::TypeId, "")
SIMPLEMENT_CLASS_END(RavenTriggerEvent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenTriggerEvent, Starlite::Raven, RavenTriggerEvent)
#endif
