using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEditor;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    public class PropertyAspectRatioAnimationDataView : PropertyAnimationDataView {
        public List<PropertyAnimationDataView> AnimationDataViews = new List<PropertyAnimationDataView>();
        PropertyAspectRatioAnimationData AnimationData = null;
        public int SelectedIndex = 0;
        public int firstRender = 0;

        public override void OnInit(object animData) {
            AnimationData = animData as PropertyAspectRatioAnimationData;

            foreach (PropertyAnimationData myAnimData in AnimationData.AnimationDatas) {
                Type t = myAnimData.GetType();
                MethodInfo method = typeof(PropertyAspectRatioAnimationDataView).GetMethod("AddData");
                method = method.MakeGenericMethod(t);
                object[] parametersArray = new object[] { myAnimData };
                method.Invoke(this, parametersArray);
            }

            base.OnInit(animData);
        }

        public override string Name() {
            return "ARAnimation Data";
        }

        public override void OnDrawGui(BaseProperty property, Rect curveRect, List<Parameter> parameters, bool optimizedView, bool hover) {
            //string s = AnimationDataViews[SelectedIndex].ToString();
            AnimationDataViews[SelectedIndex].DrawGui(property, curveRect, parameters, optimizedView);

            //draw selection for aspect ratio
            string[] stringOptions = new string[AnimationDataViews.Count];
            int[] intOptions = new int[AnimationDataViews.Count];

            for (int i = 0; i < AnimationDataViews.Count; i++) {
                stringOptions[i] = AnimationData.AspectRatios[i].ToString();
                intOptions[i] = i;
            }

            int oldIndex = SelectedIndex;
            SelectedIndex = EditorGUI.IntPopup(
                new Rect(curveRect.x + curveRect.width - 60f, curveRect.y, 60f, 10f),
                "", 
                SelectedIndex, stringOptions, intOptions);
            AnimationData.SelectedIndex = SelectedIndex;
            if (oldIndex != SelectedIndex) {
                SequencerSequence sequence = ParentEvent.gameObject.GetComponent<SequencerSequence>();
                ParentEvent.Evaluate(0, sequence.GetCurrentTime());
            }

            //draw add button

            if (GUI.Button(new Rect(curveRect.x + curveRect.width - 80f, curveRect.y, 20f, 15f), "+")) {
                //create new anim data
                Type t = AnimationData.AnimationDatas[SelectedIndex].GetType();
                MethodInfo method = typeof(PropertyAspectRatioAnimationDataView).GetMethod("AddData");
                method = method.MakeGenericMethod(t);
                object[] parametersArray = new object[] { null };
                method.Invoke(this, parametersArray);
            }
        }

        public override void OnUpdateWhileRecording(BaseProperty property, float absoluteTime, float duration, float normalizedTime, float multiplier, float offset, bool remap, float remap0, float remap1) {
            AnimationDataViews[SelectedIndex].OnUpdateWhileRecording(property, absoluteTime, duration, normalizedTime, multiplier, offset, remap, remap0, remap1);
        }

        public virtual bool AddData<T>(object ctrl) where T : Component {
            //if controller comes in at init, it just casts it and creates View for it
            //if function is called to add new, it creates controller first, and then view afterwards
            T controller;
            if (ctrl == null) {
                controller = (T) Undo.AddComponent<T>(AnimationData.gameObject);
                PropertyAnimationData animationData = controller as PropertyAnimationData;
                /*BaseProperty property = AnimationData.AnimationDatas[0].Property as BaseProperty;
                animationData.Property = property;
                bool success;
                animationData.SetStartingValues(property.Value(out success));*/
                AddAnimationData(animationData, null, 1.0f);
            } else
                controller = (T) ctrl;
            Type viewType = Type.GetType("Outfit7.Sequencer." + controller.GetType().Name + "View", true);
            PropertyAnimationDataView instance = Activator.CreateInstance(viewType) as PropertyAnimationDataView;
            instance.Init(controller);
            AnimationDataViews.Add(instance);
            return true;
        }

        public void AddAnimationData(PropertyAnimationData animData, PropertyAnimationDataView animDataView, float aspectRatio) {
            AnimationData.AnimationDatas.Add(animData);
            AnimationData.AspectRatios.Add(aspectRatio);
            if (animDataView != null) {
                AnimationDataViews.Add(animDataView);
            }

        }
    }
}