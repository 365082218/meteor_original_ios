//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Util;

namespace Outfit7.Util.Collection {

    /// <summary>
    /// Circular buffer data structure is a backed by the linked list.
    /// </summary>
    public class CircularBuffer<T> : ICollection<T> {

        private readonly LinkedList<T> List;

        public int MaxSize { get; private set; }

        public CircularBuffer(int maxSize) : this(null, maxSize) {
        }

        public CircularBuffer(ICollection<T> buffer, int maxSize) {
            Assert.IsTrue(maxSize > 0, "maxSize must be > 0");
            MaxSize = maxSize;
            List = (buffer == null) ? new LinkedList<T>() : new LinkedList<T>(buffer);
            MaintainMaxSize();
        }

        private void MaintainMaxSize() {
            while (List.Count > MaxSize) {
                List.RemoveFirst();
            }
        }

        public void Add(T item) {
            List.AddLast(item);
            MaintainMaxSize();
        }

        public void CopyTo(T[] array, int arrayIndex) {
            List.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            return List.Remove(item);
        }

        public void Clear() {
            List.Clear();
        }

        public bool Contains(T item) {
            return List.Contains(item);
        }

        public int Count {
            get {
                return List.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            return List.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return List.GetEnumerator();
        }

        public override bool Equals(object obj) {
            return List.Equals(obj);
        }

        public override int GetHashCode() {
            return List.GetHashCode();
        }

        public override string ToString() {
            return string.Format("[CircularBuffer: {0}]", StringUtils.CollectionToCommaDelimitedString(List));
        }
    }
}

