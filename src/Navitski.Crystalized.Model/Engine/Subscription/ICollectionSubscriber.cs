using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

/// <summary>
///     Provides a way to subscribe to collection changes
/// </summary>
/// <typeparam name="TEntity">An entity type</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
public interface ICollectionSubscriber<TEntity, TProperties> : ISubscriber<ICollectionChangeSet<TEntity, TProperties>>
    where TEntity : Entity
    where TProperties : Properties
{
}
