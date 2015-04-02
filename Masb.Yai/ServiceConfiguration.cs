using System;
using System.Linq.Expressions;

namespace Masb.Yai
{
    public class ServiceConfiguration<T>
    {
        private readonly ServiceProvider containerBuilder;

        public ServiceConfiguration(ServiceProvider containerBuilder)
        {
            this.containerBuilder = containerBuilder;
        }

        public ServiceConfigurationExpr<T2> WithExpression<T2>(Expression<Func<T2>> expression)
            where T2 : T
        {
            return null;
        }
    }
}