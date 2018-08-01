using NUnit.Framework;
using System;

namespace Outfit7.Video {

    [TestFixture]
    public class VideoPlaybackManagerTest {

        private VideoPlaybackManager VideoPlaybackManager;

        private class VideoPlayBackNotSupportedPlugin : VideoPlaybackPlugin {
            public override bool IsPlaybackSupported { get { return false; } }
        }

        private void SetupVideoPlaybackNotSupportedPlugin() {
            VideoPlaybackManager.VideoPlaybackPlugin = new VideoPlayBackNotSupportedPlugin();
            VideoPlaybackManager.VideoPlaybackPlugin.VideoPlaybackManager = VideoPlaybackManager;
            VideoPlaybackManager.Init();
        }

        private void SetupVideoPlaybackSupportedPlugin() {
            VideoPlaybackManager.VideoPlaybackPlugin = new VideoPlaybackPlugin();
            VideoPlaybackManager.VideoPlaybackPlugin.VideoPlaybackManager = VideoPlaybackManager;
            VideoPlaybackManager.Init();
        }

        [SetUp]
        public void Setup() {
            VideoPlaybackManager = new VideoPlaybackManager();
        }

        [TearDown]
        public void Dispose() {
            VideoPlaybackManager = null;
        }

        [Test]
        public void TestIsVideoPlaybackSupported() {
            SetupVideoPlaybackSupportedPlugin();
            Assert.AreEqual(true, VideoPlaybackManager.IsPlaybackSupported);
        }

        [Test]
        public void TestPlayVideoArgumentException() {
            SetupVideoPlaybackSupportedPlugin();
            Assert.Catch(delegate {
                VideoPlaybackManager.Play(null);
            }, "url not provided");

            Assert.Catch(delegate {
                VideoPlaybackManager.Play(" ");
            }, "empty url");
        }

        [Test]
        public void TestPlayVideoNotSupportedException() {
            SetupVideoPlaybackNotSupportedPlugin();
            Assert.Catch(delegate {
                VideoPlaybackManager.Play("testUrl");
            }, "Video playback not suported");
            Assert.AreEqual(false, VideoPlaybackManager.IsPlaybackInProgress);
        }

        [Test]
        public void TestPlayVideoSupported() {
            SetupVideoPlaybackSupportedPlugin();
            Assert.DoesNotThrow(delegate {
                VideoPlaybackManager.Play("testUrl");
            }, "Video playback should be supported");
            Assert.AreEqual(true, VideoPlaybackManager.IsPlaybackInProgress);
        }

        [Test]
        public void TestCanPlayNotSupported() {
            SetupVideoPlaybackNotSupportedPlugin();
            Assert.AreEqual(false, VideoPlaybackManager.CanPlay());
        }

        [Test]
        public void TestCanPlayPlaybackInProgress() {
            SetupVideoPlaybackSupportedPlugin();
            VideoPlaybackManager.Play("videoUrl");
            Assert.AreEqual(false, VideoPlaybackManager.CanPlay());
        }

        [Test]
        public void TestCanPlayReady() {
            SetupVideoPlaybackSupportedPlugin();
            Assert.AreEqual(true, VideoPlaybackManager.CanPlay());
        }

        [Test]
        public void TestPlaybackStartedCallback() {
            SetupVideoPlaybackSupportedPlugin();
            VideoPlaybackManager.PlaybackStartedCallback = () => {
                throw new Exception("playbackStartCallbackException");
            };
            Assert.Catch(() => VideoPlaybackManager.Play("videoUrl"));
        }
    }
}
