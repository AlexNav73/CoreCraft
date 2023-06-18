using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription.Builders;

/// <summary>
///     Represents a builder for creating subscriptions to collection changes
/// </summary>
/// <typeparam name="TEntity">The type of the entity object</typeparam>
/// <typeparam name="TProperties">The type of the properties associated with the entity</typeparam>
public interface ICollectionSubscriptionBuilder<TEntity, TProperties> : ISubscriptionBuilder<ICollectionChangeSet<TEntity, TProperties>>
    where TEntity : Entity
    where TProperties : Properties
{
    /// <summary>
    ///     Binds an object to a specific collection to receive notifications about the collection's changes
    /// </summary>
    /// <param name="observer">An observer of collection changes</param>
    /// <returns>A subscription</returns>
    IDisposable Bind(IObserver<BindingChanges<TEntity, TProperties>> observer);

    /// <summary>
    ///     Binds an object to the specified entity to receive notifications about the entity's changes
    /// </summary>
    /// <param name="entity">An entity to which an object will be bound</param>
    /// <param name="observer">An observer of entity changes</param>
    /// <returns>A subscription</returns>
    IDisposable Bind(TEntity entity, IObserver<IEntityChange<TEntity, TProperties>> observer);
}
