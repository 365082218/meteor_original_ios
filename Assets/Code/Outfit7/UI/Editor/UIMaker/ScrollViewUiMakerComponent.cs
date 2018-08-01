using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace Outfit7.UI {
    public class ScrollViewUiMakerComponent : UiMakerComponent {

        private enum Orientation {
            Vertical,
            Horizontal
        }

        public override string TypeName { get { return "ScrollView"; } }

        public override bool IsSimpleType { get { return false; } }

        public override string NamePrefix { get { return InfiniteScrollView ? "isc" : "scr"; } }

        private bool InfiniteScrollView = false;

        private Orientation OrientationValue = Orientation.Vertical;
        List<string> OrientationTypeStrings = new List<string>((int) Orientation.Horizontal);

        //        private Vector2 ContentPivotPoint = new Vector2(0.5f, 0.5f);

        private TextAnchor ChildAlignment = TextAnchor.UpperCenter;

        private bool Snap = false;

        // ScrollViewController
        private bool FullScreen = false;
        // InfiniteScrollViewController
        private int CellControllers = 3;

        private bool CellFoldOut = true;

        private ScrollViewUiMakerData Data;

        public void SetScrollViewType(bool infinite) {
            InfiniteScrollView = infinite;
        }

        public override void Init() {
            Data = ProjectUiMakerComponent.GetData<ScrollViewUiMakerData>();

            OrientationTypeStrings.Clear();
            for (int i = 0; i <= (int) Orientation.Horizontal; i++) {
                OrientationTypeStrings.Add(((Orientation) i).ToString());
            }
        }

        public override void OnGui() {
            base.OnGui();

            EditorGUILayout.Separator();
            EditorGUI.BeginChangeCheck();
            InfiniteScrollView = EditorGUILayout.Toggle("Infinite Scroll View", InfiniteScrollView);
            if (EditorGUI.EndChangeCheck()) {
                if (InfiniteScrollView) {
                    ChildAlignment = TextAnchor.MiddleCenter;
                } else {
                    if (OrientationValue == Orientation.Horizontal) {
                        ChildAlignment = TextAnchor.MiddleLeft;
                    } else { // vertical
                        ChildAlignment = TextAnchor.UpperCenter;
                    }
                }
            }
            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            OrientationValue = (Orientation) GUILayout.SelectionGrid((int) OrientationValue, OrientationTypeStrings.ToArray(), 2, EditorStyles.miniButton, GUILayout.MinHeight(30f));
            if (EditorGUI.EndChangeCheck()) {
                if (InfiniteScrollView) {
                    ChildAlignment = TextAnchor.MiddleCenter;
                } else {
                    if (OrientationValue == Orientation.Horizontal) {
                        ChildAlignment = TextAnchor.MiddleLeft;
                    } else { // vertical
                        ChildAlignment = TextAnchor.UpperCenter;
                    }
                }
            }

            if (InfiniteScrollView) {
                EditorGUILayout.HelpBox("Infinite ScrollView has the only limitation: all cells must be of the same size.\nAlso the containers are prepared in advance", MessageType.Info);

                CellControllers = EditorGUILayout.IntField("Number of containers", CellControllers);
            } else {
                FullScreen = EditorGUILayout.Toggle("Resize Cell To Full Screen", FullScreen);
            }

            Snap = EditorGUILayout.Toggle("Page Snap", Snap);

//            ContentPivotPoint = EditorGUILayout.Vector2Field("Content Pivot Point", ContentPivotPoint);
            ChildAlignment = (TextAnchor) EditorGUILayout.EnumPopup("Child Alignment", ChildAlignment);

            OnNewGameObjectGui();

            OnCellGui();
            EditorGUILayout.Separator();
        }

        protected override RectTransform OnCreateExecute() {
            RectTransform rectTransform = base.OnCreateExecute();

            GameObject viewPort = new GameObject("pnl_viewport");
            RectTransform viewPortRectTransform = viewPort.AddComponent<RectTransform>();
            viewPortRectTransform.SetParent(rectTransform, false);
            SetToStretch(viewPortRectTransform);

            GameObject content = new GameObject("pnl_content");
            RectTransform contentRectTransform = content.AddComponent<RectTransform>();
            contentRectTransform.SetParent(viewPortRectTransform, false);
            SetToStretch(contentRectTransform);

            // scrollrect stuff

            ScrollRect scrollRect;
            AbstractScrollViewController abstractScrollViewController;
            if (InfiniteScrollView) {
                scrollRect = rectTransform.gameObject.AddComponent<InfiniteScrollRect>();
                InfiniteScrollViewController infiniteScrollViewController = rectTransform.gameObject.AddComponent<InfiniteScrollViewController>();
                abstractScrollViewController = infiniteScrollViewController;
                for (int i = 0; i < CellControllers; i++) {
                    GameObject cellContainer = new GameObject(string.Format("Container{0}", i + 1));
                    RectTransform containerRectTransform = cellContainer.AddComponent<RectTransform>();
                    containerRectTransform.SetParent(contentRectTransform, false);
                    LayoutElement layoutElement = cellContainer.AddComponent<LayoutElement>();
                    infiniteScrollViewController.AddContainer(layoutElement);
                }
            } else {
                scrollRect = rectTransform.gameObject.AddComponent<ScrollRect>();
                abstractScrollViewController = rectTransform.gameObject.AddComponent<ScrollViewController>();

                ScrollViewController scrollViewController = abstractScrollViewController as ScrollViewController;
                scrollViewController.SetFullScreen(FullScreen);

                // TODO: Add scrollbar stuff
                scrollViewController.SetScrollBar(null);
            }

            abstractScrollViewController.SetScrollRectEditor(scrollRect);
            abstractScrollViewController.SetContentEditor(contentRectTransform);
            abstractScrollViewController.ToggleSnap(Snap);
            abstractScrollViewController.SetScrollViewRectTransformEditor(rectTransform);

            // TODO: update all scrollrect assets in project
            #if UNITY_5_2
            scrollRect.viewport = viewPortRectTransform;
            #endif

            scrollRect.content = contentRectTransform;

            ContentSizeFitter contentSizeFitter = contentRectTransform.gameObject.AddComponent<ContentSizeFitter>();
            HorizontalOrVerticalLayoutGroup layoutGroup;

            if (OrientationValue == Orientation.Vertical) {
                scrollRect.vertical = true;
                scrollRect.horizontal = false;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                layoutGroup = contentRectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
            } else {
                scrollRect.vertical = false;
                scrollRect.horizontal = true;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                layoutGroup = contentRectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = true;
            }

            layoutGroup.childAlignment = ChildAlignment;
            switch (ChildAlignment) {
                case TextAnchor.UpperLeft:
                    contentRectTransform.pivot = new Vector2(0f, 1f);
                    break;
                case TextAnchor.UpperCenter:
                    contentRectTransform.pivot = new Vector2(0.5f, 1f);
                    break;
                case TextAnchor.UpperRight:
                    contentRectTransform.pivot = new Vector2(1f, 1f);
                    break;
                case TextAnchor.MiddleLeft:
                    contentRectTransform.pivot = new Vector2(0f, 0.5f);
                    break;
                case TextAnchor.MiddleCenter:
                    contentRectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case TextAnchor.MiddleRight:
                    contentRectTransform.pivot = new Vector2(1f, 0.5f);
                    break;
                case TextAnchor.LowerLeft:
                    contentRectTransform.pivot = new Vector2(0f, 0f);
                    break;
                case TextAnchor.LowerCenter:
                    contentRectTransform.pivot = new Vector2(0.5f, 0f);
                    break;
                case TextAnchor.LowerRight:
                    contentRectTransform.pivot = new Vector2(1f, 0f);
                    break;
            }

            SetToStretch(rectTransform);

            return rectTransform;
        }

        private void OnNewGameObjectGui() {
            SetCommonCreatePanel("Create ScrollView");
        }

        private void OnCellGui() {
            if (UiMakerEditorWindow.Compiling) {
                EditorGUILayout.HelpBox("Compiling", MessageType.Error);
                return;
            }

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<AbstractScrollViewController>() != null) {
                AbstractScrollViewController scrollViewController = Selection.activeGameObject.GetComponent<AbstractScrollViewController>();

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                CellFoldOut = EditorGUILayout.Foldout(CellFoldOut, string.Format("Cell Definition ({0})", scrollViewController.CellsToCreate.Count));
                if (CellFoldOut) {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < scrollViewController.CellsToCreate.Count; i++) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Name", GUILayout.Width(60f));
                        scrollViewController.CellsToCreate[i].SetName(GUILayout.TextField(scrollViewController.CellsToCreate[i].Name));
                        if (scrollViewController is ScrollViewController) {
                            EditorGUILayout.LabelField(scrollViewController.GetScrollRect().horizontal ? "Width" : "Height", GUILayout.Width(55f));
                            scrollViewController.CellsToCreate[i].SetMainAxisLength(EditorGUILayout.FloatField(scrollViewController.CellsToCreate[i].MainAxisLength, GUILayout.Width(50f)));
                        }
                        EditorGUILayout.EndHorizontal();
                    }
            
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(18f);
                    if (GUILayout.Button("+", GUILayout.Height(33f))) {
                        scrollViewController.CellsToCreate.Add(new AbstractScrollViewController.CellToCreate());
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(18f);
                    if (scrollViewController.CellsToCreate.Count > 0 && GUILayout.Button("-", GUILayout.Height(33f))) {
                        scrollViewController.CellsToCreate.RemoveAt(scrollViewController.CellsToCreate.Count - 1);
                    }
                    EditorGUILayout.EndHorizontal();


                    if (scrollViewController.CellsToCreate.Count > 0 && !string.IsNullOrEmpty(scrollViewController.CellsToCreate[0].Name)) {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(18f);
                        if (GUILayout.Button("(Re)create Cells", GUILayout.Height(33f))) {
                            OnCellCreateExecute(scrollViewController);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(18f);
                        if (GUILayout.Button("Connect Cells To Selected ScrollView", GUILayout.Height(33f))) {
                            CreateAndLinkCellPrefabs(scrollViewController);
                        }
                        EditorGUILayout.EndHorizontal();
                    } else {
                        EditorGUILayout.HelpBox("Create cells for the selected ScrollView", MessageType.Info);    
                    }
                    EditorGUI.indentLevel--;
                }
            } else {
                EditorGUILayout.HelpBox("Select a ScrollView to create or/and connect cells to it", MessageType.Info);
            }
        }

        private void OnCellCreateExecute(AbstractScrollViewController scrollViewController) {
            // TODO: code for reusing a cell list instead of creating a new one

            for (int i = 0; i < scrollViewController.CellsToCreate.Count; i++) {
                CreateCellItem(scrollViewController.CellsToCreate[i]);
            }

            AssetDatabase.Refresh();
        }

        private void CreateCellItem(AbstractScrollViewController.CellToCreate cell) {
            string cellName = RemoveSuffix(cell.Name, "Cell");

            cellName = SetPascalCase(cellName);

            string cellDataName = cellName + "CellData";
            string cellControllerName = cellName + "CellController";

            CreateFromTemplate(Data.CellDataFolderPath, Data.TemplateCellDataFilePath, cellDataName, "txt", "cs", new Dictionary<string, string> {
                { "CellDataTemplate", cellDataName }
            });

            CreateFromTemplate(Data.CellControllerFolderPath, Data.TemplateCellControllerFilePath, cellControllerName, "txt", "cs", new Dictionary<string, string> {
                { "CellDataTemplate", cellDataName },
                { "CellControllerTemplate", cellControllerName }
            });
        }

        private void CreateAndLinkCellPrefabs(AbstractScrollViewController scrollViewController) {

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            for (int i = 0; i < scrollViewController.CellsToCreate.Count; i++) {

                AbstractScrollViewController.CellToCreate cellToCreate = scrollViewController.CellsToCreate[i];

                string cellName = SetPascalCase(cellToCreate.Name);
                string cellControllerName = cellName + "CellController";
                
                Type cellControllerType = GetTypeInNamespace("Outfit7.UI.Cell", cellControllerName);
                
                GameObject cellGameObject = new GameObject(cellName + "Cell");
                cellGameObject.AddComponent(cellControllerType);
                if (scrollViewController is ScrollViewController) {
                    LayoutElement layoutElement = cellGameObject.AddComponent<LayoutElement>();
                    if (scrollViewController.GetScrollRect().vertical) {
                        layoutElement.preferredHeight = cellToCreate.MainAxisLength;
                        layoutElement.minHeight = cellToCreate.MainAxisLength;
                    } else if (scrollViewController.GetScrollRect().horizontal) {
                        layoutElement.preferredWidth = cellToCreate.MainAxisLength;
                        layoutElement.minWidth = cellToCreate.MainAxisLength;
                    }
                }
                
                string cellPrefabFolderPath = string.Format("{0}/{1}", Application.dataPath, Data.CellPrefabFolderPath);
                if (!System.IO.Directory.Exists(cellPrefabFolderPath)) {
                    if (EditorUtility.DisplayDialog("Are you sure?", 
                            "Directory " + Data.CellPrefabFolderPath + " for prefab doesn't exits. Do you want to create it?", 
                            "Yes", 
                            "No")) {
                        System.IO.Directory.CreateDirectory(cellPrefabFolderPath);
                    }
                }

                string prefabCreatePath = string.Format("Assets/{0}/{1}Cell.prefab", Data.CellPrefabFolderPath, cellName);
                string prefabPath = string.Format("{0}/{1}Cell.prefab", cellPrefabFolderPath, cellName);
                CreatePrefab(cellName + "Cell", prefabPath, prefabCreatePath, cellGameObject,
                    obj => scrollViewController.RemoveCellControllerPrefabList(obj.GetComponent<AbstractCellController>()),
                    obj => {
                        AbstractCellController prefabCellController = obj.GetComponent<AbstractCellController>();
                        scrollViewController.AddCellControllerPrefabList(prefabCellController);
                    }
                );
                UnityEngine.Object.DestroyImmediate(cellGameObject);
            }
        }
    }
}
