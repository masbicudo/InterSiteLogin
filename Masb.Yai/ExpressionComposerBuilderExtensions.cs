using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Masb.Yai
{
    public static class ExpressionComposerBuilderExtensions
    {
        /// <summary>
        /// Converts the given <see cref="ExpressionComposerBuilder"/> to an <see cref="Action{ExpressionFilterContext}"/>,
        /// capable of building component expressions.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ExpressionComposerBuilder"/> used to build the composer action.
        /// </param>
        /// <returns> An <see cref="Action"/> that does the job of processing expressions. </returns>
        public static Action<ExpressionFilterContext> ToAction(this ExpressionComposerBuilder builder)
        {
            Action<ExpressionFilterContext> action = null;

            // ReSharper disable once LoopCanBeConvertedToQuery - LINQ is ugly in this case
            foreach (var eachFilter in builder)
                action += eachFilter.ProcessIfNeeded;

            return action;
        }

        public static ImmutableExpressionComposer ToImmutable(this ExpressionComposerBuilder builder)
        {
            return new ImmutableExpressionComposer(builder.ToAction());
        }

        public static ThreadSafeExpressionComposer ToThreadSafe(this ExpressionComposerBuilder builder)
        {
            return new ThreadSafeExpressionComposer(builder);
        }

        public static void Add(this ExpressionComposerBuilder builder, [NotNull] Action<ExpressionFilterContext> expressionFilter)
        {
            if (expressionFilter == null)
                throw new ArgumentNullException("expressionFilter");

            builder.Add(new CustomDelegateExpressionFilter(expressionFilter));
        }

        /// <summary>
        /// <see cref="IExpressionFilter"/> that uses a custom delegate to process the expression.
        /// </summary>
        private class CustomDelegateExpressionFilter :
            IExpressionFilter
        {
            private readonly Action<ExpressionFilterContext> action;

            public CustomDelegateExpressionFilter([NotNull] Action<ExpressionFilterContext> action)
            {
                if (action == null)
                    throw new ArgumentNullException("action");

                this.action = action;
            }

            public bool CanProcess(CompositionContext context)
            {
                return true;
            }

            public void Process(ExpressionFilterContext context)
            {
                this.action(context);
            }

            public FilterGroup DefaultGroup
            {
                get { return FilterGroup.Initializer; }
            }
        }
    }
}