
using System.Collections.Generic;

namespace Outfit7.Util.Collection {
    public static class IDictionaryExtension {

        /// <summary>
        /// Gets the value indexed by the key or returns the specified default value.
        /// </summary>
        /// <returns>The default.</returns>
        /// <param name="dict">Dict.</param>
        /// <param name="key">Key.</param>
        /// <param name="default">Default.</param>
        /// <typeparam name="K">The 1st type parameter.</typeparam>
        /// <typeparam name="V">The 2nd type parameter.</typeparam>
        public static V GetValue<K,V>(this IDictionary<K,V> dict, K key, V @default) {
            V value;
            if (!dict.TryGetValue(key, out value)) {
                return @default;
            } else {
                return value;
            }
        }
    }
}

