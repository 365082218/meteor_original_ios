using UnityEngine;
using UnityEngine.UI;
using System;
using Outfit7.Util;

namespace Outfit7.UI {
    [AddComponentMenu("UI/Atlas Int", 62), RequireComponent(typeof(HorizontalLayoutGroup))]
    public class AtlasInt : AbstractAtlasText {

        private const string Tag = "AtlasInt";

        [SerializeField] protected bool UseNumberGroupSeparator = false;
#if UNITY_EDITOR
        [SerializeField] private int EditorTest;
#endif

        protected int NumberGroupSeparatorIndex = int.MinValue;
        private int PreviousValue = int.MinValue;

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            SetExceptionalCharacters();

            if (UnityEditor.PrefabUtility.GetPrefabParent(gameObject) == null && UnityEditor.PrefabUtility.GetPrefabObject(gameObject) != null) {
                return;
            }

            if (Sprites.Count > 0 && Sprites[0] != null && Characters.Count > 0 && AtlasImages.Count > 0) {
                Number = EditorTest;
            }
        }
#endif

        protected override void Awake() {
            base.Awake();

            SetExceptionalCharacters();
        }

        protected override void OnSpriteInfoChanged() {
            base.OnSpriteInfoChanged();

            PreviousValue = int.MinValue;
        }

        private void SetExceptionalCharacters() {
            if (UseNumberGroupSeparator) {
                System.Globalization.NumberFormatInfo numberFormatInfo = new System.Globalization.NumberFormatInfo();
                if (UseNumberGroupSeparator) {
                    bool dotGroupSeparator = numberFormatInfo.NumberDecimalSeparator == ".";
                    NumberGroupSeparatorIndex = dotGroupSeparator ? Characters.IndexOf('.') : Characters.IndexOf(',');
                    Assert.State(NumberGroupSeparatorIndex > -1, "Number Group (Thousands) Separator sprite doesn't exist");
                }
            }
        }

        public int Number {
            set {
                if (Application.isPlaying && PreviousValue == value) {
                    return;
                }

                PreviousValue = value;

                int digit;
                int spriteIndex;
                int lastGroupSeparatorIndex = 0;
                for (int i = AtlasImages.Count - 1; i >= 0; i--) {
                    if (value > 0 || value == 0 && i == AtlasImages.Count - 1) {
                        AtlasImages[i].gameObject.SetActive(true);
                        int val = (AtlasImages.Count - 1 - i);
                        if (UseNumberGroupSeparator && (val == lastGroupSeparatorIndex + 3)) {
                            AtlasImages[i].sprite = Sprites[NumberGroupSeparatorIndex];
                            lastGroupSeparatorIndex = val + 1;
                            continue;
                        }
                        digit = value % 10;
                        spriteIndex = Characters.IndexOf(Convert.ToChar('0' + digit));
                        if (DifferentAtlasSprites || !Application.isPlaying) {
                            AtlasImages[i].SetSpriteAndMaterialWithSprite(Sprites[spriteIndex]);
                        } else {
                            AtlasImages[i].sprite = Sprites[spriteIndex];
                        }
                        value = value / 10;
                    } else {
                        AtlasImages[i].gameObject.SetActive(false);
                    }
                }

                if (value != 0) {
                    O7Log.ErrorT(Tag, "Not enough AtlasImages for characters");
                }
            }
        }
    }
}