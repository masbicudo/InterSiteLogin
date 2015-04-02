namespace Masb.Yai
{
    public interface IExpressionFilter
    {
        bool CanProcess(CompositionContext context);

        void Process(ExpressionFilterContext context);

        FilterGroup DefaultGroup { get; }
    }
}