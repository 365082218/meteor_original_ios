//{ "engine_register_objects":["Starlite__Raven__RavenGoToEvent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenGoToEvent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenGoToEvent")]
        public sealed partial class RavenGoToEvent : global::Starlite.Raven.RavenTriggerEvent {
        	// Properties
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Settings")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private int m_FrameToJumpTo = 0;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenGoToEvent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenGoToEvent, RavenGoToEvent, RavenGoToEvent, Starlite::Raven::RavenTriggerEvent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenGoToEvent, m_FrameToJumpTo, m_FrameToJumpTo, kInt, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenGoToEvent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenGoToEvent, Starlite::Raven, RavenGoToEvent)
#endif
