namespace Navitski.Crystalized.Model.Engine.Subscription;

/// <summary>
///     A common interface for all subscribers
/// </summary>
/// <typeparam name="T">A change type</typeparam>
public interface ISubscriber<T>
{
    /// <summary>
    ///     Subscribes a delegate to a change event
    /// </summary>
    /// <param name="handler">A delegate which will handle changes</param>
    /// <returns>A subscription</returns>
    IDisposable By(Action<Change<T>> handler);
}
