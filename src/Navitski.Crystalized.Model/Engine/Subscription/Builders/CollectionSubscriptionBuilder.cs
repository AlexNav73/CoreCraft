using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Subscription.Binding;

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

    public IDisposable Bind(ICollectionBinding<TEntity, TProperties> binding)
    {
        var subscription = _root.Bind(binding);

        NotifyIfHasChanges(collection =>
        {
            var changes = CreateBindingChanges(_changes!.OldModel, _changes.NewModel, collection);

            binding.OnCollectionChanged(changes);
        });

        return subscription;
    }

    public IDisposable Bind(TEntity entity, IEntityBinding<TEntity, TProperties> binding)
    {
        var subscription = _root.Bind(entity, binding);

        NotifyIfHasChanges(collection =>
        {
            foreach (var change in collection.Where(c => c.Action == CollectionAction.Modify))
            {
                binding.OnEntityChanged(change.OldData!, change.NewData!);
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

    private BindingChanges<TEntity, TProperties> CreateBindingChanges(
        IModel oldModel,
        IModel newModel,
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

        return new BindingChanges<TEntity, TProperties>(
            oldModel,
            newModel,
            added,
            removed,
            modified);
    }
}
