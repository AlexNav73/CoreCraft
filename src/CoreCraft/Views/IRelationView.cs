using CoreCraft.Subscription.Builders;

namespace CoreCraft.Views;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TParent"></typeparam>
/// <typeparam name="TChild"></typeparam>
public interface IRelationView<TParent, TChild> :
    IRelation<TParent, TChild>,
    IRelationSubscriptionBuilder<TParent, TChild>,
    IDisposable
    where TParent : Entity
    where TChild : Entity
{
}
