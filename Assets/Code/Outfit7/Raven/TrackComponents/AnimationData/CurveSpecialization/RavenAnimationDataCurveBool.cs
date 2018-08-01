//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataCurveBool"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataCurveBool
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataCurveBool")]
        public sealed partial class RavenAnimationDataCurveBool : global::Starlite.Raven.RavenAnimationDataCurve<bool> {
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataCurveBool
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataCurveBool, RavenAnimationDataCurveBool, RavenAnimationDataCurveBool, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveBool, m_Curves, m_Curves, kArray, kObject, Starlite::AnimationCurve::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveBool, m_EaseType, m_EaseType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveBool, m_ValueType, m_ValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveBool, m_RepeatCount, m_RepeatCount, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveBool, m_Mirror, m_Mirror, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataCurveBool)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataCurveBool, Starlite::Raven, RavenAnimationDataCurveBool)
#endif
