using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masb.Yai
{
    public class ThreadSafeComponentContainer : ComponentContainer
    {
        private readonly ExpressionComposer composer;
        private readonly Dictionary<Type, ComponentEntry> dicTypesToEntries
            = new Dictionary<Type, ComponentEntry>();

        public ThreadSafeComponentContainer(ExpressionComposer composer)
        {
            this.composer = composer;
        }

        public override ComponentEntry<TComponent> GetComponentEntry<TComponent>()
        {
            ComponentEntry<TComponent> entryT;
            lock (this.dicTypesToEntries)
            {
                ComponentEntry entry;
                if (!this.dicTypesToEntries.TryGetValue(typeof(TComponent), out entry))
                {
                    Expression<Func<TComponent>> expression = null;

                    if (this.composer != null)
                        expression = this.composer.Compose<TComponent>();

                    if (expression == null)
                        return null;

                    entryT = new ComponentEntry<TComponent>(expression);

                    this.dicTypesToEntries.Add(typeof(TComponent), entryT);
                }
                else
                {
                    entryT = (ComponentEntry<TComponent>)entry;
                }
            }

            return entryT;
        }

        public override ComponentEntry GetComponentEntry(Type componentType)
        {
            ComponentEntry entry;
            lock (this.dicTypesToEntries)
            {
                if (!this.dicTypesToEntries.TryGetValue(componentType, out entry))
                {
                    Expression expression = null;

                    if (this.composer != null)
                        expression = this.composer.Compose(componentType);

                    if (expression == null)
                        throw new Exception("Cannot build component");

                    entry = ComponentEntryBuilder.Create(componentType, expression);

                    this.dicTypesToEntries.Add(componentType, entry);
                }
            }

            return entry;
        }
    }
}