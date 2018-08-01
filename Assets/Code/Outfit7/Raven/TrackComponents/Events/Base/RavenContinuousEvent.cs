//{ "engine_register_objects":["Starlite__Raven__RavenContinuousEvent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenContinuousEvent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenContinuousEvent")]
        public abstract partial class RavenContinuousEvent : global::Starlite.Raven.RavenEvent {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected int m_LastFrame = 30;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::UnityEngine.GameObject m_Target;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_Interpolate = true;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenContinuousEvent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_ABSTRACT_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenContinuousEvent, RavenContinuousEvent, RavenContinuousEvent, Starlite::Raven::RavenEvent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenContinuousEvent, m_LastFrame, m_LastFrame, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenContinuousEvent, m_Target, m_Target, kObject, kUnknown, Starlite::SceneObject::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenContinuousEvent, m_Interpolate, m_Interpolate, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenContinuousEvent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenContinuousEvent, Starlite::Raven, RavenContinuousEvent)
#endif
