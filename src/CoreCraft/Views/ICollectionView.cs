using CoreCraft.Subscription.Builders;

namespace CoreCraft.Views;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TProperties"></typeparam>
public interface ICollectionView<TEntity, TProperties> :
    ICollection<TEntity, TProperties>,
    ICollectionSubscriptionBuilder<TEntity, TProperties>,
    IDisposable
    where TEntity : Entity
    where TProperties : Properties
{
}
