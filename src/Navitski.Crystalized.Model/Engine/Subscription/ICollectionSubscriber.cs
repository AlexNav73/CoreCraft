using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

/// <summary>
///     Provides a way to subscribe to collection changes
/// </summary>
/// <typeparam name="TEntity">An entity type</typeparam>
/// <typeparam name="TProperties">A type of a properties</typeparam>
public interface ICollectionSubscriber<TEntity, TProperties>
    where TEntity : Entity
    where TProperties : Properties
{
    /// <summary>
    ///     Subscribes a delegate to a collection change event
    /// </summary>
    /// <param name="onCollectionChanged">A delegate which will handle collection changes</param>
    /// <returns>A subscription</returns>
    IDisposable Subscribe(Action<Message<ICollectionChangeSet<TEntity, TProperties>>> onCollectionChanged);
}
