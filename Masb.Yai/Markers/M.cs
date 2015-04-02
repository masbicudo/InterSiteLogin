#define PLACE_INSIDE_M
using System;
using System.Linq.Expressions;

namespace Masb.Yai.Markers
{
    public partial class M
    {
        /// <summary>
        /// Marker method used inside an <see cref="Expression"/>,
        /// to indicate that an expression representing the service should be used.
        /// <para>A component name is going to be inferred by looking around the marker method.</para>
        /// <para>This method is not meant to be called.</para>
        /// </summary>
        /// <typeparam name="T">Type that must be returned from the replacer <see cref="Expression"/>.</typeparam>
        /// <exception cref="Exception">Thrown if called. This method is not meant to be called.</exception>
        /// <returns>This method is not meant to be called. It never returns.</returns>
        [ExpressionMarkerMethod]
        public static T Get<T>()
        {
            throw new Exception("Get method is a marker method, and is not meant to be called.");
        }

        /// <summary>
        /// Marker method used inside an <see cref="Expression"/>,
        /// to indicate that an expression representing the named component should be used.
        /// <para> This method is not meant to be called. </para>
        /// </summary>
        /// <param name="componentName"> The component name. </param>
        /// <typeparam name="T"> Type that must be returned from the replacer <see cref="Expression"/>. </typeparam>
        /// <exception cref="Exception"> Thrown if called. This method is not meant to be called. </exception>
        /// <returns> This method is not meant to be called. It never returns. </returns>
        [ExpressionMarkerMethod]
        public static T Get<T>(string componentName)
        {
            throw new Exception("Get method is a marker method, and is not meant to be called.");
        }

        /// <summary>
        /// Marker method used inside an <see cref="Expression"/>,
        /// to indicate that an expression representing the named service should be used.
        /// <para>A component name is going to be inferred by looking around the marker method.</para>
        /// <para> This method is not meant to be called. </para>
        /// </summary>
        /// <param name="expressionBuiler"> A delegate that can build the expression with which to replace the current method call. </param>
        /// <typeparam name="T"> Type that must be returned from the replacer <see cref="Expression"/>. </typeparam>
        /// <exception cref="Exception"> Thrown if called. This method is not meant to be called. </exception>
        /// <returns> This method is not meant to be called. It never returns. </returns>
        [ExpressionMarkerMethod]
        public static T Get<T>(Func<ExpressionFilterContext, Expression<Func<T>>> expressionBuiler)
        {
            throw new Exception("Get method is a marker method, and is not meant to be called.");
        }

        [ExpressionMarkerMethod]
        public static T Get<T>(Func<Expression<Func<T>>> expressionBuiler)
        {
            throw new Exception("Get method is a marker method, and is not meant to be called.");
        }
    }

#if PLACE_INSIDE_M
    public partial class M
    {
#endif
        public interface IMarkerTypeMarker
        {
        }

        /// <summary>
        /// Marker type that represents the component type of the parent context, when that type must be a class.
        /// When used in a component expression, this type will be replaced by the type of the component of the parent context.
        /// </summary>
        /// <typeparam name="T">Type representing the type-context to get the parent component from.</typeparam>
        // ReSharper disable once UnusedTypeParameter
        [TypeMarker]
        public sealed class Parent<T> : IMarkerTypeMarker
            where T : IMarkerTypeMarker
        {
            private Parent()
            {
            }

            /// <summary>
            /// Marker type that represents the component type of the parent context, when that type must be a struct.
            /// When used in a component expression, this type will be replaced by the type of the component of the parent context.
            /// </summary>
            [SubTypeMarker]
            public struct Struct : IMarkerTypeMarker
            {
            }

            /// <summary>
            /// Marker type that represents the component type of the parent context, when that type must be a class with a public constructor.
            /// When used in a component expression, this type will be replaced by the type of the component of the parent context.
            /// </summary>
            [SubTypeMarker]
            public sealed class New : IMarkerTypeMarker
            {
                public New()
                {
                    throw new Exception("This is a marker type. Cannot instantiate.");
                }
            }
        }

