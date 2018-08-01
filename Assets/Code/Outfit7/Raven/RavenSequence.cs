//{ "engine_register_objects":["Starlite__Raven__RavenSequence"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenSequence
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenSequence")]
        public sealed partial class RavenSequence : global::Starlite.SceneObjectComponent {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_Loop = false;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_PlayOnAwake = false;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_PlayOnEnable = false;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private double m_Duration = 2.000000e+00;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private double m_TimeScale = 1.000000e+00;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private int m_Fps = 30;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private System.Collections.Generic.List<global::Starlite.Raven.RavenBookmarkEvent> m_SortedBookmarks = new System.Collections.Generic.List<global::Starlite.Raven.RavenBookmarkEvent>();
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private System.Collections.Generic.List<global::Starlite.Raven.RavenEvent> m_SortedEvents = new System.Collections.Generic.List<global::Starlite.Raven.RavenEvent>();
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private System.Collections.Generic.List<global::Starlite.Raven.RavenParameter> m_Parameters = new System.Collections.Generic.List<global::Starlite.Raven.RavenParameter>();
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private System.Collections.Generic.List<global::Starlite.Raven.RavenTrackGroup> m_SortedTrackGroups = new System.Collections.Generic.List<global::Starlite.Raven.RavenTrackGroup>();
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private global::Starlite.Raven.RavenBookmarkTrack m_BookmarkTrack;
        	[UnityEngine.SerializeField] [UnityEngine.HideInInspector] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private string m_CustomName;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenSequence
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenSequence, RavenSequence, RavenSequence, Starlite::SceneObjectComponent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_FUNCTION_STRING_OVERLOAD(RavenSequence, Play, "Play|System.Boolean,System.Boolean", void(RavenSequence::*)(bool, bool), "")
SIMPLEMENT_FUNCTION_STRING_OVERLOAD(RavenSequence, Play, "Play|System.String,System.Boolean", void(RavenSequence::*)(const char *, bool), "")
SIMPLEMENT_FUNCTION_STRING_OVERLOAD(RavenSequence, SetActorListParameter, "SetActorListParameter|System.Int32,System.Collections.Generic.List`1[UnityEngine.GameObject]", void(RavenSequence::*)(int, const ListBase<Ref<SceneObject, false>, Array<Ref<SceneObject, false>, Starlite::MemoryPool::kContainer> > &), "")
SIMPLEMENT_FUNCTION_STRING_OVERLOAD(RavenSequence, SetActorListParameter, "SetActorListParameter|System.String,System.Collections.Generic.List`1[UnityEngine.GameObject]", void(RavenSequence::*)(const StringBase<Array<char, Starlite::MemoryPool::kString> > &, const ListBase<Ref<SceneObject, false>, Array<Ref<SceneObject, false>, Starlite::MemoryPool::kContainer> > &), "")
SIMPLEMENT_FUNCTION_STRING_OVERLOAD(RavenSequence, SetBoolParameter, "SetBoolParameter|System.Int32,System.Boolean", void(RavenSequence::*)(int, bool), "")
SIMPLEMENT_FUNCTION_STRING_OVERLOAD(RavenSequence, SetBoolParameter, "SetBoolParameter|System.String,System.Boolean", void(RavenSequence::*)(const StringBase<Array<char, Starlite::MemoryPool::kString> > &, bool), "")
SIMPLEMENT_FUNCTION(RavenSequence, Stop, Stop, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_Loop, m_Loop, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_PlayOnAwake, m_PlayOnAwake, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_PlayOnEnable, m_PlayOnEnable, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_Duration, m_Duration, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_TimeScale, m_TimeScale, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_Fps, m_Fps, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_SortedBookmarks, m_SortedBookmarks, kList, kObject, Starlite::Raven::RavenBookmarkEvent::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_SortedEvents, m_SortedEvents, kList, kObject, Starlite::Raven::RavenEvent::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_Parameters, m_Parameters, kList, kObject, Starlite::Raven::RavenParameter::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_SortedTrackGroups, m_SortedTrackGroups, kList, kObject, Starlite::Raven::RavenTrackGroup::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_BookmarkTrack, m_BookmarkTrack, kObject, kUnknown, Starlite::Raven::RavenBookmarkTrack::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenSequence, m_CustomName, m_CustomName, kString, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenSequence)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenSequence, Starlite::Raven, RavenSequence)
#endif
