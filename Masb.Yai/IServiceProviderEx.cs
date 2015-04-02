using System;
using System.Linq.Expressions;

namespace Masb.Yai
{
    public interface IServiceProviderEx : IServiceProvider
    {
        Func<object> GetServiceFunc(Type serviceType);

        Expression GetServiceExpression(Type serviceType);

        bool TryGetService<T>(out T outService);

        bool TryGetServiceFunc<T>(out Func<T> outGetter);

        bool TryGetServiceExpression<T>(out Expression<Func<T>> outExpression);

        T GetService<T>();

        Func<T> GetServiceFunc<T>();

        Expression<Func<T>> GetServiceExpression<T>();
    }
}