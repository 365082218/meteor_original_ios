//{ "engine_register_objects":["Starlite__Raven__RavenPropertyEvent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenPropertyEvent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenPropertyEvent")]
        public sealed partial class RavenPropertyEvent : global::Starlite.Raven.RavenContinuousEvent {
        	// Properties
        	[UnityEngine.SerializeField] [UnityEngine.HideInInspector] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_IsSetProperty;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private global::Starlite.Raven.RavenAnimationPropertyComponentBase m_Property;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private global::UnityEngine.GameObject m_TriggerTarget;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenPropertyEvent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenPropertyEvent, RavenPropertyEvent, RavenPropertyEvent, Starlite::Raven::RavenContinuousEvent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenPropertyEvent, m_IsSetProperty, m_IsSetProperty, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenPropertyEvent, m_Property, m_Property, kObject, kUnknown, Starlite::Raven::RavenAnimationPropertyComponentBase::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenPropertyEvent, m_TriggerTarget, m_TriggerTarget, kObject, kUnknown, Starlite::SceneObject::TypeId, "")
SIMPLEMENT_CLASS_END(RavenPropertyEvent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenPropertyEvent, Starlite::Raven, RavenPropertyEvent)
#endif
