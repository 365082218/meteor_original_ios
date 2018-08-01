using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Outfit7.UI {
    public abstract class AbstractAtlasText : UIBehaviour {

        [SerializeField] protected bool DifferentAtlasSprites = false;
        [SerializeField] protected List<Sprite> CharacterSprites = new List<Sprite>();
        [SerializeField] protected List<char> TextCharacters = new List<char>();
        [SerializeField] protected List<AtlasImage> AtlasImages = new List<AtlasImage>();

        protected List<Sprite> Sprites;
        protected List<char> Characters;

        protected override void Awake() {
            base.Awake();

            Init();
        }

        protected virtual void Init() {
            if (Sprites == null || Characters == null) {
                Sprites = CharacterSprites;
                Characters = TextCharacters;
            }
        }

        public void SetCharacterSprites(List<Sprite> sprites) {
            Sprites = sprites;
            OnSpriteInfoChanged();
        }

        public void SetTextCharacters(List<char> chars) {
            Characters = chars;
            OnSpriteInfoChanged();
        }

        protected virtual void OnSpriteInfoChanged() {
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            for (int i = 0; i < AtlasImages.Count; i++) {
                AtlasImages[i].SetControllerByMonoBehaviour();
            }

            Sprites = CharacterSprites;
            Characters = TextCharacters;
        }
#endif
    }
}