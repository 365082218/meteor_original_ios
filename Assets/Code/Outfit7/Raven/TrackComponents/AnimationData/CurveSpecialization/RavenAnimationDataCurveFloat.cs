//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataCurveFloat"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataCurveFloat
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataCurveFloat")]
        public sealed partial class RavenAnimationDataCurveFloat : global::Starlite.Raven.RavenAnimationDataCurve<float> {
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataCurveFloat
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataCurveFloat, RavenAnimationDataCurveFloat, RavenAnimationDataCurveFloat, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveFloat, m_Curves, m_Curves, kArray, kObject, Starlite::AnimationCurve::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveFloat, m_EaseType, m_EaseType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveFloat, m_ValueType, m_ValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveFloat, m_RepeatCount, m_RepeatCount, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveFloat, m_Mirror, m_Mirror, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataCurveFloat)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataCurveFloat, Starlite::Raven, RavenAnimationDataCurveFloat)
#endif
