using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.Internal;
using System;

namespace Outfit7.Sequencer {
    public class SequencerContinuousEvent : SequencerEvent {
        public float Duration = .33f;
        [NonSerialized]
        public bool Active = false;
        [NonSerialized]
        public bool ConditionFailed = false;

        public SequencerEventSnapNode LeftSnapNode {
            get {
                return SnapNodes[0];
            }
        }

        public SequencerEventSnapNode RightSnapNode {
            get {
                return SnapNodes[1];
            }
        }

        public override void GetActionPoints(ref BinarySortList<ActionPoint> actionPoints, float prevTime, float currentTime) {
            float endTime = StartTime + Duration;
            float startTime = StartTime;

            bool currentTimeInsideEvent = currentTime >= startTime && endTime > currentTime;
            float absoluteTime = currentTime - startTime;

            if (!Active &&
                currentTimeInsideEvent) {
                if (ConditionFailed || !ConditionsMet()) {
                    ConditionFailed = true;
                    return;
                }
                actionPoints.AddSorted(new ActionPoint(startTime, 0, LeftSnapNode.GetOrder(), EventDirection, this), ActionPoint.DefaultActionPointComparer);
                // we have to process AFTER start if absolute time is 0 aka same time
                int order = absoluteTime == 0f ? 65535 : 0;
                actionPoints.AddSorted(new ActionPoint(startTime + absoluteTime, 1, order, EventDirection, this), ActionPoint.DefaultActionPointComparer);
            } else if (Active) {
                if (currentTimeInsideEvent) {
                    actionPoints.AddSorted(new ActionPoint(startTime + absoluteTime, 1, 0, EventDirection, this), ActionPoint.DefaultActionPointComparer);
                } else {
                    actionPoints.AddSorted(new ActionPoint(endTime, 2, RightSnapNode.GetOrder(), EventDirection, this), ActionPoint.DefaultActionPointComparer);
                }
            } else if (!currentTimeInsideEvent) {
                bool endTimeCheck = currentTime >= endTime;
                bool prevTimeBeforeEvent = prevTime <= startTime;
                // goto check
                bool prevTimeInsideEvent = prevTime >= startTime && prevTime < endTime;
                if (endTimeCheck && (prevTimeBeforeEvent || prevTimeInsideEvent)) {
                    if (!ConditionsMet()) {
                        return;
                    }
                    actionPoints.AddSorted(new ActionPoint(startTime, 0, LeftSnapNode.GetOrder(), EventDirection, this), ActionPoint.DefaultActionPointComparer);
                    actionPoints.AddSorted(new ActionPoint(endTime, 2, RightSnapNode.GetOrder(), EventDirection, this), ActionPoint.DefaultActionPointComparer);
                }
                ConditionFailed = false;
            }
        }

        public override void OnEvaluate(float deltaTime, float currentTime) {
            float endTime = StartTime + Duration;
            bool currentTimeInsideEvent = (currentTime >= StartTime && currentTime <= endTime);
            float absoluteTime = currentTime - StartTime;
            float normalizedTime = absoluteTime / Duration;

            if (!Active &&
                currentTimeInsideEvent) {
                if (ConditionFailed || !ConditionsMet()) {
                    ConditionFailed = true;
                    return;
                }
                Enter(absoluteTime, normalizedTime);
                Process(absoluteTime, normalizedTime);
            } else if (Active) {
                if (currentTimeInsideEvent) {
                    Process(absoluteTime, normalizedTime);
                } else {
                    Exit(Mathf.Clamp(absoluteTime, 0, Duration), Mathf.Clamp01(normalizedTime));
                }
            } else if (!currentTimeInsideEvent) {
                ConditionFailed = false;
            }
        }

        public override void TriggerActionPoint(int index, float previousTime, float currentTime) {
            float absoluteTime = currentTime - StartTime;
            float normalizedTime = absoluteTime / Duration;

            switch (index) {
                case 0:
                    Enter(absoluteTime, normalizedTime);
                    break;
                case 1:
                    Process(absoluteTime, normalizedTime);
                    break;
                case 2:
                    Exit(Mathf.Clamp(absoluteTime, 0, Duration), Mathf.Clamp01(normalizedTime));
                    break;
            }
        }

        public override float GetEndPoint() {
            return StartTime + Duration;
        }

        public float GetNormalizedTime(float currentTime) {
            float absoluteTime = currentTime - StartTime;
            return absoluteTime / Duration;
        }

        public void Enter(float absoluteTime, float normalizedTime) {
            Active = true;
            for (int i = 0; i < Objects.Count; i++) {
                OnEnter(Objects[i].Components, absoluteTime, normalizedTime);
            }
        }

        public void Process(float absoluteTime, float normalizedTime) {
            for (int i = 0; i < Objects.Count; i++) {
                OnProcess(Objects[i].Components, absoluteTime, normalizedTime);
            }
        }

        public void Exit(float absoluteTime, float normalizedTime) {
            Active = false;
            for (int i = 0; i < Objects.Count; i++) {
                OnProcess(Objects[i].Components, absoluteTime, normalizedTime);
                OnExit(Objects[i].Components);
            }
        }

        public virtual void OnEnter(List<Component> components, float absoluteTime, float normalizedTime) {
            Debug.LogError("ContinuousEvent Start");
        }

        public virtual void OnProcess(List<Component> components, float absoluteTime, float normalizedTime) {
            //Debug.LogError("ContinuousEvent Process");
        }

        public virtual void OnExit(List<Component> components) {
            Debug.LogError("ContinuousEvent End");
        }

        public override void DoPreplay() {
            Active = false;
            base.DoPreplay();
        }
    }
}