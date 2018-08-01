//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataPropertyColor"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataPropertyColor
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataPropertyColor")]
        public sealed partial class RavenAnimationDataPropertyColor : global::Starlite.Raven.RavenAnimationDataPropertyBase<UnityEngine.Color> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool[] m_ApplyValues = new bool[0];
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataPropertyColor
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataPropertyColor, RavenAnimationDataPropertyColor, RavenAnimationDataPropertyColor, Starlite::Raven::RavenAnimationPropertyComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyColor, m_PropertyType, m_PropertyType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyColor, m_Offset, m_Offset, kColor, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyColor, m_Multiplier, m_Multiplier, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyColor, m_ApplyMultiplier, m_ApplyMultiplier, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyColor, m_ApplyOffset, m_ApplyOffset, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyColor, m_ExecuteFunctionOnly, m_ExecuteFunctionOnly, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyColor, m_ApplyValues, m_ApplyValues, kArray, kBool, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataPropertyColor)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataPropertyColor, Starlite::Raven, RavenAnimationDataPropertyColor)
#endif
