using System;

namespace Masb.Yai
{
    public abstract class ComponentContainer
    {
        public abstract ComponentEntry<T> GetComponentEntry<T>();

        public abstract ComponentEntry GetComponentEntry(Type componentType);
    }
}