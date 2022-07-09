using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal class RelationSubscriber<TChangesFrame, TParent, TChild> :
    Subscriber<IRelationChangeSet<TParent, TChild>>,
    IRelationSubscriber<TParent, TChild>,
    ISubscription<TChangesFrame>
    where TChangesFrame : class, IChangesFrame
    where TParent : Entity
    where TChild : Entity
{
    private readonly Func<TChangesFrame, IRelationChangeSet<TParent, TChild>> _accessor;

    public RelationSubscriber(Func<TChangesFrame, IRelationChangeSet<TParent, TChild>> accessor)
    {
        _accessor = accessor;
    }

    public void Push(Message<TChangesFrame> message)
    {
        var relationChangeSet = _accessor(message.Changes);
        if (relationChangeSet.HasChanges())
        {
            var msg = new Message<IRelationChangeSet<TParent, TChild>>(message.OldModel, message.NewModel, relationChangeSet);

            Notify(msg);
        }
    }
}
