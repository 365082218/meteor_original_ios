using System;
using System.Collections.Generic;
using UnityEngine;

namespace Outfit7.Logic.StateMachineInternal {

    [Serializable]
    public class Parameter : Base {
        // Serialized
        public ParameterType ParameterType;
        public int ParameterIndex;
        public float ValueFloat;
        public int ValueInt;
        public Vector4 ValueVector;
        public Component ValueComponent;
        public List<Component> ValueComponentList = new List<Component>();
        public object UserData;

        public void SetInt(int i) {
            ValueInt = i;
            UserData = null;
        }

        public void SetFloat(float f) {
            ValueFloat = f;
            UserData = null;
        }

        public void SetBool(bool b) {
            ValueInt = b ? 1 : 0;
            UserData = null;
        }

        public void SetBoolTrigger(object userData = null) {
            SetBool(true);
            UserData = userData;
        }

        public void SetIntTrigger(int i, object userData = null) {
            SetInt(i);
            UserData = userData;
        }

        public void SetVector(Vector4 v) {
            ValueVector = v;
            UserData = null;
        }

        public void SetComponent(Component c) {
            ValueComponent = c;
            UserData = null;
        }

        public void SetComponentList(List<Component> cl) {
            ValueComponentList = cl;
            UserData = null;
        }

        public void AddComponent(Component c) {
            ValueComponentList.Add(c);
            UserData = null;
        }

        public void ClearComponentList() {
            ValueComponentList.Clear();
        }

        public void ResetTrigger() {
            ValueFloat = 0.0f;
            ValueInt = 0;
            UserData = null;
        }

        public bool IsIndexer {
            get { 
                return ParameterType == ParameterType.Int || ParameterType == ParameterType.IntTrigger || ParameterType == ParameterType.Enum || ParameterType == ParameterType.EnumTrigger || ParameterType == ParameterType.EnumBitMask;
            }
        }

        public bool IsEnum { 
            get { 
                return ParameterType == ParameterType.Enum || ParameterType == ParameterType.EnumTrigger || ParameterType == ParameterType.EnumBitMask;
            }
        }
    }

}