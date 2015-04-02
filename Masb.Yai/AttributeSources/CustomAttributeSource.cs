using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Masb.Yai.AttributeSources
{
    /// <summary>
    /// An <see cref="IAttributeSource"/> that gets attributes added manually.
    /// </summary>
    public class CustomAttributeSource :
        IAttributeSource,
        IDisposable
    {
        private readonly Dictionary<object, List<Attribute>> dictionary = new Dictionary<object, List<Attribute>>();
        private readonly ReadWriteLock rwlock = new ReadWriteLock();

        public void Add(MemberInfo member, Attribute attribute)
        {
            this.Add((object)member, attribute);
        }

        public void Add(Type type, Attribute attribute)
        {
            this.Add((object)type, attribute);
        }

        public void Add(ParameterInfo parameter, Attribute attribute)
        {
            this.Add((object)parameter, attribute);
        }

        private void Add(object obj, Attribute attribute)
        {
            this.rwlock.WriteWith(() =>
            {
                List<Attribute> list;
                if (!this.dictionary.TryGetValue(obj, out list))
                    this.dictionary[obj] = list = new List<Attribute>();

                list.Add(attribute);
            });
        }

        private IEnumerable<T> Get<T>(object obj)
            where T : Attribute
        {
            var result = this.rwlock.ReadWith(() =>
            {
                List<Attribute> list;
                this.dictionary.TryGetValue(obj, out list);
                return list != null ? list.OfType<T>() : Enumerable.Empty<T>();
            });

            return result;
        }

        public IEnumerable<T> GetCustomAttributes<T>(MemberInfo member, bool inherit = false)
            where T : Attribute
        {
            IEnumerable<T> result = this.Get<T>(member);

            IEnumerable<T> inherited = null;
            if (inherit)
            {
                MethodInfo asMethod;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (inherited == null && (asMethod = member as MethodInfo) != null)
                {
                    var baseMethods = asMethod.GetBaseMethods();
                    inherited = baseMethods.SelectMany(m => this.GetCustomAttributes<T>(m));
                }

                EventInfo asEvent;
                if (inherited == null && (asEvent = member as EventInfo) != null)
                {
                    var baseEvents = asEvent.GetBaseEvents();
                    inherited = baseEvents.SelectMany(e => this.GetCustomAttributes<T>(e));
                }

                PropertyInfo asProperty;
                if (inherited == null && (asProperty = member as PropertyInfo) != null)
                {
                    var baseProps = asProperty.GetBaseProperties();
                    inherited = baseProps.SelectMany(p => this.GetCustomAttributes<T>(p));
                }

                if (inherited != null)
                {
                    inherited = inherited
                        .Where(
                            x => x.GetType()
                                .GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                                .OfType<AttributeUsageAttribute>()
                                .First()
                                .Inherited);
                }
            }

            return inherited == null ? result : result.Concat(inherited);
        }

        public IEnumerable<T> GetCustomAttributes<T>(Type type, bool inherit = false)
            where T : Attribute
        {
            lock (this.dictionary)
            {
                var result = this.Get<T>(type);
                return result;
            }
        }

        public IEnumerable<T> GetCustomAttributes<T>(ParameterInfo parameter, bool inherit = false)
            where T : Attribute
        {
            lock (this.dictionary)
            {
                IEnumerable<T> result = this.Get<T>(parameter);
                IEnumerable<T> inherited = null;
                if (inherit)
                {
                    bool isReturnParam = (parameter.Attributes & ParameterAttributes.Retval) != 0;

                    var member = parameter.Member;

                    MethodInfo asMethod;
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (inherited == null && (asMethod = member as MethodInfo) != null)
                    {
                        var baseMethods = asMethod.GetBaseMethods();
                        inherited = baseMethods.SelectMany(
                            m => this.GetCustomAttributes<T>(
                                isReturnParam ? m.ReturnParameter : m.GetParameters()[parameter.Position]));
                    }

                    PropertyInfo asProperty;
                    if (inherited == null && (asProperty = member as PropertyInfo) != null)
                    {
                        var baseProps = asProperty.GetBaseProperties();
                        inherited = baseProps.SelectMany(p => this.GetCustomAttributes<T>(
                            p.GetIndexParameters()[parameter.Position]));
                    }

                    if (inherited != null)
                    {
                        inherited = inherited
                            .Where(
                                x => x.GetType()
                                    .GetCustomAttributes(typeof(AttributeUsageAttribute), true)
                                    .OfType<AttributeUsageAttribute>()
                                    .First()
                                    .Inherited);
                    }
                }

                return inherited == null ? result : result.Concat(inherited);
            }
        }

        public void Dispose()
        {
            if (this.rwlock != null)
                this.rwlock.Dispose();
        }
    }
}