//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using Outfit7.Event;
using Outfit7.Util;

namespace Outfit7.Video {

    /// <summary>
    /// Video clip manager.
    /// </summary>
    public class VideoClipManager {

        protected const string Tag = "VideoClipManager";
        protected VideoClipData videoClipData;

        public Action<string, int> VideoClipCompletionCallback { get; set; }

        public VideoClipPlugin VideoClipPlugin { get; set; }

        public EventBus EventBus { get; set; }

        public virtual VideoClipData VideoClipData {
            get {
                return this.videoClipData;
            }
            internal set {
                if (this.videoClipData == value)
                {
                    O7Log.Error("this.videoClipData == value, dont fireevent CommonEvents.VIDEO_CLIP_AVAILABILITY_CHANGE");
                    return;
                }
                this.videoClipData = value;
                O7Log.Error("EventBus.FireEvent(CommonEvents.VIDEO_CLIP_AVAILABILITY_CHANGE, value)");
                EventBus.FireEvent(CommonEvents.VIDEO_CLIP_AVAILABILITY_CHANGE, value);
            }
        }

        public virtual void Init() {
            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, OnGridChange);
        }

        public void StartLoadingVideoClip() {
            VideoClipPlugin.StartLoadingVideoClip();
        }

        public virtual void ShowVideoClip() {
            Assert.State(VideoClipData != null, "Video clip is not available");
            VideoClipPlugin.StartShowingVideoClip();
            VideoClipData = null;
        }

        internal virtual void OnVideoClipCompletion(string id, int amount) {
            if (VideoClipCompletionCallback == null) {
                O7Log.WarnT(Tag, "Got video clip {0} with reward {1}, but there is no completion callback", id, amount);
                return;
            }

            VideoClipCompletionCallback(id, amount);
        }

        public virtual void OnAppStartOrResume() {
            StartLoadingVideoClip();
        }

        protected virtual void OnGridChange(object eventData) {
            StartLoadingVideoClip();
        }
    }
}
