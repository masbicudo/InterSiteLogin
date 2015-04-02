using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Masb.Yai
{
    public class ExpressionReflectionVisitor : ExpressionVisitor
    {
        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            var newTest = this.VisitType(node.Test);
            if (newTest != node.Test)
            {
                var newVariable = this.VisitAndConvert(node.Variable, "VisitCatchBlock");
                var newFilter = this.Visit(node.Filter);
                var newBody = this.Visit(node.Body);

                var newNode = Expression.MakeCatchBlock(newTest, newVariable, newFilter, newBody);
                return newNode;
            }

            return base.VisitCatchBlock(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var asType = node.Value as Type;
            if (asType != null)
            {
                var newType = this.VisitType(asType);
                if (asType != newType)
                    return Expression.Constant(newType);
            }

            return base.VisitConstant(node);
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            var newType = this.VisitType(node.Type);
            if (newType != node.Type)
                return Expression.Default(newType);

            return base.VisitDefault(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            // todo: is dynamic inside lambda possible?
            return base.VisitDynamic(node);
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            // todo: a method is used here
            return base.VisitElementInit(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var type = typeof(T);
            var newType = this.VisitType(type);
            if (newType != type)
            {
                var newBody = this.Visit(node.Body);
                var newArgs = this.VisitAndConvert(node.Parameters, "VisitLambda");
                var newNode = Expression.Lambda(newType, newBody, node.Name, node.TailCall, newArgs);
                return newNode;
            }

            return base.VisitLambda(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var newMethod = this.VisitMethodInfo(node.Method);
            if (newMethod != node.Method)
            {
                Expression instance = this.Visit(node.Object);
                var expressionArray = this.Visit(node.Arguments);
                var newNode = Expression.Call(instance, newMethod, expressionArray);
                return newNode;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
            {
                var newType = this.VisitType(node.Type);
                if (newType != node.Type)
                {
                    var newOperand = this.Visit(node.Operand);

                    var newNode = node.NodeType == ExpressionType.Convert
                        ? Expression.Convert(newOperand, newType)
                        : Expression.ConvertChecked(newOperand, newType);

                    return newNode;
                }
            }

            return base.VisitUnary(node);
        }

        protected virtual Type VisitType(Type type)
        {
            Type newType = null;
            if (type.ReflectedType != null)
            {
                var newReflectedType = this.VisitType(type.ReflectedType);
                if (newReflectedType != type.ReflectedType)
                    newType = FindEquivalentType(type, newReflectedType);
            }

            Type[] newTypeParams = null;
            if (type.IsGenericType)
                newTypeParams = this.VisitTypeGenericParameters(type);

            if (newTypeParams != null)
            {
                newType = newType ?? type;

                newType = newType.IsGenericTypeDefinition
                    ? newType
                    : newType.GetGenericTypeDefinition();

                newType = newType.MakeGenericType(newTypeParams);
            }

            newType = newType ?? type;
            return newType;
        }

        protected virtual Type[] VisitTypeGenericParameters(Type type)
        {
            Type[] newTypeParams = null;
            var typeParams = type.GetGenericArguments();
            for (int itParam = 0; itParam < typeParams.Length; itParam++)
            {
                var newTypeParam = this.VisitType(typeParams[itParam]);
                if (newTypeParams != null)
                    newTypeParams[itParam] = newTypeParam;
                else if (newTypeParam != typeParams[itParam])
                    newTypeParams = typeParams;
            }

            return typeParams;
        }

        protected virtual MethodInfo VisitMethodInfo(MethodInfo method)
        {
            MethodInfo newMethod = null;
            if (method.ReflectedType != null)
            {
                var newReflectedType = this.VisitType(method.ReflectedType);
                if (newReflectedType != method.ReflectedType)
                    newMethod = FindEquivalentMethod(method, newReflectedType);
            }

            Type[] newTypeParams = null;
            if (method.IsGenericMethod)
                newTypeParams = this.VisitMethodGenericParameters(method);

            if (newTypeParams != null)
            {
                newMethod = newMethod ?? method;

                newMethod = newMethod.IsGenericMethodDefinition
                    ? newMethod
                    : newMethod.GetGenericMethodDefinition();

                newMethod = newMethod.MakeGenericMethod(newTypeParams);
            }

            newMethod = newMethod ?? method;
            return newMethod;
        }

        protected virtual Type[] VisitMethodGenericParameters(MethodInfo method)
        {
            Type[] newTypeParams = null;
            var typeParams = method.GetGenericArguments();
            for (int itParam = 0; itParam < typeParams.Length; itParam++)
            {
                var newTypeParam = this.VisitType(typeParams[itParam]);
                if (newTypeParams != null)
                    newTypeParams[itParam] = newTypeParam;
                else if (newTypeParam != typeParams[itParam])
                    newTypeParams = typeParams;
            }

            return typeParams;
        }

        protected static Type FindEquivalentType(Type subtype, Type newReflectedType)
        {
            if (subtype.ReflectedType == null)
                throw new Exception("Passed `subtype` must have a `ReflectedType`.");

            const BindingFlags allFlags =
                BindingFlags.Instance | BindingFlags.Static
                | BindingFlags.Public | BindingFlags.NonPublic;

            var oldTypes = subtype.ReflectedType.GetNestedTypes(allFlags)
                .Where(m => AreTypesGenericallyEqual(m, subtype))
                .ToArray();

            var newTypes = newReflectedType.GetNestedTypes(allFlags)
                .Where(m => AreTypesGenericallyEqual(m, subtype))
                .ToArray();

            for (int it = 0; it < oldTypes.Length; it++)
                if (oldTypes[it] == subtype)
                    return newTypes[it];

            return null;
        }

        protected static MethodInfo FindEquivalentMethod(MethodInfo method, Type newReflectedType)
        {
            if (method.ReflectedType == null)
                throw new Exception("Passed `method` must have a `ReflectedType`.");

            const BindingFlags allFlags =
                BindingFlags.Instance | BindingFlags.Static
                | BindingFlags.Public | BindingFlags.NonPublic;

            var oldMethods = method.ReflectedType.GetMethods(allFlags)
                .Where(m => AreMethodsGenericallyEqual(m, method))
                .ToArray();

            var newMethods = newReflectedType.GetMethods(allFlags)
                .Where(m => AreMethodsGenericallyEqual(m, method))
                .ToArray();

            for (int it = 0; it < oldMethods.Length; it++)
                if (oldMethods[it] == method)
                    return newMethods[it];

            return null;
        }

        protected static bool AreTypesGenericallyEqual(Type a, Type b)
        {
#if NET45
            return a.MetadataToken == b.MetadataToken;
#else
            if (a.Attributes != b.Attributes)
                return false;

            if (a.Name != b.Name)
                return false;

            if (a.DeclaringType != null && b.DeclaringType != null)
            {
                if (a.DeclaringType.IsGenericType != b.DeclaringType.IsGenericType)
                    return false;

                var da = a.DeclaringType;
                var db = b.DeclaringType;

                if (a.DeclaringType.IsGenericType)
                {
                    da = da.GetGenericTypeDefinition();
                    db = db.GetGenericTypeDefinition();
                }

                if (!AreTypesGenericallyEqual(da, db))
                    return false;
            }
            else if (a.DeclaringType != b.DeclaringType)
                return false;

            if (a.IsGenericType != b.IsGenericType)
                return false;

            if (a.IsGenericType)
            {
                var ta = a.GetGenericTypeDefinition().GetGenericArguments();
                var tb = b.GetGenericTypeDefinition().GetGenericArguments();

                if (ta.Length != tb.Length)
                    return false;

                if (!ta.Select(tp => tp.Attributes).SequenceEqual(tb.Select(tp => tp.Attributes)))
                    return false;

                if (!ta.Select(tp => tp.Name).SequenceEqual(tb.Select(tp => tp.Name)))
                    return false;
            }

            return true;
#endif
        }

        protected static bool AreMethodsGenericallyEqual(MethodInfo a, MethodInfo b)
        {
#if NET45
            return a.MetadataToken == b.MetadataToken;
#else
            if (a.Attributes != b.Attributes)
                return false;

            if (a.Name != b.Name)
                return false;

            if (a.DeclaringType != null && b.DeclaringType != null)
            {
                if (a.DeclaringType.IsGenericType != b.DeclaringType.IsGenericType)
                    return false;

                var da = a.DeclaringType;
                var db = b.DeclaringType;

                if (a.DeclaringType.IsGenericType)
                {
                    da = da.GetGenericTypeDefinition();
                    db = db.GetGenericTypeDefinition();
                }

                if (!AreTypesGenericallyEqual(da, db))
                    return false;
            }
            else if (a.DeclaringType != b.DeclaringType)
                return false;

            if (a.IsGenericMethod != b.IsGenericMethod)
                return false;

            if (a.IsGenericMethod)
            {
                var ta = a.GetGenericMethodDefinition().GetGenericArguments();
                var tb = b.GetGenericMethodDefinition().GetGenericArguments();

                if (ta.Length != tb.Length)
                    return false;

                if (!ta.Select(tp => tp.Attributes).SequenceEqual(tb.Select(tp => tp.Attributes)))
                    return false;

                if (!ta.Select(tp => tp.Name).SequenceEqual(tb.Select(tp => tp.Name)))
                    return false;
            }

            var pa = a.GetParameters();
            var pb = b.GetParameters();

            if (pa.Length != pb.Length)
                return false;

            if (!pa.Select(tp => tp.Attributes).SequenceEqual(pb.Select(tp => tp.Attributes)))
                return false;

            if (!pa.Select(tp => tp.Name).SequenceEqual(pb.Select(tp => tp.Name)))
                return false;

            return true;
#endif
        }

        public virtual object VisitAny(object obj)
        {
            if (obj is Expression)
                return this.Visit(obj as Expression);

            if (obj is ReadOnlyCollection<Expression>)
                return this.Visit(obj as ReadOnlyCollection<Expression>);

            if (obj is Type)
                return this.VisitType(obj as Type);

            if (obj is MethodInfo)
                return this.VisitMethodInfo(obj as MethodInfo);

            return obj;
        }
    }
}