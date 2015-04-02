using System.Linq.Expressions;

namespace Masb.Yai
{
    /// <summary>
    /// Expression filter that replaces marker method calls in the expression tree,
    /// with the corresponding expression.
    /// <para>(Marker calls are like `M.Get`)</para>
    /// </summary>
    public class MarkerReplacerExpressionFilter :
        IExpressionFilter
    {
        public bool CanProcess(CompositionContext context)
        {
            return true;
        }

        public void Process(ExpressionFilterContext context)
        {
            var result = context.Result;
            if (result != null)
            {
                var visitor = new MarkerExpressionReplacer(context);
                context.Result = (LambdaExpression)visitor.Visit(result);
            }
        }

        public FilterGroup DefaultGroup
        {
            get { return FilterGroup.PostProcessor; }
        }
    }
}