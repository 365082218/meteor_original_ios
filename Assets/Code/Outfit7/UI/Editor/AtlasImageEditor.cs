using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEditor.AnimatedValues;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Outfit7.UI {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AtlasImage), true)]
    public class AtlasImageEditor : ImageEditor {

        private SerializedProperty CanvasAtlasProperty;
        private SerializedProperty Sprite;
        private SerializedProperty PreserveAspect;
        private SerializedProperty Type;
        private SerializedProperty Material;
        private SerializedProperty UpdateOnRectTransformDimensionsChangeProperty;
        private SerializedProperty RaycastTarget;
        private AnimBool ShowType;
        private AtlasImage AtlasImage;

        protected override void OnEnable() {
            base.OnEnable();

            CanvasAtlasProperty = serializedObject.FindProperty("CanvasAtlas");
            Sprite = serializedObject.FindProperty("m_Sprite");
            PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
            Type = serializedObject.FindProperty("m_Type");
            Material = serializedObject.FindProperty("m_Material");
            UpdateOnRectTransformDimensionsChangeProperty = serializedObject.FindProperty("UpdateOnRectTransformDimensionsChange");
            RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");

            ShowType = new AnimBool(Sprite.objectReferenceValue != null);
            ShowType.valueChanged.AddListener(Repaint);

            AtlasImage = target as AtlasImage;
        }

        protected override void OnDisable() {
            base.OnDisable();

            ShowType.valueChanged.RemoveListener(Repaint);
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.PropertyField(UpdateOnRectTransformDimensionsChangeProperty);
            EditorGUILayout.PropertyField(RaycastTarget);

            CanvasAtlas canvasAtlas = AtlasImage.GetCanvasAtlas();

            if (canvasAtlas == null || (PrefabUtility.GetPrefabParent(target) == null && PrefabUtility.GetPrefabObject(target) != null)) {
                EditorGUILayout.HelpBox("No CanvasAtlas found - data is serialized (no fear) but consider putting a CanvasAtlas on the GameObject", MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            List<Material> materials = new List<Material>();
            List<Texture> textures = new List<Texture>();
            List<Sprite> sprites = new List<Sprite>();
            List<string> spriteNames = new List<string>();
            canvasAtlas.GetCanvasAtlasListsEditor(ref materials, ref textures, ref sprites, ref spriteNames);

            if (PrefabUtility.GetPrefabParent(target) == null && PrefabUtility.GetPrefabObject(target) != null && canvasAtlas != null) {
                EditorGUILayout.HelpBox("Instantiate prefab to assign values - or drag it into the scene", MessageType.Info);
            } else {
                int selectedSprite = 0;
                if (Sprite.objectReferenceValue != null) {
                    for (int i = 0; i < sprites.Count; i++) {
                        if (sprites[i] == Sprite.objectReferenceValue as Sprite) {
                            if (Material.objectReferenceValue == materials[i]) {
                                selectedSprite = i;
                                break;
                            }
                        }
                    }
                    if (selectedSprite == -1) {
                        Material.objectReferenceValue = null;
                        selectedSprite = 0;
                    }
                }

                EditorGUI.BeginChangeCheck();
                selectedSprite = EditorGUILayout.Popup("Sprite", selectedSprite, spriteNames.ToArray());
                if (EditorGUI.EndChangeCheck()) {
                    Sprite newSprite = sprites[selectedSprite];
                    Sprite.objectReferenceValue = newSprite;
                    Material.objectReferenceValue = materials[selectedSprite];
                    if (newSprite != null) {
                        Image.Type oldType = (Image.Type) Type.enumValueIndex;
                        if (newSprite.border.SqrMagnitude() > 0) {
                            Type.enumValueIndex = (int) Image.Type.Sliced;
                        } else if (oldType == Image.Type.Sliced) {
                            Type.enumValueIndex = (int) Image.Type.Simple;
                        }
                    }
                }
            }

            GUI.enabled = false;
            EditorGUILayout.PropertyField(Sprite);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(m_Color);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(CanvasAtlasProperty);
            EditorGUILayout.PropertyField(Material);
            GUI.enabled = true;

            ShowType.target = Sprite.objectReferenceValue != null;
            if (EditorGUILayout.BeginFadeGroup(ShowType.faded)) {
                TypeGUI();
            }
            EditorGUILayout.EndFadeGroup();

            SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded)) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PreserveAspect);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void SetShowNativeSize(bool instant) {
            Image.Type type = (Image.Type) Type.enumValueIndex;
            bool showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled);
            base.SetShowNativeSize(showNativeSize, instant);
        }
    }
}