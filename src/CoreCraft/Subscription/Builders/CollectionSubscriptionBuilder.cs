using CoreCraft.ChangesTracking;

namespace CoreCraft.Subscription.Builders;

internal sealed class CollectionSubscriptionBuilder<T, TEntity, TProperties> : ICollectionSubscriptionBuilder<TEntity, TProperties>
    where T : class, IChangesFrame
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly CollectionSubscription<T, TEntity, TProperties> _root;
    private readonly Change<T>? _changes;

    public CollectionSubscriptionBuilder(CollectionSubscription<T, TEntity, TProperties> root, Change<T>? changes)
    {
        _root = root;
        _changes = changes;
    }

    public IDisposable Bind(IObserver<Change<CollectionChangeGroups<TEntity, TProperties>>> observer)
    {
        var subscription = _root.Bind(observer);

        NotifyIfHasChanges(collection =>
        {
            var changes = _changes!.Map(_ => CreateCollectionChangeGroups(collection));

            observer.OnNext(changes);
        });

        return subscription;
    }

    public IDisposable Bind(TEntity entity, IObserver<IEntityChange<TEntity, TProperties>> observer)
    {
        var subscription = _root.Bind(entity, observer);

        NotifyIfHasChanges(collection =>
        {
            foreach (var change in collection.Where(c => c.Action == CollectionAction.Modify))
            {
                observer.OnNext(change);
            }
        });

        return subscription;
    }

    public IDisposable Subscribe(IObserver<Change<ICollectionChangeSet<TEntity, TProperties>>> observer)
    {
        var subscription = _root.Subscribe(observer);

        NotifyIfHasChanges(collection => observer.OnNext(_changes!.Map(_ => collection)));

        return subscription;
    }

    private void NotifyIfHasChanges(Action<ICollectionChangeSet<TEntity, TProperties>> action)
    {
        if (_changes != null)
        {
            var collection = _root.Accessor(_changes.Hunk);
            if (collection.HasChanges())
            {
                action(collection);
            }
        }
    }

    private static CollectionChangeGroups<TEntity, TProperties> CreateCollectionChangeGroups(
        ICollectionChangeSet<TEntity, TProperties> collectionChanges)
    {
        var added = new List<ICollectionChange<TEntity, TProperties>>();
        var removed = new List<ICollectionChange<TEntity, TProperties>>();
        var modified = new List<ICollectionChange<TEntity, TProperties>>();

        foreach (var change in collectionChanges)
        {
            if (change.Action == CollectionAction.Add)
            {
                added.Add(change);
            }
            else if (change.Action == CollectionAction.Remove)
            {
                removed.Add(change);
            }
            else if (change.Action == CollectionAction.Modify)
            {
                modified.Add(change);
            }
        }

        return new CollectionChangeGroups<TEntity, TProperties>(
            added,
            removed,
            modified);
    }
}
