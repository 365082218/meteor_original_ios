//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Threading;
using Outfit7.Util;
using Outfit7.Util.Collection;

namespace Outfit7.Common.Promo.Creatives.Images {

    /// <summary>
    /// A pool of promo creative image handlers identified by the image URLs.
    /// Thread safe.
    /// </summary>
    public class PromoCreativeImageHandlerPool {

        protected readonly object Lock;
        protected readonly Dictionary<string, PromoCreativeImageHandler> UrlHandlers;

        public MainExecutor MainExecutor { get; set; }

        public PromoCreativeImageCachePool CachePool { get; set; }

        public PromoCreativeImageHandlerPool() {
            Lock = new object();
            UrlHandlers = new Dictionary<string, PromoCreativeImageHandler>();
        }

        public virtual PromoCreativeImageHandler this[string url] {
            get {
                return Get(url);
            }
            set {
                Set(url, value);
            }
        }

        public virtual PromoCreativeImageHandler Get(string url) {
            lock (Lock) {
                return UrlHandlers.GetValue(url, null);
            }
        }

        public virtual void Set(string url, PromoCreativeImageHandler handler) {
            Assert.HasText(url, "url");
            Assert.NotNull(handler, "handler");

            lock (Lock) {
                UrlHandlers[url] = handler;
            }
        }

        public virtual PromoCreativeImageHandler GetOrCreate(string url, string cacheDirPath) {
            lock (Lock) {
                PromoCreativeImageHandler handler = Get(url);
                if (handler == null) {
                    handler = Create(url, cacheDirPath);
                    Set(url, handler);
                }
                return handler;
            }
        }

        protected virtual PromoCreativeImageHandler Create(string url, string cacheDirPath) {
            return new PromoCreativeImageHandler(MainExecutor, CachePool, cacheDirPath, url);
        }
    }
}
