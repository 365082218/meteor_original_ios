using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Outfit7.UI {
    public class ImageUiMakerComponent : UiMakerComponent {

        public override string TypeName { get { return "Image"; } }

        public override bool IsSimpleType { get { return true; } }

        public override string NamePrefix { get { return IsRawImage ? "rim" : "img"; } }

        private Material Material = null;

        private bool IsRawImage = false;
        private bool ImageLoader = false;
        private bool ImageLoaderWithFrame = true;
        private Texture2D DefaultTexture = null;
        private Texture2D Texture = null;
        private Sprite AtlasImageSprite = null;

        //        private readonly List<string> AtlasTypes = new List<string>(CanvasAtlas.AtlasTypeCount);

        public void PreSelectRawImage() {
            IsRawImage = true;
            ImageLoader = false;
        }

        public void PreSelectImageLoader(bool imageLoaderWithFrame) {
            IsRawImage = true;
            ImageLoader = true;
            ImageLoaderWithFrame = imageLoaderWithFrame;
        }

        public void PreSelectAtlasImage() {
            IsRawImage = false;
            ImageLoader = false;
        }

        public override void Init() {
//            AtlasTypes.Clear();
//            for (int i = 0; i < CanvasAtlas.AtlasTypeCount; i++) {
//                AtlasTypes.Add(((CanvasAtlas.AtlasType) i).ToString());
//            }
        }

        public override void OnGui() {
            base.OnGui();

            IsRawImage = EditorGUILayout.Toggle("Raw Image", IsRawImage);
            if (IsRawImage) {
                EditorGUI.indentLevel++;
                ImageLoader = EditorGUILayout.Toggle("Image Loader", ImageLoader);
                if (ImageLoader) {
                    EditorGUI.indentLevel++;
                    if (ImageLoaderWithFrame) {
                        ImageLoaderWithFrame = EditorGUILayout.Toggle("Image Loader With Frame", ImageLoaderWithFrame);        
                    }
                    DefaultTexture = EditorGUILayout.ObjectField("Default Texture", DefaultTexture, typeof(Texture2D), false) as Texture2D;
                    EditorGUI.indentLevel--;
                } else {
                    Texture = EditorGUILayout.ObjectField(Texture, typeof(Texture2D), false) as Texture2D;
                }
                EditorGUI.indentLevel--;
            }

            if (IsRawImage) {
                GUILayout.Label("Material", EditorStyles.boldLabel);
                EditorGUILayout.Separator();
                Material = EditorGUILayout.ObjectField(Material, typeof(Material), false) as Material;
            } else {
                if (Selection.activeGameObject != null) {
                    CanvasAtlas canvasAtlas = Selection.activeGameObject.GetComponentInParent<CanvasAtlas>();
                    if (canvasAtlas != null) {
                        GUILayout.Label("Material", EditorStyles.boldLabel);

                        List<Sprite> sprites = canvasAtlas.GetSpritesEditor();

                        int selectedSprite = 0;
                        if (AtlasImageSprite != null) {
                            selectedSprite = sprites.IndexOf(AtlasImageSprite) + 1;
                        }

                        List<string> spritesStrings = new List<string>();
                        spritesStrings.Add("---");
                        for (int i = 0; i < sprites.Count; i++) {
                            spritesStrings.Add(sprites[i].name);
                        }

                        EditorGUI.BeginChangeCheck();
                        selectedSprite = EditorGUILayout.Popup("Sprite", selectedSprite, spritesStrings.ToArray());
                        if (EditorGUI.EndChangeCheck()) {
                            Sprite newSprite;
                            if (selectedSprite == 0) {
                                newSprite = null;
                            } else {
                                newSprite = sprites.Count > 0 ? sprites[Mathf.Max(selectedSprite - 1, 0)] : null;
                            }
                            AtlasImageSprite = newSprite;
                        }

                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(AtlasImageSprite, typeof(Sprite), false);
                        GUI.enabled = true;
                    }
                }
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            SetCommonCreatePanel();
        }

        protected override RectTransform OnCreateExecute() {
            if (IsRawImage) {
                if (ImageLoader && DefaultTexture == null) {
                    throw new UnityException("Default Texture for profile is null");
                }
                if (Material == null) {
                    throw new UnityException("Material for Raw Image is null");
                }
            }

            RectTransform rectTransform = base.OnCreateExecute();

            if (IsRawImage) {
                RawImage rawImage = rectTransform.gameObject.AddComponent<RawImage>();
                rawImage.material = Material;
                if (ImageLoader) {
                    CommonImageLoader imageLoader = rectTransform.gameObject.AddComponent<CommonImageLoader>();
                    imageLoader.SetRawImage(rawImage);
                    imageLoader.SetDefaultTexture(DefaultTexture);

                    if (ImageLoaderWithFrame) {
                        GameObject goFrame = new GameObject();
                        goFrame.transform.SetParent(rectTransform, false);
                        goFrame.name = "img_frame";
                        goFrame.AddComponent<AtlasImage>();

                        RectTransform newRt = goFrame.transform as RectTransform;
                        newRt.anchorMin = Vector2.zero;
                        newRt.anchorMax = Vector2.one;
                        newRt.offsetMin = new Vector2(5, 5);
                        newRt.offsetMax = new Vector2(-5, -5);
                    }
                } else {
                    rawImage.texture = Texture;
                }

                rawImage.SetNativeSize();
            } else {
                AtlasImage atlasImage = rectTransform.gameObject.AddComponent<AtlasImage>();
                if (Selection.activeGameObject != null) {
                    CanvasAtlas canvasAtlas = Selection.activeGameObject.GetComponentInParent<CanvasAtlas>();
                    if (canvasAtlas != null) {
                        atlasImage.SetSpriteAndMaterialWithSprite(AtlasImageSprite);
                    }
                }
                atlasImage.SetNativeSize();
            }

            SetDefaults();

            return rectTransform;
        }

        protected override void SetDefaults() {
            base.SetDefaults();

            IsRawImage = false;
            Texture = null;
            AtlasImageSprite = null;
            DefaultTexture = null;
            ImageLoader = false;
            ImageLoaderWithFrame = true;
        }
    }
}
