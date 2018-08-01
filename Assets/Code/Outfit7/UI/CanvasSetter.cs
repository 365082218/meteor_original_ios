using UnityEngine;

namespace Outfit7.UI {
	[ExecuteInEditMode]
	[RequireComponent(typeof(Canvas)), RequireComponent(typeof(UnityEngine.UI.CanvasScaler))]
	[AddComponentMenu("UI/Canvas Setter", 53)]

	public class CanvasSetter : UnityEngine.EventSystems.UIBehaviour {

		private Camera UiCamera;

		private void SetCanvasValues() {
			if (UiCamera.aspect > 0.6666666f) {
				GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = 1f;
			} else {
				GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = 0f;
			}
		}

		// UIBehaviour
		protected override void Awake() {
			base.Awake();
			UpdateCanvas();
		}

		// MonoBehaviour
		protected void Update() {
			#if UNITY_EDITOR
			UpdateCanvas();
			#endif
		}

		private void UpdateCanvas() {

			if (UiCamera == null) {
				UiCamera = WorldCamera;
				Outfit7.Util.Assert.NotNull(UiCamera, "UI Camera on the Canvas not set!");
			}

			SetCanvasValues();
		}

		protected virtual Camera WorldCamera {
			get{ 
				return GetComponent<Canvas>().worldCamera;
			}
		}
	}
}