//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.IO;
using System.Text;
using SharpCompress.Compressor;
using SharpCompress.Compressor.Deflate;

namespace Outfit7.Util {

    /// <summary>
    /// GZIP compression &amp; decompression utilities.
    /// </summary>
    public static class Gzip {

        public static byte[] CompressUtf8String(string text) {
            return CompressBytes(Encoding.UTF8.GetBytes(text));
        }

        public static byte[] CompressBytes(byte[] bytes) {
            if (CollectionUtils.IsEmpty(bytes))
                return bytes;

            using (MemoryStream outStream = new MemoryStream()) {
                using (GZipStream compressor = new GZipStream(outStream, CompressionMode.Compress)) {
                    compressor.Write(bytes, 0, bytes.Length);
                }
                return outStream.ToArray();
            }
        }

        public static string DecompressToUtf8String(byte[] bytes) {
			if (bytes == null)
				return null;
			byte[] decompressed = DecompressBytes(bytes);
			return Encoding.UTF8.GetString(decompressed, 0, decompressed.Length);
        }

        public static byte[] DecompressBytes(byte[] bytes) {
            if (CollectionUtils.IsEmpty(bytes))
                return bytes;

            using (MemoryStream outStream = new MemoryStream()) {
                using (MemoryStream inStream = new MemoryStream(bytes)) {
                    using (GZipStream decompressor = new GZipStream(inStream, CompressionMode.Decompress)) {
                        StreamUtils.Copy(decompressor, outStream);
                    }
                }
                return outStream.ToArray();
            }
        }
    }
}
