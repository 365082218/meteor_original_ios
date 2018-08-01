using UnityEditor;
using UnityEngine;

namespace Outfit7.UI {
    public static class UiTools {
        [MenuItem("Outfit7/uGUI/Anchors to Corners %[", false, 49)]
        private static void AnchorsToCorners() {
            RectTransform t = Selection.activeTransform as RectTransform;
            RectTransform pt = Selection.activeTransform.parent as RectTransform;
            
            if (t == null || pt == null)
                return;
            
            Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                                        t.anchorMin.y + t.offsetMin.y / pt.rect.height);
            Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                                        t.anchorMax.y + t.offsetMax.y / pt.rect.height);
            
            t.anchorMin = newAnchorsMin;
            t.anchorMax = newAnchorsMax;
            t.offsetMin = t.offsetMax = new Vector2(0, 0);
        }

        [MenuItem("Outfit7/uGUI/Corners to Anchors %]", false, 50)]
        private static void CornersToAnchors() {
            RectTransform t = Selection.activeTransform as RectTransform;
            
            if (t == null)
                return;
            
            t.offsetMin = t.offsetMax = new Vector2(0, 0);
        }

        [MenuItem("GameObject/O7 UI/Panel", false, 0)]
        private static void CreatePanel() {
            RectTransform t = Selection.activeTransform as RectTransform;

            GameObject go = new GameObject();
            go.transform.SetParent(t, false);

            RectTransform newRt = go.AddComponent<RectTransform>();
            newRt.anchorMin = Vector2.zero;
            newRt.anchorMax = Vector2.one;
            newRt.offsetMin = Vector2.zero;
            newRt.offsetMax = Vector2.zero;

            go.name = "pnl_";

            Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/O7 UI/Touch Zone", false, 1)]
        private static void CreatetouchZone() {
            RectTransform t = Selection.activeTransform as RectTransform;

            GameObject go = new GameObject();
            go.transform.SetParent(t, false);

            RectTransform newRt = go.AddComponent<RectTransform>();
            newRt.anchorMin = Vector2.zero;
            newRt.anchorMax = Vector2.one;
            newRt.offsetMin = Vector2.zero;
            newRt.offsetMax = Vector2.zero;

            go.AddComponent<TouchRectTransform>();

            go.name = "pnl_touchZone";

            Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/O7 UI/Canvas ...", false, 2)]
        private static void CreateCanvas() {
            UiMakerEditorWindow.ShowUIMaker<CanvasUiMakerComponent>();
        }

        [MenuItem("GameObject/O7 UI/Atlas Image ...", false, 3)]
        private static void CreateAtlasImage() {
            UiMakerEditorWindow.ShowUIMaker<ImageUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<ImageUiMakerComponent>().PreSelectAtlasImage();
        }

        [MenuItem("GameObject/O7 UI/Raw Image ...", false, 4)]
        private static void CreateRawImage() {
            UiMakerEditorWindow.ShowUIMaker<ImageUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<ImageUiMakerComponent>().PreSelectRawImage();
        }

        [MenuItem("GameObject/O7 UI/Profile Image ...", false, 5)]
        private static void CreateProfileImage() {
            UiMakerEditorWindow.ShowUIMaker<ImageUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<ImageUiMakerComponent>().PreSelectImageLoader(false);
        }

        [MenuItem("GameObject/O7 UI/Profile Image With Frame ...", false, 6)]
        private static void CreateProfileImageWithFrame() {
            UiMakerEditorWindow.ShowUIMaker<ImageUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<ImageUiMakerComponent>().PreSelectImageLoader(true);
        }

        [MenuItem("GameObject/O7 UI/Text/Unlocalized Text ...", false, 7)]
        private static void CreateText() {
            UiMakerEditorWindow.ShowUIMaker<TextUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<TextUiMakerComponent>().SetUnlocalizedText(false);
        }

        [MenuItem("GameObject/O7 UI/Text/Localized Static Text ...", false, 9)]
        private static void CreateLocalizedStaticText() {
            UiMakerEditorWindow.ShowUIMaker<TextUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<TextUiMakerComponent>().SetStaticLocalizedText(false);
        }

        [MenuItem("GameObject/O7 UI/Text/Localized Dynamic Text ...", false, 10)]
        private static void CreateLocalizedDynamicText() {
            UiMakerEditorWindow.ShowUIMaker<TextUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<TextUiMakerComponent>().SetDynamicLocalizedText(false);
        }

        [MenuItem("GameObject/O7 UI/RichText/Unlocalized Text ...", false, 7)]
        private static void CreateRichText() {
            UiMakerEditorWindow.ShowUIMaker<TextUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<TextUiMakerComponent>().SetUnlocalizedText(true);
        }

        [MenuItem("GameObject/O7 UI/RichText/Localized Static Text ...", false, 9)]
        private static void CreateLocalizedStaticRichText() {
            UiMakerEditorWindow.ShowUIMaker<TextUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<TextUiMakerComponent>().SetStaticLocalizedText(true);
        }

        [MenuItem("GameObject/O7 UI/RichText/Localized Dynamic Text ...", false, 10)]
        private static void CreateLocalizedDynamicRichText() {
            UiMakerEditorWindow.ShowUIMaker<TextUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<TextUiMakerComponent>().SetDynamicLocalizedText(true);
        }

        [MenuItem("GameObject/O7 UI/GameAction Button ...", false, 16)]
        private static void CreateGameActionButton() {
            UiMakerEditorWindow.ShowUIMaker<ButtonUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<ButtonUiMakerComponent>().SetGameActionButton();
        }

        [MenuItem("GameObject/O7 UI/DialogAction Button ...", false, 17)]
        private static void CreateDialogActionButton() {
            UiMakerEditorWindow.ShowUIMaker<ButtonUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<ButtonUiMakerComponent>().SetDialogActionButton();
        }

        [MenuItem("GameObject/O7 UI/ScrollView ...", false, 17)]
        private static void CreateScrollView() {
            UiMakerEditorWindow.ShowUIMaker<ScrollViewUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<ScrollViewUiMakerComponent>().SetScrollViewType(false);
        }

        [MenuItem("GameObject/O7 UI/InfiniteScrollView ...", false, 17)]
        private static void CreateInfiniteScrollView() {
            UiMakerEditorWindow.ShowUIMaker<ScrollViewUiMakerComponent>();
            UiMakerEditorWindow.GetComponent<ScrollViewUiMakerComponent>().SetScrollViewType(true);
        }

        [MenuItem("GameObject/O7 UI/ProgressBar ...", false, 17)]
        private static void CreateProgressBar() {
            UiMakerEditorWindow.ShowUIMaker<ProgressBarUiMakerComponent>();
        }
    }
}