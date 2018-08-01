//{ "engine_register_objects":["Starlite__Raven__RavenEvent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        namespace Internal {
        
            // RavenEventFlags
            [System.Flags]
            public enum RavenEventFlags { 
            	Enabled = 1, 
            	TrackEnabled = 2, 
            	Default = 3, 
            }
            
        }
        
        // RavenEvent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenEvent")]
        public abstract partial class RavenEvent : global::Starlite.Raven.RavenComponent {
        	// Properties
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Internal")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected int m_StartFrame = 0;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected int m_SubTrackIndex = 0;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected int m_TrackIndex = 0;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected System.Collections.Generic.List<global::Starlite.Raven.RavenCondition> m_Conditions = new System.Collections.Generic.List<global::Starlite.Raven.RavenCondition>();
        	[UnityEngine.SerializeField] [UnityEngine.HideInInspector] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private Starlite.Raven.Internal.RavenEventFlags m_Flags = (Starlite.Raven.Internal.RavenEventFlags) 3;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenEvent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_ABSTRACT_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenEvent, RavenEvent, RavenEvent, Starlite::Raven::RavenComponent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenEvent, m_StartFrame, m_StartFrame, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenEvent, m_SubTrackIndex, m_SubTrackIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenEvent, m_TrackIndex, m_TrackIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenEvent, m_Conditions, m_Conditions, kList, kObject, Starlite::Raven::RavenCondition::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenEvent, m_Flags, m_Flags, kInt, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenEvent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenEvent, Starlite::Raven, RavenEvent)
#endif
