//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataTweenDouble"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataTweenDouble
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataTweenDouble")]
        public sealed partial class RavenAnimationDataTweenDouble : global::Starlite.Raven.RavenAnimationDataTween<double> {
        	// Properties
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Remap")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_Remap = false;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_Remap")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private double m_RemapStart = 0.000000e+00;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_Remap")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private double m_RemapEnd = 1.000000e+00;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataTweenDouble
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataTweenDouble, RavenAnimationDataTweenDouble, RavenAnimationDataTweenDouble, Starlite::Raven::RavenAnimationDataComponentBase, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_StartValueStart, m_StartValueStart, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_StartValueEnd, m_StartValueEnd, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_StartValueType, m_StartValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_StartValueIsObjectLink, m_StartValueIsObjectLink, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_StartValueObjectLink, m_StartValueObjectLink, kObject, kUnknown, Starlite::Object::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_EndValueStart, m_EndValueStart, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_EndValueEnd, m_EndValueEnd, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_EndValueType, m_EndValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_EndValueIsObjectLink, m_EndValueIsObjectLink, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_EndValueObjectLink, m_EndValueObjectLink, kObject, kUnknown, Starlite::Object::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_StartParameterIndex, m_StartParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_EndParameterIndex, m_EndParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_UseCustomEaseCurve, m_UseCustomEaseCurve, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_EaseType, m_EaseType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_EaseCurve, m_EaseCurve, kObject, kUnknown, Starlite::AnimationCurve::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_EaseAmplitude, m_EaseAmplitude, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_EasePeriod, m_EasePeriod, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_RepeatCount, m_RepeatCount, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_Mirror, m_Mirror, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_Remap, m_Remap, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_RemapStart, m_RemapStart, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenDouble, m_RemapEnd, m_RemapEnd, kDouble, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataTweenDouble)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataTweenDouble, Starlite::Raven, RavenAnimationDataTweenDouble)
#endif
