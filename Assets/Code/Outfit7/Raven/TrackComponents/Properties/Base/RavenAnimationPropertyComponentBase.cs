//{ "engine_register_objects":["Starlite__Raven__RavenAnimationPropertyComponentBase"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationPropertyComponentBase
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationPropertyComponentBase")]
        public abstract partial class RavenAnimationPropertyComponentBase : global::Starlite.Raven.RavenPropertyComponent {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::Starlite.Raven.RavenTriggerPropertyComponentBase m_TriggerProperty;
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Internal")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] protected global::Starlite.Raven.RavenAnimationDataComponentBase m_AnimationData;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private string m_ComponentType;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private string m_MemberName;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private ulong m_TargetHash;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationPropertyComponentBase
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_ABSTRACT_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationPropertyComponentBase, RavenAnimationPropertyComponentBase, RavenAnimationPropertyComponentBase, Starlite::Raven::RavenPropertyComponent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationPropertyComponentBase, m_TriggerProperty, m_TriggerProperty, kObject, kUnknown, Starlite::Raven::RavenTriggerPropertyComponentBase::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationPropertyComponentBase, m_AnimationData, m_AnimationData, kObject, kUnknown, Starlite::Raven::RavenAnimationDataComponentBase::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationPropertyComponentBase, m_ComponentType, m_ComponentType, kString, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationPropertyComponentBase, m_MemberName, m_MemberName, kString, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationPropertyComponentBase, m_TargetHash, m_TargetHash, kInt64, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationPropertyComponentBase)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationPropertyComponentBase, Starlite::Raven, RavenAnimationPropertyComponentBase)
#endif
