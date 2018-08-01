//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Util;

namespace Outfit7.Threading.Task {

    /// <summary>
    /// Provides a wrapper around another task feedback. All feedbacks are posted through executor.
    /// </summary>
    public class ExecutorTaskFeedbackWrapper<V> : TaskFeedback<V> {

        public override Action OnStart {
            get {
                return base.OnStart;
            }
            protected set {
                base.OnStart = delegate {
                    Executor.Post(value);
                };
            }
        }

        public override Action OnCancel {
            get {
                return base.OnCancel;
            }
            protected set {
                base.OnCancel = delegate {
                    Executor.Post(value);
                };
            }
        }

        public override Action<V> OnFinish {
            get {
                return base.OnFinish;
            }
            protected set {
                base.OnFinish = delegate(V result) {
                    Executor.Post(delegate {
                        value(result);
                    });
                };
            }
        }

        public override Action<Exception> OnError {
            get {
                return base.OnError;
            }
            protected set {
                base.OnError = delegate(Exception e) {
                    Executor.Post(delegate {
                        value(e);
                    });
                };
            }
        }

        public virtual Executor Executor { get; protected set; }

        public ExecutorTaskFeedbackWrapper(TaskFeedback<V> feedback) : this(feedback, new Executor()) {
        }

        public ExecutorTaskFeedbackWrapper(TaskFeedback<V> feedback, Executor executor) : base(feedback) {
            Assert.NotNull(executor, "executor");
            Executor = executor;
        }
    }
}
