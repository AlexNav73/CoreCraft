using System.Collections;
using System.Diagnostics;

namespace PricingCalc.Model.Engine.ChangesTracking;

[DebuggerDisplay("{_collection}")]
public class TrackableCollection<TEntity, TData> : IMutableCollection<TEntity, TData>
    where TEntity : Entity
    where TData : Properties
{
    private readonly ICollectionChangeSet<TEntity, TData> _changes;
    private readonly IMutableCollection<TEntity, TData> _collection;

    public TrackableCollection(
        ICollectionChangeSet<TEntity, TData> changesCollection,
        ICollection<TEntity, TData> modelCollection)
    {
        _changes = changesCollection;
        _collection = (IMutableCollection<TEntity, TData>)modelCollection;
    }

    public int Count => _collection.Count;

    public TEntity Add(TData data)
    {
        var entity = _collection.Add(data);
        _changes.Add(CollectionAction.Add, entity, default, data);
        return entity;
    }

    public TEntity Add(Guid id, Func<TData, TData> init)
    {
        var entity = _collection.Add(id, init);
        var data = _collection.Get(entity);

        _changes.Add(CollectionAction.Add, entity, default, data);

        return entity;
    }

    public void Add(TEntity entity, TData data)
    {
        _collection.Add(entity, data);
        _changes.Add(CollectionAction.Add, entity, default, data);
    }

    public TData Get(TEntity entity)
    {
        return _collection.Get(entity);
    }

    public void Modify(TEntity entity, Func<TData, TData> modifier)
    {
        var oldData = _collection.Get(entity);
        _collection.Modify(entity, modifier);
        var newData = _collection.Get(entity);

        if (!oldData.Equals(newData))
        {
            _changes.Add(CollectionAction.Modify, entity, oldData, newData);
        }
    }

    public void Remove(TEntity entity)
    {
        var data = _collection.Get(entity);
        _changes.Add(CollectionAction.Remove, entity, data, default);
        _collection.Remove(entity);
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ICollection<TEntity, TData> Copy()
    {
        throw new InvalidOperationException("Collection can't be copied because it is attached to changes tracking system");
    }
}
