//{ "engine_register_objects":["Starlite__Raven__RavenCondition"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenCondition
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenCondition"), System.Serializable]
        public sealed partial class RavenCondition : global::Starlite.SerializableObject {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public Starlite.Raven.ERavenConditionMode m_ConditionMode = (Starlite.Raven.ERavenConditionMode) 0;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public float m_ValueFloat = 0.000000e+00f;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public int m_ParameterIndex = -1;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public int m_ValueIndex = -1;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public int m_ValueInt = 0;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenCondition
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenCondition, RavenCondition, RavenCondition, Starlite::SerializableObject, Starlite::SerializableObject, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenCondition, m_ConditionMode, m_ConditionMode, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenCondition, m_ValueFloat, m_ValueFloat, kFloat, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenCondition, m_ParameterIndex, m_ParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenCondition, m_ValueIndex, m_ValueIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenCondition, m_ValueInt, m_ValueInt, kInt, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenCondition)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenCondition, Starlite::Raven, RavenCondition)
#endif
