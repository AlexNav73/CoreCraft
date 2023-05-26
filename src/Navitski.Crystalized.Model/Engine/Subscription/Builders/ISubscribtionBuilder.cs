namespace Navitski.Crystalized.Model.Engine.Subscription.Builders;

/// <summary>
///     A common interface for all subscription builders
/// </summary>
/// <typeparam name="T">A change type</typeparam>
public interface ISubscriptionBuilder<T>
{
    /// <summary>
    ///     Subscribes a delegate to a change event
    /// </summary>
    /// <param name="handler">A delegate which will handle changes</param>
    /// <returns>A subscription</returns>
    IDisposable Subscribe(Action<Change<T>> handler);
}
