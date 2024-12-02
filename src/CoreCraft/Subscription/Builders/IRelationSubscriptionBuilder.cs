using CoreCraft.ChangesTracking;

namespace CoreCraft.Subscription.Builders;

/// <summary>
///     Subscription builder which provides a way to subscribe to relation changes
/// </summary>
/// <typeparam name="TParent">A type of parent entity</typeparam>
/// <typeparam name="TChild">A type of child entity</typeparam>
public interface IRelationSubscriptionBuilder<TParent, TChild> : IObservable<Change<IRelationChangeSet<TParent, TChild>>>
    where TParent : Entity
    where TChild : Entity
{
}
