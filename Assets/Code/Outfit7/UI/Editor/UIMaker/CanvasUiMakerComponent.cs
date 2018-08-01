using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

namespace Outfit7.UI {
    public class CanvasUiMakerComponent : UiMakerComponent {

        public enum RaycasterType {
            Graphic,
            Physics,
            Physics2D,
        }

        public override string TypeName { get { return "Canvas"; } }

        public override bool IsSimpleType { get { return true; } }

        public override string NamePrefix { get { return "can"; } }

        private bool PixelPerfect = false;

        private RaycasterType Raycaster = RaycasterType.Graphic;
        
        private Vector2 ReferenceResolution = new Vector2(960f, 1440f);

        // camera stuff
        private Camera Camera = null;

        private bool CreateCamera = false;

        private string CameraName = "UICamera";

        private int PlaneDistance = 10;

        private int Layer = LayerMask.NameToLayer("UI");

        private int OrthographicSize = 5;

        private float NearClipPlane = 0.3f;
        private float FarClipPlane = 20f;

        private float CameraZPosition = -10;

        private int Depth = 0;

        // Canvas atlas
        //        private bool CanvasAtlasToggle = true;
        //        private readonly bool[] AtlasPropertiesFoldOut = new bool[CanvasAtlas.AtlasTypeCount];
        //        private readonly CanvasAtlas.AtlasProperty[] AtlasProperties = new CanvasAtlas.AtlasProperty[CanvasAtlas.AtlasTypeCount];
        //        private readonly int[] SelectedAtlasTexture = new int[CanvasAtlas.AtlasTypeCount];
        //        private readonly bool[] SpritesFoldOut = new bool[CanvasAtlas.AtlasTypeCount];
        List<Texture> AtlasTextures = new List<Texture>();

        private CanvasUiMakerData Data;

        public override void Init() {
//            for (int i = 0; i < AtlasProperties.Length; i++) {
//                AtlasProperties[i] = new CanvasAtlas.AtlasProperty();
//            }
//
//            for (int i = 0; i < SpritesFoldOut.Length; i++) {
//                SpritesFoldOut[i] = true;
//            }

            // TODO: This goes into OnProjectFilesChanged or RefreshButton
            AtlasTextures.Clear();
            string[] assetguids = AssetDatabase.FindAssets("_ATL_RGB");
            for (int j = 0; j < assetguids.Length; j++) {
                Texture asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetguids[j]), typeof(Texture)) as Texture;
                if (asset != null) {
                    AtlasTextures.Add(asset);
                }
            }

            Data = ProjectUiMakerComponent.GetData<CanvasUiMakerData>();

