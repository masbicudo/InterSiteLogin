using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Masb.Yai
{
    public class TypedAndNamedCollection
    {
        private Dictionary<object, object> data;

        public T GetOrCreateObject<T>([NotNull] Func<T> creator)
        {
            return this.GetOrCreateObject(null, creator);
        }

        public T GetOrCreateObject<T>(string name, [NotNull] Func<T> creator)
        {
            if (creator == null)
                throw new ArgumentNullException("creator");

            var dt = this.data;
            if (dt == null)
                this.data = dt = new Dictionary<object, object>();

            var key = new { type = typeof(T), name };
            object obj;
            if (dt.TryGetValue(key, out obj))
                return (T)obj;

            T result = creator();
            dt.Add(key, result);

            return result;
        }
    }
}