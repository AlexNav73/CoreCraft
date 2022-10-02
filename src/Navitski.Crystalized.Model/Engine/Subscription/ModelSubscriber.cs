using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal sealed class ModelSubscriber : Subscriber<IModelChanges>
{
    private readonly IDictionary<Type, ISubscription<IModelChanges>> _modelShardSubscriptions;

    public ModelSubscriber()
    {
        _modelShardSubscriptions = new Dictionary<Type, ISubscription<IModelChanges>>();
    }

    public IModelShardSubscriber<T> GetOrCreateSubscriberFor<T>() where T : class, IChangesFrame
    {
        if (_modelShardSubscriptions.TryGetValue(typeof(T), out var subs))
        {
            return (IModelShardSubscriber<T>)subs;
        }

        var subscriber = new ModelShardSubscriber<T>();
        _modelShardSubscriptions.Add(typeof(T), subscriber);

        return subscriber;
    }

    public override void Publish(Change<IModelChanges> change)
    {
        base.Publish(change);

        foreach (var subscription in _modelShardSubscriptions.Values)
        {
            subscription.Publish(change);
        }
    }
}
