using CoreCraft.ChangesTracking;

namespace CoreCraft.Subscription;

internal sealed class CollectionSubscription<TChangesFrame, TEntity, TProperties> :
    Subscription<ICollectionChangeSet<TEntity, TProperties>>,
    ISubscription<TChangesFrame>
    where TChangesFrame : class, IChangesFrame
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly List<IObserver<Change<CollectionChangeGroups<TEntity, TProperties>>>> _collectionBindings;
    private readonly Dictionary<TEntity, List<IObserver<IEntityChange<TEntity, TProperties>>>> _entityBindings;

    public CollectionSubscription(Func<TChangesFrame, ICollectionChangeSet<TEntity, TProperties>> accessor)
    {
        _collectionBindings = new();
        _entityBindings = new();

        Accessor = accessor;
    }

    internal Func<TChangesFrame, ICollectionChangeSet<TEntity, TProperties>> Accessor { get; }

    public IDisposable Bind(IObserver<Change<CollectionChangeGroups<TEntity, TProperties>>> observer)
    {
        _collectionBindings.Add(observer);

        return new UnsubscribeOnDispose<IObserver<Change<CollectionChangeGroups<TEntity, TProperties>>>>(observer, r => _collectionBindings.Remove(r));
    }

    public IDisposable Bind(TEntity entity, IObserver<IEntityChange<TEntity, TProperties>> observer)
    {
        if (_entityBindings.TryGetValue(entity, out var bindings))
        {
            bindings.Add(observer);
        }
        else
        {
            _entityBindings.Add(entity, [observer]);
        }

        return new UnsubscribeOnDispose<IObserver<IEntityChange<TEntity, TProperties>>>(observer, r => RemoveEntityBinding(entity, r));
    }

    public void Publish(Change<TChangesFrame> change)
    {
        var collectionChangeSet = Accessor(change.Hunk);
        if (collectionChangeSet.HasChanges())
        {
            var changes = change.Map(_ => collectionChangeSet);

            Publish(changes);
            UpdateBindings(changes);
        }
    }

    private void UpdateBindings(Change<ICollectionChangeSet<TEntity, TProperties>> changes)
    {
        var collectionChangeGroups = ParseCollectionChanges(changes);

        NotifyCollectionBindings(changes.Map(_ => collectionChangeGroups));
    }

    private void NotifyCollectionBindings(Change<CollectionChangeGroups<TEntity, TProperties>> changes)
    {
        var bindings = _collectionBindings.ToArray();

        for (var i = 0; i < bindings.Length; i++)
        {
            bindings[i].OnNext(changes);
        }
    }

    private void NotifyEntityBinding(ICollectionChange<TEntity, TProperties> change)
    {
        if (_entityBindings.TryGetValue(change.Entity, out var references))
        {
            foreach (var reference in references.ToArray())
            {
                reference.OnNext(change);
            }
        }
    }

    private void RemoveEntityBinding(TEntity entity, IObserver<IEntityChange<TEntity, TProperties>> observer)
    {
        if (_entityBindings.TryGetValue(entity, out var observers))
        {
            observers.Remove(observer);

            if (observers.Count == 0)
            {
                _entityBindings.Remove(entity);
            }
        }
    }

    private CollectionChangeGroups<TEntity, TProperties> ParseCollectionChanges(Change<ICollectionChangeSet<TEntity, TProperties>> changes)
    {
        var added = new List<ICollectionChange<TEntity, TProperties>>();
        var removed = new List<ICollectionChange<TEntity, TProperties>>();
        var modified = new List<ICollectionChange<TEntity, TProperties>>();

        foreach (var change in changes.Hunk)
        {
            if (change.Action == CollectionAction.Add)
            {
                added.Add(change);
            }
            else if (change.Action == CollectionAction.Remove)
            {
                removed.Add(change);
                _entityBindings.Remove(change.Entity);
            }
            else if (change.Action == CollectionAction.Modify)
            {
                modified.Add(change);
                NotifyEntityBinding(change);
            }
        }

        return new CollectionChangeGroups<TEntity, TProperties>(added, removed, modified);
    }
}
