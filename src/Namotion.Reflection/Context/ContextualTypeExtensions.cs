using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// Type and member extension methods to extract contextual or cached types.
    /// </summary>
    public static class ContextualTypeExtensions
    {
        private static readonly object Lock = new object();
        private static readonly Dictionary<string, object> Cache = new Dictionary<string, object>();

        internal static void ClearCache()
        {
            lock (Lock)
            {
                Cache.Clear();
            }
        }

        /// <summary>
        /// Gets an enumerable of <see cref="ContextualPropertyInfo"/> and <see cref="ContextualFieldInfo"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static IEnumerable<ContextualMemberInfo> GetContextualPropertiesAndFields(this Type type)
        {
            var key = "PropertiesAndFields:" + type.TypeKey();

            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        Cache[key] = type.GetContextualProperties()
                            .OfType<ContextualMemberInfo>()
                            .Union(type.GetContextualFields())
                            .ToArray();
                    }
                }
            }

            return (IEnumerable<ContextualMemberInfo>)Cache[key];
        }

        /// <summary>
        /// Gets an array of <see cref="ContextualParameterInfo"/> for the given <see cref="MethodBase"/> instance.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The runtime properties.</returns>
        public static ContextualParameterInfo[] GetContextualParameters(this MethodBase method)
        {
            var key = "Parameters:" + method.Name + ":" + method.DeclaringType.TypeKey() + ":" + EnumeratedParameters(method.GetParameters());

            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        Cache[key] = method.GetParameters()
                            .Select(p => p.ToContextualParameter())
                            .ToArray();
                    }
                }
            }

            return (ContextualParameterInfo[])Cache[key];
        }

        private static string EnumeratedParameters(ParameterInfo[] parameters)
        {
            return string.Join("-", parameters.Select(p => p.ParameterType.TypeKey()));
        }

        /// <summary>
        /// Gets an array of <see cref="ContextualGenericInfo"/> for the given <see cref="MethodBase"/> instance.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The runtime properties.</returns>
        public static ContextualGenericInfo[] GetContextualGenerics(this MethodBase method) {
            return method.GetGenericArguments()
                .Select(g => g.ToContextualGeneric(method))
                .ToArray();
        }

        /// <summary>
        /// Gets an array of <see cref="ContextualPropertyInfo"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The runtime properties.</returns>
        public static ContextualPropertyInfo[] GetContextualProperties(this Type type)
        {
            var key = "Properties:" + type.TypeKey();

            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
#if NET40
                        Cache[key] = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
#else
                        Cache[key] = type.GetRuntimeProperties()
#endif
                            .Select(p => p.ToContextualProperty()).ToArray();
                    }
                }
            }

            return (ContextualPropertyInfo[])Cache[key];
        }

        /// <summary>
        /// Gets an array of <see cref="ContextualFieldInfo"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The runtime fields.</returns>
        public static ContextualFieldInfo[] GetContextualFields(this Type type)
        {
            var key = "Fields:" + type.TypeKey();
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
#if NET40
                        Cache[key] = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
#else
                        Cache[key] = type.GetRuntimeFields()
#endif
                            .Select(p => p.ToContextualField()).ToArray();
                    }
                }
            }

            return (ContextualFieldInfo[])Cache[key];
        }

        /// <summary>
        /// Gets a <see cref="ContextualParameterInfo"/> for the given <see cref="ParameterInfo"/> instance.
        /// </summary>
        /// <param name="parameterInfo">The parameter info.</param>
        /// <returns>The <see cref="ContextualParameterInfo"/>.</returns>
        public static ContextualParameterInfo ToContextualParameter(this ParameterInfo parameterInfo)
        {
            var key = "Parameter:" + parameterInfo.Name + ":" + parameterInfo.ParameterType.TypeKey() + ":" + parameterInfo.Member.Name + ":" + parameterInfo.Member.DeclaringType.TypeKey();
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        var index = 0;
                        Cache[key] = new ContextualParameterInfo(parameterInfo, ref index);
                    }

                }
            }

            return (ContextualParameterInfo)Cache[key];
        }

        /// <summary>
        /// Gets a <see cref="ContextualGenericInfo"/> for the given <see cref="ParameterInfo"/> instance.
        /// </summary>
        /// <param name="typeInfo">The generic type.</param>
        /// <param name="method">The method with generic types</param>
        /// <returns>The <see cref="ContextualGenericInfo"/>.</returns>
        public static ContextualGenericInfo ToContextualGeneric(this Type typeInfo, MethodBase method) {
            var index = 0;
            return new ContextualGenericInfo(typeInfo, method, ref index);
        }

        /// <summary>
        /// Gets a <see cref="ContextualPropertyInfo"/> for the given <see cref="PropertyInfo"/> instance.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns>The <see cref="ContextualPropertyInfo"/>.</returns>
        public static ContextualPropertyInfo ToContextualProperty(this PropertyInfo propertyInfo)
        {
            var key = "Property:" + propertyInfo.Name + ":" + propertyInfo.DeclaringType.TypeKey();
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        var index = 0;
                        Cache[key] = new ContextualPropertyInfo(propertyInfo, ref index);
                    }

                    return (ContextualPropertyInfo)Cache[key];
                }
            }

            return (ContextualPropertyInfo)Cache[key];
        }

        /// <summary>
        /// Gets a <see cref="ContextualFieldInfo"/> for the given <see cref="FieldInfo"/> instance.
        /// </summary>
        /// <param name="fieldInfo">The field info.</param>
        /// <returns>The <see cref="ContextualFieldInfo"/>.</returns>
        public static ContextualFieldInfo ToContextualField(this FieldInfo fieldInfo)
        {
            var key = "Field:" + fieldInfo.Name + ":" + fieldInfo.DeclaringType.TypeKey();
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        var index = 0;
                        Cache[key] = new ContextualFieldInfo(fieldInfo, ref index);
                    }
                }
            }

            return (ContextualFieldInfo)Cache[key];
        }

        /// <summary>
        /// Gets a <see cref="ContextualMemberInfo"/> for the given <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The <see cref="ContextualMemberInfo"/>.</returns>
        public static ContextualMemberInfo ToContextualMember(this MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.ToContextualProperty();
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.ToContextualField();
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets an uncached <see cref="ContextualType"/> for the given <see cref="Type"/> instance and attributes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns>The <see cref="CachedType"/>.</returns>
        public static ContextualType ToContextualType(this Type type, IEnumerable<Attribute> attributes)
        {
            // TODO: Is there a way to cache these contextual types?
            return ContextualType.ForType(type, attributes);
        }

        /// <summary>
        /// Gets a <see cref="CachedType"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="CachedType"/>.</returns>
        public static ContextualType ToContextualType(this Type type)
        {
            if (type.FullName == null) // Generic types are not cached
                return ContextualType.ForType(type, new Attribute[0]);

            var key = "Type:Context:" + type.TypeKey();
            lock (Lock)
            {
                if (!Cache.ContainsKey(key))
                {
                    Cache[key] = ContextualType.ForType(type, new Attribute[0]);
                }

                return (ContextualType)Cache[key];
            }
        }

        /// <summary>
        /// Gets a <see cref="CachedType"/> for the given <see cref="Type"/> instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The <see cref="CachedType"/>.</returns>
        public static CachedType ToCachedType(this Type type)
        {
            if (type.FullName == null) // Generic types are not cached
                return new CachedType(type);

            var key = "Type:" + type.TypeKey();
            if (!Cache.ContainsKey(key))
            {
                lock (Lock)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        Cache[key] = new CachedType(type);
                    }
                }
            }

            return (CachedType)Cache[key];
        }
    }
}
