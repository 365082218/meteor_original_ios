//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System;
using Outfit7.Common.Promo.Creatives.Images;
using Outfit7.Threading.Task;
using Outfit7.Util;

namespace Outfit7.Common.Promo.Creatives {

    /// <summary>
    /// Promo creative handler.
    /// </summary>
    public class PromoCreativeHandler {

        protected const string Tag = "PromoCreativeHandler";

        protected readonly object Lock;
        public readonly PromoCreativeImageHandler BackgroundImageHandler;
        public readonly PromoCreativeImageHandler OverlayImageHandler;
        protected bool PreparingRequested;
        protected bool FirstStarted;
        protected bool FirstCanceled;
        protected bool FirstFinished;
        protected Exception FirstError;

        public PromoCreativeData CreativeData { get; protected set; }

        public virtual byte[] BackgroundImageData {
            get {
                return BackgroundImageHandler.ImageData;
            }
        }

        public virtual byte[] OverlayImageData {
            get {
                return ShouldUseOverlayImage ? OverlayImageHandler.ImageData : null;
            }
        }

        public virtual bool ShouldUseOverlayImage {
            get {
                return (OverlayImageHandler != null);
            }
        }

        public virtual bool IsPreparing {
            get {
                lock (Lock) {
                    // Background or overlay image may already be preparing if it is shared with other creative that is
                    // already preparing.
                    // We need to be sure that both images are being prepared if, for example, only one is shared!
                    // Real logic for this would be complex and not thread-safe, so it is better to simplify it with
                    // PreparingRequested, which guarantees that both images are being prepared.

                    if (IsDone) return false; // Cannot be preparing if both done
                    return PreparingRequested;
                }
            }
        }

        public virtual bool IsDone {
            get {
                lock (Lock) {
                    // Both are done
                    if (!BackgroundImageHandler.IsDone) return false;
                    if (!ShouldUseOverlayImage) return true;
                    return OverlayImageHandler.IsDone;
                }
            }
        }

        public virtual bool IsReady {
            get {
                lock (Lock) {
                    // Both are ready
                    if (!BackgroundImageHandler.IsReady) return false;
                    if (!ShouldUseOverlayImage) return true;
                    return OverlayImageHandler.IsReady;
                }
            }
        }

        public PromoCreativeHandler(PromoCreativeData data, PromoCreativeImageHandlerPool pool, string cacheDirPath) {
            Assert.NotNull(data, "data");
            Lock = new object();
            CreativeData = data;
            BackgroundImageHandler = pool.GetOrCreate(data.BackgroundImageUrl, cacheDirPath);
            if (StringUtils.HasText(data.OverlayImageUrl)) {
                OverlayImageHandler = pool.GetOrCreate(data.OverlayImageUrl, cacheDirPath);
            }
        }

        public virtual void PrepareAsync(TaskFeedback<Null> feedback, bool force) {
            lock (Lock) {
                if (CanPrepare(force)) {
                    Prepare(feedback, force);
                    return;
                }
            }

            if (feedback != null) {
                feedback.OnStart();
                feedback.OnCancel();
            }
        }

        public virtual bool CanPrepare(bool force) {
            lock (Lock) {
                if (IsReady) return false;
                if (force) return true;
                if (IsDone) return false;
                return !PreparingRequested;
            }
        }

