using CoreCraft.ChangesTracking;
using System.Runtime.CompilerServices;

namespace CoreCraft.Subscription;

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
        var collectionName = GetCollectionOrRelationName(expression);
        if (_subscriptions.TryGetValue(collectionName, out var subs))
        {
            return (CollectionSubscription<T, TEntity, TProperties>)subs;
        }

        var subscription = new CollectionSubscription<T, TEntity, TProperties>(accessor);
        _subscriptions.Add(collectionName, subscription);

        return subscription;
    }

    public RelationSubscription<T, TParent, TChild> With<TParent, TChild>(
        Func<T, IRelationChangeSet<TParent, TChild>> accessor,
        [CallerArgumentExpression(nameof(accessor))] string expression = "")
        where TParent : Entity
        where TChild : Entity
    {
        var relationName = GetCollectionOrRelationName(expression);
        if (_subscriptions.TryGetValue(relationName, out var subs))
        {
            return (RelationSubscription<T, TParent, TChild>)subs;
        }

        var subscription = new RelationSubscription<T, TParent, TChild>(accessor);
        _subscriptions.Add(relationName, subscription);

        return subscription;
    }

    public void Publish(Change<IModelChanges> change, bool forView = false)
    {
        if (change.Hunk.TryGetFrame<T>(out var frame) && frame.HasChanges())
        {
            var msg = change.Map(_ => frame);

            Publish(msg, forView);

            foreach (var subscription in _subscriptions.Values.ToArray())
            {
                subscription.Publish(msg, forView);
            }
        }
    }

    private static string GetCollectionOrRelationName(string expression)
    {
        var indexOfLastPoint = expression.LastIndexOf('.');
        if (indexOfLastPoint == -1)
        {
            throw new InvalidOperationException($"Unable to subscribe to missing member in {typeof(T).FullName}");
        }

        return expression.Substring(indexOfLastPoint + 1);
    }
}
