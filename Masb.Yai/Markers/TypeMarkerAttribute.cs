using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Masb.Yai.Markers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class TypeMarkerAttribute : TypeMarkerBaseAttribute
    {
        public override Type GetTypeFor(
            Type type,
            ExpressionFilterContext context,
            string name,
            object info,
            IEnumerable<CompositionNode> compositionNodes)
        {
            var context2 = this.GetContextFor(type, context);

            if (context2 == null)
                throw new Exception(
                    string.Format(
                        "Component type for '{0}' does not exist.\n"
                        + "You may have used `Parent<>` nested too many times.",
                        type));

            return context2.ComponentType;
        }

        [CanBeNull]
        private CompositionContext GetContextFor([NotNull] Type type, [NotNull] ExpressionFilterContext context)
        {
            if (type.Name == "Current")
                return context;

            if (type.Name == "Parent")
            {
                if (!type.IsGenericType)
                    return context.ParentContext;

                var typeArgs = type.GetGenericArguments();
                if (typeArgs.Length != 1)
                    throw new Exception("Generic `Parent<>` marker type must have one single type parameter.");

                var subType = typeArgs[0];

                var subContext = this.GetContextFor(subType, context);

                return subContext == null ? null : subContext.ParentContext;
            }

            throw new NotSupportedException("Unsupported marker type.");
        }
    }
}