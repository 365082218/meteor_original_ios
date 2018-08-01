//{ "engine_register_objects":["Starlite__Raven__RavenAnimationDataTweenVector3TransformPosition"] }
#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // RavenAnimationDataTweenVector3TransformPosition
        [Starlite.ObjectClassAttribute("Starlite::Raven::RavenAnimationDataTweenVector3TransformPosition")]
        public partial class RavenAnimationDataTweenVector3TransformPosition : global::Starlite.Raven.RavenAnimationDataTweenVector3Transform {
        	// Properties
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Tangents")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private int m_StartTangentParameterIndex = -1;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartTangentParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private Starlite.Raven.ERavenValueType m_StartTangentValueType = (Starlite.Raven.ERavenValueType) 0;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartTangentParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private UnityEngine.Vector3 m_StartTangentStart;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_StartTangentParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private UnityEngine.Vector3 m_StartTangentEnd;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private int m_EndTangentParameterIndex = -1;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_EndTangentParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private Starlite.Raven.ERavenValueType m_EndTangentValueType = (Starlite.Raven.ERavenValueType) 0;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_EndTangentParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private UnityEngine.Vector3 m_EndTangentStart;
        	[UnityEngine.SerializeField] [Raven.VisibleConditionAttribute("m_EndTangentParameterIndex < 0")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private UnityEngine.Vector3 m_EndTangentEnd;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_UseBezierCurve = false;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_RotateWithTangent = false;
        	[UnityEngine.SerializeField] [UnityEngine.HeaderAttribute("Camera Interpolation")] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private global::UnityEngine.Camera m_StartCamera;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private int m_StartCameraParameterIndex = -1;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private global::UnityEngine.Camera m_EndCamera;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private int m_EndCameraParameterIndex = -1;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private float m_CameraDepthDestination = 1.000000e+01f;
        	[UnityEngine.SerializeField] [Starlite.ObjectPropertyAttribute(Starlite.SourceCCProperty.PropertyAttribute.Preload)] private bool m_FromPointCasting = false;
        }
        
    }
    
}
#pragma warning restore
#else
// RavenAnimationDataTweenVector3TransformPosition
namespace Starlite { namespace Raven { 
SIMPLEMENT_CLASS_WITH_PROPERTIES_IN_CLASS_SCOPED(Starlite::Raven::RavenAnimationDataTweenVector3TransformPosition, RavenAnimationDataTweenVector3TransformPosition, RavenAnimationDataTweenVector3TransformPosition, Starlite::Raven::RavenAnimationDataTweenVector3Transform, Starlite::SceneObjectComponent, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_StartTangentParameterIndex, m_StartTangentParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_StartTangentValueType, m_StartTangentValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_StartTangentStart, m_StartTangentStart, kVector3, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_StartTangentEnd, m_StartTangentEnd, kVector3, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_EndTangentParameterIndex, m_EndTangentParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_EndTangentValueType, m_EndTangentValueType, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_EndTangentStart, m_EndTangentStart, kVector3, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_EndTangentEnd, m_EndTangentEnd, kVector3, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_UseBezierCurve, m_UseBezierCurve, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_RotateWithTangent, m_RotateWithTangent, kBool, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_StartCamera, m_StartCamera, kObject, kUnknown, Starlite::Camera::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_StartCameraParameterIndex, m_StartCameraParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_EndCamera, m_EndCamera, kObject, kUnknown, Starlite::Camera::TypeId, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_EndCameraParameterIndex, m_EndCameraParameterIndex, kInt, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_CameraDepthDestination, m_CameraDepthDestination, kFloat, kUnknown, -1, "")
SIMPLEMENT_PROPERTY_UNIVERSAL(RavenAnimationDataTweenVector3TransformPosition, m_FromPointCasting, m_FromPointCasting, kBool, kUnknown, -1, "")
SIMPLEMENT_CLASS_END(RavenAnimationDataTweenVector3TransformPosition)
} } 
SIMPLEMENT_CLASS_SCOPE(Starlite__Raven__RavenAnimationDataTweenVector3TransformPosition, Starlite::Raven, RavenAnimationDataTweenVector3TransformPosition)
#endif
