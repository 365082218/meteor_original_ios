using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
[AddComponentMenu("UI/InfiniteScrollRect", 57)]
public class InfiniteScrollRect : ScrollRect {

    // TODO: (NonFullScreen question) What if there are only 2 non full screen objects but we need 3 to fill the page

    // TODO: EditorUI

    public delegate void OnObjectEventDelegate(RectTransform child);

    public OnObjectEventDelegate OnNextSpawned;
    public OnObjectEventDelegate OnPreviousSpawned;
    public OnObjectEventDelegate OnNextDespawned;
    public OnObjectEventDelegate OnPreviousDespawned;

    public bool BlockToNext { get; set; }

    public bool BlockToPrevious { get; set; }

    public float[] NormalizedPagePositions { get; protected set; }

    public float PageSize { get; protected set; }

    private bool FullScrollView = true;
    private float CustomOtherAxis = float.MinValue;

    private bool ChangedNormalizedPosition = false;

    private int SetupCount = 0;

    private bool DragEnabled = true;

    private bool ModifiedScale = false;
    private Vector2 AverageChildSize;
    private bool FiniteLogic = false;

    public void SetFiniteLogic(bool val) {
        FiniteLogic = val;
    }

    public void SetFullScrollView(bool val) {
        FullScrollView = val;
    }

    public void SetModifiedScale(bool val) {
        ModifiedScale = val;
    }

    public void SetDragEnabled(bool enable) {
        DragEnabled = enable; 
    }

    public bool IsDragEnabled() {
        return DragEnabled;
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        if (!DragEnabled)
            return;

        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData) {
        if (!DragEnabled)
            return;

        if (ChangedNormalizedPosition) {
            ChangedNormalizedPosition = false;
            OnEndDrag(eventData);
            OnBeginDrag(eventData);
        }
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData) {
        if (!DragEnabled)
            return;

        base.OnEndDrag(eventData);
    }

    public void SetCustomOtherAxis(float axis) {
        CustomOtherAxis = axis;
        RefreshCellCount();
    }

    // UIBehaviour
    protected override void OnEnable() {
        base.OnEnable();

        if (!Application.isPlaying) {
            return;
        }
        onValueChanged.AddListener(UpdateView);
    }

    protected override void OnDisable() {
        base.OnDisable();

        if (!Application.isPlaying) {
            return;
        }
        onValueChanged.RemoveListener(UpdateView);
    }

    protected override void LateUpdate() {
        base.LateUpdate();

        if (SetupCount > 0)
            return;

        if (!Application.isPlaying)
            return;

        SetupCount++;
        Setup();
    }

    private void Setup() {
        if (FullScrollView) {
            LayoutElement element;
            if (horizontal) {
                float width = (transform as RectTransform).rect.width;
                for (int i = 0; i < content.childCount; i++) {
                    element = content.GetChild(i).GetComponent<LayoutElement>();
                    if (element != null) {
                        element.minWidth = width;
                        element.preferredWidth = width;
                        if (CustomOtherAxis != float.MinValue) {
                            element.minHeight = CustomOtherAxis;
                            element.preferredHeight = CustomOtherAxis;
                        }
                    }
                }
            } else if (vertical) {
                float height = (transform as RectTransform).rect.height;
                for (int i = 0; i < content.childCount; i++) {
                    element = content.GetChild(i).GetComponent<LayoutElement>();
                    if (element != null) {
                        element.minHeight = height;
                        element.preferredHeight = height;
                        if (CustomOtherAxis != float.MinValue) {
                            element.minWidth = CustomOtherAxis;
                            element.preferredWidth = CustomOtherAxis;
                        }
                    }
                }    
            }
        }
        if (ModifiedScale) {
            AverageChildSize = content.rect.size / content.childCount;
        }
        RefreshCellCount();
    }

    public void RefreshCellCount() {
        if (FiniteLogic) {
            int pageCount = content.childCount;
            if (pageCount > 0) {
                NormalizedPagePositions = new float[pageCount];
                PageSize = 0.5f;

                for (int i = 0; i < pageCount; i++) {
                    NormalizedPagePositions[i] = i * PageSize;
                }
            }
        } else {
            int pageCount = content.childCount;
            if (pageCount > 0) {
                NormalizedPagePositions = new float[pageCount];
                PageSize = 1f / (float) (pageCount - 1);

                for (int i = 0; i < pageCount; i++) {
                    NormalizedPagePositions[i] = i * PageSize;
                }
            }
        }
    }

