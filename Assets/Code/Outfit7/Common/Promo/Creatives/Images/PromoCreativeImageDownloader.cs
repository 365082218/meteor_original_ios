//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Threading;
using Outfit7.Threading.Task;
using Outfit7.Util;
using Outfit7.Web;
using UnityEngine;

namespace Outfit7.Common.Promo.Creatives.Images {

    /// <summary>
    /// Promo creative image downloader.
    /// Thread independent and thus safe.
    /// </summary>
    public class PromoCreativeImageDownloader {

        protected readonly MainExecutor MainExecutor;
        public readonly string Url;

        public PromoCreativeImageDownloader(MainExecutor executor, string url) {
            Assert.NotNull(executor, "executor");
            Assert.HasText(url, "url");
            MainExecutor = executor;
            Url = url;
        }

        public virtual void DownloadDataAsync(TaskFeedback<byte[]> feedback) {
            Assert.NotNull(feedback, "feedback");

            // Create task wrapper only if Looper exists for this thread
            // If Looper exists: this thread is Unity's main or from ThreadExecutor, feedback call is expected to be on this thread
            // If Looper does not exist: this thread is from runtime's thread pool, feedback cannot be handled in same thread
            // Do not call feedback on Unity's main thread if this is not Unity's main thread
            // Also skip creating unnecessary task wrapper if this is Unity's main thread
            TaskFeedback<byte[]> executorFeedback = feedback;
            bool noLooperThread = !MainExecutor.IsMainThread;
            if (noLooperThread && Looper.DoesLooperExistForCurrentThread) {
                executorFeedback = new ExecutorTaskFeedbackWrapper<byte[]>(feedback);
                noLooperThread = false;
            }

            MainExecutor.RunOnMainThread(() =>
                MainExecutor.StartCoroutine(DownloadData(executorFeedback, noLooperThread)));
        }

        protected virtual IEnumerator<WWW> DownloadData(TaskFeedback<byte[]> feedback, bool callBackAsync) {
            if (callBackAsync) {
                SimpleWorker.RunAsync(feedback.OnStart);
            } else {
                feedback.OnStart();
            }

            string url = RestCall.FixUrl(Url);
            WWW request = new WWW(url);
            yield return request;

            if (!StringUtils.IsNullOrEmpty(request.error)) {
                Exception e = new Exception(request.error);
                if (callBackAsync) {
                    SimpleWorker.RunAsync(() => feedback.OnError(e));
                } else {
                    feedback.OnError(e);
                }
                yield break;
            }

            byte[] data = request.bytes;

            if (data == null) {
                Exception e = new Exception("Downloaded data is null");
                if (callBackAsync) {
                    SimpleWorker.RunAsync(() => feedback.OnError(e));
                } else {
                    feedback.OnError(e);
                }
                yield break;
            }

            if (callBackAsync) {
                SimpleWorker.RunAsync(() => feedback.OnFinish(data));
            } else {
                feedback.OnFinish(data);
            }
        }
    }
}
