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
    IDisposable Subscribe(Action<ModelChangedEventArgs> onModelChanges);
}
