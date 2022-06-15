using System.Collections;
using System.Diagnostics;

namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <inheritdoc cref="ICollectionChangeSet{TEntity, TProperties}"/>
[DebuggerDisplay("HasChanges = {HasChanges()}")]
public class CollectionChangeSet<TEntity, TProperties> : ICollectionChangeSet<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly IList<ICollectionChange<TEntity, TProperties>> _changes;

    /// <summary>
    ///     Ctor
    /// </summary>
    public CollectionChangeSet() : this(new List<ICollectionChange<TEntity, TProperties>>())
    {
    }

    private CollectionChangeSet(IList<ICollectionChange<TEntity, TProperties>> changes)
    {
        _changes = changes;
    }

    /// <inheritdoc />
    public void Add(CollectionAction action, TEntity entity, TProperties? oldData, TProperties? newData)
    {
        _changes.Add(new CollectionChange<TEntity, TProperties>(action, entity, oldData, newData));
    }

    /// <inheritdoc />
    public ICollectionChangeSet<TEntity, TProperties> Invert()
    {
        var inverted = _changes.Reverse().Select(x => x.Invert()).ToList();
        return new CollectionChangeSet<TEntity, TProperties>(inverted);
    }

    /// <inheritdoc />
    public bool HasChanges() => _changes.Count > 0;

    /// <inheritdoc />
    public void Apply(IMutableCollection<TEntity, TProperties> collection)
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

                        return (TProperties)d.ReadFrom(bag);
                    });
                    break;
                default:
                    throw new NotSupportedException($"An action [{change.Action}] is not supported.");
            }
        }
    }

    /// <inheritdoc />
    public IEnumerator<ICollectionChange<TEntity, TProperties>> GetEnumerator()
    {
        return _changes.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _changes.GetEnumerator();
    }
}
