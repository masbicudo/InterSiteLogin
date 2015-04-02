using System.Collections.Generic;

namespace Masb.Yai
{
    internal class FilterEntryComparer :
        IComparer<FilterEntry>
    {
        public int Compare(FilterEntry x, FilterEntry y)
        {
            var cmpGroup = Comparer<FilterGroup>.Default.Compare(x.group, y.group);
            if (cmpGroup != 0)
                return cmpGroup;

            var cmpOrder = Comparer<int>.Default.Compare(x.order, y.order);
            if (cmpOrder != 0)
                return cmpOrder;

            return 0;
        }
    }
}