//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util.Io;
using System;
using Outfit7.Util;

namespace Outfit7.UI {

    /// <summary>
    /// Web image manager.
    /// </summary>
    public static class WebImageManager {
        private const string Tag = "WebImageManager";

        private const string UrlsPref = "WebImageManagerUrls";

        private static int ActionLimit = 3;

        // Processed on the go
        private static List<CachedImage> LoadingImages = new List<CachedImage>();
        private static Dictionary<string, CachedImage> UrlCachedImages = new Dictionary<string, CachedImage>();
        // For caching
        private static List<CachedImage> CachedImages = new List<CachedImage>();

        private static string CachePath {
            get {
                return Application.persistentDataPath + "/ImageCache";
            }
        }

        public static void Init(int actionLimit) {
            O7Log.DebugT(Tag, "Init");

            ActionLimit = actionLimit;

            try {
                O7Directory.CreateDirectory(CachePath);
            } catch (Exception e) {
                if (BuildConfig.IsProdOrDevel) {
                    throw e;
                } else {
                    O7Log.WarnT(Tag, e.ToString());
                }
            }
            List<string> urls = new List<string>();
            urls = UserPrefs.GetCollectionAsList(UrlsPref, urls);
            CachedImage cachedImage;

            for (int i = 0; i < urls.Count; i++) {
                try {
                    cachedImage = new CachedImage(CachePath, urls[i], ThreadPriority.Low, true);
                    byte[] bytes = O7File.ReadAllBytes(cachedImage.CacheFilePath);
                    cachedImage.Bytes = bytes;
                    cachedImage.Texture.LoadImage(bytes);
                    UrlCachedImages.Add(urls[i], cachedImage);
                    CachedImages.Add(cachedImage);
                } catch (Exception e) {
                    O7Log.WarnT(Tag, "cannot read preloaded image {0}, exception: {1}", urls[i], e);
                }
            }
        }

        public static void SaveCachedImages() {
            O7Log.DebugT(Tag, "SaveCachedImages");
            string fullPath;
            CachedImage cachedImage;
            List<string> urls = new List<string>();

            for (int i = 0; i < CachedImages.Count; i++) {
                cachedImage = CachedImages[i];
                bool expired = cachedImage.HasExpired();
                if (expired) {
                    cachedImage.DeleteImage();
                    if (cachedImage.Texture != null || cachedImage.Bytes != null && cachedImage.Bytes.Length > 0) {
                        UrlCachedImages.Remove(cachedImage.Url);
                    }
                    continue;
                }
                if ((cachedImage.Texture == null && (cachedImage.Bytes == null || cachedImage.Bytes.Length == 0)) || cachedImage.Www != null) {
                    continue;
                }
                if (urls.Contains(cachedImage.Url)) {
                    continue;
                }
                
                fullPath = cachedImage.CacheFilePath;
                try {
                    if (!O7File.Exists(fullPath)) {
                        O7File.WriteAllBytes(fullPath, cachedImage.Bytes);
                        if (BuildConfig.IsProdOrDevel) {
                            O7Log.DebugT(Tag, "Image saved to: {0}", fullPath);
                        }
                    }
                    urls.Add(cachedImage.Url);
                } catch (Exception e) {
                    O7Log.WarnT(Tag, "cannot save cached image {0}, exception: {1}", fullPath, e);
                }
            }
            UserPrefs.SetCollection(UrlsPref, urls);
        }

        public static void UnloadTextures() {
            for (int i = 0; i < CachedImages.Count; i++) {
                CachedImages[i].UnloadTexture();
            }
        }

        public static void LateUpdate() {
            CachedImage cachedImage;
            int actionLimit = ActionLimit;
            for (int i = 0; i < LoadingImages.Count && actionLimit > 0; i++) {
                cachedImage = LoadingImages[i];

                if (cachedImage.ErrorTimer > 0f) {
                    cachedImage.ErrorTimer -= Time.deltaTime;
                    return;
                }

                if (cachedImage.Www == null) {
                    cachedImage.InitWWW();
                }

                CachedImage.DownloadStatus error = cachedImage.IsTextureDownloaded();
                switch (error) {
                    case CachedImage.DownloadStatus.Done:
                        if (!CachedImages.Contains(LoadingImages[i])) {
                            CachedImages.Add(LoadingImages[i]);
                        }
                        LoadingImages.RemoveAt(i);
                        i--;
                        actionLimit--;
                        break;
                    case CachedImage.DownloadStatus.NotDone:
                        actionLimit--;
                        break;
                    case CachedImage.DownloadStatus.Error:
                        LoadingImages.RemoveAt(i);
                        i--;
                        break;
                    case CachedImage.DownloadStatus.FormatNotSupported:
                        LoadingImages.RemoveAt(i);
                        i--;
                        break;
                }
            }
        }

