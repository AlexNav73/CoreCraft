using Navitski.Crystalized.Model.Engine.ChangesTracking;
using System.Runtime.CompilerServices;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal sealed class ModelShardSubscriber<T> : Subscriber<T>, IModelShardSubscriber<T>, ISubscription<IModelChanges>
    where T : class, IChangesFrame
{
    private readonly IDictionary<string, ISubscription<T>> _subscriptions;

    public ModelShardSubscriber()
    {
        _subscriptions = new Dictionary<string, ISubscription<T>>();
    }

    public ICollectionSubscriber<TEntity, TProperties> With<TEntity, TProperties>(
        Func<T, ICollectionChangeSet<TEntity, TProperties>> accessor,
        [CallerArgumentExpression(nameof(accessor))] string expression = "")
        where TEntity : Entity
        where TProperties : Properties
    {
        if (_subscriptions.TryGetValue(expression, out var subs))
        {
            return (ICollectionSubscriber<TEntity, TProperties>)subs;
        }

        var subscriber = new CollectionSubscriber<T, TEntity, TProperties>(accessor);
        _subscriptions.Add(expression, subscriber);

        return subscriber;
    }

    public IRelationSubscriber<TParent, TChild> With<TParent, TChild>(
        Func<T, IRelationChangeSet<TParent, TChild>> accessor,
        [CallerArgumentExpression(nameof(accessor))] string expression = "")
        where TParent : Entity
        where TChild : Entity
    {
        if (_subscriptions.TryGetValue(expression, out var subs))
        {
            return (IRelationSubscriber<TParent, TChild>)subs;
        }

        var subscriber = new RelationSubscriber<T, TParent, TChild>(accessor);
        _subscriptions.Add(expression, subscriber);

        return subscriber;
    }

    public void Publish(Change<IModelChanges> change)
    {
        var (oldModel, newModel, hunk) = change;

        if (hunk.TryGetFrame<T>(out var frame) && frame.HasChanges())
        {
            var msg = new Change<T>(oldModel, newModel, frame);

            Publish(msg);

            foreach (var subscription in _subscriptions.Values.ToArray())
            {
                subscription.Publish(msg);
            }
        }
    }
}
