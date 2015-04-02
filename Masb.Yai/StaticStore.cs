using System;
using System.Linq.Expressions;

namespace Masb.Yai
{
    public static class StaticStore
    {
        public static T GetOrCreate<T>(Func<T> func)
        {
            if (Inner<T>.HasValue)
                return Inner<T>.Value;

            Inner<T>.SetVaue(func());
            return Inner<T>.Value;
        }

        public static Expression<Func<T>> GetExpression<T>()
        {
            return () => GetInstance<T>();
        }

        public static Expression<Func<T>> GetExpression<T>(Expression<Func<T>> expression)
        {
            Func<T> marker = null;
            Expression<Func<T>> result = () => HasInstance<T>() ? GetInstanceOrDefault<T>() : GetOrCreate(marker);

            // replace 'marker' closure with 'expression' body
            var exprOfFunc = expression.ToFuncExpressionOfFunc();
            var replacer = new ConstantReplacerVisitor(exprOfFunc.Body);
            result = (Expression<Func<T>>)replacer.Visit(result);

            return result;
        }

        public static T GetInstance<T>()
        {
            if (!Inner<T>.HasValue)
                throw new Exception("Cannot get the singleton instance: not yet initialized.");

            return Inner<T>.Value;
        }

        public static T GetInstanceOrDefault<T>()
        {
            return Inner<T>.Value;
        }

        public static void SetInstance<T>(T instance)
        {
            if (Inner<T>.HasValue)
                throw new Exception("Cannot set the singleton instance: already initialized.");

            Inner<T>.SetVaue(instance);
        }

        public static bool HasInstance<T>()
        {
            return Inner<T>.HasValue;
        }

        private static class Inner<T>
        {
            // ReSharper disable once StaticFieldInGenericType - each type must have it's own locker
            private static readonly object locker = new object();

            // ReSharper disable once StaticFieldInGenericType - each type must have it's own flag
            private static bool hasValue;

            // ReSharper disable once StaticFieldInGenericType - each type must have it's own value
            private static T value;

            public static bool HasValue
            {
                get { return hasValue; }
            }

            public static T Value
            {
                get { return value; }
            }

            public static void SetVaue(T singleValue)
            {
                lock (locker)
                    if (!hasValue)
                    {
                        value = singleValue;
                        hasValue = true;
                    }
            }
        }

        private class ConstantReplacerVisitor : ExpressionVisitor
        {
            private readonly Expression replacerExpression;

            public ConstantReplacerVisitor(Expression replacerExpression)
            {
                this.replacerExpression = replacerExpression;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                return this.replacerExpression;
            }
        }
    }
}