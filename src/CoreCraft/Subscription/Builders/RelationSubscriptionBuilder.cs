using CoreCraft.ChangesTracking;

namespace CoreCraft.Subscription.Builders;

internal sealed class RelationSubscriptionBuilder<TFrame, TParent, TChild> : IRelationSubscriptionBuilder<TParent, TChild>
    where TFrame : class, IChangesFrame
    where TParent : Entity
    where TChild : Entity
{
    private readonly RelationSubscription<TFrame, TParent, TChild> _root;
    private readonly Change<TFrame>? _changes;

    public RelationSubscriptionBuilder(RelationSubscription<TFrame, TParent, TChild> root, Change<TFrame>? changes)
    {
        _root = root;
        _changes = changes;
    }

    public IDisposable Subscribe(IObserver<Change<IRelationChangeSet<TParent, TChild>>> observer)
    {
        var subscription = _root.Subscribe(observer);

        if (_changes != null)
        {
            var relation = _root.Accessor(_changes.Hunk);
            if (relation.HasChanges())
            {
                observer.OnNext(_changes.Map(_ => relation));
            }
        }

        return subscription;
    }

    internal TView SubscribeView<TView>(TView newView)
        where TView : DataView<TFrame>
    {
        var view = _root.SubscribeView(newView);

        if (_changes is not null)
        {
            view.OnNext(_changes);
        }

        return view;
    }
}
