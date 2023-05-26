using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal sealed class CollectionSubscription<TChangesFrame, TEntity, TProperties> :
    Subscription<ICollectionChangeSet<TEntity, TProperties>>,
    ISubscription<TChangesFrame>
    where TChangesFrame : class, IChangesFrame
    where TEntity : Entity
    where TProperties : Properties
{

    public CollectionSubscription(Func<TChangesFrame, ICollectionChangeSet<TEntity, TProperties>> accessor)
    {
        Accessor = accessor;
    }

    public Func<TChangesFrame, ICollectionChangeSet<TEntity, TProperties>> Accessor { get; }

    public void Publish(Change<TChangesFrame> change)
    {
        var (oldModel, newModel, hunk) = change;

        var collectionChangeSet = Accessor(hunk);
        if (collectionChangeSet.HasChanges())
        {
            var msg = new Change<ICollectionChangeSet<TEntity, TProperties>>(
                oldModel,
                newModel,
                collectionChangeSet);

            Publish(msg);
        }
    }
}
