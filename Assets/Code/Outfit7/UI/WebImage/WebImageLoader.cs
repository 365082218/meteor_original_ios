using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Outfit7.UI {
    public abstract class WebImageLoader : UIBehaviour {

        public string Url { get; private set; }

        protected virtual string Tag {
            get {
                return "WebImageLoader";
            }
        }

        protected virtual bool LoadTexture(Action<CachedImage> onTextureDownloaded = null, Action<CachedImage, CachedImage.DownloadStatus> onTextureErrorLoading = null) {
            if (string.IsNullOrEmpty(Url)) {
                return false;
            }

            return WebImageManager.Load(this, onTextureDownloaded, onTextureErrorLoading);
        }

        public virtual void Load(string url, Action<CachedImage> onTextureDownloaded = null, Action<CachedImage, CachedImage.DownloadStatus> onTextureErrorLoading = null) {
            WebImageManager.Remove(this);
            Url = url;
            if (!LoadTexture(onTextureDownloaded, onTextureErrorLoading)) {
                SetDefaultTexture();
            }
        }

        public void SetTextureFromCachedImage(Texture2D texture) {
            SetTexture(texture);
        }

        public abstract void SetDefaultTexture();

        protected abstract void SetTexture(Texture2D texture);

        protected override void OnDisable() {
            base.OnDisable();

            WebImageManager.Remove(this);
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            WebImageManager.Remove(this);
        }
    }
}
