using UnityEngine;
using UnityEngine.UI;

namespace Outfit7.UI {
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    [AddComponentMenu("UI/Canvas Camera Enabler", 53)]

    public class CanvasCameraEnabler : UnityEngine.EventSystems.UIBehaviour {

        private Canvas Canvas;

        private void Check() {
            Canvas.worldCamera.enabled = GraphicRegistry.GetGraphicsForCanvas(Canvas).Count > 0;
        }

        protected override void Awake() {
            base.Awake();
            Canvas = GetComponent<Canvas>();
            Canvas.willRenderCanvases += Check;
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            Canvas.willRenderCanvases -= Check;
        }
    }
}