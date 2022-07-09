using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

/// <summary>
///     Provides a way to subscribe to relation changes
/// </summary>
/// <typeparam name="TParent">A type of parent entity</typeparam>
/// <typeparam name="TChild">A type of child entity</typeparam>
public interface IRelationSubscriber<TParent, TChild>
    where TParent : Entity
    where TChild : Entity
{
    /// <summary>
    ///     Subscribes a delegate to a relation change event
    /// </summary>
    /// <param name="onRelationChanged">A delegate which will handle relation changes</param>
    /// <returns>A subscription</returns>
    IDisposable Subscribe(Action<Message<IRelationChangeSet<TParent, TChild>>> onRelationChanged);
}
