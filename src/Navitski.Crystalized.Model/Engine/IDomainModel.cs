using Navitski.Crystalized.Model.Engine.ChangesTracking;
using Navitski.Crystalized.Model.Engine.Commands;
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
    IDisposable Subscribe(Action<Change<IModelChanges>> onModelChanges);

    /// <summary>
    ///     Provides a precise subscription mode to subscribe to a specific part of the model
    /// </summary>
    /// <param name="builder">A subscription builder</param>
    /// <returns>Subscription</returns>
    IDisposable SubscribeTo<T>(Func<IModelShardSubscriber<T>, IDisposable> builder)
         where T : class, IChangesFrame;

    /// <summary>
    ///     Runs an action as a command on the specific model shard
    /// </summary>
    /// <remarks>
    ///     Type T here is a mutable type of the model shard
    /// </remarks>
    /// <typeparam name="T">Is a mutable version of the model shard</typeparam>
    /// <param name="command">An action with business logic</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>A task to wait till command execution finishes</returns>
    Task Run<T>(Action<T, CancellationToken> command, CancellationToken token = default)
        where T : IModelShard;

    /// <summary>
    ///     Runs an action as a command on a model
    /// </summary>
    /// <remarks>
    ///     <see cref="IModel"/> contains only mutable model shards inside
    /// </remarks>
    /// <param name="command">An action with business logic</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>A task to wait till command execution finishes</returns>
    Task Run(Action<IModel, CancellationToken> command, CancellationToken token = default);

    /// <summary>
    ///     Runs a command on a model
    /// </summary>
    /// <param name="command">An command object with business logic</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>A task to wait till command execution finishes</returns>
    Task Run(ICommand command, CancellationToken token = default);
}
