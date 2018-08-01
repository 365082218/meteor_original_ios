using UnityEditor;
using UnityEngine;

namespace Outfit7.UI {
    public class CanvasUiMakerData : UiMakerData {

        protected override string TypeName { get { return "Canvas"; } }

        public Vector2 ReferenceResolution = new Vector2(960f, 1440f);
        public string CameraName = "UICamera";
        public int PlaneDistance = 10;
        public int Layer = LayerMask.NameToLayer("UI");
        public int OrthographicSize = 5;
        public float NearClipPlane = 0.3f;
        public float FarClipPlane = 20f;
        public float CameraZPosition = -10;
        public int CameraDepth = 0;

        private Vector2 CurrentReferenceResolution = Vector2.zero;
        private string CurrentCameraName = string.Empty;
        private int CurrentPlaneDistance = 0;
        private int CurrentLayer = 0;
        private int CurrentOrthographicSize = 0;
        private float CurrentNearClipPlane = 0f;
        private float CurrentFarClipPlane = 0f;
        private float CurrentCameraZPosition = 0f;
        private int CurrentCameraDepth = 0;

        public override bool ChangesMade { 
            get { 
                return 
                    ReferenceResolution != CurrentReferenceResolution ||
                    CameraName != CurrentCameraName ||
                    PlaneDistance != CurrentPlaneDistance ||
                    Layer != CurrentLayer ||
                    OrthographicSize != CurrentOrthographicSize ||
                    NearClipPlane != CurrentNearClipPlane ||
                    FarClipPlane != CurrentFarClipPlane ||
                    CameraZPosition != CurrentCameraZPosition ||
                    CameraDepth != CurrentCameraDepth;
            }
        }

        public override void OnInit() {
            OnRevertChangedData();
        }

        public override void OnGui() {
            FoldOut = EditorGUILayout.Foldout(FoldOut, TypeName);
            if (FoldOut) {
                EditorGUI.indentLevel++;
                CurrentReferenceResolution = EditorGUILayout.Vector2Field("Default reference resolution", CurrentReferenceResolution);
                CurrentCameraName = EditorGUILayout.TextField("Default camera name", CurrentCameraName);
                CurrentPlaneDistance = EditorGUILayout.IntField("Default plane distance", CurrentPlaneDistance);
                CurrentLayer = EditorGUILayout.LayerField("Default layer", CurrentLayer);
                CurrentOrthographicSize = EditorGUILayout.IntField("Default orthographic size", CurrentOrthographicSize);
                CurrentNearClipPlane = EditorGUILayout.FloatField("Default near clip plane", CurrentNearClipPlane);
                CurrentFarClipPlane = EditorGUILayout.FloatField("Default far clip plane", CurrentFarClipPlane);
                CurrentCameraZPosition = EditorGUILayout.FloatField("Default camera Z position", CurrentCameraZPosition);
                CurrentCameraDepth = EditorGUILayout.IntField("Default camera depth", CurrentCameraDepth);

                OnBottomButtonGui();
                EditorGUI.indentLevel--;
            }
        }

        public override void OnApplyChangedData() {
            ReferenceResolution = CurrentReferenceResolution;
            CameraName = CurrentCameraName;
            PlaneDistance = CurrentPlaneDistance;
            Layer = CurrentLayer;
            OrthographicSize = CurrentOrthographicSize;
            NearClipPlane = CurrentNearClipPlane;
            FarClipPlane = CurrentFarClipPlane;
            CameraZPosition = CurrentCameraZPosition;
            CameraDepth = CurrentCameraDepth;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public override void OnRevertChangedData() {
            CurrentReferenceResolution = ReferenceResolution;
            CurrentCameraName = CameraName;
            CurrentPlaneDistance = PlaneDistance;
            CurrentLayer = Layer;
            CurrentOrthographicSize = OrthographicSize;
            CurrentNearClipPlane = NearClipPlane;
            CurrentFarClipPlane = FarClipPlane;
            CurrentCameraZPosition = CameraZPosition;
            CurrentCameraDepth = CameraDepth;
            AssetDatabase.SaveAssets();
        }
    }
}
