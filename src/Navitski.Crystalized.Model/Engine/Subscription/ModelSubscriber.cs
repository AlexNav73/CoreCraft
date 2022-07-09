using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal class ModelSubscriber : Subscriber<IModelChanges>, IModelSubscriber, ISubscription<IModelChanges>
{
    private readonly IDictionary<Type, ISubscription<IModelChanges>> _modelShardSubscriptions;

    public ModelSubscriber()
    {
        _modelShardSubscriptions = new Dictionary<Type, ISubscription<IModelChanges>>();
    }

    public IModelShardSubscriber<T> To<T>() where T : class, IChangesFrame
    {
        if (_modelShardSubscriptions.TryGetValue(typeof(T), out var subs))
        {
            return (IModelShardSubscriber<T>)subs;
        }

        var subscriber = new ModelShardSubscriber<T>();
        _modelShardSubscriptions.Add(typeof(T), subscriber);

        return subscriber;
    }

    public void Push(Message<IModelChanges> message)
    {
        Notify(message);

        foreach (var subscription in _modelShardSubscriptions.Values)
        {
            subscription.Push(message);
        }
    }
}
