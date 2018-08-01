#if !UNITY_5_1 && !UNITY_5_2

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Outfit7.Util;

namespace Outfit7.UI {
    public class RichText : UnityEngine.UI.Text {

        private const string Tag = "RichText";

        private const string IconTemplateArg = "<quad name={0} width={1}/>";

        [System.Serializable]
        protected class SpriteInfo {
            public Sprite Sprite = null;
            public Vector2 Offset = Vector2.zero;
            public float Scale = 1f;
        }

        [TextArea(3, 10)][SerializeField] protected string UnformatedText = String.Empty;
        [SerializeField] private List<SpriteInfo> SpritesInfo = new List<SpriteInfo>();
        [SerializeField] private List<AtlasImage> ImageComponents = new List<AtlasImage>();
        [SerializeField] protected bool ParseImages = false;
        [SerializeField] protected CanvasAtlas CanvasAtlas;
        
        private readonly List<Pair<int, SpriteInfo>> ImagesVertexIndicesAndSpriteInfo = new List<Pair<int, SpriteInfo>>();
        private readonly List<Vector2> Positions = new List<Vector2>();
        private bool IsPupulatingText = false;

        public override string text {
            get {
                return UnformatedText;
            }
            set {
                UnformatedText = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        public int GetSpriteInfoCount() {
            return SpritesInfo.Count;
        }

        public void SetSpriteInfoOffset(int index, Vector2 offset) {
            SpritesInfo[index].Offset = offset;
        }

        public void SetSpriteInfoScale(int index, float scale) {
            SpritesInfo[index].Scale = scale;
        }

        public void SetSpriteInfoSprite(int index, Sprite sprite) {
            SpritesInfo[index].Sprite = sprite;
        }

        protected override void Awake() {
            base.Awake();

            // TODO: image offset and best fit don't work together
            // TODO: activate this if images are found in text
            alignByGeometry = true;
            supportRichText = true;

        }

        protected override void Start() {
            base.Start();

            SetCanvasAtlas();
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            SetCanvasAtlas();

            if (!ParseImages) {
                for (int i = 0; i < ImageComponents.Count; i++) {
                    ImageComponents[i].enabled = false;
                }
            }

            for (int i = 0; i < ImageComponents.Count; i++) {
                ImageComponents[i].SetControllerByMonoBehaviour();
            }
        }

        public void OnCanvasAtlasRefreshedEditor() {
            SetCanvasAtlas();

            SetVerticesDirty();
        }
#endif

        protected override void OnTransformParentChanged() {
            base.OnTransformParentChanged();

            SetCanvasAtlas();
        }

        private void SetCanvasAtlas() {
            if (CanvasAtlas == null) {
                CanvasAtlas = GetComponent<CanvasAtlas>();
                if (CanvasAtlas == null) {
                    CanvasAtlas = GetComponentInParent<CanvasAtlas>();
                }
                if (CanvasAtlas == null) {
                    O7Log.WarnT(Tag, "CanvasAtlas doesn't exist on the atlas");
                }
            }
        }

        public override void SetVerticesDirty() {
            if (IsPupulatingText) {
                return;
            }
            base.SetVerticesDirty();
            m_Text = ParseText(UnformatedText);
        }

        protected override void OnDisable() {
            base.OnDisable();

            if (ImageComponents == null) {
                return;
            }

            for (int i = 0; i < ImageComponents.Count; i++) {
                if (ImageComponents[i] != null) {
                    ImageComponents[i].enabled = false;
                }
            }
        }

        public void GetSpriteInfo(int index, Vector2 imageOffset) {
            if (index < 0 || index >= SpritesInfo.Count) {
                O7Log.ErrorT(Tag, "Setting sprite info index out of bounds");
                return;
            }

            SpritesInfo[index].Offset = imageOffset;
        }

        public void GetSpriteInfo(int index, Vector2 offset, float scale) {
            if (index < 0 || index >= SpritesInfo.Count) {
                O7Log.ErrorT(Tag, "Setting sprite info index out of bounds");
                return;
            }

            SpritesInfo[index].Offset = offset;
            SpritesInfo[index].Scale = scale;
        }

        private string ParseText(string unformatedText) {

            if (ParseImages) {
                unformatedText = ParseImagesInText(unformatedText);
            }

            return unformatedText;
        }

        private string ParseImagesInText(string unformatedText) {
            ImagesVertexIndicesAndSpriteInfo.Clear();

            int start = 0;
            int end;
            int currentImages = 0;

            while (start >= 0) {
                start = unformatedText.IndexOf("[", start);
                if (start == -1)
                    break;
                start++;
                end = unformatedText.IndexOf("]", start);
                if (end == -1)
                    break;

                int index;
                bool success = int.TryParse(unformatedText.Substring(start, end - start), out index);
                if (success && index >= 0 && index < SpritesInfo.Count) {
                    if (currentImages >= ImageComponents.Count) {
                        O7Log.WarnT(Tag, string.Format("Add more AtlasImage components (children) to this text control ({0}/{1})", currentImages + 1, ImageComponents.Count));
                        break;
                    }

                    SpriteInfo spriteInfo = SpritesInfo[index];
                    Sprite sprite = spriteInfo.Sprite;

                    AtlasImage img = ImageComponents[currentImages];
                    if (img == null) {
                        O7Log.WarnT(Tag, "Image component is null");
                        break; 
                    } 

                    CanvasAtlas canvasAtlas = img.GetCanvasAtlas();
                    if (canvasAtlas == null) {
                        break;
                    }

                    Sprite s;
                    Material m;
                    canvasAtlas.GetSpriteAndMaterial(sprite, out s, out m);
                    if (s == null) {
                        O7Log.WarnT(Tag, "Sprite not found");
                        img.sprite = null;
                        img.material = null;
                        img.enabled = false;
                        continue;
                    }

                    img.SetSpriteAndMaterial(s, m);

                    TextGenerationSettings settings = GetGenerationSettings(rectTransform.rect.size);

                    IsPupulatingText = true;
                    cachedTextGenerator.Populate(unformatedText, settings);
                    IsPupulatingText = false;

                    float size;
                    if (resizeTextForBestFit) {
                        size = cachedTextGenerator.fontSizeUsedForBestFit / pixelsPerUnit - 0.5f;
                    } else {
                        size = fontSize - 0.5f;
                    }

                    int picIndex = start - 1;
                    int endIndex = picIndex * 4 + 3;
                    ImagesVertexIndicesAndSpriteInfo.Add(new Pair<int, SpriteInfo>(endIndex, spriteInfo));

                    sprite = img.sprite;
                    float y = size;
                    float factor = sprite.rect.height / size;
                    float x = img.sprite.rect.width / factor;
                    float aspect = (sprite.rect.width / img.sprite.rect.height);

                    unformatedText = unformatedText.Remove(start - 1, end - start + 2);
                    unformatedText = unformatedText.Insert(start - 1, string.Format(IconTemplateArg, sprite.name, aspect));

                    img.rectTransform.sizeDelta = new Vector2(x * spriteInfo.Scale, y * spriteInfo.Scale);

                    img.enabled = true;
                    img.rectTransform.localScale = Vector3.one;
                    currentImages++;
                }
            }

            if (currentImages > 0) {
                Canvas.willRenderCanvases += PositionIcons;
            }

            for (int i = currentImages; i < ImageComponents.Count; i++) {
                if (ImageComponents[i] != null) {
                    ImageComponents[i].enabled = false;
                }
            }

            return unformatedText;
        }

        public void PositionIcons() {
            Canvas.willRenderCanvases -= PositionIcons;
            if (IsDestroyed() || !IsActive() || !enabled) {
                return;
            }

            for (int i = 0; i < ImagesVertexIndicesAndSpriteInfo.Count; i++) {
                AtlasImage img = ImageComponents[i];
                if (i < Positions.Count) {
                    img.rectTransform.anchorMax = img.rectTransform.anchorMin = rectTransform.pivot;
                    img.rectTransform.anchoredPosition = Positions[i];
                }
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill) {
            if (!ParseImages) {
                base.OnPopulateMesh(toFill);
                return;
            }

            string temporaryWorkaroundFixString = UnformatedText;
            UnformatedText = base.text;
            base.OnPopulateMesh(toFill);
            UnformatedText = temporaryWorkaroundFixString;

            m_DisableFontTextureRebuiltCallback = true;

            Positions.Clear();

            UIVertex vert = new UIVertex();

            for (int i = 0; i < ImagesVertexIndicesAndSpriteInfo.Count; i++) {
                int endIndex = ImagesVertexIndicesAndSpriteInfo[i].First;
                SpriteInfo spriteInfo = ImagesVertexIndicesAndSpriteInfo[i].Second;
                RectTransform rt = ImageComponents[i].rectTransform;
                Vector2 size = rt.rect.size;
                if (endIndex < toFill.currentVertCount) {
                    toFill.PopulateUIVertex(ref vert, endIndex);
                    Positions.Add(new Vector2((vert.position.x + size.x / 2 + spriteInfo.Offset.x * size.x), (vert.position.y + size.y / 2 + spriteInfo.Offset.y * size.y)));


                    // Erase the lower left corner of the black specks
                    toFill.PopulateUIVertex(ref vert, endIndex - 3);
                    Vector3 pos = vert.position;
                    for (int j = endIndex, m = endIndex - 3; j > m; j--) {
//                        toFill.PopulateUIVertex(ref vert, endIndex);
                        vert.position = pos;
                        toFill.SetUIVertex(vert, j);
                    }
                } else {
//                UNITY MESSAGE: Trying to remove Image (UnityEngine.UI.Image) from rebuild list while we are already inside a rebuild loop.
//                This is not supported.
//                ImageComponents[i].enabled = false;
                    // so screwing with scale is the workaround
                    rt.localScale = Vector3.zero;
                }
            }

            m_DisableFontTextureRebuiltCallback = false;
        }
    }
}
#endif