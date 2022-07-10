using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Subscription;

namespace Navitski.Crystalized.Model.Engine;

/// <summary>
///     A base interface for a domain model implementation
/// </summary>
public interface IDomainModel : IModelShardAccessor
{
    /// <summary>
    ///     Subscribes to the model changes notifications
    /// </summary>
    /// <param name="onModelChanges">A subscriber</param>
    /// <returns>Subscription</returns>
    IDisposable Subscribe(Action<Message<IModelChanges>> onModelChanges);

    /// <summary>
    ///     Provides a precise subscription mode to subscribe to a specific part of the model
    /// </summary>
    /// <param name="builder">A subscription builder</param>
    /// <returns>Subscription</returns>
    IDisposable Subscribe(Func<IModelSubscriber, IDisposable> builder);
}
