//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Util;
using Outfit7.Util.Collection;

namespace Outfit7.Common.Promo.Creatives.Images {

    /// <summary>
    /// A pool of promo creative image caches identified by the file paths.
    /// Thread safe.
    /// </summary>
    public class PromoCreativeImageCachePool {

        protected readonly object Lock;
        protected readonly Dictionary<string, PromoCreativeImageCache> FilePathCaches;

        public PromoCreativeImageCachePool() {
            Lock = new object();
            FilePathCaches = new Dictionary<string, PromoCreativeImageCache>();
        }

        public virtual PromoCreativeImageCache this[string filePath] {
            get {
                return Get(filePath);
            }
            set {
                Set(filePath, value);
            }
        }

        public virtual PromoCreativeImageCache Get(string filePath) {
            lock (Lock) {
                return FilePathCaches.GetValue(filePath, null);
            }
        }

        public virtual void Set(string filePath, PromoCreativeImageCache cache) {
            Assert.HasText(filePath, "filePath");
            Assert.NotNull(cache, "cache");

            lock (Lock) {
                FilePathCaches[filePath] = cache;
            }
        }

        public virtual PromoCreativeImageCache GetOrCreate(string filePath) {
            lock (Lock) {
                PromoCreativeImageCache cache = Get(filePath);
                if (cache == null) {
                    cache = Create(filePath);
                    Set(filePath, cache);
                }
                return cache;
            }
        }

        protected virtual PromoCreativeImageCache Create(string filePath) {
            return new PromoCreativeImageCache(filePath);
        }
    }
}
