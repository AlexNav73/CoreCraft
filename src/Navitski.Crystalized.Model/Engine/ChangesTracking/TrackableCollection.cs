using System.Collections;
using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <inheritdoc cref="IMutableCollection{TEntity, TProperties}"/>
[DebuggerDisplay("{_collection}")]
public sealed class TrackableCollection<TEntity, TProperties> : IMutableCollection<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly ICollectionChangeSet<TEntity, TProperties> _changes;
    private readonly IMutableCollection<TEntity, TProperties> _collection;

    /// <summary>
    ///     Ctor
    /// </summary>
    public TrackableCollection(
        ICollectionChangeSet<TEntity, TProperties> changesCollection,
        ICollection<TEntity, TProperties> modelCollection)
    {
        _changes = changesCollection;
        _collection = (IMutableCollection<TEntity, TProperties>)modelCollection;
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Count"/>
    public int Count => _collection.Count;

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Add(TProperties)"/>
    public TEntity Add(TProperties properties)
    {
        var entity = _collection.Add(properties);
        _changes.Add(CollectionAction.Add, entity, default, properties);
        return entity;
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Add(Guid, Func{TProperties, TProperties})" />
    public TEntity Add(Guid id, Func<TProperties, TProperties> init)
    {
        var entity = _collection.Add(id, init);
        var properties = _collection.Get(entity);

        _changes.Add(CollectionAction.Add, entity, default, properties);

        return entity;
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Add(TEntity, TProperties)" />
    public void Add(TEntity entity, TProperties properties)
    {
        _collection.Add(entity, properties);
        _changes.Add(CollectionAction.Add, entity, default, properties);
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Get(TEntity)" />
    public TProperties Get(TEntity entity)
    {
        return _collection.Get(entity);
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Contains(TEntity)" />
    public bool Contains(TEntity entity)
    {
        return _collection.Contains(entity);
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Modify(TEntity, Func{TProperties, TProperties})" />
    public void Modify(TEntity entity, Func<TProperties, TProperties> modifier)
    {
        var oldProps = _collection.Get(entity);
        _collection.Modify(entity, modifier);
        var newProps = _collection.Get(entity);

        if (!oldProps.Equals(newProps))
        {
            _changes.Add(CollectionAction.Modify, entity, oldProps, newProps);
        }
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Remove(TEntity)" />
    public void Remove(TEntity entity)
    {
        var properties = _collection.Get(entity);
        _changes.Add(CollectionAction.Remove, entity, properties, default);
        _collection.Remove(entity);
    }

    /// <inheritdoc cref="ICopy{T}.Copy" />
    public ICollection<TEntity, TProperties> Copy()
    {
        throw new InvalidOperationException("Collection can't be copied because it is attached to changes tracking system");
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator" />
    public IEnumerator<TEntity> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    /// <inheritdoc cref="IEnumerable.GetEnumerator" />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
