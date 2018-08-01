#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataSet
        public abstract partial class RavenAnimationDataSet<T> : global::Starlite.Raven.RavenAnimationDataBase<T> {
        	// Properties
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected T m_StartValueStart;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected T m_StartValueEnd;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected Starlite.Raven.ERavenValueType m_StartValueType = (Starlite.Raven.ERavenValueType) 0;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_StartValueIsObjectLink;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::UnityEngine.Object m_StartValueObjectLink;
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Parameters")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected int m_StartParameterIndex = -1;
        }
        
    }
    
}
#pragma warning restore
#else
#endif
