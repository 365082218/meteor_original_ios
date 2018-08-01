using UnityEngine;
using UnityEngine.EventSystems;
using Outfit7.Util;

namespace Outfit7.UI {
    public partial class ScrollViewController : IBeginDragHandler, IEndDragHandler, IDragHandler {
        private float[] NormalizedPagePositions;

        private bool DragInit = true;
        private int DragStartNearest;
        private int TargetIndexInternal;

        public int TargetIndex {
            get { 
                if (Snap) {
                    TargetIndexInternal = FindNearest(ScrollRect.horizontal ? ScrollRect.horizontalNormalizedPosition : ScrollRect.verticalNormalizedPosition);
                    return TargetIndexInternal;
                } else {
                    return int.MinValue;
                }
            }
        }

        private float ClampMinPos = float.MinValue;
        private float ClampMaxPos = float.MaxValue;

        // TODO: optimize with binary search
        private int FindNearest(float currentNormalizedPosition) {
            float distance = Mathf.Infinity;
            int output = 0;

            float velocity = 0f;
            if (ScrollRect.horizontal) {
                velocity = ScrollRect.velocity.x;
            } else if (ScrollRect.vertical) {
                velocity = ScrollRect.velocity.y;
            }
            int steps = (int) Mathf.Abs(velocity) / PagesInOneSwipeDamp;
            if (MaxSteps > 0 && steps > MaxSteps) {
                steps = MaxSteps;
            }

            for (int index = 0; index < NormalizedPagePositions.Length; index++) {
                float npp = NormalizedPagePositions[index];
                float curdist = Mathf.Abs(npp - currentNormalizedPosition);
                if (curdist < distance) {
                    distance = curdist;
                    output = index;
                }
            }
            if (steps > 0) {
                if (velocity > 0) {
                    output -= steps;
                } else if (velocity < 0) {
                    output += steps;
                }
            }
            output = Mathf.Clamp(output, 0, NormalizedPagePositions.Length - 1);
            return output;
        }

        private void RefreshSnapParameters() {
            if (!Snap)
                return;
            
            int pageCount = Content.childCount;//ContainerList.Count;
            if (pageCount > 0) {
                NormalizedPagePositions = new float[pageCount];
                float pageSize = 1f / (float) (pageCount - 1);

                for (int i = 0; i < pageCount; i++) {
                    NormalizedPagePositions[i] = i * pageSize;
                }
                //
                if (ScrollRect.horizontal) {
                    TargetIndexInternal = FindNearest(ScrollRect.horizontalNormalizedPosition);
                } else if (ScrollRect.vertical) {
                    TargetIndexInternal = FindNearest(ScrollRect.verticalNormalizedPosition);
                }
                //
                Clamp();
            }
        }

        public void OnBeginDrag(PointerEventData eventData) {
            Lerping = false;
            if (!Snap)
                return;
        }

        public void OnDrag(PointerEventData eventData) {
            Lerping = false;
            Dragged = true;
            Clamp();

            if (!Snap)
                return;

            if (DragInit) {
                DragStartNearest = FindNearest(ScrollRect.horizontal ? ScrollRect.horizontalNormalizedPosition : ScrollRect.verticalNormalizedPosition);
                DragInit = false;
            }
        }

        void Clamp() {
            if (ScrollRect.horizontal) {
                if (ClampMinPos != float.MinValue) {
                    if (ScrollRect.horizontalNormalizedPosition < ClampMinPos)
                        ScrollRect.horizontalNormalizedPosition = ClampMinPos;
                }
                if (ClampMaxPos != float.MaxValue) {
                    if (ScrollRect.horizontalNormalizedPosition > ClampMaxPos)
                        ScrollRect.horizontalNormalizedPosition = ClampMaxPos;
                }
            } else if (ScrollRect.vertical) {
                if (ClampMinPos != float.MinValue) {
                    if (ScrollRect.verticalNormalizedPosition < ClampMinPos)
                        ScrollRect.verticalNormalizedPosition = ClampMinPos;
                }
                if (ClampMaxPos != float.MaxValue) {
                    if (ScrollRect.verticalNormalizedPosition > ClampMaxPos)
                        ScrollRect.verticalNormalizedPosition = ClampMaxPos;
                }
            }
        }

        public void SetLimits(int posmin, int posmax) {
            ClampMinPos = Mathf.Clamp01((float) posmin / (float) (Count - 1));
            ClampMaxPos = Mathf.Clamp01((float) posmax / (float) (Count - 1));
            Clamp();
        }

        public void OnEndDrag(PointerEventData eventData) {
            Clamp();

            if (!Snap)
                return;

            float velocity = 0f;
            float normalizedPos = 0.5f;
            if (ScrollRect.horizontal) {
                velocity = ScrollRect.velocity.x;
                normalizedPos = ScrollRect.horizontalNormalizedPosition;
            } else if (ScrollRect.vertical) {
                velocity = ScrollRect.velocity.y;
                normalizedPos = ScrollRect.verticalNormalizedPosition;
            }
                
            TargetIndexInternal = FindNearest(normalizedPos);
                
            if (TargetIndexInternal == DragStartNearest && Mathf.Abs(velocity) > InertiaCutoffMagnitude) {
                if (velocity < 0) {
                    TargetIndexInternal = DragStartNearest + 1;
                } else if (velocity > 1) {
                    TargetIndexInternal = DragStartNearest - 1;
                }
                TargetIndexInternal = Mathf.Clamp(TargetIndexInternal, 0, NormalizedPagePositions.Length - 1);
            }

            if (normalizedPos > 0f && normalizedPos < 1f) {
                TargetSnap = NormalizedPagePositions[TargetIndexInternal];
                StopAnimating();
                Lerping = true;
            }

            DragInit = true;
        }

        public override void PreviousContinuousEnd() {
            Canvas.willRenderCanvases -= ContinousPreviousPressUpdate;
            if (Snap) {
                // TODO: make transform half of the OnEndDrag method into a OnTouchEnd like in infinite scroll view
//                if (Time.time - PreviousPressedTime <= ContinuousPressTimeTreshold) {
//                    Previous();
//                    return;
//                }
//                OnTouchEnd(-1);
                throw new System.NotImplementedException();    
            }
        }

        public override void NextContinuousEnd() {
            Canvas.willRenderCanvases -= ContinousNextPressUpdate;
            if (Snap) {
                // TODO: make transform half of the OnEndDrag method into a OnTouchEnd like in infinite scroll view
//                if (Time.time - NextPressedTime <= ContinuousPressTimeTreshold) {
//                    Next();
//                    return;
//                }
//                OnTouchEnd(1);
                throw new System.NotImplementedException();    
            }
        }
    }
}