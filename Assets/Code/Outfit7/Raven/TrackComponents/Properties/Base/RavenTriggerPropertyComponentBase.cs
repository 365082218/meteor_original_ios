//{ "engine_register_objects":["Starlite__Raven__RavenTriggerPropertyComponentBase"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenTriggerPropertyComponentBase
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenTriggerPropertyComponentBase")]
        public abstract partial class RavenTriggerPropertyComponentBase : global::Starlite.Raven.RavenPropertyComponent {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private int m_ParameterIndex = -1;
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Internal")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private string m_ComponentType;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private string m_FunctionName;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private ulong m_TargetHash;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenTriggerPropertyComponentBase
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_ABSTRACT_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenTriggerPropertyComponentBase, RavenTriggerPropertyComponentBase, RavenTriggerPropertyComponentBase, Starlite::Raven::RavenPropertyComponent, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTriggerPropertyComponentBase, m_ParameterIndex, m_ParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTriggerPropertyComponentBase, m_ComponentType, m_ComponentType, kString, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTriggerPropertyComponentBase, m_FunctionName, m_FunctionName, kString, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenTriggerPropertyComponentBase, m_TargetHash, m_TargetHash, kInt64, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenTriggerPropertyComponentBase)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenTriggerPropertyComponentBase, Starlite::Raven, RavenTriggerPropertyComponentBase)
#endif
