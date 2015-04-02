using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infra
{
    class test
    {
        public void xpto()
        {
            Expression<Action> e1 = () => A<int>.M();
            Expression<Action> e2 = () => A<float>.M();

            CatchBlock node = null;
            var test = node.Test;

            Expression expr1 = null;
            ExpressionVisitor v = null;
            var expr = v.Visit(expr1);

            MethodInfo mi = null;
            //mi.MetadataToken;

            Type t = null;
            t.GetMethod("").GetBaseDefinition();
            //t.MetadataToken
            //Interlocked.Increment(ref this.readWriteBalance);
            //ReaderWriterLockSlim
            int[] array = null;
            array.ToArray();

            ManualResetEvent mre = null;
            mre.WaitOne(0);
        }

        class A<T>
        {
            public static void M() { }
        }

        class visitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression node)
            {
                return base.VisitExtension(node);
            }

            protected override Expression VisitInvocation(InvocationExpression node)
            {
                return base.VisitInvocation(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return base.VisitLambda(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                return base.VisitMethodCall(node);
            }
        }
    }
}
