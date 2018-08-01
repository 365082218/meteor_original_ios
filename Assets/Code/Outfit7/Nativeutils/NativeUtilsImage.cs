#if !SILVERLIGHT

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;

namespace Outfit7.Util {

    public static partial class NativeUtils {

        public struct ImageInfo {
            public int Width;
            public int Height;
            public int ComponentCount;
        }

        public enum ImageFileFormat {
            Unknown,
            WebP,
            JPEG,
            PNG
        }

        public enum WebPStatus {
            Ok = 0,
            OutOfMemory,
            InvalidParam,
            BitstreamError,
            UnsupportedFeature,
            Suspended,
            UserAbort,
            NotEnoughData
        };

        [DllImport(import)]
        public static extern int GetJPEGInfo(ref ImageInfo info, IntPtr source, int source_size);

        [DllImport(import)]
        public static extern int DecodeJPEG(IntPtr destination, int destination_size, IntPtr source, int source_size);

        // WebP
        [DllImport(import)]
        public static extern WebPStatus WebPGetImageInfo(IntPtr source, int source_size, ref ImageInfo info);

        [DllImport(import)]
        public static extern IntPtr WebPDecompressBegin(IntPtr destination, int destination_size, IntPtr source, int source_size);

        [DllImport(import)]
        public static extern WebPStatus WebPDecompressUpdate(IntPtr handle, int amount_bytes);

        [DllImport(import)]
        public static extern WebPStatus WebPDecompressEnd(IntPtr handle);


        public static ImageFileFormat GetImageFileFormat(byte[] buffer) {
            // webp
            if (buffer.Length > 12 && buffer[0] == 82 && buffer[1] == 73 && buffer[2] == 70 && buffer[3] == 70 &&
                buffer[8] == 87 && buffer[9] == 69 && buffer[10] == 66 && buffer[11] == 80) {
                return ImageFileFormat.WebP;
            }
            // png
            if (buffer.Length > 8 && buffer[0] == 137 && buffer[1] == 80 && buffer[2] == 78 && buffer[3] == 71 &&
                buffer[4] == 13 && buffer[5] == 10 && buffer[6] == 26 && buffer[7] == 10) {
                return ImageFileFormat.PNG;
            }
            // jpeg
            if (buffer.Length > 2 && buffer[0] == 255 && buffer[1] == 216) {
                return ImageFileFormat.JPEG;
            }
            return ImageFileFormat.Unknown;
        }

        public static Texture2D LoadTexture(byte[] buffer) {
            Texture2D tex = null;
            ImageFileFormat iff = GetImageFileFormat(buffer);
            switch (iff) {
                case ImageFileFormat.JPEG:
                    tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
                    tex.LoadImage(buffer);
                    tex.wrapMode = TextureWrapMode.Clamp;
                    if (tex.height < 16 && tex.width < 16) {
                        return null;
                    } else {
                        return tex;
                    }
                case ImageFileFormat.PNG:
                    tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    tex.LoadImage(buffer);
                    tex.wrapMode = TextureWrapMode.Clamp;
                    if (tex.height < 16 && tex.width < 16) {
                        return null;
                    } else {
                        return tex;
                    }
                case ImageFileFormat.WebP:
                    ImageInfo info = new ImageInfo();
                    WebPGetInfo(buffer, ref info);
                    byte[] tmpBuffer = new byte[info.Height * info.Width * info.ComponentCount];
                    if (info.ComponentCount == 3) {
                        tex = new Texture2D(info.Width, info.Height, TextureFormat.RGB24, false);
                    } else {
                        tex = new Texture2D(info.Width, info.Height, TextureFormat.ARGB32, false);
                    }
                    if (DecodeWebP(tmpBuffer, buffer) == 0) {
                        tex.LoadRawTextureData(tmpBuffer);
                        tex.Apply(false, true);
                        tex.wrapMode = TextureWrapMode.Clamp;
                        return tex;
                    } else {
                        return null;
                    }
            }
            return null;
        }

