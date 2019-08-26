using Ionic.Zlib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public static byte[] ConvertBytesZlib(byte[] data, CompressionMode compressionMode)
    {
        CompressionMode mode = compressionMode;
        if (mode != CompressionMode.Compress)
        {
            if (mode != CompressionMode.Decompress)
            {
                throw new NotImplementedException();
            }
            return ZlibStream.UncompressBuffer(data);
        }
        return ZlibStream.CompressBuffer(data);
    }
}
