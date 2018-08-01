using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("wrot")]
    [SequencerQuickSearchDisplayAttribute("World Rotation")]
    [SequencerPropertyAttribute("Transform/World Rotation")]
    public class WorldRotationPropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(Transform);
            base.OnInit(property, data);
        }

        public override Type GetComponentType() {
            return typeof(Transform);
        }


        public override float OnDrawGui(float indent, float offset, List<Parameter> parameters) {
            DrawVector3ApplyField(new Rect(indent + 60, offset, 60f, 15f));
            return base.OnDrawGui(indent, offset, parameters);
        }

        public override string Name() {
            return "World Rotation";
        }
    }
}