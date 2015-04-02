using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Masb.Yai.Markers
{
    public abstract class ExpressionMarkerBaseAttribute : Attribute
    {
        [NotNull]
        public abstract Expression GetExpressionFor(
            [NotNull] MethodCallExpression node,
            [NotNull] ExpressionFilterContext parentContext,
            string componentName,
            object info);
    }
}