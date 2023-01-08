using Navitski.Crystalized.Model.Engine.ChangesTracking;
#if NETSTANDARD2_0_OR_GREATER
using Navitski.Crystalized.Model.Engine.Exceptions;
using System.Linq.Expressions;
#elif NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Navitski.Crystalized.Model.Engine.Subscription;

internal sealed class ModelShardSubscriber<T> : Subscriber<T>, IModelShardSubscriber<T>, ISubscription<IModelChanges>
    where T : class, IChangesFrame
{
    private readonly IDictionary<string, ISubscription<T>> _subscriptions;

    public ModelShardSubscriber()
    {
        _subscriptions = new Dictionary<string, ISubscription<T>>();
    }

#if NET5_0_OR_GREATER
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

#elif NETSTANDARD2_0_OR_GREATER
    public ICollectionSubscriber<TEntity, TProperties> With<TEntity, TProperties>(
        Expression<Func<T, ICollectionChangeSet<TEntity, TProperties>>> accessor)
        where TEntity : Entity
        where TProperties : Properties
    {
        if (accessor.Body is MemberExpression memberExpression)
        {
            if (_subscriptions.TryGetValue(memberExpression.Member.Name, out var subs))
            {
                return (ICollectionSubscriber<TEntity, TProperties>)subs;
            }

            var subscriber = new CollectionSubscriber<T, TEntity, TProperties>(accessor.Compile());
            _subscriptions.Add(memberExpression.Member.Name, subscriber);

            return subscriber;
        }

        throw new InvalidPropertySubscriptionException("Accessor should contain only property access");
    }

    public IRelationSubscriber<TParent, TChild> With<TParent, TChild>(
        Expression<Func<T, IRelationChangeSet<TParent, TChild>>> accessor)
        where TParent : Entity
        where TChild : Entity
    {
        if (accessor.Body is MemberExpression memberExpression)
        {
            if (_subscriptions.TryGetValue(memberExpression.Member.Name, out var subs))
            {
                return (IRelationSubscriber<TParent, TChild>)subs;
            }

            var subscriber = new RelationSubscriber<T, TParent, TChild>(accessor.Compile());
            _subscriptions.Add(memberExpression.Member.Name, subscriber);

            return subscriber;
        }

        throw new InvalidPropertySubscriptionException("Accessor should contain only property access");
    }
#endif

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
