using System.Collections;
using System.Diagnostics;
using CoreCraft.Engine.Exceptions;

namespace CoreCraft.Engine.Core;

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
    public Collection(string id, Func<Guid, TEntity> entityCreator, Func<TProperties> propsCreator)
        : this(id, new Dictionary<TEntity, TProperties>(), entityCreator, propsCreator)
    {
    }

    private Collection(
        string id,
        IDictionary<TEntity, TProperties> relation,
        Func<Guid, TEntity> entityFactory,
        Func<TProperties> dataFactory)
    {
        _relation = relation;
        _entityFactory = entityFactory;
        _propsFactory = dataFactory;

        Id = id;
    }

    /// <inheritdoc cref="IHaveId.Id"/>
    public string Id { get; }

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
    public void Modify(TEntity entity, Func<TProperties, TProperties> modifier)
    {
        if (_relation.TryGetValue(entity, out var properties))
        {
            _relation[entity] = modifier(properties);
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

    /// <inheritdoc cref="ICopy{T}.Copy()"/>
    public ICollection<TEntity, TProperties> Copy()
    {
        return new Collection<TEntity, TProperties>(
            Id,
            new Dictionary<TEntity, TProperties>(_relation),
            _entityFactory,
            _propsFactory);
    }

    /// <inheritdoc />
    public IEnumerator<TEntity> GetEnumerator()
    {
        return _relation.Keys.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Pairs()" />
    public IEnumerable<(TEntity entity, TProperties properties)> Pairs()
    {
        foreach (var pair in _relation)
        {
            yield return (pair.Key, pair.Value);
        }
    }
}
