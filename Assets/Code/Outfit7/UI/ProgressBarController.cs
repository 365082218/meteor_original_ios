using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Outfit7.UI {
    [ExecuteInEditMode]
    public class ProgressBarController : UIBehaviour {
        [SerializeField] protected Image ProgressImage = null;
        [SerializeField] protected Material MaterialPrefab = null;
        [SerializeField] protected Color ProgressColor = Color.white;
        [SerializeField, Range(0f, 1f)] protected float ProgressValue = 0.5f;

        private int ColorId;
        private int ValueId;

        public Color Color {
            get {
                return ProgressColor;
            }
            set {
                ProgressColor = value;
                SetMaterial();
                ProgressImage.material.SetColor(ColorId, value);
            }
        }

        public virtual float Value {
            get {
                return ProgressValue;
            }
            set {
                ProgressValue = Mathf.Clamp01(value);
                SetMaterial();
                ProgressImage.material.SetFloat(ValueId, ProgressValue);
            }
        }

        protected override void Awake() {
            base.Awake();

            ColorId = Shader.PropertyToID("_Color");
            ValueId = Shader.PropertyToID("_Progress");

            SetProgressParameters();
        }

        private void SetProgressParameters() {
            if (ProgressImage == null) {
                return;
            }

            RectTransform rt = ProgressImage.rectTransform;
            float aspect = rt.rect.width / rt.rect.height;
            SetMaterial();
            ProgressImage.material.SetFloat("_Aspect", aspect);
        }

        private void SetMaterial() {
            if (ProgressImage.material == null || ProgressImage.material == ProgressImage.defaultMaterial) {
                ProgressImage.material = Instantiate(MaterialPrefab);
            }
        }

        protected virtual void Update() {
#if UNITY_EDITOR
            SetProgressParameters();
#endif
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            if (ProgressImage == null) {
                return;
            }

            SetMaterial();
            ProgressImage.material.SetColor(ColorId, ProgressColor);
            ProgressImage.material.SetFloat(ValueId, ProgressValue);
        }
#endif
    }
}