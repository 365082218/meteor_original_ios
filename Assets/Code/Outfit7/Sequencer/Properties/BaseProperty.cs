using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class BaseProperty : MonoBehaviour {
        public bool Enabled = true;
        public bool UsedThisFrame = false;
        public ParameterComponentListField Components = new ParameterComponentListField();
        public bool ApplyX = true;
        public bool ApplyY = true;
        public bool ApplyZ = true;
        public bool ApplyW = true;

        public virtual void LiveInit(SequencerSequence sequence) {
            Components.LiveInit(sequence.Parameters);
        }

        public void Apply(Vector4 value) {
            for (int i = 0; i < Components.Value.Count; i++) {
                OnApply(Components.Value[i], value);
                //UsedThisFrame = true;
            }
        }

        public bool[] GetValuesUsed() {
            bool[] values = new bool[]{ false, false, false, false };
            for (int i = 0; i < GetNumberOfValuesUsed(); i++)
                values[i] = true;
            if (!ApplyX)
                values[0] = false;
            if (!ApplyY)
                values[1] = false;
            if (!ApplyZ)
                values[2] = false;
            if (!ApplyW)
                values[3] = false;
            return values;
        }

        public virtual int GetNumberOfValuesUsed() {
            //for UI and optimization
            return 4;
        }

        public enum DisplayMode {
            CURVE,
            COLOR
        }

        public virtual DisplayMode GetDisplayMode() {
            return DisplayMode.CURVE;
        }

        public virtual void OnApply(Component c, Vector4 value) {
        }

        public Vector4 Value(out bool success) {
            for (int i = 0; i < Components.Value.Count; i++) {
                if (Components.Value[i] == null)
                    continue;
                return OnValue(Components.Value[i], out success);
            }
            success = false;
            return Vector4.zero;
        }

        public virtual Vector4 OnValue(Component c, out bool sucesss) {
            sucesss = false;
            return Vector4.zero;
        }

        public Vector2 ApplyPartial(Vector2 currentValue, Vector4 applyValue) {
            if (ApplyX && ApplyY)
                return (Vector2) applyValue;
            return new Vector2(
                ApplyX ? applyValue.x : currentValue.x,
                ApplyY ? applyValue.y : currentValue.y);

        }

        public Vector3 ApplyPartial(Vector3 currentValue, Vector4 applyValue) {
            if (ApplyX && ApplyY && ApplyZ)
                return (Vector3) applyValue;
            return new Vector3(
                ApplyX ? applyValue.x : currentValue.x,
                ApplyY ? applyValue.y : currentValue.y,
                ApplyZ ? applyValue.z : currentValue.z);

        }

        public Vector4 ApplyPartial(Vector4 currentValue, Vector4 applyValue) {
            if (ApplyX && ApplyY && ApplyZ && ApplyW)
                return applyValue;
            return new Vector4(
                ApplyX ? applyValue.x : currentValue.x,
                ApplyY ? applyValue.y : currentValue.y,
                ApplyZ ? applyValue.z : currentValue.z,
                ApplyW ? applyValue.w : currentValue.w);
        }

        public Color ApplyPartial(Color currentValue, Vector4 applyValue) {
            if (ApplyX && ApplyY && ApplyZ && ApplyW)
                return new Color(applyValue.x, applyValue.y, applyValue.z, applyValue.w);
            return new Color(
                ApplyX ? applyValue.x : currentValue.r,
                ApplyY ? applyValue.y : currentValue.g,
                ApplyZ ? applyValue.z : currentValue.b,
                ApplyW ? applyValue.w : currentValue.a);
        }

        public Rect ApplyPartial(Rect currentValue, Vector4 applyValue) {
            if (ApplyX && ApplyY && ApplyZ && ApplyW)
                return new Rect(applyValue.x, applyValue.y, applyValue.z, applyValue.w);
            return new Rect(
                ApplyX ? applyValue.x : currentValue.x,
                ApplyY ? applyValue.y : currentValue.y,
                ApplyZ ? applyValue.z : currentValue.width,
                ApplyW ? applyValue.w : currentValue.height);
        }

        public bool IsValueActive(int i) {
            switch (i) {
                case 0:
                    return ApplyX;
                case 1:
                    return ApplyY;
                case 2:
                    return ApplyZ;
                case 3:
                    return ApplyW;
                default:
                    return true;
            }
        }
    }
}