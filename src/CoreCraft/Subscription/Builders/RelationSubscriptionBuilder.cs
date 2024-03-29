﻿using CoreCraft.ChangesTracking;

namespace CoreCraft.Subscription.Builders;

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
                observer.OnNext(_changes.Map(_ => relation));
            }
        }

        return subscription;
    }
}
