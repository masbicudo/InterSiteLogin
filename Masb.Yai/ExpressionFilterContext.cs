using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Masb.Yai
{
    public abstract class ExpressionFilterContext : CompositionContext
    {
        internal ExpressionFilterContext([NotNull] Type componentType, [NotNull] ExpressionComposer composer)
            : base(componentType, composer)
        {
        }

        internal ExpressionFilterContext(
            [NotNull] CompositionContext parentContext,
            [NotNull] Type componentType,
            [NotNull] string componentName,
            [CanBeNull] object reflectedDestinationInfo)
            : base(parentContext, componentType, componentName, reflectedDestinationInfo)
        {
        }

        public LambdaExpression Result
        {
            get { return this.GetResult(); }
            set { this.SetResult(value); }
        }

        protected abstract void SetResult(LambdaExpression resultExpression);

        protected abstract LambdaExpression GetResult();
    }

    public sealed class ExpressionFilterContext<T> : ExpressionFilterContext
    {
        internal ExpressionFilterContext(ExpressionComposer composer)
            : base(typeof(T), composer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionFilterContext{T}"/> class. 
        /// </summary>
        /// <param name="parentContext">The context for the dependent service this service.</param>
        /// <param name="componentName">The name of the component that is being retrieved.</param>
        /// <param name="reflectedDestinationInfo">
        ///     A reflected info-object about what is going to be filled with the service value:
        ///     <para> <see cref="C:ParameterInfo"/> when a parameter is going to be filled with the service value; </para>
        ///     <para> <see cref="C:PropertyInfo"/> when a property is going to be filled with the service value; </para>
        ///     <para> <see cref="C:EventInfo"/> when an event is going to be filled with the service value; </para>
        ///     <para> <see cref="C:FieldInfo"/> when a field is going to be filled with the service value. </para>
        /// </param>
        internal ExpressionFilterContext(
            CompositionContext parentContext,
            string componentName,
            object reflectedDestinationInfo)
            : base(parentContext, typeof(T), componentName, reflectedDestinationInfo)
        {
        }

        /// <summary>
        /// Gets or sets the resulting expression, created from information in the current service context.
        /// </summary>
        public new Expression<Func<T>> Result { get; set; }

        protected override void SetResult(LambdaExpression resultExpression)
        {
            this.Result = (Expression<Func<T>>)resultExpression;
        }

        protected override LambdaExpression GetResult()
        {
            return this.Result;
        }
    }
}