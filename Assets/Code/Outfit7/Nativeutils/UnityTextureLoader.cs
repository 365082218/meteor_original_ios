using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Outfit7.Util {

    public class UnityTextureLoader {

        const string Tag = "UnityTextureLoader";

        const int WebPDecompressionUpdateSize = 4 * 1024;
        const long WorkLoadTimePerFrame = 10; // ms

        public delegate void WorkCompleted(Texture2D tex, object userData);

        private class TextureLoadWorker {
            public int UID = 0;
            public WorkCompleted OnWorkCompleted;
            public byte[] SourceBuffer;
            public Texture2D Texture;
            public object UserData;

            // decompression specific
            public IntPtr WebPHandle;
            public byte[] TempBuffer;
            public GCHandle DestinationHandle;
            public GCHandle SourceHandle;

            private static int UIDGenerator = 1000;

            public void Reset() {
                OnWorkCompleted = null;
                SourceBuffer = null;
                Texture = null;
                UserData = null;
                WebPHandle = IntPtr.Zero;
                TempBuffer = null;
                UID = UIDGenerator++;
            }

            public void CreteAndLoadUnityTexture(TextureFormat tf) {
                Texture = new Texture2D(1, 1, tf, false);
                Texture.LoadImage(SourceBuffer);
                Texture.wrapMode = TextureWrapMode.Clamp;
                if (Texture.height == 1 && Texture.width == 1) {
                    O7Log.ErrorT(Tag, "CreteAndLoadUnityTexture failed: uid {0}", UID);
                    OnWorkCompleted(null, UserData);
                } else {
                    O7Log.DebugT(Tag, "OnWorkCompleted ok: uid {0}", CurrentWorkerItem.UID);
                    OnWorkCompleted(Texture, UserData);
                }
            }

            public void InitWebP() {
                // get image info, create texture
                NativeUtils.ImageInfo info = new NativeUtils.ImageInfo();
                NativeUtils.WebPGetInfo(SourceBuffer, ref info);
                TempBuffer = new byte[info.Height * info.Width * info.ComponentCount];
                if (info.ComponentCount == 3) {
                    Texture = new Texture2D(info.Width, info.Height, TextureFormat.RGB24, false);
                } else {
                    Texture = new Texture2D(info.Width, info.Height, TextureFormat.ARGB32, false);
                }

                // pin buffer handles
                DestinationHandle = GCHandle.Alloc(TempBuffer, GCHandleType.Pinned);
                IntPtr destinationPtr = DestinationHandle.AddrOfPinnedObject();
                SourceHandle = GCHandle.Alloc(SourceBuffer, GCHandleType.Pinned);
                IntPtr sourcePtr = SourceHandle.AddrOfPinnedObject();

                // create handle
                WebPHandle = NativeUtils.WebPDecompressBegin(destinationPtr, TempBuffer.Length, sourcePtr, SourceBuffer.Length);
                if (WebPHandle == IntPtr.Zero) {
                    DestinationHandle.Free();
                    SourceHandle.Free();
                }
            }
        }

        static List<TextureLoadWorker> WorkerQueue = new List<TextureLoadWorker>();
        static Stack<TextureLoadWorker> PoolFree = new Stack<TextureLoadWorker>();
        static TextureLoadWorker CurrentWorkerItem = null;
        static System.Diagnostics.Stopwatch StopWatch = new System.Diagnostics.Stopwatch();

        public static int EnqueueWorker(byte[] buffer, WorkCompleted workComplete, object userData) {
            O7Log.DebugT(Tag, "EnqueueWorker: {0}; userData: {1}", workComplete, userData.ToString());
            Assert.NotNull(workComplete, "workComplete");
            TextureLoadWorker tlw = null;
            if (PoolFree.Count > 0) {
                tlw = PoolFree.Pop();
            } else {
                tlw = new TextureLoadWorker();
            }
            tlw.Reset();
            tlw.SourceBuffer = buffer;
            tlw.OnWorkCompleted = workComplete;
            tlw.UserData = userData;
            WorkerQueue.Add(tlw);
            O7Log.DebugT(Tag, "EnqueueWorker done: UID: {0}", tlw.UID);
            return tlw.UID;
        }

        public static void CancelWorker(int uid) {
            O7Log.DebugT(Tag, "CancelWorker UID: {0}", uid);
            if (CurrentWorkerItem != null && CurrentWorkerItem.UID == uid) {
                CurrentWorkerItem.OnWorkCompleted = null;
                O7Log.DebugT(Tag, "CancelWorker CurrentWorkerItem");
            }
            for (int a = 0; a < WorkerQueue.Count; a++) {
                TextureLoadWorker tlw = WorkerQueue[a];
                if (tlw.UID == uid) {
                    WorkerQueue.RemoveAt(a);
                    O7Log.DebugT(Tag, "CancelWorker WorkerQueue");
                    break;
                }
            }
        }

        public static void OnUpdate() {
            long timeSpent = 0;
            StopWatch.Reset();
            while (WorkerQueue.Count > 0 || CurrentWorkerItem != null) {
                StopWatch.Start();
                InternalUpdate();
                StopWatch.Stop();
                timeSpent += StopWatch.ElapsedMilliseconds;
                if (timeSpent >= WorkLoadTimePerFrame) {
                    break;
                }
            }
        }

        static void ClearCurrentWorkerItem() {
            CurrentWorkerItem.Reset();
            PoolFree.Push(CurrentWorkerItem);
            CurrentWorkerItem = null;
        }


        static void HandleJPEG() {
            O7Log.DebugT(Tag, "HandleJPEG: uid: {0}", CurrentWorkerItem.UID);
            CurrentWorkerItem.CreteAndLoadUnityTexture(TextureFormat.RGB24);
            ClearCurrentWorkerItem();
        }

        static void HandlePNG() {
            O7Log.DebugT(Tag, "HandlePNG: uid: {0}", CurrentWorkerItem.UID);
            CurrentWorkerItem.CreteAndLoadUnityTexture(TextureFormat.ARGB32);
            ClearCurrentWorkerItem();
        }

        static void HandleWEBP() {
            // init handle if needed
            if (CurrentWorkerItem.WebPHandle == IntPtr.Zero) {
                CurrentWorkerItem.InitWebP();

                // handle failed, report back
                if (CurrentWorkerItem.WebPHandle == IntPtr.Zero) {
                    O7Log.ErrorT(Tag, "HandleWEBP webp handle failed: uid {0}", CurrentWorkerItem.UID);
                    CurrentWorkerItem.OnWorkCompleted(null, CurrentWorkerItem.UserData);
                    ClearCurrentWorkerItem();
                    return;
                }

                O7Log.DebugT(Tag, "HandleWEBP: uid {0}", CurrentWorkerItem.UID);
            }

            // do update
            NativeUtils.WebPStatus status = NativeUtils.WebPDecompressUpdate(CurrentWorkerItem.WebPHandle, WebPDecompressionUpdateSize);
            switch (status) {
                // job finished?
                case NativeUtils.WebPStatus.Ok:
                    // cleanup
                    NativeUtils.WebPDecompressEnd(CurrentWorkerItem.WebPHandle);
                    CurrentWorkerItem.DestinationHandle.Free();
                    CurrentWorkerItem.SourceHandle.Free();

                    // load raw
                    CurrentWorkerItem.Texture.LoadRawTextureData(CurrentWorkerItem.TempBuffer);
                    CurrentWorkerItem.Texture.Apply(false, true);
                    CurrentWorkerItem.Texture.wrapMode = TextureWrapMode.Clamp;
                    // report back
                    O7Log.DebugT(Tag, "OnWorkCompleted ok: uid {0}", CurrentWorkerItem.UID);
                    CurrentWorkerItem.OnWorkCompleted(CurrentWorkerItem.Texture, CurrentWorkerItem.UserData);

                    ClearCurrentWorkerItem();
                    break;
                // nope, still got work to do
                case NativeUtils.WebPStatus.Suspended:
                    O7Log.DebugT(Tag, "HandleWEBP: processing: uid {0}", CurrentWorkerItem.UID);
                    break;
                // everything else is wrong
                default:
                    NativeUtils.WebPDecompressEnd(CurrentWorkerItem.WebPHandle);
                    CurrentWorkerItem.DestinationHandle.Free();
                    CurrentWorkerItem.SourceHandle.Free();
                    O7Log.ErrorT(Tag, "OnWorkCompleted webp failed: uid {0}; status: {1}", CurrentWorkerItem.UID, status.ToString());
                    CurrentWorkerItem.OnWorkCompleted(null, CurrentWorkerItem.UserData);
                    ClearCurrentWorkerItem();
                    break;
            }
        }

        static void InternalUpdate() {
            if (CurrentWorkerItem == null && WorkerQueue.Count > 0) {
                CurrentWorkerItem = WorkerQueue[0];
                WorkerQueue.RemoveAt(0);
                Assert.NotNull(CurrentWorkerItem, "CurrentWorkerItem");
                O7Log.DebugT(Tag, "InternalUpdate: new worker: uid: {0}", CurrentWorkerItem.UID);
            }

            if (CurrentWorkerItem != null) {
                NativeUtils.ImageFileFormat iff = NativeUtils.GetImageFileFormat(CurrentWorkerItem.SourceBuffer);
                switch (iff) {
                    case NativeUtils.ImageFileFormat.JPEG:
                        HandleJPEG();
                        break;
                    case NativeUtils.ImageFileFormat.PNG:
                        HandlePNG();
                        break;
                    case NativeUtils.ImageFileFormat.WebP:
                        HandleWEBP();
                        break;
                    default:
                        O7Log.ErrorT(Tag, "OnWorkCompleted failed: uid {0}; image format {1} not supported", CurrentWorkerItem.UID, iff.ToString());
                        CurrentWorkerItem.OnWorkCompleted(null, CurrentWorkerItem.UserData);
                        ClearCurrentWorkerItem();
                        break;
                }
            }
        }
    }
}
