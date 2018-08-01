//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataSetMaterial"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataSetMaterial
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataSetMaterial")]
        public sealed partial class RavenAnimationDataSetMaterial : global::Starlite.Raven.RavenAnimationDataSet<global::UnityEngine.Material> {
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataSetMaterial
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataSetMaterial, RavenAnimationDataSetMaterial, RavenAnimationDataSetMaterial, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetMaterial, m_StartValueStart, m_StartValueStart, kObject, kUnknown, Starlite::Material::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetMaterial, m_StartValueEnd, m_StartValueEnd, kObject, kUnknown, Starlite::Material::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetMaterial, m_StartValueType, m_StartValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetMaterial, m_StartValueIsObjectLink, m_StartValueIsObjectLink, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetMaterial, m_StartValueObjectLink, m_StartValueObjectLink, kObject, kUnknown, Starlite::Object::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetMaterial, m_StartParameterIndex, m_StartParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataSetMaterial)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataSetMaterial, Starlite::Raven, RavenAnimationDataSetMaterial)
#endif
