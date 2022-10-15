using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal sealed class RelationSubscriber<TChangesFrame, TParent, TChild> :
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

    public void Publish(Change<TChangesFrame> change)
    {
        var (oldModel, newModel, hunk) = change;

        var relationChangeSet = _accessor(hunk);
        if (relationChangeSet.HasChanges())
        {
            var msg = new Change<IRelationChangeSet<TParent, TChild>>(oldModel, newModel, relationChangeSet);

            Publish(msg);
        }
    }
}
