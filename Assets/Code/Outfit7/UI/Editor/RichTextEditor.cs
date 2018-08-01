#if !UNITY_5_1 && !UNITY_5_2
using UnityEditor;
using UnityEngine.UI;
using System.Linq;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;
using Outfit7.Util;

namespace Outfit7.UI {
    [CustomEditor(typeof(RichText), true)]
    [CanEditMultipleObjects]
    public class RichTextEditor : GraphicEditor {
        private SerializedProperty m_Text;
        private SerializedProperty m_FontData;
        private SerializedProperty CanvasAtlasProperty;
        private SerializedProperty ParseImages;
        private ReorderableList GraphicComponents;
        private ReorderableList SpritesInfo;

        private List<Sprite> Sprites = new List<Sprite>();
        private List<string> SpriteNames = new List<string>();

        protected override void OnEnable() {
            base.OnEnable();
            m_Text = serializedObject.FindProperty("UnformatedText");
            m_FontData = serializedObject.FindProperty("m_FontData");

            InitializeSpriteInfoList();
            InitializeGraphicComponentList();
            CanvasAtlasProperty = serializedObject.FindProperty("CanvasAtlas");
            ParseImages = serializedObject.FindProperty("ParseImages");
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
                serializedObject.FindProperty("ImageComponents"), 
                false, true, true, true);
            GraphicComponents.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = GraphicComponents.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                GUI.enabled = false;
                EditorGUI.PropertyField(new Rect(rect.x + ReorderableList.Defaults.dragHandleWidth - ReorderableList.Defaults.padding, rect.y, rect.width - ReorderableList.Defaults.dragHandleWidth, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                GUI.enabled = true;
            };
            GraphicComponents.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "GraphicComponent");
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
                atlasImage.enabled = false;
                newGameObject.transform.SetParent((target as RichText).transform, false);
                newGameObject.name = "img_graphic";

                SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.objectReferenceValue = newGameObject;
            };
        }

        private void InitializeSpriteInfoList() {
            SpritesInfo = new ReorderableList(serializedObject, 
                serializedObject.FindProperty("SpritesInfo"), 
                true, true, true, true);
            SpritesInfo.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                SerializedProperty element = SpritesInfo.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                Sprite sprite = element.FindPropertyRelative("Sprite").objectReferenceValue as Sprite;
                int spriteIndex = Sprites.IndexOf(sprite);

                if (spriteIndex < 0) {
                    if (sprite != null) {
                        O7Log.Error(string.Format("Atlas {0} with containing sprite {1} was removed from CanvasAtlas", sprite.texture, sprite.name));
                    } else {
                        O7Log.Error("A texture with the previously set sprite was removed");
                    }
                    element.FindPropertyRelative("Sprite").objectReferenceValue = null;
                } else {
                    EditorGUI.BeginChangeCheck();
                    spriteIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width - 180, EditorGUIUtility.singleLineHeight), spriteIndex, SpriteNames.ToArray());
                    if (EditorGUI.EndChangeCheck()) {
                        element.FindPropertyRelative("Sprite").objectReferenceValue = Sprites[spriteIndex];
                    }
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width - 40 - 40 - 80 - 10, rect.y, 90, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Offset"), GUIContent.none);
                    EditorGUI.LabelField(new Rect(rect.x + rect.width - 40 - 35, rect.y, 30, EditorGUIUtility.singleLineHeight), "Scale");
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width - 30 - 10, rect.y, 30, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Scale"), GUIContent.none);
                }
            };
            SpritesInfo.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "SpritesInfo");
            SpritesInfo.onCanRemoveCallback = (ReorderableList l) => l.count > 0;
            SpritesInfo.onRemoveCallback = (ReorderableList l) => ReorderableList.defaultBehaviours.DoRemoveButton(l);
            SpritesInfo.onAddCallback = (ReorderableList l) => {
                int index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
                SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("Sprite").objectReferenceValue = Sprites[0];
                element.FindPropertyRelative("Offset").vector2Value = Vector2.zero;
                element.FindPropertyRelative("Scale").floatValue = 1f;
            };
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Text);
            EditorGUILayout.PropertyField(m_FontData);
            AppearanceControlsGUI();
            RaycastControlsGUI();

            EditorGUILayout.PropertyField(ParseImages);

            if (ParseImages.boolValue) {
                EditorGUILayout.Separator();

                if (RefreshAtlasData()) {
                    SpritesInfo.DoLayoutList();
                    EditorGUILayout.Separator();
                }

                RefreshGraphicComponentList(GraphicComponents);
                GraphicComponents.index = GraphicComponents.serializedProperty.arraySize - 1;
                GraphicComponents.DoLayoutList();

                EditorGUILayout.Separator();

                GUI.enabled = false;
                EditorGUILayout.PropertyField(CanvasAtlasProperty);
                GUI.enabled = true;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private bool RefreshAtlasData() {
            CanvasAtlas canvasAtlas = CanvasAtlasProperty.objectReferenceValue as CanvasAtlas;
            if (canvasAtlas == null) {
                EditorGUILayout.HelpBox("CanvasAtlas on parent not found", MessageType.Error);
                if (PrefabUtility.GetPrefabParent(target) == null && PrefabUtility.GetPrefabObject(target) != null) {
                    EditorGUILayout.HelpBox("Instantiate prefab to assign values - or drag it into the scene", MessageType.Info);
                }
                return false;
            }

            Sprites.Clear();
            SpriteNames.Clear();
            canvasAtlas.GetCanvasAtlasListsEditor(ref Sprites, ref SpriteNames);

            return true;
        }
    }

    /// <summary>
    /// Editor class used to edit UI Graphics.
    /// </summary>

    [CustomEditor(typeof(MaskableGraphic), false)]
    [CanEditMultipleObjects]
    public class GraphicEditor : UnityEditor.Editor {
        protected SerializedProperty m_Script;
        protected SerializedProperty m_Color;
        protected SerializedProperty m_Material;
        protected SerializedProperty m_RaycastTarget;

        private GUIContent m_CorrectButtonContent;
        protected AnimBool m_ShowNativeSize;

        protected virtual void OnDisable() {
            Tools.hidden = false;
            m_ShowNativeSize.valueChanged.RemoveListener(Repaint);
        }

        protected virtual void OnEnable() {
            m_CorrectButtonContent = new GUIContent("Set Native Size", "Sets the size to match the content.");

            m_Script = serializedObject.FindProperty("m_Script");
            m_Color = serializedObject.FindProperty("m_Color");
            m_Material = serializedObject.FindProperty("m_Material");
            m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");

            m_ShowNativeSize = new AnimBool(false);
            m_ShowNativeSize.valueChanged.AddListener(Repaint);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Script);
            AppearanceControlsGUI();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }

        protected void SetShowNativeSize(bool show, bool instant) {
            if (instant)
                m_ShowNativeSize.value = show;
            else
                m_ShowNativeSize.target = show;
        }

        protected void NativeSizeButtonGUI() {
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded)) {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    if (GUILayout.Button(m_CorrectButtonContent, EditorStyles.miniButton)) {
                        foreach (Graphic graphic in targets.Select(obj => obj as Graphic)) {
                            Undo.RecordObject(graphic.rectTransform, "Set Native Size");
                            graphic.SetNativeSize();
                            EditorUtility.SetDirty(graphic);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFadeGroup();
        }

        protected void AppearanceControlsGUI() {
            EditorGUILayout.PropertyField(m_Color);
            EditorGUILayout.PropertyField(m_Material);
        }

        protected void RaycastControlsGUI() {
            EditorGUILayout.PropertyField(m_RaycastTarget);
        }
    }

}
#endif