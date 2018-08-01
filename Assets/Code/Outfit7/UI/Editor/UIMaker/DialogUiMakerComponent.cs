using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Outfit7.UI {
    public class DialogUiMakerComponent : UiMakerComponent {

        public override string TypeName { get { return "Dialog"; } }

        public override bool IsSimpleType { get { return false; } }

        public override string NamePrefix { get { return "dlg"; } }

        private CanvasUiMakerComponent.RaycasterType Raycaster = CanvasUiMakerComponent.RaycasterType.Graphic;
        private MonoScript SelectedScript = null;

        //        private bool CanvasAtlasToggle = true;
        //        private readonly bool[] AtlasPropertiesFoldOut = new bool[CanvasAtlas.AtlasTypeCount];
        //        private readonly CanvasAtlas.AtlasProperty[] AtlasProperties = new CanvasAtlas.AtlasProperty[CanvasAtlas.AtlasTypeCount];
        //        private readonly int[] SelectedAtlasTexture = new int[CanvasAtlas.AtlasTypeCount];
        //        private readonly bool[] SpritesFoldOut = new bool[CanvasAtlas.AtlasTypeCount];
        private List<Texture> AtlasTextures = new List<Texture>();

        private DialogUiMakerData Data;

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

            Data = ProjectUiMakerComponent.GetData<DialogUiMakerData>();
        }

        public override void OnGui() {
            base.OnGui();

            Raycaster = (CanvasUiMakerComponent.RaycasterType) EditorGUILayout.EnumPopup("Raycaster", Raycaster);
            SelectedScript = EditorGUILayout.ObjectField("Dialog controller", SelectedScript, typeof(MonoScript), false) as MonoScript;

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

            if (Selection.activeTransform != null && Selection.activeTransform.GetComponentInParent<Canvas>() != null) {
                EditorGUILayout.HelpBox("The dialog will be created on the current parent canvas regardless of where in the hierarchy it is", MessageType.Info);

                if (GUILayout.Button("Create", GUILayout.Height(33f))) {
                    OnCreateExecute();
                }
                if (Selection.activeGameObject != null && PrefabUtility.GetPrefabParent(Selection.activeGameObject) == null) {
                    if (GUILayout.Button("Create a prefab from selected GO", GUILayout.Height(33f))) {
                        OnCreatePrefab();
                    }
                }
            } else if (Selection.activeTransform == null) {
                EditorGUILayout.HelpBox("Select a Canvas, a Dialog or any of its children", MessageType.Info);
            }
        }

        protected override RectTransform OnCreateExecute() {
            RectTransform rectTransform = base.OnCreateExecute();

            rectTransform.SetParent(GetTopMostCanvasObject().transform, false);
            SetToStretch(rectTransform);

            rectTransform.gameObject.AddComponent(SelectedScript.GetClass());
            rectTransform.gameObject.AddComponent<Canvas>();

            switch (Raycaster) {
                case CanvasUiMakerComponent.RaycasterType.Graphic:
                    {
                        rectTransform.gameObject.AddComponent<GraphicRaycaster>();
                    }
                    break;
                case CanvasUiMakerComponent.RaycasterType.Physics: 
                    {
                        rectTransform.gameObject.AddComponent<PhysicsRaycaster>();
                    }
                    break;
                case CanvasUiMakerComponent.RaycasterType.Physics2D:
                    {
                        rectTransform.gameObject.AddComponent<Physics2DRaycaster>();
                    }
                    break;
            }
            rectTransform.gameObject.AddComponent<CanvasGroup>();
//            if (CanvasAtlasToggle) {
//                CanvasAtlas canvasAtlas = rectTransform.gameObject.AddComponent<CanvasAtlas>();
//                canvasAtlas.SetAtlasProperties(AtlasProperties);
//            }

//            CreateScripts();

            SetDefaults();

            return rectTransform;
        }

        protected void CreateScripts() {
            // TODO: not used due to different implementations of DialogControllerAnd
//            if (Data == null) {
//                return;
//            }
//
//            string name = SetPascalCase(RemoveSuffix(Name, "Dialog"));
//
//            string dialogControllerName = name + "DialogController";
//            string dialogStateName = name + "DialogState";
//
//            CreateFromTemplate(Data.DialogControllerFolderPath, Data.TemplateDialogControllerFilePath, dialogControllerName, "txt", "cs", new Dictionary<string, string> {
//                { "DialogControllerTemplate", dialogControllerName }
//            });
//
//            CreateFromTemplate(Data.DialogStateFolderPath, Data.TemplateDialogStateFilePath, dialogStateName, "txt", "cs", new Dictionary<string, string> {
//                { "DialogStateTemplate", dialogStateName },
//                { "DialogControllerTemplate", dialogControllerName }
//            });
        }

        protected void OnCreatePrefab() {
            string dialogPrefabFolderPath = string.Format("{0}/{1}", Application.dataPath, Data.DialogPrefabFolderPath);
            if (!System.IO.Directory.Exists(dialogPrefabFolderPath)) {
                if (EditorUtility.DisplayDialog("Are you sure?", 
                        "Directory for prefab doesn't exits. Do you want to create it?", 
                        "Yes", 
                        "No")) {
                    System.IO.Directory.CreateDirectory(dialogPrefabFolderPath);
                }
            }

            string dialogName = SetPascalCase(RemoveSuffix(Name, "Dialog")) + "Dialog";
            string prefabCreatePath = string.Format("Assets/{0}/{1}.prefab", Data.DialogPrefabFolderPath, dialogName);
            string prefabPath = string.Format("{0}/{1}.prefab", Data.DialogPrefabFolderPath, dialogName);
            CreatePrefab(dialogName, prefabPath, prefabCreatePath, Selection.activeGameObject, null, null);
        }

        protected override void SetDefaults() {
            base.SetDefaults();

            Name = string.Empty;

//            for (int i = 0; i < AtlasProperties.Length; i++) {
//                AtlasPropertiesFoldOut[i] = false;
//                SelectedAtlasTexture[i] = 0;
//                SpritesFoldOut[i] = true;
//                AtlasProperties[i].MainTexture = null;
//                AtlasProperties[i].Material = null;
//                AtlasProperties[i].Sprites.Clear();
//            }

            SelectedScript = null;
        }
    }
}
