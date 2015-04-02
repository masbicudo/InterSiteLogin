using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Masb.Yai
{
    public class ComponentEntry<T> : ComponentEntry
    {
        private readonly Option<T> value;
        private Func<T> getter;
        private Expression<Func<T>> expression;

        public ComponentEntry(Option<T> value)
        {
            lock (this)
                this.value = value;
        }

        public ComponentEntry([NotNull] Func<T> getter)
        {
            if (getter == null) throw new ArgumentNullException("getter");
            this.getter = getter;
        }

        public ComponentEntry([NotNull] Expression<Func<T>> expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            this.expression = expression;
        }

        /// <summary>
        /// Gets the service value.
        /// </summary>
        /// <return>An Option containing the value of the service if any.</return>
        public Option<T> GetValue()
        {
            if (this.value.HasValue)
                return this.value;

            if (this.getter != null)
                return new Option<T>(this.getter());

            if (this.expression != null)
            {
                this.getter = this.expression.Compile();
                return new Option<T>(this.getter());
            }

            return Option<T>.None;
        }

        /// <summary>
        /// Gets a delegate that returns the service value.
        /// </summary>
        public Func<T> GetGetter()
        {
            if (this.getter != null)
                return this.getter;

            if (this.expression != null)
            {
                this.getter = this.expression.Compile();
                return this.getter;
            }

            if (this.value.HasValue)
            {
                var value2 = this.value.Value;
                this.getter = () => value2;
                return this.getter;
            }

            this.getter = () => default(T);
            return this.getter;
        }

        /// <summary>
        /// Gets an expression that represents a service getter function.
        /// </summary>
        public Expression<Func<T>> GetLambdaExpression()
        {
            if (this.expression != null)
                return this.expression;

            if (this.getter != null)
            {
                var getter2 = this.getter;
                this.expression = () => getter2();
                return this.expression;
            }

            if (this.value.HasValue)
            {
                var value2 = this.value.Value;
                this.expression = () => value2;
                return this.expression;
            }

            this.expression = () => default(T);
            return this.expression;
        }

        /// <summary>
        /// Gets the service value as an object, instead of an Option.
        /// </summary>
        /// <returns>An object reference poiting to the value of the service.</returns>
        public override object GetValueAsObject()
        {
            var result = this.GetValue().ToObject();
            return result;
        }

        public override Func<object> GetObjectGetter()
        {
#if VARIANCE_ENALED
            return this.GetGetter();
#else
            var fn = this.GetGetter();
            Func<object> result = () => fn();
            return result;
#endif
        }

        public override Expression GetExpression()
        {
            return this.GetLambdaExpression();
        }
    }

    public abstract class ComponentEntry
    {
        public abstract object GetValueAsObject();

        public abstract Func<object> GetObjectGetter();

        public abstract Expression GetExpression();
    }
}