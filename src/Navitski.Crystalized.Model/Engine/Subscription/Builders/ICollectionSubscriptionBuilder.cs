using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Subscription.Binding;

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
    /// <param name="binding">The collection binding to be bound</param>
    void Bind(ICollectionBinding<TEntity, TProperties> binding);

    /// <summary>
    ///     Binds an object to the specified entity to receive notifications about the entity's changes
    /// </summary>
    /// <param name="entity">An entity to which an object will be bound</param>
    /// <param name="binding">An object which will receive notifications about entity changes</param>
    void Bind(TEntity entity, IEntityBinding<TEntity, TProperties> binding);
}
