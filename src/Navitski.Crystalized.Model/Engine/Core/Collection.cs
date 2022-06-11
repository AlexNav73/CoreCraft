using Navitski.Crystalized.Model.Engine.Exceptions;
using System.Collections;
using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.Core;

/// <summary>
///     A collection of entity-properties pairs when entity is a key, and properties
///     is a data associated with a given entity
/// </summary>
/// <typeparam name="TEntity">An entity type</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
[DebuggerDisplay("Count = {Count}")]
public class Collection<TEntity, TProperties> : IMutableCollection<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly IDictionary<Guid, TProperties> _relation;
    private readonly Func<Guid, TEntity> _entityFactory;
    private readonly Func<TProperties> _propsFactory;

    public Collection(Func<Guid, TEntity> entityCreator, Func<TProperties> propsCreator)
        : this(new Dictionary<Guid, TProperties>(), entityCreator, propsCreator)
    {
    }

    private Collection(
        IDictionary<Guid, TProperties> relation,
        Func<Guid, TEntity> entityFactory,
        Func<TProperties> dataFactory)
    {
        _relation = relation;
        _entityFactory = entityFactory;
        _propsFactory = dataFactory;
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Count"/>
    public int Count => _relation.Count;

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
        if (_relation.ContainsKey(entity.Id))
        {
            throw new DuplicateKeyException($"Entity [{entity}] can't be added to the collection");
        }

        _relation.Add(entity.Id, properties);
    }

    /// <inheritdoc cref="ICollection{TEntity, TProperties}.Get(TEntity)"/>
    public TProperties Get(TEntity entity)
    {
        if (_relation.TryGetValue(entity.Id, out var properties))
        {
            return properties;
        }

        throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Modify(TEntity, Func{TProperties, TProperties})"/>
    public void Modify(TEntity entity, Func<TProperties, TProperties> modifier)
    {
        if (_relation.TryGetValue(entity.Id, out var properties))
        {
            _relation[entity.Id] = modifier(properties);
        }
        else
        {
            throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
        }
    }

    /// <inheritdoc cref="IMutableCollection{TEntity, TProperties}.Remove(TEntity)"/>
    public void Remove(TEntity entity)
    {
        if (_relation.ContainsKey(entity.Id))
        {
            _relation.Remove(entity.Id);
        }
        else
        {
            throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
        }
    }

    /// <inheritdoc cref="ICopy{T}.Copy"/>
    public ICollection<TEntity, TProperties> Copy()
    {
        return new Collection<TEntity, TProperties>(
            new Dictionary<Guid, TProperties>(_relation),
            _entityFactory,
            _propsFactory);
    }

    /// <inheritdoc />
    public IEnumerator<TEntity> GetEnumerator()
    {
        return _relation.Keys.Select(_entityFactory).GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
