using System.Collections;
using System.Diagnostics;
using Navitski.Crystalized.Model.Engine.Exceptions;

namespace Navitski.Crystalized.Model.Engine.ChangesTracking;

/// <inheritdoc cref="ICollectionChangeSet{TEntity, TProperties}"/>
[DebuggerDisplay("HasChanges = {HasChanges()}")]
public sealed class CollectionChangeSet<TEntity, TProperties> : ICollectionChangeSet<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly IList<ICollectionChange<TEntity, TProperties>> _changes;

    /// <summary>
    ///     Ctor
    /// </summary>
    public CollectionChangeSet()
        : this(new List<ICollectionChange<TEntity, TProperties>>())
    {
    }

    private CollectionChangeSet(IList<ICollectionChange<TEntity, TProperties>> changes)
    {
        _changes = changes;
    }

    /// <inheritdoc />
    public void Add(CollectionAction action, TEntity entity, TProperties? oldData, TProperties? newData)
    {
        for (var i = _changes.Count - 1; i >= 0; i--)
        {
            var change = _changes[i];
            if (change.Entity == entity)
            {
                if ((change.Action == CollectionAction.Add || change.Action == CollectionAction.Modify) && action == CollectionAction.Modify)
                {
                    _changes[i] = new CollectionChange<TEntity, TProperties>(change.Action, change.Entity, change.OldData, newData);
                }
                else if ((change.Action == CollectionAction.Add || change.Action == CollectionAction.Modify) && action == CollectionAction.Remove)
                {
                    _changes.RemoveAt(i);
                }
                else if ((change.Action == CollectionAction.Add || change.Action == CollectionAction.Modify) && action == CollectionAction.Add)
                {
                    throw new InvalidChangeSequenceException(
                        change,
                        new CollectionChange<TEntity, TProperties>(action, entity, oldData, newData),
                        $"Can't add an entity [{entity}], because it already has been added");
                }
                else if (change.Action == CollectionAction.Remove && action == CollectionAction.Add)
                {
                    _changes[i] = new CollectionChange<TEntity, TProperties>(CollectionAction.Modify, change.Entity, change.OldData, newData);
                }
                else if (change.Action == CollectionAction.Remove && action == CollectionAction.Modify)
                {
                    throw new InvalidChangeSequenceException(
                        change,
                        new CollectionChange<TEntity, TProperties>(action, entity, oldData, newData),
                        $"Can't modify an entity [{entity}], because it already has been removed");
                }
                else if (change.Action == CollectionAction.Remove && action == CollectionAction.Remove)
                {
                    throw new InvalidChangeSequenceException(
                        change,
                        new CollectionChange<TEntity, TProperties>(action, entity, oldData, newData),
                        $"Can't remove an entity [{entity}], because it already has been removed");
                }

                return;
            }
        }

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

    /// <inheritdoc cref="ICollectionChangeSet{TEntity, TProperties}.Merge(ICollectionChangeSet{TEntity, TProperties})"/>
    public ICollectionChangeSet<TEntity, TProperties> Merge(ICollectionChangeSet<TEntity, TProperties> changeSet)
    {
        var result = new CollectionChangeSet<TEntity, TProperties>(_changes.ToList());

        foreach (var change in changeSet)
        {
            result.Add(change.Action, change.Entity, change.OldData, change.NewData);
        }

        return result;
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
