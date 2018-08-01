using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Outfit7.Logic.StateMachineInternal;
using UnityEditor;

namespace Outfit7.Sequencer {
    public class PropertyAnimationDataView {
        PropertyAnimationData AnimationData = null;
        public SequencerEvent ParentEvent;
        protected float Repeat = 1;
        protected bool Bounce = false;


        public void Init(object animData) {
            OnInit(animData);
        }

        public virtual PropertyAnimationData GetAnimData() {
            return AnimationData;
        }

        public virtual void OnInit(object animData) {
            AnimationData = animData as PropertyAnimationData;
        }

        public virtual Vector4 GetSingleValue() {
            return Vector4.one;
        }

        public virtual string Name() {
            return "Property";
        }

        public void DrawGui(BaseProperty property, Rect curveRect, List<Parameter> parameters, bool optimizedView, bool hover = false) {
            OnDrawGui(property, curveRect, parameters, optimizedView, hover);
        }

        public void SetData(float repeat, bool bounce) {
            Repeat = repeat;
            Bounce = bounce;
        }

        public virtual void OnContextMenu(GenericMenu menu) {
            
        }

        public virtual void OnDrawGui(BaseProperty property, Rect curveRect, List<Parameter> parameters, bool optimizedView, bool hover) {
        }

        public virtual void OnUpdateWhileRecording(BaseProperty property, float absoluteTime, float duration, float normalizedTime, float multiplier, float offset, bool remap, float remap0, float remap1) {
        }

        protected Vector4 InverseRemap(Vector4 input, bool remap, float remap0, float remap1) {
            if (remap) {
                input.x = (input.x - remap0) / (remap1 - remap0);
                input.y = (input.y - remap0) / (remap1 - remap0);
                input.z = (input.z - remap0) / (remap1 - remap0);
                input.w = (input.w - remap0) / (remap1 - remap0);
                return input;
            } else {
                return input;
            }
        }

        public virtual bool OnHandleInput(BaseProperty property, TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            return false;
        }
    }
}