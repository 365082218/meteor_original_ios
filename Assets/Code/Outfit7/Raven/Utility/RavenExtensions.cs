using System.Collections.Generic;
using UnityEngine;

namespace Starlite.Raven {

    public static class StringExtensions {

        public static string ReplaceFirst(this string text, string search, string replace, out int position, int startIndex = 0) {
            position = text.IndexOf(search, startIndex);
            if (position < 0) {
                return text;
            }
            return text.Substring(0, position) + replace + text.Substring(position + search.Length);
        }
    }

    public static class ListExtensions {

        public static int AddSorted<T>(this List<T> list, T item, IComparer<T> comparer) {
            int index = list.BinarySearch(item, comparer);
            if (index < 0) {
                index = ~index;
            }
            list.Insert(index, item);
            return index;
        }

        public static bool RemoveSorted<T>(this List<T> list, T item, IComparer<T> comparer) {
            int index = list.BinarySearch(item, comparer);
            if (index < 0) {
                return false;
            }
            list.RemoveAt(index);
            return true;
        }

        public static int AddSorted<T>(this List<T> list, T item) {
            int index = list.BinarySearch(item);
            if (index < 0) {
                index = ~index;
            }
            list.Insert(index, item);
            return index;
        }

        public static bool RemoveSorted<T>(this List<T> list, T item) {
            int index = list.BinarySearch(item);
            if (index < 0) {
                return false;
            }
            list.RemoveAt(index);
            return true;
        }
    }

    public static class UnityExtensions {

        public static Vector4 ToVector4(this Quaternion quaternion) {
            return new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public static Quaternion ToQuaternion(this Vector4 vector) {
            return new Quaternion(vector.x, vector.y, vector.z, vector.w);
        }

        public static Vector4 ToVector4(this Rect rect) {
            return new Vector4(rect.xMin, rect.yMin, rect.width, rect.height);
        }

        public static Rect ToRect(this Vector4 vector) {
            return new Rect(vector.x, vector.y, vector.z, vector.w);
        }
    }
}