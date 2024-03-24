using CoreCraft.ChangesTracking;
using CoreCraft.Commands;
using CoreCraft.Subscription;
using CoreCraft.Subscription.Builders;

namespace CoreCraft;

/// <summary>
///     A base interface for a domain model implementation
/// </summary>
public interface IDomainModel : IModel
{
    /// <summary>
    ///     Subscribes to the model changes notifications
    /// </summary>
    /// <param name="onModelChanges">A changes handler</param>
    /// <returns>Subscription</returns>
    IDisposable Subscribe(Action<Change<IModelChanges>> onModelChanges);

    /// <summary>
    ///     Provides a precise subscription mode for a specific part of the model
    /// </summary>
    /// <returns>Subscription builder</returns>
    IModelShardSubscriptionBuilder<T> For<T>()
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
        where T : IMutableModelShard;

    /// <summary>
    ///     Runs an action as a command on a model
    /// </summary>
    /// <remarks>
    ///     <see cref="IModel"/> contains only mutable model shards inside
    /// </remarks>
    /// <param name="command">An action with business logic</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>A task to wait till command execution finishes</returns>
    Task Run(Action<IMutableModel, CancellationToken> command, CancellationToken token = default);

    /// <summary>
    ///     Runs a command on a model
    /// </summary>
    /// <param name="command">An command object with business logic</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>A task to wait till command execution finishes</returns>
    Task Run(ICommand command, CancellationToken token = default);
}
