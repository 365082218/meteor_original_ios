//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using SimpleJSON;

namespace Outfit7.Json {

    /// <summary>
    /// SimpleJSON utils.
    /// </summary>
    public static class SimpleJsonUtils {

        public static JSONArray EnsureJsonArray(JSONNode node) {
            if (node == null)
                return null;

            if (node is JSONArray)
                return node as JSONArray;

            // Create array with single node
            JSONArray array = new JSONArray();
            array.Add(node);
            return array;
        }

        public static JSONArray CreateJsonArray(IEnumerable<string> items) {
            if (items == null)
                return null;

            JSONArray array = new JSONArray();
            foreach (string item in items) {
                array.Add(new JSONData(item));
            }
            return array;
        }

        public static HashSet<string> CreateHashSet(JSONArray array) {
            if (array == null)
                return null;

            HashSet<string> strings = new HashSet<string>();
            foreach (JSONNode n in array) {
                strings.Add(n.Value);
            }
            return strings;
        }

        public static HashSet<string> CreateHashSet(JSONNode node) {
            JSONArray array = EnsureJsonArray(node);
            return CreateHashSet(array);
        }

        public static List<string> CreateList(JSONArray array) {
            if (array == null)
                return null;

            List<string> strings = new List<string>(array.Count);
            foreach (JSONNode n in array) {
                strings.Add(n.Value);
            }
            return strings;
        }

        public static List<string> CreateList(JSONNode node) {
            JSONArray array = EnsureJsonArray(node);
            return CreateList(array);
        }
    }
}
