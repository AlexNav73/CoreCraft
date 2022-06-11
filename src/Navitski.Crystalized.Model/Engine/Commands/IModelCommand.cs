namespace Navitski.Crystalized.Model.Engine.Commands;

/// <summary>
///     A command is a atomic pies of logic which modifies a model
/// </summary>
/// <remarks>
///     A model for the whole application is a read-only data storage, but
///     to modify it commands exist. A command is a small piece of logic
///     which will only modify the model, so it doesn't return any values.
///     Using the separation of reading and writing, it is possible to run
///     commands in the another thread and don't block the whole application.
///     Commands are executed sequentially and each time, a command finishes
///     subscribers of a model receive notification that model has been changed
///     and all recorded changes are propagated to the subscribers to analyze
///     what was changed and how to react to these changes.
/// </remarks>
public interface IModelCommand
{
    /// <summary>
    ///     Starts an execution of the command
    /// </summary>
    /// <param name="token">A cancellation token</param>
    /// <returns>A task which might be awaited if it necessary</returns>
    Task Execute(CancellationToken token = default);
}
