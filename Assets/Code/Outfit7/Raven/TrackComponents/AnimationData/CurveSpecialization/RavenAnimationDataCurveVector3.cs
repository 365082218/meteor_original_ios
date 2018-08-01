//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataCurveVector3"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataCurveVector3
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataCurveVector3")]
        public sealed partial class RavenAnimationDataCurveVector3 : global::Starlite.Raven.RavenAnimationDataCurve<UnityEngine.Vector3> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_UniformCurves = false;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataCurveVector3
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataCurveVector3, RavenAnimationDataCurveVector3, RavenAnimationDataCurveVector3, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveVector3, m_Curves, m_Curves, kArray, kObject, Starlite::AnimationCurve::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveVector3, m_EaseType, m_EaseType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveVector3, m_ValueType, m_ValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveVector3, m_RepeatCount, m_RepeatCount, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveVector3, m_Mirror, m_Mirror, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveVector3, m_UniformCurves, m_UniformCurves, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataCurveVector3)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataCurveVector3, Starlite::Raven, RavenAnimationDataCurveVector3)
#endif
