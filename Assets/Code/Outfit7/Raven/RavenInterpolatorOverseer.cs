using System;
using System.Collections.Generic;

namespace Starlite.Raven {

    public static class RavenInterpolatorOverseer {
        private static Dictionary<Type, object> s_InterpolatorRegistry = new Dictionary<Type, object>();

        static RavenInterpolatorOverseer() {
            // register interpolators here
            RegisterInterpolator(RavenValueInterpolatorBool.Default);
            RegisterInterpolator(RavenValueInterpolatorInt.Default);
            RegisterInterpolator(RavenValueInterpolatorFloat.Default);
            RegisterInterpolator(RavenValueInterpolatorDouble.Default);
            RegisterInterpolator(RavenValueInterpolatorVector2.Default);
            RegisterInterpolator(RavenValueInterpolatorVector3.Default);
            RegisterInterpolator(RavenValueInterpolatorVector4.Default);
            RegisterInterpolator(RavenValueInterpolatorQuaternion.Default);
            RegisterInterpolator(RavenValueInterpolatorColor.Default);
            RegisterInterpolator(RavenValueInterpolatorRect.Default);
            RegisterInterpolator(RavenValueInterpolatorGradient.Default);
            RegisterInterpolator(RavenValueInterpolatorSprite.Default);
#if UNITY_5_6_OR_NEWER
            RegisterInterpolator(RavenValueInterpolatorMinMaxGradient.Default);
#endif

            // weird stuff here
            RegisterInterpolator(RavenValueInterpolatorMaterial.Default);
        }

        public static void RegisterInterpolator<T>(RavenValueInterpolatorBase<T> interpolator) {
            s_InterpolatorRegistry[typeof(T)] = interpolator;
        }

        public static RavenValueInterpolatorBase<T> GetInterpolator<T>() {
            return s_InterpolatorRegistry[typeof(T)] as RavenValueInterpolatorBase<T>;
        }
    }
}