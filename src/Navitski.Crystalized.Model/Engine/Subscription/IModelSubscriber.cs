using Navitski.Crystalized.Model.Engine.ChangesTracking;

namespace Navitski.Crystalized.Model.Engine.Subscription;

/// <summary>
///     Provides a way to subscribe to model changes
/// </summary>
public interface IModelSubscriber
{
    /// <summary>
    ///     Specifies which model shard changes to subscribe
    /// </summary>
    /// <typeparam name="T">A model shard's changes frame</typeparam>
    /// <returns>A model shard subscriber</returns>
    IModelShardSubscriber<T> To<T>() where T : class, IChangesFrame;
}
