using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InfiniteScrollRect))]
[AddComponentMenu("UI/InfiniteScrollViewController", 58)]
public partial class InfiniteScrollViewController : AbstractScrollViewController, IPointerDownHandler, IPointerUpHandler {

    [Header("InfiniteScrollViewController")]
    [SerializeField] protected bool FiniteLogic = false;
    [SerializeField] protected bool TwoSideHiddenContainers = false;
    [SerializeField] protected int CellsVisibleAtTheSameTime = 0;
    [SerializeField] protected List<LayoutElement> ContainerList = new List<LayoutElement>();
    [SerializeField] protected bool StartOnLeft = true;
    protected Dictionary<RectTransform, AbstractCellData> ContainerCellData = new Dictionary<RectTransform, AbstractCellData>();

    private bool ShouldUpdateView = false;
    private int Index = -1;
    private bool Spawned = false;
    private bool RefreshCalled = false;
    private bool DragEnabled = true;
    private Vector2 PreviousVal = Vector2.zero;

    public Action<int> OnNextCell;
    public Action<int> OnPreviousCell;

#if UNITY_EDITOR
    public void SetContainerList(List<LayoutElement> containerList) {
        ContainerList = containerList;
    }

    public void AddContainer(LayoutElement container) {
        ContainerList.Add(container);
    }
#endif

    public void SetContainerWidth(float width) {
        for (int i = 0; i < ContainerList.Count; i++) {
            ContainerList[i].preferredWidth = width;
        }
    }

    public void SetContainerHeight(float height) {
        for (int i = 0; i < ContainerList.Count; i++) {
            ContainerList[i].preferredHeight = height;
        }
    }

    public override bool IsBusy {
        get {
            return CenterOnLerp || base.IsBusy;
        }
    }

    public int CurrentIndex {
        get {
            return Index;
        }
    }

    public AbstractCellData CurrentData {
        get {
            if (Index >= 0 && Index < CellDataList.Count) {
                return CellDataList[Index];
            } else {
                return null;
            }
        }
    }

    public AbstractCellController CurrentController {
        get {
            if (CurrentData != null) {
                return CurrentData.CellControllerRef;
            }
            return null;
        }
    }

    public int ContainerCount {
        get {
            return ContainerList.Count;
        }
    }

    public new InfiniteScrollRect GetScrollRect() {
        return ScrollRect as InfiniteScrollRect;
    }

    public void SetDragEnabled(bool enable) {
        DragEnabled = enable;
        UpdateSwipeEnabled();
    }

    public int Previous(bool stopLerping = true) {
//            if (TwoSideHiddenContainers && CellDataList.Count + 2 < ContainerList.Count || !TwoSideHiddenContainers && CellDataList.Count < ContainerList.Count) {
//                return Index;
//            }

        if (FiniteLogic && GetPreviousIndex() == CellDataList.Count - 1) {
            return Index;
        }

        if (IsTouchRectTransformNullOrEnabled()) {
            if (stopLerping) {
                StopLerping();
            }
            int idx = GetPreviousIndex();
            CenterOnCellAnimated(idx, -1, false);
            return idx;
        }

        return Index;
    }

    public int Next(bool stopLerping = true) {
//            if (TwoSideHiddenContainers && CellDataList.Count + 2 < ContainerList.Count || !TwoSideHiddenContainers && CellDataList.Count < ContainerList.Count) {
//                return Index;
//            }

        if (FiniteLogic && GetNextIndex() == 0) {
            return Index;
        }
            
        if (IsTouchRectTransformNullOrEnabled()) {
            if (stopLerping) {
                StopLerping();
            }
            int idx = GetNextIndex();
            CenterOnCellAnimated(idx, 1, false);
            return idx;
        }

        return Index;
    }

