//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataCurveDouble"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataCurveDouble
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataCurveDouble")]
        public sealed partial class RavenAnimationDataCurveDouble : global::Starlite.Raven.RavenAnimationDataCurve<double> {
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataCurveDouble
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataCurveDouble, RavenAnimationDataCurveDouble, RavenAnimationDataCurveDouble, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveDouble, m_Curves, m_Curves, kArray, kObject, Starlite::AnimationCurve::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveDouble, m_EaseType, m_EaseType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveDouble, m_ValueType, m_ValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveDouble, m_RepeatCount, m_RepeatCount, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveDouble, m_Mirror, m_Mirror, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataCurveDouble)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataCurveDouble, Starlite::Raven, RavenAnimationDataCurveDouble)
#endif
