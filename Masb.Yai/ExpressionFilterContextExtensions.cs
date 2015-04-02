using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Masb.Yai
{
    public static class ExpressionFilterContextExtensions
    {
        public static bool IsComponent<TComponent>([NotNull] this ExpressionFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            return context.ComponentType == typeof(TComponent);
        }

        public static bool IsComponent<TComponent>([NotNull] this ExpressionFilterContext context, string componentName)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            return context.ComponentType == typeof(TComponent) && context.ComponentName == componentName;
        }

        public static void SetResult<TComponent>([NotNull] this ExpressionFilterContext context, Expression<Func<TComponent>> expression)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.Result = expression;
        }
    }
}