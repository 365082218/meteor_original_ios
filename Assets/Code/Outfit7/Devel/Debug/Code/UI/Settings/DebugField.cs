//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System;

namespace Outfit7.Devel.O7Debug {

#if STRIP_DBG_SETTINGS
    [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
    [AttributeUsage(AttributeTargets.Field)]
    public class DebugField : Attribute {
        public string FieldName;

        public float StartF = 0f;
        public float EndF = 1.0f;

        public DebugField() {

        }

        public DebugField(string name) {
            FieldName = name;
        }

        public DebugField(int start, int end) : this(start, end, "") {
        }

        public DebugField(int start, int end, string name) : this((float) start, (float) end, name) {
        }

        public DebugField(float start, float end) : this(start, end, "") {
        }

        public DebugField(float start, float end, string name) {
            StartF = start;
            EndF = end;
            FieldName = name;
        }

    }

}
