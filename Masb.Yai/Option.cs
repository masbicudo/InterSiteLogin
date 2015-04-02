using System;
using JetBrains.Annotations;

namespace Masb.Yai
{
    public struct Option<T>
    {
        private readonly bool hasValue;
        private readonly T value;

        public static Option<T> None
        {
            get { return default(T); }
        }

        static Option()
        {
            if (typeof(T).IsGenericType)
                if (typeof(T).GetGenericTypeDefinition() == typeof(Option<>))
                    throw new Exception("Cannot declare an Option of an Option: Option<Option<T>>");
        }

        public Option(T value)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (value == null)
            {
                // ReSharper disable once ExpressionIsAlwaysNull - cannot replace 'value' by null here
                this.value = value;
                this.hasValue = false;
            }
            else
            {
                this.value = value;
                this.hasValue = true;
            }
        }

        public T Value
        {
            get
            {
                if (!this.hasValue)
                    throw new InvalidOperationException("Cannot read the Value property of an Option without a value.");

                return this.value;
            }
        }

        public bool HasValue
        {
            get { return this.hasValue; }
        }

        public TResult Match<TResult>(Func<T, TResult> whenSome, Func<TResult> whenNone)
        {
            if (!this.hasValue)
                return whenNone();

            return whenSome(this.value);
        }

        public TResult MatchSomeOrDefault<TResult>(Func<T, TResult> whenSome)
        {
            if (!this.hasValue)
                return default(TResult);

            return whenSome(this.value);
        }

        public TResult MatchNoneOrDefault<TResult>(Func<TResult> whenNone)
        {
            if (!this.hasValue)
                return whenNone();

            return default(TResult);
        }

        public static implicit operator Option<T>(T value)
        {
            return new Option<T>(value);
        }

        public static explicit operator T(Option<T> value)
        {
            return value.Value;
        }

        public object ToObject()
        {
            if (!this.hasValue)
                return null;

            return this.value;
        }

        public T GetValueOrDefault()
        {
            if (!this.hasValue)
                return default(T);

            return this.value;
        }

        public T GetValueOrDefault(T defaultValue)
        {
            if (!this.hasValue)
                return defaultValue;

            return this.value;
        }

        public T GetValueOrDefault([NotNull] Func<T> defaultValueGetter)
        {
            if (defaultValueGetter == null) throw new ArgumentNullException("defaultValueGetter");

            if (!this.hasValue)
                return defaultValueGetter();

            return this.value;
        }
    }
}