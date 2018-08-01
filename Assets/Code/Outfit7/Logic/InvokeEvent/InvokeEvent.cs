using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Logic {

    [Serializable]
    public class InvokeEvent {
        public enum ParameterType {
            None,
            Int,
            Bool,
            Float,
            String,
            ObjectReference,
            AnimationCurve,
            AssetReference,
        }

        [Serializable]
        public class Parameter {
            public ParameterType ParameterType;
            public int IntOrBoolParameter;
            public float FloatParameter;
            public string StringParameter;
            public UnityEngine.Object ObjectReferenceParameter;
            public AnimationCurve AnimationCurveParameter;
            public AssetReference AssetReferenceParameter;
        }

        public float Time;
        public string Method;
        public Parameter[] Parameters;
        public object[] ParameterObjects;

        public void Initialize() {
            if (Parameters == null) {
                return;
            }
            ParameterObjects = new object[Parameters.Length];
            for (int j = 0; j < Parameters.Length; j++) {
                InvokeEvent.Parameter parameter = Parameters[j];
                switch (parameter.ParameterType) {
                    case InvokeEvent.ParameterType.Int:
                        ParameterObjects[j] = parameter.IntOrBoolParameter;
                        break;
                    case InvokeEvent.ParameterType.Bool:
                        ParameterObjects[j] = parameter.IntOrBoolParameter > 0;
                        break;
                    case InvokeEvent.ParameterType.Float:
                        ParameterObjects[j] = parameter.FloatParameter;
                        break;
                    case InvokeEvent.ParameterType.String:
                        ParameterObjects[j] = parameter.StringParameter;
                        break;
                    case InvokeEvent.ParameterType.ObjectReference:
                        ParameterObjects[j] = parameter.ObjectReferenceParameter;
                        break;
                    case InvokeEvent.ParameterType.AnimationCurve:
                        ParameterObjects[j] = parameter.AnimationCurveParameter;
                        break;
                    case InvokeEvent.ParameterType.AssetReference:
                        ParameterObjects[j] = parameter.AssetReferenceParameter;
                        break;
                }
            }
        }

        public void Invoke(List<MonoBehaviour> monoBehaviours) {
            for (int j = 0; j < monoBehaviours.Count; j++) {
                InvokeUtils.Invoke(monoBehaviours[j], Method, ParameterObjects);
            }
        }
    }

}