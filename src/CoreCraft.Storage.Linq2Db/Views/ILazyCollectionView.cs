using CoreCraft.Core;
using CoreCraft.Subscription.Builders;

namespace CoreCraft.Storage.Linq2Db.Views;

/// <summary>
///     Represents a view of a collection of entities and their properties.
/// </summary>
/// <remarks>
///     This interface simplifies working with collections by removing the
///     need to retrieve the model shard containing the collection each time the domain
///     model changes. Views provide easy access to the most up-to-date entities
///     and properties, and also make it straightforward to subscribe to collection changes.
///     They are aware of the types of changes they observe and have access to the
///     domain model's subscription mechanism.
/// </remarks>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TProperties">The type of properties associated with the entity.</typeparam>
public interface ILazyCollectionView<TEntity, TProperties> :
    ILazyCollection<TEntity, TProperties>,
    ICollectionSubscriptionBuilder<TEntity, TProperties>,
    IDisposable
    where TEntity : Entity
    where TProperties : Properties, IHaveEntityId<TEntity>
{
}
