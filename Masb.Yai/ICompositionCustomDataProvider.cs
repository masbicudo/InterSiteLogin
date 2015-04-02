using JetBrains.Annotations;

namespace Masb.Yai
{
    public interface ICompositionCustomDataProvider
    {
        [NotNull]
        TypedAndNamedCollection CustomData { get; }
    }
}