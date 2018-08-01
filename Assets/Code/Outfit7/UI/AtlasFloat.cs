using UnityEngine;
using UnityEngine.UI;
using System;
using Outfit7.Util;

namespace Outfit7.UI {
    [AddComponentMenu("UI/Atlas Float", 63), RequireComponent(typeof(HorizontalLayoutGroup))]
    public class AtlasFloat : AbstractAtlasText {

        private const string Tag = "AtlasFloat";

        [SerializeField] protected bool UseNumberGroupSeparator = false;
        [SerializeField] protected int DecimalNumbers = 2;
#if UNITY_EDITOR
        [SerializeField] private float EditorTest;
#endif

        protected int NumberGroupSeparatorIndex = int.MinValue;
        protected int NumberDecimalSeparatorIndex = int.MinValue;
        private float PreviousValue = float.MinValue;

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            SetExceptionalCharacters();

            if (UnityEditor.PrefabUtility.GetPrefabParent(gameObject) == null && UnityEditor.PrefabUtility.GetPrefabObject(gameObject) != null) {
                return;
            }

            Number = EditorTest;
        }
#endif

        protected override void Awake() {
            base.Awake();

            SetExceptionalCharacters();
        }

        protected override void OnSpriteInfoChanged() {
            base.OnSpriteInfoChanged();

            PreviousValue = float.MinValue;
        }

        private void SetExceptionalCharacters() {
            System.Globalization.NumberFormatInfo numberFormatInfo = new System.Globalization.NumberFormatInfo();
            if (UseNumberGroupSeparator) {
                if (UseNumberGroupSeparator) {
                    bool dotGroupSeparator = numberFormatInfo.NumberDecimalSeparator == ".";
                    NumberGroupSeparatorIndex = dotGroupSeparator ? Characters.IndexOf('.') : Characters.IndexOf(',');
                    Assert.State(NumberGroupSeparatorIndex > -1, "Number Group (Thousands) Separator sprite doesn't exist");
                }
            }

            bool dotDecimalSeparator = numberFormatInfo.NumberDecimalSeparator == ".";
            NumberDecimalSeparatorIndex = dotDecimalSeparator ? Characters.IndexOf('.') : Characters.IndexOf(',');
            Assert.State(NumberDecimalSeparatorIndex > -1, "Number Decimal Separator sprite doesn't exist");
        }

        public float Number {
            set {
                if (Application.isPlaying && PreviousValue == value) {
                    return;
                }

                PreviousValue = value;

                int number = (int) value;
                float fraction = value * Mathf.Pow(10, DecimalNumbers + 1);
                float lastNumber = fraction % 10;
                fraction /= 10f;
                int decimalRange = Mathf.RoundToInt(Mathf.Pow(10, DecimalNumbers));
                int fractionDigits = Mathf.RoundToInt(fraction) - number * decimalRange;
                if (fractionDigits > decimalRange && lastNumber < 0.5f) {
                    fractionDigits--;
                }

                int digit;
                int spriteIndex;

                for (int i = AtlasImages.Count - 1; i >= 0; i--) {
                    int val = AtlasImages.Count - 1 - i;
                    if (val < DecimalNumbers) {
                        AtlasImages[i].gameObject.SetActive(true);
                        if (fractionDigits >= 0) {
                            digit = fractionDigits % 10;
                            spriteIndex = Characters.IndexOf(Convert.ToChar('0' + digit));
                            AtlasImages[i].sprite = Sprites[spriteIndex];
                            fractionDigits = fractionDigits / 10;
                        }
                    } else {
                        break;
                    }
                }

                int digitCharacterIndex = (AtlasImages.Count - 1 - DecimalNumbers);
                AtlasImages[digitCharacterIndex].gameObject.SetActive(true);
                AtlasImages[digitCharacterIndex].sprite = Sprites[NumberDecimalSeparatorIndex];

                int lastGroupSeparatorIndex = DecimalNumbers;

                int count = AtlasImages.Count - 1 - DecimalNumbers - 1;
                for (int i = count; i >= 0; i--) {
                    if (number > 0 || number == 0 && i == count) {
                        AtlasImages[i].gameObject.SetActive(true);
                        int val = count - i;
                        if (UseNumberGroupSeparator && (val == lastGroupSeparatorIndex + 3)) {
                            AtlasImages[i].sprite = Sprites[NumberGroupSeparatorIndex];
                            lastGroupSeparatorIndex = val + 1;
                            continue;
                        }
                        digit = number % 10;
                        spriteIndex = Characters.IndexOf(Convert.ToChar('0' + digit));
                        AtlasImages[i].sprite = Sprites[spriteIndex];
                        number = number / 10;
                    } else {
                        AtlasImages[i].gameObject.SetActive(false);
                    }
                }

                if (number != 0) {
                    O7Log.ErrorT(Tag, "Not enough AtlasImages for characters");
                }
            }
        }
    }
}