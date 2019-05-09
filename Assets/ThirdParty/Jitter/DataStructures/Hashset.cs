﻿/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/


// The .NET framework 2.0 doesn't provide a hashset
// so we fake it here by using the Dictionary object.

#if NET_20 || WINDOWS_PHONE || XBOX

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace System.Collections.Generic
{
    public class HashSet<T> : ICollection<T>
    {
        private readonly Dictionary<T, object> dict;

        public HashSet()
        {
            dict = new Dictionary<T, object>();
        }

        public HashSet(IEnumerable<T> items)
            : this()
        {
            if (items == null)
            {
                return;
            }

            foreach (T item in items)
            {
                Add(item);
            }
        }

        public HashSet<T> NullSet { get { return new HashSet<T>(); } }

#region ICollection<T> Members

        public void Add(T item)
        {
            if (null == item)
            {
                throw new ArgumentNullException("item");
            }

            dict[item] = null;
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            dict.Clear();
        }

        public bool Contains(T item)
        {
            return dict.ContainsKey(item);
        }

        /// <summary>
        /// Copies the items of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the items copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-<paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.-or-The number of items in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type T cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0 || arrayIndex >= array.Length || arrayIndex >= Count)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }

            dict.Keys.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public bool Remove(T item)
        {
            return dict.Remove(item);
        }

        /// <summary>
        /// Gets the number of items contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of items contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return dict.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

#endregion

        public HashSet<T> Union(HashSet<T> set)
        {
            HashSet<T> unionSet = new HashSet<T>(this);

            if (null == set)
            {
                return unionSet;
            }

            foreach (T item in set)
            {
                if (unionSet.Contains(item))
                {
                    continue;
                }

                unionSet.Add(item);
            }

            return unionSet;
        }

        public HashSet<T> Subtract(HashSet<T> set)
        {
            HashSet<T> subtractSet = new HashSet<T>(this);

            if (null == set)
            {
                return subtractSet;
            }

            foreach (T item in set)
            {
                if (!subtractSet.Contains(item))
                {
                    continue;
                }

                subtractSet.dict.Remove(item);
            }

            return subtractSet;
        }

        public bool IsSubsetOf(HashSet<T> set)
        {
            HashSet<T> setToCompare = set ?? NullSet;

            foreach (T item in this)
            {
                if (!setToCompare.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        public HashSet<T> Intersection(HashSet<T> set)
        {
            HashSet<T> intersectionSet = NullSet;

            if (null == set)
            {
                return intersectionSet;
            }

            foreach (T item in this)
            {
                if (!set.Contains(item))
                {
                    continue;
                }

                intersectionSet.Add(item);
            }

            foreach (T item in set)
            {
                if (!Contains(item) || intersectionSet.Contains(item))
                {
                    continue;
                }

                intersectionSet.Add(item);
            }

            return intersectionSet;
        }

        public bool IsProperSubsetOf(HashSet<T> set)
        {
            HashSet<T> setToCompare = set ?? NullSet;

            // A is a proper subset of a if the b is a subset of a and a != b
            return (IsSubsetOf(setToCompare) && !setToCompare.IsSubsetOf(this));
        }

        public bool IsSupersetOf(HashSet<T> set)
        {
            HashSet<T> setToCompare = set ?? NullSet;

            foreach (T item in setToCompare)
            {
                if (!Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsProperSupersetOf(HashSet<T> set)
        {
            HashSet<T> setToCompare = set ?? NullSet;

            // B is a proper superset of a if b is a superset of a and a != b
            return (IsSupersetOf(setToCompare) && !setToCompare.IsSupersetOf(this));
        }

        public List<T> ToList()
        {
            return new List<T>(this);
        }

        //#region Implementation of ISerializable

        ///// <summary>
        ///// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        ///// </summary>
        ///// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data. </param><param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization. </param><exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    if (info == null) throw new ArgumentNullException("info");
        //    dict.GetObjectData(info, context);
        //}

        //#endregion

        //#region Implementation of IDeserializationCallback

        ///// <summary>
        ///// Runs when the entire object graph has been deserialized.
        ///// </summary>
        ///// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented. </param>
        //public void OnDeserialization(object sender)
        //{
        //    dict.OnDeserialization(sender);
        //}

        //#endregion

#region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.Keys.GetEnumerator();
        }

        public Dictionary<T,object>.KeyCollection.Enumerator GetEnumerator()
        {
            return dict.Keys.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            // note: this causes boxing - it is just here because the
            // IEnumerable<T> contract demands it.
            Debug.Assert(false);
            return dict.Keys.GetEnumerator();
        }

 

#endregion

    }

}
#endif
