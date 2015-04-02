using System.Collections.Generic;
using JetBrains.Annotations;

namespace Masb.Yai
{
    public interface ICompositionStackProvider
    {
        [NotNull]
        Stack<CompositionNode> CompositionStack { get; }
    }
}