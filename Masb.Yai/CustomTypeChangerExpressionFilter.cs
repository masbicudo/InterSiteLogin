using System;

namespace Masb.Yai
{
    [Obsolete]
    public class CustomTypeChangerExpressionFilter : TypeChangerExpressionFilter
    {
        private readonly Predicate<Type> typePredicate;
        private readonly Func<ExpressionFilterContext, Type, Type> newTypeGetter;

        public CustomTypeChangerExpressionFilter(Predicate<Type> typePredicate, Func<ExpressionFilterContext, Type, Type> newTypeGetter)
        {
            this.typePredicate = typePredicate;
            this.newTypeGetter = newTypeGetter;
        }

        public Predicate<Type> TypePredicate
        {
            get { return this.typePredicate; }
        }

        public Func<ExpressionFilterContext, Type, Type> NewTypeGetter
        {
            get { return this.newTypeGetter; }
        }

        protected override Type ChangeType(ExpressionFilterContext context, Type type)
        {
            throw new NotImplementedException();
        }
    }
}