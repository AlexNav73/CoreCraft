using CoreCraft.Engine.ChangesTracking;

namespace CoreCraft.Engine.Subscription.Builders;

/// <summary>
///     Subscription builder which provides a way to subscribe to relation changes
/// </summary>
/// <typeparam name="TParent">A type of parent entity</typeparam>
/// <typeparam name="TChild">A type of child entity</typeparam>
public interface IRelationSubscriptionBuilder<TParent, TChild> : ISubscriptionBuilder<IRelationChangeSet<TParent, TChild>>
    where TParent : Entity
    where TChild : Entity
{
}
