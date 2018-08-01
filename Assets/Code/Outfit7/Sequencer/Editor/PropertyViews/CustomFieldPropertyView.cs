using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Outfit7.Sequencer {
    public class CustomFieldPropertyView : BasePropertyView {
        CustomFieldProperty Property;

        public override void OnInit(object property, object data) {
            Property = property as CustomFieldProperty;
            if (data != null) {
                object[] dataArray = (object[]) data;
                Type componentType = (Type) dataArray[0];
                Property.ComponentName = componentType.FullName;

                FieldInfo fieldInfo = (FieldInfo) dataArray[1];
                Property.FieldName = fieldInfo.Name;
            }
            base.OnInit(property, data);
        }

        public override string Name() {
            return Property.FieldName;
        }

        public override Type GetComponentType() {
            return typeof(Transform);
        }

        public override void Refresh(object actor, SequencerCurveTrackView sequencerCurveTrackView) {
            if (Property.ComponentName == "")
                return;
            Property.Components.Clear();
            Assembly assembly = typeof(CustomFieldProperty).Assembly;
            ComponentType = assembly.GetType(Property.ComponentName, false);

            if (ComponentType == null) {
                Property.Components.Add(null);
                return;
            }

            MethodInfo method = typeof(SequencerCurveTrackView).GetMethod("GetActorComponent");
            method = method.MakeGenericMethod(ComponentType);
            object[] parametersArray = new object[] { actor, false };
            Property.Components.Add((Component) method.Invoke(sequencerCurveTrackView, parametersArray));
        }

    }
}