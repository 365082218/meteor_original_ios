//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Util;

namespace Outfit7.Threading.Task {

    /// <summary>
    /// Provides feedback on long running task.
    /// </summary>
    public class TaskFeedback<V> {

        public virtual Action OnStart { get; protected set; }

        public virtual Action OnCancel { get; protected set; }

        public virtual Action<V> OnFinish { get; protected set; }

        public virtual Action<Exception> OnError { get; protected set; }

        public TaskFeedback(TaskFeedback<V> feedback) : this(feedback.OnStart, feedback.OnCancel, feedback.OnFinish, feedback.OnError) {
        }

        public TaskFeedback(Action onCancel, Action<V> onFinish, Action<Exception> onError) : this(null, onCancel, onFinish, onError) {
        }

        public TaskFeedback(Action onStart, Action onCancel, Action<V> onFinish, Action<Exception> onError) {
            Assert.NotNull(onCancel, "onCancel");
            Assert.NotNull(onFinish, "onFinish");
            Assert.NotNull(onError, "onError");

            if (onStart == null) {
                OnStart = delegate {
                };
            } else {
                OnStart = onStart;
            }
            OnCancel = onCancel;
            OnFinish = onFinish;
            OnError = onError;
        }
    }
}
