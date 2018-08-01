using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Starlite.Raven {

    public static class RavenUtility {
        public static readonly ulong s_InvalidHash = HashString("INVALID_PROPERTY");

#if UNITY_EDITOR

        public enum EMemberType {
            FieldOrProperty,
            Function
        }

        public const int c_MaxFunctionParameters = 10;

        public static readonly Type s_DefaultFallbackType = typeof(int);

        private static readonly Dictionary<string, Type> s_TypeCache = new Dictionary<string, Type>();
        private static readonly Dictionary<string, Type> s_BaseTypeCache = new Dictionary<string, Type>();
        private static readonly Dictionary<Type, List<Type>> s_FinalTypeCache = new Dictionary<Type, List<Type>>();

        #region Type

        public static Type GetTypeFromLoadedAssemblies(string typeName) {
            Type type = null;
            if (s_TypeCache.TryGetValue(typeName, out type)) {
                return type;
            }

            // generic type needs special handling because we don't serialize assembly qualified names...
            var indexOfGeneric = typeName.IndexOf('`');
            var endOfOuterType = indexOfGeneric + 2;
            if (indexOfGeneric != -1 && typeName.Length > endOfOuterType) {
                var outerTypeName = typeName.Substring(0, endOfOuterType);
                var nParams = int.Parse(outerTypeName.Substring(indexOfGeneric + 1, endOfOuterType - indexOfGeneric - 1));
                if (nParams > 1) {
                    s_TypeCache[typeName] = null;
                    return null;
                }

                var outerType = GetTypeFromLoadedAssemblies(outerTypeName);
                if (outerType == null) {
                    s_TypeCache[typeName] = null;
                    return null;
                }

                var innerTypeStartIndex = typeName.IndexOf('[') + 1;
                var innerTypeName = typeName.Substring(innerTypeStartIndex, typeName.IndexOf(']') - innerTypeStartIndex);
                var innerType = GetTypeFromLoadedAssemblies(innerTypeName);
                if (innerType == null) {
                    s_TypeCache[typeName] = null;
                    return null;
                }

                var finalType = outerType.MakeGenericType(innerType);
                s_TypeCache[typeName] = finalType;
                return finalType;
            }

            var asses = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < asses.Length; ++i) {
                var t = asses[i].GetType(typeName);
                if (t != null) {
                    s_TypeCache[typeName] = t;
                    return t;
                }
            }

            s_TypeCache[typeName] = null;
            return null;
        }

        public static List<Type> GetTypesForGenericType(string typeName) {
            var types = new List<Type>();
            for (int i = 0; i < c_MaxFunctionParameters; ++i) {
                var type = GetTypeFromLoadedAssemblies(GetGenericTypenameForParameters(typeName, i));
                if (type != null) {
                    types.Add(type);
                }
            }

            return types;
        }

        public static Type GetBaseTypeForMemberInType(Type type, string member, EMemberType memberType, params string[] argTypes) {
            var fullName = type.ToString() + "|" + member;
            if (type == null) {
                s_BaseTypeCache[fullName] = null;
                return null;
            }

            Type existingType;
            if (s_BaseTypeCache.TryGetValue(fullName, out existingType)) {
                return existingType;
            }

            var baseType = type.BaseType;
            while (baseType != null) {
                if (baseType.IsGenericTypeDefinition) {
                    break;
                }

                var memberInfos = baseType.GetMembers(BindingFlags.Public | BindingFlags.Instance);
                bool found = false;
                for (int i = 0; i < memberInfos.Length; ++i) {
                    var memberInfo = memberInfos[i];
                    if (memberInfo.Name != member) {
                        continue;
                    }

                    switch (memberInfo.MemberType) {
                        case MemberTypes.Field:
                            if (memberType != EMemberType.FieldOrProperty) {
                                continue;
                            }

                            var fieldInfo = memberInfo as FieldInfo;

                            if (fieldInfo.FieldType.IsAssignableFrom(GetTypeFromLoadedAssemblies(argTypes[0]))) {
                                found = true;
                            }
                            break;

                        case MemberTypes.Property:
                            if (memberType != EMemberType.FieldOrProperty) {
                                continue;
                            }

                            var propertyInfo = memberInfo as PropertyInfo;
                            if (propertyInfo.PropertyType.IsAssignableFrom(GetTypeFromLoadedAssemblies(argTypes[0]))) {
                                found = true;
                            }
                            break;

                        case MemberTypes.Method:
                            if (memberType != EMemberType.Function) {
                                continue;
                            }

                            var methodInfo = memberInfo as MethodInfo;
                            if (argTypes.Length == 1 && string.IsNullOrEmpty(argTypes[0])) {
                                argTypes = new string[0];
                            }
                            var parameterTypes = argTypes.Select((x) => GetTypeFromLoadedAssemblies(x)).ToArray();
                            var methodParameters = methodInfo.GetParameters();
                            if (methodParameters.Length == parameterTypes.Length) {
                                var matchingParameters = true;
                                for (int j = 0; j < parameterTypes.Length; ++j) {
                                    if (methodParameters[j].ParameterType != parameterTypes[j]) {
                                        matchingParameters = false;
                                        break;
                                    }
                                }
                                if (matchingParameters) {
                                    found = true;
                                }
                            }
                            break;
                    }

                    if (found) {
                        break;
                    }
                }

                if (found) {
                    type = baseType;
                    baseType = baseType.BaseType;
                } else {
                    break;
                }
            }

            s_BaseTypeCache[fullName] = type;
            return type;
        }

        public static Type GetBaseTypeForMemberInType(string typeName, string member, EMemberType memberType, params string[] argTypes) {
            return GetBaseTypeForMemberInType(GetTypeFromLoadedAssemblies(typeName), member, memberType, argTypes);
        }

        public static List<Type> GetFinalTypesForGenericType(string typeName, int nParameters, bool sealedOnly = true) {
            var types = new List<Type>();
            var genericType = GetTypeFromLoadedAssemblies(GetGenericTypenameForParameters(typeName, nParameters));
            if (genericType == null) {
                return types;
            }

            return GetFinalTypesForGenericType(genericType, sealedOnly);
        }

        public static List<Type> GetFinalTypesForGenericType(Type genericType, bool sealedOnly = true) {
            List<Type> types;
            if (s_FinalTypeCache.TryGetValue(genericType, out types)) {
                return types;
            }

            types = new List<Type>();

            var asses = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < asses.Length; ++i) {
                var ass = asses[i];
                var assTypes = ass.GetTypes();
                for (int j = 0; j < assTypes.Length; ++j) {
                    var type = assTypes[j];
                    if (type.IsAbstract || sealedOnly && !type.IsSealed) {
                        continue;
                    }

                    var foundBase = IsSubclassOfGeneric(type.BaseType, genericType);
                    if (foundBase) {
                        types.Add(type);
                    }
                }
            }

            s_FinalTypeCache[genericType] = types;
            return types;
        }

        public static bool IsSubclassOfGeneric(Type type, Type genericType) {
            if (genericType == null) {
                return false;
            }

            while (type != null && type != typeof(object)) {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (genericType == cur) {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        public static Type GetTheMostSpecializedTypeAmongTwoSimilarTypes(Type mostSpecializedType, Type wantedType) {
            if (mostSpecializedType == wantedType) {
                return mostSpecializedType;
            }

            // if mostSpecializedType is a specialization of wantedType, return the most specialized type
            if (IsSubclassOfGeneric(mostSpecializedType, wantedType)) {
                return mostSpecializedType;
            }

            // if wantedType is a specialization of mostSpecializedType, return the least specialized type
            if (IsSubclassOfGeneric(wantedType, mostSpecializedType)) {
                return mostSpecializedType;
            }

            // if none of the above is true, find the most base class of wantedType that's not generic
            // skipping various null/generic checks here because assume user not noob (...)
            var baseWantedType = wantedType.BaseType;
            while (!baseWantedType.IsGenericType) {
                wantedType = baseWantedType;
                baseWantedType = baseWantedType.BaseType;
            }

            return wantedType;
        }

        public static string[] GetParameterNamesForFunction(string componentType, string functionName, params string[] parameterTypes) {
            if (parameterTypes.Length == 0 || string.IsNullOrEmpty(parameterTypes[0])) {
                return new string[0];
            }

            var type = GetTypeFromLoadedAssemblies(componentType);
            var paramTypes = parameterTypes.Select(x => GetTypeFromLoadedAssemblies(x)).ToArray();
            var methodInfo = type.GetMethod(functionName, paramTypes);

            if (methodInfo == null) {
                return new string[0];
            }
            var parameters = methodInfo.GetParameters();
            return parameters.Select(x => x.Name).ToArray();
        }

        public static bool GenericTypeExists(string typeName, int nParameters) {
            var type = GetTypeFromLoadedAssemblies(GetGenericTypenameForParameters(typeName, nParameters));
            return type != null;
        }

        public static string GetTypeWithoutNamespace(string typeName) {
            return typeName.Split('.').Last();
        }

        public static int GetNumberOfParametersForGenericTypeName(string typeName) {
            var charIndex = typeName.IndexOf('`');
            if (charIndex == -1) {
                return -1;
            }
            var startIndex = charIndex + 1;
            var endIndex = typeName.IndexOf('[');
            return int.Parse(typeName.Substring(startIndex, endIndex == -1 ? (typeName.Length - startIndex) : (endIndex - startIndex)));
        }

        private static string GetGenericTypenameForParameters(string typeName, int nParameters) {
            return typeName + "`" + nParameters;
        }

        public static string GetTemplatedGenericFunction(Type genericType) {
            if (!genericType.IsGenericType) {
                return genericType.ToString();
            }

            var fullName = genericType.ToString();
            var nArguments = GetNumberOfParametersForGenericTypeName(fullName);
            var plainName = fullName.Substring(0, fullName.IndexOf('`'));
            return plainName + GetTemplateTypesForTemplateArguments(nArguments, genericType.GetGenericArguments().Select((x) => GetTypeableTypeFromOriginal(x.ToString())).ToArray());
        }

        public static string GetTemplateTypesForTemplateArguments(int nArguments, params string[] args) {
            var sb = new StringBuilder();
            for (int j = 0; j < nArguments; ++j) {
                if (j == 0) {
                    sb.Append("<");
                } else {
                    sb.Append(", ");
                }

                if (j < args.Length && !string.IsNullOrEmpty(args[j])) {
                    sb.Append(args[j]);
                } else {
                    sb.Append("T");
                    sb.Append(j.ToString());
                }

                if (j == nArguments - 1) {
                    sb.Append(">");
                }
            }
            return sb.ToString();
        }

        public static string GetTemplateTypesForTypeArguments(string packedParameters) {
            if (string.IsNullOrEmpty(packedParameters)) {
                return string.Empty;
            }

            var unpackedArgs = GetFunctionParameterTypesUnpacked(packedParameters);
            return GetTemplateTypesForTemplateArguments(unpackedArgs.Length, unpackedArgs);
        }

        public static string GetFunctionParameterTypesPacked(MethodInfo mi) {
            var sb = new StringBuilder();
            var parameters = mi.GetParameters();
            var counter = 0;
            var isExtensionMethod = mi.IsDefined(typeof(ExtensionAttribute), true);
            for (int i = isExtensionMethod ? 1 : 0; i < parameters.Length; ++i) {
                if (counter != 0) {
                    sb.Append(",");
                }
                sb.Append(parameters[i].ParameterType.ToString());
                ++counter;
            }

            return sb.ToString();
        }

        public static string GetFunctionParameterTypesPacked(string[] packedParameters) {
            return string.Join(",", packedParameters);
        }

        public static string[] GetFunctionParameterTypesUnpacked(string packedParameters) {
            return packedParameters.Split(',');
        }

        public static string GetFunctionNameFromPackedFunctionName(string packedName) {
            return packedName.Split('|')[0];
        }

        public static string GetFunctionParametersFromPackedFunctionName(string packedName) {
            return packedName.Split('|')[1];
        }

        public static string GetComponentNameFromFullFunctionName(string fullFunctionName) {
            return fullFunctionName.Split('|')[0];
        }

        public static string GetFunctionParametersFromFullFunctionName(string fullFunctionName) {
            return fullFunctionName.Split('|')[2];
        }

        public static string GetComponentNameFromFullMemberName(string fullMemberName) {
            return fullMemberName.Split('|')[0];
        }

        public static string GetMemberNameFromFullMemberName(string fullMemberName) {
            return fullMemberName.Split('|')[1];
        }

        public static string GetPackedFunctionNameFromFullFunctionName(string fullFunctionName) {
            return string.Join("|", fullFunctionName.Split('|').Skip(1).ToArray());
        }

        public static string CombineComponentTypeAndFunctionName(string componentType, string functionName) {
            return componentType + "|" + functionName;
        }

        public static string StitchTypeAndMember(string type, string member) {
            return type + "." + member;
        }

        public static string GetTypeableTypeFromOriginal(string typeName) {
            var subClassTypesFix = typeName.Replace('+', '.');
            var regex1 = new System.Text.RegularExpressions.Regex("`.+?\\[(.+?)\\]");
            var genericFix = regex1.Replace(subClassTypesFix, "<$1>");
            return genericFix;
        }

        #endregion Type

#endif

        /// <summary>
        /// UTF8 strings only.
        /// </summary>
        public static ulong HashString(string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
#if STARLITE_EDITOR
            return Starlite.StarliteEditor.Hash64(bytes, bytes.Length, 0x1337);
#else
            return Outfit7.Util.NativeUtils.xxHash64(bytes, bytes.Length, 0x1337);
#endif
        }
    }
}