//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using UnityEngine;
using System.Collections;

namespace Outfit7.MyTalkingAngela.Gui.MainScene {
    public class HelperHandController : MonoBehaviour {

        public float alpha = 0f;

        public UITexture UiTexture = null;

        void Start() {
            StartCoroutine(ShowHand());
        }

        public void FadeInHelperHand() {
            if (!gameObject.activeInHierarchy)
                return;
            StartCoroutine(ShowHand());
        }

        private IEnumerator ShowHand() {
            UiTexture.alpha = 0;
            yield return new WaitForSeconds(0.5f);

            while (UiTexture.alpha < 1f) {
                alpha += Time.deltaTime * 0.9f;
                alpha = Mathf.Clamp01(alpha);
                UiTexture.alpha = alpha;
                yield return null;
            }
        }

    }
}
