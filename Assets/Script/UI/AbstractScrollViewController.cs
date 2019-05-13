using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public abstract partial class AbstractScrollViewController : UIBehaviour {

    [Header("AbstractScrollViewController")]
//        [SerializeField] protected int ActionLimit = 5;
    [SerializeField] protected bool NoDeactivations = false;
    [SerializeField] protected ScrollRect ScrollRect;
    [SerializeField] protected RectTransform Content;
    [SerializeField] protected RectTransform ScrollViewRectTransform = null;
    [SerializeField] protected TouchRectTransform TouchRectTransform = null;
    [SerializeField] protected List<AbstractCellController> CellControllerPrefabList;

    protected List<AbstractCellData> CellDataList = new List<AbstractCellData>();
    protected Dictionary<Type, List<AbstractCellController>> ReusableCellPool = new Dictionary<Type, List<AbstractCellController>>();


    protected bool Dragged = false;

    public bool IsDragged {
        get { 
            return Dragged;
        }
    }

    public virtual bool IsBusy {
        get { 
            return Dragged || Moving || Lerping;
        }
    }

    public void EnableTouchRectTransform(bool enable) {
        if (TouchRectTransform == null)
            return;

        TouchRectTransform.enabled = enable;
    }

    public bool IsTouchRectTransformNullOrEnabled() {
        if (TouchRectTransform == null)
            return true;

        return TouchRectTransform.enabled;
    }

    public ScrollRect GetScrollRect() {
        return ScrollRect;
    }

    public RectTransform GetContent() {
        return Content;
    }

#if UNITY_EDITOR
    [Serializable]
    public class CellToCreate {
        public string Name { get; private set; }

        public float MainAxisLength { get; private set; }

        public CellToCreate() {
            Name = "";
            MainAxisLength = 200f;
        }

        public void SetName(string name) {
            Name = name;
        }

        public void SetMainAxisLength(float mainAxisLength) {
            MainAxisLength = mainAxisLength;
        }
    }

    public List<CellToCreate> CellsToCreate = new List<CellToCreate>();

    public void SetScrollRectEditor(ScrollRect scrollRect) {
        ScrollRect = scrollRect;
    }

    public void SetContentEditor(RectTransform content) {
        Content = content;
    }

    public void SetScrollViewRectTransformEditor(RectTransform rt) {
        ScrollViewRectTransform = rt;
    }
#endif

    public RectTransform GetScrollViewRectTransform() {
        // TODO: Remove this eventually
        if (ScrollViewRectTransform == null) {
            ScrollViewRectTransform = transform as RectTransform;
        }
        return ScrollViewRectTransform;
    }

    public void SetCellControllerPrefabList(List<AbstractCellController> list) {
        CellControllerPrefabList = new List<AbstractCellController>(list);
    }

    public void AddCellControllerPrefabList(AbstractCellController cellController) {
        CellControllerPrefabList.Add(cellController);
    }

    public void RemoveCellControllerPrefabList(AbstractCellController cellController) {
        if (CellControllerPrefabList.Contains(cellController)) {
            CellControllerPrefabList.Remove(cellController);
        }
    }

    public abstract void RefreshView();

    public abstract void ForceUpdateCells();

    protected abstract AbstractCellController GetFirstInactiveReusableCell(Type type);

    protected virtual AbstractCellController GetReusableCell(Type type) {
        AbstractCellController cc = GetFirstInactiveReusableCell(type);
        if (cc != null) {
            return cc;
        } else {
            for (int i = 0; i < CellControllerPrefabList.Count; i++) {
                if (CellControllerPrefabList[i].GetType().Name.Equals(type.Name)) {
                    return Instantiate(CellControllerPrefabList[i]);
                }
            }

            throw new InvalidOperationException("Did not find CellController - check if the cells are linked in CellControllerList");
        }
    }

    protected abstract AbstractCellController Spawn(RectTransform parent, AbstractCellData cellData);

    protected virtual void UpdateCell(AbstractCellController cc, AbstractCellData cd) {
        if (cc != null) {
            cc.UpdateCell(cd);
        }
    }

    protected virtual void UpdateCell(AbstractCellData cd) {
        UpdateCell(cd.CellControllerRef, cd);
    }

    protected virtual RectTransform GetCellRectTransformPrefab(Type type) {
        for (int i = 0; i < CellControllerPrefabList.Count; i++) {
            if (CellControllerPrefabList[i].GetType().Name.Equals(type.Name)) {
                return CellControllerPrefabList[i].transform as RectTransform;
            }
        }

        throw new Exception("CellRectTransform of type " + type + " not found");
    }

    protected virtual void OnScrollValueChanged(Vector2 val) {
            
    }

    // UIBehaviour
    protected override void OnEnable() {
        base.OnEnable();

        if (Snap) {
            Canvas.willRenderCanvases -= OnWillRenderCanvasSnap;
            Canvas.willRenderCanvases += OnWillRenderCanvasSnap;
        }
        ScrollRect.onValueChanged.AddListener(OnScrollValueChanged);

        // windows shouldn't scroll ever!
        GetScrollRect().scrollSensitivity = 0f;
    }

    protected override void OnDisable() {
        base.OnDisable();

        Canvas.willRenderCanvases -= OnWillRenderCanvasSnap;
        ScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
    }

    protected override void Start() {
        base.Start();

        RefreshView();
    }
}