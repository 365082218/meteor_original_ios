//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System.IO;

namespace Outfit7.Util {

    /// <summary>
    /// Stream utilities.
    /// </summary>
    public static class StreamUtils {

        public static void Copy(Stream input, Stream output) {
            Copy(input, output, 8192);
        }

        public static void Copy(Stream input, Stream output, int bufferSize) {
            byte[] buffer = new byte[bufferSize];
            int numRead;
            while ((numRead = input.Read(buffer, 0, buffer.Length)) != 0) {
                output.Write(buffer, 0, numRead);
            }
        }

        public static byte[] ReadAllBytes(Stream input) {
            using (MemoryStream memory = new MemoryStream()) {
                Copy(input, memory);
                return memory.ToArray();
            }
        }

        public static void WriteAllBytes(byte[] bytes, Stream output) {
            using (MemoryStream memory = new MemoryStream(bytes)) {
                Copy(memory, output);
            }
        }
    }
}
