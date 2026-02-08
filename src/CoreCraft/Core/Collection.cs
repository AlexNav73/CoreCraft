using System.Collections;
using System.Diagnostics;
using CoreCraft.ChangesTracking;
using CoreCraft.Exceptions;
using CoreCraft.Persistence;

namespace CoreCraft.Core;

/// <summary>
///     A collection of entity-properties pairs when entity is a key, and properties
///     is a data associated with a given entity
/// </summary>
/// <typeparam name="TEntity">An entity type</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
[DebuggerDisplay("Count = {Count}")]
public sealed class Collection<TEntity, TProperties> :
    IMutableCollection<TEntity, TProperties>,
    IMutableState<ICollection<TEntity, TProperties>>
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly IDictionary<TEntity, TProperties> _relation;
    private readonly Func<Guid, TEntity> _entityFactory;
    private readonly Func<TProperties> _propsFactory;

    /// <summary>
    ///     Ctor
    /// </summary>
    public Collection(CollectionInfo info, Func<Guid, TEntity> entityCreator, Func<TProperties> propsCreator)
        : this(info, new Dictionary<TEntity, TProperties>(), entityCreator, propsCreator)
    {
    }

    private Collection(
        CollectionInfo info,
        IDictionary<TEntity, TProperties> relation,
        Func<Guid, TEntity> entityFactory,
        Func<TProperties> dataFactory)
    {
        _relation = relation;
        _entityFactory = entityFactory;
        _propsFactory = dataFactory;

        Info = info;
    }

    /// <inheritdoc cref="IHaveInfo{T}.Info"/>
    public CollectionInfo Info { get; }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Entities"/>
    public IEnumerable<TEntity> Entities => _relation.Keys;

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Count"/>
    public int Count => _relation.Count;

    /// <inheritdoc cref="IMutableState{T}.AsReadOnly()" />
    public ICollection<TEntity, TProperties> AsReadOnly()
    {
        return this;
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Add(TProperties)"/>
    public TEntity Add(TProperties properties)
    {
        var entity = _entityFactory(Guid.NewGuid());
        Add(entity, properties);
        return entity;
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Add(Guid, Func{TProperties, TProperties})"/>
    public TEntity Add(Guid id, Func<TProperties, TProperties> init)
    {
        var entity = _entityFactory(id);
        Add(entity, init(_propsFactory()));
        return entity;
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Add(TEntity, TProperties)"/>
    public void Add(TEntity entity, TProperties properties)
    {
        if (_relation.ContainsKey(entity))
        {
            throw new DuplicateKeyException($"Entity [{entity}] can't be added to the collection");
        }

        _relation.Add(entity, properties);
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Get(TEntity)"/>
    public TProperties Get(TEntity entity)
    {
        if (_relation.TryGetValue(entity, out var properties))
        {
            return properties;
        }

        throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Contains(TEntity)"/>
    public bool Contains(TEntity entity)
    {
        return _relation.ContainsKey(entity);
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Modify(TEntity, Func{TProperties, TProperties})"/>
    public TProperties Modify(TEntity entity, Func<TProperties, TProperties> modifier)
    {
        if (_relation.TryGetValue(entity, out var properties))
        {
            var newData = modifier(properties);
            _relation[entity] = newData;
            return newData;
        }
        else
        {
            throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
        }
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Remove(TEntity)"/>
    public void Remove(TEntity entity)
    {
        if (_relation.ContainsKey(entity))
        {
            _relation.Remove(entity);
        }
        else
        {
            throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
        }
    }

    /// <inheritdoc cref="ILoadable.Load(IRepository)"/>
    public void Load(IRepository repository)
    {
        if (_relation.Count != 0)
        {
            throw new NonEmptyModelException($"The [{Info.ShardName}.{Info.Name}] is not empty. Clear or recreate the model before loading data");
        }

        repository.Load(this);
    }

    /// <inheritdoc cref="ICopy{T}.Copy()"/>
    public ICollection<TEntity, TProperties> Copy()
    {
        return new Collection<TEntity, TProperties>(
            Info,
            new Dictionary<TEntity, TProperties>(_relation),
            _entityFactory,
            _propsFactory);
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.ApplyAsync(ICollectionChangeSet{TEntity, TProperties}, CancellationToken)" />
    public Task ApplyAsync(ICollectionChangeSet<TEntity, TProperties> changeSet, CancellationToken token = default)
    {
        foreach (var change in changeSet)
        {
            switch (change.Action)
            {
                case CollectionAction.Add:
                    // Add expects NewData to be non-null
                    Add(change.Entity, change.NewData!);
                    break;
                case CollectionAction.Remove:
                    Remove(change.Entity);
                    break;
                case CollectionAction.Modify:
                    Modify(change.Entity, d =>
                    {
                        var bag = new PropertiesBag();
                        change.NewData!.WriteTo(bag);

                        return (TProperties)d.ReadFrom(bag);
                    });
                    break;
                default:
                    throw new NotSupportedException($"An action [{change.Action}] is not supported.");
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Pairs()" />
    public IEnumerable<(TEntity entity, TProperties properties)> Pairs()
    {
        foreach (var pair in _relation)
        {
            yield return (pair.Key, pair.Value);
        }
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Save(IRepository)" />
    public void Save(IRepository repository)
    {
        repository.Save(this);
    }

    /// <inheritdoc />
    public IEnumerator<TProperties> GetEnumerator()
    {
        return _relation.Values.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
