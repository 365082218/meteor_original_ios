#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationPropertyBase
        public abstract partial class RavenAnimationPropertyBase<T> : global::Starlite.Raven.RavenAnimationPropertyComponentBase {
        	// Properties
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Settings")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected Starlite.Raven.ERavenAnimationPropertyType m_PropertyType = (Starlite.Raven.ERavenAnimationPropertyType) 0;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_ApplyOffset")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected T m_Offset;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_ApplyMultiplier")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected double m_Multiplier = 1.000000e+00;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_ApplyMultiplier = false;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_ApplyOffset = false;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_ExecuteFunctionOnly = false;
        }
        
    }
    
}
#pragma warning restore
#else
#endif
