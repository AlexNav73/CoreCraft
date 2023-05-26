using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal sealed class ModelSubscription : Subscription<IModelChanges>
{
    private readonly IDictionary<Type, ISubscription<IModelChanges>> _modelShardSubscriptions;

    public ModelSubscription()
    {
        _modelShardSubscriptions = new Dictionary<Type, ISubscription<IModelChanges>>();
    }

    public ModelShardSubscription<T> GetOrCreateSubscriberFor<T>() where T : class, IChangesFrame
    {
        if (_modelShardSubscriptions.TryGetValue(typeof(T), out var subs))
        {
            return (ModelShardSubscription<T>)subs;
        }

        var subscription = new ModelShardSubscription<T>();
        _modelShardSubscriptions.Add(typeof(T), subscription);

        return subscription;
    }

    public override void Publish(Change<IModelChanges> change)
    {
        base.Publish(change);

        foreach (var subscription in _modelShardSubscriptions.Values.ToArray())
        {
            subscription.Publish(change);
        }
    }
}
