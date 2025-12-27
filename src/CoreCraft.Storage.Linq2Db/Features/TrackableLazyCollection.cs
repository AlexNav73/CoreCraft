using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using CoreCraft.ChangesTracking;
using CoreCraft.Core;
using LinqToDB;

namespace CoreCraft.Storage.Linq2Db.Features;

/// <inheritdoc cref="IMutableLazyCollection{TEntity, TProperties}"/>
[DebuggerDisplay("{_collection}")]
public sealed class TrackableLazyCollection<TEntity, TProperties> :
    IMutableLazyCollection<TEntity, TProperties>,
    IMutableState<ILazyCollection<TEntity, TProperties>>
    where TEntity : Entity
    where TProperties : Properties, IHaveEntityId<TEntity>
{
    private readonly ICollectionChangeSet<TEntity, TProperties> _changes;
    private readonly IMutableLazyCollection<TEntity, TProperties> _collection;

    /// <summary>
    ///     Ctor
    /// </summary>
    public TrackableLazyCollection(
        ICollectionChangeSet<TEntity, TProperties> changesCollection,
        IMutableLazyCollection<TEntity, TProperties> modelCollection)
    {
        _changes = changesCollection;
        _collection = modelCollection;
    }

    /// <inheritdoc cref="IHaveInfo{T}.Info" />
    public CollectionInfo Info => _collection.Info;
    
    public Type ElementType => _collection.ElementType;
    
    public Expression Expression => _collection.Expression;

    public IQueryProvider Provider => _collection.Provider;

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Count"/>
    public int Count() => _collection.Count();

    /// <inheritdoc cref="IMutableState{T}.AsReadOnly()" />
    public ILazyCollection<TEntity, TProperties> AsReadOnly()
    {
        return ((IMutableState<ILazyCollection<TEntity, TProperties>>)_collection).AsReadOnly();
    }

    /// <inheritdoc cref="IMutableLazyCollection{TEntity, TProperties}.AddAsync(TProperties, CancellationToken)"/>
    public async Task<TEntity> AddAsync(TProperties properties, CancellationToken token = default)
    {
        var entity = await _collection.AddAsync(properties, token);
        _changes.Add(CollectionAction.Add, entity, default, properties);
        return entity;
    }

    public Task<TProperties?> GetAsync(TEntity entity, CancellationToken token = default)
    {
        return _collection.GetAsync(entity, token);
    }

    public Task<bool> ContainsAsync(TEntity entity, CancellationToken token = default)
    {
        return _collection.ContainsAsync(entity, token);
    }

    public async Task<TProperties?> ModifyAsync(TEntity entity, Expression<Func<TProperties, TProperties>> modifier, CancellationToken token = default)
    {
        var oldProps = await _collection.GetAsync(entity, token);
        var newProps = await _collection.ModifyAsync(entity, modifier, token);

        if (oldProps is null || !oldProps.Equals(newProps))
        {
            _changes.Add(CollectionAction.Modify, entity, oldProps, newProps);
        }

        return newProps;
    }

    public async Task RemoveAsync(TEntity entity, CancellationToken token = default)
    {
        var properties = await _collection.GetAsync(entity, token);
        _changes.Add(CollectionAction.Remove, entity, properties, default);
        await _collection.RemoveAsync(entity, token);
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.ApplyAsync(ICollectionChangeSet{TEntity, TProperties}, CancellationToken)"/>
    public Task ApplyAsync(ICollectionChangeSet<TEntity, TProperties> changeSet, CancellationToken token = default)
    {
        throw new InvalidOperationException("Unable to apply changes to the collection");
    }

    /// <inheritdoc cref="IEnumerable.GetEnumerator" />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    IEnumerator<TProperties> IEnumerable<TProperties>.GetEnumerator()
    {
        return _collection.GetEnumerator();
    }
}
