using System;
using System.Collections.Generic;

namespace Masb.Yai
{
    public class ImmutableComponentContainer : ComponentContainer
    {
        private readonly Dictionary<Type, ComponentEntry> dicTypesToEntries
            = new Dictionary<Type, ComponentEntry>();

        public ImmutableComponentContainer(IDictionary<Type, ComponentEntry> entries, IEqualityComparer<Type> typeComparer)
        {
            this.dicTypesToEntries = new Dictionary<Type, ComponentEntry>(entries, typeComparer);
        }

        public ImmutableComponentContainer(Dictionary<Type, ComponentEntry> entries)
        {
            this.dicTypesToEntries = new Dictionary<Type, ComponentEntry>(entries, entries.Comparer);
        }

        public override ComponentEntry<TComponent> GetComponentEntry<TComponent>()
        {
            ComponentEntry<TComponent> entryT;
            lock (this.dicTypesToEntries)
            {
                ComponentEntry entry;
                if (!this.dicTypesToEntries.TryGetValue(typeof(TComponent), out entry) || (entryT = entry as ComponentEntry<TComponent>) == null)
                    throw new Exception(string.Format("Entry does not exist in this Container for type {0}.", typeof(TComponent).Name));
            }

            return entryT;
        }

        public override ComponentEntry GetComponentEntry(Type componentType)
        {
            ComponentEntry entry;
            lock (this.dicTypesToEntries)
            {
                if (!this.dicTypesToEntries.TryGetValue(componentType, out entry))
                    throw new Exception(string.Format("Entry does not exist in this Container for type {0}.", componentType.Name));
            }

            return entry;
        }
    }
}