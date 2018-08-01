//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Common;
using Outfit7.Threading;
using Outfit7.Threading.Task;
using Outfit7.Util;
using Outfit7.Util.Io;
using Outfit7.Web;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.AddOn {

    public class AddOnCacheHandler {

        protected const string Tag = "AddOnCacheHandler";
        private const int FetchTimeoutMs = 120 * 1000;
        private const string JsonCacheUrl = "url";
        private const string JsonCacheContent = "content";

        private string fileName;
        private string UserAgentName;
        private JsonFileReaderWriter CacheReaderWriter;
        private JSONNode Cache;

        public string Url { get; private set; }

        public MainExecutor MainExecutor { get; set; }

        public virtual void ClearPrefs() {
            O7File.Delete(CreateFilePath());
        }

        public virtual void Init() {
            // Make sure that previous cache is invalid on app update
            FileName = string.Format("O7AddOns_{0}.json", AppPlugin.AppVersion); // More simple and error-prone as deleting previous cache

            UserAgentName = AppPlugin.UserAgentName;

            CacheReaderWriter = new JsonFileReaderWriter(CreateFilePath());
        }

#region Cache

        protected virtual string CreateFilePath() {
            return Application.persistentDataPath + "/" + FileName;
        }

        public string FileName {
            get {
                return this.fileName;
            }
            set {
                Assert.NotNull(value, "FileName");
                this.fileName = value;
            }
        }

        public virtual JSONNode LoadAddOnsCache() {
            if (Cache == null) {
                // First load
                Cache = CacheReaderWriter.ReadJson();
                if (Cache == null) return null;
            }

            Url = Cache[JsonCacheUrl];
            return Cache[JsonCacheContent];
        }

        public virtual void SaveAddOnsCache(JSONNode cacheJ, string url) {
            if (Cache == cacheJ) return;

            // Create root object to include URL
            JSONClass dataJ = new JSONClass();
            dataJ[JsonCacheUrl] = url;
            dataJ[JsonCacheContent] = cacheJ;

            Cache = dataJ;
            CacheReaderWriter.WriteJson(Cache);
            Url = url;
        }

#endregion

#region Download

        public virtual void DownloadAddOns(string url, TaskFeedback<JSONNode> feedback) {
            ExecutorTaskFeedbackWrapper<JSONNode> executorFeedback = new ExecutorTaskFeedbackWrapper<JSONNode>(feedback);
            SimpleWorker.RunAsync(delegate {
                O7Log.VerboseT(Tag, "Starting to download add-ons from {0}...", url);
                executorFeedback.OnStart();

                // Fetch data
                JSONNode dataJ;
                try {
                    dataJ = FetchAddOns(url);

                } catch (RestCallException e) {
                    O7Log.WarnT(Tag, e, "Error downloading add-ons");
                    executorFeedback.OnError(e);
                    return;
                }

                // Done
                O7Log.VerboseT(Tag, "Done downloading add-ons from {0}", url);
                executorFeedback.OnFinish(dataJ);
            });
        }

        protected virtual JSONNode FetchAddOns(string url) {
            Dictionary<string, string> headers = new Dictionary<string, string> {
                { RestCall.Header.UserAgent, UserAgentName }
            };

            ThreadedRestCall call = new ThreadedRestCall(url, headers);
            call.Start(MainExecutor);
            call.WaitForResponse(FetchTimeoutMs);

            call.CheckForError();

            // Check response but prevent crash in any circumstances, because response is much less important
            JSONNode responseJ;
            try {
                responseJ = JSON.Parse(call.ResponseBody);

            } catch {
                throw new RestCallException("Invalid response body");
            }

            if (responseJ == null) {
                throw new RestCallException("Empty response body");
            }

            return responseJ;
        }

#endregion
    }
}
