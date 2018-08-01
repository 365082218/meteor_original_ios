//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataTweenFloat"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataTweenFloat
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataTweenFloat")]
        public sealed partial class RavenAnimationDataTweenFloat : global::Starlite.Raven.RavenAnimationDataTween<float> {
        	// Properties
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Remap")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_Remap = false;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_Remap")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private float m_RemapStart = 0.000000e+00f;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_Remap")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private float m_RemapEnd = 1.000000e+00f;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataTweenFloat
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataTweenFloat, RavenAnimationDataTweenFloat, RavenAnimationDataTweenFloat, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_StartValueStart, m_StartValueStart, kFloat, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_StartValueEnd, m_StartValueEnd, kFloat, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_StartValueType, m_StartValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_StartValueIsObjectLink, m_StartValueIsObjectLink, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_StartValueObjectLink, m_StartValueObjectLink, kObject, kUnknown, Starlite::Object::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_EndValueStart, m_EndValueStart, kFloat, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_EndValueEnd, m_EndValueEnd, kFloat, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_EndValueType, m_EndValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_EndValueIsObjectLink, m_EndValueIsObjectLink, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_EndValueObjectLink, m_EndValueObjectLink, kObject, kUnknown, Starlite::Object::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_StartParameterIndex, m_StartParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_EndParameterIndex, m_EndParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_UseCustomEaseCurve, m_UseCustomEaseCurve, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_EaseType, m_EaseType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_EaseCurve, m_EaseCurve, kObject, kUnknown, Starlite::AnimationCurve::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_EaseAmplitude, m_EaseAmplitude, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_EasePeriod, m_EasePeriod, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_RepeatCount, m_RepeatCount, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_Mirror, m_Mirror, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_Remap, m_Remap, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_RemapStart, m_RemapStart, kFloat, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenFloat, m_RemapEnd, m_RemapEnd, kFloat, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataTweenFloat)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataTweenFloat, Starlite::Raven, RavenAnimationDataTweenFloat)
#endif
