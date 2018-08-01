//{ "engine_register_objects":["Starlite__Raven__RavenMaterialPropertyColor"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenMaterialPropertyColor
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenMaterialPropertyColor")]
        public sealed partial class RavenMaterialPropertyColor : global::Starlite.Raven.RavenMaterialPropertyBase<UnityEngine.Color> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool[] m_ApplyValues = new bool[0];
        }
        
    }
    
}
#pragma warning restore
#else
// RavenMaterialPropertyColor
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenMaterialPropertyColor, RavenMaterialPropertyColor, RavenMaterialPropertyColor, Starlite::Raven::RavenAnimationPropertyComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_PropertyType, m_PropertyType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_Offset, m_Offset, kColor, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_Multiplier, m_Multiplier, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_ApplyMultiplier, m_ApplyMultiplier, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_ApplyOffset, m_ApplyOffset, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_ExecuteFunctionOnly, m_ExecuteFunctionOnly, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_UseSharedMaterial, m_UseSharedMaterial, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_TargetMaterialIndex, m_TargetMaterialIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_TargetMaterialProperty, m_TargetMaterialProperty, kString, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_RestoreMaterialOnEnd, m_RestoreMaterialOnEnd, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyColor, m_ApplyValues, m_ApplyValues, kArray, kBool, -1, "")
SIMPLEMENT_CLASS_END(RavenMaterialPropertyColor)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenMaterialPropertyColor, Starlite::Raven, RavenMaterialPropertyColor)
#endif