        public static void Preload(string url, Action<CachedImage> onTextureDownloaded = null) {
            if (!UrlCachedImages.ContainsKey(url)) {
                if (BuildConfig.IsProdOrDevel) {
                    O7Log.DebugT(Tag, "Downloading image: {0}", url);
                }

                CachedImage cachedImage = new CachedImage(CachePath, url, ThreadPriority.Low, false);
                if (onTextureDownloaded != null) {
                    cachedImage.OnTextureDownloaded -= onTextureDownloaded;
                    cachedImage.OnTextureDownloaded += onTextureDownloaded;
                }
                UrlCachedImages.Add(url, cachedImage);
                LoadingImages.Add(cachedImage);
            } else {
                if (onTextureDownloaded != null) {
                    onTextureDownloaded(UrlCachedImages[url]);
                }
            }
        }

        /// <summary>
        /// Load the specified imageLoader.
        /// Returns true if loaded from cache or false if it must be downloaded from the web
        /// </summary>
        /// <param name="imageLoader">Image loader.</param>
        public static bool Load(WebImageLoader imageLoader, Action<CachedImage> onTextureDownloaded = null, Action<CachedImage, CachedImage.DownloadStatus> onTextureErrorLoading = null) {
            CachedImage cachedImage;
            if (UrlCachedImages.ContainsKey(imageLoader.Url)) {
                cachedImage = UrlCachedImages[imageLoader.Url];
                cachedImage.AddImageLoader(imageLoader);
                if (cachedImage.Texture == null) {
                    if (!cachedImage.LoadTextureFromBytes()) {
                        List<WebImageLoader> webImageLoaders = new List<WebImageLoader>(cachedImage.WebImageLoaders);
                        UrlCachedImages.Remove(imageLoader.Url);
                        // Try loading again
                        Preload(imageLoader.Url);
                        cachedImage = UrlCachedImages[imageLoader.Url];
                        if (onTextureDownloaded == null) {
                            for (int i = 0; i < webImageLoaders.Count; i++) {
                                cachedImage.SetTextureToWebImageLoader(webImageLoaders[i]);
                            }
                        } else {
                            cachedImage.OnTextureDownloaded -= onTextureDownloaded;
                            cachedImage.OnTextureDownloaded += onTextureDownloaded;
                        }
                        return false;
                    }
                }
                cachedImage.SetTextureToWebImageLoader(imageLoader);
                return true;
            } else {
                Preload(imageLoader.Url);
                cachedImage = UrlCachedImages[imageLoader.Url];
                if (onTextureDownloaded == null) {
                    cachedImage.AddImageLoader(imageLoader);
                } else {
                    cachedImage.OnTextureDownloaded -= onTextureDownloaded;
                    cachedImage.OnTextureDownloaded += onTextureDownloaded;
                }
                if (onTextureErrorLoading != null) {
                    cachedImage.OnTextureErrorLoading -= onTextureErrorLoading;
                    cachedImage.OnTextureErrorLoading += onTextureErrorLoading;
                }
                return false;
            }
        }

        public static void Remove(WebImageLoader imageLoader) {
            if (!string.IsNullOrEmpty(imageLoader.Url) && UrlCachedImages.ContainsKey(imageLoader.Url)) {
                CachedImage cachedImage = UrlCachedImages[imageLoader.Url];
                cachedImage.RemoveImageLoader(imageLoader);
            }
        }

        public static void Delete(string url) {
            if (!string.IsNullOrEmpty(url)) {
                if (UrlCachedImages.ContainsKey(url)) {
                    UrlCachedImages[url].DeleteImage();
                }
            }
        }
    }
}
