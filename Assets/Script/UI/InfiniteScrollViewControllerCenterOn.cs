using UnityEngine;

public partial class InfiniteScrollViewController {
    private int StepsRemaining = 0;
    private float StepsNotNormalized = float.MinValue;
    private bool CenterOnLerp = false;
    private int MovingDirection = 0;

    public bool IsCenterLerping {
        get {
            return CenterOnLerp;
        }
    }

    public override void CenterOnCell(AbstractCellData data) {
        CenterOnCell(IndexOf(data));
    }

    public override void CenterOnCell(int index) {
        Index = index;
        if (RefreshCalled) {
            GetScrollRect().StopMovement();
            UpdateView();
            if (FiniteLogic && (GetScrollRect().BlockToNext || GetScrollRect().BlockToPrevious)) {
                for (int i = 0; i < ContainerList.Count; i++) {
                    RectTransform rt = ContainerList[i].transform as RectTransform;
                    if (ContainerCellData.ContainsKey(rt) && ContainerCellData[rt] == CellDataList[index]) {
                        GetScrollRect().normalizedPosition = GetCenterOnCellTargetPosition(rt);
                        break;
                    }
                }
            } else {
                GetScrollRect().normalizedPosition = new Vector2(0.5f, 0.5f);
            }
        }

        PreviousVal = GetScrollRect().normalizedPosition;
    }

    public override void CenterOnCellAnimated(AbstractCellData data, bool modal) {
        CenterOnCellAnimated(IndexOf(data), modal);
    }

    public override void CenterOnCellAnimated(int index, bool modal) {
        CenterOnCellAnimated(index, 0, modal);
    }

    // directionPriority -1, 0 or 1
    public void CenterOnCellAnimated(int index, int directionPriority, bool modal) {
        StopLerping();
        EnableTouchRectTransform(!modal);
        StepsRemaining = GetNormalizedLengthToScrollTo(index);
        if (Mathf.Abs(StepsRemaining) > ContainerList.Count) {
            if (StepsRemaining < 0) {
                StepsRemaining = -ContainerList.Count;
                Index = index + ContainerList.Count;
            } else {
                StepsRemaining = ContainerList.Count;
                Index = index - ContainerList.Count;
            }
        }

        if (directionPriority != 0) {
            StepsRemaining = Mathf.Abs(StepsRemaining) * directionPriority;
        }

        if (StepsNotNormalized == float.MinValue) {
            if (GetScrollRect().horizontal) {
                StepsNotNormalized = GetScrollRect().horizontalNormalizedPosition;
            } else {
                StepsNotNormalized = GetScrollRect().verticalNormalizedPosition;
            }
        }

        if (FiniteLogic) {
            int halfContainers = ContainerList.Count / 2;
            if (directionPriority > 0) {
                if (CurrentIndex <= halfContainers) {
//                        StepsNotNormalized += 0.5f;
                    MovingDirection = 1;
                } else if (CurrentIndex >= Count - halfContainers - 1) {
//                        StepsNotNormalized += 0.5f;
                    MovingDirection = 1;
                }
            } else if (directionPriority < 0) {
                if (CurrentIndex <= halfContainers + 1) {
//                        StepsNotNormalized -= 0.5f;
                    MovingDirection = -1;
                } else if (CurrentIndex + halfContainers + 1 > Count) {
//                        StepsNotNormalized -= 0.5f;
                    MovingDirection = -1;
                }
            }
        }

        StepsNotNormalized = GetScrollRect().NormalizedPagePositions[FindNearest(StepsNotNormalized)];

        if (FiniteLogic) {
            if (ContainerCount > CellDataList.Count && CellDataList.Count % 2 == 0) {
                StepsNotNormalized += StepsRemaining;
            } else {
                StepsNotNormalized += StepsRemaining * 0.5f;
            }
        } else {
            StepsNotNormalized += StepsRemaining * GetScrollRect().PageSize;
        }

        CenterOnLerp = true;

        if (StepsRemaining > 0) {
            GetScrollRect().OnNextSpawned -= OnNextSpawnedCenterOn;
            GetScrollRect().OnNextSpawned += OnNextSpawnedCenterOn;
        } else {
            GetScrollRect().OnPreviousSpawned -= OnPreviousSpawnedCenterOn;
            GetScrollRect().OnPreviousSpawned += OnPreviousSpawnedCenterOn;
        }
    }

    private int GetNormalizedLengthToScrollTo(int goal) {
        // Logic by GregorM :)
        int at = Index;
        int distance = Mathf.Abs(goal - at);
        if (distance <= CellDataList.Count / 2) {
            return goal > at ? distance : -distance;
        } else {
            return goal > at ? -(CellDataList.Count - distance) : (CellDataList.Count - at + goal);
        }
    }

    private void LateUpdate() {
        if (CenterOnLerp) {
            if (UpdateNormalizedPosition(StepsNotNormalized)) {
                StopLerping();
                EnableTouchRectTransform(true);
                GetScrollRect().OnNextSpawned -= OnNextSpawnedCenterOn;
                GetScrollRect().OnPreviousSpawned -= OnPreviousSpawnedCenterOn;
                if (OnCenteringEnded != null) {
                    OnCenteringEnded();
                }
                StepsNotNormalized = float.MinValue;
            }
        }
    }

    private void OnNextSpawnedCenterOn(RectTransform container) {
        StepsNotNormalized -= GetScrollRect().PageSize;
    }

    private void OnPreviousSpawnedCenterOn(RectTransform container) {
        StepsNotNormalized += GetScrollRect().PageSize;
    }
}


