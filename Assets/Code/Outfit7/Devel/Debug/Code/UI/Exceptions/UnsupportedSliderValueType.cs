using System;

namespace Outfit7.Devel.O7Debug.UI.Exceptions {

    public class UnsupportedSliderValueType : Exception {

        public UnsupportedSliderValueType() : base("Unsupported slider's type.") {
        }
    }
}
