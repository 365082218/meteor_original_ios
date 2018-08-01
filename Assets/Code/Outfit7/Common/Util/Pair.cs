//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

namespace Outfit7.Util {

    /// <summary>
    /// Pair of two objects
    /// </summary>
    public class Pair<F, S> {

        public F First { get; set; }

        public S Second { get; set; }

        public Pair() {
        }

        public Pair(F first, S second) {
            this.First = first;
            this.Second = second;
        }

        public override string ToString() {
            return string.Format("[Pair: First={0}, Second={1}]", First, Second);
        }
    }
}

