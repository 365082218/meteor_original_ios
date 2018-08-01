//{ "engine_register_objects":["Starlite__Raven__RavenBookmarkEvent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenBookmarkEvent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenBookmarkEvent")]
        public sealed partial class RavenBookmarkEvent : global::Starlite.Raven.RavenEvent {
            // ERavenBookmarkType
            public enum ERavenBookmarkType { 
            	Ignore = 0, 
            	Pause = 1, 
            	Stop = 2, 
            	Loop = 3, 
            }
            
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private Starlite.Raven.RavenBookmarkEvent.ERavenBookmarkType m_BookmarkType = (Starlite.Raven.RavenBookmarkEvent.ERavenBookmarkType) 0;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private string m_BookmarkName = "Le Bookmark";
        }
        
    }
    
}
#pragma warning restore
#else
// RavenBookmarkEvent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenBookmarkEvent, RavenBookmarkEvent, RavenBookmarkEvent, Starlite::Raven::RavenEvent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenBookmarkEvent, m_BookmarkType, m_BookmarkType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenBookmarkEvent, m_BookmarkName, m_BookmarkName, kString, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenBookmarkEvent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenBookmarkEvent, Starlite::Raven, RavenBookmarkEvent)
#endif
