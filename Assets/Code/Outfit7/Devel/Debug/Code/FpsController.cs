using UnityEngine;

namespace Outfit7.Devel.O7Debug {
    public class FpsController : MonoBehaviour {

#pragma warning disable 0414
        [SerializeField] private UnityEngine.UI.Text FpsText = null;
        [SerializeField] private GameObject Content = null;
#pragma warning restore 0414

#if UNITY_EDITOR || DEVEL_BUILD || PROD_BUILD
        private float FpsUpdateInterval = 0.5F;
        // FPS accumulated over the interval
        private float Accum = 0;
        // Frames drawn over the interval
        private int Frames = 0;
        // Left time for current interval
        private float TimeLeft;

        private int LastFPS = 0;

        private bool PrevFpsPing = false;
        public static bool ShowFps = false;

        public void Start() {
            TimeLeft = FpsUpdateInterval;
#if UNITY_EDITOR || DEVEL_BUILD
            ShowFps = true;
#endif
        }

        private void LateUpdate() {
            if (PrevFpsPing != ShowFps) {
                PrevFpsPing = ShowFps;
                Content.SetActive(ShowFps);
            }

            TimeLeft -= Time.deltaTime;
            Accum += Time.timeScale / Time.deltaTime;
            ++Frames;

            if (TimeLeft <= 0.0) {
                float fps = Accum / Frames;
                LastFPS = (int) fps;
                TimeLeft = FpsUpdateInterval;
                Accum = 0.0F;
                Frames = 0;
                FpsText.text = "fps: " + LastFPS;
            }

        }
#endif

    }
}
