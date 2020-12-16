using System;
using System.Linq;
using System.Reflection;

namespace Namotion.Reflection
{
    /// <summary>
    /// A generic info with contextual information.
    /// </summary>
    public class ContextualGenericInfo : ContextualType
    {
        private string _name;

        internal ContextualGenericInfo(Type genericType, MethodBase method, ref int nullableFlagsIndex)
            : base(genericType, GetContextualAttributes(genericType),
                null, null, ref nullableFlagsIndex,
                method.DeclaringType.IsNested ?
                    new dynamic[] { genericType, method, genericType.DeclaringType, genericType.DeclaringType.DeclaringType, genericType.DeclaringType.GetTypeInfo().Assembly } :
                    new dynamic[] { genericType, method, genericType.DeclaringType, genericType.DeclaringType.GetTypeInfo().Assembly })
        {
            GenericType = genericType;
            Method = method;
        }

        /// <summary>
        /// Gets the type context's parameter info.
        /// </summary>
        public Type GenericType { get; }

        private MethodBase Method { get; }

        /// <summary>
        /// Gets the cached parameter name.
        /// </summary>
        public string Name => _name ?? (_name = GenericType.Name);

        /// <inheritdocs />
        public override string ToString()
        {
            return Name + " (Generic) - " + base.ToString();
        }

        private static Attribute[] GetContextualAttributes(Type genericType)
        {
            try
            {
                return genericType.GetTypeInfo().GetCustomAttributes(true).OfType<Attribute>().ToArray();
            }
            catch
            {
                // Needed for legacy runtimes
                return genericType.GetTypeInfo().GetCustomAttributes(false).OfType<Attribute>().ToArray();
            }
        }
    }
}
