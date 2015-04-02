using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Masb.Yai
{
    public class ThreadSafeExpressionComposer : ExpressionComposer
    {
        private readonly object locker = new object();
        private List<FilterEntry> expressionFilters;
        private Action<ExpressionFilterContext> filterAction;

        public ThreadSafeExpressionComposer()
        {
            this.expressionFilters = new List<FilterEntry>();
        }

        internal ThreadSafeExpressionComposer(IEnumerable<FilterEntry> filterEntries)
        {
            this.expressionFilters = filterEntries.ToList();
        }

        private Action<ExpressionFilterContext> GetUpdatedFilterAction()
        {
            var action = this.filterAction;

            if (action == null)
                lock (this.locker)
                    if (action == null)
                    {
                        var newList = this.expressionFilters.OrderBy(x => x, new FilterEntryComparer()).ToList();

                        // ReSharper disable once LoopCanBeConvertedToQuery - LINQ is ugly in this case
                        foreach (var eachFilter in this.expressionFilters)
                            action += eachFilter.filter.ProcessIfNeeded;

                        this.filterAction = action;
                        this.expressionFilters = newList;
                    }

            return action;
        }

        public override Expression<Func<TResult>> Compose<TResult>()
        {
            var context = new ExpressionFilterContext<TResult>(this);
            var action = this.GetUpdatedFilterAction();
            action(context);
            return context.Result;
        }

        public override LambdaExpression Compose(Type componentType)
        {
            var context = ExpressionFilterContextBuilder.Create(componentType, this);
            var action = this.GetUpdatedFilterAction();
            action(context);
            return context.Result;
        }

        public override Expression<Func<TResult>> Compose<TResult>(CompositionContext parentContext, string componentName, object reflectedDestinationInfo)
        {
            var context = new ExpressionFilterContext<TResult>(parentContext, componentName, reflectedDestinationInfo);
            var action = this.GetUpdatedFilterAction();
            action(context);
            return context.Result;
        }

        public override LambdaExpression Compose(Type componentType, CompositionContext parentContext, string componentName, object reflectedDestinationInfo)
        {
            var context =
                ExpressionFilterContextBuilder.Create(
                    componentType,
                    parentContext,
                    componentName,
                    reflectedDestinationInfo);

            var action = this.GetUpdatedFilterAction();
            action(context);
            return context.Result;
        }

        public void Add(IExpressionFilter expressionFilter)
        {
            FilterEntry entry;
            entry.group = 0;
            entry.order = 0;
            entry.filter = expressionFilter;

            lock (this.locker)
            {
                this.filterAction = null;
                this.expressionFilters.Add(entry);
            }
        }

        public void Add(IExpressionFilter expressionFilter, int order)
        {
            FilterEntry entry;
            entry.group = 0;
            entry.order = order;
            entry.filter = expressionFilter;

            lock (this.locker)
            {
                this.filterAction = null;
                this.expressionFilters.Add(entry);
            }
        }
    }
}