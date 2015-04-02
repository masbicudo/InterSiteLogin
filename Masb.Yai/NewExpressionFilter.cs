using System;
using System.Linq.Expressions;

namespace Masb.Yai
{
    /// <summary>
    /// An <see cref="IExpressionFilter"/> that initialize the expression for a component of type <typeparamref name="TComponent"/>.
    /// </summary>
    /// <typeparam name="TComponent">Type of the component to initialize the expression.</typeparam>
    public class NewExpressionFilter<TComponent> :
        IExpressionFilter
    {
        private readonly Expression<Func<TComponent>> expression;

        public NewExpressionFilter(Expression<Func<TComponent>> expression)
        {
            this.expression = expression;
        }

        public bool CanProcess(CompositionContext context)
        {
            return typeof(TComponent) == context.ComponentType;
        }

        public void Process(ExpressionFilterContext context)
        {
            if (context.Result != null)
                throw new Exception("`NewExpressionFilter` requires `context.Result` to be null.");

            LambdaExpression result = this.expression;

            context.Result = result;
        }

        public FilterGroup DefaultGroup
        {
            get { return FilterGroup.Initializer; }
        }

        public Expression<Func<TComponent>> Expression
        {
            get { return this.expression; }
        }
    }
}