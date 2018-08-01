using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Util;
using System;

namespace Outfit7.Sequencer {

    [System.Serializable]
    public class SequencerEventSnapNode {
        [SerializeField]
        private SequencerEvent OwningEvent;
        [SerializeField]
        private SequencerEvent LeftLink;
        [SerializeField]
        private int LeftLinkIndex;
        [SerializeField]
        private SequencerEvent RightLink;
        [SerializeField]
        private int RightLinkIndex;
        [SerializeField]
        private int EventOrder = 0;

        public enum ELinkDirection {
            Left,
            Right
        }

        public SequencerEventSnapNode GetLink(ELinkDirection linkDirection) {
            if (linkDirection == ELinkDirection.Left) {
                return LeftLink != null ? LeftLink.SnapNodes[LeftLinkIndex] : null;
            } else {
                return RightLink != null ? RightLink.SnapNodes[RightLinkIndex] : null;
            }
        }

        public int GetLinkIndex(ELinkDirection linkDirection) {
            if (linkDirection == ELinkDirection.Left) {
                return LeftLinkIndex;
            } else {
                return RightLinkIndex;
            }
        }

        public SequencerEvent GetEvent() {
            return OwningEvent;
        }

        public void SetEvent(SequencerEvent evnt) {
            OwningEvent = evnt;
        }

        //public void SetRightLink(SequencerEventSnapNode node, ELinkDirection linkDirection) {
        //    if (node == this || GetLink(linkDirection) == node) {
        //        return;
        //    }

        //    var oldLeft = GetLink(ELinkDirection.Left);
        //    var oldLeftIndex = GetLinkIndex(ELinkDirection.Left);
        //    var oldRight = GetLink(ELinkDirection.Right);
        //    var oldRightIndex = GetLinkIndex(ELinkDirection.Right);

        //    // take it out from the list first
        //    SetLinkReciprocal(null, ELinkDirection.Right);
        //    SetLinkReciprocal(null, ELinkDirection.Left);
        //    // link old left and right together
        //    oldRight.SetLinkReciprocal(oldLeft, ELinkDirection.Left);

        //    // link new node
        //    SetLinkReciprocal(node, ELinkDirection.Right);
        //    // previous left from node is our left now
        //    SetLinkReciprocal(node.GetLink(ELinkDirection.Left), ELinkDirection.Left);


        //    RightLink = node == null ? null : node.GetEvent();
        //    RightLinkIndex = node == null ? 0 : Array.IndexOf(node.GetEvent().SnapNodes, node);
        //    LeftLink = oldLeft == null ? null : oldLeft.GetEvent();
        //    LeftLinkIndex = oldLeftIndex;
        //    if (node != null) {
        //        node.LeftLink = GetEvent();
        //        node.LeftLinkIndex = Array.IndexOf(GetEvent().SnapNodes, this);
        //    }
        //    if (oldRight != null) {
        //        oldRight.RightLink = node == null ? null : node.GetEvent();
        //        oldRight.RightLinkIndex = node == null ? 0 : Array.IndexOf(node.GetEvent().SnapNodes, node);
        //        oldRight.LeftLink = oldLeft == null ? null : oldLeft.GetEvent();
        //        oldRight.LeftLinkIndex = oldLeftIndex;
        //    }
        //    if (oldLeft != null) {
        //        oldLeft.LeftLink = oldRight == null ? null : oldRight.GetEvent();
        //        oldLeft.LeftLinkIndex = oldRightIndex;
        //    }
        //}

        public void SetLink(SequencerEventSnapNode node, ELinkDirection linkDirection) {
            if (node == this || GetLink(linkDirection) == node) {
                return;
            }

            var oppositeLinkDirection = GetOppositeLinkDirection(linkDirection);

            var old1 = GetLink(oppositeLinkDirection);
            var old2 = GetLink(linkDirection);

            // take it out from the list first
            SetLinkReciprocal(this, null, linkDirection);
            SetLinkReciprocal(this, null, oppositeLinkDirection);
            // link old left and right together
            SetLinkReciprocal(old2, old1, oppositeLinkDirection);

            // previous left from node is our left now
            if (node != null) {
                SetLinkReciprocal(this, node.GetLink(oppositeLinkDirection), oppositeLinkDirection);
            }
            // link new node
            SetLinkReciprocal(this, node, linkDirection);

            RecalculateEventIndexes();
        }

        public int GetOrder() {
            return EventOrder;
        }

        public void SetOrder(int idx) {
            EventOrder = idx;
        }