        public static int GetJPEGInfo(ref int width, ref int height, byte[] source) {
            ImageInfo info = new ImageInfo();
            GCHandle sourceHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr sourcePtr = sourceHandle.AddrOfPinnedObject();
            int ret = GetJPEGInfo(ref info, sourcePtr, source.Length);
            if (ret != 0) {
                sourceHandle.Free();
                return ret;
            }
            width = info.Width;
            height = info.Height;
            sourceHandle.Free();
            return 0;
        }

        public static int DecodeJPEG(byte[] destination, byte[] source) {
            GCHandle destinationHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr destinationPtr = destinationHandle.AddrOfPinnedObject();
            GCHandle sourceHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr sourcePtr = sourceHandle.AddrOfPinnedObject();
            int ret = DecodeJPEG(destinationPtr, destination.Length, sourcePtr, source.Length);
            sourceHandle.Free();
            destinationHandle.Free();
            return ret;
        }

        public static Texture2D CreateTextureFromJPEG(byte[] bytes, byte[] buffer, int width, int height) {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            Profiler.BeginSample("DecodeJPEG");
            DecodeJPEG(buffer, bytes);
            Profiler.EndSample();
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false, true);
            texture.LoadRawTextureData(buffer);
            texture.Apply(false, true);
            texture.wrapMode = TextureWrapMode.Clamp;
            stopWatch.Stop();
            LogFormat(LogLevel.Verbose, "CreateTextureFromJPEG time: {0}", (double) stopWatch.ElapsedTicks / 10000.0);
            return texture;
        }

        public static Texture2D CreateTextureFromJPEG(byte[] bytes) {
            int width = 0, height = 0;
            GetJPEGInfo(ref width, ref height, bytes);
            byte[] buffer = new byte[width * height * 3];
            return CreateTextureFromJPEG(bytes, buffer, width, height);
        }

        public static WebPStatus WebPGetInfo(byte[] source, ref ImageInfo info) {
            GCHandle sourceHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr sourcePtr = sourceHandle.AddrOfPinnedObject();
            WebPStatus ret = WebPGetImageInfo(sourcePtr, source.Length, ref info);
            if (ret != WebPStatus.Ok) {
                sourceHandle.Free();
                return ret;
            }
            sourceHandle.Free();
            return WebPStatus.Ok;
        }

        public static int DecodeWebP(byte[] destination, byte[] source) {
            GCHandle destinationHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr destinationPtr = destinationHandle.AddrOfPinnedObject();
            GCHandle sourceHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr sourcePtr = sourceHandle.AddrOfPinnedObject();

            bool ok = false;

            IntPtr handle = WebPDecompressBegin(destinationPtr, destination.Length, sourcePtr, source.Length);
            if (handle != IntPtr.Zero) {
                while (true) {
                    WebPStatus status = WebPDecompressUpdate(handle, 65536);
                    if (status == WebPStatus.Ok) {
                        ok = true;
                        break;
                    } else if (status != WebPStatus.Suspended) {
                        break;
                    }
                }
            }

            if (ok) {
                WebPDecompressEnd(handle);
            }

            sourceHandle.Free();
            destinationHandle.Free();

            return ok ? 0 : 1;
        }

        public static Texture2D CreateTextureFromWebP(byte[] bytes, byte[] buffer, int width, int height) {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            Profiler.BeginSample("DecodeWebP");
            DecodeWebP(buffer, bytes);
            Profiler.EndSample();
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false, true);
            texture.LoadRawTextureData(buffer);
            texture.Apply(false, true);
            texture.wrapMode = TextureWrapMode.Clamp;
            stopWatch.Stop();
            LogFormat(LogLevel.Verbose, "CreateTextureFromWebP time: {0}ms", stopWatch.ElapsedMilliseconds);
            return texture;
        }

        public static Texture2D CreateTextureFromWebP(byte[] bytes) {
            ImageInfo info = new ImageInfo();
            WebPGetInfo(bytes, ref info);
            byte[] buffer = new byte[info.Height * info.Width * info.ComponentCount];
            return CreateTextureFromWebP(bytes, buffer, info.Width, info.Height);
        }
    }
}

#endif