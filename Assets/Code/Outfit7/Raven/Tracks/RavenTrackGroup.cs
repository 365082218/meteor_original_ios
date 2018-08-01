//{ "engine_register_objects":["Starlite__Raven__RavenTrackGroup"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenTrackGroup
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenTrackGroup")]
        public sealed partial class RavenTrackGroup : global::Starlite.Raven.RavenComponent {
            // ERavenTrackGroupMode
            public enum ERavenTrackGroupMode { 
            	Extended = 0, 
            	Optimized = 1, 
            	Collapsed = 2, 
            }
            
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public int m_OverrideTargetsParameterIndex = -1;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public Starlite.Raven.RavenTrackGroup.ERavenTrackGroupMode m_TrackGroupMode = (Starlite.Raven.RavenTrackGroup.ERavenTrackGroupMode) 0;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public global::Starlite.Raven.RavenAudioTrack m_AudioTrack;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public global::Starlite.Raven.RavenPropertyTrack m_PropertyTrack;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public global::UnityEngine.GameObject m_Target;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private int m_TrackIndex;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenTrackGroup
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenTrackGroup, RavenTrackGroup, RavenTrackGroup, Starlite::Raven::RavenComponent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTrackGroup, m_OverrideTargetsParameterIndex, m_OverrideTargetsParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTrackGroup, m_TrackGroupMode, m_TrackGroupMode, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTrackGroup, m_AudioTrack, m_AudioTrack, kObject, kUnknown, Starlite::Raven::RavenAudioTrack::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTrackGroup, m_PropertyTrack, m_PropertyTrack, kObject, kUnknown, Starlite::Raven::RavenPropertyTrack::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTrackGroup, m_Target, m_Target, kObject, kUnknown, Starlite::SceneObject::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTrackGroup, m_TrackIndex, m_TrackIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenTrackGroup)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenTrackGroup, Starlite::Raven, RavenTrackGroup)
#endif
