using UnityEngine;
using System.Collections.Generic;
using System;
using Outfit7.Util.Io;
using Outfit7.Util;

namespace Outfit7.UI {
    public class CachedImage {

        private const string Tag = "CachedImage";
        private const float FailErrorTime = 0.5f;

        public enum DownloadStatus {
            Done,
            NotDone,
            Error,
            FormatNotSupported,
        }

        public CachedImage(string cachePath, string url, ThreadPriority threadPriority, bool createTexture) {
            Url = url;
            EncryptedUrl = CryptoUtils.Sha1(url);
            CacheFilePath = cachePath + "/" + EncryptedUrl;
            if (createTexture) {
                Texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            }
            Www = null;
            WebImageLoaders = new List<WebImageLoader>();
            ThreadPriority = threadPriority;

            // Could a be parameter, but then I would need to save the timespan also
            if (BuildConfig.IsDevel) {
                CacheExpiry = TimeSpan.FromMinutes(2);
            } else {
                CacheExpiry = TimeSpan.FromDays(7);
            }
        }

        /// <summary>
        /// Non zero values enable cache expiry
        /// </summary>
        /// <value>Time to keep files</value>
        public TimeSpan CacheExpiry { get; private set; }

        public string CacheFilePath { get; private set; }

        public string Url { get; private set; }

        public string EncryptedUrl { get; private set; }

        public Texture2D Texture { get; private set; }

        public byte[] Bytes { get; set; }

        public WWW Www { get; private set; }

        public List<WebImageLoader> WebImageLoaders { get; private set; }

        public ThreadPriority ThreadPriority { get; private set; }

        public float ErrorTimer { get; set; }

        public Action<CachedImage> OnTextureDownloaded;
        public Action<CachedImage, DownloadStatus> OnTextureErrorLoading;

        public void InitWWW() {
            Www = new WWW(Url);
            Www.threadPriority = ThreadPriority;
        }

        public DownloadStatus IsTextureDownloaded() {
            if (!Www.isDone) {
                return DownloadStatus.NotDone;
            }

            if (!StringUtils.IsNullOrEmpty(Www.error)) {
                if (BuildConfig.IsProdOrDevel) {
                    O7Log.WarnT(Tag, "Image {0} download failed with error  {1}", Url, Www.error);
                }
                Www = null;
                ErrorTimer = FailErrorTime;
                if (OnTextureErrorLoading != null) {
                    OnTextureErrorLoading(this, DownloadStatus.Error);
                }
                return DownloadStatus.Error;
            }

            if (Www.textureNonReadable == null) {
                if (BuildConfig.IsProdOrDevel) {
                    O7Log.WarnT(Tag, "Image {0} download failed - texture was null", Url);
                }
                ErrorTimer = FailErrorTime;
                if (OnTextureErrorLoading != null) {
                    OnTextureErrorLoading(this, DownloadStatus.Error);
                }
                return DownloadStatus.Error;
            }

            if (IsGif(Www)) {
                if (BuildConfig.IsProdOrDevel) {
                    O7Log.WarnT(Tag, "Image {0} download failed - texture is a gif", Url);
                }
                ErrorTimer = FailErrorTime;
                if (OnTextureErrorLoading != null) {
                    OnTextureErrorLoading(this, DownloadStatus.FormatNotSupported);
                }
                return DownloadStatus.FormatNotSupported;
            }

            if (BuildConfig.IsProdOrDevel) {
                O7Log.DebugT(Tag, "({0}B) WWW of image {1} done loading", Www.bytesDownloaded, Url);
            }

            Texture = Www.textureNonReadable;

            if (OnTextureDownloaded == null) {
                for (int i = 0; i < WebImageLoaders.Count; i++) {
                    WebImageLoaders[i].SetTextureFromCachedImage(Texture);
                }
            } else {
                OnTextureDownloaded(this);
            }
            Bytes = Www.bytes;
            Www = null;

            return DownloadStatus.Done;
        }

        protected bool IsGif(WWW www) {
            return www.bytes.Length > 2 && www.bytes[0] == 'G' && www.bytes[1] == 'I' && www.bytes[2] == 'F';
        }

        public void SetTextureToWebImageLoader(WebImageLoader webImageLoader) {
            if (Texture == null) {
                O7Log.WarnT(Tag, "CachedImage with url {0} doesnt have the Texture", webImageLoader.Url);
                return;
            }

            if (!WebImageLoaders.Contains(webImageLoader)) {
                O7Log.WarnT(Tag, "CachedImage doesnt have this webImageLoader with link: {0}", webImageLoader.Url);
                return;
            }
            if (BuildConfig.IsProdOrDevel) {
                O7Log.DebugT(Tag, "Loading cached image: {0}", Url);
            }
            webImageLoader.SetTextureFromCachedImage(Texture);
        }

        public void AddImageLoader(WebImageLoader imageLoader) {
            if (!WebImageLoaders.Contains(imageLoader)) {
                WebImageLoaders.Add(imageLoader);
            }
        }

        public void RemoveImageLoader(WebImageLoader imageLoader) {
            int index = WebImageLoaders.IndexOf(imageLoader);
            if (index > -1) {
                WebImageLoaders.RemoveAt(index);
            }
        }

        public bool HasExpired() {
            if (!O7File.Exists(CacheFilePath)) {
                O7Log.DebugT(Tag, "HasExpired(): File does not exist");
                return false;
            }

            if (CacheExpiry == TimeSpan.Zero) {
                O7Log.DebugT(Tag, "HasExpired(): Content refresh disabled");
                return false; // do not refresh
            }

            DateTime writeTime = O7File.GetLastWriteTime(CacheFilePath).ToUniversalTime();
            DateTime now = DateTime.UtcNow;
            DateTime fileExpiryTime = writeTime.Add(CacheExpiry);

            if (now > fileExpiryTime) {
                O7Log.DebugT(Tag, "HasExpired(): File outdated time now {0}, file writted {1} file expiry {2}", now, writeTime, fileExpiryTime);
                return true; // needs to be deleted
            }

            return false;
        }

        public void DeleteImage() {
            string path = CacheFilePath;
            try {
                O7File.Delete(path);
            } catch {
                O7Log.Debug("Could not delete image on path {0}", path);
            }
        }

        public bool LoadTextureFromBytes() {
            if (Www == null && Bytes != null && Bytes.Length > 0) {
                Texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                return Texture.LoadImage(Bytes);
            }

            return false;
        }

        public void UnloadTexture() {
            if (WebImageLoaders.Count == 0) {
                Texture = null;
            }
            OnTextureDownloaded = null;
        }
    }
}