    // TODO: optimize with binary search
    private int FindNearest(float currentNormalizedPosition) {
        float distance = Mathf.Infinity;
        int output = 0;

        for (int index = 0; index < NormalizedPagePositions.Length; index++) {
            if (Mathf.Abs(NormalizedPagePositions[index] - currentNormalizedPosition) < distance) {
                distance = Mathf.Abs(NormalizedPagePositions[index] - currentNormalizedPosition);
                output = index;
            }
        }

        return output;
    }

    private Vector2 PreviousValue = Vector2.zero;

    protected virtual void UpdateView(Vector2 val) {
        Vector2 v = val - PreviousValue;

        int extra = (Mathf.Max(0, NormalizedPagePositions.Length / 2 - 1));

        int nearest = 0;

        // -velocity => scroll right, +velocity => scroll left
        if (horizontal) {
            nearest = FindNearest(val.x);
            if (v.x < 0) {
                if (!FiniteLogic && (nearest - extra <= 0 && !BlockToPrevious) ||
                    FiniteLogic && (nearest - extra <= -(NormalizedPagePositions.Length / 2 - 1) && !BlockToPrevious)) {
                    RectTransform child = content.GetChild(content.childCount - 1) as RectTransform;
#if UNITY_5_1 || UNITY_5_2
                    content.anchoredPosition = new Vector2(content.anchoredPosition.x - (ModifiedScale ? AverageChildSize.x : child.rect.width), content.anchoredPosition.y);
#else
                    SetContentAnchoredPosition(new Vector2(content.anchoredPosition.x - (ModifiedScale ? AverageChildSize.x : child.rect.width), content.anchoredPosition.y));
#endif
                    if (OnNextDespawned != null) {
                        OnNextDespawned(child);
                    }
                    child.SetAsFirstSibling();
                    ChangedNormalizedPosition = true;
                    if (OnPreviousSpawned != null) {
                        OnPreviousSpawned(child);
                        BlockToNext = false;
                    }
                }
            } else if (v.x > 0) {
                if (!FiniteLogic && (nearest + extra >= NormalizedPagePositions.Length - 1 && !BlockToNext) ||
                    FiniteLogic && (nearest + extra >= (NormalizedPagePositions.Length / 2 + 1) && !BlockToNext)) {
                    RectTransform child = content.GetChild(0) as RectTransform;
#if UNITY_5_1 || UNITY_5_2
                    content.anchoredPosition = new Vector2(content.anchoredPosition.x + (ModifiedScale ? AverageChildSize.x : child.rect.width), content.anchoredPosition.y);
#else
                    SetContentAnchoredPosition(new Vector2(content.anchoredPosition.x + (ModifiedScale ? AverageChildSize.x : child.rect.width), content.anchoredPosition.y));
#endif
                    if (OnPreviousDespawned != null) {
                        OnPreviousDespawned(child);
                    }
                    child.SetAsLastSibling();
                    ChangedNormalizedPosition = true;
                    if (OnNextSpawned != null) {
                        OnNextSpawned(child);
                        BlockToPrevious = false;
                    }
                }
            }
        } else if (vertical) {
            nearest = FindNearest(val.y);
            if (v.y < 0) {
                if (nearest - extra <= 0 && !BlockToPrevious) {
                    RectTransform child = content.GetChild(0) as RectTransform;
#if UNITY_5_1 || UNITY_5_2
                    content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y - (ModifiedScale ? AverageChildSize.y : child.rect.height));
#else
                    SetContentAnchoredPosition(new Vector2(content.anchoredPosition.x, content.anchoredPosition.y - (ModifiedScale ? AverageChildSize.y : child.rect.height)));
#endif
                    if (OnNextDespawned != null) {
                        OnNextDespawned(child);
                    }
                    child.SetAsLastSibling();
                    ChangedNormalizedPosition = true;
                    if (OnPreviousSpawned != null) {
                        OnPreviousSpawned(child);
                        BlockToNext = false;
                    }
                }
            } else if (v.y > 0) {
                if (nearest + extra >= NormalizedPagePositions.Length - 1 && !BlockToNext) {
                    RectTransform child = content.GetChild(content.childCount - 1) as RectTransform;
#if UNITY_5_1 || UNITY_5_2
                    content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y + (ModifiedScale ? AverageChildSize.y : child.rect.height));
#else
                    SetContentAnchoredPosition(new Vector2(content.anchoredPosition.x, content.anchoredPosition.y + (ModifiedScale ? AverageChildSize.y : child.rect.height)));
#endif
                    if (OnPreviousDespawned != null) {
                        OnPreviousDespawned(child);
                    }
                    child.SetAsFirstSibling();
                    ChangedNormalizedPosition = true;
                    if (OnNextSpawned != null) {
                        OnNextSpawned(child);
                        BlockToPrevious = false;
                    }
                }
            }
        }

        PreviousValue = normalizedPosition;
    }
}