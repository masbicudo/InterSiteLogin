using System.Linq;

namespace LoginProvider.Domain
{
    /// <summary>
    /// Represents a repository of database persisted entities.
    /// </summary>
    /// <typeparam name="TEntity">Type of the entities.</typeparam>
    public interface IRepository<TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Gets a <see cref="IQueryable{TEntity}"/> object that allows querying the database.
        /// </summary>
        /// <returns>A new entity object.</returns>
        IQueryable<TEntity> Queryable { get; }

        /// <summary>
        /// Creates a new entity that can then be saved.
        /// </summary>
        /// <returns>A new entity object.</returns>
        TEntity CreateNew();

        /// <summary>
        /// Gets an entity by the given numeric ID exists.
        /// </summary>
        /// <param name="id">The numeric ID of the entity to be returned.</param>
        /// <returns>An entity if it exists; otherwise null.</returns>
        TEntity GetById(long id);

        /// <summary>
        /// Gets a value indicating whether an entity with the given numeric ID exists.
        /// </summary>
        /// <param name="id">The numeric ID of the entity to be checked.</param>
        /// <returns>True if an entity with the given ID exists; otherwise false.</returns>
        bool Exists(long id);

        /// <summary>
        /// Deletes an entity from the database given it's numeric ID.
        /// </summary>
        /// <param name="id">The numeric ID of the entity to be deleted.</param>
        /// <returns>True if the entity was deleted; otherwise false.</returns>
        bool Delete(long id);

        /// <summary>
        /// Deletes an entity from the database.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        /// <returns>True if the entity was deleted; otherwise false.</returns>
        bool Delete(TEntity entity);

        /// <summary>
        /// Saves an entity to the database,
        /// returning an object with auto-generated fields set accordingly.
        /// </summary>
        /// <param name="entity">The entity to save to the database.</param>
        /// <returns>An entity with the auto-generated fields set accordingly.</returns>
        TEntity Save(TEntity entity);
    }
}