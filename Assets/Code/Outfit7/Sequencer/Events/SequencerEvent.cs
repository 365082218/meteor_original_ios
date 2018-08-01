using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Internal;
using Outfit7.Util;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class ActionPoint {
        private SequencerEvent ActionEvent;
        private float Time = 0;
        private SequencerEvent.EEventDirection EventDirection;
        private int ActionOrder;
        private int Index = -1;

        private static ActionPointComparer defaultActionPointComparer;
        public static ActionPointComparer DefaultActionPointComparer {
            get {
                return (defaultActionPointComparer ?? (defaultActionPointComparer = new ActionPointComparer()));
            }
        }

        private static ActionPointComparer backwardActionPointComparer;
        public static ActionPointComparer BackwardActionPointComparer {
            get {
                return (backwardActionPointComparer ?? (backwardActionPointComparer = new ActionPointComparer(true)));
            }
        }

        public ActionPoint(float time, int index, int orderForward, SequencerEvent.EEventDirection direction, SequencerEvent actionEvent) {
            ActionEvent = actionEvent;
            Time = time;
            Index = index;
            ActionOrder = orderForward;
            EventDirection = direction;
        }

        public void Act(float previousTime, float currentTime) {
            ActionEvent.TriggerActionPoint(Index, previousTime, currentTime);
        }

        public float GetTime() {
            return Time;
        }

        public SequencerEvent.EEventDirection GetEventDirection() {
            return EventDirection;
        }

        public int GetOrder() {
            return ActionOrder;
        }

        public int GetIndex() {
            return Index;
        }

        public class ActionPointComparer : IComparer<ActionPoint> {
            private bool Backward = false;

            public ActionPointComparer(bool backward = false) {
                Backward = backward;
            }

            public int Compare(ActionPoint x, ActionPoint y) {
                var timeCompare = x.GetTime().CompareTo(y.GetTime());
                if (timeCompare == 0) {
                    // TODO: support playback backwards
                    int orderCompare = 0;
                    if (Backward) {
                        orderCompare = y.GetOrder().CompareTo(x.GetOrder());
                    } else {
                        orderCompare = x.GetOrder().CompareTo(y.GetOrder());
                    }
                    if (orderCompare == 0) {
                        var directionCompare = ((int)x.GetEventDirection()).CompareTo((int)y.GetEventDirection());
                        if (directionCompare == 0) {
                            var indexCompare = x.GetIndex().CompareTo(y.GetIndex());
                            return indexCompare;
                        }
                        return directionCompare;
                    }
                    return orderCompare;
                }
                return timeCompare;
            }
        }
    }

    [ExecuteInEditMode]
    public abstract class SequencerEvent : MonoBehaviour {

        public enum EEventDirection {
            LEFT,
            RIGHT,
            MIDDLE
        }

        [System.Serializable]
        public class ActorComponentObject {
            public List<UnityEngine.Component> Components = new List<UnityEngine.Component>();
        }

        private const int CurrentSerializedVersion = 1;

        #region LegacyCode
        [SerializeField, HideInInspector]
        private SequencerEvent leftLink;
        [SerializeField, HideInInspector]
        private SequencerEvent rightLink;
        [HideInInspector]
        public int EventIndexForward = 0;
        [HideInInspector]
        public int EventIndexBackward = 0;
        #endregion LegacyCode

        [SerializeField, HideInInspector]
        private int SerializedVersion = 0;
        [HideInInspector]
        public List<Condition> Conditions = new List<Condition>();
        public EEventDirection EventDirection = EEventDirection.RIGHT;
        public System.Type ComponentType;
        public List<ActorComponentObject> Objects = new List<ActorComponentObject>();
        public int UiTrackIndex = 0;
        public float StartTime = 0f;
        public bool Preplay = false;
        protected SequencerSequence Sequence;

        // this is some major haxing.... unity serialization sux sometimes
        [UI.ReadOnly]
        public SequencerEventSnapNode[] SnapNodes = new SequencerEventSnapNode[] {
            new SequencerEventSnapNode(),
            new SequencerEventSnapNode(),
            new SequencerEventSnapNode(),
            new SequencerEventSnapNode()
        };

        #region LegacyCode
        public virtual SequencerEvent LeftLink {
            get {
                return leftLink;
            }

            set {
                if (leftLink == value) {
                    return;
                }

                var oldLeft = LeftLink;
                leftLink = value;
                if (oldLeft != null) {
                    oldLeft.RightLink = value;
                }
            }
        }

        public virtual SequencerEvent RightLink {
            get {
                return rightLink;
            }

            set {
                if (rightLink == value) {
                    return;
                }

                var oldRight = RightLink;
                rightLink = value;
                if (oldRight != null) {
                    oldRight.LeftLink = value;
                }
            }
        }
        #endregion LegacyCode

        public void Init(Vector2 startPos) {
            if (startPos.x >= 0f)
                StartTime = startPos.x;
            if (startPos.y >= 0f) {
                UiTrackIndex = (int)startPos.y;
            }
            LinkNodes();
            ConvertToNewCode();
            OnInit();
        }

        public void LiveInit(SequencerSequence sequence) {
            Sequence = sequence;
            for (int k = 0; k < Conditions.Count; k++) {
                Condition condition = Conditions[k];
                condition.Parameter = sequence.GetParameterByIndex(condition.ParameterIndex);
                condition.ValueParameter = sequence.GetParameterByIndex(condition.ValueIndex);
            }
            OnLiveInit(sequence);
        }

        protected virtual void OnLiveInit(SequencerSequence sequence) {

        }

        public virtual float GetStartPoint() {
            return StartTime;
        }

        public virtual float GetEndPoint() {
            return StartTime;
        }

        public virtual bool IgnoreObjects() {
            return false;
        }

        public virtual void OnInit() {
        }

        public virtual void GetActionPoints(ref BinarySortList<ActionPoint> actionPoints, float prevTime, float currentTime) {
        }

        public virtual void TriggerActionPoint(int index, float previousTime, float currentTime) {
        }

        public void Evaluate(float deltaTime, float currentTime) {
            OnEvaluate(deltaTime, currentTime);
        }

        public virtual void OnEvaluate(float deltaTime, float currentTime) {

        }

        public virtual void DoPreplay() {
            if (Preplay)
                OnPreplay();
        }

        public virtual void OnPreplay() {

        }

        public virtual void ClearLinks() {
            LeftLink = null;
            RightLink = null;
            EventIndexForward = 0;
            EventIndexBackward = 0;
            for (int i = 0; i < SnapNodes.Length; ++i) {
                SnapNodes[i].ClearLinks();
            }
        }

        #region LegacyCode

        [Obsolete("Do not use this anymore. Does not work! Used for reverse compatibility.")]
        public virtual void GetLinkedEvents(EEventDirection direction, List<SequencerEvent> outEvents) {
            SequencerEvent node = null;
            switch (direction) {
                case EEventDirection.LEFT:
                    node = LeftLink;
                    while (node != null) {
                        outEvents.Add(node);
                        node = node.LeftLink;
                    }
                    break;
                case EEventDirection.RIGHT:
                    node = RightLink;
                    while (node != null) {
                        outEvents.Add(node);
                        node = node.RightLink;
                    }
                    break;
            }
        }

        [Obsolete("Do not use this anymore. Does not work! Used for reverse compatibility.")]
        public virtual SequencerEvent GetLastLinkedEvent(EEventDirection direction, SequencerEvent chainBreakEvent = null) {
            SequencerEvent node = this;
            SequencerEvent tmpNode = null;
            switch (direction) {
                case EEventDirection.LEFT:
                    tmpNode = node.LeftLink;
                    while (tmpNode != null && tmpNode != chainBreakEvent) {
                        node = tmpNode;
                        tmpNode = node.LeftLink;
                    }
                    break;
                case EEventDirection.RIGHT:
                    tmpNode = node.RightLink;
                    while (tmpNode != null && tmpNode != chainBreakEvent) {
                        node = tmpNode;
                        tmpNode = node.RightLink;
                    }
                    break;
            }

            return node;
        }

        private void ConvertToNewCode() {
            if (SerializedVersion == 0) {

                SerializedVersion = 1;
                if (EventIndexForward != 0 || EventIndexBackward != 0) {
                    List<SequencerEvent> evnts = new List<SequencerEvent>();
#pragma warning disable 618
                    var firstEvent = GetLastLinkedEvent(EEventDirection.LEFT);
                    evnts.Add(firstEvent);
                    firstEvent.GetLinkedEvents(EEventDirection.RIGHT, evnts);
#pragma warning restore 618

                    // old system only supported first node
                    for (int i = 0; i < evnts.Count; ++i) {
                        var evnt = evnts[i];

#if UNITY_EDITOR
                        if (!Application.isPlaying) {
                            Undo.RecordObject(evnt, "EventLink");
                        }
#endif

                        evnt.SerializedVersion = 1;
                        evnt.EventIndexForward = 0;
                        evnt.EventIndexBackward = 0;
                        evnt.leftLink = null;
                        evnt.rightLink = null;
                        evnt.SnapNodes[0].SetEvent(evnt);
                        var nextEvnt = i + 1 < evnts.Count ? evnts[i + 1] : null;
                        if (nextEvnt != null) {
                            // do this here because awake might not have been called yet!
                            nextEvnt.SnapNodes[0].SetEvent(nextEvnt);
                        }
                        evnt.SnapNodes[0].SetLink(nextEvnt != null ? nextEvnt.SnapNodes[0] : null, SequencerEventSnapNode.ELinkDirection.Right);
                    }
                }
            }
        }
        #endregion LegacyCode

        public virtual bool CanBeLinkedTo() {
            return true;
        }

        public void LinkToEvent(SequencerEvent toEvent, EEventDirection direction, int fromNode, int toNode, bool goToLastInChain) {
            SequencerEventSnapNode snapToNode = null;
            SequencerEventSnapNode snapFromNode = SnapNodes[fromNode];
            if (toEvent != null) {
                snapToNode = toEvent.SnapNodes[toNode];
                if (goToLastInChain) {
                    snapToNode = snapToNode.GetLastLinkedNode(direction, snapFromNode);
                }
            }
            switch (direction) {
                case EEventDirection.LEFT:
                    snapFromNode.SetLink(snapToNode, SequencerEventSnapNode.ELinkDirection.Right);
                    break;
                case EEventDirection.MIDDLE:
                case EEventDirection.RIGHT:
                    snapFromNode.SetLink(snapToNode, SequencerEventSnapNode.ELinkDirection.Left);
                    break;
            }
        }

        public void RecalculateEventIndexes() {
            for (int i = 0; i < SnapNodes.Length; ++i) {
                SnapNodes[i].RecalculateEventIndexes();
            }
        }

        public bool ConditionsMet() {
            if (Conditions.Count > 0) {
                for (int i = 0; i < Conditions.Count; i++) {
                    if (!Conditions[i].IsTrue())
                        return false;
                }
            }
            return true;
        }

        public virtual bool IsEventStartable(float currentTime, float startTime, float previousTime) {
            switch (EventDirection) {
                case EEventDirection.LEFT: {
                        if (previousTime < currentTime) {
                            return currentTime >= startTime && previousTime < startTime;
                        } else {
                            return currentTime <= startTime && previousTime >= startTime;
                        }
                    }
                case EEventDirection.RIGHT:
                case EEventDirection.MIDDLE: {
                        if (previousTime < currentTime) {
                            return currentTime >= startTime && previousTime <= startTime;
                        } else {
                            return currentTime <= startTime && previousTime > startTime;
                        }
                    }
            }
            return false;
        }

        protected virtual void Awake() {
#if UNITY_EDITOR
            LinkNodes();
#endif
            ConvertToNewCode();
            Assert.IsTrue(SerializedVersion == CurrentSerializedVersion, "Serialized version updated but not correctly reserialized!");
        }

        protected virtual void OnDestroy() {
            ClearLinks();
        }

        private void LinkNodes() {
            for (int i = 0; i < SnapNodes.Length; ++i) {
                var node = SnapNodes[i];
                if (node.GetEvent() == null) {
                    node.SetEvent(this);
                }
            }
        }
    }
}