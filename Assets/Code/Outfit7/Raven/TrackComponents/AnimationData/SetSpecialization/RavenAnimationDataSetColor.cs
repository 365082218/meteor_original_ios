//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataSetColor"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataSetColor
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataSetColor")]
        public sealed partial class RavenAnimationDataSetColor : global::Starlite.Raven.RavenAnimationDataSet<UnityEngine.Color> {
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataSetColor
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataSetColor, RavenAnimationDataSetColor, RavenAnimationDataSetColor, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetColor, m_StartValueStart, m_StartValueStart, kColor, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetColor, m_StartValueEnd, m_StartValueEnd, kColor, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetColor, m_StartValueType, m_StartValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetColor, m_StartValueIsObjectLink, m_StartValueIsObjectLink, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetColor, m_StartValueObjectLink, m_StartValueObjectLink, kObject, kUnknown, Starlite::Object::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataSetColor, m_StartParameterIndex, m_StartParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataSetColor)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataSetColor, Starlite::Raven, RavenAnimationDataSetColor)
#endif
