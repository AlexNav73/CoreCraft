using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription.Builders;

/// <summary>
///     Subscription build which provides a way to subscribe to collection changes
/// </summary>
/// <typeparam name="TEntity">An entity type</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
public interface ICollectionSubscriptionBuilder<TEntity, TProperties> : ISubscriptionBuilder<ICollectionChangeSet<TEntity, TProperties>>
    where TEntity : Entity
    where TProperties : Properties
{
}
