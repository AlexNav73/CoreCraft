using CoreCraft.Engine.ChangesTracking;

namespace CoreCraft.Engine.Subscription;

internal sealed class CollectionSubscription<TChangesFrame, TEntity, TProperties> :
    Subscription<ICollectionChangeSet<TEntity, TProperties>>,
    ISubscription<TChangesFrame>
    where TChangesFrame : class, IChangesFrame
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly List<WeakReference<IObserver<BindingChanges<TEntity, TProperties>>>> _collectionBindings;
    private readonly Dictionary<TEntity, List<WeakReference<IObserver<IEntityChange<TEntity, TProperties>>>>> _entityBindings;

    public CollectionSubscription(Func<TChangesFrame, ICollectionChangeSet<TEntity, TProperties>> accessor)
    {
        Accessor = accessor;
        _collectionBindings = new();
        _entityBindings = new();
    }

    public Func<TChangesFrame, ICollectionChangeSet<TEntity, TProperties>> Accessor { get; }

    public IDisposable Bind(IObserver<BindingChanges<TEntity, TProperties>> observer)
    {
        var reference = new WeakReference<IObserver<BindingChanges<TEntity, TProperties>>>(observer);
        _collectionBindings.Add(reference);

        return new UnsubscribeOnDispose<WeakReference<IObserver<BindingChanges<TEntity, TProperties>>>>(reference, r => _collectionBindings.Remove(r));
    }

    public IDisposable Bind(TEntity entity, IObserver<IEntityChange<TEntity, TProperties>> observer)
    {
        var reference = new WeakReference<IObserver<IEntityChange<TEntity, TProperties>>>(observer);
        if (_entityBindings.TryGetValue(entity, out var bindings))
        {
            bindings.Add(reference);
        }
        else
        {
            _entityBindings.Add(entity, new List<WeakReference<IObserver<IEntityChange<TEntity, TProperties>>>>() { reference });
        }

        return new UnsubscribeOnDispose<WeakReference<IObserver<IEntityChange<TEntity, TProperties>>>>(reference, r => RemoveEntityBinding(entity, r));
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

        var bindingChanges = new BindingChanges<TEntity, TProperties>(
            changes.OldModel,
            changes.NewModel,
            added,
            removed,
            modified);
        NotifyCollectionBindings(bindingChanges);
    }

    private void NotifyCollectionBindings(BindingChanges<TEntity, TProperties> bindingChanges)
    {
        var bindings = _collectionBindings.ToArray();

        for (var i = 0; i < bindings.Length; i++)
        {
            if (bindings[i].TryGetTarget(out var target))
            {
                target.OnNext(bindingChanges);
            }
            else
            {
                _collectionBindings.RemoveAt(i);
            }
        }
    }

    private void NotifyEntityBinding(ICollectionChange<TEntity, TProperties> change)
    {
        if (_entityBindings.TryGetValue(change.Entity, out var references))
        {
            foreach (var reference in references.ToArray())
            {
                if (reference.TryGetTarget(out var target))
                {
                    target.OnNext(change);
                }
                else
                {
                    references.Remove(reference);
                }
            }

            if (references.Count == 0)
            {
                _entityBindings.Remove(change.Entity);
            }
        }
    }

    private void RemoveEntityBinding(TEntity entity, WeakReference<IObserver<IEntityChange<TEntity, TProperties>>> reference)
    {
        if (_entityBindings.TryGetValue(entity, out var references))
        {
            references.Remove(reference);

            if (references.Count == 0)
            {
                _entityBindings.Remove(entity);
            }
        }
    }
}
