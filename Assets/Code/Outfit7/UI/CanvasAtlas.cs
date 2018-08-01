using System;
using System.Collections.Generic;
using Outfit7.Util;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace Outfit7.UI {
    [AddComponentMenu("UI/Canvas Atlas", 59)]
    public class CanvasAtlas : UIBehaviour {

        [SerializeField] private List<Material> SpriteMaterials = new List<Material>();
        [SerializeField] private List<Sprite> Sprites = new List<Sprite>();
        [SerializeField] private List<string> SpriteNames = new List<string>();

        public Sprite GetSprite(string spriteName) {
            int index = SpriteNames.IndexOf(spriteName);
            if (index >= 0) {
                return Sprites[index];
            }

            return null;
        }

        public void GetSpriteAndMaterial(string spriteName, out Sprite spriteRef, out Material materialRef, int occurance = 0) {
            if (occurance == 0) {
                int index = SpriteNames.IndexOf(spriteName);
                if (index >= 0) {
                    spriteRef = Sprites[index];
                    materialRef = SpriteMaterials[index];
                } else {
                    spriteRef = null;
                    materialRef = null;
                }
            } else {
                int o = 0;
                for (int i = 0; i < SpriteNames.Count; i++) {
                    if (SpriteNames[i] == spriteName) {
                        if (o == occurance) {
                            spriteRef = Sprites[i];
                            materialRef = SpriteMaterials[i];
                            return;
                        } else {
                            o++;
                        }
                    }
                }
                spriteRef = null;
                materialRef = null;
            }
        }

        public void GetSpriteAndMaterial(Sprite newSprite, out Sprite spriteRef, out Material materialRef) {
            int index = Sprites.IndexOf(newSprite);
            if (index >= 0) {
                spriteRef = Sprites[index];
                materialRef = SpriteMaterials[index];
            } else {
                spriteRef = null;
                materialRef = null;
            }
        }

#if UNITY_EDITOR
        [Serializable]
        public class TextureAndMaterial {
            public Texture Texture;
            public Material Material;

            public TextureAndMaterial(Texture texture, Material material) {
                Texture = texture;
                Material = material;
            }
        }

        [SerializeField] private List<TextureAndMaterial> EditorTextureAndMaterial = new List<TextureAndMaterial>();
        [SerializeField] private List<Texture> EditorSpriteTextures = new List<Texture>();
#endif

#if UNITY_EDITOR
        public List<Material> GetSpriteMaterialsEditor() {
            return SpriteMaterials;
        }

        public List<Sprite> GetSpritesEditor() { 
            return Sprites;
        }

        public List<Texture> GetTexturesEditor() { 
            return EditorSpriteTextures;
        }

        public List<string> GetSpritesNamesEditor() { 
            return SpriteNames;
        }

        public Pair<List<Texture>, List<Material>> GetUniqueMaterialsAndTexturesInProjectEditor() {
            Pair<List<Texture>, List<Material>> materialsAndTextures = new Pair<List<Texture>, List<Material>>();
            materialsAndTextures.First = new List<Texture>();
            materialsAndTextures.Second = new List<Material>();

            List<Texture> atlasTexturesInProject = new List<Texture>();

            string[] assetguids = AssetDatabase.FindAssets("_ATL_RGB");
            for (int j = 0; j < assetguids.Length; j++) {
                Texture texture = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetguids[j]), typeof(Texture)) as Texture;
                if (texture != null) {
                    atlasTexturesInProject.Add(texture);
                }
            }

            for (int i = 0; i < atlasTexturesInProject.Count; i++) {
                string path = AssetDatabase.GetAssetPath(atlasTexturesInProject[i]);
                path = path.Replace("_ATL_RGB.png", "_ATL_MAT.mat");
                Material material = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                if (material != null) {
                    materialsAndTextures.First.Add(atlasTexturesInProject[i]);
                    materialsAndTextures.Second.Add(material);
                }
            }

            return materialsAndTextures;
        }

        public void RefreshEditor() {
            List<Pair<Texture, Material>> textureAndMaterials = new List<Pair<Texture, Material>>();
            for (int i = 0; i < SpriteMaterials.Count; i++) {
                textureAndMaterials.Add(new Pair<Texture, Material>(EditorSpriteTextures[i], SpriteMaterials[i]));
            }
            SpriteMaterials.Clear();
            Sprites.Clear();
            SpriteNames.Clear();
            EditorSpriteTextures.Clear();

            Pair<List<Texture>, List<Material>> materialsAndTextures = GetUniqueMaterialsAndTexturesInProjectEditor();
            List<Texture> atlasTextures = materialsAndTextures.First;
            List<Material> atlasMaterials = materialsAndTextures.Second;

            for (int i = 0; i < EditorTextureAndMaterial.Count; i++) {
                if (EditorTextureAndMaterial[i].Texture != null) {
                    Texture texture = EditorTextureAndMaterial[i].Texture;
                    string atlasPath = AssetDatabase.GetAssetPath(texture);
                    List<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(atlasPath).OfType<Sprite>().ToList();
                    Sprites.AddRange(sprites);
                    Material material = EditorTextureAndMaterial[i].Material;
                    if (material == null) {
                        int idx = atlasTextures.IndexOf(texture);
                        if (idx >= 0) {
                            material = atlasMaterials[idx];
                        } else {
                            material = null;
                        }
                    }
                    for (int j = 0; j < sprites.Count; j++) {
                        SpriteMaterials.Add(material);
                        SpriteNames.Add(sprites[j].name);
                        EditorSpriteTextures.Add(texture);
                    }
                } else {
                    EditorTextureAndMaterial.RemoveAt(i);
                    i--;
                }
            }

            BroadcastMessage("OnCanvasAtlasRefreshedEditor", SendMessageOptions.DontRequireReceiver);
        }

        public void GetCanvasAtlasListsEditor(ref List<Material> materials, ref List<Texture> textures, ref List<Sprite> sprites, ref List<string> spriteNames) {
            if (materials != null) {
                materials.Add(null);
                materials.AddRange(GetSpriteMaterialsEditor());
            }

            if (textures != null) {
                textures.Add(null);
                textures.AddRange(GetTexturesEditor());
            }

            GetCanvasAtlasListsEditor(ref sprites, ref spriteNames);
        }

        public void GetCanvasAtlasListsEditor(ref List<Sprite> sprites, ref List<string> spriteNames) {
            if (sprites != null) {
                sprites.Add(null);
                sprites.AddRange(GetSpritesEditor());
            }

            if (spriteNames != null) {
                spriteNames.Add("---");
                List<string> originalSpriteNames = GetSpritesNamesEditor();
                for (int i = 0; i < originalSpriteNames.Count; i++) {
                    spriteNames.Add(string.Format("{0} ({1})", originalSpriteNames[i].Replace("_UI_TEX", ""), SpriteMaterials[i].name));
                }
            }
        }
#endif
    }
}