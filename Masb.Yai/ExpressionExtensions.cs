using System;
using System.Linq.Expressions;

namespace Masb.Yai
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<Func<T>>> ToFuncExpressionOfFunc<T>(this Expression<Func<T>> expression)
        {
            return null;
        }

        public static Expression<Func<Expression<Func<T>>>> ToFuncExpressionOfFuncExpression<T>(this Expression<Func<T>> expression)
        {
            return null;
        }
    }
}