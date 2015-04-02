using System;
using Masb.Yai.Markers;

namespace Masb.Yai
{
    [Obsolete]
    public class RelativeTypeChangerExpressionFilter : TypeChangerExpressionFilter
    {
        protected override Type ChangeType(ExpressionFilterContext context, Type type)
        {
            var context2 = this.GetContext(context, type);
            return context2.ComponentType;
        }

        private CompositionContext GetContext(ExpressionFilterContext context, Type type)
        {
            if (type == typeof(M.Current))
                return context;

            if (type == typeof(M.Current.New))
                return context;

            if (type == typeof(M.Current.Struct))
                return context;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(M.Parent<>))
                return this.GetContext(context, type.GetGenericArguments()[0]).ParentContext;

            if (type.TypeHandle == typeof(M.Parent<>.New).TypeHandle) // todo: check this TypeHandle
                return this.GetContext(context, type.GetGenericArguments()[0]).ParentContext;

            if (type.TypeHandle == typeof(M.Parent<>.Struct).TypeHandle) // todo: check this TypeHandle
                return this.GetContext(context, type.GetGenericArguments()[0]).ParentContext;

            throw new Exception("Invalid GetContext call");
        }
    }
}