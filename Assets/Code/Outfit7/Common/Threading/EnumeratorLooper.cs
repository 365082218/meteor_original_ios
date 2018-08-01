//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Util;

namespace Outfit7.Threading {

    /// <summary>
    /// Class used to loop through a queue of enumerators and the current enumerator itself in a single threaded meaning.
    /// </summary>
    public sealed class EnumeratorLooper {

        /// <summary>
        /// Internal looper task.
        /// </summary>
        private struct LooperTask {

            public IEnumerator<Null> Enumerator;
            public DateTime When;

            public LooperTask(IEnumerator<Null> enumerator, DateTime when) {
                Enumerator = enumerator;
                When = when.ToUniversalTime();
            }
        }

        private readonly LinkedList<LooperTask> TaskQueue = new LinkedList<LooperTask>();
        private LooperTask? CurrentTask;

        /// <summary>
        /// Enumerates once the current enumerator or goes to the next in the queue.
        /// If next enumerator is scheduled in the past or exactly now, it is being taken as current and enumerated once.
        /// If next enumerator is scheduled in the future, it remains in the queue for the next loop.
        /// </summary>
        /// <returns>
        /// <code>false</code> if the queue is empty.
        /// </returns>
        public bool LoopOnce() {
            // Take current task or take first in queue
            LooperTask task;
            if (CurrentTask.HasValue) { // Current task is being run
                task = CurrentTask.Value;

            } else { // No current task
                if (TaskQueue.Count == 0) {
                    // Empty queue
                    return false;
                }

                // Take first task (has minimum when)
                task = TaskQueue.First.Value;

                if (task.When > DateTime.UtcNow) {
                    // Delayed after now, skip for now
                    return true;
                }
            }

            // Save current task, because it can be removed from queue before completion
            CurrentTask = task;

            // Enumerate
            bool moreWork = task.Enumerator.MoveNext();
            if (moreWork) {
                // Enumerator has more work to do
                return true;
            }

            // Task is finished
            CurrentTask = null;
            TaskQueue.Remove(task); // May not be in taskQueue anymore if it was removed with RemoveAllSchedules

            return TaskQueue.Count != 0;
        }

        /// <summary>
        /// Schedule the given enumerator to be started enumerating at the given time.
        /// Note that the enumerator that is currently being enumerated does get finished before this enumerator
        /// is started even if the given time is now or in the past.
        /// </summary>
        /// <param name='enumerator'>
        /// The enumerator to be run (null not permitted).
        /// </param>
        /// <param name='when'>
        /// The time when the enumerator should be started enumerating.
        /// </param>
        public void Schedule(IEnumerator<Null> enumerator, DateTime when) {
            Assert.NotNull(enumerator, "enumerator");

            // Insert new enumerator in queue to be sorted by when
            LooperTask task = new LooperTask(enumerator, when);
            LinkedListNode<LooperTask> node = TaskQueue.First;
            while (node != null) {
                if (task.When < node.Value.When) {
                    TaskQueue.AddBefore(node, task);
                    return;
                }

                node = node.Next;
            }

            TaskQueue.AddLast(task);
        }

        /// <summary>
        /// Removes all schedules of the specified enumerator. Does not break the enumerator that is currently being enumerated.
        /// </summary>
        /// <returns>
        /// The number of schedules of the specified enumerator that were removed from the queue.
        /// </returns>
        /// <param name='enumerator'>
        /// The enumerator of schedules to be removed (null not permitted).
        /// </param>
        public int RemoveAllSchedules(IEnumerator<Null> enumerator) {
            Assert.NotNull(enumerator, "enumerator");

            int removedCount = 0;
            LinkedListNode<LooperTask> node = TaskQueue.First;
            while (node != null) {
                LooperTask task = node.Value;

                // Remove node if enumerator is found
                if (task.Enumerator == enumerator) {
                    LinkedListNode<LooperTask> nodeToRemove = node;
                    node = node.Next;
                    TaskQueue.Remove(nodeToRemove);
                    removedCount++;

                } else {
                    node = node.Next;
                }
            }

            return removedCount;
        }
    }
}
