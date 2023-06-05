using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Subscription.Binding;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal sealed class CollectionSubscription<TChangesFrame, TEntity, TProperties> :
    Subscription<ICollectionChangeSet<TEntity, TProperties>>,
    ISubscription<TChangesFrame>
    where TChangesFrame : class, IChangesFrame
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly List<WeakReference<ICollectionBinding<TEntity, TProperties>>> _collectionBindings;
    private readonly Dictionary<TEntity, List<WeakReference<IEntityBinding<TEntity, TProperties>>>> _entityBindings;

    public CollectionSubscription(Func<TChangesFrame, ICollectionChangeSet<TEntity, TProperties>> accessor)
    {
        Accessor = accessor;
        _collectionBindings = new();
        _entityBindings = new();
    }

    public Func<TChangesFrame, ICollectionChangeSet<TEntity, TProperties>> Accessor { get; }

    public void Bind(ICollectionBinding<TEntity, TProperties> binding)
    {
        var reference = new WeakReference<ICollectionBinding<TEntity, TProperties>>(binding);
        _collectionBindings.Add(reference);
    }

    public void Bind(TEntity entity, IEntityBinding<TEntity, TProperties> binding)
    {
        var reference = new WeakReference<IEntityBinding<TEntity, TProperties>>(binding);
        if (_entityBindings.TryGetValue(entity, out var bindings))
        {
            bindings.Add(reference);
        }
        else
        {
            _entityBindings.Add(entity, new List<WeakReference<IEntityBinding<TEntity, TProperties>>>() { reference });
        }
    }

    public void Publish(Change<TChangesFrame> change)
    {
        var (oldModel, newModel, hunk) = change;

        var collectionChangeSet = Accessor(hunk);
        if (collectionChangeSet.HasChanges())
        {
            var changes = new Change<ICollectionChangeSet<TEntity, TProperties>>(
                oldModel,
                newModel,
                collectionChangeSet);

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
                target.OnCollectionChanged(bindingChanges);
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
                    target.OnEntityChanged(change.OldData!, change.NewData!);
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
}
