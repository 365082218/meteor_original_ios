using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Outfit7.Util;

namespace Outfit7.UI {
    [RequireComponent(typeof(ScrollRect))]
    [AddComponentMenu("UI/ScrollViewController", 56)]
    public partial class ScrollViewController : AbstractScrollViewController, IPointerDownHandler, IPointerUpHandler {

        public int BinarySearch(List<RectTransform> list, float value, int cornerIndex, int axisIndex, bool isVertical) {

            Vector3[] corners = new Vector3[4];

            float itemValue;

            int iMin = 0;

            int iMax = list.Count - 1;
            int iCmp = 0;
            while (iMin <= iMax) {
                int iMid = iMin + ((iMax - iMin) / 2);
                RectTransform item = list[iMid];

                item.GetWorldCorners(corners);
                itemValue = corners[cornerIndex][axisIndex];

                iCmp = isVertical ? value.CompareTo(itemValue) : itemValue.CompareTo(value);

                if (iCmp == 0)
                    return iMid;
                else if (iCmp > 0)
                    iMax = iMid - 1;
                else
                    iMin = iMid + 1;
            }

            return ~iMin;
        }

        [Header("ScrollViewController")]
        [SerializeField] protected LayoutElement CustomContainerPrefab;
        [SerializeField] protected bool FullScreen;

        [Header("Scrollbar")]
        [SerializeField] protected Scrollbar Scrollbar;
        [SerializeField] protected Image ScrollbarHandleImage;
        [SerializeField] protected CanvasGroup ScrollbarCanvasGroup;
        [SerializeField] protected bool ScrollbarFadeAway = true;
        protected List<LayoutElement> ContainerList = new List<LayoutElement>();
        protected List<LayoutElement> ContainerPool = new List<LayoutElement>();
        protected List<RectTransform> ContainerRectTransformList = new List<RectTransform>();
        protected List<AbstractCellController> CellsToDeactivate = new List<AbstractCellController>();
        protected List<LayoutElement> ContainersToDeactivate = new List<LayoutElement>();
        protected List<AbstractCellController> SpawnedCellControllers = new List<AbstractCellController>();
        protected float CustomOtherAxis = float.MinValue;

        private bool ShouldUpdateView = false;
        private int LastVisibleMinIndex = -1;
        private int LastVisibleMaxIndex = -1;
        private bool CountChanged = false;

        public Scrollbar GetScrollBar() {
            return Scrollbar;
        }

#if UNITY_EDITOR
        public void SetScrollBar(Scrollbar scrollbar) {
            Scrollbar = scrollbar;
        }

        public void SetFullScreen(bool fullScreen) {
            FullScreen = fullScreen;
        }
#endif

        // For setting e.g. the size of the prefab to instantiate
        public LayoutElement GetContainerPrefab() {
            return CustomContainerPrefab;
        }

        public override void RefreshView() {
            if (FullScreen) {
                if (GetScrollRect().horizontal) {
#if UNITY_5_2
                    float width = GetScrollRect().viewport.rect.width;
#else
                    float width = GetScrollRect().GetComponent<RectTransform>().rect.width;
#endif
                    for (int i = 0; i < ContainerList.Count; i++) {
                        ContainerList[i].minWidth = width;
                        ContainerList[i].preferredWidth = width;
                        if (CustomOtherAxis != float.MinValue) {
                            ContainerList[i].minHeight = CustomOtherAxis;
                            ContainerList[i].preferredHeight = CustomOtherAxis;
                        }
                    }
                } else if (GetScrollRect().vertical) {
#if UNITY_5_2
                    float height = GetScrollRect().viewport.rect.height;
#else
                    float height = GetScrollRect().GetComponent<RectTransform>().rect.height;
#endif
                    for (int i = 0; i < ContainerList.Count; i++) {
                        ContainerList[i].minHeight = height;
                        ContainerList[i].preferredHeight = height;
                        if (CustomOtherAxis != float.MinValue) {
                            ContainerList[i].minWidth = CustomOtherAxis;
                            ContainerList[i].preferredWidth = CustomOtherAxis;
                        }
                    }
                }
            }
            OnScrollValueChanged(Vector2.zero);
            ShouldUpdateView = true;
            RefreshSnapParameters();
        }

        public override void ForceUpdateCells() {
            for (int a = 0; a < SpawnedCellControllers.Count; a++) {
                AbstractCellController cc = SpawnedCellControllers[a];
                cc.UpdateCell();
            }
        }

