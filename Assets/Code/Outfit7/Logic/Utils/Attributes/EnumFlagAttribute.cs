using UnityEngine;
using System;

namespace Outfit7.Logic.Util {

    public class EnumFlagAttribute : PropertyAttribute {
        public Type EnumType;

        public EnumFlagAttribute(Type type) {
            EnumType = type;
        }
    }

}