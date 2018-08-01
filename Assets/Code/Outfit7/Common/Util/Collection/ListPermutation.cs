//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Outfit7.Util.Collection {

    /// <summary>
    /// List permutation class. A helper class that provides permutation-like access to it's items.
    /// </summary>
    public class ListPermutation<T> : IEnumerable<T> {

#region Class variables

        private readonly List<T> items;

        private int currentIndex = -1;

        private bool shuffleItemsWhenEndReached = true;
        private bool endlessPermutation = true;
        private bool ensureEndAndStartItemDontRepeat = true;

#endregion

#region Properties

        public bool ShuffleItemsWhenEndReached {
            get { return shuffleItemsWhenEndReached; }
            set { shuffleItemsWhenEndReached = value; }
        }

        public bool IsEndlessPermutation {
            get { return endlessPermutation; }
            set { endlessPermutation = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Outfit7.Util.Collection.Permutation`1"/> ensure that end
        /// and start item don't repeat on (re)suffle.
        /// </summary>
        /// <value><c>true</c> if ensure end and start item don't repeat; otherwise, <c>false</c>.</value>
        public bool EnsureEndAndStartItemDontRepeat {
            get { return ensureEndAndStartItemDontRepeat; }
            set { ensureEndAndStartItemDontRepeat = value; }
        }

        /// <summary><para>
        /// Gets the current item. You can't get the current item unless MoveToNextItem() has been called at least once
        /// (since creation or Restart())!</para>
        /// <para></para>
        /// <para>
        /// CurrentItem is usualy retrieved by "MoveToNextItem().CurrentItem;"</para>
        /// </summary>
        /// <value>The current item.</value>
        public T CurrentItem {
            get {
                if (currentIndex == -1) {
                    throw new Exception("currentIndex == -1. You have to call MoveToNextItem() at least once after intialization, reset or shuffling.");
                }

                return items[currentIndex];
            }
        }

        public int Count { get { return items.Count; } }

        public bool IsEmpty { get { return items.Count == 0; } }

#endregion

#region Indexer

        public T this[int index] {
            get { return items[index]; }
            set { items[index] = value; }
        }

#endregion

#region Class constructors

        public ListPermutation() {
            items = new List<T>();
        }

        public ListPermutation(IEnumerable<T> collection) {
            items = new List<T>(collection);
        }

#endregion

#region IEnumerable implementation

        public IEnumerator<T> GetEnumerator() {
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

#endregion

        /// <summary>
        /// Just reset the currentIndex so that the "MoveToNextItem().CurrentItem;" start returning items from the start
        /// of the permutation (this doesn't shuffle the permutation).
        /// </summary>
        public void Restart() {
            currentIndex = -1;
        }

        public void Clear() {
            items.Clear();
            Restart();
        }

        public void Shuffle() {
            if (IsEmpty) {
                return;
            }

            T previousLastItem = default(T);
            if (ensureEndAndStartItemDontRepeat) {
                previousLastItem = items[items.Count - 1];
            }

            CollectionUtils.Shuffle(items);
            Restart();

            if (ensureEndAndStartItemDontRepeat && previousLastItem.Equals(items[0])) {
                items[0] = items[items.Count - 1];
                items[items.Count - 1] = previousLastItem;
            }
        }

        /// <summary>
        /// Adds the item to the end of the permutation. If Shuffle() isn't called, the permutation might remain "sorted".
        /// </summary>
        /// <param name="item">Item.</param>
        public void AddItem(T item) {
            items.Add(item);
        }

        /// <summary>
        /// Adds the item to a random position. You might loose the reference to your CurrentItem!
        /// </summary>
        /// <param name="item">Item.</param>
        public void AddItemRandomly(T item) {
            items.Insert(UnityEngine.Random.Range(0, items.Count), item);
        }

        /// <summary>
        /// Removes the item. You might loose the reference to your CurrentItem!
        /// </summary>
        /// <returns><c>true</c>, if item was removed, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        public bool RemoveItem(T item) {
            return items.Remove(item);
        }

        /// <summary>
        /// Moves to next item but does not retrieve it. To retrieve the next item call "MoveToNextItem().CurrentItem".
        /// </summary>
        /// <returns>The to next item.</returns>
        public ListPermutation<T> MoveToNextItem() {
            if (IsEmpty) {
                return null;
            }

            currentIndex++;

            if (endlessPermutation) {
                int previousIndex = currentIndex;
                currentIndex %= items.Count;

                // if currentIndex was reset to 0 (by 'currentIndex % items.Count') (re)shuffle items
                if (currentIndex != previousIndex && shuffleItemsWhenEndReached) {
                    Shuffle();
                    currentIndex++; // Shuffle() resets the index to -1
                }
            } else if (currentIndex >= items.Count) {
                return null; // signal end reached in non-endless permutation
            }

            return this;
        }

        /// <summary>
        /// Lists all items in the permutation in a single (miltiline) string (unually needed for debugging).
        /// </summary>
        /// <returns>The items.</returns>
        public string ListItems() {
            StringBuilder builder = new StringBuilder();
            foreach (var item in items) {
                builder.AppendFormat("{0},\n", item);
            }
            return builder.ToString();
        }
    }
}
