#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataTween
        public abstract partial class RavenAnimationDataTween<T> : global::Starlite.Raven.RavenAnimationDataBase<T> {
        	// Properties
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected T m_StartValueStart;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected T m_StartValueEnd;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected Starlite.Raven.ERavenValueType m_StartValueType = (Starlite.Raven.ERavenValueType) 0;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_StartValueIsObjectLink = false;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::UnityEngine.Object m_StartValueObjectLink;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_EndParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected T m_EndValueStart;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_EndParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected T m_EndValueEnd;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_EndParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected Starlite.Raven.ERavenValueType m_EndValueType = (Starlite.Raven.ERavenValueType) 0;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_EndParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_EndValueIsObjectLink = false;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_EndParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::UnityEngine.Object m_EndValueObjectLink;
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Parameters")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected int m_StartParameterIndex = -1;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected int m_EndParameterIndex = -1;
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Easing")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_UseCustomEaseCurve = false;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("!m_UseCustomEaseCurve")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected Starlite.Raven.ERavenEaseType m_EaseType;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_UseCustomEaseCurve")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::UnityEngine.AnimationCurve m_EaseCurve = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0f, 0f), new UnityEngine.Keyframe(1f, 1f));
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("!m_UseCustomEaseCurve")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected double m_EaseAmplitude = 1.000000e+00;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("!m_UseCustomEaseCurve")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected double m_EasePeriod = 1.000000e+00;
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Repeat")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected int m_RepeatCount = 1;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_RepeatCount > 1")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_Mirror = false;
        }
        
    }
    
}
#pragma warning restore
#else
#endif