    protected void UpdateSwipeEnabled() {
        bool infiniteLogicLowData = (!FiniteLogic && CellDataList.Count > ContainerList.Count) || FiniteLogic || TwoSideHiddenContainers && CellDataList.Count + 2 > ContainerList.Count;
        GetScrollRect().SetDragEnabled(CellDataList.Count > 1 && infiniteLogicLowData && DragEnabled);
    }

    public override void RefreshView() {
        UpdateView();
        ShouldUpdateView = true;
        RefreshCalled = true;

        GetScrollRect().RefreshCellCount();
        UpdateSwipeEnabled();
        //bool infiniteLogicLowData = (!FiniteLogic && CellDataList.Count > ContainerList.Count) || FiniteLogic || TwoSideHiddenContainers && CellDataList.Count + 2 > ContainerList.Count;
        //GetScrollRect().SetDragEnabled(CellDataList.Count > 1 && infiniteLogicLowData && DragEnabled);
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

    protected virtual void Despawn(RectTransform container) {
        if (!ContainerCellData.ContainsKey(container))
            return;

        AbstractCellData cellData = ContainerCellData[container];

        if (cellData.CellControllerRef == null)
            return;

        AbstractCellController cellController = cellData.CellControllerRef;

        if (!ReusableCellPool.ContainsKey(cellController.GetType())) {
            ReusableCellPool.Add(cellController.GetType(), new List<AbstractCellController>());
        }

        ReusableCellPool[cellController.GetType()].Insert(0, cellController);
        if (NoDeactivations) {
            cellController.transform.SetParent(null, false);
        } else {
            cellController.gameObject.SetActive(false);

            cellController.transform.SetParent(transform, false);
            cellController.transform.SetSiblingIndex(-1000);
        }
        cellController.enabled = false;

        if (cellData.OnDespawn != null) {
            cellData.OnDespawn(cellData);
        }

        cellData.CellControllerRef = null;

        ContainerCellData.Remove(container);
    }

    protected override AbstractCellController GetFirstInactiveReusableCell(Type type) {
        if (ReusableCellPool.ContainsKey(type)) {
            List<AbstractCellController> reusableControllerList = ReusableCellPool[type];
            if (reusableControllerList.Count > 0) {
                AbstractCellController cellController = reusableControllerList[0];
                ReusableCellPool[cellController.GetType()].Remove(cellController);
                cellController.enabled = true;
                if (!NoDeactivations) {
                    cellController.gameObject.SetActive(true);
                }
                return cellController;
            }
        }

        return null;
    }


    protected override AbstractCellController Spawn(RectTransform parent, AbstractCellData cellData) {
        if (cellData.CellControllerRef != null) {
            return cellData.CellControllerRef;
        }

        AbstractCellController cc = GetReusableCell(cellData.CellControllerType);
        cc.transform.SetParent(parent, false);
        UpdateCell(cc, cellData);

        #if UNITY_EDITOR
        cc.name = parent.name;
        #endif

        if (ContainerCellData.ContainsKey(parent)) {
            ContainerCellData[parent] = cellData;
        } else {
            ContainerCellData.Add(parent, cellData);
        }

        if (cellData.OnSpawn != null) {
            cellData.OnSpawn(cellData);
        }

        return cc;
    }

    public override void ForceUpdateCells() {
        for (int i = 0; i < ContainerList.Count; i++) {
            ContainerCellData[ContainerList[i].transform as RectTransform].UpdateCellController();
        }
    }


    // UIBehaviour
    protected override void OnEnable() {
        base.OnEnable();

        GetScrollRect().OnNextSpawned += OnNextSpawned;
        GetScrollRect().OnPreviousSpawned += OnPreviousSpawned;
        GetScrollRect().OnNextDespawned += OnNextDespawned;
        GetScrollRect().OnPreviousDespawned += OnPreviousDespawned;

        ShouldUpdateView = true;
    }

    protected override void OnDisable() {
        base.OnDisable();

        Canvas.willRenderCanvases -= OnWillRenderCanvas;
        Canvas.willRenderCanvases -= OnWillRenderCanvasSnap;

        GetScrollRect().OnNextSpawned -= OnNextSpawned;
        GetScrollRect().OnPreviousSpawned -= OnPreviousSpawned;
    }

    protected override void Awake() {
        base.Awake();
        GetScrollRect().SetFiniteLogic(FiniteLogic);
    }

    protected override void OnScrollValueChanged(Vector2 val) {
        base.OnScrollValueChanged(val);

        Vector2 delta = PreviousVal - val;
        PreviousVal = val;

        if (FiniteLogic && ContainerCount < CellDataList.Count) {
            if (Index == 0 && val.x > 0.25f && (delta.x < 0 || MovingDirection > 0)) {
                IndexIncrement();
            } else if (Index == Count - CellsVisibleAtTheSameTime && val.x < 0.75f && (delta.x > 0 || MovingDirection < 0)) {
                IndexDecrement();
            } else if (Index == Count - CellsVisibleAtTheSameTime - 1 && val.x > 0.75f && (delta.x < 0 || MovingDirection > 0)) {
                IndexIncrement();
            } else if (Index == CellsVisibleAtTheSameTime && val.x < 0.25f && (delta.x > 0 || MovingDirection < 0)) {
                IndexDecrement();
            }
        }

        if (TwoSideHiddenContainers && ContainerList.Count == CellDataList.Count + 1) {
            if (GetScrollRect().horizontal) {
                Transform leftMost = Content.GetChild(0);
                Transform rightMost = Content.GetChild(Content.childCount - 1);
                if (val.x > 0.5f && leftMost.childCount > 0 ||
                    val.x < 0.5f && rightMost.childCount > 0) {
                    leftMost.SetSiblingIndex(Content.childCount - 1);
                    rightMost.SetSiblingIndex(0);
                }
            } else if (GetScrollRect().vertical) {
                throw new NotImplementedException();
            }
        }
    }

    private int GetContainerIndexWithOffset(int index, int i) {
        int containerIdx = index + i;
        if (containerIdx < 0) {
            containerIdx = ContainerList.Count + containerIdx;
        }

        return containerIdx;
    }

    private int GetCellDataIndex(int index, int i) {
        int cellDataIdx = index + i;
        if (cellDataIdx < 0) {
            cellDataIdx = CellDataList.Count + cellDataIdx;
        } else if (cellDataIdx >= CellDataList.Count) {
            cellDataIdx = cellDataIdx - CellDataList.Count;
        }

        return cellDataIdx;
    }

    protected virtual void UpdateView() {

        Canvas.ForceUpdateCanvases();

        if (Index == -1) {
            Index = 0;
        }

        for (int i = 0; i < ContainerList.Count; i++) {
            ContainerList[i].transform.SetSiblingIndex(i);
        }

        if (Spawned) {
            for (int i = 0; i < ContainerList.Count; i++) {
                Despawn(ContainerList[i].transform as RectTransform);
            }
        }

        if (CellDataList.Count == 0) {
            return;
        }

        InfiniteScrollRect scrollRect = GetScrollRect();
        if (StartOnLeft && (!TwoSideHiddenContainers && CellDataList.Count < ContainerList.Count || TwoSideHiddenContainers && CellDataList.Count + 1 < ContainerList.Count)) {
            for (int i = 0; i < CellDataList.Count; i++) {
                Spawn(ContainerList[i].transform as RectTransform, CellDataList[i]);  
                if (FiniteLogic && Index - ContainerList.Count / 2 < 0) {
                    if (CellDataList.Count <= ContainerList.Count) {
                        scrollRect.BlockToNext = true;
                    }
                    scrollRect.BlockToPrevious = true;
                } else if (FiniteLogic && CellDataList.Count - ContainerList.Count / 2 - 1 < Index) {
                    if (CellDataList.Count <= ContainerList.Count) {
                        scrollRect.BlockToPrevious = true;
                    }
                    scrollRect.BlockToNext = true;   
                }
            }
            if (FiniteLogic) {
                scrollRect.normalizedPosition = scrollRect.horizontal ? new Vector2(0f, 0.5f) : new Vector2(0f, 0f);
            }
        } else {
            int containersOnSides = ContainerList.Count / 2;
            scrollRect.BlockToPrevious = false;
            scrollRect.BlockToNext = false;

            if (scrollRect.horizontal) {
                if (FiniteLogic && Index - ContainerList.Count / 2 < 0) {
                    scrollRect.RefreshCellCount();
                    scrollRect.horizontalNormalizedPosition = scrollRect.NormalizedPagePositions[0];
                    if (Index > 0) {
                        float width = (Content.GetChild(Content.childCount - 1) as RectTransform).rect.width;
                        Content.anchoredPosition = new Vector2(Content.anchoredPosition.x - width * Index + width * 0.5f, Content.anchoredPosition.y);
                    }
                    for (int i = 0; i < ContainerList.Count; i++) {
                        if (i < CellDataList.Count) {
                            ContainerList[i].gameObject.SetActive(true);
                            Spawn(ContainerList[i].transform as RectTransform, CellDataList[i]);  
                        } else {
                            ContainerList[i].gameObject.SetActive(false);
                        }
                    }
                    if (CellDataList.Count <= ContainerList.Count) {
                        scrollRect.BlockToNext = true;
                    }
                    scrollRect.BlockToPrevious = true;
//                        Index = containersOnSides;
                } else if (FiniteLogic && CellDataList.Count - ContainerList.Count / 2 - 1 < Index) {
                    scrollRect.RefreshCellCount();
                    scrollRect.horizontalNormalizedPosition = scrollRect.NormalizedPagePositions[scrollRect.NormalizedPagePositions.Length - 1];
                    if (Index < CellDataList.Count) {
                        float width = (Content.GetChild(0) as RectTransform).rect.width;
                        Content.anchoredPosition = new Vector2(Content.anchoredPosition.x + width * (CellDataList.Count - 1 - Index) - width * 0.5f, Content.anchoredPosition.y);
                    }
                    for (int i = 0; i < ContainerList.Count; i++) {
                        if (i < CellDataList.Count) {
                            ContainerList[ContainerList.Count - 1 - i].gameObject.SetActive(true);
                            int count = CellDataList.Count > ContainerCount ? ContainerCount : CellDataList.Count;
                            // TODO: FIX: very highly buggy code
                            Spawn(ContainerList[ContainerList.Count - 1 - i].transform as RectTransform, CellDataList[CellDataList.Count - count + i]);  
                        } else {
                            ContainerList[ContainerList.Count - 1 - i].gameObject.SetActive(false);
                        }
                    }
                    if (CellDataList.Count <= ContainerList.Count) {
                        scrollRect.BlockToPrevious = true;
                    }
                    scrollRect.BlockToNext = true;
//                        Index = CellDataList.Count - containersOnSides - 1;
                } else {
                    // center
                    Spawn(ContainerList[containersOnSides].transform as RectTransform, CellDataList[Index]);

                    // right side
                    for (int i = 0; i < containersOnSides; i++) {
                        int containerIdx = i + containersOnSides + 1;
                        int cellDataIdx = GetCellDataIndex(Index, i + 1);
                        if (CellDataList.Count <= cellDataIdx) {
                            break;
                        }
                        Spawn(ContainerList[containerIdx].transform as RectTransform, CellDataList[cellDataIdx]);
                    }

                    // left side
                    for (int i = 0; i < containersOnSides; i++) {
                        int cellDataIdx = GetCellDataIndex(Index, -containersOnSides + i);
                        if (cellDataIdx < 0) {
                            break;
                        }
                        Spawn(ContainerList[i].transform as RectTransform, CellDataList[cellDataIdx]);
                    }
                }
            } else if (scrollRect.vertical) {
                if (FiniteLogic && Index - ContainerList.Count / 2 < 0) {
                    scrollRect.RefreshCellCount();
                    scrollRect.verticalNormalizedPosition = scrollRect.NormalizedPagePositions[0];
                    if (Index > 0) {
                        float height = (Content.GetChild(Content.childCount - 1) as RectTransform).rect.height;
                        Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, Content.anchoredPosition.y - height * Index + height * 0.5f);
                    }
                    for (int i = 0; i < ContainerList.Count; i++) {
                        if (i < CellDataList.Count) {
                            ContainerList[i].gameObject.SetActive(true);
                            int count = CellDataList.Count > ContainerCount ? ContainerCount : CellDataList.Count;
                            Spawn(ContainerList[i].transform as RectTransform, CellDataList[count - i - 1]);  
                        } else {
                            ContainerList[i].gameObject.SetActive(false);
                        }
                    }
                    if (CellDataList.Count <= ContainerList.Count) {
                        scrollRect.BlockToNext = true;
                    }
                    scrollRect.BlockToPrevious = true;
//                        Index = containersOnSides;
                } else if (FiniteLogic && Index + ContainerList.Count / 2 >= CellDataList.Count - 1) {
                    scrollRect.RefreshCellCount();
                    scrollRect.verticalNormalizedPosition = scrollRect.NormalizedPagePositions[scrollRect.NormalizedPagePositions.Length - 1];
                    if (Index < CellDataList.Count) {
                        float height = (Content.GetChild(0) as RectTransform).rect.height;
                        Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, Content.anchoredPosition.y + height * (CellDataList.Count - 1 - Index) - height * 0.5f);
                    }
                    for (int i = 0; i < ContainerList.Count; i++) {
                        if (i < CellDataList.Count) {
                            ContainerList[ContainerList.Count - 1 - i].gameObject.SetActive(true);
                            int count = CellDataList.Count > ContainerCount ? ContainerCount : CellDataList.Count;
                            Spawn(ContainerList[ContainerList.Count - 1 - i].transform as RectTransform, CellDataList[CellDataList.Count - count + i]);  
                        } else {
                            ContainerList[ContainerList.Count - 1 - i].gameObject.SetActive(false);
                        }
                    }
                    if (CellDataList.Count <= ContainerList.Count) {
                        scrollRect.BlockToPrevious = true;
                    }
                    scrollRect.BlockToNext = true;
//                        Index = CellDataList.Count - containersOnSides - 1;
                } else {
                    // center
                    int dataCount = CellDataList.Count;
                    if (containersOnSides < dataCount) {
                        Spawn(ContainerList[containersOnSides].transform as RectTransform, CellDataList[Index]);
                    }

                    // top side
                    for (int i = 0; i < containersOnSides && i < dataCount; i++) {
                        int containerIdx = containersOnSides - 1 - i;
                        int cellDataIdx = GetCellDataIndex(Index, 1 + i);
                        Spawn(ContainerList[containerIdx].transform as RectTransform, CellDataList[cellDataIdx]);
                    }

                    // bottom side
                    for (int i = 0; i < containersOnSides && i < (dataCount - containersOnSides + 1); i++) {
                        int containerIdx = containersOnSides + 1 + i;
                        int cellDataIdx = GetCellDataIndex(Index, -1 - i);
                        Spawn(ContainerList[containerIdx].transform as RectTransform, CellDataList[cellDataIdx]);
                    }

                    if (FiniteLogic) {
                        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
                    }
                }
            }
        }


//            if (!TwoSideHiddenContainers && CellDataList.Count < ContainerList.Count || TwoSideHiddenContainers && CellDataList.Count + 1 < ContainerList.Count) {
//                for (int i = 0; i < ContainerList.Count; i++) {
//                    if (ContainerList[i].transform.childCount == 0) {
//                        ContainerList[i].ignoreLayout = true;
//                    }
//                }
//            } else {
//                for (int i = 0; i < ContainerList.Count; i++) {
//                    ContainerList[i].ignoreLayout = false;
//                }
//            }

