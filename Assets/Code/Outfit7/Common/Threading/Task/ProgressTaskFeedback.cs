//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Util;

namespace Outfit7.Threading.Task {

    /// <summary>
    /// Provides simultaneously feedback on long running task for each progress change.
    /// </summary>
    public class ProgressTaskFeedback<V> : TaskFeedback<V> {

        /// <value>The progress percentage (0-100).</value>
        public virtual Action<int> OnProgressChange { get; set; }

        public ProgressTaskFeedback(ProgressTaskFeedback<V> feedback) : this(feedback.OnStart, feedback.OnProgressChange, feedback.OnCancel, feedback.OnFinish, feedback.OnError) {
        }

        public ProgressTaskFeedback(Action<int> onProgressChange, Action onCancel, Action<V> onFinish, Action<Exception> onError) : this(null, onProgressChange, onCancel, onFinish, onError) {
        }

        public ProgressTaskFeedback(Action onStart, Action<int> onProgressChange, Action onCancel, Action<V> onFinish, Action<Exception> onError) : base(onStart, onCancel, onFinish, onError) {
            Assert.NotNull(onProgressChange, "onProgressChange");
            OnProgressChange = onProgressChange;
        }
    }
}
