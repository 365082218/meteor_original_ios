#if !STARLITE_EDITOR

namespace Starlite {

    public static class SourceCCProperty {

        [System.Flags]
        public enum PropertyAttribute {
            Preload = 1,
            Prefab = 2,
            Event = 4,
            MaskDefault = Preload,
        }

    }
}

#endif