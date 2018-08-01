using UnityEngine;
using UnityEngine.UI;

namespace Outfit7.UI {
    public class InstantiateUiGraphicMaterial : MonoBehaviour {

        public InstantiatePhase MaterialInstantiatePhase = InstantiatePhase.Awake;

        public enum InstantiatePhase 
        {
            Awake,
            Start
        }

        private void InstantiateMaterial()
        {
            Graphic graphic = GetComponent<Graphic>();
            graphic.material = Instantiate(graphic.material) as Material;
        }

        private void Awake()
        {
            if (MaterialInstantiatePhase == InstantiatePhase.Awake) {
                InstantiateMaterial();
            }
        }

        private void Start()
        {
            if (MaterialInstantiatePhase == InstantiatePhase.Start) {
                InstantiateMaterial();
            }
        }
    }
}
