using System;

namespace Masb.Yai
{
    public abstract class TypeChangerExpressionFilter :
        IExpressionFilter
    {
        protected abstract Type ChangeType(ExpressionFilterContext context, Type type);

        public bool CanProcess(CompositionContext context)
        {
            return true;
        }

        public void Process(ExpressionFilterContext context)
        {
            if (context.Result == null)
                throw new Exception("`TypeChangerExpressionFilter` requires a `context.Result`.");

            throw new NotImplementedException();
        }

        public FilterGroup DefaultGroup
        {
            get { return FilterGroup.PostProcessor; }
        }
    }
}