using System;
using System.Linq.Expressions;

namespace Masb.Yai
{
    /// <summary>
    /// Represents objects that can compose expression-trees to create components.
    /// </summary>
    public abstract class ExpressionComposer
    {
        /// <summary>
        /// Composes an expression-tree to create a component of type <typeparamref name="TComponent"/>.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component to build.</typeparam>
        /// <returns>An expression-tree that can create a component of type <typeparamref name="TComponent"/>.</returns>
        public abstract Expression<Func<TComponent>> Compose<TComponent>();

        /// <summary>
        /// Composes an expression-tree to create a component of the given type.
        /// </summary>
        /// <param name="componentType">The <see cref="Type"/> of the component to build an expression-tree for.</param>
        /// <returns>A lambda expression-tree that can create a component of type <paramref name="componentType"/>.</returns>
        public abstract LambdaExpression Compose(Type componentType);

        /// <summary>
        /// Composes an expression-tree to create a subcomponent of type <typeparamref name="TComponent"/>,
        /// inside the context of creation of another component that depends on this subcomponent.
        /// </summary>
        /// <param name="parentContext">The context of creation of the parent component.</param>
        /// <param name="componentName">The name of the subcomponent, that can be used to distinguish components of the same type.</param>
        /// <param name="reflectedDestinationInfo">
        ///     The reflection object representing the assigned property, parameter or field of the parent component,
        ///     indicating how the subcomponent will be passed to the component that depends on it.
        /// </param>
        /// <typeparam name="TComponent">Type of the component to build.</typeparam>
        /// <returns>An expression-tree that can create a component of type <typeparamref name="TComponent"/>.</returns>
        public abstract Expression<Func<TComponent>> Compose<TComponent>(
            CompositionContext parentContext,
            string componentName,
            object reflectedDestinationInfo);

        /// <summary>
        /// Composes an expression-tree to create a subcomponent of the given type,
        /// inside the context of creation of another component that depends on this subcomponent.
        /// </summary>
        /// <param name="componentType">The <see cref="Type"/> of the subcomponent to build an expression-tree for.</param>
        /// <param name="parentContext">The context of creation of the parent component.</param>
        /// <param name="componentName">The name of the subcomponent, that can be used to distinguish components of the same type.</param>
        /// <param name="reflectedDestinationInfo">
        ///     The reflection object representing the assigned property, parameter or field of the parent component,
        ///     indicating how the subcomponent will be passed to the component that depends on it.
        /// </param>
        /// <returns>A lambda expression-tree that can create a component of type <paramref name="componentType"/>.</returns>
        public abstract LambdaExpression Compose(
            Type componentType,
            CompositionContext parentContext,
            string componentName,
            object reflectedDestinationInfo);
    }
}