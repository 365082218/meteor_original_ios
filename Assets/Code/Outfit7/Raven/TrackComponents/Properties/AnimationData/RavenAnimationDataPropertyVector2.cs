//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataPropertyVector2"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataPropertyVector2
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataPropertyVector2")]
        public sealed partial class RavenAnimationDataPropertyVector2 : global::Starlite.Raven.RavenAnimationDataPropertyBase<UnityEngine.Vector2> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool[] m_ApplyValues = new bool[0];
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataPropertyVector2
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataPropertyVector2, RavenAnimationDataPropertyVector2, RavenAnimationDataPropertyVector2, Starlite::Raven::RavenAnimationPropertyComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector2, m_PropertyType, m_PropertyType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector2, m_Offset, m_Offset, kVector2, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector2, m_Multiplier, m_Multiplier, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector2, m_ApplyMultiplier, m_ApplyMultiplier, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector2, m_ApplyOffset, m_ApplyOffset, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector2, m_ExecuteFunctionOnly, m_ExecuteFunctionOnly, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyVector2, m_ApplyValues, m_ApplyValues, kArray, kBool, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataPropertyVector2)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataPropertyVector2, Starlite::Raven, RavenAnimationDataPropertyVector2)
#endif
