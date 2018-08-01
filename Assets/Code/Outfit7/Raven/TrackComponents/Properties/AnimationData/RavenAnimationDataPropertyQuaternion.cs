//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataPropertyQuaternion"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataPropertyQuaternion
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataPropertyQuaternion")]
        public sealed partial class RavenAnimationDataPropertyQuaternion : global::Starlite.Raven.RavenAnimationDataPropertyBase<UnityEngine.Quaternion> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool[] m_ApplyValues = new bool[0];
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataPropertyQuaternion
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataPropertyQuaternion, RavenAnimationDataPropertyQuaternion, RavenAnimationDataPropertyQuaternion, Starlite::Raven::RavenAnimationPropertyComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyQuaternion, m_PropertyType, m_PropertyType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyQuaternion, m_Offset, m_Offset, kQuaternion, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyQuaternion, m_Multiplier, m_Multiplier, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyQuaternion, m_ApplyMultiplier, m_ApplyMultiplier, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyQuaternion, m_ApplyOffset, m_ApplyOffset, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyQuaternion, m_ExecuteFunctionOnly, m_ExecuteFunctionOnly, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyQuaternion, m_ApplyValues, m_ApplyValues, kArray, kBool, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataPropertyQuaternion)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataPropertyQuaternion, Starlite::Raven, RavenAnimationDataPropertyQuaternion)
#endif
