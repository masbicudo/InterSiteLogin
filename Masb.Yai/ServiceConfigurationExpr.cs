using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Masb.Yai
{
    public class ServiceConfigurationExpr<T>
    {
        private readonly ServiceProvider containerBuilder;

        public ServiceConfigurationExpr(ServiceProvider containerBuilder)
        {
            this.containerBuilder = containerBuilder;
        }

        public ServiceConfigurationExpr<T> ReplaceType<TReplace>([NotNull] Func<ExpressionFilterContext, Type> otherTypeGetter)
        {
            if (otherTypeGetter == null) throw new ArgumentNullException("otherTypeGetter");
            return null;
        }

        public ServiceConfigurationExpr<T2> DecorateWithExpression<T2>(Expression<Func<ExpressionFilterContext<T>, T2>> expression)
            where T2 : T
        {
            return null;
        }
    }
}
