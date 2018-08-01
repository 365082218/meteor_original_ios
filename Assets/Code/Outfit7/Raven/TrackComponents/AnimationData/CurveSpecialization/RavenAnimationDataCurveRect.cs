//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataCurveRect"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataCurveRect
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataCurveRect")]
        public sealed partial class RavenAnimationDataCurveRect : global::Starlite.Raven.RavenAnimationDataCurve<UnityEngine.Rect> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_UniformCurves = false;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataCurveRect
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataCurveRect, RavenAnimationDataCurveRect, RavenAnimationDataCurveRect, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveRect, m_Curves, m_Curves, kArray, kObject, Starlite::AnimationCurve::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveRect, m_EaseType, m_EaseType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveRect, m_ValueType, m_ValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveRect, m_RepeatCount, m_RepeatCount, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveRect, m_Mirror, m_Mirror, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveRect, m_UniformCurves, m_UniformCurves, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataCurveRect)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataCurveRect, Starlite::Raven, RavenAnimationDataCurveRect)
#endif
