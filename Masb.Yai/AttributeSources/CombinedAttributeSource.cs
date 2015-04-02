using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Masb.Yai.AttributeSources
{
    /// <summary>
    /// An <see cref="IAttributeSource"/> that gets attributes from multiple other <see cref="IAttributeSource"/> objects.
    /// </summary>
    public class CombinedAttributeSource :
        IAttributeSource
    {
        private readonly IAttributeSource[] list;

        public CombinedAttributeSource(IAttributeSource[] list)
        {
            this.list = list;
        }

        public IEnumerable<T> GetCustomAttributes<T>(MemberInfo member, bool inherit = false) where T : Attribute
        {
            var listOfLists = this.list.Select(each => each.GetCustomAttributes<T>(member, inherit));
            var result = Aggregate(listOfLists);
            return result;
        }

        public IEnumerable<T> GetCustomAttributes<T>(Type type, bool inherit = false) where T : Attribute
        {
            var listOfLists = this.list.Select(each => each.GetCustomAttributes<T>(type, inherit));
            var result = Aggregate(listOfLists);
            return result;
        }

        public IEnumerable<T> GetCustomAttributes<T>(ParameterInfo parameter, bool inherit = false) where T : Attribute
        {
            var listOfLists = this.list.Select(each => each.GetCustomAttributes<T>(parameter, inherit));
            var result = Aggregate(listOfLists);
            return result;
        }

        private static T[] Aggregate<T>(IEnumerable<IEnumerable<T>> listOfLists) where T : Attribute
        {
            return listOfLists
                .Aggregate(
                    Enumerable.Empty<Attribute>(),
                    (current, attrs) => current.Union(attrs.OfType<Attribute>(), AttributeComparer.Instance))
                .OfType<T>()
                .ToArray();
        }

        private class AttributeComparer :
            IEqualityComparer<Attribute>
        {
            public static readonly AttributeComparer Instance = new AttributeComparer();

            public bool Equals(Attribute x, Attribute y)
            {
                var typeX = x.GetType();
                var typeY = y.GetType();

                if (typeX != typeY)
                    return false;

                var usage = typeX.GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                    .OfType<AttributeUsageAttribute>()
                    .Single();

                if (!usage.AllowMultiple)
                    return false;

                return true;
            }

            public int GetHashCode(Attribute obj)
            {
                return obj.GetType().GetHashCode();
            }
        }
    }
}