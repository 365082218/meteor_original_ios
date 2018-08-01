//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace Outfit7.Util {

    /// <summary>
    /// Collection utilities.
    /// Partly copied from Spring.Core.Util.CollectionUtils, Spring.Core.Util.Generic.CollectionUtils and Spring.Core.Util.ObjectUtils.
    /// </summary>
    public static class CollectionUtils {

        /// <summary>
        /// Determines whether the specified collection is null or empty.
        /// </summary>
        /// <param name="enumerable">The collection to check.</param>
        /// <returns>
        ///  <c>true</c> if the specified collection is empty or null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty<T>(IEnumerable<T> enumerable) {
            if (enumerable == null) {
                return true;
            }

            if (enumerable is ICollection<T>) {
                return ((enumerable as ICollection<T>).Count == 0);
            }

            IEnumerator<T> it = enumerable.GetEnumerator();
            if (!it.MoveNext()) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified collection is null or empty.
        /// </summary>
        /// <param name="collection">The collection to check.</param>
        /// <returns>
        ///  <c>true</c> if the specified collection is empty or null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty<T>(ICollection<T> collection) {
            return (collection == null || collection.Count == 0);
        }

        /// <summary>
        /// Determines whether the specified dictionary is null or empty.
        /// </summary>
        /// <param name="dictionary">The dictionary to check.</param>
        /// <returns>
        ///  <c>true</c> if the specified dictionary is empty or null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty<T, V>(IDictionary<T, V> dictionary) {
            return (dictionary == null || dictionary.Count == 0);
        }

        /// <param name="collection">The collection to check.</param>
        /// <returns>The number of elements in the collection or 0 if <c>null</c>.</returns>
        public static int Count<T>(ICollection<T> collection) {
            return (collection == null) ? 0 : collection.Count;
        }

        /// <param name="dictionary">The dictionary to check.</param>
        /// <returns>The number of elements in the dictionary or 0 if <c>null</c>.</returns>
        public static int Count<T, V>(IDictionary<T, V> dictionary) {
            return (dictionary == null) ? 0 : dictionary.Count;
        }

        /// <summary>
        /// Checks if the given array or collection has elements and none of the elements is null.
        /// </summary>
        /// <param name="collection">the collection to be checked.</param>
        /// <returns>true if the collection has a length and contains only non-null elements.</returns>
        public static bool HasElements<T>(ICollection<T> collection) {
            if (IsEmpty(collection)) {
                return false;
            }

            IEnumerator<T> it = collection.GetEnumerator();
            while (it.MoveNext()) {
                if (it.Current == null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determine whether a given collection only contains
        /// a single unique object
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool HasUniqueObject<T>(ICollection<T> collection) {
            if (IsEmpty(collection)) {
                return false;
            }

            T candidate = default(T);
            foreach (T elem in collection) {
                if (candidate == null) {
                    candidate = elem;
                } else if (!candidate.Equals(elem)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines whether the <paramref name="enumerable"/> contains the specified <paramref name="element"/>.
        /// </summary>
        /// <param name="enumerable">The enumerable to check.</param>
        /// <param name="element">The object to locate in the enumerable.</param>
        /// <returns><see lang="true"/> if the element is in the enumerable, <see lang="false"/> otherwise.</returns>
        public static bool Contains<T>(IEnumerable<T> enumerable, T element) {
            if (enumerable == null) {
                return false;
            }

            if (enumerable is ICollection<T>) {
                return (enumerable as ICollection<T>).Contains(element);
            }

            foreach (T item in enumerable) {
                if (object.Equals(item, element)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the <paramref name="collection"/> contains the specified <paramref name="element"/>.
        /// </summary>
        /// <param name="collection">The collection to check.</param>
        /// <param name="element">The object to locate in the collection.</param>
        /// <returns><see lang="true"/> if the element is in the collection, <see lang="false"/> otherwise.</returns>
        public static bool Contains<T>(ICollection<T> collection, T element) {
            return (collection != null && collection.Contains(element));
        }

        /// <summary>
        /// Determines whether the collection contains all the elements in the specified collection.
        /// </summary>
        /// <param name="targetCollection">The collection to check.</param>
        /// <param name="sourceCollection">Collection whose elements would be checked for containment.</param>
        /// <returns>true if the target collection contains all the elements of the specified collection.</returns>
        public static bool ContainsAll<T>(ICollection<T> targetCollection, ICollection<T> sourceCollection) {
            if (targetCollection == null) {
                throw new ArgumentNullException("targetCollection", "Collection must not be null.");
            }
            if (sourceCollection == null) {
                throw new ArgumentNullException("sourceCollection", "Collection must not be null.");
            }
            if (sourceCollection == targetCollection)
                return true;
            if (sourceCollection.Count == 0 && targetCollection.Count > 1)
                return true;

            foreach (T c in sourceCollection) {
                if (!targetCollection.Contains(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether one collection contains all the elements in the other collection.
        /// </summary>
        /// <param name="firstCollection">The first collection to check.</param>
        /// <param name="secondCollection">The second collection to check.</param>
        /// <returns>true if the one collection contains all the elements of the other collection and vice versa.</returns>
        public static bool EqualsAll<T>(ICollection<T> firstCollection, ICollection<T> secondCollection) {
            if (firstCollection == secondCollection)
                return true;
            if (firstCollection == null && secondCollection != null)
                return false;
            if (firstCollection != null && secondCollection == null)
                return false;
            if (firstCollection.Count == 0 && secondCollection.Count == 0)
                return true;
            if (firstCollection.Count != secondCollection.Count)
                return false;

            if (firstCollection is HashSet<T>) {
                return (firstCollection as HashSet<T>).SetEquals(secondCollection);
            }
            if (secondCollection is HashSet<T>) {
                return (secondCollection as HashSet<T>).SetEquals(firstCollection);
            }

            return ContainsAll(firstCollection, secondCollection) && ContainsAll(secondCollection, firstCollection);
        }

        // This method does not work on some low performance iOS devices like iPod 4 & iPad 1.
        // It does not crash, but does not remove all items, like NOP. The problem might lie in generics since non-generic foreach works.
        // Tested and examined by two guys for two days.
        //        /// <summary>
        //        /// Removes all the elements from the target collection that are contained in the source collection.
        //        /// </summary>
        //        /// <param name="targetCollection">Collection where the elements will be removed.</param>
        //        /// <param name="sourceCollection">Elements to remove from the target collection.</param>
        //        public static void RemoveAll<T>(ICollection<T> targetCollection, ICollection<T> sourceCollection) {
        //            if (targetCollection == null) {
        //                throw new ArgumentNullException("targetCollection", "Collection cannot be null.");
        //            }
        //            if (sourceCollection == null) {
        //                throw new ArgumentNullException("sourceCollection", "Collection cannot be null.");
        //            }
        //
        //            foreach (T element in sourceCollection) {
        //                if (targetCollection.Contains(element)) {
        //                    targetCollection.Remove(element);
        //                }
        //            }
        //        }

        /// <summary>
        /// Returns the first element contained in both, <paramref name="source"/> and <paramref name="candidates"/>.
        /// </summary>
        /// <remarks>The implementation assumes that <paramref name="candidates"/> &lt;&lt;&lt; <paramref name="source"/></remarks>
        /// <param name="source">the source enumerable. may be <c>null</c></param>
        /// <param name="candidates">the list of candidates to match against <paramref name="source"/> elements. may be <c>null</c></param>
        /// <returns>the first element found in both enumerables or <c>null</c></returns>
        public static T FindFirstMatch<T>(IEnumerable<T> source, IEnumerable<T> candidates) {
            if (IsEmpty(source) || IsEmpty(candidates)) {
                return default(T);
            }

            foreach (T sourceElement in source) {
                if (Contains(candidates, sourceElement)) {
                    return sourceElement;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Adds all elements from source to target.
        /// </summary>
        /// <param name="target">the target collection to add elements to</param>
        /// <param name="source">the source collection to get elements from (<c>null</c> permitted)</param>
        public static void AddAll<T>(ICollection<T> target, IEnumerable<T> source) {
            if (source == null)
                return;

            foreach (T item in source) {
                target.Add(item);
            }
        }

        /// <summary>
        /// Shuffle the specified list.
        /// </summary>
        public static void Shuffle<T>(IList<T> list) {
            // Based on http://en.wikipedia.org/wiki/Fisher-Yates_shuffle
            // Copied & adapted from http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp
            Random random = new Random();
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Returns the first element in the supplied <paramref name="enumerator"/>.
        /// </summary>
        /// <param name="enumerator">
        /// The <see cref="System.Collections.IEnumerator"/> to use to enumerate
        /// elements.
        /// </param>
        /// <returns>
        /// The first element in the supplied <paramref name="enumerator"/>.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If the supplied <paramref name="enumerator"/> did not have any elements.
        /// </exception>
        public static T EnumerateFirstElement<T>(IEnumerator<T> enumerator) {
            return EnumerateElementAtIndex(enumerator, 0);
        }

        /// <summary>
        /// Returns the first element in the supplied <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">
        /// The <see cref="System.Collections.IEnumerable"/> to use to enumerate
        /// elements.
        /// </param>
        /// <returns>
        /// The first element in the supplied <paramref name="enumerable"/>.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// If the supplied <paramref name="enumerable"/> did not have any elements.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="enumerable"/> is <see langword="null"/>.
        /// </exception>
        public static T EnumerateFirstElement<T>(IEnumerable<T> enumerable) {
            if (enumerable == null) {
                throw new ArgumentNullException("enumerable", "Enumerable must not be null.");
            }
            return EnumerateElementAtIndex(enumerable.GetEnumerator(), 0);
        }

        /// <summary>
        /// Returns the element at the specified index using the supplied
        /// <paramref name="enumerator"/>.
        /// </summary>
        /// <param name="enumerator">
        /// The <see cref="System.Collections.IEnumerator"/> to use to enumerate
        /// elements until the supplied <paramref name="index"/> is reached.
        /// </param>
        /// <param name="index">
        /// The index of the element in the enumeration to return.
        /// </param>
        /// <returns>
        /// The element at the specified index using the supplied
        /// <paramref name="enumerator"/>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the supplied <paramref name="index"/> was less than zero, or the
        /// supplied <paramref name="enumerator"/> did not contain enough elements
        /// to be able to reach the supplied <paramref name="index"/>.
        /// </exception>
        public static T EnumerateElementAtIndex<T>(IEnumerator<T> enumerator, int index) {
            if (index < 0) {
                throw new ArgumentOutOfRangeException();
            }
            T element = default(T);
            int i = 0;
            while (enumerator.MoveNext()) {
                element = enumerator.Current;
                if (++i > index) {
                    break;
                }
            }
            if (i < index) {
                throw new ArgumentOutOfRangeException();
            }
            return element;
        }

        /// <summary>
        /// Returns the element at the specified index using the supplied
        /// <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">
        /// The <see cref="System.Collections.IEnumerable"/> to use to enumerate
        /// elements until the supplied <paramref name="index"/> is reached.
        /// </param>
        /// <param name="index">
        /// The index of the element in the enumeration to return.
        /// </param>
        /// <returns>
        /// The element at the specified index using the supplied
        /// <paramref name="enumerable"/>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If the supplied <paramref name="index"/> was less than zero, or the
        /// supplied <paramref name="enumerable"/> did not contain enough elements
        /// to be able to reach the supplied <paramref name="index"/>.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="enumerable"/> is <see langword="null"/>.
        /// </exception>
        public static T EnumerateElementAtIndex<T>(IEnumerable<T> enumerable, int index) {
            if (enumerable == null) {
                throw new ArgumentNullException("enumerable", "Enumerable must not be null.");
            }
            return EnumerateElementAtIndex(enumerable.GetEnumerator(), index);
        }
    }
}
