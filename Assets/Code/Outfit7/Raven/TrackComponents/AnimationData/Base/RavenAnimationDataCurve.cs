#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataCurve
        public abstract partial class RavenAnimationDataCurve<T> : global::Starlite.Raven.RavenAnimationDataBase<T> {
        	// Properties
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Settings")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::UnityEngine.AnimationCurve[] m_Curves = new global::UnityEngine.AnimationCurve[0];
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected Starlite.Raven.ERavenEaseType m_EaseType = (Starlite.Raven.ERavenEaseType) 0;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected Starlite.Raven.ERavenValueType m_ValueType = (Starlite.Raven.ERavenValueType) 0;
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Repeat")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected int m_RepeatCount = 1;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_RepeatCount > 1")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_Mirror = false;
        }
        
    }
    
}
#pragma warning restore
#else
#endif
