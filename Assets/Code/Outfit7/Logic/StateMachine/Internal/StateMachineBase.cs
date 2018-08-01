using System;
using UnityEngine;
using System.Collections.Generic;

namespace Outfit7.Logic.StateMachineInternal {

    public enum ParameterType {
        Int,
        Float,
        Bool,
        Enum,
        BoolTrigger,
        IntTrigger,
        EnumTrigger,
        EnumBitMask,
        Vector4,
        Component,
        ComponentList
    }

    public enum ConditionMode {
        Less,
        LessOrEqual,
        Equal,
        GreaterOrEqual,
        Greater,
        NotEqual,
        BitSet,
        BitNotSet,
        BitMaskSet,
        BitMaskNotSet,
    }

    public enum DebugInfoType {
        None = 0,
        Default,
        Detailed,
        Custom,
        CustomNoGUI,
    }

    [Serializable]
    public class Base {
        public string Name;
    }

}