        /// <summary>
        /// Marker type that represents the current component type, when the type must be a class.
        /// When used in a component expression, this type will be replaced by the type of the component.
        /// </summary>
        // ReSharper disable once ConvertToStaticClass - static classes cannot be used as type-parameters
        [TypeMarker]
        public sealed class Current : IMarkerTypeMarker
        {
            private Current()
            {
            }

            /// <summary>
            /// Marker type that represents the current component type, when the type must be a struct.
            /// When used in a component expression, this type will be replaced by the type of the component.
            /// </summary>
            [SubTypeMarker]
            public struct Struct : IMarkerTypeMarker
            {
            }

            /// <summary>
            /// Marker type that represents the current component type, when the type must be a class with a public constructor.
            /// When used in a component expression, this type will be replaced by the type of the component.
            /// </summary>
            [SubTypeMarker]
            public sealed class New : IMarkerTypeMarker
            {
                public New()
                {
                    throw new Exception("This is a marker type. Cannot instantiate.");
                }
            }
        }

        public struct MarkerStruct<T>
            where T : INumMarkerZeroTrimmed
        {
        }

        // ReSharper disable once ConvertToStaticClass - static classes cannot be used as type-parameters
        public sealed class MarkerClass<T>
            where T : INumMarkerZeroTrimmed
        {
            private MarkerClass()
            {
            }

            public sealed class New
            {
                public New()
                {
                    throw new Exception("This is a marker type. Cannot instantiate.");
                }
            }
        }

        public sealed class MarkerClass
        {
            private MarkerClass()
            {
            }

            public sealed class New
            {
                public New()
                {
                    throw new Exception("This is a marker type. Cannot instantiate.");
                }
            }
        }

        public interface INumMarker
        {
        }

        public interface INumMarkerZeroTrimmed : INumMarker
        {
        }

        public sealed class _0 : INumMarkerZeroTrimmed { private _0() { } }
        public sealed class _0<T> : INumMarker where T : INumMarker { private _0() { } }

        public sealed class _1 : INumMarkerZeroTrimmed { private _1() { } }
        public sealed class _1<T> : INumMarkerZeroTrimmed where T : INumMarker { private _1() { } }

        public sealed class _2 : INumMarkerZeroTrimmed { private _2() { } }
        public sealed class _2<T> : INumMarkerZeroTrimmed where T : INumMarker { private _2() { } }

        public sealed class _3 : INumMarkerZeroTrimmed { private _3() { } }
        public sealed class _3<T> : INumMarkerZeroTrimmed where T : INumMarker { private _3() { } }

        public sealed class _4 : INumMarkerZeroTrimmed { private _4() { } }
        public sealed class _4<T> : INumMarkerZeroTrimmed where T : INumMarker { private _4() { } }

        public sealed class _5 : INumMarkerZeroTrimmed { private _5() { } }
        public sealed class _5<T> : INumMarkerZeroTrimmed where T : INumMarker { private _5() { } }

        public sealed class _6 : INumMarkerZeroTrimmed { private _6() { } }
        public sealed class _6<T> : INumMarkerZeroTrimmed where T : INumMarker { private _6() { } }

        public sealed class _7 : INumMarkerZeroTrimmed { private _7() { } }
        public sealed class _7<T> : INumMarkerZeroTrimmed where T : INumMarker { private _7() { } }

        public sealed class _8 : INumMarkerZeroTrimmed { private _8() { } }
        public sealed class _8<T> : INumMarkerZeroTrimmed where T : INumMarker { private _8() { } }

        public sealed class _9 : INumMarkerZeroTrimmed { private _9() { } }
        public sealed class _9<T> : INumMarkerZeroTrimmed where T : INumMarker { private _9() { } }

#if PLACE_INSIDE_M
    }
#endif
}