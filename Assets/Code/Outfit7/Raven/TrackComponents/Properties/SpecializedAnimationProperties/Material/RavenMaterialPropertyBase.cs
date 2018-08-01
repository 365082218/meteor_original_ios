#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenMaterialPropertyBase
        public abstract partial class RavenMaterialPropertyBase<T> : global::Starlite.Raven.RavenAnimationPropertyBase<T> {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_UseSharedMaterial = false;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected int m_TargetMaterialIndex = 0;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected string m_TargetMaterialProperty = "";
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("!m_UseSharedMaterial")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected bool m_RestoreMaterialOnEnd = false;
        }
        
    }
    
}
#pragma warning restore
#else
#endif
