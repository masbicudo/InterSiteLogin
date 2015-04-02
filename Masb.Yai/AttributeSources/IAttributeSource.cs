using System;
using System.Collections.Generic;
using System.Reflection;

namespace Masb.Yai.AttributeSources
{
    public interface IAttributeSource
    {
        IEnumerable<T> GetCustomAttributes<T>(MemberInfo member, bool inherit = false) where T : Attribute;

        IEnumerable<T> GetCustomAttributes<T>(Type type, bool inherit = false) where T : Attribute;

        IEnumerable<T> GetCustomAttributes<T>(ParameterInfo parameter, bool inherit = false) where T : Attribute;
    }
}