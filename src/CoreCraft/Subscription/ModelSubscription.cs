using CoreCraft.ChangesTracking;

namespace CoreCraft.Subscription;

internal sealed class ModelSubscription : Subscription<IModelChanges>
{
    private readonly HashSet<DataView> _views;
    private readonly IDictionary<Type, ISubscription<IModelChanges>> _modelShardSubscriptions;

    public ModelSubscription()
    {
        _views = new HashSet<DataView>();
        _modelShardSubscriptions = new Dictionary<Type, ISubscription<IModelChanges>>();
    }

    public TView SubscribeView<TView>(TView view)
        where TView : DataView
    {
        if (!_views.Add(view))
        {
            view = (TView)_views.Single(x => x.Equals(view));
        }
        else
        {
            view.Subscription = new UnsubscribeOnDispose<DataView>(view, s => _views.Remove(s));
        }

        return view;
    }

    public ModelShardSubscription<T> GetOrCreateSubscriptionFor<T>() where T : class, IChangesFrame
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
        foreach (var handler in _views.ToArray())
        {
            handler.OnNext(change);
        }

        base.Publish(change);

        foreach (var subscription in _modelShardSubscriptions.Values.ToArray())
        {
            subscription.Publish(change);
        }
    }
}
