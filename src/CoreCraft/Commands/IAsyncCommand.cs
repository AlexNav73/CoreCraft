namespace CoreCraft.Commands;

/// <summary>
///     Represents an asynchronous command that performs a mutation on the domain model.
/// </summary>
/// <remarks>
///     The application model is treated as read-only for readers. Commands are used
///     to modify the model. A command is a small, self-contained unit of logic that
///     performs mutations on the model and does not return a value.
///
///     Separating read and write operations makes it possible to run commands on a
///     separate thread without blocking the rest of the application. Commands are
///     executed sequentially; when a command completes, model subscribers are
///     notified and the recorded changes are propagated so observers can react.
/// </remarks>
public interface IAsyncCommand
{
    /// <summary>
    ///     Executes the command asynchronously.
    /// </summary>
    /// <remarks>
    ///     <see cref="IMutableModel"/> provides mutable versions of model shards.
    ///     For example, if the model exposes an "IExampleModelShard", the mutable
    ///     view will expose an "IMutableExampleModelShard" that the command can use
    ///     to perform modifications.
    /// </remarks>
    /// <param name="model">Mutable model view used to perform modifications.</param>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    Task ExecuteAsync(IMutableModel model, CancellationToken token);
}
