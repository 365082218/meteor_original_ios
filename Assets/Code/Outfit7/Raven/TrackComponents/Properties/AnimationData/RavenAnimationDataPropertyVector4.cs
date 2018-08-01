//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataPropertyVector4"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataPropertyVector4
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataPropertyVector4")]
        public sealed partial class RavenAnimationDataPropertyVector4 : global::Starlite.Raven.RavenAnimationDataPropertyBase<UnityEngine.Vector4> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool[] m_ApplyValues = new bool[0];
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataPropertyVector4
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataPropertyVector4, RavenAnimationDataPropertyVector4, RavenAnimationDataPropertyVector4, Starlite::Raven::RavenAnimationPropertyComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector4, m_PropertyType, m_PropertyType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector4, m_Offset, m_Offset, kVector4, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector4, m_Multiplier, m_Multiplier, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector4, m_ApplyMultiplier, m_ApplyMultiplier, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector4, m_ApplyOffset, m_ApplyOffset, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector4, m_ExecuteFunctionOnly, m_ExecuteFunctionOnly, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector4, m_ApplyValues, m_ApplyValues, kArray, kBool, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataPropertyVector4)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataPropertyVector4, Starlite::Raven, RavenAnimationDataPropertyVector4)
#endif
