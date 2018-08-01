using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.Internal;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class SequencerTriggerEvent : SequencerEvent {

        public SequencerEventSnapNode SnapNode {
            get {
                return SnapNodes[0];
            }
        }

        public override void GetActionPoints(ref BinarySortList<ActionPoint> actionPoints, float prevTime, float currentTime) {
            if (!ConditionsMet())
                return;
            //Debug.LogError((StartTime < currentTime) + " " + (StartTime > currentTime - deltaTime));
            if (IsEventStartable(currentTime, StartTime, prevTime)) {
                actionPoints.AddSorted(new ActionPoint(StartTime, 0, SnapNode.GetOrder(), EventDirection, this), prevTime > currentTime ? ActionPoint.BackwardActionPointComparer : ActionPoint.DefaultActionPointComparer);
            }
        }

        public override void OnEvaluate(float deltaTime, float currentTime) {
            if (!ConditionsMet())
                return;
            //Debug.LogError((StartTime < currentTime) + " " + (StartTime > currentTime - deltaTime));
            if (IgnoreObjects()) {
                Trigger(null, currentTime);
            } else {
                for (int i = 0; i < Objects.Count; i++) {
                    Trigger(Objects[i].Components, currentTime);
                }
            }
        }

        public override void TriggerActionPoint(int index, float previousTime, float currentTime) {
            if (index == 0) {
                if (IgnoreObjects()) {
                    Trigger(null, currentTime);
                } else {
                    for (int i = 0; i < Objects.Count; i++) {
                        Trigger(Objects[i].Components, currentTime);
                    }
                }
            }

        }

        public void Trigger(List<Component> components, float currentTime) {
            OnTrigger(components, currentTime);
        }

        public virtual void OnTrigger(List<Component> components, float currentTime) {
            Debug.LogError("TriggerEvent Trigger");
        }
    }
}