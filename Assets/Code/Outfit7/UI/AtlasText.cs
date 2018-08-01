using Outfit7.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Outfit7.UI {
    [AddComponentMenu("UI/Atlas Text", 61), RequireComponent(typeof(HorizontalLayoutGroup))]
    public class AtlasText : AbstractAtlasText {

        private const string Tag = "AtlasText";
#if UNITY_EDITOR
        [SerializeField] private string EditorTest;
#endif

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            if (UnityEditor.PrefabUtility.GetPrefabParent(gameObject) == null && UnityEditor.PrefabUtility.GetPrefabObject(gameObject) != null) {
                return;
            }

            Text = EditorTest;
        }
#endif

        /// <summary>
        /// Use sparingly. Use Chars instead.
        /// </summary>
        /// <value>The text.</value>
        public string Text {
            set {
                Chars = value.ToCharArray();
            }
        }

        public char[] Chars {
            set {
                Init();
                for (int i = 0; i < AtlasImages.Count; i++) {
                    if (i < value.Length) {
                        AtlasImages[i].gameObject.SetActive(true);
                        AtlasImages[i].SetSpriteAndMaterialWithSprite(Sprites[Characters.IndexOf(value[i])]);
                    } else {
                        AtlasImages[i].gameObject.SetActive(false);
                    }
                }

                if (value.Length > AtlasImages.Count) {
                    O7Log.ErrorT(Tag, "Not enough AtlasImages for characters");
                }
            }
        }
    }
}