            SetDefaults();
        }

        public override void OnGui() {
            base.OnGui();

            if (Selection.activeTransform == null || Selection.activeTransform != null && Selection.activeTransform.GetComponentInParent<Canvas>() == null) {
                PixelPerfect = EditorGUILayout.Toggle("Pixel Perfect", PixelPerfect);
                ReferenceResolution = EditorGUILayout.Vector2Field("Reference Resolution", ReferenceResolution);
                PlaneDistance = EditorGUILayout.IntField("Plane Distance", PlaneDistance);
                Raycaster = (RaycasterType) EditorGUILayout.EnumPopup("Raycaster", Raycaster);
                Layer = EditorGUILayout.LayerField("Layer", Layer);
                EditorGUILayout.Separator();

                GUILayout.Label("Camera");
                CreateCamera = EditorGUILayout.Toggle("Create Camera", CreateCamera);

                EditorGUI.indentLevel++;
                if (CreateCamera) {
                    CameraName = EditorGUILayout.TextField("Camera name", CameraName);
                    OrthographicSize = EditorGUILayout.IntField("Orthographic size", OrthographicSize);
                    NearClipPlane = EditorGUILayout.FloatField("Near Clip plane", NearClipPlane);
                    FarClipPlane = EditorGUILayout.FloatField("Far Clip Plane", FarClipPlane);
                    Depth = Mathf.Clamp(EditorGUILayout.IntField("Depth", Depth), -100, 100);
                    CameraZPosition = EditorGUILayout.FloatField("Camera Z Position", CameraZPosition);
                } else {
                    Camera = EditorGUILayout.ObjectField(Camera, typeof(Camera), true) as Camera;
                    if (Camera == null) {
                        Camera[] cameras = Object.FindObjectsOfType<Camera>();
                        for (int i = 0; i < cameras.Length; i++) {
                            if (cameras[i].cullingMask == 1 << Layer) { //LayerMask.NameToLayer("UI");
                                Camera = cameras[i];
                            }
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();
//            CanvasAtlasToggle = EditorGUILayout.Toggle("Add canvas atlas", CanvasAtlasToggle);
//            if (CanvasAtlasToggle) {
//                EditorGUI.indentLevel++;
//                for (int i = 0; i < CanvasAtlas.AtlasTypeCount; i++) {
//
//                    AtlasPropertiesFoldOut[i] = EditorGUILayout.Foldout(AtlasPropertiesFoldOut[i], string.Format("{0} {1}", ((CanvasAtlas.AtlasType) i), AtlasProperties[i].MainTexture != null ? string.Format("({0})", AtlasProperties[i].MainTexture.name.Replace("_ATL_RGB", "")) : ""));
//                    if (AtlasPropertiesFoldOut[i]) {
//                        CanvasAtlas.AtlasProperty atlasProperty = AtlasProperties[i];
//
//                        EditorGUI.indentLevel++;
//
//                        SelectedAtlasTexture[i] = atlasProperty.MainTexture == null ? 0 : 1;
//                        if (SelectedAtlasTexture[i] > 0) {
//                            SelectedAtlasTexture[i] = AtlasTextures.IndexOf(atlasProperty.MainTexture) + 1;
//                        }
//
//                        List<string> atlases = new List<string>();
//                        atlases.Add("---");
//                        for (int j = 0; j < AtlasTextures.Count; j++) {
//                            string textureName = AtlasTextures[j].name.Replace("_ATL_RGB", "");
//                            atlases.Add(textureName);
//                        }
//
//                        EditorGUI.BeginChangeCheck();
//                        SelectedAtlasTexture[i] = EditorGUILayout.Popup("Atlas", SelectedAtlasTexture[i], atlases.ToArray());
//                        bool atlasChanged = EditorGUI.EndChangeCheck();
//                        if (SelectedAtlasTexture[i] == 0) {
//                            atlasProperty.MainTexture = null;
//                        } else {
//                            atlasProperty.MainTexture = AtlasTextures[SelectedAtlasTexture[i] - 1];
//                        }
//
//                        GUI.enabled = false;
//                        EditorGUILayout.ObjectField(atlasProperty.MainTexture, typeof(Texture), false);
//                        GUI.enabled = true;
//                        if (atlasChanged) {
//                            if (atlasProperty.MainTexture != null) {
//                                string filter = atlasProperty.MainTexture.name.Remove(atlasProperty.MainTexture.name.Length - 8) + "_ATL_MAT";
//                                Material[] materials = atlasProperty.MainTexture != null ? UiMakerComponent.GetAssetsWithFilter<Material>(filter) : null;
//                                atlasProperty.Material = materials != null && materials.Length > 0 ? materials[0] : null;
//                            } else {
//                                atlasProperty.Material = null;
//                            }
//                        }
//                        atlasProperty.Material = EditorGUILayout.ObjectField(atlasProperty.Material, typeof(Material), false) as Material;
//                        if (atlasProperty.Material == null) {
//                            EditorGUILayout.HelpBox("No material found.\nMaterial must be named [TextureName]_ATL_MAT\nRGB texture must be named [TextureName]_ATL_RGB\nAlpha texture must be named [TextureName]_ATL_A", MessageType.Error);
//                        }
//
//                        if (atlasChanged) {
//                            if (AtlasProperties[i] != null) {
//                                if (AtlasProperties[i].MainTexture != null) {
//                                    Texture texture = AtlasProperties[i].MainTexture;
//                                    string atlasPath = AssetDatabase.GetAssetPath(texture);
//                                    List<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(atlasPath).OfType<Sprite>().ToList();
//                                    AtlasProperties[i].Sprites = sprites;
//                                } else {
//                                    AtlasProperties[i].Sprites.Clear();
//                                }
//                            }
//                        }
//
//                        int count = AtlasProperties[i].Sprites.Count;
//                        if (count > 1) {
//                            EditorGUI.indentLevel++;
//                            SpritesFoldOut[i] = EditorGUILayout.Foldout(SpritesFoldOut[i], string.Format("Sprites ({0})", count));
//                            GUI.enabled = false;
//                            if (SpritesFoldOut[i]) {
//                                for (int j = 0; j < AtlasProperties[i].Sprites.Count; j++) {
//                                    EditorGUILayout.ObjectField(AtlasProperties[i].Sprites[j], typeof(Sprite), false);    
//                                }
//                            }
//                            EditorGUI.indentLevel--;
//                            GUI.enabled = true;
//                        }
//
//                        EditorGUI.indentLevel--;
//                    }
//                }
//                EditorGUI.indentLevel--;
//            }

            SetHelpBox();

            if (GUILayout.Button("Create", GUILayout.Height(33f))) {
                OnCreateExecute();
            }
        }

        private void SetHelpBox() {
            if (Selection.activeTransform == null || Selection.activeTransform != null && Selection.activeTransform.GetComponentInParent<Canvas>() == null) {
                EditorGUILayout.HelpBox("This will create a Canvas in scene", MessageType.Info);
            } else {
                EditorGUILayout.HelpBox("This will create an inherited Canvas", MessageType.Info);
            }
        }

        protected override void SetDefaults() {
            base.SetDefaults();

            Name = "Canvas";
            if (Data != null) {
                ReferenceResolution = Data.ReferenceResolution;
                CameraName = Data.CameraName;
                PlaneDistance = Data.PlaneDistance;
                Layer = Data.Layer;
                OrthographicSize = Data.OrthographicSize;
                NearClipPlane = Data.NearClipPlane;
                FarClipPlane = Data.FarClipPlane;
                CameraZPosition = Data.CameraZPosition;
                Depth = Data.CameraDepth;
            }
            Camera = null;

//            for (int i = 0; i < AtlasProperties.Length; i++) {
//                AtlasPropertiesFoldOut[i] = false;
//                SelectedAtlasTexture[i] = 0;
//                SpritesFoldOut[i] = true;
//                AtlasProperties[i].MainTexture = null;
//                AtlasProperties[i].Material = null;
//                AtlasProperties[i].Sprites.Clear();
//            }
        }

        protected override RectTransform OnCreateExecute() {
            if (Selection.activeTransform == null && CreateCamera) {
                GameObject cameraGameObject = new GameObject(CameraName);
                Camera camera = cameraGameObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.Nothing;
                camera.cullingMask = 1 << Layer;
                camera.orthographic = true;
                camera.orthographicSize = OrthographicSize;
                camera.nearClipPlane = NearClipPlane;
                camera.farClipPlane = FarClipPlane;
                camera.depth = Depth;
                camera.transform.localPosition = Vector3.forward * CameraZPosition;
                camera.useOcclusionCulling = false;
                Camera = camera;
            } else if (Camera == null && (Selection.activeTransform == null || Selection.activeTransform.GetComponentInParent<Canvas>() == null)) {
                throw new UnityException("Camera not set");
            }

            RectTransform rectTransform = base.OnCreateExecute();

            rectTransform.SetAsFirstSibling();
            if (Camera != null) {
                Camera.transform.SetAsFirstSibling();
            }

            if (Selection.activeTransform != null && Selection.activeTransform.GetComponentInParent<Canvas>() == null) {
                rectTransform.SetParent(null, false);
            }


            Canvas canvas = rectTransform.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera;
            canvas.planeDistance = PlaneDistance;
            canvas.pixelPerfect = PixelPerfect;
            canvas.gameObject.layer = Layer;

            CanvasScaler canvasScaler = rectTransform.gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = ReferenceResolution;

            switch (Raycaster) {
                case RaycasterType.Graphic:
                    {
                        rectTransform.gameObject.AddComponent<GraphicRaycaster>();
                    }
                    break;
                case RaycasterType.Physics: 
                    {
                        rectTransform.gameObject.AddComponent<PhysicsRaycaster>();
                    }
                    break;
                case RaycasterType.Physics2D:
                    {
                        rectTransform.gameObject.AddComponent<Physics2DRaycaster>();
                    }
                    break;
            }

            if (Camera != null) {
                rectTransform.gameObject.AddComponent<CanvasSetter>();
            }
//            if (CanvasAtlasToggle) {
//                CanvasAtlas canvasAtlas = rectTransform.gameObject.AddComponent<CanvasAtlas>();
//                canvasAtlas.SetAtlasProperties(AtlasProperties);
//            }

            SetDefaults();

            return rectTransform;
        }
    }
}