        public void Move(int index, int cellDataIdx) {
            Move(index, CellDataList[cellDataIdx]);
        }

        public void Move(int index, AbstractCellData item) {
            throw new NotImplementedException();

//            if (!CellDataList.Contains(item))
//                return;
//
//            if (CellDataList.Remove(item)) {
//                CellDataList.Insert(index, item);
//                SetSiblingIndex(index);
//            }
//            for (int i = index + 1; i < CellDataList.Count; i++) {
//                SetSiblingIndex(i);
//            }
        }

        public RectTransform GetContainer(int idx) {
            if (ContainerRectTransformList.Count <= idx || idx < 0) {
                return null;
            }

            return ContainerRectTransformList[idx];
        }

        protected LayoutElement GetFirstInactiveReusableContainer() {
            LayoutElement le = null;
            if (ContainerPool.Count > 0) {
                le = ContainerPool[0];
                ContainerPool.RemoveAt(0);
                le.enabled = true;
                if (ContainersToDeactivate.Contains(le)) {
                    ContainersToDeactivate.Remove(le);
                }
                le.gameObject.SetActive(true);
                return le;
            }

            if (CustomContainerPrefab != null) {
                return Instantiate(CustomContainerPrefab);
            }

            GameObject go = new GameObject();
            go.AddComponent<RectTransform>();
            le = go.AddComponent<LayoutElement>();
            return le;
        }

        protected override AbstractCellController GetFirstInactiveReusableCell(Type type) {
            if (ReusableCellPool.ContainsKey(type)) {
                List<AbstractCellController> reusableControllerList = ReusableCellPool[type];
                if (reusableControllerList.Count > 0) {
                    AbstractCellController cellController = reusableControllerList[0];
                    ReusableCellPool[cellController.GetType()].Remove(cellController);
                    cellController.enabled = true;
                    if (!NoDeactivations) {
                        if (CellsToDeactivate.Contains(cellController)) {
                            CellsToDeactivate.Remove(cellController);
                        }
                        cellController.gameObject.SetActive(true);
                    }
                    return cellController;
                }
            }

            return null;
        }

        protected RectTransform SetupExistingContainer(LayoutElement currentContainer, Type type) {
            return SetupContainer(currentContainer, type);
        }

        protected RectTransform SetupNewContainer(Type type) {
            return SetupContainer(null, type);
        }


        protected RectTransform SetupContainer(LayoutElement currentContainer, Type type) {
            RectTransform prefabRectTransform = GetCellRectTransformPrefab(type);
            LayoutElement le = currentContainer ?? GetFirstInactiveReusableContainer();
            RectTransform rt = le.transform as RectTransform;

            rt.anchorMin = prefabRectTransform.anchorMin;
            rt.anchorMax = prefabRectTransform.anchorMax;
            rt.pivot = prefabRectTransform.pivot;

            LayoutElement prefabLayoutElement = prefabRectTransform.GetComponent<LayoutElement>();
            le.minWidth = prefabLayoutElement.minWidth;
            le.minHeight = prefabLayoutElement.minHeight;
            le.preferredWidth = prefabLayoutElement.preferredWidth;
            le.preferredHeight = prefabLayoutElement.preferredHeight;

            #if UNITY_EDITOR
            rt.name = string.Format("Container{0}", ContainerList.Count);
            #endif

            if (currentContainer == null) {
                ContainerList.Add(le);
                ContainerRectTransformList.Add(le.transform as RectTransform);
            }

            return rt;
        }

        protected void RemoveContainer(int index) {
            LayoutElement le = ContainerList[index];
            ContainerList.RemoveAt(index);
            ContainerRectTransformList.RemoveAt(index);
            le.enabled = false;
            ContainerPool.Add(le);
            ContainersToDeactivate.Add(le);
        }

        protected virtual void Despawn(AbstractCellData cellData) {
            if (cellData.CellControllerRef == null)
                return;

            AbstractCellController cellController = cellData.CellControllerRef;

            SpawnedCellControllers.Remove(cellController);


            if (!ReusableCellPool.ContainsKey(cellController.GetType())) {
                ReusableCellPool.Add(cellController.GetType(), new List<AbstractCellController>());
            }

            ReusableCellPool[cellController.GetType()].Insert(0, cellController);

            if (NoDeactivations) {
                cellController.transform.SetParent(null, false);
            } else {
                cellController.transform.SetParent(transform, false);
                cellController.transform.SetSiblingIndex(-1000);
                CellsToDeactivate.Add(cellController);
            }
            cellController.enabled = false;

            if (cellData.OnDespawn != null) {
                cellData.OnDespawn(cellData);
            }

            cellData.CellControllerRef = null;
        }

