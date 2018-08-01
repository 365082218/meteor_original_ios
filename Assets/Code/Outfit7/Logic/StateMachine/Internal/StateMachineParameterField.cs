using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Logic.StateMachineInternal {

    public enum ParameterFieldType {
        SOLID,
        RANGE,
        CURRENT
    }

    public class ParameterField {

        public void LiveInit(List<Parameter> parameters) {
            if (ParameterIndex >= 0) {
                Parameter = parameters[ParameterIndex];
            }
        }

        public virtual void Init(object value) {
        }

        [NonSerialized] public Parameter Parameter = null;
        public int ParameterIndex = -1;
        public ParameterFieldType FieldType = ParameterFieldType.SOLID;
    }

    [Serializable]
    public class ParameterFloatField : ParameterField {
        [HideInInspector] [SerializeField] private float Value_;
        public float MinValue;
        public float MaxValue;

        public ParameterFloatField(float value) {
            Value_ = value;
            MinValue = value;
            MaxValue = value;
        }

        public override void Init(object value) {
            Randomize(value);
        }

        public void Randomize(object value) {
            if (FieldType == ParameterFieldType.RANGE)
                Value_ = StaticRandom.NextFloat(MinValue, MaxValue);
            else if (FieldType == ParameterFieldType.CURRENT) {
                if (value == null)
                    return;
                float val = (float) value;
                Value_ = val;
            }
        }

        public float Value {
            get {
                if (ParameterIndex == -1 || Parameter == null)
                    return Value_;
                else
                    return Parameter.ValueFloat;
            }
            set {
                Value_ = value;
            }
        }
    }

    [Serializable]
    public class ParameterIntField : ParameterField {
        [HideInInspector] [SerializeField]  private int Value_;
        public int MinValue;
        public int MaxValue;

        public ParameterIntField(int value) {
            Value_ = value;
        }

        public override void Init(object value) {
            Randomize(value);
        }

        public void Randomize(object value) {
            if (FieldType == ParameterFieldType.RANGE)
                Value_ = StaticRandom.Next(MinValue, MaxValue);
            else if (FieldType == ParameterFieldType.CURRENT) {
                if (value == null)
                    return;
                int val = (int) value;
                Value_ = val;
            }
        }

        public int Value {
            get {
                if (ParameterIndex == -1 || Parameter == null)
                    return Value_;
                else
                    return Parameter.ValueInt;
            }
            set {
                Value_ = value;
            }
        }
    }

    [Serializable]
    public class ParameterVectorField : ParameterField {
        [HideInInspector] [SerializeField] private Vector4 Value_;
        public Vector4 MinValue;
        public Vector4 MaxValue;

        public ParameterVectorField(Vector4 value) {
            Value_ = value;
        }

        public override void Init(object value) {
            Randomize(value);
        }

        public void Randomize(object value) {
            if (FieldType == ParameterFieldType.RANGE)
                Value_ = new Vector4(StaticRandom.NextFloat(MinValue.x, MaxValue.x), 
                    StaticRandom.NextFloat(MinValue.y, MaxValue.y),
                    StaticRandom.NextFloat(MinValue.z, MaxValue.z),
                    StaticRandom.NextFloat(MinValue.w, MaxValue.w));
            else if (FieldType == ParameterFieldType.CURRENT) {
                if (value == null)
                    return;
                Vector4 val = (Vector4) value;
                Value_ = val;
            }
        }

        public Vector4 Value {
            get {
                if (ParameterIndex == -1 || Parameter == null)
                    return Value_;
                else
                    return Parameter.ValueVector;
            }
            set { 
                Value_ = value;
            }
        }

        public float ValueX {
            get {
                return Value_.x;
            }
            set { 
                Value_.x = value;
            }
        }

        public float ValueY {
            get {
                return Value_.y;
            }
            set { 
                Value_.y = value;
            }
        }

        public float ValueZ {
            get {
                return Value_.z;
            }
            set { 
                Value_.z = value;
            }
        }

        public float ValueW {
            get {
                return Value_.w;
            }
            set { 
                Value_.w = value;
            }
        }
    }

    [Serializable]
    public class ParameterComponentField : ParameterField {
        [HideInInspector] [SerializeField] private Component Value_;

        public ParameterComponentField(Component value) {
            Value_ = value;
        }

        public Component Value {
            get {
                if (ParameterIndex == -1 || Parameter == null)
                    return Value_;
                else
                    return Parameter.ValueComponent;
            }
            set { 
                Value_ = value;
            }
        }
    }

    [Serializable]
    public class ParameterBoolField : ParameterField {
        [HideInInspector] [SerializeField] private bool Value_;

        public ParameterBoolField(bool value) {
            Value_ = value;
        }

        public override void Init(object value) {
            Randomize(value);
        }

        public void Randomize(object value) {
            if (FieldType == ParameterFieldType.RANGE)
                Value_ = StaticRandom.NextBool();
            else if (FieldType == ParameterFieldType.CURRENT) {
                if (value == null)
                    return;
                bool val = (bool) value;
                Value_ = val;
            }
        }

        public bool Value {
            get {
                if (ParameterIndex == -1 || Parameter == null)
                    return Value_;
                else
                    return Parameter.ValueInt > 0;
            }
            set { 
                Value_ = value;
            }
        }
    }

    [Serializable]
    public class ParameterComponentListField : ParameterField {
        [SerializeField] private List<Component> Value_ = new List<Component>();

        public override void Init(object value) {
        }

        public List<Component> Value {
            get {
                if (ParameterIndex == -1 || Parameter == null)
                    return Value_;
                else
                    return Parameter.ValueComponentList;
            }
            set { 
                Value_ = value;
            }
        }

        public void Clear() {
            Value_.Clear();
        }

        public void Add(Component c) {
            Value_.Add(c);
        }
    }

}
