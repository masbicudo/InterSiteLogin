using System;
using System.Linq.Expressions;

namespace Masb.Yai
{
    public class DecoratorExpressionFilter<TResult> :
        IExpressionFilter
    {
        private readonly Expression<Func<ExpressionFilterContext<TResult>, TResult>> expression;

        public DecoratorExpressionFilter(Expression<Func<ExpressionFilterContext<TResult>, TResult>> expression)
        {
            this.expression = expression;
        }

        public bool CanProcess(CompositionContext context)
        {
            return typeof(TResult) == context.ComponentType;
        }

        public void Process(ExpressionFilterContext context)
        {
            if (context.Result != null)
                throw new Exception("`NewExpressionFilter` requires `context.Result` to be null.");

            throw new NotImplementedException();
        }

        public Expression<Func<ExpressionFilterContext<TResult>, TResult>> Expression
        {
            get { return this.expression; }
        }

        public FilterGroup DefaultGroup
        {
            get { return FilterGroup.Decorator; }
        }
    }
}