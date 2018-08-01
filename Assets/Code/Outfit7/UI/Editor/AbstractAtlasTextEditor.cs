using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Outfit7.UI {
    [CustomEditor(typeof(AbstractAtlasText), true)]
    public class AbstractAtlasTextEditor : UnityEditor.Editor {

        private SerializedProperty DifferentAtlasSpritesProperty;
        private SerializedProperty TextCharactersProperty;

        private ReorderableList GraphicComponents;
        private ReorderableList SpritesInfo;

        private GUIContent NoContent;

        protected virtual void OnEnable() {
            DifferentAtlasSpritesProperty = serializedObject.FindProperty("DifferentAtlasSprites");
            TextCharactersProperty = serializedObject.FindProperty("TextCharacters");

            InitializeGraphicComponentList();
            InitializeSpriteInfoList();

            NoContent = new GUIContent("");
        }

        private void RefreshGraphicComponentList(ReorderableList l) {
            for (int i = 0; i < l.serializedProperty.arraySize; i++) {
                if (l.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue == null) {
                    l.serializedProperty.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }
        }

        private void InitializeGraphicComponentList() {
            GraphicComponents = new ReorderableList(serializedObject,
                serializedObject.FindProperty("AtlasImages"),
                false, true, true, true);
            GraphicComponents.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = GraphicComponents.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                GUI.enabled = false;
                EditorGUI.PropertyField(new Rect(rect.x + ReorderableList.Defaults.dragHandleWidth - ReorderableList.Defaults.padding, rect.y, rect.width - ReorderableList.Defaults.dragHandleWidth, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                GUI.enabled = true;
            };
            GraphicComponents.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Atlas Images");
            GraphicComponents.onCanRemoveCallback = (ReorderableList l) => l.count > 0;
            GraphicComponents.onRemoveCallback = (ReorderableList l) => {
                AtlasImage toBeDeletedGraphic = l.serializedProperty.GetArrayElementAtIndex(l.index).objectReferenceValue as AtlasImage;
                if (toBeDeletedGraphic != null) {
                    Object.DestroyImmediate(toBeDeletedGraphic.gameObject);
                }
                ReorderableList.defaultBehaviours.DoRemoveButton(l);

                RefreshGraphicComponentList(l);
                // select the last row so that "-" button is enabled
                l.index = l.serializedProperty.arraySize - 1;
            };
            GraphicComponents.onAddCallback = (ReorderableList l) => {
                int index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;

                GameObject newGameObject = new GameObject();
                AtlasImage atlasImage = newGameObject.AddComponent<AtlasImage>();
                atlasImage.SetControllerByMonoBehaviour();
                newGameObject.transform.SetParent((target as AbstractAtlasText).transform, false);
                newGameObject.name = "img_graphic";

                SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.objectReferenceValue = newGameObject;
            };
        }

        private void InitializeSpriteInfoList() {
            SpritesInfo = new ReorderableList(serializedObject,
                serializedObject.FindProperty("CharacterSprites"),
                true, true, true, true);
            SpritesInfo.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                SerializedProperty characterSpriteProperty = SpritesInfo.serializedProperty.GetArrayElementAtIndex(index);
                SerializedProperty textCharacterProperty = TextCharactersProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight), characterSpriteProperty, NoContent);
                EditorGUI.PropertyField(new Rect(rect.x + rect.width - 50 - 10, rect.y, 50, EditorGUIUtility.singleLineHeight), textCharacterProperty, NoContent);
            };
            SpritesInfo.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Sprites Info");
            SpritesInfo.onCanRemoveCallback = (ReorderableList l) => l.count > 0;
            SpritesInfo.onRemoveCallback = (ReorderableList l) => ReorderableList.defaultBehaviours.DoRemoveButton(l);
            SpritesInfo.onAddCallback = (ReorderableList l) => {
                int index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                TextCharactersProperty.arraySize++;
                l.index = index;
            };
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.PropertyField(DifferentAtlasSpritesProperty);

            EditorGUILayout.Separator();
            SpritesInfo.DoLayoutList();
            EditorGUILayout.Separator();

            RefreshGraphicComponentList(GraphicComponents);
            GraphicComponents.index = GraphicComponents.serializedProperty.arraySize - 1;
            GraphicComponents.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}