        protected override AbstractCellController Spawn(RectTransform parent, AbstractCellData cellData) {

            if (cellData.CellControllerRef != null) {
                return cellData.CellControllerRef;
            }

            AbstractCellController cc = GetReusableCell(cellData.CellControllerType);
            cc.transform.SetParent(parent, false);
            UpdateCell(cc, cellData);

            #if UNITY_EDITOR
            cc.name = string.Format("{0}/{1}", cellData.GetType().Name, cellData.CellControllerType.Name);
            #endif

            if (cellData.OnSpawn != null) {
                cellData.OnSpawn(cellData);
            }

            SpawnedCellControllers.Add(cc);

            return cc;
        }

        protected virtual void SetContainerIndexAndParentIt(int idx, RectTransform t, Type type) {
            t.SetParent(Content, false);
            t.SetSiblingIndex(idx);

            #if UNITY_EDITOR
            t.name = string.Format("Container{0}", idx);
            #endif
        }

        protected virtual void SetContainerIndex(int idx) {
            ContainerRectTransformList[idx].SetSiblingIndex(idx);

            #if UNITY_EDITOR
            ContainerList[idx].name = string.Format("Container{0}", idx);
            #endif
        }


        protected override void OnScrollValueChanged(Vector2 val) {
            base.OnScrollValueChanged(val);

            Canvas.ForceUpdateCanvases();

            if (ScrollRect.horizontal) {
                UpdateViewHorizontal();
            } else if (ScrollRect.vertical) {
                UpdateViewVertical();
            }

            if (!NoDeactivations) {
                for (int i = 0; i < CellsToDeactivate.Count; i++) {
                    CellsToDeactivate[i].gameObject.SetActive(false);
                }
                CellsToDeactivate.Clear();
            }

            for (int i = 0; i < ContainersToDeactivate.Count; i++) {
                ContainersToDeactivate[i].gameObject.SetActive(false);
                Transform t = ContainersToDeactivate[i].transform;
                t.SetParent(transform, false);
                t.SetSiblingIndex(-1000);
            }
            ContainersToDeactivate.Clear();
            Canvas.ForceUpdateCanvases();
        }

        protected virtual void OnScrollBarValueChanged(Vector2 val) {
            if (Count == 0 ||
                ScrollRect.vertical && Content.rect.height <= (ScrollRect.transform as RectTransform).rect.height ||
                ScrollRect.horizontal && Content.rect.width <= (ScrollRect.transform as RectTransform).rect.width)
                return;

            StopAllCoroutines();
            if (ScrollbarFadeAway == true) {
                StartCoroutine(FadeScrollbar());
            }
        }

