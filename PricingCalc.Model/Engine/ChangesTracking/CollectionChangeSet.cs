using System.Collections;
using System.Diagnostics;

namespace PricingCalc.Model.Engine.ChangesTracking;

[DebuggerDisplay("HasChanges = {HasChanges()}")]
public class CollectionChangeSet<TEntity, TData> : ICollectionChangeSet<TEntity, TData>
    where TEntity : Entity
    where TData : Properties
{
    private readonly IList<ICollectionChange<TEntity, TData>> _changes;

    public CollectionChangeSet() : this(new List<ICollectionChange<TEntity, TData>>())
    {
    }

    private CollectionChangeSet(IList<ICollectionChange<TEntity, TData>> changes)
    {
        _changes = changes;
    }

    public void Add(CollectionAction action, TEntity entity, TData? oldData, TData? newData)
    {
        _changes.Add(new CollectionChange<TEntity, TData>(action, entity, oldData, newData));
    }

    public ICollectionChangeSet<TEntity, TData> Invert()
    {
        var inverted = _changes.Reverse().Select(x => x.Invert()).ToList();
        return new CollectionChangeSet<TEntity, TData>(inverted);
    }

    public bool HasChanges() => _changes.Count > 0;

    public void Apply(ICollection<TEntity, TData> collection)
    {
        foreach (var change in _changes)
        {
            switch (change.Action)
            {
                case CollectionAction.Add:
                    collection.Add(change.Entity, change.NewData!);
                    break;
                case CollectionAction.Remove:
                    collection.Remove(change.Entity);
                    break;
                case CollectionAction.Modify:
                    collection.Modify(change.Entity, d =>
                    {
                        var bag = new PropertiesBag();
                        change.NewData!.WriteTo(bag);

                        return (TData)d.ReadFrom(bag);
                    });
                    break;
                default:
                    break;
            }
        }
    }

    public IEnumerator<ICollectionChange<TEntity, TData>> GetEnumerator()
    {
        return _changes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _changes.GetEnumerator();
    }
}
