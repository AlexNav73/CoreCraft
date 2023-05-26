using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription.Builders;

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

    public IDisposable Subscribe(Action<Change<ICollectionChangeSet<TEntity, TProperties>>> handler)
    {
        var subscription = _root.Add(handler);

        if (_changes != null)
        {
            var collection = _root.Accessor(_changes.Hunk);
            if (collection.HasChanges())
            {
                handler(new Change<ICollectionChangeSet<TEntity, TProperties>>(_changes.OldModel, _changes.NewModel, collection));
            }
        }

        return subscription;
    }
}
