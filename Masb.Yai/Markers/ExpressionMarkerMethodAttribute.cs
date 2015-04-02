using System;
using System.Linq.Expressions;

namespace Masb.Yai.Markers
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ExpressionMarkerMethodAttribute : ExpressionMarkerBaseAttribute
    {
        public override Expression GetExpressionFor(
            MethodCallExpression node,
            ExpressionFilterContext parentContext,
            string componentName,
            object info)
        {
            var arguments = node.Method.GetParameters();

            if (arguments.Length == 0)
            {
                var lambda = parentContext.Composer.Compose(node.Type, parentContext, componentName, info);

                if (lambda == null)
                    throw new Exception(
                        string.Format(
                            "Could not compose a component with the given information:\n"
                            + " - Component type: {0}\n"
                            + " - Component name: {1}",
                            node.Type,
                            componentName));

                return lambda.Body;
            }

            if (arguments.Length == 1 && arguments[0].ParameterType == typeof(string))
            {
                var expression = node.Arguments[0];
                var value = Expression.Lambda<Func<string>>(expression).Compile()();

                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception(
                        string.Format(
                            "The marker method `{0}{1}(string)` must be passed an expression that evaluates to a string that is not empty, not white-spaces and not null.\n"
                            + " - invalid: {0}{1}(\"\")\n"
                            + " - invalid: {0}{1}(null)\n"
                            + " - invalid: {0}{1}(AnythingThatReturnsNullOrEmpty())\n"
                            + " - correct: {0}{1}(\"ComponentName\")",
                            node.Method.ReflectedType == null ? "" : node.Method.ReflectedType.Name + '.',
                            node.Method.Name));

                var lambda = parentContext.Composer.Compose(node.Type, parentContext, value, info);
                return lambda.Body;
            }

            if (arguments.Length == 1)
            {
                var returnType = typeof(Expression<>).MakeGenericType(typeof(Func<>).MakeGenericType(node.Type));
                var expectedType1 = typeof(Func<>).MakeGenericType(returnType);
                var expectedType2 = typeof(Func<,>).MakeGenericType(typeof(ExpressionFilterContext), returnType);

                if (arguments[0].ParameterType == expectedType1 || arguments[0].ParameterType == expectedType2)
                {
                    var gotType = arguments[0].ParameterType;
                    var expression = node.Arguments[0];

                    if (gotType == expectedType1)
                    {
                        var emptyLambda = Expression.Lambda(
                            gotType,
                            Expression.Convert(Expression.Constant(null), returnType));

                        var ternary = Expression.Coalesce(expression, emptyLambda);

                        var callToLambda = Expression.Call(
                            ternary,
                            "Invoke",
                            null);

                        var cast = Expression.ConvertChecked(callToLambda, typeof(LambdaExpression));
                        var lambdaReturningFunc = Expression.Lambda(cast);

                        var compiled = (Func<LambdaExpression>)lambdaReturningFunc.Compile();
                        var result = compiled();

                        if (result == null)
                            throw new Exception(
                                string.Format(
                                    "Expression passed to the marker method `{0}{1}(Func<ExpressionFilterContext, Expression<Func<{2}>>>)` must neither evaluate to null nor return null:\n"
                                    + " - invalid: {0}{1}(null)\n"
                                    + " - invalid: {0}{1}(() => null)\n"
                                    + " - invalid: {0}{1}(() => AnythingThatReturnsNull())\n"
                                    + " - correct: {0}{1}(() => Expression.Lambda<Func<Component>>(Expression.Call(typeof(Component), methodName, null)))",
                                    node.Method.ReflectedType == null ? "" : node.Method.ReflectedType.Name + '.',
                                    node.Method.Name,
                                    node.Type.Name));

                        return result.Body;
                    }
                    else
                    {
                        var emptyLambda = Expression.Lambda(
                            gotType,
                            Expression.Convert(Expression.Constant(null), returnType),
                            Expression.Parameter(typeof(ExpressionFilterContext), "c2"));

                        var ternary = Expression.Coalesce(expression, emptyLambda);

                        var param = Expression.Parameter(typeof(ExpressionFilterContext), "cx");
                        var callToLambda = Expression.Call(
                            ternary,
                            "Invoke",
                            null,
                            param);

                        var cast = Expression.ConvertChecked(callToLambda, typeof(LambdaExpression));
                        var lambdaReturningFunc = Expression.Lambda(cast, param);

                        var compiled = (Func<ExpressionFilterContext, LambdaExpression>)lambdaReturningFunc.Compile();
                        var context = ExpressionFilterContextBuilder.Create(node.Type, parentContext, componentName, info);
                        var result = compiled(context);

                        if (result == null)
                            throw new Exception(
                                string.Format(
                                    "Expression passed to the marker method `{0}{1}(Func<ExpressionFilterContext, Expression<Func<{2}>>>)` must neither evaluate to null nor return null:\n"
                                    + " - invalid: {0}{1}(null)\n"
                                    + " - invalid: {0}{1}(c => null)\n"
                                    + " - invalid: {0}{1}(c => AnythingThatReturnsNull())\n"
                                    + " - correct: {0}{1}(c => c.Composer.Compose<ComponentType>())",
                                    node.Method.ReflectedType == null ? "" : node.Method.ReflectedType.Name + '.',
                                    node.Method.Name,
                                    node.Type.Name));

                        return result.Body;
                    }
                }
            }

            throw new NotSupportedException(
                "`ExpressionMarkerMethodAttribute` does not supported the marker method signature.");
        }
    }
}