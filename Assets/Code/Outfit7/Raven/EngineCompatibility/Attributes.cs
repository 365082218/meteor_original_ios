#if !STARLITE_EDITOR

using System;

namespace Starlite {
    public class ObjectClassAttribute : Attribute {
        public string ccName;

        public ObjectClassAttribute(string _ccName) {
            ccName = _ccName;
        }
    }

    public class ObjectMethodAttribute : Attribute {
    }

    public class ObjectPropertyAttribute : Attribute {
        public SourceCCProperty.PropertyAttribute PropertyAttributes = 0;

        public ObjectPropertyAttribute(SourceCCProperty.PropertyAttribute attributes) {
            PropertyAttributes = attributes;
        }
    }
}

#endif