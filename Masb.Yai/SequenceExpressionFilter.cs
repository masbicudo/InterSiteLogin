namespace Masb.Yai
{
    public class SequenceExpressionFilter :
        IExpressionFilter
    {
        private readonly IExpressionFilter first;
        private readonly IExpressionFilter second;

        public SequenceExpressionFilter(IExpressionFilter first, IExpressionFilter second)
        {
            this.first = first;
            this.second = second;
        }

        public bool CanProcess(CompositionContext context)
        {
            return this.first.CanProcess(context);
        }

        public void Process(ExpressionFilterContext context)
        {
            var oldResult = context.Result;
            this.first.Process(context);
            if (this.second.CanProcess(context))
            {
                this.second.Process(context);
            }
            else
            {
                context.Result = oldResult;
            }
        }

        public FilterGroup DefaultGroup
        {
            get { return this.first.DefaultGroup; }
        }
    }
}