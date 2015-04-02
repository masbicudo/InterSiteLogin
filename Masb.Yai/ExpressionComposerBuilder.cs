using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Masb.Yai
{
    /// <summary>
    /// Collection of expressions filters used to build component expressions.
    /// </summary>
    public class ExpressionComposerBuilder :
        IEnumerable<IExpressionFilter>,
        IEnumerable<FilterEntry>
    {
        private readonly List<FilterEntry> expressionFilters
            = new List<FilterEntry>();

        /// <summary>
        /// Adds an expression filter to the collection in the default group and order.
        /// </summary>
        /// <param name="expressionFilter">An <see cref="IExpressionFilter"/> to add to the collection.</param>
        public void Add([NotNull] IExpressionFilter expressionFilter)
        {
            if (expressionFilter == null)
                throw new ArgumentNullException("expressionFilter");

            FilterEntry entry;
            entry.group = expressionFilter.DefaultGroup;
            entry.order = 0;
            entry.filter = expressionFilter;
            this.expressionFilters.Add(entry);
        }

        /// <summary>
        /// Adds an expression filter to the collection in the default group and the given order.
        /// </summary>
        /// <param name="expressionFilter">An <see cref="IExpressionFilter"/> to add to the collection.</param>
        /// <param name="order">
        /// The order that this expression filter will be evaluated.
        /// Many filters inserted with the same order are evaluated in insertion order.
        /// </param>
        public void Add([NotNull] IExpressionFilter expressionFilter, int order)
        {
            if (expressionFilter == null)
                throw new ArgumentNullException("expressionFilter");

            FilterEntry entry;
            entry.group = expressionFilter.DefaultGroup;
            entry.order = order;
            entry.filter = expressionFilter;
            this.expressionFilters.Add(entry);
        }

        /// <summary>
        /// Adds an expression filter to the collection in the given group and order.
        /// </summary>
        /// <param name="expressionFilter">An <see cref="IExpressionFilter"/> to add to the collection.</param>
        /// <param name="group">Overrides the default <paramref name="expressionFilter"/> group, affecting the evaluation order.</param>
        /// <param name="order">
        /// The order that this expression filter will be evaluated.
        /// Many filters inserted with the same order are evaluated in insertion order.
        /// </param>
        public void Add([NotNull] IExpressionFilter expressionFilter, FilterGroup group, int order)
        {
            if (expressionFilter == null)
                throw new ArgumentNullException("expressionFilter");

            FilterEntry entry;
            entry.group = group;
            entry.order = order;
            entry.filter = expressionFilter;
            this.expressionFilters.Add(entry);
        }

        IEnumerator<FilterEntry> IEnumerable<FilterEntry>.GetEnumerator()
        {
            return this.expressionFilters.GetEnumerator();
        }

        /// <summary>
        /// Enumerates all the filters ordered in sequence of evaluation,
        /// that is, in the order the filters should be evaluated to produce an expression.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> that returns filters in order of evaluation.</returns>
        public IEnumerator<IExpressionFilter> GetEnumerator()
        {
            return this.expressionFilters
                .OrderBy(x => x, new FilterEntryComparer())
                .Select(x => x.filter)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}