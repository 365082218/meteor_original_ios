using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Starlite.Raven {

    public static class RavenEditorUtility {
        public const string c_RavenNamespace = "Starlite.Raven";
        public const string c_RavenCompilerNamespace = "Starlite.Raven.Compiler";

        private static readonly Dictionary<Type, Dictionary<Type, List<string>>> s_MemberCache = new Dictionary<Type, Dictionary<Type, List<string>>>();
        private static readonly Dictionary<Type[], Dictionary<Type, List<string>>> s_FunctionCache = new Dictionary<Type[], Dictionary<Type, List<string>>>(new TypeArrayComparer());
        private static readonly Type s_DummyType = typeof(TypeArrayComparer);

        private class TypeArrayComparer : IEqualityComparer<Type[]> {
            public bool Equals(Type[] x, Type[] y) {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(Type[] obj) {
                int hash = 0;
                for (int i = 0; i < obj.Length; i++) {
                    hash ^= obj[i].GetHashCode();
                }
                return hash;
            }
        }

        #region MemberReflection

        public static List<string> GetMemberList(RavenAnimationPropertyComponentBase property, GameObject target) {
            return GetMemberList(property.GetPropertyType(), target);
        }

        public static List<string> GetMemberList(Type propertyType, GameObject target) {
            var members = new List<string>();
            if (target == null) {
                return members;
            }

            var components = target.GetComponents<Component>();
            Dictionary<Type, List<string>> componentTypeToMemberList;
            List<string> componentMembers;
            if (!s_MemberCache.TryGetValue(propertyType, out componentTypeToMemberList)) {
                componentTypeToMemberList = new Dictionary<Type, List<string>>();
                s_MemberCache[propertyType] = componentTypeToMemberList;
            }

            for (int i = 0; i < components.Length; ++i) {
                var componentType = components[i].GetType();
                if (!componentTypeToMemberList.TryGetValue(componentType, out componentMembers)) {
                    componentMembers = GetComponentMembers(propertyType, componentType);
                    componentTypeToMemberList[componentType] = componentMembers;
                }
                members.AddRange(componentMembers);
            }

            var targetType = target.GetType();
            if (!componentTypeToMemberList.TryGetValue(targetType, out componentMembers)) {
                componentMembers = GetComponentMembers(propertyType, targetType);
                componentTypeToMemberList[targetType] = componentMembers;
            }
            members.AddRange(componentMembers);

            return members;
        }

        public static List<string> GetComponentMembers(Type propertyType, Type type) {
            var members = type
                .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => (x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property) &&
                    x.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length == 0);

            var list = new List<string>();
            foreach (var member in members) {
                Type memberType = null;
                if (member is FieldInfo) {
                    memberType = ((FieldInfo)member).FieldType;
                } else {
                    var property = (PropertyInfo)member;
                    if (!property.CanWrite || property.GetSetMethod() == null) {
                        continue;
                    }
                    memberType = property.PropertyType;
                }

                if (!propertyType.IsAssignableFrom(memberType)) {
                    continue;
                }
                list.Add(string.Format("{0}|{1}", type.ToString(), member.Name));
            }

            return list;
        }

        #endregion MemberReflection

        #region FunctionReflection

        public static List<string> GetFunctionList(GameObject target, params string[] parameterConstraints) {
            return GetFunctionList(target, parameterConstraints == null ? null : parameterConstraints.Select(x => RavenUtility.GetTypeFromLoadedAssemblies(x)).ToArray());
        }

        public static List<string> GetFunctionList(GameObject target, params Type[] parameterConstraints) {
            var methods = new List<string>();
            if (target == null) {
                return methods;
            }

            if (parameterConstraints == null || parameterConstraints[0] == null) {
                parameterConstraints = new Type[1] {
                    s_DummyType
                };
            }

            var components = target.GetComponents<Component>();
            Dictionary<Type, List<string>> componentTypeToFunctionList;
            List<string> componentFunctions;
            if (!s_FunctionCache.TryGetValue(parameterConstraints, out componentTypeToFunctionList)) {
                componentTypeToFunctionList = new Dictionary<Type, List<string>>();
                s_FunctionCache[parameterConstraints] = componentTypeToFunctionList;
            }

            for (int i = 0; i < components.Length; ++i) {
                var componentType = components[i].GetType();
                if (!componentTypeToFunctionList.TryGetValue(componentType, out componentFunctions)) {
                    componentFunctions = GetComponentFunctions(componentType, parameterConstraints);
                    componentTypeToFunctionList[componentType] = componentFunctions;
                }
                methods.AddRange(componentFunctions);
            }

            var targetType = target.GetType();
            if (!componentTypeToFunctionList.TryGetValue(targetType, out componentFunctions)) {
                componentFunctions = GetComponentFunctions(targetType, parameterConstraints);
                componentTypeToFunctionList[targetType] = componentFunctions;
            }
            methods.AddRange(componentFunctions);

            return methods;
        }

        public static List<string> GetComponentFunctions(System.Type type, params Type[] parameterConstraints) {
            // haxx0r
            var paramConstraints = parameterConstraints != null && parameterConstraints[0] != s_DummyType;

            var methods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => !x.Name.StartsWith("get_") &&
                    !x.Name.StartsWith("set_") &&
                    !x.IsGenericMethodDefinition &&
                    x.GetParameters().Length < RavenUtility.c_MaxFunctionParameters &&
                    x.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length == 0 &&
                    x.GetParameters().FirstOrDefault(p => p.IsOut || !ParameterTypeValid(p.ParameterType)) == null &&
                    (!paramConstraints || x.GetParameters().Select(y => y.ParameterType).SequenceEqual(parameterConstraints)));

            var extensionMethods = GetExtensionMethods(type, parameterConstraints);

            var list = new List<string>();
            foreach (var method in methods.Concat(extensionMethods)) {
                list.Add(string.Format("{0}|{1}|{2}", type.ToString(), method.Name, RavenUtility.GetFunctionParameterTypesPacked(method)));
            }

            return list;
        }

        private static IEnumerable<MethodInfo> GetExtensionMethods(Type extendedType, Type[] parameterConstraints) {
            var paramConstraints = parameterConstraints != null && parameterConstraints[0] != s_DummyType;
            var assembly = typeof(RavenSequence).Assembly;
            var query = from type in assembly.GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where extendedType.IsSubclassOf(method.GetParameters()[0].ParameterType) || extendedType == method.GetParameters()[0].ParameterType
                        where (!paramConstraints || method.GetParameters().Select(y => y.ParameterType).SequenceEqual(parameterConstraints))
                        select method;
            return query;
        }

        private static bool ParameterTypeValid(Type parameterType) {
            var success = !parameterType.IsByRef;
            success &= !parameterType.IsGenericTypeDefinition;
            success &= parameterType.IsValueType || // value types (leaks structs but whatever)
                parameterType.IsDefined(typeof(SerializableAttribute), true) || // custom classes, leaks some system classes
                parameterType.IsSubclassOf(typeof(UnityEngine.Object)) ||   // unity obj
                (parameterType.IsArray && ParameterTypeValid(parameterType.GetElementType())) ||    // array
                (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(List<>));    // list
            return success;
        }

        #endregion FunctionReflection

        public static Type GetGenericViewType(Type realType, Type argumentType) {
            var viewType = Type.GetType(realType.ToString() + "View", false);
            while (viewType == null) {
                var baseType = realType.BaseType;
                var baseTypeString = baseType.ToString();
                var genericIndex = baseTypeString.IndexOf('`');
                Type baseViewType;
                if (genericIndex >= 0) {
                    var genericView = baseTypeString.Substring(0, genericIndex) + "View`1";
                    baseViewType = Type.GetType(genericView);
                } else {
                    baseViewType = Type.GetType(baseTypeString + "View");
                }

                if (baseViewType != null) {
                    if (genericIndex >= 0) {
                        viewType = baseViewType.MakeGenericType(argumentType);
                    } else {
                        viewType = baseViewType;
                    }
                    break;
                }

                realType = baseType;
            }
            return viewType;
        }
    }
}