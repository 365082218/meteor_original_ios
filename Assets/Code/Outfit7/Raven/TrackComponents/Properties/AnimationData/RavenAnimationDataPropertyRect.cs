//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataPropertyRect"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataPropertyRect
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataPropertyRect")]
        public sealed partial class RavenAnimationDataPropertyRect : global::Starlite.Raven.RavenAnimationDataPropertyBase<UnityEngine.Rect> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool[] m_ApplyValues = new bool[0];
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataPropertyRect
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataPropertyRect, RavenAnimationDataPropertyRect, RavenAnimationDataPropertyRect, Starlite::Raven::RavenAnimationPropertyComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyRect, m_PropertyType, m_PropertyType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyRect, m_Offset, m_Offset, kRectangle, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyRect, m_Multiplier, m_Multiplier, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyRect, m_ApplyMultiplier, m_ApplyMultiplier, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyRect, m_ApplyOffset, m_ApplyOffset, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyRect, m_ExecuteFunctionOnly, m_ExecuteFunctionOnly, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataPropertyRect, m_ApplyValues, m_ApplyValues, kArray, kBool, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataPropertyRect)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataPropertyRect, Starlite::Raven, RavenAnimationDataPropertyRect)
#endif
