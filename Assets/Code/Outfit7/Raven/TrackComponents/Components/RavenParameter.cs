//{ "engine_register_objects":["Starlite__Raven__RavenParameter"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenParameter
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenParameter"), System.Serializable]
        public sealed partial class RavenParameter : global::Starlite.SerializableObject {
        	// Properties
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public Starlite.Raven.ERavenParameterType m_ParameterType = (Starlite.Raven.ERavenParameterType) 0;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public float m_ValueFloat = 0.000000e+00f;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public double m_ValueDouble = 0.000000e+00;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public int m_ValueInt = 0;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public System.Collections.Generic.List<global::UnityEngine.GameObject> m_ValueGameObjectList = new System.Collections.Generic.List<global::UnityEngine.GameObject>();
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public global::UnityEngine.Object m_ValueObject;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public string m_Name;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] public UnityEngine.Vector4 m_ValueVector;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenParameter
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenParameter, RavenParameter, RavenParameter, Starlite::SerializableObject, Starlite::SerializableObject, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenParameter, m_ParameterType, m_ParameterType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenParameter, m_ValueFloat, m_ValueFloat, kFloat, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenParameter, m_ValueDouble, m_ValueDouble, kDouble, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenParameter, m_ValueInt, m_ValueInt, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenParameter, m_ValueGameObjectList, m_ValueGameObjectList, kList, kObject, Starlite::SceneObject::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenParameter, m_ValueObject, m_ValueObject, kObject, kUnknown, Starlite::Object::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenParameter, m_Name, m_Name, kString, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenParameter, m_ValueVector, m_ValueVector, kVector4, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenParameter)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenParameter, Starlite::Raven, RavenParameter)
#endif
