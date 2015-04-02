using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Masb.Yai.AttributeSources
{
    /// <summary>
    /// An <see cref="IAttributeSource"/> that gets attributes using reflection.
    /// </summary>
    public class ReflectedAttributeSource :
        IAttributeSource
    {
        public IEnumerable<T> GetCustomAttributes<T>(MemberInfo member, bool inherit = false)
            where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }

        public IEnumerable<T> GetCustomAttributes<T>(Type type, bool inherit = false)
            where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }

        public IEnumerable<T> GetCustomAttributes<T>(ParameterInfo parameter, bool inherit = false)
            where T : Attribute
        {
            return parameter.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }
    }
}