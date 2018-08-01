#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // ERavenEventTriggerPointType
        public enum ERavenEventTriggerPointType { 
        	Start = 1000, 
        	Process = 2000, 
        	Pause = 3000, 
        	Bookmark = 4000, 
        	Barrier = 5000, 
        	End = 6000, 
        }
        
    }
    
}
#pragma warning restore
#else
#endif
