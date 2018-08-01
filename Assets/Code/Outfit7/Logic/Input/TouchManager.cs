using System.Collections.Generic;
using Outfit7.Logic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Outfit7.Util;

namespace Outfit7.Logic.Input {

    public class TouchManager : Manager<TouchManager> {
        private const string Tag = "TouchManager";
        public const string InputPassiveTag = "INPUT_PASSIVE";
        public const string InputActiveTag = "INPUT_ACTIVE";

        public const int MaxNumOfTouchEvents = 10;
        public const int MaxNumOfMouseEvents = 3;
        public const int MaxNumOfInputEvents = MaxNumOfTouchEvents + MaxNumOfMouseEvents;

        private List<ITouchHandler> TouchHandlers = new List<ITouchHandler>(5);
        private List<TouchWrapper> OldTouches = new List<TouchWrapper>();
        private List<TouchWrapper> Touches = new List<TouchWrapper>(MaxNumOfInputEvents);

        // mouse input
        private Vector3 PreviousMouseInputPosition;
        private bool MouseDown = false;

        private bool InputEnabledInternal = true;

        public bool InputEnabled {
            get {
                return InputEnabledInternal;
            }
            set {
                InputEnabledInternal = value;
                Touches.Clear();
            }
        }

        public void TransformMouseEventsToTouch() {
            for (int i = 0; i < 3; ++i) {
                int inputIndex = MaxNumOfTouchEvents + i;
                if (UnityEngine.Input.GetMouseButton(i)) {
                    if (UnityEngine.Input.GetMouseButtonDown(i)) {
                        TouchWrapper t = new TouchWrapper(UnityEngine.Input.mousePosition, inputIndex);
                        t.Phase = TouchPhase.Began;
                        MouseDown = true;
                        PreviousMouseInputPosition = UnityEngine.Input.mousePosition;
                        Touches.Add(t);
                        return;
                    }
                    if (MouseDown) {
                        if (Vector3.Distance(PreviousMouseInputPosition, UnityEngine.Input.mousePosition) > 0) {
                            TouchWrapper t = new TouchWrapper(UnityEngine.Input.mousePosition, inputIndex);
                            t.Phase = TouchPhase.Moved;
                            Touches.Add(t);
                        } else {
                            TouchWrapper t = new TouchWrapper(UnityEngine.Input.mousePosition, inputIndex);
                            t.Phase = TouchPhase.Stationary;
                            Touches.Add(t);
                        }
                        PreviousMouseInputPosition = UnityEngine.Input.mousePosition;
                        return;
                    }
                } else if (UnityEngine.Input.GetMouseButtonUp(i)) {
                    TouchWrapper t = new TouchWrapper(UnityEngine.Input.mousePosition, inputIndex);
                    t.Phase = TouchPhase.Ended;
                    MouseDown = false;
                    PreviousMouseInputPosition = UnityEngine.Input.mousePosition;
                    Touches.Add(t);
                    return;
                }
            }
        }

        public void TransformTouchInputToGenericTouch() {
            for (int i = 0; i < UnityEngine.Input.touchCount; ++i) {
                UnityEngine.Touch touch = UnityEngine.Input.GetTouch(i);
                TouchWrapper tw = new TouchWrapper(touch);
                Touches.Add(tw);
            }
        }

        public override void OnPreUpdate(float deltaTime) {

            if (!InputEnabledInternal) {
                return;
            }

            OldTouches.Clear();
            OldTouches.AddRange(Touches);

            Touches.Clear();
            TransformTouchInputToGenericTouch();

            if (Touches.Count == 0) {
                TransformMouseEventsToTouch();
            }

            // it can happen that unity loses events (wooooot) so we need to handle those
            for (int i = 0; i < OldTouches.Count; ++i) {
                var oldTouch = OldTouches[i];
                var newTouch = default(TouchWrapper);
                var foundNewTouchWithSameId = false;
                for (int j = 0; j < Touches.Count; ++j) {
                    newTouch = Touches[j];
                    if (newTouch.FingerId == oldTouch.FingerId) {
                        foundNewTouchWithSameId = true;
                        break;
                    }
                }

                // add old ones if we didn't get end event for them
                if ((!foundNewTouchWithSameId && oldTouch.Phase < TouchPhase.Ended)
                    || (foundNewTouchWithSameId && oldTouch.Phase < TouchPhase.Ended && newTouch.Phase == TouchPhase.Began)) {
                    //O7Log.DebugT(Tag, "OldTouch: {0}", oldTouch.ToString());
                    oldTouch.Phase = TouchPhase.Canceled;
                    Touches.Insert(0, oldTouch);
                }
            }


            HandleTouches();
        }

        private void HandleTouches() {
            for (int i = 0; i < Touches.Count; ++i) {
                TouchWrapper touch = Touches[i];
#if !STRIP_LOGS
                if (touch.Phase == TouchPhase.Began || touch.Phase == TouchPhase.Canceled || touch.Phase == TouchPhase.Ended) {
                    O7Log.DebugT(Tag, "Touch: {0}", touch.ToString());
                }
#endif
                if (touch.FingerId >= MaxNumOfInputEvents) {
                    continue;
                }

                int realTouchIndex = touch.FingerId >= MaxNumOfTouchEvents ? (touch.FingerId - MaxNumOfTouchEvents - 1) : touch.FingerId;

                if (BlockTouch(realTouchIndex)) {
                    continue;
                }

                for (int a = TouchHandlers.Count - 1; a >= 0; a--) {
                    ITouchHandler handler = TouchHandlers[a];
                    if (handler.HandleTouch(touch)) {
#if !STRIP_LOGS
                        if (touch.Phase == TouchPhase.Began || touch.Phase == TouchPhase.Canceled || touch.Phase == TouchPhase.Ended) {
                            O7Log.DebugT(Tag, "Touch: {0} handled by: {1}", touch.ToString(), handler.ToString());
                        }
#endif
                        break;
                    }
                }
            }
        }

        private bool BlockTouch(int fingerId) {
            EventSystem cur = EventSystem.current;
            if (cur == null) {
                return false;
            }

            if (!cur.IsPointerOverGameObject(fingerId)) {
                return false;
            }

            if (!cur.currentSelectedGameObject) {
                return false;
            }
            if (cur.currentSelectedGameObject.tag == InputPassiveTag) {
                return false;
            }
            if (cur.currentSelectedGameObject.tag == InputActiveTag) {
                return true;
            }
            if (cur.currentSelectedGameObject.GetComponent<Button>() == null) {
                return false;
            }
            return true;
        }

        public bool TouchOnUI() {
            for (int i = 0; i < Touches.Count; ++i) {
                TouchWrapper touch = Touches[i];
                if (touch.FingerId >= MaxNumOfInputEvents) {
                    continue;
                }

                int realTouchIndex = touch.FingerId >= MaxNumOfTouchEvents ? (touch.FingerId - MaxNumOfTouchEvents - 1) : touch.FingerId;
                if (BlockTouch(realTouchIndex)) {
                    return true;
                }
            }
            return false;
        }

        public void Register(ITouchHandler touchHandler) {
            TouchHandlers.Add(touchHandler);
            TouchHandlers.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }

        public void Unregister(ITouchHandler touchHandler) {
            TouchHandlers.Remove(touchHandler);
            TouchHandlers.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }
    }
}
