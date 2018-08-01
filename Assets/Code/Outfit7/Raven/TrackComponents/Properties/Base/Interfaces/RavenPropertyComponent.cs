//{ "engine_register_objects":["Starlite__Raven__RavenPropertyComponent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenPropertyComponent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenPropertyComponent")]
        public abstract partial class RavenPropertyComponent : global::Starlite.SceneObjectComponent {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::UnityEngine.Object m_TargetComponent;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::UnityEngine.GameObject m_Target;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenPropertyComponent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_ABSTRACT_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenPropertyComponent, RavenPropertyComponent, RavenPropertyComponent, Starlite::SceneObjectComponent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenPropertyComponent, m_TargetComponent, m_TargetComponent, kObject, kUnknown, Starlite::Object::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenPropertyComponent, m_Target, m_Target, kObject, kUnknown, Starlite::SceneObject::TypeId, "")
SIMPLEMENT_CLASS_END(RavenPropertyComponent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenPropertyComponent, Starlite::Raven, RavenPropertyComponent)
#endif
