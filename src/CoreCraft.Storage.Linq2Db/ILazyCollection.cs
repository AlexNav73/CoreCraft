using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db;

/// <summary>
///     A read-only collection of entity-properties pairs
/// </summary>
/// <typeparam name="TEntity">An entity type</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
public interface ILazyCollection<TEntity, TProperties> : IQueryable<TProperties>, IHaveInfo<CollectionInfo>
    where TEntity : Entity
    where TProperties : Properties, IHaveEntityId<TEntity>
{
    /// <summary>
    ///     Returns properties of a given entity
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <returns>Properties for a given entities</returns>
    /// <exception cref="KeyNotFoundException">Throws when an entity is not present in the collection</exception>
    Task<TProperties?> GetAsync(TEntity entity, CancellationToken token = default);

    /// <summary>
    ///     Tests if a collection contains an entity
    /// </summary>
    /// <param name="entity">An entity to check</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <returns>True - if a collection contains an entity</returns>
    Task<bool> ContainsAsync(TEntity entity, CancellationToken token = default);
}
