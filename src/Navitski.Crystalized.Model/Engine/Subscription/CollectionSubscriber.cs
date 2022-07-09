using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal class CollectionSubscriber<TChangesFrame, TEntity, TProperties> :
    Subscriber<ICollectionChangeSet<TEntity, TProperties>>,
    ICollectionSubscriber<TEntity, TProperties>,
    ISubscription<TChangesFrame>
    where TChangesFrame : class, IChangesFrame
    where TEntity : Entity
    where TProperties : Properties
{
    private readonly Func<TChangesFrame, ICollectionChangeSet<TEntity, TProperties>> _accessor;

    public CollectionSubscriber(Func<TChangesFrame, ICollectionChangeSet<TEntity, TProperties>> accessor)
    {
        _accessor = accessor;
    }

    public void Push(Message<TChangesFrame> message)
    {
        var collectionChangeSet = _accessor(message.Changes);
        if (collectionChangeSet.HasChanges())
        {
            var msg = new Message<ICollectionChangeSet<TEntity, TProperties>>(message.OldModel, message.NewModel, collectionChangeSet);
            
            Notify(msg);
        }
    }
}