        Spawned = true;

        Canvas.ForceUpdateCanvases();
    }

    private void OnNextSpawned(RectTransform container) {
        float amount = FiniteLogic ? 0.5f : GetScrollRect().PageSize;
        if (Snap) {
            TargetSnap -= amount;
        }
        if (GetScrollRect().horizontal) {
            PreviousVal.x -= amount;
        } else {
            PreviousVal.y -= amount;
        }
        IndexIncrement();
        int nextIndex = GetCellDataIndex(Index, ContainerList.Count / 2 + CellsVisibleAtTheSameTime / 2);
        Spawn(container, CellDataList[nextIndex]);
        if (FiniteLogic && nextIndex == CellDataList.Count - 1) {
            GetScrollRect().BlockToNext = true;
        }
        //Debug.LogWarningFormat("OnNextSpawned: TargetSnap: {0}", TargetSnap);
        if (OneCellPerSwipe) {
            SwipeNextCell = false;
        }
    }

    private void OnPreviousSpawned(RectTransform container) {
        float amount = FiniteLogic ? 0.5f : GetScrollRect().PageSize;
        if (Snap) {
            TargetSnap += amount;
        }
        if (GetScrollRect().horizontal) {
            PreviousVal.x += amount;
        } else {
            PreviousVal.y += amount;
        }
        IndexDecrement();
        int previousIndex = GetCellDataIndex(Index, -ContainerList.Count / 2 + CellsVisibleAtTheSameTime / 2);
        Spawn(container, CellDataList[previousIndex]);
        if (FiniteLogic && previousIndex == 0) {
            GetScrollRect().BlockToPrevious = true;
        }
        //Debug.LogWarningFormat("OnPreviousSpawned: TargetSnap: {0}", TargetSnap);
        if (OneCellPerSwipe) {
            SwipeNextCell = false;
        }
    }

    private void OnNextDespawned(RectTransform container) {
        Despawn(container);
    }

    private void OnPreviousDespawned(RectTransform container) {
        Despawn(container);
    }

    private int GetNextIndex() {
        int i = Index + 1;
        if (i >= CellDataList.Count) {
            i = 0;
        }
        return i;
    }

    private int GetPreviousIndex() {
        int i = Index - 1;
        if (i < 0) {
            i = CellDataList.Count - 1;
        }
        return i;
    }

    private void IndexIncrement() {
        Index = GetNextIndex();

        if (OnNextCell != null) {
            OnNextCell(Index);
        }
    }

    private void IndexDecrement() {
        Index = GetPreviousIndex();

        if (OnPreviousCell != null) {
            OnPreviousCell(Index);
        }
    }

    protected virtual void OnWillRenderCanvas() {

        if (ShouldUpdateView) {
            ShouldUpdateView = false;

            // Don't know why but it is going into -
            // To reproduce this bug save a scene in 9:16 ratio and change to lets say 4:5 and when you enter the scene the scrollview will scroll
            GetScrollRect().normalizedPosition = new Vector2(Mathf.Clamp01(GetScrollRect().normalizedPosition.x), Mathf.Clamp01(GetScrollRect().normalizedPosition.y));

            //                UpdateView(Vector2.zero);
        }
    }

#region IPointerDownHandler implementation

    public void OnPointerDown(PointerEventData eventData) {
        if (OneCellPerSwipe && OnCellDragged) {
            return;
        }
        StopLerping();
    }

#endregion

#region IPointerUpHandler implementation

    public void OnPointerUp(PointerEventData eventData) {
        if (OneCellPerSwipe && OnCellDragged) {
            return;
        }

        if (!Dragged && Snap) {
            CenterOnCellAnimated(Index, false);
        }

        Dragged = false;
    }

#endregion

    public void StopLerping(bool swipenextcell = true) {
        CenterOnLerp = false;
        MovingDirection = 0;
        Lerping = false;
        StepsNotNormalized = float.MinValue;
        if (swipenextcell) {
            SwipeNextCell = true;
        }
    }
}