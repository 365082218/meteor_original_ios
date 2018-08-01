
using System;
using UnityEngine;

namespace Outfit7.Util {

    [Serializable]
    public partial class AssetReference {
        public static bool InSceneLoading = false;

        public string PrefabPath = string.Empty;
        public string EditorAssetGUID = string.Empty;

        public bool IsValid { get { return PrefabPath.Length > 0; } }

        private WeakReference ResourceReference;

        public AssetReference Clone() {
            return MemberwiseClone() as AssetReference;
        }

        public UnityEngine.Object Load(Type type) {
            if (Application.isPlaying) {
                // IsAlive check is not good enough
                if (ResourceReference != null) {
                    var cachedResource = ResourceReference.Target as UnityEngine.Object;
                    if (cachedResource != null) {
                        return cachedResource;
                    }
                }
            }

#if !UNITY_EDITOR
            if (InSceneLoading) {
                O7Log.Error("Trying to load resource {0} sync while loading scene!", PrefabPath);
            }
#endif

            var resource = UnityEngine.Resources.Load(PrefabPath, type);
            ResourceReference = new WeakReference(resource);
            return resource;
        }

        public ResourceRequest LoadAsync(Type type) {
#if !UNITY_EDITOR
            if (InSceneLoading) {
                O7Log.Error("Trying to load resource {0} async while loading scene!", PrefabPath);
            }
#endif
            return UnityEngine.Resources.LoadAsync(PrefabPath, type);
        }

        public virtual Type GetTypeOfT() {
            return typeof(UnityEngine.Object);
        }
    }

    public class AssetReferenceT<T> : AssetReference where T : UnityEngine.Object {

        public override Type GetTypeOfT() {
            return typeof(T);
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class AssetReferenceDescriptionAttribute : PropertyAttribute {
        public readonly Type Type;

        public AssetReferenceDescriptionAttribute(Type type) {
            Type = type;
        }
    }
}
