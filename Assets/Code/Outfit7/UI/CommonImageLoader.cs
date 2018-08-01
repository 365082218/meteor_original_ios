using UnityEngine;
using UnityEngine.UI;
using Outfit7.Util;
using System;

namespace Outfit7.UI {
    public class CommonImageLoader : WebImageLoader {

        private const string WebUrlProof = "://";

        protected override string Tag {
            get {
                return "CommonImageLoader";
            }
        }

        [SerializeField] protected RawImage Image;
        [SerializeField] protected Texture2D DefaultTexture;

        protected bool IsLocalUrl(string url) { 
            return !string.IsNullOrEmpty(url) && !url.Contains(WebUrlProof); 
        }

        public RawImage GetImage() {
            return Image;
        }

        public override void Load(string url, Action<CachedImage> onTextureDownloaded = null, Action<CachedImage, CachedImage.DownloadStatus> onTextureErrorLoading = null) {
            // If it's a local url, load it from resources
            if (IsLocalUrl(url)) {
                Texture2D texture = Resources.Load(url, typeof(Texture2D)) as Texture2D;
                if (texture == null) {
                    O7Log.WarnT(Tag, "There was no texture in resources folder on path {0}! Setting default texture...", url);
                    SetTexture(DefaultTexture);
                } else {
                    SetTexture(texture);
                }
                return;
            }
            base.Load(url, onTextureDownloaded, onTextureErrorLoading);
        }

        protected override void SetTexture(Texture2D texture) {
            #if UNITY_EDITOR
            if (!Application.isPlaying || !IsDestroyed()) {
                Image.texture = texture;
            }
            #else
            if (!IsDestroyed()) {
                Image.texture = texture;
            }
            #endif
        }

        public override void SetDefaultTexture() {
            if (DefaultTexture != null) {
                SetTexture(DefaultTexture);
            }
        }

#if UNITY_EDITOR
        public void SetRawImage(RawImage rawImage) {
            Image = rawImage;
        }

        public void SetDefaultTexture(Texture2D texture) {
            DefaultTexture = texture;
            SetDefaultTexture();
        }
#endif
    }
}
