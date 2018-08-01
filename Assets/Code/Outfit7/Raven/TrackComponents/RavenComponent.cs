//{ "engine_register_objects":["Starlite__Raven__RavenComponent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenComponent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenComponent")]
        public abstract partial class RavenComponent : global::Starlite.SceneObjectComponent {
        }
        
    }
    
}
#pragma warning restore
#else
// RavenComponent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_ABSTRACT_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenComponent, RavenComponent, RavenComponent, Starlite::SceneObjectComponent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_CLASS_END(RavenComponent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenComponent, Starlite::Raven, RavenComponent)
#endif
