//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System;
using System.IO;
using Outfit7.Threading;
using Outfit7.Threading.Task;
using Outfit7.Util;

namespace Outfit7.Common.Promo.Creatives.Images {

    /// <summary>
    /// Promo creative image handler.
    /// Thread safe.
    /// </summary>
    public class PromoCreativeImageHandler {

        protected const string Tag = "PromoCreativeImageHandler";

        public event Action<PromoCreativeImageHandler> OnPrepareStart;
        public event Action<PromoCreativeImageHandler, Exception> OnCacheReadError;
        public event Action<PromoCreativeImageHandler> OnZeroCacheReadError;
        public event Action<PromoCreativeImageHandler, int, Exception> OnCacheWriteError;
        public event Action<PromoCreativeImageHandler> OnDownloadStart;
        public event Action<PromoCreativeImageHandler, Exception> OnDownloadError;
        public event Action<PromoCreativeImageHandler, bool> OnPrepareFinish;

        protected readonly object Lock;
        protected readonly MainExecutor MainExecutor;
        protected readonly PromoCreativeImageCachePool CachePool;
        protected readonly string CacheDirPath;
        protected readonly string Url;
        protected PromoCreativeImageCache cache;
        protected PromoCreativeImageDownloader downloader;
        protected volatile bool preparing;
        protected volatile bool done;

        public virtual bool IsPreparing {
            get {
                return this.preparing;
            }
        }

        public virtual bool IsDone {
            get {
                return this.done;
            }
        }

        public virtual bool IsReady {
            get {
                return ImageData != null;
            }
        }

        public byte[] ImageData { get; protected set; }

        public PromoCreativeImageHandler(MainExecutor executor, PromoCreativeImageCachePool cachePool,
            string cacheDirPath, string url) {
            Assert.NotNull(executor, "executor");
            Assert.NotNull(cachePool, "executor");
            Assert.HasText(cacheDirPath, "cacheDirPath");
            Assert.HasText(url, "url");
            Lock = new object();
            MainExecutor = executor;
            CachePool = cachePool;
            CacheDirPath = cacheDirPath;
            Url = url;
        }

        public virtual void PrepareAsync(TaskFeedback<byte[]> feedback, bool force) {
            O7Log.VerboseT(Tag, "PrepareAsync (force={0}) - {1}", force, this);

            bool cancel = false;
            lock (Lock) {
                if (this.preparing || (!force && this.done)) {
                    cancel = true;
                    O7Log.DebugT(Tag, "Won't prepare (preparing={0}, done={1}, force={2}) - {3}", this.preparing,
                        this.done, force, this);

                } else {
                    this.preparing = true;
                    this.done = false;
                }
            }

            if (feedback != null) {
                feedback.OnStart();
            }

            if (cancel) {
                if (feedback != null) {
                    feedback.OnCancel();
                }
                return;
            }

            // Create task wrapper only if Looper exists for this thread
            // If Looper exists: this thread is Unity's main or from ThreadExecutor, feedback call is expected to be on this thread
            // If Looper does not exist: this thread is from runtime's thread pool, feedback cannot be handled in same thread
            TaskFeedback<byte[]> executorFeedback = feedback;
            if (Looper.DoesLooperExistForCurrentThread) {
                executorFeedback = new ExecutorTaskFeedbackWrapper<byte[]>(feedback);
            }

            SimpleWorker.RunAsync(() => Prepare(executorFeedback));
        }

        protected virtual void Prepare(TaskFeedback<byte[]> feedback) {
            O7Log.DebugT(Tag, "Preparing started... - {0}", this);
            if (OnPrepareStart != null) {
                OnPrepareStart(this);
            }

            // Try loading from cache first
            bool prepared = TryPrepareFromCache(feedback);
            if (prepared) return;

            // No cache or storage failure. Download it
            O7Log.DebugT(Tag, "Cache not valid, downloading... - {0}", this);

            StartDownloading(feedback);
        }

