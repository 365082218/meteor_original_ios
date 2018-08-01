using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Outfit7.Logic.StateMachineInternal {

    public static class ParameterFieldView {
        private static void CalculateRect(Rect rect, ParameterField field, bool vertical, out Rect paramRect, out Rect valueRect) {
            paramRect = rect;
            valueRect = rect;
            if (field.ParameterIndex == -1) {
                paramRect = new Rect(rect.x, rect.y, rect.width, rect.height / 2);
                valueRect = new Rect(rect.x, rect.y + rect.height / 2, rect.width, rect.height / 2);
            }
        }

        private static void DrawParam(Rect paramRect, ParameterField field, string[] paramNames, List<Parameter> parameters, int numberOfFieldTypes) {
            int index;
            if (StateMachineParameterEditor.DrawParameter(paramRect, field.Parameter, paramNames, out index, (int) field.FieldType)) {
                if (index < numberOfFieldTypes) {
                    field.FieldType = (ParameterFieldType) index;
                    field.ParameterIndex = -1;
                    field.Parameter = null;
                } else {
                    index = parameters.FindIndex(x => paramNames[index] == x.Name);
                    field.ParameterIndex = index;
                    field.Parameter = parameters[index];
                }
            }
        }

        public static void DrawParameterField(Rect rect, ParameterFloatField field, List<Parameter> parameters) {
            string[] typeNames = System.Enum.GetNames(typeof(ParameterFieldType));
            string[] paramNames = StateMachineParameterEditor.GetParameterNames(parameters, x => x.ParameterType == ParameterType.Float, typeNames);
            Rect topRect;
            Rect bottomRect;
            CalculateRect(rect, field, true, out topRect, out bottomRect);
            DrawParam(topRect, field as ParameterField, paramNames, parameters, typeNames.Length);
            if (field.ParameterIndex == -1) {
                if (field.FieldType == ParameterFieldType.SOLID) {
                    field.Value = EditorGUI.FloatField(bottomRect, field.Value);
                } else if (field.FieldType == ParameterFieldType.RANGE) {
                    field.MinValue = EditorGUI.FloatField(new Rect(bottomRect.x, bottomRect.y, bottomRect.width / 2, bottomRect.height), field.MinValue);
                    field.MaxValue = EditorGUI.FloatField(new Rect(bottomRect.x + bottomRect.width / 2, bottomRect.y, bottomRect.width / 2, bottomRect.height), field.MaxValue);
                }
            }
        }

        public static void DrawParameterField(Rect rect, ParameterIntField field, List<Parameter> parameters) {
            string[] typeNames = System.Enum.GetNames(typeof(ParameterFieldType));
            string[] paramNames = StateMachineParameterEditor.GetParameterNames(parameters, x => x.ParameterType == ParameterType.Int, typeNames);
            Rect topRect;
            Rect bottomRect;
            CalculateRect(rect, field, true, out topRect, out bottomRect);
            DrawParam(topRect, field as ParameterField, paramNames, parameters, typeNames.Length);
            if (field.ParameterIndex == -1) {
                if (field.FieldType == ParameterFieldType.SOLID) {
                    field.Value = EditorGUI.IntField(bottomRect, field.Value);
                } else if (field.FieldType == ParameterFieldType.RANGE) {
                    field.MinValue = EditorGUI.IntField(new Rect(bottomRect.x, bottomRect.y, bottomRect.width / 2, bottomRect.height), field.MinValue);
                    field.MaxValue = EditorGUI.IntField(new Rect(bottomRect.x + bottomRect.width / 2, bottomRect.y, bottomRect.width / 2, bottomRect.height), field.MaxValue);
                }
            }
        }

        public static void DrawParameterField(Rect rect, ParameterBoolField field, List<Parameter> parameters) {
            string[] typeNames = System.Enum.GetNames(typeof(ParameterFieldType));
            string[] paramNames = StateMachineParameterEditor.GetParameterNames(parameters, x => x.ParameterType == ParameterType.Bool, typeNames);
            Rect topRect;
            Rect bottomRect;
            CalculateRect(rect, field, true, out topRect, out bottomRect);
            DrawParam(topRect, field as ParameterField, paramNames, parameters, typeNames.Length);
            if (field.ParameterIndex == -1) {
                if (field.FieldType == ParameterFieldType.SOLID) {
                    field.Value = EditorGUI.Toggle(bottomRect, "", field.Value);
                } else if (field.FieldType == ParameterFieldType.RANGE) {
                    EditorGUI.LabelField(bottomRect, "RND");
                }
            } 
        }

        public static void DrawParameterField(Rect rect, ParameterVectorField field, List<Parameter> parameters, int rows = 4) {
            bool[] rowValues = new bool[]{ false, false, false, false };
            for (int i = 0; i < rows; i++)
                rowValues[i] = true;

            DrawParameterField(rect, field, parameters, rowValues);
        }

        public static void DrawParameterField(Rect rect, ParameterVectorField field, List<Parameter> parameters, bool[] rows) {
            string[] typeNames = System.Enum.GetNames(typeof(ParameterFieldType));
            string[] paramNames = StateMachineParameterEditor.GetParameterNames(parameters, x => x.ParameterType == ParameterType.Vector4, typeNames);
            Rect topRect;
            Rect xRect;
            Rect yRect;
            Rect zRect;
            Rect wRect;

            topRect = new Rect(rect.x, rect.y, rect.width, rect.height / 5);
            xRect = new Rect(rect.x, rect.y + (rect.height / 5), rect.width, rect.height / 5);
            yRect = new Rect(rect.x, rect.y + (rect.height / 5) * 2, rect.width, rect.height / 5);
            zRect = new Rect(rect.x, rect.y + (rect.height / 5) * 3, rect.width, rect.height / 5);
            wRect = new Rect(rect.x, rect.y + (rect.height / 5) * 4, rect.width, rect.height / 5);
            DrawParam(topRect, field as ParameterField, paramNames, parameters, typeNames.Length);
            if (field.ParameterIndex == -1) {
                if (field.FieldType == ParameterFieldType.SOLID) {
                    if (rows[0]) {
                        field.ValueX = EditorGUI.FloatField(xRect, "", field.ValueX);
                    }
                    if (rows[1]) {
                        field.ValueY = EditorGUI.FloatField(yRect, "", field.ValueY);
                    }
                    if (rows[2]) {
                        field.ValueZ = EditorGUI.FloatField(zRect, "", field.ValueZ);
                    }
                    if (rows[3]) {
                        field.ValueW = EditorGUI.FloatField(wRect, "", field.ValueW);
                    }
                } else if (field.FieldType == ParameterFieldType.RANGE) {
                    if (rows[0]) {
                        field.MinValue.x = EditorGUI.FloatField(new Rect(xRect.x, xRect.y, xRect.width / 2, xRect.height), field.MinValue.x);
                        field.MaxValue.x = EditorGUI.FloatField(new Rect(xRect.x + xRect.width / 2, xRect.y, xRect.width / 2, xRect.height), field.MaxValue.x);
                    }
                    if (rows[1]) {
                        field.MinValue.y = EditorGUI.FloatField(new Rect(yRect.x, yRect.y, yRect.width / 2, yRect.height), field.MinValue.y);
                        field.MaxValue.y = EditorGUI.FloatField(new Rect(yRect.x + yRect.width / 2, yRect.y, yRect.width / 2, yRect.height), field.MaxValue.y);
                    }
                    if (rows[2]) {
                        field.MinValue.z = EditorGUI.FloatField(new Rect(zRect.x, zRect.y, zRect.width / 2, zRect.height), field.MinValue.z);
                        field.MaxValue.z = EditorGUI.FloatField(new Rect(zRect.x + zRect.width / 2, zRect.y, zRect.width / 2, zRect.height), field.MaxValue.z);
                    }
                    if (rows[3]) {
                        field.MinValue.w = EditorGUI.FloatField(new Rect(wRect.x, wRect.y, wRect.width / 2, wRect.height), field.MinValue.w);
                        field.MaxValue.w = EditorGUI.FloatField(new Rect(wRect.x + wRect.width / 2, wRect.y, wRect.width / 2, wRect.height), field.MaxValue.w);
                    }
                }
            }
        }

        public static void DrawParameterFieldAsColor(Rect rect, ParameterVectorField field, List<Parameter> parameters, bool[] rows) {
            string[] typeNames = System.Enum.GetNames(typeof(ParameterFieldType));
            string[] paramNames = StateMachineParameterEditor.GetParameterNames(parameters, x => x.ParameterType == ParameterType.Vector4, typeNames);
            Rect topRect;
            Rect xRect;
            Rect yRect;

            topRect = new Rect(rect.x, rect.y, rect.width, rect.height / 5);
            xRect = new Rect(rect.x, rect.y + (rect.height / 5), rect.width, rect.height / 5);
            yRect = new Rect(rect.x, rect.y + (rect.height / 5) * 2, rect.width, rect.height / 5);
            DrawParam(topRect, field as ParameterField, paramNames, parameters, typeNames.Length);
            if (field.ParameterIndex == -1) {
                if (field.FieldType == ParameterFieldType.SOLID) {
                    Color c = new Color(field.ValueX, field.ValueY, field.ValueZ, field.ValueW);
                    c = EditorGUI.ColorField(xRect, "", c);
                    field.ValueX = c.r;
                    field.ValueY = c.g;
                    field.ValueZ = c.b;
                    field.ValueW = c.a;

                } else if (field.FieldType == ParameterFieldType.RANGE) {

                    Color c1 = new Color(field.ValueX, field.ValueY, field.ValueZ, field.ValueW);
                    c1 = EditorGUI.ColorField(xRect, "", c1);
                    field.MinValue.x = c1.r;
                    field.MinValue.y = c1.g;
                    field.MinValue.z = c1.b;
                    field.MinValue.w = c1.a;


                    Color c2 = new Color(field.ValueX, field.ValueY, field.ValueZ, field.ValueW);
                    c2 = EditorGUI.ColorField(yRect, "", c2);
                    field.MaxValue.x = c2.r;
                    field.MaxValue.y = c2.g;
                    field.MaxValue.z = c2.b;
                    field.MaxValue.w = c2.a;
                }
            }
        }

        public static void DrawParameterField(Rect rect, ParameterComponentField field, Type T, List<Parameter> parameters) {
            string[] typeNames = System.Enum.GetNames(typeof(ParameterFieldType));
            string[] paramNames = StateMachineParameterEditor.GetParameterNames(parameters, x => x.ParameterType == ParameterType.Component, typeNames);
            Rect topRect;
            Rect bottomRect;
            CalculateRect(rect, field, true, out topRect, out bottomRect);
            DrawParam(topRect, field as ParameterField, paramNames, parameters, typeNames.Length);
            if (field.ParameterIndex == -1) {
                field.Value = (Component) EditorGUI.ObjectField(new Rect(bottomRect.x, bottomRect.y, bottomRect.width, bottomRect.height), field.Value, T, true);
            }
        }

        public static void DrawParameterField(Rect rect, ParameterComponentListField field, List<Parameter> parameters) {
            string[] typeNames = System.Enum.GetNames(typeof(ParameterFieldType));
            string[] paramNames = StateMachineParameterEditor.GetParameterNames(parameters, x => x.ParameterType == ParameterType.ComponentList, typeNames);
            Rect topRect;
            Rect bottomRect;
            CalculateRect(rect, field, true, out topRect, out bottomRect);
            DrawParam(topRect, field as ParameterField, paramNames, parameters, typeNames.Length);
        }
    }

}
