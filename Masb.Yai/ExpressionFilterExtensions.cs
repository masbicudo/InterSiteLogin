namespace Masb.Yai
{
    public static class ExpressionFilterExtensions
    {
        public static void ProcessIfNeeded(this IExpressionFilter filter, ExpressionFilterContext context)
        {
            if (filter.CanProcess(context))
                filter.Process(context);
        }

        public static SequenceExpressionFilter AndThen(this IExpressionFilter first, IExpressionFilter second)
        {
            return new SequenceExpressionFilter(first, second);
        }
    }
}