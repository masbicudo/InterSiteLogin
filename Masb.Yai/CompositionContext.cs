using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Masb.Yai
{
    /// <summary>
    /// Context for the composition of a component.
    /// </summary>
    public abstract class CompositionContext :
        ICompositionStackProvider,
        ICompositionCustomDataProvider
    {
        private Stack<CompositionNode> compositionStack;
        private ExpressionComposer composer;
        private TypedAndNamedCollection customData;

        internal CompositionContext([NotNull] Type componentType, [NotNull] ExpressionComposer composer)
        {
            if (componentType == null)
                throw new ArgumentNullException("componentType");
            if (composer == null)
                throw new ArgumentNullException("composer");

            this.ComponentType = componentType;
            this.composer = composer;
            this.compositionStack = new Stack<CompositionNode>(10);
        }

        internal CompositionContext(
            [NotNull] CompositionContext parentContext,
            [NotNull] Type componentType,
            [NotNull] string componentName,
            [CanBeNull] object reflectedDestinationInfo)
        {
            if (parentContext == null)
                throw new ArgumentNullException("parentContext");
            if (componentType == null)
                throw new ArgumentNullException("componentType");
            if (componentName == null)
                throw new ArgumentNullException("componentName");

            this.ParentContext = parentContext;
            this.ComponentType = componentType;
            this.ComponentName = componentName;

            this.ReflectedDestinationInfo = reflectedDestinationInfo;
        }

        [NotNull]
        public Type ComponentType { get; private set; }

        /// <summary>
        /// Gets the context of the parent component.
        /// <para>(the component depending on the current one)</para>
        /// </summary>
        [CanBeNull]
        public CompositionContext ParentContext { get; private set; }

        /// <summary>
        /// Gets the current component name.
        /// </summary>
        [CanBeNull]
        public string ComponentName { get; private set; }

        /// <summary>
        /// Gets the reflection info object that represents the argument, property or variable that will hold the built component.
        /// </summary>
        [CanBeNull]
        public object ReflectedDestinationInfo { get; private set; }

        /// <summary>
        /// Gets the composer that is being used to make the component.
        /// </summary>
        [NotNull]
        public ExpressionComposer Composer
        {
            get
            {
                if (this.composer != null)
                    return this.composer;

                Debug.Assert(this.ParentContext != null, "this.ParentContext != null");
                this.composer = this.ParentContext.Composer;
                return this.composer;
            }
        }

        Stack<CompositionNode> ICompositionStackProvider.CompositionStack
        {
            get
            {
                if (this.compositionStack != null)
                    return this.compositionStack;

                var parent = this.ParentContext as ICompositionStackProvider;

                this.compositionStack = parent == null
                    ? new Stack<CompositionNode>()
                    : parent.CompositionStack;

                return this.compositionStack;
            }
        }

        TypedAndNamedCollection ICompositionCustomDataProvider.CustomData
        {
            get
            {
                if (this.customData != null)
                    return this.customData;

                var parent = this.ParentContext as ICompositionCustomDataProvider;

                this.customData = parent == null
                    ? new TypedAndNamedCollection()
                    : parent.CustomData;

                return this.customData;
            }
        }
    }
}