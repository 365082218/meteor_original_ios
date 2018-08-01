//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataCurveInt"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataCurveInt
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataCurveInt")]
        public sealed partial class RavenAnimationDataCurveInt : global::Starlite.Raven.RavenAnimationDataCurve<int> {
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataCurveInt
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataCurveInt, RavenAnimationDataCurveInt, RavenAnimationDataCurveInt, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveInt, m_Curves, m_Curves, kArray, kObject, Starlite::AnimationCurve::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveInt, m_EaseType, m_EaseType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveInt, m_ValueType, m_ValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveInt, m_RepeatCount, m_RepeatCount, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveInt, m_Mirror, m_Mirror, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataCurveInt)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataCurveInt, Starlite::Raven, RavenAnimationDataCurveInt)
#endif
