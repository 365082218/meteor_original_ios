//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Util;

namespace Outfit7.Threading {

    /// <summary>
    /// Class used to run a queue of actions usually for one thread.
    /// </summary>
    public sealed class Looper {

        private const string Tag = "Looper";
        private static readonly object Guard = new object();

        // Different looper instance for each thread
        [ThreadStatic]
        private static Looper looper;

        /// <value>
        /// <c>true</c> if looper does exist for the current thread.
        /// </value>
        public static bool DoesLooperExistForCurrentThread {
            get {
                return looper != null;
            }
        }

        /// <summary>
        /// Gets the looper for current thread or creates one.
        /// </summary>
        /// <value>
        /// The instance for current thread.
        /// </value>
        public static Looper ThreadInstance {
            get {
                lock (Guard) {
                    if (looper == null) {
                        // New thread - attach new looper to it
                        O7Log.InfoT(Tag, "Creating new on thread id={0}", O7Thread.CurrentThreadId);
                        looper = new Looper();
                    }
                    return looper;
                }
            }
        }

        /// <summary>
        /// Internal looper task.
        /// </summary>
        private struct LooperTask {

            public Action Action;
            public DateTime When;

            public LooperTask(Action action, DateTime when) {
                this.Action = action;
                this.When = when.ToUniversalTime();
            }
        }

        private readonly object Locker = new object();
        private readonly LinkedList<LooperTask> TaskQueue = new LinkedList<LooperTask>();
        private readonly List<LooperTask> ReadyTasks = new List<LooperTask>();

        /// <summary>
        /// Loops the queue of actions once. All actions, which times are scheduled in the past or exactly now,
        /// are being run. Others remain in the queue for the next loop.
        /// Don't call this method concurrently from more threads.
        /// </summary>
        /// <returns>
        /// The number of actions that were run in this loop. >= 0.
        /// </returns>
        public int LoopOnce() {
            // This is simple read-only operation and is harmless if task queue is changed outside this thread.
            // Most of the time task queue is empty so this avoids thread locking for better performance.
            if (TaskQueue.Count == 0) return 0;

            // Go through all tasks from the beginning (FIFO) and run actions if not delayed after now

            // First, fill separate list of tasks to run, so that actions can schedule new actions to or remove other
            // actions from the queue
            if (ReadyTasks.Count > 0) {
                throw new InvalidOperationException("Concurrent call is not allowed");
            }
            lock (Locker) {
                DateTime now = DateTime.UtcNow;
                LinkedListNode<LooperTask> node = TaskQueue.First;
                while (node != null) {
                    LooperTask task = node.Value;

                    if (task.When <= now) {
                        ReadyTasks.Add(task);

                        LinkedListNode<LooperTask> nodeToRemove = node;
                        node = node.Next;
                        TaskQueue.Remove(nodeToRemove);

                    } else {
                        node = node.Next;
                    }
                }
            }

            int count = ReadyTasks.Count;
            if (count == 0) return 0;

            // Now, run actions
            for (int i = 0; i < count; i++) {
                ReadyTasks[i].Action();
            }

            ReadyTasks.Clear();

            return count;
        }

        /// <summary>
        /// Schedule the given action to be run at the given time.
        /// </summary>
        /// <param name='action'>
        /// The action to be run (null not permitted).
        /// </param>
        /// <param name='when'>
        /// The time when the action should be run.
        /// </param>
        /// <param name='atFrontQueue'>
        /// <c>true</c> to schedule the action in front of all others.
        /// </param>
        public void Schedule(Action action, DateTime when, bool atFrontQueue) {
            Assert.NotNull(action, "action");

            lock (Locker) {
                LooperTask task = new LooperTask(action, when);
                if (atFrontQueue) {
                    TaskQueue.AddFirst(task);
                } else {
                    TaskQueue.AddLast(task);
                }
            }
        }

        /// <summary>
        /// Removes all schedules of the specified action.
        /// </summary>
        /// <returns>
        /// The number of schedules of the specified action that were removed from the queue.
        /// </returns>
        /// <param name='action'>
        /// The action of schedules to be removed (null not permitted).
        /// </param>
        public int RemoveAllSchedules(Action action) {
            Assert.NotNull(action, "action");

            lock (Locker) {
                int removedCount = 0;
                LinkedListNode<LooperTask> node = TaskQueue.First;
                while (node != null) {
                    LooperTask task = node.Value;

                    // Remove node if action is found
                    if (task.Action == action) {
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
}
