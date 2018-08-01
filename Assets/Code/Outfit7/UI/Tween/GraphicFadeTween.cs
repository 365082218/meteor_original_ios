using UnityEngine;
using UnityEngine.UI;

namespace Outfit7.UI.Tween {
    public class GraphicFadeTween : BaseTween {
        
        [SerializeField]
        private Graphic Image = null;

        private Material Material;
        private Color InitialColor;

        public override void StartAnimation(Type type) {
            Material = Instantiate(Image.material) as Material;
            Image.material = Material;
            InitialColor = Material.color;
            base.StartAnimation(type);
        }

        public override void UpdateProgress(float progress) {
            Material.SetColor("_Color", new Color(InitialColor.r, InitialColor.g, InitialColor.b, InitialColor.a * progress));
        }
    }
}