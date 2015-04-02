using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Masb.Yai.Markers
{
    public abstract class TypeMarkerBaseAttribute : Attribute
    {
        [NotNull]
        public abstract Type GetTypeFor(
            [NotNull] Type type,
            [NotNull] ExpressionFilterContext context,
            string name,
            object info,
            IEnumerable<CompositionNode> compositionNodes);
    }
}