using System.Collections;
using System.Linq.Expressions;
using CoreCraft.Persistence;
using LinqToDB;

namespace CoreCraft.Core;

/// <summary>
///     A read-only collection of entity-properties pairs
/// </summary>
/// <typeparam name="TEntity">An entity type</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
public interface ICollection<TEntity, TProperties> : IEnumerable<TEntity>, IHaveInfo<CollectionInfo>, ICopy<ICollection<TEntity, TProperties>>
    where TEntity : Entity
    where TProperties : Properties
{
    /// <summary>
    ///     A count of entities
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Returns properties of a given entity
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <returns>Properties for a given entities</returns>
    /// <exception cref="KeyNotFoundException">Throws when an entity is not present in the collection</exception>
    TProperties Get(TEntity entity);

    /// <summary>
    ///     Tests if a collection contains an entity
    /// </summary>
    /// <param name="entity">An entity to check</param>
    /// <returns>True - if a collection contains an entity</returns>
    bool Contains(TEntity entity);

    /// <summary>
    ///     Returns an iterator over entity-property pairs
    /// </summary>
    /// <returns>An iterator over entity-property pairs</returns>
    IEnumerable<(TEntity entity, TProperties properties)> Pairs();

    /// <summary>
    ///     Saves the collection to the specified repository.
    /// </summary>
    /// <param name="repository">The repository where the entities will be saved.</param>
    void Save(IRepository repository);
}

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
    /// 
    /// </summary>
    IQueryable<TEntity> Entities { get; }

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
    Task<TEntity> AddAsync(TProperties properties, CancellationToken token = default);

    /// <summary>
    ///     Modifies properties of the given entity
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <param name="property"></param>
    /// <param name="value"></param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <exception cref="KeyNotFoundException">Throws when trying to modify an entity which is not present in the collection</exception>
    Task ModifyAsync<T>(TEntity entity, Expression<Func<TProperties, T>> property, T value, CancellationToken token = default);

    /// <summary>
    ///     Removes entity with properties from the collection
    /// </summary>
    /// <param name="entity">An entity</param>
    /// <param name="token">Cancellation token to cancel the asynchronous operation</param>
    /// <exception cref="KeyNotFoundException">Throws when trying to remove an entity which is not present in the collection</exception>
    Task RemoveAsync(TEntity entity, CancellationToken token = default);
}

/// <summary>
///     Represents an object that has an entity identifier.
/// </summary>
public interface IHaveEntityId<out TEntity> where TEntity : Entity
{
    /// <summary>
    ///     Gets the unique identifier of the entity.
    /// </summary>
    TEntity EntityId { get; }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TProperties"></typeparam>
public sealed class LazyCollection<TEntity, TProperties> : IMutableLazyCollection<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties, IHaveEntityId<TEntity>
{
    private readonly ITable<TProperties> _table;

    /// <inheritdoc />
    public Type ElementType => _table.ElementType;

    /// <inheritdoc />
    public Expression Expression => _table.Expression;

    /// <inheritdoc />
    public IQueryProvider Provider => _table.Provider;

    /// <inheritdoc />
    public CollectionInfo Info { get; }

    /// <inheritdoc />
    public IQueryable<TEntity> Entities => _table.Select(p => p.EntityId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="table"></param>
    /// <param name="info"></param>
    public LazyCollection(ITable<TProperties> table, CollectionInfo info)
    {
        _table = table;
        
        Info = info;
    }

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TProperties properties, CancellationToken token = default)
    {
        var inserted = await _table.InsertWithOutputAsync(properties, token);

        return inserted.EntityId;
    }

    /// <inheritdoc />
    public Task ModifyAsync<T>(TEntity entity, Expression<Func<TProperties, T>> property, T value, CancellationToken token = default)
    {
        return _table.Where(p => p.EntityId == entity)
            .Set(property, value)
            .UpdateAsync(token);
    }

    /// <inheritdoc />
    public Task RemoveAsync(TEntity entity, CancellationToken token = default)
    {
        return _table.Where(p => p.EntityId == entity)
            .DeleteAsync(token);
    }

    /// <inheritdoc />
    public Task<TProperties?> GetAsync(TEntity entity, CancellationToken token = default)
    {
        return _table.FirstOrDefaultAsync(p => p.EntityId == entity, token);
    }

    /// <inheritdoc />
    public Task<bool> ContainsAsync(TEntity entity, CancellationToken token = default)
    {
        return _table.AnyAsync(p => p.EntityId == entity, token);
    }

    /// <inheritdoc />
    public IEnumerator<TProperties> GetEnumerator()
    {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
