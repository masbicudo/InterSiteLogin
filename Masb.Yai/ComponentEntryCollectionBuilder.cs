using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masb.Yai
{
    public class ComponentEntryCollectionBuilder
    {
        private readonly ExpressionComposer composer;
        private readonly Dictionary<Type, ComponentEntry> dicTypesToEntries
            = new Dictionary<Type, ComponentEntry>();

        public ComponentEntryCollectionBuilder(ExpressionComposer composer)
        {
            this.composer = composer;
        }

        public void Add<TComponent>()
        {
            if (!this.dicTypesToEntries.ContainsKey(typeof(TComponent)))
            {
                Expression<Func<TComponent>> expression = null;

                if (this.composer != null)
                    expression = this.composer.Compose<TComponent>();

                if (expression == null)
                    throw new Exception("Cannot build component");

                var entryT = new ComponentEntry<TComponent>(expression);

                this.dicTypesToEntries.Add(typeof(TComponent), entryT);
            }
        }

        public void Add(Type componentType)
        {
            if (!this.dicTypesToEntries.ContainsKey(componentType))
            {
                Expression expression = null;

                if (this.composer != null)
                    expression = this.composer.Compose(componentType);

                if (expression == null)
                    throw new Exception("Cannot build component");

                var entry = ComponentEntryBuilder.Create(componentType, expression);

                this.dicTypesToEntries.Add(componentType, entry);
            }
        }

        public Dictionary<Type, ComponentEntry> ToDictionary()
        {
            return new Dictionary<Type, ComponentEntry>(this.dicTypesToEntries);
        }
    }
}