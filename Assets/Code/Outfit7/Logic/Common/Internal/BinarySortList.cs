using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Logic.Internal {

    public class BinarySortList<T> : List<T> {
        public BinarySortList() {
        }

        public BinarySortList(int size) : base(size) {
        }

        public void AddSorted(T item, IComparer<T> comparer) {
            int index = this.BinarySearch(item, comparer);
            if (index < 0) {
                index = ~index;
            }
            this.Insert(index, item);
        }

        public bool RemoveSorted(T item, IComparer<T> comparer) {
            int index = this.BinarySearch(item, comparer);
            if (index < 0) {
                return false;
            }
            this.RemoveAt(index);
            return true;
        }

        public void AddSorted(T item) {
            int index = this.BinarySearch(item);
            if (index < 0) {
                index = ~index;
            }
            this.Insert(index, item);
        }

        public bool RemoveSorted(T item) {
            int index = this.BinarySearch(item);
            if (index < 0) {
                return false;
            }
            this.RemoveAt(index);
            return true;
        }
    }

}