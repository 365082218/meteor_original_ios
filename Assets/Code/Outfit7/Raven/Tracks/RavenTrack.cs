//{ "engine_register_objects":["Starlite__Raven__RavenTrack"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenTrack
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenTrack")]
        public abstract partial class RavenTrack : global::Starlite.Raven.RavenComponent {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private System.Collections.Generic.List<global::Starlite.Raven.RavenEvent> m_Events = new System.Collections.Generic.List<global::Starlite.Raven.RavenEvent>();
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_IsEnabled = true;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenTrack
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_ABSTRACT_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenTrack, RavenTrack, RavenTrack, Starlite::Raven::RavenComponent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTrack, m_Events, m_Events, kList, kObject, Starlite::Raven::RavenEvent::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTrack, m_IsEnabled, m_IsEnabled, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenTrack)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenTrack, Starlite::Raven, RavenTrack)
#endif
