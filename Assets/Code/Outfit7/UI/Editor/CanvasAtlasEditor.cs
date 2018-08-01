using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Outfit7.Util;
using UnityEditorInternal;

namespace Outfit7.UI {
    [CustomEditor(typeof(CanvasAtlas), true)]
    public class CanvasAtlasEditor : UnityEditor.Editor {

        private ReorderableList EditorTexureAndMaterialList;

        private CanvasAtlas CanvasAtlas;

        private Vector2 AtlasesTotalSize = Vector2.zero;

        private List<Material> AtlasMaterials = new List<Material>();
        private List<Texture> AtlasTextures = new List<Texture>();

        protected virtual void OnEnable() {
            CanvasAtlas = target as CanvasAtlas;

            Pair<List<Texture>, List<Material>> materialsAndTextures = CanvasAtlas.GetUniqueMaterialsAndTexturesInProjectEditor();
            AtlasTextures = materialsAndTextures.First;
            AtlasMaterials = materialsAndTextures.Second;

            InitializeMaterialAndTexturesList();

            CanvasAtlas.RefreshEditor();
        }

        private void InitializeMaterialAndTexturesList() {
            EditorTexureAndMaterialList = new ReorderableList(serializedObject, 
                serializedObject.FindProperty("EditorTextureAndMaterial"), 
                false, true, true, true);
            EditorTexureAndMaterialList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 5 + 4;
            EditorTexureAndMaterialList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                SerializedProperty element = EditorTexureAndMaterialList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                if (index == 0) {
                    AtlasesTotalSize = Vector2.zero;
                }

                Texture texture = element.FindPropertyRelative("Texture").objectReferenceValue as Texture;
                if (texture == null) {
                    EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, rect.height - 5), "The serialized atlas texture was removed!", MessageType.Error);
                } else {
                    string textureName = texture.name;
                    textureName = textureName.Replace("_UI_ATL_RGB", "");
                    textureName = textureName.Replace("_ATL_RGB", "");

                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Texture", string.Format("{0} ({1}x{2})", textureName, texture.width, texture.height));
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Material"));
                    AtlasesTotalSize += new Vector2(texture.width, texture.height);
                }
            };
            EditorTexureAndMaterialList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, string.Format("Atlas list ({0}x{1})", AtlasesTotalSize.x, AtlasesTotalSize.y));
            };
            EditorTexureAndMaterialList.onSelectCallback = (ReorderableList l) => {
                Texture texture = l.serializedProperty.GetArrayElementAtIndex(l.index).FindPropertyRelative("Texture").objectReferenceValue as Texture;
                if (texture != null) {
                    EditorGUIUtility.PingObject(texture);
                }
            };
            EditorTexureAndMaterialList.onCanRemoveCallback = (ReorderableList l) => {
                return l.count > 0;
            };
            EditorTexureAndMaterialList.onRemoveCallback = (ReorderableList l) => {
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
                CanvasAtlas.RefreshEditor();
            };
            EditorTexureAndMaterialList.onAddCallback = (ReorderableList l) => {
                int index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
            };
            EditorTexureAndMaterialList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
                GenericMenu menu = new GenericMenu();
                //
                for (int i = 0; i < AtlasMaterials.Count; i++) {
                    string materialName = AtlasMaterials[i].name;
                    materialName = materialName.Replace("_UI_ATL_MAT", "");
                    materialName = materialName.Replace("_ATL_MAT", "");
                    menu.AddItem(new GUIContent(materialName), false, OnMenuSelect, new Pair<Material, Texture>(AtlasMaterials[i], AtlasTextures[i]));
                }
                    
                menu.ShowAsContext();
            };
        }

        private void OnMenuSelect(object menuData) {
            Pair<Material, Texture> data = menuData as Pair<Material, Texture>;
            int index = EditorTexureAndMaterialList.serializedProperty.arraySize;
            EditorTexureAndMaterialList.serializedProperty.arraySize++;
            EditorTexureAndMaterialList.index = index;
            SerializedProperty element = EditorTexureAndMaterialList.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("Material").objectReferenceValue = data.First;
            element.FindPropertyRelative("Texture").objectReferenceValue = data.Second;
            serializedObject.ApplyModifiedProperties();
            CanvasAtlas.RefreshEditor();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.Separator();

            EditorTexureAndMaterialList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
//
//            if (AtlasChanged) {
//                AtlasChanged = false;
//                CanvasAtlas.RefreshAtlasImages();
//            }
        }
    }
}