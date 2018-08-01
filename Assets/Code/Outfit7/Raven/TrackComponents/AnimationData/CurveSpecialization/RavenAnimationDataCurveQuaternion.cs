//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataCurveQuaternion"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataCurveQuaternion
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataCurveQuaternion")]
        public sealed partial class RavenAnimationDataCurveQuaternion : global::Starlite.Raven.RavenAnimationDataCurve<UnityEngine.Quaternion> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_UniformCurves = false;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataCurveQuaternion
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataCurveQuaternion, RavenAnimationDataCurveQuaternion, RavenAnimationDataCurveQuaternion, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveQuaternion, m_Curves, m_Curves, kArray, kObject, Starlite::AnimationCurve::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveQuaternion, m_EaseType, m_EaseType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveQuaternion, m_ValueType, m_ValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveQuaternion, m_RepeatCount, m_RepeatCount, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveQuaternion, m_Mirror, m_Mirror, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataCurveQuaternion, m_UniformCurves, m_UniformCurves, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataCurveQuaternion)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataCurveQuaternion, Starlite::Raven, RavenAnimationDataCurveQuaternion)
#endif
