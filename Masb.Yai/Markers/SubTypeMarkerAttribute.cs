using System;
using System.Collections.Generic;
using System.Linq;
using Masb.Yai.AttributeSources;

namespace Masb.Yai.Markers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class SubTypeMarkerAttribute : TypeMarkerBaseAttribute
    {
        public override Type GetTypeFor(
            Type type,
            ExpressionFilterContext context,
            string name,
            object info,
            IEnumerable<CompositionNode> compositionNodes)
        {
            var type2 = type.DeclaringType;

            if (type2 == null)
                throw new NotSupportedException(
                    "`SubTypeMarkerAttribute` is meant to be used with nested marker types.");

            var attrSource = GetAttributeSource(context);

            var attr = attrSource.GetCustomAttributes<TypeMarkerBaseAttribute>(type2, true).Single();

            var result = attr.GetTypeFor(type, context, name, info, compositionNodes);
            return result;
        }

        private static IAttributeSource GetAttributeSource(ExpressionFilterContext context)
        {
            var customDataProvider = context as ICompositionCustomDataProvider;
            IAttributeSource attrSource;
            if (customDataProvider == null)
            {
                attrSource = new ReflectedAttributeSource();
            }
            else
            {
                attrSource = customDataProvider.CustomData.GetOrCreateObject(
                    () =>
                    {
                        var expr = context.Composer.Compose<IAttributeSource>();
                        var result0 = expr != null ? expr.Compile().Invoke() : new ReflectedAttributeSource();
                        return result0;
                    });
            }

            return attrSource;
        }
    }
}