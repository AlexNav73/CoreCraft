using System.Linq.Expressions;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;

namespace CoreCraft.Storage.Linq2Db;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TProperties"></typeparam>
public interface IMutableLazyCollection<TEntity, TProperties> : ILazyCollection<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties, IHaveEntityId<TEntity>
{
    /// <summary>
    ///     Adds a new properties to the collection with a generated entity
    /// </summary>
    /// <param name="properties">A new properties</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <returns>New entity</returns>
    TEntity Add(TProperties properties);

    /// <summary>
    ///     Adds a new properties to the collection with a generated entity
    /// </summary>
    /// <param name="properties">A new properties</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <returns>New entity</returns>
    Task<TEntity> AddAsync(TProperties properties, CancellationToken token = default);

    /// <summary>
    ///     Modifies properties of the given entity
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <param name="modifier"></param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <exception cref="KeyNotFoundException">Throws when trying to modify an entity which is not present in the collection</exception>
    TProperties? Modify(TEntity entity, Expression<Func<TProperties, TProperties>> modifier);

    /// <summary>
    ///     Modifies properties of the given entity
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <param name="modifier"></param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <exception cref="KeyNotFoundException">Throws when trying to modify an entity which is not present in the collection</exception>
    Task<TProperties?> ModifyAsync(TEntity entity, Expression<Func<TProperties, TProperties>> modifier, CancellationToken token = default);

    /// <summary>
    ///     Removes entity with properties from the collection
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <exception cref="KeyNotFoundException">Throws when trying to remove an entity which is not present in the collection</exception>
    void Remove(TEntity entity);

    /// <summary>
    ///     Removes entity with properties from the collection
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <exception cref="KeyNotFoundException">Throws when trying to remove an entity which is not present in the collection</exception>
    Task RemoveAsync(TEntity entity, CancellationToken token = default);

    /// <summary>
    ///     Asynchronously applies the specified set of changes to the underlying data store.
    /// </summary>
    /// <param name="changeSet">A collection of entity changes to be applied. Cannot be null.</param>
    /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous apply operation.</returns>
    Task ApplyAsync(ICollectionChangeSet<TEntity, TProperties> changeSet, CancellationToken token = default);
}
