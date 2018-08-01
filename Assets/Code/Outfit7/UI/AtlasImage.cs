using UnityEngine;
using UnityEngine.UI;
using Outfit7.Util;

namespace Outfit7.UI {
    [AddComponentMenu("UI/Atlas Image", 60)]
    public class AtlasImage : Image {

        private const string Tag = "AtlasImage";

        [SerializeField] protected bool UpdateOnRectTransformDimensionsChange = true;
        [SerializeField] protected CanvasAtlas CanvasAtlas;

#if UNITY_EDITOR
        [SerializeField] private bool ControllerByMonoBehaviour = false;

        public void SetControllerByMonoBehaviour() {
            ControllerByMonoBehaviour = true;
        }
#endif

        public bool CallOnRectTransformDimensionsChange {
            get {
                return UpdateOnRectTransformDimensionsChange;
            }
            set {
                UpdateOnRectTransformDimensionsChange = value;
            }
        }

        public string SpriteName {
            get {
                return sprite != null ? sprite.name : string.Empty;
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    sprite = null;
                } else {
                    SetCanvasAtlas();
                    Sprite s = CanvasAtlas.GetSprite(value);
                    if (BuildConfig.IsProdOrDevel && s == null) {
                        O7Log.WarnT(Tag, string.Format("SpriteName \"{0}\" doesn't exist in atlas \"{1}\"", value, material.name));
                    }
                    sprite = s;
                }
            }
        }

        public void SetSpriteAndMaterialWithName(string spriteName, int occurance = 0) {
            if (string.IsNullOrEmpty(spriteName)) {
                sprite = null;
                material = null;
            } else {
                SetCanvasAtlas();
                Sprite s;
                Material m;
                CanvasAtlas.GetSpriteAndMaterial(spriteName, out s, out m, occurance);
                material = m;
                sprite = s;
            }
        }

        public void SetSpriteAndMaterialWithSprite(Sprite newSprite) {
            if (newSprite == null) {
                sprite = null;
                material = null;
                return;
            }
                
            SetCanvasAtlas();
            Sprite s;
            Material m;
            CanvasAtlas.GetSpriteAndMaterial(newSprite, out s, out m);
            material = m;
            sprite = s;
        }

        public void SetSpriteAndMaterial(Sprite newSprite, Material newMaterial) {
            material = newMaterial;
            sprite = newSprite;
        }

        public CanvasAtlas GetCanvasAtlas() {
            SetCanvasAtlas();
            return CanvasAtlas;
        }

        protected override void Start() {
            base.Start();

            SetCanvasAtlas();
        }

        private void SetCanvasAtlas() {
            if (CanvasAtlas == null) {
                CanvasAtlas = GetComponentInParent<CanvasAtlas>();
                if (CanvasAtlas == null) {
                    O7Log.WarnT(Tag, "CanvasAtlas doesn't exist on the atlas");
                }
            }
        }

        protected override void OnRectTransformDimensionsChange() {
            if (!Application.isPlaying || UpdateOnRectTransformDimensionsChange) {
                base.OnRectTransformDimensionsChange();
            }
        }

        protected override void OnTransformParentChanged() {
            base.OnTransformParentChanged();

            if (CanvasAtlas == null) {
                CanvasAtlas = GetComponentInParent<CanvasAtlas>();
            }
        }

#if UNITY_EDITOR
        public void OnCanvasAtlasRefreshedEditor() {
            if (ControllerByMonoBehaviour) {
                return;
            }

            if (CanvasAtlas == null) {
                CanvasAtlas = GetComponentInParent<CanvasAtlas>();
            }

            if (enabled) {
                enabled = false;
                enabled = true;
            }
        }
#endif
    }
}