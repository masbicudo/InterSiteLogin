using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Infra
{
    public static class ModelStateDictionaryExtensions
    {
        public static void AddModelError(
            this ModelStateDictionary modelStateDictionary,
            [Localizable(true)] string error,
            params Expression<Func<object>>[] lambdaExpressions)
        {
            foreach (var lambdaExpression in lambdaExpressions)
                modelStateDictionary.AddModelError(GetPropertyName(lambdaExpression), error);
        }

        private static string GetPropertyName(Expression lambdaExpression)
        {
            var lambda = (LambdaExpression)lambdaExpression;
            var inner = lambda.Body.NodeType == ExpressionType.Convert
                ? ((UnaryExpression)lambda.Body).Operand
                : lambda.Body;

            var path = inner.GetPath(
                expr => ((MemberExpression)expr).Expression,
                expr => expr.NodeType != ExpressionType.MemberAccess)
                .Cast<MemberExpression>()
                .Reverse()
                .ToList();

            // if first item in the path is a view-model, we skip it
            bool skipViewModel = path.First().Member
                .With(m => m.Name, m => m.GetUnderlyingType().Name)
                .Where(m => m != null)
                .Any(n => n.EndsWith("ViewModel", StringComparison.InvariantCultureIgnoreCase));

            var result = string.Join(".", (skipViewModel ? path.Skip(1) : path).Select(n => n.Member.Name));

            return result;
        }
    }
}