using Navitski.Crystalized.Model.Engine.ChangesTracking;
using System.Runtime.CompilerServices;

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal sealed class ModelShardSubscription<T> : Subscription<T>, ISubscription<IModelChanges>
    where T : class, IChangesFrame
{
    private readonly IDictionary<string, ISubscription<T>> _subscriptions;

    public ModelShardSubscription()
    {
        _subscriptions = new Dictionary<string, ISubscription<T>>();
    }

    public CollectionSubscription<T, TEntity, TProperties> With<TEntity, TProperties>(
        Func<T, ICollectionChangeSet<TEntity, TProperties>> accessor,
        [CallerArgumentExpression(nameof(accessor))] string expression = "")
        where TEntity : Entity
        where TProperties : Properties
    {
        if (_subscriptions.TryGetValue(expression, out var subs))
        {
            return (CollectionSubscription<T, TEntity, TProperties>)subs;
        }

        var subscriber = new CollectionSubscription<T, TEntity, TProperties>(accessor);
        _subscriptions.Add(expression, subscriber);

        return subscriber;
    }

    public RelationSubscription<T, TParent, TChild> With<TParent, TChild>(
        Func<T, IRelationChangeSet<TParent, TChild>> accessor,
        [CallerArgumentExpression(nameof(accessor))] string expression = "")
        where TParent : Entity
        where TChild : Entity
    {
        if (_subscriptions.TryGetValue(expression, out var subs))
        {
            return (RelationSubscription<T, TParent, TChild>)subs;
        }

        var subscriber = new RelationSubscription<T, TParent, TChild>(accessor);
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
