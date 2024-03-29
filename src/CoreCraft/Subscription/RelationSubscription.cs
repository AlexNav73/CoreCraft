﻿using CoreCraft.ChangesTracking;

namespace CoreCraft.Subscription;

internal sealed class RelationSubscription<TChangesFrame, TParent, TChild> :
    Subscription<IRelationChangeSet<TParent, TChild>>,
    ISubscription<TChangesFrame>
    where TChangesFrame : class, IChangesFrame
    where TParent : Entity
    where TChild : Entity
{
    public RelationSubscription(Func<TChangesFrame, IRelationChangeSet<TParent, TChild>> accessor)
    {
        Accessor = accessor;
    }

    public Func<TChangesFrame, IRelationChangeSet<TParent, TChild>> Accessor { get; }

    public void Publish(Change<TChangesFrame> change)
    {
        var relationChangeSet = Accessor(change.Hunk);
        if (relationChangeSet.HasChanges())
        {
            var msg = change.Map(_ => relationChangeSet);

            Publish(msg);
        }
    }
}
