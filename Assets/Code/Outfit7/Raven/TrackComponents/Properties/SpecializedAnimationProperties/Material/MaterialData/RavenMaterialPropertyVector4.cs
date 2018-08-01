//{ "engine_register_objects":["Starlite__Raven__RavenMaterialPropertyVector4"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenMaterialPropertyVector4
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenMaterialPropertyVector4")]
        public sealed partial class RavenMaterialPropertyVector4 : global::Starlite.Raven.RavenMaterialPropertyBase<UnityEngine.Vector4> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool[] m_ApplyValues = new bool[0];
        }
        
    }
    
}
#pragma warning restore
#else
// RavenMaterialPropertyVector4
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenMaterialPropertyVector4, RavenMaterialPropertyVector4, RavenMaterialPropertyVector4, Starlite::Raven::RavenAnimationPropertyComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_PropertyType, m_PropertyType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_Offset, m_Offset, kVector4, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_Multiplier, m_Multiplier, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_ApplyMultiplier, m_ApplyMultiplier, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_ApplyOffset, m_ApplyOffset, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_ExecuteFunctionOnly, m_ExecuteFunctionOnly, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_UseSharedMaterial, m_UseSharedMaterial, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_TargetMaterialIndex, m_TargetMaterialIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_TargetMaterialProperty, m_TargetMaterialProperty, kString, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_RestoreMaterialOnEnd, m_RestoreMaterialOnEnd, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenMaterialPropertyVector4, m_ApplyValues, m_ApplyValues, kArray, kBool, -1, "")
SIMPLEMENT_CLASS_END(RavenMaterialPropertyVector4)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenMaterialPropertyVector4, Starlite::Raven, RavenMaterialPropertyVector4)
#endif
