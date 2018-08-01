//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using System.Text;
using Outfit7.Util;
using UnityEngine;

namespace Outfit7.Web {

    /// <summary>
    /// Represents a REST call.
    /// </summary>
    public class RestCall {

        private const string Tag = "RestCall";

        public enum Method {
            GET,
            POST,
        }

        public static class Header {
            public const string UserAgent = "USER-AGENT";
            public const string AcceptEncoding = "ACCEPT-ENCODING";
            public const string ContentEncoding = "CONTENT-ENCODING";
#if UNITY_WP8
            // Headers are case-sensitive. Unity uses pascal-case in WP8 to detect if content-type is already set.
            // If it is not, they add "application/x-www-form-urlencoded".
            public const string ContentType = "Content-Type";
#else
            public const string ContentType = "CONTENT-TYPE";
#endif
        }

        public static class ContentEncoding {
            public const string Gzip = "gzip";
        }

        public static class ContentType {
            public const string Json = "application/json";
        }

        public static string FixUrl(string url) {
            url = url.Trim();

#if UNITY_IOS && FORCE_HTTPS
            // Replace HTTP request with secure one as required on iOS
            // This is hack on client, because our backend is not able to replace all URLs
            if (url.StartsWith("http://")) {
                url = url.Insert(4, "s");
                O7Log.VerboseT(Tag, "URL fixed to '{0}'", url);
            }
#endif

            return url;
        }

        protected bool running;
        protected readonly string url;
        protected readonly byte[] body;
        protected readonly Dictionary<string, string> headers;

        public string Error { get; private set; }

        public Dictionary<string, string> ResponseHeaders { get; private set; }

        public string ResponseBody { get; private set; }

        public bool Done {
            get {
                return ResponseHeaders != null;
            }
        }

        public RestCall(string url, Dictionary<string, string> headers) : this(url, null, headers) {
        }

        public RestCall(string url, Dictionary<string, string> headers, Method method) : this(url,
                (method == Method.POST) ? ":)" : null, // Fake body to convince WWW to use POST method
                headers) {
        }

        public RestCall(string url, string body, Dictionary<string, string> headers) {
            this.url = FixUrl(url);

            // Compress body by default if it makes sense
            bool compress = body != null && body.Length > 200;

            if (compress) {
                this.body = Gzip.CompressUtf8String(body);
            } else if (body != null) {
                this.body = Encoding.UTF8.GetBytes(body);
            }

            headers[Header.AcceptEncoding] = ContentEncoding.Gzip;
            if (headers.ContainsKey(Header.UserAgent)) {
                headers[Header.UserAgent] = string.Format("{0} {1}", headers[Header.UserAgent], ContentEncoding.Gzip);
            } else {
                headers[Header.UserAgent] = ContentEncoding.Gzip;
            }
            if (compress) {
                headers[Header.ContentEncoding] = ContentEncoding.Gzip;
            }
            this.headers = headers;

            if (O7Log.DebugEnabled) {
                O7Log.DebugT(Tag, "REST request url='{0}', headers={1}, body='{2}'",
                    this.url, StringUtils.CollectionToCommaDelimitedString(this.headers), body);
            }
        }

        /// <summary>
        /// Starts executing the REST call.
        /// </summary>
        public IEnumerator<WWW> Execute() {
            Assert.State(!running, "Already started");
            running = true;

            IEnumerator<WWW> e = ExecuteInternal();
            while (e.MoveNext()) {
                yield return e.Current;
            }
        }

        protected IEnumerator<WWW> ExecuteInternal() {
            WWW request = new WWW(url, body, headers);

            yield return request;

            ResponseHeaders = request.responseHeaders;
            Error = request.error;
            if (!StringUtils.HasText(request.error)) {
// WWW automatically decompresses on iOS, try to decompress on others
#if UNITY_IPHONE && !UNITY_EDITOR
                ResponseBody = request.text;
#else
                // Decompress body if compressed
                if (!CollectionUtils.IsEmpty(request.bytes)
                    && ResponseHeaders.ContainsKey(Header.ContentEncoding)
                    && ResponseHeaders[Header.ContentEncoding].ToLowerInvariant() == ContentEncoding.Gzip) {
                    try {
                        ResponseBody = Gzip.DecompressToUtf8String(request.bytes);

                    } catch {
                        // If we get error about bad GZIP header or stream, response may have already been decompressed.
                        // Yes, Unity has problem even with that. Editor 4.5+ on OS X decompresses but not on Windows.
                        // So just ignore GZIP errors because content will be checked anyway.
                        ResponseBody = request.text;
                    }

                } else {
                    ResponseBody = request.text;
                }
#endif
            }

            if (O7Log.DebugEnabled) {
                O7Log.DebugT(Tag, "REST response url='{0}', headers={1}, error='{2}', body='{3}'",
                    url, StringUtils.CollectionToCommaDelimitedString(ResponseHeaders), Error, ResponseBody);
            }
        }

        public void CheckForError() {
            if (StringUtils.HasText(Error)) {
                throw new RestCallException("Error in REST response: " + Error);
            }
        }
    }
}