        protected virtual void Prepare(TaskFeedback<Null> feedback, bool force) {
            PreparingRequested = true;
            FirstStarted = false;
            FirstCanceled = false;
            FirstFinished = false;
            FirstError = null;

            TaskFeedback<byte[]> backgroundCallback = new TaskFeedback<byte[]>(delegate {
                O7Log.DebugT(Tag, "Started preparing background image {0}: {1}", CreativeData.Id, CreativeData.BackgroundImageUrl);
                CheckStartedAndCallback(feedback);

            }, delegate {
                O7Log.DebugT(Tag, "Canceled preparing background image {0}: {1}", CreativeData.Id, CreativeData.BackgroundImageUrl);
                CheckCanceledAndCallback(feedback);

            }, delegate(byte[] data) {
                O7Log.DebugT(Tag, "Finished preparing background image {0}: {1}", CreativeData.Id, CreativeData.BackgroundImageUrl);
                CheckFinishedAndCallback(feedback);

            }, delegate(Exception e) {
                O7Log.WarnT(Tag, e, "Error preparing background image {0}: {1}", CreativeData.Id, CreativeData.BackgroundImageUrl);
                CheckErrorAndCallback(feedback, e);
            });

            BackgroundImageHandler.PrepareAsync(backgroundCallback, force);

            if (ShouldUseOverlayImage) {
                TaskFeedback<byte[]> overlayCallback = new TaskFeedback<byte[]>(delegate {
                    O7Log.DebugT(Tag, "Started preparing overlay image {0}: {1}", CreativeData.Id, CreativeData.OverlayImageUrl);
                    CheckStartedAndCallback(feedback);

                }, delegate {
                    O7Log.DebugT(Tag, "Canceled preparing overlay image {0}: {1}", CreativeData.Id, CreativeData.OverlayImageUrl);
                    CheckCanceledAndCallback(feedback);

                }, delegate(byte[] data) {
                    O7Log.DebugT(Tag, "Finished preparing overlay image {0}: {1}", CreativeData.Id, CreativeData.OverlayImageUrl);
                    CheckFinishedAndCallback(feedback);

                }, delegate(Exception e) {
                    O7Log.WarnT(Tag, e, "Error preparing overlay image {0}: {1}", CreativeData.Id, CreativeData.OverlayImageUrl);
                    CheckErrorAndCallback(feedback, e);
                });

                OverlayImageHandler.PrepareAsync(overlayCallback, force);
            }
        }

        protected virtual void CheckStartedAndCallback(TaskFeedback<Null> feedback) {
            if (feedback == null) return;

            if (!ShouldUseOverlayImage) {
                // Only one
                feedback.OnStart();
                return;
            }

            // Check if this is first started
            lock (Lock) {
                if (FirstStarted) return;
                FirstStarted = true;
            }

            // First started

            feedback.OnStart();
        }

        protected virtual void CheckCanceledAndCallback(TaskFeedback<Null> feedback) {
            if (feedback == null) return;

            if (!ShouldUseOverlayImage) {
                // Only one
                feedback.OnCancel();
                return;
            }

            // Check if this is first completed
            lock (Lock) {
                if (!FirstCanceled && !FirstFinished && FirstError == null) {
                    FirstCanceled = true;
                    return;
                }
            }

            // Both completed. Determine result. Thread locking not necessary

            if (FirstError != null) {
                feedback.OnError(FirstError);

            } else {
                feedback.OnCancel();
            }
        }

        protected virtual void CheckFinishedAndCallback(TaskFeedback<Null> feedback) {
            if (feedback == null) return;

            if (!ShouldUseOverlayImage) {
                // Only one
                feedback.OnFinish(null);
                return;
            }

            // Check if this is first completed
            lock (Lock) {
                if (!FirstCanceled && !FirstFinished && FirstError == null) {
                    FirstFinished = true;
                    return;
                }
            }

            // Both completed. Determine result. Thread locking not necessary

            if (FirstError != null) {
                feedback.OnError(FirstError);

            } else if (FirstCanceled) {
                feedback.OnCancel();

            } else {
                feedback.OnFinish(null);
            }
        }

        protected virtual void CheckErrorAndCallback(TaskFeedback<Null> feedback, Exception error) {
            if (feedback == null) return;

            if (!ShouldUseOverlayImage) {
                // Only one
                feedback.OnError(error);
                return;
            }

            // Check if this is first completed
            lock (Lock) {
                if (!FirstCanceled && !FirstFinished && FirstError == null) {
                    FirstError = error;
                    return;
                }
            }

            // Both completed. Return first error. Thread locking not necessary

            feedback.OnError(FirstError ?? error);
        }

        public override string ToString() {
            return string.Format("[PromoCreativeHandler: CreativeData={0}, ShouldUseOverlayImage={1}, IsPreparing={2}, IsDone={3}, IsReady={4}]",
                CreativeData, ShouldUseOverlayImage, IsPreparing, IsDone, IsReady);
        }
    }
}
