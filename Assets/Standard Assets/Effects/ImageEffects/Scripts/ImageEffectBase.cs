using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [RequireComponent(typeof (Camera))]
    [AddComponentMenu("")]
    public class ImageEffectBase : MonoBehaviour
    {
        /// Provides a shader property that is set in the inspector
        /// and a material instantiated from the shader
        public Shader shader;

        private Material m_Material;


        protected virtual void Start()
        {
            // Disable if we don't support image effects
            if (!SystemInfo.supportsImageEffects)
            {
                enabled = false;
                Debug.LogError("not support Image Effect");
                return;
            }

            if (shader == null) {
                shader = Find("GrayscaleEffect");
            }
            // Disable the image effect if the shader can't
            // run on the users graphics card
            if (!shader || !shader.isSupported) {
                enabled = false;
                Debug.LogError("shader not support");
            }
        }

        public static Shader Find(string shader) {
            int nindex = shader.LastIndexOf('/');
            string sub = shader.Substring(nindex + 1);
            Shader sh = Resources.Load<Shader>(sub);
            return sh;
        }

        protected Material material
        {
            get
            {
                if (shader == null) {
                    shader = Find("GrayscaleEffect");
                }
                if (m_Material == null)
                {
                    m_Material = new Material(shader);
                    m_Material.hideFlags = HideFlags.HideAndDontSave;
                }
                return m_Material;
            }
        }


        protected virtual void OnDisable()
        {
            if (m_Material)
            {
                DestroyImmediate(m_Material);
            }
        }
    }
}
