using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Masb.Yai.AttributeSources
{
    internal static class ReflectionExtensions
    {
        public static IEnumerable<MethodInfo> GetBaseMethods([NotNull] this MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            var currentType = method.DeclaringType;
            var baseMethod = method.GetBaseDefinition();
            if (currentType == null)
                return Enumerable.Empty<MethodInfo>();

            var flags = (method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var baseMethods = currentType.GetBaseTypes()
                .SelectMany(
                    bt => bt.GetMethods(flags)
                        .Where(bm => bm.GetBaseDefinition().MethodHandle == baseMethod.MethodHandle));

            return baseMethods;
        }

        public static IEnumerable<EventInfo> GetBaseEvents([NotNull] this EventInfo @event)
        {
            var method = @event.GetAddMethod(true);
            var result = method.GetBaseMethods().Select(x => x.GetAssociatedEvent());
            return result;
        }

        public static IEnumerable<PropertyInfo> GetBaseProperties([NotNull] this PropertyInfo property)
        {
            var method = property.GetAccessors(true).First();
            var result = method.GetBaseMethods().Select(x => x.GetAssociatedProperty());
            return result;
        }

        public static IEnumerable<Type> GetBaseTypes([NotNull] this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var currentType = type;
            while (true)
            {
                currentType = currentType.BaseType;
                if (currentType == null)
                    break;
                yield return currentType;
            }
        }

        public static EventInfo GetAssociatedEvent([NotNull] this MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            var declType = method.DeclaringType;
            if (declType == null)
                return null;

            var flags = (method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var events = declType.GetEvents(flags).SingleOrDefault(
                evt => evt.GetAddMethod(!method.IsPublic).MethodHandle == method.MethodHandle
                       || evt.GetRemoveMethod(!method.IsPublic).MethodHandle == method.MethodHandle
                       || evt.GetRaiseMethod(!method.IsPublic).MethodHandle == method.MethodHandle);

            return events;
        }

        public static PropertyInfo GetAssociatedProperty([NotNull] this MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            var declType = method.DeclaringType;
            if (declType == null)
                return null;

            var flags = (method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var props = declType.GetProperties(flags).SingleOrDefault(
                p => p.GetAccessors(!method.IsPublic).Any(m => m.MethodHandle == method.MethodHandle));

            return props;
        }
    }
}
