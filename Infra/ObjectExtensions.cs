using System;
using System.Collections.Generic;
using System.Linq;

namespace Infra
{
    public static class ObjectExtensions
    {
        public static IEnumerable<T> GetPath<T>(this T target, Func<T, T> nextFunc)
            where T : class
        {
            var current = target;
            while (current != null)
            {
                yield return current;
                current = nextFunc(current);
            }
        }

        public static IEnumerable<T> GetPath<T>(this T target, Func<T, T> nextFunc, Predicate<T> pathEnd)
        {
            var current = target;
            while (!pathEnd(current))
            {
                yield return current;
                current = nextFunc(current);
            }
        }

        public static TResult With<T, TResult>(this T target, Func<T, TResult> withFunc)
        {
            return withFunc(target);
        }

        public static IEnumerable<TResult> With<T, TResult>(this T target, params Func<T, TResult>[] withFuncs)
        {
            return withFuncs.Select(withFunc => withFunc(target));
        }

        public static void With<T>(this T target, Action<T> withFunc)
        {
            withFunc(target);
        }

        public static void With<T>(this T target, params Action<T>[] withFuncs)
        {
            foreach (var withFunc in withFuncs)
                withFunc(target);
        }
    }
}