        protected virtual bool TryPrepareFromCache(TaskFeedback<byte[]> feedback) {
            byte[] data;
            try {
                data = Cache.ReadData();

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Cannot read data from cache {0} - {1}", Cache.FilePath, this);
                if (OnCacheReadError != null) {
                    OnCacheReadError(this, e);
                }
                return false;
            }

            if (data == null) {
                O7Log.DebugT(Tag, "No cache {0} - {1}", Cache.FilePath, this);
                return false;
            }

            if (data.Length == 0) {
                O7Log.WarnT(Tag, "Got zero data from cache {0} - {1}", Cache.FilePath, this);
                if (OnZeroCacheReadError != null) {
                    OnZeroCacheReadError(this);
                }
                return false;
            }

            HandleFinish(feedback, data, true);
            return true;
        }

        protected virtual void StartDownloading(TaskFeedback<byte[]> feedback) {
            TaskFeedback<byte[]> callback = new TaskFeedback<byte[]>(delegate {
                HandleDownloadStart(feedback);

            }, delegate {
                HandleDownloadCancel(feedback);

            }, delegate(byte[] data) {
                HandleDownloadFinish(feedback, data);

            }, delegate(Exception e) {
                HandleDownloadError(feedback, e);
            });
            Downloader.DownloadDataAsync(callback);
        }

        protected virtual void HandleFinish(TaskFeedback<byte[]> feedback, byte[] data, bool fromCache) {
            SetDone(data);

            if (fromCache) {
                O7Log.DebugT(Tag, "Preparing finished. Got data ({0} B) from cache {1} - {2}", data.Length,
                    Cache.FilePath, this);
            } else {
                O7Log.DebugT(Tag, "Preparing finished. Downloaded data ({0} B) & cached to {1} - {2}", data.Length,
                    Cache.FilePath, this);
            }
            if (OnPrepareFinish != null) {
                OnPrepareFinish(this, fromCache);
            }

            if (feedback != null) {
                feedback.OnFinish(data);
            }
        }

        protected virtual void HandleDownloadStart(TaskFeedback<byte[]> feedback) {
            // OnStart feedback is already called

            if (OnDownloadStart != null) {
                OnDownloadStart(this);
            }
        }

        protected virtual void HandleDownloadCancel(TaskFeedback<byte[]> feedback) {
            SetDone(null);

            O7Log.DebugT(Tag, "Preparing canceled. Downloading from {0} - {1}", Url, this);
            if (feedback != null) {
                feedback.OnCancel();
            }
        }

        protected virtual void HandleDownloadFinish(TaskFeedback<byte[]> feedback, byte[] data) {
            if (data.Length == 0) {
                Exception error = new Exception("Downloaded zero data");
                HandleDownloadError(feedback, error);
                return;
            }

            try {
                Cache.WriteData(data);

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Cannot write data ({0} B) to cache {1} - {2}", data.Length, Cache.FilePath, this);
                if (OnCacheWriteError != null) {
                    OnCacheWriteError(this, data.Length, e);
                }
            }

            HandleFinish(feedback, data, false);
        }

        protected virtual void HandleDownloadError(TaskFeedback<byte[]> feedback, Exception error) {
            SetDone(null);

            O7Log.WarnT(Tag, error, "Preparing error. Downloading from {0} - {1}", Url, this);
            if (OnDownloadError != null) {
                OnDownloadError(this, error);
            }

            if (feedback != null) {
                feedback.OnError(error);
            }
        }

        protected virtual void SetDone(byte[] data) {
            lock (Lock) {
                ImageData = data;
                this.preparing = false;
                this.done = true;
            }
        }

        public virtual PromoCreativeImageCache Cache {
            get {
                if (this.cache == null) {
                    string fileName = CryptoUtils.Sha1(Url);
                    string filePath = Path.Combine(CacheDirPath, fileName);
                    this.cache = CachePool.GetOrCreate(filePath);
                }
                return this.cache;
            }
        }

        public virtual PromoCreativeImageDownloader Downloader {
            get {
                if (this.downloader == null) {
                    this.downloader = new PromoCreativeImageDownloader(MainExecutor, Url);
                }
                return this.downloader;
            }
        }

        public override string ToString() {
            return string.Format("[PromoCreativeImageHandler: Url={0}, FilePath={1}, IsPreparing={2}, IsDone={3}, IsReady={4}]",
                Url, Cache.FilePath, this.preparing, this.done, IsReady);
        }
    }
}
