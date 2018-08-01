using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using Outfit7.Logic.StateMachineInternal;
using UnityEditor;

namespace Outfit7.Sequencer {
    [System.AttributeUsage(System.AttributeTargets.Class,
        AllowMultiple = true)  // Multiuse attribute.
    ]
    public class SequencerPropertyAttribute : System.Attribute {
        string path;

        public SequencerPropertyAttribute(string path) {
            this.path = path;
        }

        public string GetPath() {
            return path;
        }
    }

    public abstract class BasePropertyView {
        BaseProperty Property;
        public Type ComponentType;
        private List<Vector4> SaveVector4 = new List<Vector4>();
        public SequencerTrackView TrackView;

        public void Init(object property, object data, SequencerTrackView trackView) {
            OnInit(property, data);
            TrackView = trackView;
        }

        protected virtual bool ForceExactComponentName() {
            return false;
        }

        public virtual BaseProperty GetProperty() {
            return Property;
        }

        public virtual void OnInit(object property, object data) {
            Property = property as BaseProperty;
        }

        public virtual string Name() {
            return "Property";
        }

        public float DrawGui(float indent, float offset, List<Parameter> parameters) {
            ParameterFieldView.DrawParameterField(new Rect(indent + 135f, offset, 30f, 15f), Property.Components, parameters);
            return OnDrawGui(indent, offset, parameters);
        }

        public virtual float OnDrawGui(float indent, float offset, List<Parameter> parameters) {
            Property.Enabled = GUI.Toggle(new Rect(indent - 20f, offset, 80f, 15f), Property.Enabled, Name());
            return 15f;
        }

        public abstract Type GetComponentType();

        public virtual void Refresh(object actor, SequencerCurveTrackView sequencerCurveTrackView) {
            Property.Components.Clear();
            if (ComponentType == null) {
                Property.Components.Add(null);
                return;
            }

            MethodInfo method = typeof(SequencerCurveTrackView).GetMethod("GetActorComponent");
            method = method.MakeGenericMethod(ComponentType);
            object[] parametersArray = new object[] { actor, ForceExactComponentName() };
            Property.Components.Add((Component) method.Invoke(sequencerCurveTrackView, parametersArray));
        }

        public void RecordingStart() {
            OnRecordingStart();
        }

        public void RecordingStop() {
            OnRecordingStop();
        }

        public virtual void OnRecordingStart() {
            SaveVector4.Clear();
            for (int i = 0; i < Property.Components.Value.Count; i++) {
                bool success;
                SaveVector4.Add(Property.OnValue(Property.Components.Value[i], out success));
            }
        }

        public virtual void OnRecordingStop() {
            for (int i = 0; i < Property.Components.Value.Count; i++) {
                if (i >= SaveVector4.Count)
                    return;
                Property.OnApply(Property.Components.Value[i], SaveVector4[i]);
            }
        }

        public void DrawVector2ApplyField(Rect pos) {
            pos.width = 30f;
            GUILayout.BeginArea(pos);
            GUILayout.BeginHorizontal();
            Property.ApplyX = EditorGUILayout.Toggle(Property.ApplyX);
            Property.ApplyY = EditorGUILayout.Toggle(Property.ApplyY);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        public void DrawVector3ApplyField(Rect pos) {
            pos.width = 45f;
            GUILayout.BeginArea(pos);
            GUILayout.BeginHorizontal();
            Property.ApplyX = EditorGUILayout.Toggle(Property.ApplyX);
            Property.ApplyY = EditorGUILayout.Toggle(Property.ApplyY);
            Property.ApplyZ = EditorGUILayout.Toggle(Property.ApplyZ);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        public void DrawVector4ApplyField(Rect pos) {
            pos.width = 60f;
            GUILayout.BeginArea(pos);
            GUILayout.BeginHorizontal();
            Property.ApplyX = EditorGUILayout.Toggle(Property.ApplyX);
            Property.ApplyY = EditorGUILayout.Toggle(Property.ApplyY);
            Property.ApplyZ = EditorGUILayout.Toggle(Property.ApplyZ);
            Property.ApplyW = EditorGUILayout.Toggle(Property.ApplyW);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}