//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataSetFloat"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataSetFloat
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataSetFloat")]
        public sealed partial class RavenAnimationDataSetFloat : global::Starlite.Raven.RavenAnimationDataSet<float> {
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataSetFloat
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataSetFloat, RavenAnimationDataSetFloat, RavenAnimationDataSetFloat, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetFloat, m_StartValueStart, m_StartValueStart, kFloat, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetFloat, m_StartValueEnd, m_StartValueEnd, kFloat, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetFloat, m_StartValueType, m_StartValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetFloat, m_StartValueIsObjectLink, m_StartValueIsObjectLink, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetFloat, m_StartValueObjectLink, m_StartValueObjectLink, kObject, kUnknown, Starlite::Object::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetFloat, m_StartParameterIndex, m_StartParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataSetFloat)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataSetFloat, Starlite::Raven, RavenAnimationDataSetFloat)
#endif
