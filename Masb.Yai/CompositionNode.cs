using System.Linq.Expressions;

namespace Masb.Yai
{
    public struct CompositionNode
    {
        public readonly object ReflectedDestinationInfo;
        public readonly string ComponentName;
        public readonly Expression ExpressionNode;

        public CompositionNode(object reflectedDestinationInfo, string componentName, Expression expressionNode)
        {
            this.ReflectedDestinationInfo = reflectedDestinationInfo;
            this.ComponentName = componentName;
            this.ExpressionNode = expressionNode;
        }
    }
}