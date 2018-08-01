//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataCurveColor"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataCurveColor
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataCurveColor")]
        public sealed partial class RavenAnimationDataCurveColor : global::Starlite.Raven.RavenAnimationDataCurve<UnityEngine.Color> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_UniformCurves = false;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataCurveColor
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataCurveColor, RavenAnimationDataCurveColor, RavenAnimationDataCurveColor, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveColor, m_Curves, m_Curves, kArray, kObject, Starlite::AnimationCurve::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveColor, m_EaseType, m_EaseType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveColor, m_ValueType, m_ValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveColor, m_RepeatCount, m_RepeatCount, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveColor, m_Mirror, m_Mirror, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveColor, m_UniformCurves, m_UniformCurves, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataCurveColor)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataCurveColor, Starlite::Raven, RavenAnimationDataCurveColor)
#endif
