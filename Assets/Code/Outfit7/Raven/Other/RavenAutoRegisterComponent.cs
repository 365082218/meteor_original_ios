//{ "engine_register_objects":["Starlite__Raven__RavenAutoRegisterComponent"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAutoRegisterComponent
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAutoRegisterComponent")]
        public sealed partial class RavenAutoRegisterComponent : global::Starlite.SceneObjectComponent {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public string m_Parameter;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAutoRegisterComponent
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAutoRegisterComponent, RavenAutoRegisterComponent, RavenAutoRegisterComponent, Starlite::SceneObjectComponent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAutoRegisterComponent, m_Parameter, m_Parameter, kString, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAutoRegisterComponent)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAutoRegisterComponent, Starlite::Raven, RavenAutoRegisterComponent)
#endif