        protected virtual System.Collections.IEnumerator FadeScrollbar() {
            float fadeSpeed = 3.33333f;
            float waitTime = 0.3f;

            if (ScrollbarHandleImage != null) {
                ScrollbarHandleImage.enabled = true;
            }

            while (ScrollbarCanvasGroup.alpha < 1f) {
                ScrollbarCanvasGroup.alpha = Mathf.MoveTowards(ScrollbarCanvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
                yield return null;
            }

            while (waitTime > 0f) {
                waitTime -= Time.deltaTime;
                yield return null;
            }

            while (ScrollbarCanvasGroup.alpha > 0f) {
                ScrollbarCanvasGroup.alpha = Mathf.MoveTowards(ScrollbarCanvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
                yield return null;
            }

            if (ScrollbarHandleImage != null) {
                ScrollbarHandleImage.enabled = false;
            }
        }

        // UIBehaviour
        protected override void OnEnable() {
            base.OnEnable();

            Canvas.willRenderCanvases += OnWillRenderCanvas;
            if (ScrollbarHandleImage != null && ScrollbarFadeAway == true) {
                ScrollbarHandleImage.enabled = false;
            }

            ScrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            if (Scrollbar != null) {
                ScrollRect.onValueChanged.AddListener(OnScrollBarValueChanged);
            }

            ShouldUpdateView = true;
        }

        protected override void OnDisable() {
            base.OnDisable();

            Canvas.willRenderCanvases -= OnWillRenderCanvas;
            ScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            if (Scrollbar != null) {
                ScrollRect.onValueChanged.RemoveListener(OnScrollBarValueChanged);
            }
            StopAnimating();
        }

        protected void ProcessViewCells(int minIndex, int maxIndex) {
            if (minIndex < 0) {
                minIndex = ~minIndex;
            }

            if (maxIndex < 0) {
                maxIndex = ~maxIndex;
            }

            if (CountChanged) {
                LastVisibleMinIndex = -1;
                LastVisibleMaxIndex = -1;
                CountChanged = false;
            }

            if (minIndex == LastVisibleMinIndex && maxIndex == LastVisibleMaxIndex) {
                return;
            }

            int spawnMinIndex, spawnMaxIndex, despawnMinIndex, despawnMaxIndex;

//            Debug.LogError(minIndex + ", " + maxIndex + " | " + LastVisibleMinIndex + ", " + LastVisibleMaxIndex);

            if (maxIndex < LastVisibleMinIndex || minIndex > LastVisibleMaxIndex || minIndex < LastVisibleMinIndex && maxIndex > LastVisibleMaxIndex || minIndex > LastVisibleMinIndex && maxIndex < LastVisibleMaxIndex) {
                // moved completely to another location
//                Debug.LogError("MovedCompletely");

                despawnMinIndex = LastVisibleMinIndex;
                despawnMaxIndex = LastVisibleMaxIndex;

                spawnMinIndex = minIndex;
                spawnMaxIndex = maxIndex;

            } else if (minIndex < LastVisibleMinIndex || maxIndex < LastVisibleMaxIndex) {
                // moving up
//                Debug.LogError("MovingUp");

                despawnMinIndex = maxIndex;
                despawnMaxIndex = LastVisibleMaxIndex;

                spawnMinIndex = minIndex;
                spawnMaxIndex = LastVisibleMinIndex;

            } else { //if (maxIndex >= LastVisibleMaxIndex || minIndex >= LastVisibleMinIndex) {
                // moving down
//                Debug.LogError("MovingDown");

                despawnMinIndex = LastVisibleMinIndex;
                despawnMaxIndex = minIndex;

                spawnMinIndex = LastVisibleMaxIndex;
                spawnMaxIndex = maxIndex;

            }

//            Debug.LogError(despawnMinIndex + ", " + despawnMaxIndex + " | " + spawnMinIndex + ", " + spawnMaxIndex);

            for (int i = despawnMinIndex; i < despawnMaxIndex && i < Count; i++) {
                Despawn(CellDataList[i]);
            }

            for (int i = spawnMinIndex; i < spawnMaxIndex; i++) {
                Spawn(ContainerRectTransformList[i], CellDataList[i]);
            }

            LastVisibleMinIndex = minIndex;
            LastVisibleMaxIndex = maxIndex;
        }

        protected void UpdateViewHorizontal() {
            Vector3[] corners = new Vector3[4];
            GetScrollViewRectTransform().GetWorldCorners(corners);

            int minIndex = BinarySearch(ContainerRectTransformList, corners[0].x, 2, 0, false);
            int maxIndex = BinarySearch(ContainerRectTransformList, corners[2].x, 0, 0, false);

            ProcessViewCells(minIndex, maxIndex);
        }

        protected void UpdateViewVertical() {

            Vector3[] corners = new Vector3[4];
            GetScrollViewRectTransform().GetWorldCorners(corners);

            int minIndex = BinarySearch(ContainerRectTransformList, corners[2].y, 0, 1, true);
            int maxIndex = BinarySearch(ContainerRectTransformList, corners[0].y, 2, 1, true);

            ProcessViewCells(minIndex, maxIndex);
        }

        protected virtual void OnWillRenderCanvas() {

            if (ShouldUpdateView) {
                ShouldUpdateView = false;

                // Don't know why but it is going into -
                // To reproduce this bug save a scene in 9:16 ratio and change to lets say 4:5 and when you enter the scene the scrollview will scroll
                ScrollRect.normalizedPosition = new Vector2(Mathf.Clamp01(ScrollRect.normalizedPosition.x), Mathf.Clamp01(ScrollRect.normalizedPosition.y));

//                UpdateView(Vector2.zero);
            }
        }

#region IPointerDownHandler implementation

        public void OnPointerDown(PointerEventData eventData) {
            StopLerping();
            StopAnimating();
        }

        public void StopLerping() {
            Lerping = false;
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (!Dragged && Snap) {
                TargetSnap = NormalizedPagePositions[TargetIndexInternal];
                StopAnimating();
                Lerping = true;
            }
            Dragged = false;
        }

#endregion
    }
}