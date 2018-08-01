//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using SimpleJSON;

namespace Outfit7.Util.Io {

    /// <summary>
    /// File reader and writer for JSON content.
    /// </summary>
    public class JsonFileReaderWriter : SimpleFileReaderWriter {

        public JsonFileReaderWriter(string filePath) : base(filePath) {
        }

        public virtual JSONNode ReadJson() {
            lock (Lock) {
                string data = ReadContent();
                if (data == null)
                    return null;
                try {
                    return JSON.Parse(data);

                } catch (Exception e) {
                    O7Log.ErrorT(Tag, e, "Cannot parse JSON from file {0}", FilePath);
                    return null;
                }
            }
        }

        public virtual void WriteJson(JSONNode json) {
            WriteContent(json == null ? null : json.ToString());
        }
    }
}
