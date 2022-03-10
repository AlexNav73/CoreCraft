using System.Collections;
using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.Core;

[DebuggerDisplay("Count = {Count}")]
public class Collection<TEntity, TData> : IMutableCollection<TEntity, TData>
    where TEntity : Entity
    where TData : Properties
{
    private readonly IDictionary<Guid, TData> _relation;
    private readonly Func<Guid, TEntity> _entityFactory;
    private readonly Func<TData> _dataFactory;

    public Collection(Func<Guid, TEntity> entityCreator, Func<TData> dataCreator)
        : this(new Dictionary<Guid, TData>(), entityCreator, dataCreator)
    {
    }

    private Collection(
        IDictionary<Guid, TData> relation,
        Func<Guid, TEntity> entityFactory,
        Func<TData> dataFactory)
    {
        _relation = relation;
        _entityFactory = entityFactory;
        _dataFactory = dataFactory;
    }

    public int Count => _relation.Count;

    public TEntity Add(TData data)
    {
        var entity = _entityFactory(Guid.NewGuid());
        Add(entity, data);
        return entity;
    }

    public TEntity Add(Guid id, Func<TData, TData> init)
    {
        var entity = _entityFactory(id);
        Add(entity, init(_dataFactory()));
        return entity;
    }

    public void Add(TEntity entity, TData data)
    {
        if (_relation.ContainsKey(entity.Id))
        {
            throw new InvalidOperationException($"Entity [{entity}] can't be added to the collection");
        }

        _relation.Add(entity.Id, data);
    }

    public TData Get(TEntity entity)
    {
        if (_relation.TryGetValue(entity.Id, out var data))
        {
            return data;
        }

        throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
    }

    public void Modify(TEntity entity, Func<TData, TData> modifier)
    {
        if (_relation.TryGetValue(entity.Id, out var data))
        {
            _relation[entity.Id] = modifier(data);
        }
        else
        {
            throw new KeyNotFoundException($"Collection doesn't contain entity [{entity}]");
        }
    }

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

    public ICollection<TEntity, TData> Copy()
    {
        return new Collection<TEntity, TData>(
            new Dictionary<Guid, TData>(_relation),
            _entityFactory,
            _dataFactory);
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        return _relation.Keys.Select(_entityFactory).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