        public bool HasLinks(ELinkDirection linkDirection) {
            switch (linkDirection) {
                case ELinkDirection.Left:
                    return LeftLink != null;
                case ELinkDirection.Right:
                    return RightLink != null;
            }

            return false;
        }

        public void GetLinkedNodes(ELinkDirection linkDirection, List<SequencerEventSnapNode> outEvents) {
            SequencerEventSnapNode node = null;
            node = GetLink(linkDirection);
            while (node != null) {
                outEvents.Add(node);
                node = node.GetLink(linkDirection);
            }
        }

        public virtual SequencerEventSnapNode GetLastLinkedNode(SequencerEvent.EEventDirection direction, SequencerEventSnapNode chainBreakNode = null) {
            return GetLastLinkedNode(GetLinkDirectionFromEventDirection(direction), chainBreakNode);
        }

        public virtual SequencerEventSnapNode GetLastLinkedNode(ELinkDirection linkDirection, SequencerEventSnapNode chainBreakNode = null) {
            SequencerEventSnapNode node = this;
            SequencerEventSnapNode tmpNode = null;

            tmpNode = node.GetLink(linkDirection);
            while (tmpNode != null && tmpNode != chainBreakNode) {
                node = tmpNode;
                tmpNode = node.GetLink(linkDirection);
            }

            return node;
        }

        public void RecalculateEventIndexes() {
            List<SequencerEventSnapNode> nodes = new List<SequencerEventSnapNode>();
            var firstNode = GetLastLinkedNode(ELinkDirection.Left);
            nodes.Add(firstNode);
            firstNode.GetLinkedNodes(ELinkDirection.Right, nodes);

            for (int begin = 0; begin < nodes.Count; ++begin) {
                var snapNode = nodes[begin];
                snapNode.SetOrder(begin);
            }
        }

        public void ClearLinks() {
            var oldLeft = LeftLink;
            var oldRight = RightLink;
            SetLink(null, ELinkDirection.Left);
            SetLink(null, ELinkDirection.Right);
            SetOrder(0);

            if (oldLeft != null) {
                oldLeft.RecalculateEventIndexes();
            }
            if (oldRight != null) {
                oldRight.RecalculateEventIndexes();
            }
        }

        public static ELinkDirection GetOppositeLinkDirection(ELinkDirection linkDirection) {
            switch (linkDirection) {
                case ELinkDirection.Left:
                    return ELinkDirection.Right;
                case ELinkDirection.Right:
                    return ELinkDirection.Left;
            }

            return 0;
        }

        public static ELinkDirection GetLinkDirectionFromEventDirection(SequencerEvent.EEventDirection eventDirection) {
            switch (eventDirection) {
                case SequencerEvent.EEventDirection.LEFT:
                    return ELinkDirection.Left;
                case SequencerEvent.EEventDirection.MIDDLE:
                case SequencerEvent.EEventDirection.RIGHT:
                    return ELinkDirection.Right;
            }

            return 0;
        }

        private static void SetLinkReciprocal(SequencerEventSnapNode fromNode, SequencerEventSnapNode toNode, ELinkDirection linkDirection) {
            if (toNode == null && fromNode == null) {
                return;
            }

            if (linkDirection == ELinkDirection.Left) {
                if (toNode == null) {
                    fromNode.LeftLink = null;
                    fromNode.LeftLinkIndex = 0;
                } else if (fromNode == null) {
                    toNode.RightLink = null;
                    toNode.RightLinkIndex = 0;
                } else {
                    fromNode.LeftLink = toNode.GetEvent();
                    fromNode.LeftLinkIndex = Array.IndexOf(toNode.GetEvent().SnapNodes, toNode);
                    toNode.RightLink = fromNode.GetEvent();
                    toNode.RightLinkIndex = Array.IndexOf(fromNode.GetEvent().SnapNodes, fromNode);
                }
            } else {
                if (toNode == null) {
                    fromNode.RightLink = null;
                    fromNode.RightLinkIndex = 0;
                } else if (fromNode == null) {
                    toNode.LeftLink = null;
                    toNode.LeftLinkIndex = 0;
                } else {
                    fromNode.RightLink = toNode.GetEvent();
                    fromNode.RightLinkIndex = Array.IndexOf(toNode.GetEvent().SnapNodes, toNode);
                    toNode.LeftLink = fromNode.GetEvent();
                    toNode.LeftLinkIndex = Array.IndexOf(fromNode.GetEvent().SnapNodes, fromNode);
                }
            }
        }
    }

}