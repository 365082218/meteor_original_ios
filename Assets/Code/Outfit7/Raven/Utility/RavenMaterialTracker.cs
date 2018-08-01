using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Starlite.Raven {

    public static class RavenMaterialTracker {

        private class MaterialRegistry {
            public UnityEngine.Object m_Renderer = null;
            public Material[] m_OldMaterials = null;
            public int m_ModificationCount = 0;
        }

        private static readonly Dictionary<int, MaterialRegistry> s_RendererToMaterialMap = new Dictionary<int, MaterialRegistry>(128);

        public static void RegisterRenderer(UnityEngine.Object r) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif

            var instanceId = r.GetInstanceID();
            if (s_RendererToMaterialMap.ContainsKey(instanceId)) {
                return;
            }

            RegisterRendererInternal(r, instanceId);
        }

        public static void BeginMaterialModification(UnityEngine.Object r) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif

            var instanceId = r.GetInstanceID();

            MaterialRegistry registry = null;
            if (!s_RendererToMaterialMap.TryGetValue(instanceId, out registry)) {
                registry = RegisterRendererInternal(r, instanceId);
            }

            ++registry.m_ModificationCount;
        }

        public static void EndMaterialModification(UnityEngine.Object r) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif

            var instanceId = r.GetInstanceID();

            MaterialRegistry materialRegistry = null;
            if (!s_RendererToMaterialMap.TryGetValue(instanceId, out materialRegistry)) {
                RavenAssert.IsTrue(false, "Renderer {0} not present in tracking system!", r);
            }

            if (--materialRegistry.m_ModificationCount == 0) {
                if (r is Renderer) {
                    (r as Renderer).sharedMaterials = materialRegistry.m_OldMaterials;
                } else {
                    (r as Graphic).material = materialRegistry.m_OldMaterials[0];
                }
            }
            RavenAssert.IsTrue(materialRegistry.m_ModificationCount >= 0, "Modification count for {0} going below 0!", r);
        }

        public static Material[] GetOriginalMaterial(UnityEngine.Object r) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return null;
            }
#endif

            MaterialRegistry materialRegistry = null;
            if (!s_RendererToMaterialMap.TryGetValue(r.GetInstanceID(), out materialRegistry)) {
                return null;
            }

            return materialRegistry.m_OldMaterials;
        }

        public static void Cleanup() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif

            List<int> toRemove = new List<int>(s_RendererToMaterialMap.Count / 2);

            foreach (var kvp in s_RendererToMaterialMap) {
                if (kvp.Value.m_Renderer == null) {
                    toRemove.Add(kvp.Key);
                }
            }

            for (int i = 0; i < toRemove.Count; ++i) {
                s_RendererToMaterialMap.Remove(toRemove[i]);
            }
        }

        private static MaterialRegistry RegisterRendererInternal(UnityEngine.Object r, int instanceId) {
            var materialRegistry = new MaterialRegistry() {
                m_Renderer = r,
                m_OldMaterials = r is Renderer ? (r as Renderer).sharedMaterials : new Material[] { (r as Graphic).material }
            };
            s_RendererToMaterialMap[instanceId] = materialRegistry;
            return materialRegistry;
        }
    }
}
