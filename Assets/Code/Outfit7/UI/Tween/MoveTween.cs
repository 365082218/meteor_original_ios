using UnityEngine;

namespace Outfit7.UI.Tween {
    public class MoveTween : BaseTween {

        [SerializeField]
        private RectTransform RectTransform = null;

        public Vector2 From = Vector2.zero;
        public Vector2 To = Vector2.zero;

        public override void UpdateProgress(float progress) {
            RectTransform.anchoredPosition = Vector2.Lerp(From, To, progress);
        }
    }
}
