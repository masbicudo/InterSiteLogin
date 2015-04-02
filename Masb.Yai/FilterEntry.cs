namespace Masb.Yai
{
    internal struct FilterEntry
    {
        public FilterGroup group;
        public int order;
        public IExpressionFilter filter;
    }
}