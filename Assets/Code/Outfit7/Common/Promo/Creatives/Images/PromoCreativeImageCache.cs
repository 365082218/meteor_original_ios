//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System.IO;
using Outfit7.Util;
using Outfit7.Util.Io;

namespace Outfit7.Common.Promo.Creatives.Images {

    /// <summary>
    /// Promo creative image cache.
    /// Thread safe.
    /// </summary>
    public class PromoCreativeImageCache {

        protected const string Tag = "PromoCreativeImageCache";

        protected readonly object Lock;
        public readonly string FilePath;

        public PromoCreativeImageCache(string filePath) {
            Assert.HasText(filePath, "filePath");
            Lock = new object();
            FilePath = filePath;
        }

        public virtual byte[] ReadData() {
            lock (Lock) {
                return ReadFile(FilePath);
            }
        }

        protected virtual byte[] ReadFile(string filePath) {
            if (!O7File.Exists(filePath)) {
                O7Log.VerboseT(Tag, "No file {0}", filePath);
                return null;
            }
            O7Log.VerboseT(Tag, "Reading data from file {0}", filePath);
            return O7File.ReadAllBytes(filePath);
        }

        public virtual void WriteData(byte[] content) {
            lock (Lock) {
                O7Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                WriteFile(FilePath, content);
            }
        }

        protected virtual void WriteFile(string filePath, byte[] data) {
            O7Log.VerboseT(Tag, "Writing data ({0} B) to file {1}", data.Length, filePath);
            O7File.WriteAllBytes(filePath, data);
        }
    }
}
