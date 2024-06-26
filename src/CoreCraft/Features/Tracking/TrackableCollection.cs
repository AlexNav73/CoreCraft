﻿using System.Collections;
using System.Diagnostics;
using CoreCraft.ChangesTracking;
using CoreCraft.Persistence;

namespace CoreCraft.Features.Tracking;

/// <inheritdoc cref="IMutableCollection{TEntity, TProperties}"/>
[DebuggerDisplay("{_collection}")]
public sealed class TrackableCollection<TEntity, TProperties> :
    IMutableCollection<TEntity, TProperties>,
    IMutableState<ICollection<TEntity, TProperties>>
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
        IMutableCollection<TEntity, TProperties> modelCollection)
    {
        _changes = changesCollection;
        _collection = modelCollection;
    }

    /// <inheritdoc cref="IHaveInfo{T}.Info" />
    public CollectionInfo Info => _collection.Info;

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Count"/>
    public int Count => _collection.Count;

    /// <inheritdoc cref="IMutableState{T}.AsReadOnly()" />
    public ICollection<TEntity, TProperties> AsReadOnly()
    {
        return ((IMutableState<ICollection<TEntity, TProperties>>)_collection).AsReadOnly();
    }

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

    /// <inheritdoc cref="ILoadable.Load(IRepository)" />
    public void Load(IRepository repository)
    {
        if (_collection.Count > 0)
        {
            // Multiple independent parts of an application can be dependent on the same data.
            // These application parts should not know, whether the other parts already had been
            // loaded the data, so they can try to load the same data multiple times. In this case
            // load data only once and the subsequent loads should be a noop.
            return;
        }

        repository.Load(this);
    }

    /// <inheritdoc cref="ICopy{T}.Copy" />
    public ICollection<TEntity, TProperties> Copy()
    {
        throw new InvalidOperationException("Collection can't be copied because it is attached to changes tracking system");
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Pairs" />
    public IEnumerable<(TEntity entity, TProperties properties)> Pairs()
    {
        return _collection.Pairs();
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Save(IRepository)" />
    public void Save(IRepository repository)
    {
        _collection.Save(repository);
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
