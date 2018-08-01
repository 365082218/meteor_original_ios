//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Outfit7.Util {

    /// <summary>
    /// Invoke utils.
    /// </summary>
    public static class InvokeUtils {

        static private Dictionary<ulong, MethodInfo> Methods = new Dictionary<ulong, MethodInfo>(64);

#if NETFX_CORE
        static private MethodInfo GetMethod(Type type, string name) {
            TypeInfo typeInfo = type.GetTypeInfo();
            foreach (MethodInfo methodInfo in typeInfo.DeclaredMethods) {
                if (methodInfo.Name == name) {
                    return methodInfo;
                }
            }
            if (typeInfo.BaseType != null) {
                return GetMethod(typeInfo.BaseType, name);
            }
            return null;
        }
#endif

        public static bool IsSubclassOf(Type current, Type expected) {
#if NETFX_CORE
            return current.GetTypeInfo().IsSubclassOf(expected);
#else
            return current.IsSubclassOf(expected);
#endif
        }

        public static MethodInfo GetMethodInfo(Action action) {
#if NETFX_CORE
            return action.GetMethodInfo();
#else
            return action.Method;
#endif
        }

        /// <summary>
        /// Invoke method on object.
        /// </summary>
        static public object Invoke(object objectThis, string name, object[] parameters) {
            Type type = objectThis.GetType();
            // Check if this method is already in dictionary
            MethodInfo methodInfo;
            ulong hash64 = (((ulong) type.Name.GetHashCode()) << 32) + ((ulong) name.GetHashCode());
            if (!Methods.TryGetValue(hash64, out methodInfo)) {
#if NETFX_CORE
                methodInfo = GetMethod(type, name);
#else
#if (UNITY_EDITOR || DEVEL_BUILD || PROD_BUILD)
                try {
#endif
                    methodInfo = type.GetMethod(name);
#if (UNITY_EDITOR || DEVEL_BUILD || PROD_BUILD)
                } catch (Exception exception) {
                    throw new InvalidOperationException(string.Format("Invoke get method failed: {0}.{1}\nException: {2}", type.Name, name, exception));
                }
#endif
#endif
                Methods.Add(hash64, methodInfo);
            }
            // Invoke it!
            if (methodInfo != null) {
#if (UNITY_EDITOR || DEVEL_BUILD || PROD_BUILD)
                try {
#endif
#if !NETFX_CORE
                    ParameterInfo[] parameterInfos = null;
                    for (int i = 0; i < parameters.Length; i++) {
                        if (parameters[i] == null || parameters[i].Equals(null)) {
                            if (parameterInfos == null) {
                                parameterInfos = methodInfo.GetParameters();
                            }
                            parameters[i] = Convert.ChangeType(null, parameterInfos[i].GetType());
                        }
                    }
#endif
                    return methodInfo.Invoke(objectThis, parameters);
#if (UNITY_EDITOR || DEVEL_BUILD || PROD_BUILD)
                } catch (Exception exception) {
                    throw new InvalidOperationException(string.Format("Invoke failed: {0}.{1}/{2}\nException: {3}", type.Name, name, methodInfo.Name, exception));
                }
#endif
            }
            return null;
        }
    }
}
