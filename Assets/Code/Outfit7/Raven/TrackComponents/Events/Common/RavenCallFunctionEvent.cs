//{ "engine_register_objects":["Starlite__Raven__RavenCallFunctionEvent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenCallFunctionEvent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenCallFunctionEvent")]
        public sealed partial class RavenCallFunctionEvent : global::Starlite.Raven.RavenTriggerEvent {
        	// Properties
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_TrackIndex == 0 && m_SubTrackIndex == 0")] [UnityEngine.HeaderAttribute("Settings")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_IsBarrier;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private global::Starlite.Raven.RavenTriggerPropertyComponentBase m_Property;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenCallFunctionEvent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenCallFunctionEvent, RavenCallFunctionEvent, RavenCallFunctionEvent, Starlite::Raven::RavenTriggerEvent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenCallFunctionEvent, m_IsBarrier, m_IsBarrier, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenCallFunctionEvent, m_Property, m_Property, kObject, kUnknown, Starlite::Raven::RavenTriggerPropertyComponentBase::TypeId, "")
SIMPLEMENT_CLASS_END(RavenCallFunctionEvent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenCallFunctionEvent, Starlite::Raven, RavenCallFunctionEvent)
#endif
