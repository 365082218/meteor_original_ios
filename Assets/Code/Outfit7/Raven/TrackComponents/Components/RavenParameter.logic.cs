using System;
using System.Collections.Generic;
using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenParameter {

        // TODO: port this
        public Gradient m_ValueGradient = new Gradient();

        public void SetInt(int i) {
            m_ValueInt = i;
        }

        public void SetFloat(float f) {
            m_ValueFloat = f;
        }

        public void SetBool(bool b) {
            m_ValueInt = b ? 1 : 0;
        }

        public void SetBoolTrigger(object userData = null) {
            SetBool(true);
         }

        public void SetIntTrigger(int i, object userData = null) {
            SetInt(i);
        }

        public void SetVector(Vector4 v) {
            m_ValueVector = v;
        }

        public void SetGradient(Gradient g) {
            m_ValueGradient = g;
        }

        public void SetObject(UnityEngine.Object c) {
            m_ValueObject = c;
        }

        public void SetGameObjectList(List<GameObject> cl) {
            m_ValueGameObjectList = cl;
        }

        public void AddGameObject(GameObject c) {
            m_ValueGameObjectList.Add(c);
        }

        public void ClearGameObjectList() {
            m_ValueGameObjectList.Clear();
        }

        public RavenParameter ShallowCopy() {
            return MemberwiseClone() as RavenParameter;
        }

        public void ResetTrigger() {
            m_ValueFloat = 0.0f;
            m_ValueInt = 0;
        }

        public bool IsIndexer {
            get {
                return m_ParameterType == ERavenParameterType.Int || m_ParameterType == ERavenParameterType.IntTrigger || m_ParameterType == ERavenParameterType.Enum || m_ParameterType == ERavenParameterType.EnumTrigger || m_ParameterType == ERavenParameterType.EnumBitMask;
            }
        }

        public bool IsEnum {
            get {
                return m_ParameterType == ERavenParameterType.Enum || m_ParameterType == ERavenParameterType.EnumTrigger || m_ParameterType == ERavenParameterType.EnumBitMask;
            }
        }

        public Type GetValueType() {
            switch (m_ParameterType) {
                case ERavenParameterType.Bool:
                case ERavenParameterType.BoolTrigger:
                    return typeof(bool);

                case ERavenParameterType.Object:
                    return typeof(UnityEngine.Object);

                case ERavenParameterType.Enum:
                case ERavenParameterType.EnumBitMask:
                case ERavenParameterType.EnumTrigger:
                case ERavenParameterType.Int:
                case ERavenParameterType.IntTrigger:
                    return typeof(int);

                case ERavenParameterType.Float:
                    return typeof(float);

                case ERavenParameterType.Vector4:
                    return typeof(Vector4);

                case ERavenParameterType.Gradient:
                    return typeof(Gradient);
            }

            return typeof(void);
        }

        public bool CanBeAssignedTo(Type t) {
            if (t == null) {
                return false;
            }

            var type = GetValueType();
            if (type == typeof(Vector4)) {
                return t == typeof(Vector2) || t == typeof(Vector3) || t == typeof(Vector4) || t == typeof(Quaternion) || t == typeof(Color) || t == typeof(Rect);
            }

            if (type == typeof(UnityEngine.Object)) {
                return t.IsValueType || t.IsSubclassOf(typeof(UnityEngine.Object)) || t == typeof(UnityEngine.Object);
            }

            return t.IsAssignableFrom(type);
        }
    }
}