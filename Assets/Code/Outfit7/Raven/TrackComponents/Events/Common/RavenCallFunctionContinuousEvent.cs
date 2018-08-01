//{ "engine_register_objects":["Starlite__Raven__RavenCallFunctionContinuousEvent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenCallFunctionContinuousEvent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenCallFunctionContinuousEvent")]
        public sealed partial class RavenCallFunctionContinuousEvent : global::Starlite.Raven.RavenContinuousEvent {
        	// Properties
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Settings")] [Raven.VisibleConditionAttribute("m_TrackIndex == 0 && m_SubTrackIndex == 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_IsBarrier;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private global::Starlite.Raven.RavenTriggerPropertyComponentBase m_Property;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenCallFunctionContinuousEvent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenCallFunctionContinuousEvent, RavenCallFunctionContinuousEvent, RavenCallFunctionContinuousEvent, Starlite::Raven::RavenContinuousEvent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenCallFunctionContinuousEvent, m_IsBarrier, m_IsBarrier, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenCallFunctionContinuousEvent, m_Property, m_Property, kObject, kUnknown, Starlite::Raven::RavenTriggerPropertyComponentBase::TypeId, "")
SIMPLEMENT_CLASS_END(RavenCallFunctionContinuousEvent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenCallFunctionContinuousEvent, Starlite::Raven, RavenCallFunctionContinuousEvent)
#endif
