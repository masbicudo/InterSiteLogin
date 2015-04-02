using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Masb.Yai.AttributeSources;
using Masb.Yai.Markers;

namespace Masb.Yai
{
    public class MarkerExpressionReplacer : ExpressionReflectionVisitor
    {
        private readonly ExpressionFilterContext context;
        private readonly Stack<CompositionNode> stack;
        private readonly IAttributeSource attributeSource;

        public MarkerExpressionReplacer([NotNull] ExpressionFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.context = context;

            var customData = context as ICompositionCustomDataProvider;
            this.attributeSource = customData.CustomData.GetOrCreateObject(() => new ReflectedAttributeSource());

            var stackProvider = context as ICompositionStackProvider;
            this.stack = stackProvider.CompositionStack;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var newExpr = (NewExpression)this.VisitNew(node.NewExpression);
            var cns = node.Bindings.Select(p => new CompositionNode(p.Member, p.Member.Name, node)).ToArray();
            var args = this.VisitWithReflectedInfos(node.Bindings, cns, this.VisitMemberBinding);
            return node.Update(newExpr, args);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return base.VisitMemberAssignment(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var cns = node.Constructor.GetParameters().Select(p => new CompositionNode(p, p.Name, node)).ToArray();
            var args = this.VisitWithReflectedInfos(node.Arguments, cns, this.Visit);
            return node.Update(args);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var attr = this.attributeSource
                .GetCustomAttributes<ExpressionMarkerBaseAttribute>(node.Method, true)
                .SingleOrDefault();

            if (attr != null)
            {
                var cns = this.stack.Peek();
                var result = attr.GetExpressionFor(node, this.context, cns.ComponentName, cns.ReflectedDestinationInfo);
                return result;
            }
            else
            {
                var newMethod = this.VisitMethodInfo(node.Method);
                Expression newObject = this.Visit(node.Object);
                var cns = node.Method.GetParameters().Select(p => new CompositionNode(p, p.Name, node)).ToArray();
                var newArguments = this.VisitWithReflectedInfos(node.Arguments, cns, this.Visit);

                if (newMethod != node.Method)
                {
                    var newNode = Expression.Call(newObject, newMethod, newArguments);
                    return newNode;
                }

                return node.Update(newObject, newArguments);
            }
        }

        /// <summary>
        /// Visits a <see cref="Type"/> object from the expression tree,
        /// replacing marker types with the corresponding types
        /// by using the <see cref="TypeMarkerBaseAttribute"/>.
        /// </summary>
        /// <param name="type">The type to analyze.</param>
        /// <returns>The type passed in <paramref name="type"/> if no changes are needed, or the type corresponding to the marker type.</returns>
        protected override Type VisitType(Type type)
        {
            var attr = this.attributeSource
                .GetCustomAttributes<TypeMarkerBaseAttribute>(type, true)
                .SingleOrDefault();

            if (attr != null)
            {
                var cns = this.stack.Peek();
                var result = attr.GetTypeFor(type, this.context, cns.ComponentName, cns.ReflectedDestinationInfo, this.stack);
                return result;
            }

            return base.VisitType(type);
        }

        protected override Type[] VisitMethodGenericParameters(MethodInfo method)
        {
            var typeParams = new ReadOnlyCollection<Type>(method.GetGenericArguments());
            var genericParams = method.GetGenericMethodDefinition().GetGenericArguments();

            var cns = genericParams.Select(p => new CompositionNode(p, p.Name, null)).ToArray();
            var newTypeParams = this.VisitWithReflectedInfos(typeParams, cns, this.VisitType);

            return newTypeParams.ToArrayOrSelf();
        }

        protected override Type[] VisitTypeGenericParameters(Type type)
        {
            var typeParams = new ReadOnlyCollection<Type>(type.GetGenericArguments());
            var genericParams = type.GetGenericTypeDefinition().GetGenericArguments();

            var cns = genericParams.Select(p => new CompositionNode(p, p.Name, null)).ToArray();
            var newTypeParams = this.VisitWithReflectedInfos(typeParams, cns, this.VisitType);

            return newTypeParams.ToArrayOrSelf();
        }

        /// <summary>
        /// Visits multiple <see cref="T"/> elements, and for each of these,
        /// placing the corresponding <see cref="CompositionNode"/> in the composition stack,
        /// and then visiting the element using the `<paramref name="visitor"/>`.
        /// </summary>
        /// <typeparam name="T">Type of the element to visit.</typeparam>
        /// <param name="args">List of elements to visit, with stacking the corresponding <see cref="CompositionNode"/>.</param>
        /// <param name="cns">List of <see cref="CompositionNode"/>s that correspond to each element.</param>
        /// <param name="visitor">The visitor delegate used to visit each element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the visited elements.</returns>
        private IEnumerable<T> VisitWithReflectedInfos<T>(IList<T> args, CompositionNode[] cns, Func<T, T> visitor)
            where T : class
        {
            T[] result = null;
            var argsCount = args.Count;
            for (int itArg = 0; itArg < argsCount; itArg++)
            {
                this.stack.Push(cns[itArg]);

                try
                {
                    var currentArg = args[itArg];
                    var newArg = visitor(currentArg);
                    if (result != null)
                    {
                        result[itArg] = newArg;
                    }
                    else if (currentArg != newArg)
                    {
                        result = new T[argsCount];
                        for (int itCopy = 0; itCopy < itArg; itCopy++)
                            result[itCopy] = args[itCopy];

                        result[itArg] = newArg;
                    }
                }
                finally
                {
                    this.stack.Pop();
                }
            }

            args = result ?? args;
            return args;
        }
    }
}