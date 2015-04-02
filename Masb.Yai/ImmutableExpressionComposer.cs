using System;
using System.Linq.Expressions;

namespace Masb.Yai
{
    /// <summary>
    /// An <see cref="ExpressionComposer"/> that is immutable, that is,
    /// it does not allow insertion of new expression filters.
    /// <para>Immutable objects are always thread safe.</para>
    /// <para>Note that the source expression filters can still be mutated.</para>
    /// </summary>
    public sealed class ImmutableExpressionComposer : ExpressionComposer
    {
        private readonly Action<ExpressionFilterContext> filterAction;

        public ImmutableExpressionComposer(Action<ExpressionFilterContext> filterAction)
        {
            this.filterAction = filterAction;
        }

        public override Expression<Func<TResult>> Compose<TResult>()
        {
            var context = new ExpressionFilterContext<TResult>(this);
            this.filterAction(context);
            return context.Result;
        }

        public override LambdaExpression Compose(Type componentType)
        {
            var context = ExpressionFilterContextBuilder.Create(componentType, this);
            this.filterAction(context);
            return context.Result;
        }

        public override Expression<Func<TResult>> Compose<TResult>(
            CompositionContext parentContext,
            string componentName,
            object reflectedDestinationInfo)
        {
            var context = new ExpressionFilterContext<TResult>(parentContext, componentName, reflectedDestinationInfo);
            this.filterAction(context);
            return context.Result;
        }

        public override LambdaExpression Compose(Type componentType, CompositionContext parentContext, string componentName, object reflectedDestinationInfo)
        {
            var context =
                ExpressionFilterContextBuilder.Create(
                    componentType,
                    parentContext,
                    componentName,
                    reflectedDestinationInfo);

            this.filterAction(context);
            return context.Result;
        }
    }
}