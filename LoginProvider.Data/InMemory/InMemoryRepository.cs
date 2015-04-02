using System;
using System.Collections.Generic;
using System.Linq;
using LoginProvider.Domain;

namespace LoginProvider.Data
{
    public class InMemoryRepository<T> :
        IRepository<T>
        where T : class, IEntity
    {
        private static readonly List<T> Items = new List<T>();

        public virtual IQueryable<T> Queryable
        {
            get { return Items.AsQueryable(); }
        }

        public virtual T CreateNew()
        {
            return Activator.CreateInstance<T>();
        }

        public T GetById(long id)
        {
            var idx = (int)(id - 1);
            var exists = idx >= 0 && idx < Items.Count;
            return exists ? Items[idx] : null;
        }

        public bool Exists(long id)
        {
            var idx = (int)(id - 1);
            var exists = idx >= 0 && idx < Items.Count;
            return exists;
        }

        public bool Delete(long id)
        {
            var idx = (int)(id - 1);
            var exists = idx >= 0 && idx < Items.Count;
            if (exists)
                Items.RemoveAt(idx);
            return exists;
        }

        public bool Delete(T entity)
        {
            var idx = (int)(entity.Id - 1);
            var exists = idx >= 0 && idx < Items.Count;
            if (exists)
                Items.RemoveAt(idx);
            return exists;
        }

        public T Save(T entity)
        {
            if (entity.Id == 0)
            {
                var idx = Items.Count;
                entity.Id = idx + 1;
                Items.Add(entity);
            }

            return entity;
        }
    }
}
