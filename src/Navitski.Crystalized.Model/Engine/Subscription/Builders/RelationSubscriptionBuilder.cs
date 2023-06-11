using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription.Builders;

internal sealed class RelationSubscriptionBuilder<T, TParent, TChild> : IRelationSubscriptionBuilder<TParent, TChild>
    where T : class, IChangesFrame
    where TParent : Entity
    where TChild : Entity
{
    private readonly RelationSubscription<T, TParent, TChild> _root;
    private readonly Change<T>? _changes;

    public RelationSubscriptionBuilder(RelationSubscription<T, TParent, TChild> root, Change<T>? changes)
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
                observer.OnNext(new Change<IRelationChangeSet<TParent, TChild>>(_changes.OldModel, _changes.NewModel, relation));
            }
        }

        return subscription;
    }

    public IDisposable Subscribe(Action<Change<IRelationChangeSet<TParent, TChild>>> handler)
    {
        return Subscribe(new AnonymousObserver<IRelationChangeSet<TParent, TChild>>(handler));
    }
}
